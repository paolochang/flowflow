namespace FlowFlow.Models;

public sealed class ScreenshotInfo
{
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public ScreenshotKind Kind { get; set; } = ScreenshotKind.Reference;
    public string Memo { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
}
