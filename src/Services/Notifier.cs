namespace OptimizeRK.Services;

using Microsoft.Toolkit.Uwp.Notifications;
using OptimizeRK.Helpers;
using OptimizeRK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

/// <summary>
/// Shows a Windows notification when a batch of files finishes optimizing.
/// </summary>
public static class Notifier {
    // Windows titles notifications with the name and icon of the Start menu shortcut carrying this ID
    // (without one it shows the raw ID), so it must match AppUserModelID in installer/install-generator.iss.
    public const string AppId = "RobertKlein.OptimizeRK";

    private static readonly ManualResetEventSlim Activated = new();
    private static bool revealed;

    // Raised (on a background thread) when the user clicks the notification itself rather than a button.
    public static event Action? ShowRequested;

    // Must run before anything else touches notifications. Also groups the window with the shortcut on the taskbar.
    public static void SetAppId() {
        SetCurrentProcessExplicitAppUserModelID(AppId);
    }

    public static void Initialize() {
        ToastNotificationManagerCompat.OnActivated += OnActivated;
    }

    // Windows starts the app when a notification is clicked after the app was closed. Returns true
    // if that click was "Show in folder", which has been handled and needs no window.
    public static bool WasLaunchedToRevealFile() {
        if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated()) {
            return false;
        }

        Activated.Wait(TimeSpan.FromSeconds(5));
        return revealed;
    }

    public static void ShowSummary(IReadOnlyCollection<FileItem> batch) {
        var optimized = batch.Where(f => f.Status == ProcessStatus.Optimized).ToList();
        var failed = batch.Count(f => f.Status == ProcessStatus.Failed);

        // Nothing but unsupported files, which are left alone.
        if (optimized.Count == 0 && failed == 0) {
            return;
        }

        var builder = new ToastContentBuilder();

        if (optimized.Count == 0) {
            builder
                .AddText("Optimization failed")
                .AddText(failed == 1 ? $"Couldn't optimize {batch.First(f => f.Status == ProcessStatus.Failed).Name}." : $"Couldn't optimize {failed} files.");
        } else {
            long original = optimized.Sum(f => f.OriginalSize);
            long saved = original - optimized.Sum(f => f.NewSize);

            var message = saved > 0
                ? $"Saved {ByteSize.Format(saved)} ({100.0 * saved / original:0}% smaller)"
                : "No size reduction";

            if (failed > 0) {
                message += $" · {failed} failed";
            }

            var first = optimized[0];

            builder
                .AddText(optimized.Count == 1 ? $"Optimized {first.Name}" : $"Optimized {optimized.Count} files")
                .AddText(message)
                .AddButton(new ToastButton()
                    .SetContent("Show in folder")
                    .AddArgument("action", "reveal")
                    .AddArgument("path", first.OutputPath ?? first.Path));
        }

        try {
            builder.Show();
        } catch (Exception ex) {
            Debug.WriteLine($"[Notifier] Couldn't show notification: {ex.Message}");
        }
    }

    // Removes the registrations Windows keeps for the app's notifications. Run by the uninstaller.
    public static void Uninstall() {
        ToastNotificationManagerCompat.Uninstall();
    }

    private static void OnActivated(ToastNotificationActivatedEventArgsCompat e) {
        var args = ToastArguments.Parse(e.Argument);

        if (args.TryGetValue("action", out var action) && action == "reveal" && args.TryGetValue("path", out var path)) {
            RevealInExplorer(path);
            revealed = true;
        } else {
            ShowRequested?.Invoke();
        }

        Activated.Set();
    }

    private static void RevealInExplorer(string path) {
        if (File.Exists(path)) {
            Process.Start("explorer.exe", $"/select,\"{path}\"");
        } else if (Path.GetDirectoryName(path) is { } folder && Directory.Exists(folder)) {
            Process.Start("explorer.exe", $"\"{folder}\"");
        }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void SetCurrentProcessExplicitAppUserModelID(string appId);
}
