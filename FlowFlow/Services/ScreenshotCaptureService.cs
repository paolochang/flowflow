using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FlowFlow.Services;

public sealed class ScreenshotCaptureService
{
    public Task CaptureFullScreenAsync(string path)
    {
        return Task.Run(() =>
        {
            var left = GetSystemMetrics(SystemMetric.SM_XVIRTUALSCREEN);
            var top = GetSystemMetrics(SystemMetric.SM_YVIRTUALSCREEN);
            var width = GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN);
            var height = GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN);
            CaptureRectangle(new Rectangle(left, top, width, height), path);
        });
    }

    public Task CaptureRegionAsync(Rectangle region, string path)
    {
        return Task.Run(() => CaptureRectangle(region, path));
    }

    private static void CaptureRectangle(Rectangle region, string path)
    {
        using var bitmap = new Bitmap(region.Width, region.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(region.Left, region.Top, 0, 0, region.Size, CopyPixelOperation.SourceCopy);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        bitmap.Save(path, ImageFormat.Png);
    }

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(SystemMetric metric);

    private enum SystemMetric
    {
        SM_XVIRTUALSCREEN = 76,
        SM_YVIRTUALSCREEN = 77,
        SM_CXVIRTUALSCREEN = 78,
        SM_CYVIRTUALSCREEN = 79
    }
}
