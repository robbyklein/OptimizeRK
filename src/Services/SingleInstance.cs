namespace OptimizeRK.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Keeps OptimizeRK to one window. Explorer starts a separate copy of the app for every file
/// selected when using "Optimize with OptimizeRK", so later copies pass their files to the
/// first one over a named pipe and exit.
/// </summary>
public static class SingleInstance {
    private const int ConnectTimeoutMs = 10_000;
    private const int AsfwAny = -1;

    private static readonly string Name =
        $"OptimizeRK-{Environment.UserName}-{Process.GetCurrentProcess().SessionId}";

    // Held for the life of the process so the next copy of the app can see this one is running.
    private static Mutex? mutex;
    private static bool isPrimary;

    // Returns true if this is the first copy of the app.
    public static bool TryClaim() {
        mutex = new Mutex(true, $@"Local\{Name}", out isPrimary);
        return isPrimary;
    }

    // Sends files to the first copy of the app, which also brings its window to the front.
    public static bool TrySend(IEnumerable<string> files) {
        try {
            using var client = new NamedPipeClientStream(".", Name, PipeDirection.Out, PipeOptions.CurrentUserOnly);
            client.Connect(ConnectTimeoutMs);

            // Windows only lets the process the user just started take focus, so pass that on.
            AllowSetForegroundWindow(AsfwAny);

            using var writer = new StreamWriter(client, new UTF8Encoding(false));
            foreach (var file in files) {
                writer.WriteLine(file);
            }

            return true;
        } catch (Exception ex) when (ex is TimeoutException or IOException) {
            return false;
        }
    }

    // Calls onFilesReceived (on a background thread) each time another copy of the app sends files.
    public static void Listen(Action<string[]> onFilesReceived) {
        if (!isPrimary) {
            return;
        }

        _ = Task.Run(async () => {
            while (true) {
                try {
                    await using var server = new NamedPipeServerStream(
                        Name,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

                    await server.WaitForConnectionAsync();

                    using var reader = new StreamReader(server, Encoding.UTF8);
                    var text = await reader.ReadToEndAsync();
                    onFilesReceived(text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
                } catch (IOException) {
                    // The other copy exited mid-send; keep listening.
                }
            }
        });
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(int processId);
}
