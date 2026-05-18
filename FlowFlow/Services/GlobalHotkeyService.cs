using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace FlowFlow.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_1 = 0x31;
    private const uint VK_2 = 0x32;
    private readonly Dictionary<int, Action> _actions = [];
    private IntPtr _hwnd;
    private IntPtr _oldWndProc;
    private WndProcDelegate? _newWndProc;
    private bool _registered;

    public void Register(Window window, Action captureFullScreen, Action captureRegion)
    {
        _hwnd = WindowNative.GetWindowHandle(window);
        _actions[1] = captureFullScreen;
        _actions[2] = captureRegion;
        _newWndProc = WndProc;
        _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));

        RegisterHotKey(_hwnd, 1, MOD_CONTROL | MOD_SHIFT, VK_1);
        RegisterHotKey(_hwnd, 2, MOD_CONTROL | MOD_SHIFT, VK_2);
        _registered = true;
    }

    public void Dispose()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_hwnd, 1);
        UnregisterHotKey(_hwnd, 2);

        if (_oldWndProc != IntPtr.Zero)
        {
            SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldWndProc);
        }
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
