using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace FlowFlow.Services;

public sealed record HotkeyRegistration(string Chord, bool Succeeded, string? Error);

public sealed class GlobalHotkeyService : IDisposable
{
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint VK_1 = 0x31;
    public const uint VK_2 = 0x32;

    private const int WM_HOTKEY = 0x0312;
    private const int ERROR_HOTKEY_ALREADY_REGISTERED = 1409;
    private readonly Dictionary<int, Action> _actions = [];
    private readonly HashSet<int> _registeredHotkeyIds = [];
    private IntPtr _hwnd;
    private IntPtr _oldWndProc;
    private WndProcDelegate? _newWndProc;
    private bool _attached;

    public void Attach(Window window)
    {
        if (_attached)
        {
            return;
        }

        _hwnd = WindowNative.GetWindowHandle(window);
        _newWndProc = WndProc;
        _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
        _attached = true;
    }

    public HotkeyRegistration RegisterHotkey(int id, uint modifiers, uint vk, string chordLabel, Action handler)
    {
        if (!_attached)
        {
            return new HotkeyRegistration(chordLabel, false, "window not attached");
        }

        if (!RegisterHotKey(_hwnd, id, modifiers, vk))
        {
            var code = Marshal.GetLastWin32Error();
            var error = code == ERROR_HOTKEY_ALREADY_REGISTERED
                ? "already in use by another application"
                : $"Win32 error {code}";
            return new HotkeyRegistration(chordLabel, false, error);
        }

        _actions[id] = handler;
        _registeredHotkeyIds.Add(id);
        return new HotkeyRegistration(chordLabel, true, null);
    }

    public void Dispose()
    {
        if (!_attached)
        {
            return;
        }

        foreach (var id in _registeredHotkeyIds)
        {
            UnregisterHotKey(_hwnd, id);
        }

        if (_oldWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldWndProc);
        }

        _registeredHotkeyIds.Clear();
        _actions.Clear();
        _attached = false;
    }

    private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_HOTKEY && _actions.TryGetValue(wParam.ToInt32(), out var action))
        {
            action();
            return IntPtr.Zero;
        }

        return CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam);
    }

    private const int GWLP_WNDPROC = -4;

    private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
