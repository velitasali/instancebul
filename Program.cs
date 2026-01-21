using System.Text.Json;
using Microsoft.Win32;

namespace Instancebul;

static class Program
{
    private static NotifyIcon? _trayIcon;
    private static HotkeyManager? _hotkeyManager;
    private static List<ShortcutConfig>? _shortcuts;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Ensure single instance
        using var mutex = new Mutex(true, "Instancebul_SingleInstance", out bool isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show("Instancebul is already running!", "Instancebul", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Load configuration
        if (!LoadConfig())
        {
            return;
        }

        // Create hidden window for receiving hotkey messages
        var messageWindow = new MessageWindow();
        
        // Initialize hotkey manager
        _hotkeyManager = new HotkeyManager(messageWindow.Handle);
        
        // Register hotkeys
        RegisterHotkeys();

        // Setup system tray
        SetupTrayIcon();

        // Show startup notification
        _trayIcon?.ShowBalloonTip(3000, "Instancebul", "Running in the background. Right-click the tray icon to configure.", ToolTipIcon.Info);

        // Handle hotkey events
        messageWindow.HotkeyPressed += OnHotkeyPressed;

        // Run the application
        Application.Run();

        // Cleanup
        _hotkeyManager.UnregisterAll();
        _trayIcon?.Dispose();
    }

    private static bool LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        
        if (!File.Exists(configPath))
        {
            // Create default config
            var defaultConfig = new Config
            {
                Shortcuts = new List<ShortcutConfig>
                {
                    new ShortcutConfig
                    {
                        Hotkey = "Alt+F9",
                        ProcessName = "Code",
                        LaunchPath = @"C:\Program Files\Microsoft VS Code\Code.exe"
                    },
                    new ShortcutConfig
                    {
                        Hotkey = "Alt+F10",
                        ProcessName = "chrome",
                        LaunchPath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
                    },
                    new ShortcutConfig
                    {
                        Hotkey = "Alt+F11",
                        ProcessName = "AppleMusic",
                        LaunchPath = @"finduwp:AppleMusic"
                    },
                    new ShortcutConfig
                    {
                        Hotkey = "Ctrl+Alt+T",
                        ProcessName = "WindowsTerminal",
                        LaunchPath = @"finduwp:WindowsTerminal"
                    }
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, options));
            
            MessageBox.Show(
                $"Created default config at:\n{configPath}\n\nPlease edit it and restart the app.",
                "Instancebul - First Run",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            return false;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(json);
            _shortcuts = config?.Shortcuts ?? new List<ShortcutConfig>();

            if (_shortcuts.Count == 0)
            {
                MessageBox.Show("No shortcuts defined in config.json", "Instancebul", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        catch (System.Text.Json.JsonException ex)
        {
            MessageBox.Show(
                $"Invalid JSON in config file:\n\n{ex.Message}\n\n" +
                $"Common issues:\n" +
                $"• Missing comma between entries\n" +
                $"• Unclosed brackets or quotes\n" +
                $"• Extra comma after last entry\n" +
                $"• Single backslash (use \\\\ instead)\n\n" +
                $"Location: {configPath}",
                "Instancebul - Config Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading config:\n\n{ex.Message}\n\nLocation: {configPath}",
                "Instancebul",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return false;
        }
    }

    private static void RegisterHotkeys()
    {
        if (_shortcuts == null || _hotkeyManager == null) return;

        for (int i = 0; i < _shortcuts.Count; i++)
        {
            var shortcut = _shortcuts[i];
            if (!_hotkeyManager.RegisterHotkey(i, shortcut.Hotkey))
            {
                MessageBox.Show(
                    $"Failed to register hotkey: {shortcut.Hotkey}\nIt may be in use by another application.",
                    "Instancebul",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }

    private static void OnHotkeyPressed(int id)
    {
        if (_shortcuts == null || id < 0 || id >= _shortcuts.Count) return;

        var shortcut = _shortcuts[id];
        var expandedPath = Environment.ExpandEnvironmentVariables(shortcut.LaunchPath);

        // Try to find and activate existing window
        if (WindowManager.TryActivateWindow(shortcut.ProcessName))
        {
            return;
        }

        // Launch the application
        WindowManager.LaunchApplication(expandedPath);
    }

    private static void SetupTrayIcon()
    {
        // Try to load custom icon, fall back to system icon
        Icon? customIcon = null;
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
            if (File.Exists(iconPath))
            {
                customIcon = new Icon(iconPath);
            }
        }
        catch
        {
            // Fall back to system icon if custom icon fails to load
        }

        _trayIcon = new NotifyIcon
        {
            Icon = customIcon ?? SystemIcons.Application,
            Visible = true,
            Text = "Instancebul"
        };

        var contextMenu = new ContextMenuStrip();
        
        contextMenu.Items.Add("Open Config", null, (s, e) =>
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = configPath,
                UseShellExecute = true
            });
        });

        contextMenu.Items.Add("Reload Config", null, (s, e) =>
        {
            _hotkeyManager?.UnregisterAll();
            if (LoadConfig())
            {
                RegisterHotkeys();
                _trayIcon.ShowBalloonTip(1000, "Instancebul", "Configuration reloaded!", ToolTipIcon.Info);
            }
        });

        contextMenu.Items.Add("-");

        var startOnBootItem = new ToolStripMenuItem("Start on Boot")
        {
            CheckOnClick = true,
            Checked = IsStartOnBootEnabled()
        };
        startOnBootItem.Click += (s, e) =>
        {
            var item = (ToolStripMenuItem)s!;
            SetStartOnBoot(item.Checked);
            _trayIcon?.ShowBalloonTip(1000, "Instancebul",
                item.Checked ? "Enabled start on boot" : "Disabled start on boot",
                ToolTipIcon.Info);
        };
        contextMenu.Items.Add(startOnBootItem);

        contextMenu.Items.Add("-");

        contextMenu.Items.Add("Exit", null, (s, e) =>
        {
            Application.Exit();
        });

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) =>
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = configPath,
                UseShellExecute = true
            });
        };
    }

    private static bool IsStartOnBootEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
            var value = key?.GetValue("Instancebul");
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    private static void SetStartOnBoot(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;

            if (enabled)
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (exePath != null)
                {
                    key.SetValue("Instancebul", $"\"{exePath}\"");
                }
            }
            else
            {
                key.DeleteValue("Instancebul", false);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to update start on boot setting:\n{ex.Message}",
                "Instancebul",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}

// Hidden window to receive hotkey messages
class MessageWindow : Form
{
    public event Action<int>? HotkeyPressed;

    public MessageWindow()
    {
        this.ShowInTaskbar = false;
        this.WindowState = FormWindowState.Minimized;
        this.Visible = false;
        this.FormBorderStyle = FormBorderStyle.None;
        this.Load += (s, e) => this.Visible = false;
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY = 0x0312;
        
        if (m.Msg == WM_HOTKEY)
        {
            int id = m.WParam.ToInt32();
            HotkeyPressed?.Invoke(id);
        }

        base.WndProc(ref m);
    }
}

// Configuration classes
class Config
{
    public List<ShortcutConfig> Shortcuts { get; set; } = new();
}

class ShortcutConfig
{
    public string Hotkey { get; set; } = "";
    public string ProcessName { get; set; } = "";
    public string LaunchPath { get; set; } = "";
}
