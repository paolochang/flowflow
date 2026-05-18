using System.Text;
using FlowFlow.Models;

namespace FlowFlow.Services;

public sealed class ResumePromptGenerator
{
    public string Generate(TaskInfo task, IEnumerable<ScreenshotInfo> screenshots, string taskNotes)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Resume Task Context");
        builder.AppendLine();
        builder.AppendLine($"Task: {task.Name}");
        builder.AppendLine();
        builder.AppendLine("Reference screenshots:");
        builder.AppendLine();

        foreach (var screenshot in screenshots)
        {
            var label = ToDisplayLabel(screenshot.Kind);
            var note = string.IsNullOrWhiteSpace(screenshot.Memo) ? string.Empty : $" - {screenshot.Memo.Trim()}";
            builder.AppendLine($"* `{screenshot.RelativePath}` ({label}){note}");
        }

        builder.AppendLine();
        builder.AppendLine("User Notes:");
        builder.AppendLine();
        builder.AppendLine(string.IsNullOrWhiteSpace(taskNotes) ? "* No notes yet." : taskNotes.Trim());
        builder.AppendLine();
        builder.AppendLine("Instruction:");
        builder.AppendLine("Please continue this task using the screenshots and notes above.");

        return builder.ToString();
    }

    private static string ToDisplayLabel(ScreenshotKind kind)
    {
        return kind switch
        {
            ScreenshotKind.BugState => "bug-state",
            ScreenshotKind.ExpectedState => "expected-state",
            ScreenshotKind.ConsoleError => "console-error",
            ScreenshotKind.Reference => "reference",
            _ => "reference"
        };
    }
}
