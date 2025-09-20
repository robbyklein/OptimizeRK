namespace OptimizeRK;

using Avalonia.Controls;
using OptimizeRK.ViewModels;

public partial class SettingsWindow : Window {
    public SettingsWindow() {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}