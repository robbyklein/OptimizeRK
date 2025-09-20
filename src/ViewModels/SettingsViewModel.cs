namespace OptimizeRK.ViewModels;

using OptimizeRK.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

public class SettingsViewModel : ViewModelBase {
    public record ScaleOption(VideoScale value, string label);

    private readonly Settings settings;

    public SettingsViewModel() {
        settings = Settings.Load();
    }

    public static IEnumerable<ScaleOption> VideoScaleOptions =>
        new[] { new ScaleOption(VideoScale.Original, "Original") }
        .Concat(
            Enum.GetValues<VideoScale>()
                .Where(v => v != VideoScale.Original)
                .OrderByDescending(v => (int)v)
                .Select(v => new ScaleOption(v, $"{(int)v}px")));

    public ScaleOption SelectedScaleOption {
        get => VideoScaleOptions.First(x => x.value == settings.ScaleVideo);
        set {
            if (value != null && settings.ScaleVideo != value.value) {
                settings.ScaleVideo = value.value;
                settings.Save();
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(SelectedScaleOption));
            }
        }
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

    public int WebmQuality {
        get => settings.WebmQuality;
        set {
            if (settings.WebmQuality != value) {
                settings.WebmQuality = value;
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

    public bool RemoveAudio {
        get => settings.RemoveAudio;
        set {
            if (settings.RemoveAudio != value) {
                settings.RemoveAudio = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public bool OutputWebm {
        get => settings.OutputWebm;
        set {
            if (settings.OutputWebm != value) {
                settings.OutputWebm = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }

    public VideoScale ScaleVideo {
        get => settings.ScaleVideo;
        set {
            if (settings.ScaleVideo != value) {
                settings.ScaleVideo = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
    }
}
