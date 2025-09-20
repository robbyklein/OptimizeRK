namespace OptimizeRK.Services;

using OptimizeRK.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class FileOptimizer(Settings settings) {
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
                            await RunTool(file, "ffmpeg.exe");
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

            "ffmpeg.exe" =>
                $"-hide_banner -loglevel info -i \"{file.Path}\" -c:v libx264 -preset fast -crf {(int)(51 - (settings.Mp4Quality / 100.0 * 51))} -c:a copy -y \"{tempOutput}\"",

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

        // Replace original if configured
        var finalOutput = settings.ReplaceOriginalFile ? file.Path : tempOutput;

        if (settings.ReplaceOriginalFile) {
            File.Delete(file.Path);
            File.Move(tempOutput, file.Path, overwrite: true);
        }

        var info = new FileInfo(finalOutput);
        file.NewSize = info.Length;
    }
}
