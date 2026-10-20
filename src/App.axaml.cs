namespace OptimizeRK;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OptimizeRK.Services;
using OptimizeRK.ViewModels;

/// <summary>
/// Represents the main application class for OptimizeRK.
/// </summary>
public partial class App : Application {
    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var viewModel = new MainWindowViewModel();
            var window = new MainWindow {
                DataContext = viewModel,
            };
            desktop.MainWindow = window;

            // Files passed on the command line, e.g. from right-click → Optimize with OptimizeRK
            viewModel.AddFiles(desktop.Args ?? []);

            // Files from copies of the app started while this one is open
            SingleInstance.Listen(files => Dispatcher.UIThread.Post(() => {
                viewModel.AddFiles(files);
                window.BringToFront();
            }));

            Notifier.ShowRequested += () => Dispatcher.UIThread.Post(window.BringToFront);
        }

        base.OnFrameworkInitializationCompleted();
    }
}