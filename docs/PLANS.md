# FlowFlow Project Plans

This document organizes the next work for FlowFlow into practical phases. Each phase should leave the app more usable for a real AI coding workflow, not just more complete on paper.

## Product Direction

FlowFlow is a Windows-native, local-first context capture tool for AI-assisted coding. The core promise is simple:

* Capture the current visual state of a task quickly.
* Keep screenshots, notes, and resume prompts together in predictable local files.
* Make it easy to restart an AI coding session with the right context.

Non-goals for the near term:

* No cloud sync.
* No user accounts.
* No AI API dependency.
* No database unless local files become a proven bottleneck.

## Phase 0 - Scaffold Stabilization

Goal: make the existing WinUI scaffold buildable, runnable, and trustworthy.

Checklist:

* [x] Declare the required Visual Studio / Windows SDK packaging components in `.vsconfig` and add a local preflight check.
* [ ] Install the missing Visual Studio / Windows SDK packaging components locally and confirm WinUI build verification.
* [ ] Confirm `dotnet restore FlowFlow.sln` succeeds on a clean checkout.
* [ ] Confirm `dotnet build FlowFlow.sln -c Debug` succeeds.
* [ ] Run the app locally and verify the main window opens without runtime errors.
* [ ] Manually test project creation, task creation, full-screen capture, region capture, note editing, prompt generation, and clipboard copy.
* [ ] Document any known machine setup requirements in `README.md`.

Exit criteria:

* A new contributor can clone, restore, build, and run the app using the README.
* The current scaffold has a verified happy path.

## Phase 1 - Capture Workflow Hardening

Goal: make the screenshot capture loop reliable enough for daily use.

Checklist:

* [ ] Add error handling around full-screen and region capture failures.
* [ ] Show clear status messages when global hotkey registration fails.
* [ ] Validate empty, duplicate, or invalid project and task names before creating folders.
* [ ] Handle corrupt or missing `screenshots.json` gracefully.
* [ ] Confirm region capture works with common DPI scaling settings.
* [ ] Confirm region capture works across single-monitor and multi-monitor setups.
* [ ] Prevent zero-size or tiny accidental captures from creating confusing metadata.
* [ ] Add basic logging only if manual debugging becomes painful.

Exit criteria:

* Capture failures do not crash the app.
* The user always gets a clear status message after a capture attempt.
* Local task data remains recoverable even if metadata is malformed.

## Phase 2 - Screenshot Management MVP

Goal: turn captured screenshots from raw file records into usable task context.

Checklist:

* [ ] Add thumbnail previews in the screenshots list.
* [ ] Add a selected screenshot preview panel.
* [ ] Add delete flow for screenshots, including metadata update.
* [ ] Add "open image" action.
* [ ] Add "open containing folder" action.
* [ ] Decide whether screenshot memo changes should auto-save or require explicit save.
* [ ] Make screenshot type labels easy to scan.

Exit criteria:

* A user can inspect, label, memo, open, and delete screenshots without leaving the app.
* The resume prompt accurately reflects the current screenshot list.

## Phase 3 - Project And Task Management

Goal: make long-running projects manageable after the first few tasks.

Checklist:

* [ ] Add rename flow for projects.
* [ ] Add rename flow for tasks.
* [ ] Add delete flow for projects with confirmation.
* [ ] Add delete flow for tasks with confirmation.
* [ ] Add archive or completed state for tasks.
* [ ] Remember the last selected project and task on app restart.
* [ ] Add search or filtering if the task list becomes noisy.

Exit criteria:

* The app remains usable after dozens of tasks.
* Destructive actions are explicit and hard to trigger accidentally.

## Phase 4 - Tray-First Daily Utility

Goal: let FlowFlow behave like a small always-available Windows utility.

Checklist:

* [ ] Add `H.NotifyIcon.WinUI`.
* [ ] Add system tray icon.
* [ ] Add tray menu actions: Show, Capture full screen, Capture region, Exit.
* [ ] Decide close behavior: close window, minimize to tray, or exit.
* [ ] Add startup behavior option if needed.
* [ ] Ensure hotkeys continue working when the main window is hidden.

Exit criteria:

* FlowFlow can stay out of the way while still supporting fast capture.
* Tray behavior is predictable and documented.

## Phase 5 - Settings And Preferences

Goal: make the app configurable without complicating the core model.

Checklist:

* [ ] Add a local settings file.
* [ ] Add setting for storage root path.
* [ ] Add setting for full-screen capture hotkey.
* [ ] Add setting for region capture hotkey.
* [ ] Add theme setting if system theme is not enough.
* [ ] Add reset-to-defaults behavior.
* [ ] Validate settings before saving.

Exit criteria:

* Users can adapt FlowFlow to their local workflow without editing files manually.
* Invalid settings cannot silently break capture behavior.

## Phase 6 - Interop And Architecture Cleanup

Goal: reduce native interop risk before the app grows larger.

Checklist:

* [ ] Replace handwritten Win32 interop with `Microsoft.Windows.CsWin32` where practical.
* [ ] Introduce interfaces for store, capture, clipboard, and hotkey services if tests or settings need them.
* [ ] Keep filesystem storage as the default persistence layer.
* [ ] Review service lifetimes and window lifetime behavior.
* [ ] Add focused unit tests for filename generation, slugging, metadata loading, and prompt generation.

Exit criteria:

* Native API usage is easier to audit.
* Core non-UI behavior can be tested without launching WinUI.

## Phase 7 - Packaging And Release

Goal: make FlowFlow installable and shareable.

Checklist:

* [ ] Add GitHub Actions build workflow.
* [ ] Produce release artifacts for Windows.
* [ ] Decide packaged vs unpackaged distribution strategy.
* [ ] Add app icon and basic visual identity.
* [ ] Add screenshots or a short demo GIF to `README.md`.
* [ ] Add release checklist.
* [ ] Tag the first usable MVP release.

Exit criteria:

* A user can download, install or run, and understand FlowFlow without building from source.

## Suggested Sprint Order

### Sprint 1 - Build And Happy Path

Focus:

* Phase 0 completion.
* Manual QA of the existing scaffold.
* Small stability fixes discovered while testing.

Recommended first PRs:

* Build environment documentation update.
* Capture error handling.
* Project/task name validation.

### Sprint 2 - Screenshot UX

Focus:

* Phase 2 screenshot management.
* Preview, open, delete, and metadata workflow.

Recommended first PRs:

* Screenshot thumbnail list.
* Selected screenshot preview.
* Screenshot delete flow.

### Sprint 3 - Daily Utility

Focus:

* Phase 4 tray behavior.
* Basic settings groundwork if required by tray behavior.

Recommended first PRs:

* Add notify icon package and tray menu.
* Hide/show window behavior.
* Hotkey behavior while hidden.

### Sprint 4 - Manageability

Focus:

* Phase 3 project/task management.
* Last active state restore.

Recommended first PRs:

* Rename task/project.
* Delete task/project.
* Remember last selected task.

### Sprint 5 - Release Prep

Focus:

* Phase 6 and Phase 7 enough for a public MVP.
* Tests for core services.
* CI and release artifact.

Recommended first PRs:

* Prompt generator and storage tests.
* GitHub Actions build.
* App icon and README demo assets.

## Current Priority

The immediate next move should be Phase 0. Until build and happy-path verification are complete, larger UX or tray work will be harder to trust.
