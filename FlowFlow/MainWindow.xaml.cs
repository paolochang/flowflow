using System.Drawing;
using static FlowFlow.Services.GlobalHotkeyService;
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
            new ClipboardService(),
            DispatcherQueue);
        ShellRoot.DataContext = _viewModel;

        SetWindowChrome();
        _hotkeyService.Attach(this);
        var fullScreenResult = _hotkeyService.RegisterHotkey(
            1,
            MOD_CONTROL | MOD_SHIFT,
            VK_1,
            "Ctrl+Shift+1",
            () => _ = DispatcherQueue.TryEnqueue(async () => await _viewModel.CaptureFullScreenAsync()));
        var regionResult = _hotkeyService.RegisterHotkey(
            2,
            MOD_CONTROL | MOD_SHIFT,
            VK_2,
            "Ctrl+Shift+2",
            () => _ = DispatcherQueue.TryEnqueue(async () => await CaptureRegionAsync()));
        _viewModel.ReportHotkeyResults([fullScreenResult, regionResult]);
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
            _viewModel.SetStatus("Region capture canceled or selection too small.", StatusSeverity.Info);
            return;
        }

        try
        {
            await _captureService.CaptureRegionAsync(region.Value, path);
            _viewModel.AddCapturedRegion(path);
        }
        catch (Exception ex)
        {
            _viewModel.ReportCaptureFailure(ex);
        }
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
