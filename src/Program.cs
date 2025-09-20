namespace OptimizeRK;

using Avalonia;
using System;

internal class Program {
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

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
