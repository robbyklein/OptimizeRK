namespace OptimizeRK;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OptimizeRK.ViewModels;
using System.Diagnostics;
using System.Linq;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop);
    }

    public void OnSettingsClick(object sender, RoutedEventArgs args) {
        var settingsWindow = new SettingsWindow {
            DataContext = new ViewModels.SettingsViewModel(),
        };
        settingsWindow.Show();
    }

    public void OnInfoClick(object sender, RoutedEventArgs args) {
        var url = "https://github.com/robbyklein/optimizerk";

        try {
            Process.Start(new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true,
            });
        } catch {
            // optionally handle errors
        }
    }

    public void BringToFront() {
        if (WindowState == WindowState.Minimized) {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    private void OnDragOver(object? sender, DragEventArgs e) {
        if (e.Data.Contains(DataFormats.Files)) {
            e.DragEffects = DragDropEffects.Copy;
        } else {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void OnDrop(object? sender, DragEventArgs e) {
        if (DataContext is MainWindowViewModel vm && e.Data.Contains(DataFormats.Files)) {
            var droppedFiles = e.Data.GetFiles() ?? [];
            vm.AddFiles(droppedFiles.Select(file => file.Path.LocalPath));
        }

        e.Handled = true;
    }
}
