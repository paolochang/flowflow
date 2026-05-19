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

## Local Setup

FlowFlow targets .NET 8, Windows App SDK, and Windows 10 version 1809
(`10.0.17763.0`) or newer.

Before compiling the app, install Visual Studio 2022 with the components listed
in `.vsconfig`. Visual Studio should prompt to install missing components when
the solution is opened. You can also import `.vsconfig` from Visual Studio
Installer.

For WinUI 3, Visual Studio 2022 should include:

* .NET desktop development
* Windows application development
* Windows App SDK tooling
* MSIX/PRI packaging components
* MSVC C++ tools, used by the XAML compiler

Check the local machine first:

```powershell
.\scripts\Test-BuildPrerequisites.ps1
```

If PowerShell blocks local scripts, run the same check with an execution-policy
bypass for this process:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-BuildPrerequisites.ps1
```

If Visual Studio is installed in a custom location, pass it explicitly:

```powershell
.\scripts\Test-BuildPrerequisites.ps1 -VisualStudioPath "D:\Apps\Visual Studio\2022\BuildTools"
```

Restore and compile from the repository root:

```powershell
dotnet restore FlowFlow.sln
dotnet build FlowFlow.sln -c Debug
```

Run the generated `FlowFlow.exe` from the `FlowFlow\bin\Debug\` output tree.

FlowFlow stores local data in `Documents\FlowFlow`. To reset local app data,
close the app and delete that folder.

If compilation fails with a missing `Microsoft.Build.Packaging.Pri.Tasks.dll`,
update the Visual Studio installation using `.vsconfig`; that task is provided
by the Windows app packaging components used by WinUI resource generation. If
compilation fails with a missing `VC\Tools\MSVC` directory, install the MSVC C++
toolchain from `.vsconfig`.

## Next MVP Steps

* Add system tray support with `H.NotifyIcon.WinUI`.
* Add a settings page for configurable hotkeys and storage root.
* Add thumbnail previews for screenshots.
* Add delete/rename flows for projects, tasks, and screenshots.
* Replace handwritten Win32 interop with `Microsoft.Windows.CsWin32`.

See [docs/PLANS.md](docs/PLANS.md) for the phased project plan and suggested sprint order.
