using Avalonia.Controls;
using OptimizeRK.ViewModels;

namespace OptimizeRK;

public partial class SettingsWindow : Window {
	public SettingsWindow() {
		InitializeComponent();
		DataContext = new SettingsViewModel();
	}
}