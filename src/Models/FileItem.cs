namespace OptimizeRK.Models;

using OptimizeRK.Helpers;
using ReactiveUI;

public class FileItem : ReactiveObject {
    private string name = string.Empty;
    private string path = string.Empty;
    private long originalSize;
    private long newSize;
    private ProcessStatus status = ProcessStatus.Pending;

    public string Name {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    public string Path {
        get => path;
        set => this.RaiseAndSetIfChanged(ref path, value);
    }

    public long OriginalSize {
        get => originalSize;
        set {
            this.RaiseAndSetIfChanged(ref originalSize, value);
            this.RaisePropertyChanged(nameof(OriginalSizeDisplay));
            this.RaisePropertyChanged(nameof(SavingsDisplay));
            this.RaisePropertyChanged(nameof(SizeDifferenceDisplay));
        }
    }

    public long NewSize {
        get => newSize;
        set {
            this.RaiseAndSetIfChanged(ref newSize, value);
            this.RaisePropertyChanged(nameof(NewSizeDisplay));
            this.RaisePropertyChanged(nameof(SavingsDisplay));
            this.RaisePropertyChanged(nameof(SizeDifferenceDisplay));
        }
    }

    public ProcessStatus Status {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
    }

    public string OriginalSizeDisplay => ByteSize.Format(OriginalSize);

    public string NewSizeDisplay => ByteSize.Format(NewSize);

    public string SizeDifferenceDisplay =>
        OriginalSize > 0 && NewSize > 0 && OriginalSize > NewSize
            ? $"-{ByteSize.Format(OriginalSize - NewSize)}"
            : "—";

    public string SavingsDisplay =>
        OriginalSize > 0 && NewSize > 0
            ? $"{100 - (100.0 * NewSize / OriginalSize):0.##}%"
            : "—";
}