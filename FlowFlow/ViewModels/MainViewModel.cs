using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowFlow.Models;
using FlowFlow.Services;

namespace FlowFlow.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly FileSystemTaskStore _store;
    private readonly ScreenshotCaptureService _captureService;
    private readonly ResumePromptGenerator _promptGenerator;
    private readonly ClipboardService _clipboardService;

    private string _newProjectName = string.Empty;
    private string _newTaskName = string.Empty;
    private ProjectInfo? _selectedProject;
    private TaskInfo? _selectedTask;
    private ScreenshotInfo? _selectedScreenshot;
    private string _taskNotes = string.Empty;
    private string _resumePrompt = string.Empty;
    private string _statusText = "Choose or create a project to start.";

    public MainViewModel(
        FileSystemTaskStore store,
        ScreenshotCaptureService captureService,
        ResumePromptGenerator promptGenerator,
        ClipboardService clipboardService)
    {
        _store = store;
        _captureService = captureService;
        _promptGenerator = promptGenerator;
        _clipboardService = clipboardService;
        RefreshProjects();
    }

    public ObservableCollection<ProjectInfo> Projects { get; } = [];
    public ObservableCollection<TaskInfo> Tasks { get; } = [];
    public ObservableCollection<ScreenshotInfo> Screenshots { get; } = [];
    public IReadOnlyList<ScreenshotKind> ScreenshotKinds { get; } = Enum.GetValues<ScreenshotKind>();

    public string RootPath => _store.RootPath;

    public string NewProjectName
    {
        get => _newProjectName;
        set => SetProperty(ref _newProjectName, value);
    }

    public string NewTaskName
    {
        get => _newTaskName;
        set => SetProperty(ref _newTaskName, value);
    }

    public ProjectInfo? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
            {
                RefreshTasks();
            }
        }
    }

    public TaskInfo? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetProperty(ref _selectedTask, value))
            {
                RefreshActiveTask();
            }
        }
    }

    public ScreenshotInfo? SelectedScreenshot
    {
        get => _selectedScreenshot;
        set => SetProperty(ref _selectedScreenshot, value);
    }

    public string TaskNotes
    {
        get => _taskNotes;
        set
        {
            if (!SetProperty(ref _taskNotes, value) || SelectedTask is null)
            {
                return;
            }

            _store.SaveTaskNotes(SelectedTask, value);
            RegeneratePrompt();
        }
    }

    public string ResumePrompt
    {
        get => _resumePrompt;
        set => SetProperty(ref _resumePrompt, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    [RelayCommand]
    private void CreateProject()
    {
        var project = _store.CreateProject(NewProjectName);
        NewProjectName = string.Empty;
        RefreshProjects();
        SelectedProject = Projects.FirstOrDefault(item => item.Path == project.Path);
        StatusText = $"Created project {project.Name}.";
    }

    [RelayCommand]
    private void CreateTask()
    {
        if (SelectedProject is null)
        {
            StatusText = "Select a project before creating a task.";
            return;
        }

        var task = _store.CreateTask(SelectedProject, NewTaskName);
        NewTaskName = string.Empty;
        RefreshTasks();
        SelectedTask = Tasks.FirstOrDefault(item => item.Path == task.Path);
        StatusText = $"Created task {task.Name}.";
    }

    [RelayCommand]
    public async Task CaptureFullScreenAsync()
    {
        if (SelectedTask is null)
        {
            StatusText = "Select a task before capturing.";
            return;
        }

        var path = _store.GetNextScreenshotPath(SelectedTask);
        await _captureService.CaptureFullScreenAsync(path);
        AddScreenshot(path, ScreenshotKind.Reference);
        StatusText = $"Saved {Path.GetFileName(path)}.";
    }

    [RelayCommand]
    public void SaveScreenshotMetadata()
    {
        PersistScreenshots();
        RegeneratePrompt();
        StatusText = "Screenshot metadata saved.";
    }

    [RelayCommand]
    public void CopyResumePrompt()
    {
        RegeneratePrompt();
        _clipboardService.SetText(ResumePrompt);
        StatusText = "Resume prompt copied to clipboard.";
    }

    [RelayCommand]
    public void RegeneratePrompt()
    {
        if (SelectedTask is null)
        {
            ResumePrompt = string.Empty;
            return;
        }

        ResumePrompt = _promptGenerator.Generate(SelectedTask, Screenshots, TaskNotes);
        _store.SaveResumePrompt(SelectedTask, ResumePrompt);
    }

    public void AddCapturedRegion(string path)
    {
        AddScreenshot(path, ScreenshotKind.Reference);
        StatusText = $"Saved {Path.GetFileName(path)}.";
    }

    public string? ReserveNextScreenshotPath()
    {
        return SelectedTask is null ? null : _store.GetNextScreenshotPath(SelectedTask);
    }

    private void AddScreenshot(string path, ScreenshotKind kind)
    {
        if (SelectedTask is null)
        {
            return;
        }

        var info = new ScreenshotInfo
        {
            FileName = Path.GetFileName(path),
            RelativePath = Path.Combine("screenshots", Path.GetFileName(path)).Replace('\\', '/'),
            Kind = kind,
            CreatedAt = DateTimeOffset.Now
        };

        Screenshots.Add(info);
        SelectedScreenshot = info;
        PersistScreenshots();
        RegeneratePrompt();
    }

    private void RefreshProjects()
    {
        Projects.Clear();
        foreach (var project in _store.GetProjects())
        {
            Projects.Add(project);
        }
    }

    private void RefreshTasks()
    {
        Tasks.Clear();
        Screenshots.Clear();
        SelectedTask = null;

        if (SelectedProject is null)
        {
            return;
        }

        foreach (var task in _store.GetTasks(SelectedProject))
        {
            Tasks.Add(task);
        }
    }

    private void RefreshActiveTask()
    {
        Screenshots.Clear();
        SelectedScreenshot = null;

        if (SelectedTask is null)
        {
            TaskNotes = string.Empty;
            ResumePrompt = string.Empty;
            return;
        }

        foreach (var screenshot in _store.LoadScreenshots(SelectedTask))
        {
            Screenshots.Add(screenshot);
        }

        TaskNotes = _store.LoadTaskNotes(SelectedTask);
        RegeneratePrompt();
        StatusText = $"Active task: {SelectedTask.Name}.";
    }

    private void PersistScreenshots()
    {
        if (SelectedTask is not null)
        {
            _store.SaveScreenshots(SelectedTask, Screenshots);
        }
    }
}
