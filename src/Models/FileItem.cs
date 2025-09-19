using OptimizeRK.Helpers;
using ReactiveUI;

namespace OptimizeRK.Models;

public class FileItem : ReactiveObject {
	private string _name = string.Empty;
	private string _path = string.Empty;
	private long _originalSize;
	private long _newSize;
	private ProcessStatus _status = ProcessStatus.Pending;

	public string Name {
		get => _name;
		set => this.RaiseAndSetIfChanged(ref _name, value);
	}

	public string Path {
		get => _path;
		set => this.RaiseAndSetIfChanged(ref _path, value);
	}

	public long OriginalSize {
		get => _originalSize;
		set {
			this.RaiseAndSetIfChanged(ref _originalSize, value);
			this.RaisePropertyChanged(nameof(OriginalSizeDisplay));
			this.RaisePropertyChanged(nameof(SavingsDisplay));
			this.RaisePropertyChanged(nameof(SizeDifferenceDisplay));
		}
	}

	public long NewSize {
		get => _newSize;
		set {
			this.RaiseAndSetIfChanged(ref _newSize, value);
			this.RaisePropertyChanged(nameof(NewSizeDisplay));
			this.RaisePropertyChanged(nameof(SavingsDisplay));
			this.RaisePropertyChanged(nameof(SizeDifferenceDisplay));
		}
	}

	public ProcessStatus Status {
		get => _status;
		set => this.RaiseAndSetIfChanged(ref _status, value);
	}

	// --- Derived / display properties ---

	public string OriginalSizeDisplay => ByteSize.Format(OriginalSize);
	public string NewSizeDisplay => ByteSize.Format(NewSize);

	public string SizeDifferenceDisplay =>
		OriginalSize > 0 && NewSize > 0 && OriginalSize > NewSize
			? $"-{ByteSize.Format(OriginalSize - NewSize)}"
			: "—";

	public string SavingsDisplay =>
		OriginalSize > 0 && NewSize > 0
			? $"{100 - 100.0 * NewSize / OriginalSize:0.##}%"
			: "—";

}

