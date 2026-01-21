using System.Runtime.InteropServices;

namespace Instancebul;

class HotkeyManager
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    // Modifier keys
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    private readonly IntPtr _windowHandle;
    private readonly List<int> _registeredIds = new();

    public HotkeyManager(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }

    public bool RegisterHotkey(int id, string hotkeyString)
    {
        if (!TryParseHotkey(hotkeyString, out uint modifiers, out uint keyCode))
        {
            return false;
        }

        // Add NOREPEAT to prevent repeated firing when holding the key
        modifiers |= MOD_NOREPEAT;

        if (RegisterHotKey(_windowHandle, id, modifiers, keyCode))
        {
            _registeredIds.Add(id);
            return true;
        }

        return false;
    }

    public void UnregisterAll()
    {
        foreach (var id in _registeredIds)
        {
            UnregisterHotKey(_windowHandle, id);
        }
        _registeredIds.Clear();
    }

    private bool TryParseHotkey(string hotkeyString, out uint modifiers, out uint keyCode)
    {
        modifiers = 0;
        keyCode = 0;

        var parts = hotkeyString.Split('+', StringSplitOptions.TrimEntries);
        
        foreach (var part in parts)
        {
            var upper = part.ToUpperInvariant();
            
            switch (upper)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    // This should be the key
                    keyCode = GetVirtualKeyCode(upper);
                    if (keyCode == 0)
                    {
                        return false;
                    }
                    break;
            }
        }

        return modifiers != 0 && keyCode != 0;
    }

    private uint GetVirtualKeyCode(string key)
    {
        // Single character keys
        if (key.Length == 1)
        {
            char c = key[0];
            
            // Letters A-Z
            if (c >= 'A' && c <= 'Z')
            {
                return (uint)c;
            }
            
            // Numbers 0-9
            if (c >= '0' && c <= '9')
            {
                return (uint)c;
            }
        }

        // Function keys
        if (key.StartsWith("F") && int.TryParse(key.Substring(1), out int fNum) && fNum >= 1 && fNum <= 24)
        {
            return (uint)(0x70 + fNum - 1); // VK_F1 = 0x70
        }

        // Special keys
        return key switch
        {
            "SPACE" => 0x20,
            "ENTER" => 0x0D,
            "RETURN" => 0x0D,
            "TAB" => 0x09,
            "ESCAPE" => 0x1B,
            "ESC" => 0x1B,
            "BACKSPACE" => 0x08,
            "DELETE" => 0x2E,
            "DEL" => 0x2E,
            "INSERT" => 0x2D,
            "INS" => 0x2D,
            "HOME" => 0x24,
            "END" => 0x23,
            "PAGEUP" => 0x21,
            "PGUP" => 0x21,
            "PAGEDOWN" => 0x22,
            "PGDN" => 0x22,
            "UP" => 0x26,
            "DOWN" => 0x28,
            "LEFT" => 0x25,
            "RIGHT" => 0x27,
            "PRINTSCREEN" => 0x2C,
            "PRTSC" => 0x2C,
            "SCROLLLOCK" => 0x91,
            "PAUSE" => 0x13,
            "NUMLOCK" => 0x90,
            "CAPSLOCK" => 0x14,
            // Numpad
            "NUMPAD0" => 0x60,
            "NUMPAD1" => 0x61,
            "NUMPAD2" => 0x62,
            "NUMPAD3" => 0x63,
            "NUMPAD4" => 0x64,
            "NUMPAD5" => 0x65,
            "NUMPAD6" => 0x66,
            "NUMPAD7" => 0x67,
            "NUMPAD8" => 0x68,
            "NUMPAD9" => 0x69,
            "MULTIPLY" => 0x6A,
            "ADD" => 0x6B,
            "SUBTRACT" => 0x6D,
            "DECIMAL" => 0x6E,
            "DIVIDE" => 0x6F,
            // OEM keys (common ones)
            ";" => 0xBA,
            "SEMICOLON" => 0xBA,
            "=" => 0xBB,
            "EQUALS" => 0xBB,
            "," => 0xBC,
            "COMMA" => 0xBC,
            "-" => 0xBD,
            "MINUS" => 0xBD,
            "." => 0xBE,
            "PERIOD" => 0xBE,
            "/" => 0xBF,
            "SLASH" => 0xBF,
            "`" => 0xC0,
            "BACKTICK" => 0xC0,
            "TILDE" => 0xC0,
            "[" => 0xDB,
            "OPENBRACKET" => 0xDB,
            "\\" => 0xDC,
            "BACKSLASH" => 0xDC,
            "]" => 0xDD,
            "CLOSEBRACKET" => 0xDD,
            "'" => 0xDE,
            "QUOTE" => 0xDE,
            _ => 0
        };
    }
}
