namespace OptimizeRK.ViewModels;

using OptimizeRK.Helpers;
using OptimizeRK.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged {
    public MainWindowViewModel() {
        Files = [];

        // Recalculate when collection changes
        Files.CollectionChanged += (s, e) => {
            OnPropertyChanged(nameof(HasFiles));
            OnPropertyChanged(nameof(SummaryText));

            // Hook PropertyChanged for new items so SummaryText refreshes when sizes/status change
            if (e.NewItems != null) {
                foreach (var item in e.NewItems.OfType<FileItem>()) {
                    item.PropertyChanged += (_, __) => OnPropertyChanged(nameof(SummaryText));
                }
            }
        };

        OpenInfoCommand = ReactiveCommand.Create(OpenInfo);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string SummaryText {
        get {
            if (!Files.Any()) {
                return string.Empty;
            }

            long totalOriginal = Files.Sum(f => f.OriginalSize);
            long totalNew = Files.Sum(f => f.NewSize);
            long totalSaved = totalOriginal - totalNew;

            if (totalOriginal <= 0) {
                return string.Empty;
            }

            double avgPercent = Files.Average(f =>
                f.OriginalSize > 0 ? (1 - ((double)f.NewSize / f.OriginalSize)) * 100 : 0);

            double maxPercent = Files.Max(f =>
                f.OriginalSize > 0 ? (1 - ((double)f.NewSize / f.OriginalSize)) * 100 : 0);

            return $"Saved {ByteSize.Format(totalSaved)} out of {ByteSize.Format(totalOriginal)}. " +
                   $"{avgPercent:0.##}% per file on average (up to {maxPercent:0.##}%).";
        }
    }

    public ObservableCollection<FileItem> Files { get; }

    public ReactiveCommand<Unit, Unit> OpenInfoCommand { get; }

    public bool HasFiles => Files.Any();

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void OpenInfo() {
        var url = "https://github.com/AvaloniaUI/Avalonia";

        try {
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true,
            });
        } catch {
            // optionally handle errors
        }
    }
}
