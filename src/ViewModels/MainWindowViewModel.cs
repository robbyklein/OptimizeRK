namespace OptimizeRK.ViewModels;

using Avalonia.Threading;
using OptimizeRK.Helpers;
using OptimizeRK.Models;
using OptimizeRK.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;

public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged {
    private static readonly TimeSpan BatchDelay = TimeSpan.FromMilliseconds(500);
    private readonly List<FileItem> queued = [];
    private IDisposable? batchTimer;

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

    // Adds files to the list and optimizes them. Must be called on the UI thread.
    public void AddFiles(IEnumerable<string> paths) {
        foreach (var path in paths) {
            if (!File.Exists(path)) {
                continue;
            }

            var info = new FileInfo(path);
            var item = new FileItem {
                Name = info.Name,
                Path = info.FullName,
                OriginalSize = info.Length,
                NewSize = info.Length,
                Status = ProcessStatus.Pending,
            };

            Files.Add(item);
            queued.Add(item);
        }

        if (queued.Count == 0) {
            return;
        }

        // A multi-file right-click arrives a file at a time (one copy of the app per file),
        // so wait until they stop coming and optimize them as one batch with one notification.
        batchTimer?.Dispose();
        batchTimer = DispatcherTimer.RunOnce(OptimizeQueued, BatchDelay);
    }

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private async void OptimizeQueued() {
        var batch = queued.ToList();
        queued.Clear();

        var optimizer = new FileOptimizer(Settings.Load());
        await optimizer.OptimizeFilesAsync(batch);

        Notifier.ShowSummary(batch);
    }

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
