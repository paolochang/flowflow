using System.Text.Json;
using System.Text.RegularExpressions;
using FlowFlow.Models;

namespace FlowFlow.Services;

public sealed class FileSystemTaskStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _rootPath;

    public FileSystemTaskStore()
    {
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FlowFlow");
        Directory.CreateDirectory(_rootPath);
    }

    public string RootPath => _rootPath;

    public IReadOnlyList<ProjectInfo> GetProjects()
    {
        return Directory.GetDirectories(_rootPath)
            .Select(path => new ProjectInfo(Path.GetFileName(path), path))
            .OrderBy(project => project.Name)
            .ToList();
    }

    public ProjectInfo CreateProject(string name)
    {
        var projectName = Slugify(name);
        var path = Path.Combine(_rootPath, projectName);
        Directory.CreateDirectory(GetTasksPath(path));
        return new ProjectInfo(projectName, path);
    }

    public IReadOnlyList<TaskInfo> GetTasks(ProjectInfo project)
    {
        var tasksPath = GetTasksPath(project.Path);
        Directory.CreateDirectory(tasksPath);

        return Directory.GetDirectories(tasksPath)
            .Select(path => new TaskInfo(Path.GetFileName(path), path))
            .OrderBy(task => task.Name)
            .ToList();
    }

    public TaskInfo CreateTask(ProjectInfo project, string name)
    {
        var taskName = Slugify(name);
        var path = Path.Combine(GetTasksPath(project.Path), taskName);
        Directory.CreateDirectory(GetScreenshotsPath(path));
        EnsureTaskFiles(path);
        return new TaskInfo(taskName, path);
    }

    public IReadOnlyList<ScreenshotInfo> LoadScreenshots(TaskInfo task)
    {
        EnsureTaskFiles(task.Path);
        var metadataPath = GetMetadataPath(task.Path);
        var json = File.ReadAllText(metadataPath);
        return JsonSerializer.Deserialize<List<ScreenshotInfo>>(json, JsonOptions) ?? [];
    }

    public void SaveScreenshots(TaskInfo task, IEnumerable<ScreenshotInfo> screenshots)
    {
        EnsureTaskFiles(task.Path);
        File.WriteAllText(GetMetadataPath(task.Path), JsonSerializer.Serialize(screenshots, JsonOptions));
    }

    public string GetNextScreenshotPath(TaskInfo task)
    {
        Directory.CreateDirectory(GetScreenshotsPath(task.Path));
        var next = LoadScreenshots(task).Count + 1;
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(GetScreenshotsPath(task.Path), $"{next:000}-{timestamp}.png");
    }

    public void SaveResumePrompt(TaskInfo task, string markdown)
    {
        EnsureTaskFiles(task.Path);
        File.WriteAllText(Path.Combine(task.Path, "resume-prompt.md"), markdown);
    }

    public string LoadTaskNotes(TaskInfo task)
    {
        EnsureTaskFiles(task.Path);
        return File.ReadAllText(Path.Combine(task.Path, "task.md"));
    }

    public void SaveTaskNotes(TaskInfo task, string notes)
    {
        EnsureTaskFiles(task.Path);
        File.WriteAllText(Path.Combine(task.Path, "task.md"), notes);
    }

    private static string GetTasksPath(string projectPath) => Path.Combine(projectPath, ".ai-refs", "tasks");

    private static string GetScreenshotsPath(string taskPath) => Path.Combine(taskPath, "screenshots");

    private static string GetMetadataPath(string taskPath) => Path.Combine(taskPath, "screenshots.json");

    private static void EnsureTaskFiles(string taskPath)
    {
        Directory.CreateDirectory(taskPath);
        Directory.CreateDirectory(GetScreenshotsPath(taskPath));

        var taskMarkdown = Path.Combine(taskPath, "task.md");
        if (!File.Exists(taskMarkdown))
        {
            File.WriteAllText(taskMarkdown, "# Task Notes\r\n\r\n");
        }

        var metadataPath = GetMetadataPath(taskPath);
        if (!File.Exists(metadataPath))
        {
            File.WriteAllText(metadataPath, "[]");
        }

        var resumePromptPath = Path.Combine(taskPath, "resume-prompt.md");
        if (!File.Exists(resumePromptPath))
        {
            File.WriteAllText(resumePromptPath, string.Empty);
        }
    }

    private static string Slugify(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-");
        slug = Regex.Replace(slug, @"^-+|-+$", string.Empty);
        return string.IsNullOrWhiteSpace(slug) ? $"untitled-{DateTime.Now:yyyyMMddHHmmss}" : slug;
    }
}
