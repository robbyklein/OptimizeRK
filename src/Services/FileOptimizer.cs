namespace OptimizeRK.Services;

using OptimizeRK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class FileOptimizer(Settings settings) {
    private static readonly SemaphoreSlim VideoGate = new(1);

    private readonly Settings settings = settings;

    public async Task OptimizeFilesAsync(IEnumerable<FileItem> files) {
        var bag = new ConcurrentQueue<FileItem>(files);

        var tasks = Enumerable.Range(0, settings.MaxParallelism).Select(_ => Task.Run(async () => {
            while (bag.TryDequeue(out var file)) {
                try {
                    file.Status = ProcessStatus.Processing;

                    switch (Path.GetExtension(file.Path).ToLowerInvariant()) {
                        case ".jpg":
                        case ".jpeg":
                            await RunTool(file, "cjpeg-static.exe");
                            break;

                        case ".png":
                            await RunTool(file, "pngquant.exe");
                            break;

                        case ".gif":
                            await RunTool(file, "gifsicle.exe");
                            break;

                        case ".mp4":
                            // ffmpeg already uses every core, so encoding videos side by side only adds memory pressure
                            await VideoGate.WaitAsync();
                            try {
                                await RunTool(file, "ffmpeg.exe");
                            } finally {
                                VideoGate.Release();
                            }

                            break;

                        default:
                            file.Status = ProcessStatus.Pending; // unsupported → left alone
                            continue;
                    }

                    file.Status = ProcessStatus.Optimized;
                } catch (Exception ex) {
                    Debug.WriteLine($"[Optimizer] Failed on {file.Name}: {ex.Message}");
                    file.Status = ProcessStatus.Failed;
                }
            }
        }));

        await Task.WhenAll(tasks);
    }

    private string BuildFfmpegArgs(FileItem file, string tempOutput) {
        // Map quality 1-100 onto the CRF range that's useful for the web (lower CRF = higher quality).
        // At the default of 70: H.264 CRF 26 and VP9 CRF 34, both around VMAF 93-96.
        var h264Crf = (int)Math.Round(40 - (settings.Mp4Quality * 0.2));
        var vp9Crf = (int)Math.Round(55 - (settings.WebmQuality * 0.3));

        // Cap the width without ever upscaling, and keep both dimensions even, which yuv420p requires.
        // yuv420p is also the only pixel format every browser can play.
        var width = settings.ScaleVideo == VideoScale.Original ? "iw" : $"min(iw,{(int)settings.ScaleVideo})";
        var video = $"-vf \"scale='trunc({width}/2)*2':-2,setsar=1\" -pix_fmt yuv420p";

        var audioArgMp4 = settings.RemoveAudio ? "-an" : "-c:a aac -b:a 128k";
        var audioArgWebm = settings.RemoveAudio ? "-an" : "-c:a libopus -b:a 96k";

        // Always output MP4. faststart puts the index first so browsers can start playing before it's all downloaded,
        // and -map_metadata -1 drops metadata such as the GPS location phones record.
        var args =
            $"-hide_banner -loglevel info -i \"{file.Path}\" " +
            $"-c:v libx264 -preset slow -crf {h264Crf} {video} {audioArgMp4} -map_metadata -1 -movflags +faststart -y \"{tempOutput}\"";

        // Optionally also output WebM
        if (settings.OutputWebm) {
            var webmOutput = Path.Combine(
                Path.GetDirectoryName(file.Path)!,
                Path.GetFileNameWithoutExtension(file.Path) + "_optimized.webm");

            // row-mt and cpu-used 2 make libvpx several times faster for about the same size
            args += " " +
                $"-c:v libvpx-vp9 -b:v 0 -crf {vp9Crf} -deadline good -cpu-used 2 -row-mt 1 {video} {audioArgWebm} -map_metadata -1 -y \"{webmOutput}\"";
        }

        return args;
    }

    private async Task RunTool(FileItem file, string toolName) {
        var toolPath = Path.Combine(AppContext.BaseDirectory, "Tools", toolName);

        // Always output to a temp file first
        var tempOutput = Path.Combine(
            Path.GetDirectoryName(file.Path)!,
            Path.GetFileNameWithoutExtension(file.Path) + "_optimized" + Path.GetExtension(file.Path));

        string args = toolName switch {
            "cjpeg-static.exe" =>
                $"-quality {settings.JpgQuality} -outfile \"{tempOutput}\" \"{file.Path}\"",

            "pngquant.exe" =>
                $"--quality={Math.Max(0, settings.PngQuality - 10)}-{settings.PngQuality} --force -o \"{tempOutput}\" \"{file.Path}\"",

            "gifsicle.exe" =>
                $"--optimize=3 --lossy={settings.GifQuality * 2} \"{file.Path}\" -o \"{tempOutput}\"",

            "ffmpeg.exe" => BuildFfmpegArgs(file, tempOutput),

            _ => throw new NotSupportedException($"{toolName} not supported")
        };

        var startInfo = new ProcessStartInfo {
            FileName = toolPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };

        var stdoutLines = new List<string>();
        var stderrLines = new List<string>();

        process.OutputDataReceived += (s, e) => {
            if (e.Data != null) {
                stdoutLines.Add(e.Data);
            }
        };

        process.ErrorDataReceived += (s, e) => {
            if (e.Data != null) {
                stderrLines.Add(e.Data);
            }
        };

        var startTime = DateTime.Now;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        var elapsed = DateTime.Now - startTime;

        if (process.ExitCode != 0 || !File.Exists(tempOutput)) {
            throw new Exception($"{toolName} failed (Exit {process.ExitCode}). See logs above.");
        }

        // Keep the original if re-encoding didn't make it smaller (e.g. a video already compressed for the web)
        if (new FileInfo(tempOutput).Length >= file.OriginalSize) {
            File.Delete(tempOutput);
            file.NewSize = file.OriginalSize;
            file.OutputPath = file.Path;
            return;
        }

        // Replace original if configured
        var finalOutput = settings.ReplaceOriginalFile ? file.Path : tempOutput;

        if (settings.ReplaceOriginalFile) {
            File.Delete(file.Path);
            File.Move(tempOutput, file.Path, overwrite: true);
        }

        var info = new FileInfo(finalOutput);
        file.NewSize = info.Length;
        file.OutputPath = finalOutput;
    }
}
