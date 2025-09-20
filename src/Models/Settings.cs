namespace OptimizeRK.Models;

using System;
using System.IO;
using System.Text.Json;

public class Settings {
    // Save file path in AppData\Roaming\OptimizeRK\settings.json
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "OptimizeRK",
        "settings.json");

    private static readonly JsonSerializerOptions SerializerOptions = new() {
        WriteIndented = true,
    };

    // Settings
    public int JpgQuality { get; set; } = 70;

    public int PngQuality { get; set; } = 70;

    public int GifQuality { get; set; } = 70;

    public int Mp4Quality { get; set; } = 70;

    public int WebmQuality { get; set; } = 70;

    public int MaxParallelism { get; set; } = 4;

    public bool ReplaceOriginalFile { get; set; } = false;

    public bool RemoveAudio { get; set; } = false;

    public bool OutputWebm { get; set; } = false;

    public VideoScale ScaleVideo { get; set; } = VideoScale.Original;

    // Load settings from file, or return defaults if not found/invalid
    public static Settings Load() {
        try {
            if (File.Exists(FilePath)) {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Settings>(json, SerializerOptions)
                       ?? new Settings();
            }
        } catch {
            // If anything goes wrong, return defaults
        }

        return new Settings();
    }

    public void Save() {
        try {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(this, SerializerOptions);
            File.WriteAllText(FilePath, json);
        } catch (Exception ex) {
            Console.Error.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}
