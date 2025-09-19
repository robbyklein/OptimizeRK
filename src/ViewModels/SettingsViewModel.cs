using OptimizeRK.Models;
using ReactiveUI;

namespace OptimizeRK.ViewModels;

public class SettingsViewModel : ViewModelBase {
	private readonly Settings _settings;

	public SettingsViewModel() {
		_settings = Settings.Load();
	}

	public int JpgQuality {
		get => _settings.JpgQuality;
		set {
			if (_settings.JpgQuality != value) {
				_settings.JpgQuality = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}

	public int PngQuality {
		get => _settings.PngQuality;
		set {
			if (_settings.PngQuality != value) {
				_settings.PngQuality = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}

	public int GifQuality {
		get => _settings.GifQuality;
		set {
			if (_settings.GifQuality != value) {
				_settings.GifQuality = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}

	public int Mp4Quality {
		get => _settings.Mp4Quality;
		set {
			if (_settings.Mp4Quality != value) {
				_settings.Mp4Quality = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}

	public int MaxParallelism {
		get => _settings.MaxParallelism;
		set {
			if (_settings.MaxParallelism != value) {
				_settings.MaxParallelism = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}

	public bool ReplaceOriginalFile {
		get => _settings.ReplaceOriginalFile;
		set {
			if (_settings.ReplaceOriginalFile != value) {
				_settings.ReplaceOriginalFile = value;
				_settings.Save();
				this.RaisePropertyChanged();
			}
		}
	}
}
