using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Instancebul;

static class WindowManager
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;

    public static bool TryActivateWindow(string processName)
    {
        // Find all processes with the given name
        var processes = Process.GetProcessesByName(processName);
        
        if (processes.Length == 0)
        {
            return false;
        }

        // Find the best window to activate
        IntPtr targetWindow = IntPtr.Zero;
        
        foreach (var process in processes)
        {
            var mainWindow = process.MainWindowHandle;
            
            if (mainWindow != IntPtr.Zero && IsWindowVisible(mainWindow))
            {
                targetWindow = mainWindow;
                break;
            }
        }

        if (targetWindow == IntPtr.Zero)
        {
            // Try to get any window handle from the process
            foreach (var process in processes)
            {
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    targetWindow = process.MainWindowHandle;
                    break;
                }
            }
        }

        if (targetWindow == IntPtr.Zero)
        {
            return false;
        }

        // Activate the window
        ActivateWindow(targetWindow);
        return true;
    }

    private static void ActivateWindow(IntPtr hWnd)
    {
        // If window is minimized, restore it
        if (IsIconic(hWnd))
        {
            ShowWindow(hWnd, SW_RESTORE);
        }

        // Get the foreground window's thread
        IntPtr foregroundWindow = GetForegroundWindow();
        uint foregroundThread = GetWindowThreadProcessId(foregroundWindow, out _);
        uint currentThread = GetCurrentThreadId();

        // Attach to the foreground thread to steal focus
        if (foregroundThread != currentThread)
        {
            AttachThreadInput(currentThread, foregroundThread, true);
            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
            AttachThreadInput(currentThread, foregroundThread, false);
        }
        else
        {
            BringWindowToTop(hWnd);
            SetForegroundWindow(hWnd);
        }
    }

    public static void LaunchApplication(string path)
    {
        try
        {
            // Resolve finduwp: syntax
            if (path.StartsWith("finduwp:", StringComparison.OrdinalIgnoreCase))
            {
                var searchTerm = path.Substring("finduwp:".Length).Trim();
                var resolvedPath = UwpAppFinder.FindUwpApp(searchTerm);

                if (resolvedPath == null)
                {
                    MessageBox.Show(
                        $"Could not find UWP app matching: {searchTerm}\n\nMake sure the app is installed from the Microsoft Store.",
                        "Instancebul - UWP App Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                path = resolvedPath;
            }

            // Resolve path: syntax (find in PATH environment variable)
            if (path.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
            {
                var appName = path.Substring("path:".Length).Trim();

                // Add .exe if not present
                if (!appName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    appName += ".exe";
                }

                // Search in PATH
                var pathVar = Environment.GetEnvironmentVariable("PATH");
                if (pathVar != null)
                {
                    var paths = pathVar.Split(';');
                    foreach (var dir in paths)
                    {
                        if (string.IsNullOrWhiteSpace(dir)) continue;

                        var fullPath = Path.Combine(dir.Trim(), appName);
                        if (File.Exists(fullPath))
                        {
                            path = fullPath;
                            break;
                        }
                    }
                }

                // If still starts with path:, it wasn't found
                if (path.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        $"Could not find '{appName}' in PATH environment variable.\n\nMake sure the application is installed and in your PATH.",
                        "Instancebul - App Not Found in PATH",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
            }

            // Support ms-* protocols (for Windows settings, etc.)
            if (path.StartsWith("ms-", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                return;
            }

            // Regular file path
            if (!File.Exists(path))
            {
                MessageBox.Show(
                    $"Application not found:\n{path}",
                    "Instancebul",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to launch application:\n{ex.Message}",
                "Instancebul",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
