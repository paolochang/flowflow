using Windows.ApplicationModel.DataTransfer;

namespace FlowFlow.Services;

public sealed class ClipboardService
{
    public void SetText(string text)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }
}
