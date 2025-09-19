using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OptimizeRK.Models;
using OptimizeRK.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OptimizeRK;

public partial class MainWindow : Window {

	public MainWindow() {
		InitializeComponent();


		DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
		DropZone.AddHandler(DragDrop.DropEvent, OnDrop);

	}

	public void OnSettingsClick(object sender, RoutedEventArgs args) {
		var settingsWindow = new SettingsWindow();
		settingsWindow.DataContext = new ViewModels.SettingsViewModel();
		settingsWindow.Show();
	}

	public void OnInfoClick(object sender, RoutedEventArgs args) {
		var url = "https://github.com/robbyklein/optimizerk";

		try {
			Process.Start(new ProcessStartInfo {
				FileName = url,
				UseShellExecute = true
			});
		} catch {
			// optionally handle errors
		}
	}

	private void OnDragOver(object? sender, DragEventArgs e) {
		if (e.Data.Contains(DataFormats.Files))
			e.DragEffects = DragDropEffects.Copy;
		else
			e.DragEffects = DragDropEffects.None;

		e.Handled = true;
	}

	private async void OnDrop(object? sender, DragEventArgs e) {
		if (DataContext is MainWindowViewModel vm && e.Data.Contains(DataFormats.Files)) {
			var droppedFiles = e.Data.GetFiles() ?? [];

			var newItems = new List<FileItem>();

			foreach (var file in droppedFiles) {
				var path = file.Path.LocalPath;
				if (File.Exists(path)) {
					var originalInfo = new FileInfo(path);

					var item = new Models.FileItem {
						Name = originalInfo.Name,
						Path = originalInfo.FullName,
						OriginalSize = originalInfo.Length,
						NewSize = originalInfo.Length,
						Status = ProcessStatus.Pending
					};

					vm.Files.Add(item);
					newItems.Add(item); // track only newly added
				}
			}

			if (newItems.Count > 0) {
				var settings = Settings.Load();
				var optimizer = new Services.FileOptimizer(settings);

				// Optimize only the new items
				await optimizer.OptimizeFilesAsync(newItems);
			}
		}

		e.Handled = true;
	}
}
