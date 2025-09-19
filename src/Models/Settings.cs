using System;
using System.IO;
using System.Text.Json;

namespace OptimizeRK.Models {
	public class Settings {
		// Save file path in AppData\Roaming\OptimizeRK\settings.json
		private static readonly string FilePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"OptimizeRK",
			"settings.json"
		);

		// Settings
		public int JpgQuality { get; set; } = 70;
		public int PngQuality { get; set; } = 70;
		public int GifQuality { get; set; } = 70;
		public int Mp4Quality { get; set; } = 70;
		public int MaxParallelism { get; set; } = 4;
		public bool ReplaceOriginalFile { get; set; } = false;

		// Load settings from file, or return defaults if not found/invalid
		public static Settings Load() {
			try {
				if (File.Exists(FilePath)) {
					var json = File.ReadAllText(FilePath);
					return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
				}
			} catch {
				// If anything goes wrong, return defaults
			}

			// Return defaults if file missing or invalid
			return new Settings();
		}

		// Save settings to file (ensure directory exists first)
		public void Save() {
			try {
				var dir = Path.GetDirectoryName(FilePath);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}

				var json = JsonSerializer.Serialize(this, new JsonSerializerOptions {
					WriteIndented = true
				});

				File.WriteAllText(FilePath, json);
			} catch (Exception ex) {
				// Optional: handle/log exception (e.g. show message to user)
				Console.Error.WriteLine($"Failed to save settings: {ex.Message}");
			}
		}
	}
}
