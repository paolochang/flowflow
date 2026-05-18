# FlowFlow

FlowFlow is a Windows-native, local-first WinUI 3 utility for preserving screenshots and task context during AI coding workflows.

## MVP

Implemented in this scaffold:

* Create/select projects.
* Create/select tasks inside a project.
* Store each task as local files under `.ai-refs/tasks/task-name`.
* Capture full-screen screenshots with `Ctrl+Shift+1`.
* Capture a dragged region with `Ctrl+Shift+2` or the Region button.
* Save screenshot metadata in `screenshots.json`.
* Edit task notes in `task.md`.
* Generate and save `resume-prompt.md`.
* Copy the generated resume prompt to the clipboard.
* Use WinUI theme resources for native light/dark mode behavior.

## Storage

FlowFlow stores data in `Documents\FlowFlow`:

```text
project-name/
  .ai-refs/
    tasks/
      task-name/
        screenshots/
        task.md
        screenshots.json
        resume-prompt.md
```

## Build

The app targets .NET 8 and Windows App SDK:

```powershell
dotnet restore FlowFlow.sln
dotnet build FlowFlow.sln -c Debug
```

For WinUI 3, Visual Studio 2022 should include:

* .NET desktop development
* Windows application development
* Windows App SDK tooling
* MSIX/PRI packaging build tools
* MSVC C++ tools, used by the XAML compiler

On this machine, restore succeeded and source compilation started, but the installed VS/Windows SDK environment is missing `Microsoft.Build.Packaging.Pri.Tasks.dll`, so full local build verification is blocked until that workload component is installed.

## Next MVP Steps

* Add system tray support with `H.NotifyIcon.WinUI`.
* Add a settings page for configurable hotkeys and storage root.
* Add thumbnail previews for screenshots.
* Add delete/rename flows for projects, tasks, and screenshots.
* Replace handwritten Win32 interop with `Microsoft.Windows.CsWin32`.
