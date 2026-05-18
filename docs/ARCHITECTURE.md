# FlowFlow Architecture

FlowFlow is a local-first WinUI 3 desktop utility for preserving AI coding task context.

## Project Layout

```text
FlowFlow/
  Models/        Data records for projects, tasks, screenshots, labels
  Services/      Filesystem persistence, screenshot capture, hotkeys, clipboard, prompt generation
  ViewModels/    MVVM state and commands
  Views/         Secondary WinUI windows
  MainWindow.*   Native desktop shell
```

## Local Storage

Projects live under `Documents\FlowFlow`. Each project stores tasks with this structure:

```text
project-name/
  .ai-refs/
    tasks/
      task-name/
        screenshots/
          001-20260518-131500.png
        task.md
        screenshots.json
        resume-prompt.md
```

This keeps the app backend-free, inspectable, portable, and easy to attach to agent workflows.

## Recommended NuGet Packages

Implemented:

* `Microsoft.WindowsAppSDK` - WinUI 3 desktop app runtime.
* `CommunityToolkit.Mvvm` - observable view models and commands.

Recommended next:

* `H.NotifyIcon.WinUI` - system tray integration.
* `Microsoft.Windows.CsWin32` - generated Win32 interop instead of handwritten P/Invoke.
* `Serilog.Extensions.Logging.File` - optional local diagnostics if needed.

## MVP Feature Decisions

* Filesystem persistence is in `FileSystemTaskStore`, keeping storage replaceable without adding a database.
* Resume markdown is generated from the current task state and saved to `resume-prompt.md`.
* Screenshots are captured with local Windows APIs and stored as PNGs in the active task.
* Global hotkeys are registered on the main window: `Ctrl+Shift+1` for full desktop, `Ctrl+Shift+2` for region capture.
* No AI APIs, auth, OCR, cloud sync, embeddings, or backend server are included.
