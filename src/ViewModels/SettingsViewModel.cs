namespace OptimizeRK.ViewModels;

using OptimizeRK.Models;
using ReactiveUI;

public class SettingsViewModel : ViewModelBase {
    private readonly Settings settings;

    public SettingsViewModel() {
        settings = Settings.Load();
    }

    public int JpgQuality {
        get => settings.JpgQuality;
        set {
            if (settings.JpgQuality != value) {
                settings.JpgQuality = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public int PngQuality {
        get => settings.PngQuality;
        set {
            if (settings.PngQuality != value) {
                settings.PngQuality = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public int GifQuality {
        get => settings.GifQuality;
        set {
            if (settings.GifQuality != value) {
                settings.GifQuality = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public int Mp4Quality {
        get => settings.Mp4Quality;
        set {
            if (settings.Mp4Quality != value) {
                settings.Mp4Quality = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public int MaxParallelism {
        get => settings.MaxParallelism;
        set {
            if (settings.MaxParallelism != value) {
                settings.MaxParallelism = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public bool ReplaceOriginalFile {
        get => settings.ReplaceOriginalFile;
        set {
            if (settings.ReplaceOriginalFile != value) {
                settings.ReplaceOriginalFile = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }
}
