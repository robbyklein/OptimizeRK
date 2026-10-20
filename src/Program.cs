namespace OptimizeRK;

using Avalonia;
using OptimizeRK.Services;
using System;
using System.IO;
using System.Linq;

internal class Program {
    [STAThread]
    public static void Main(string[] args) {
        Notifier.SetAppId();

        // Run by the uninstaller.
        if (args.Contains("--uninstall")) {
            Notifier.Uninstall();
            return;
        }

        // If OptimizeRK is already open, hand it the files (e.g. from right-click → Optimize with OptimizeRK).
        var files = args.Where(File.Exists).Select(Path.GetFullPath).ToArray();
        if (!SingleInstance.TryClaim() && SingleInstance.TrySend(files)) {
            return;
        }

        Notifier.Initialize();
        if (Notifier.WasLaunchedToRevealFile()) {
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() {
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg));
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.SvgImageExtension));

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
