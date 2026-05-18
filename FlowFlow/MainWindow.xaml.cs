using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using FlowFlow.Services;
using FlowFlow.ViewModels;
using WinRT.Interop;

namespace FlowFlow;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ScreenshotCaptureService _captureService;
    private readonly GlobalHotkeyService _hotkeyService = new();

    public MainWindow()
    {
        InitializeComponent();

        _captureService = new ScreenshotCaptureService();
        _viewModel = new MainViewModel(
            new FileSystemTaskStore(),
            _captureService,
            new ResumePromptGenerator(),
            new ClipboardService());
        ShellRoot.DataContext = _viewModel;

        SetWindowChrome();
        _hotkeyService.Register(
            this,
            () => _ = DispatcherQueue.TryEnqueue(async () => await _viewModel.CaptureFullScreenAsync()),
            () => _ = DispatcherQueue.TryEnqueue(async () => await CaptureRegionAsync()));
        Closed += (_, _) => _hotkeyService.Dispose();
    }

    private async void CaptureRegion_Click(object sender, RoutedEventArgs e)
    {
        await CaptureRegionAsync();
    }

    private async Task CaptureRegionAsync()
    {
        var path = _viewModel.ReserveNextScreenshotPath();
        if (path is null)
        {
            _viewModel.StatusText = "Select a task before capturing.";
            return;
        }

        var region = await RegionSelectionWindow.PickRegionAsync();
        if (region is null)
        {
            _viewModel.StatusText = "Region capture canceled.";
            return;
        }

        await _captureService.CaptureRegionAsync(region.Value, path);
        _viewModel.AddCapturedRegion(path);
    }

    private void SetWindowChrome()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Title = "FlowFlow";
        appWindow.Resize(new Windows.Graphics.SizeInt32(1220, 760));
    }
}
