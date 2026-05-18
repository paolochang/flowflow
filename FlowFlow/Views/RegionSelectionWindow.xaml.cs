using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;

namespace FlowFlow;

public sealed partial class RegionSelectionWindow : Window
{
    private readonly TaskCompletionSource<System.Drawing.Rectangle?> _result = new();
    private Windows.Foundation.Point? _startPoint;

    public RegionSelectionWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        Activated += (_, _) => SetFullScreen();
    }

    public static Task<System.Drawing.Rectangle?> PickRegionAsync()
    {
        var window = new RegionSelectionWindow();
        window.Activate();
        return window._result.Task;
    }

    private void Overlay_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _startPoint = e.GetCurrentPoint((UIElement)sender).Position;
        SelectionRectangle.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRectangle, _startPoint.Value.X);
        Canvas.SetTop(SelectionRectangle, _startPoint.Value.Y);
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
    }

    private void Overlay_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_startPoint is null)
        {
            return;
        }

        var current = e.GetCurrentPoint((UIElement)sender).Position;
        var x = Math.Min(current.X, _startPoint.Value.X);
        var y = Math.Min(current.Y, _startPoint.Value.Y);
        var width = Math.Abs(current.X - _startPoint.Value.X);
        var height = Math.Abs(current.Y - _startPoint.Value.Y);

        Canvas.SetLeft(SelectionRectangle, x);
        Canvas.SetTop(SelectionRectangle, y);
        SelectionRectangle.Width = width;
        SelectionRectangle.Height = height;
    }

    private void Overlay_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_startPoint is null)
        {
            return;
        }

        var current = e.GetCurrentPoint((UIElement)sender).Position;
        var x = Math.Min(current.X, _startPoint.Value.X);
        var y = Math.Min(current.Y, _startPoint.Value.Y);
        var width = Math.Abs(current.X - _startPoint.Value.X);
        var height = Math.Abs(current.Y - _startPoint.Value.Y);

        if (width < 8 || height < 8)
        {
            _result.TrySetResult(null);
        }
        else
        {
            _result.TrySetResult(new System.Drawing.Rectangle((int)x, (int)y, (int)width, (int)height));
        }

        Close();
    }

    private void SetFullScreen()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
    }
}
