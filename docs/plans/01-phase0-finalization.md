# PR-01 / PLAN-01 - Phase 0 Finalization

> Source: `docs/PLANS.md` Phase 0 - Scaffold Stabilization
> Predecessor: PR #1 (`.vsconfig` + dynamic VS path resolution, merged into `main`)

## Scope

Finish Phase 0 by completing local installation of the missing Visual Studio /
Windows SDK packaging components, verifying the end-to-end happy path manually,
and turning those findings into a README setup section. No new features, no
refactors.

Linked checklist items in `docs/PLANS.md` (Phase 0):

- [ ] Install the missing Visual Studio / Windows SDK packaging components locally and confirm WinUI verification.
- [x] Confirm `dotnet restore FlowFlow.sln` succeeds on a clean checkout.
- [ ] Confirm `dotnet build FlowFlow.sln -c Debug` succeeds.
- [ ] Run the app locally and verify the main window opens without runtime errors.
- [ ] Manually test project creation, task creation, full-screen capture, region capture, note editing, prompt generation, and clipboard copy.
- [x] Document known machine setup requirements in `README.md`.

## Goals / Success Criteria

1. `dotnet restore FlowFlow.sln` succeeds on a clean clone with no warnings beyond expected SDK or offline NuGet notices.
2. `dotnet build FlowFlow.sln -c Debug` succeeds and produces a runnable WinUI app.
3. Running the built binary opens `MainWindow` without exceptions in the debug output.
4. Manual happy-path QA below passes end-to-end on the dev machine.
5. `README.md` has a "Local Setup" section that a new contributor can follow without external help.

## Out of Scope

- Any code change to capture, hotkey, store, prompt generator, or view models.
- New tests or test infrastructure.
- CI / GitHub Actions (Phase 7).
- Tray icon, settings, packaging.

## Implementation Steps

### Step 1 - Install missing setup components locally

- Run the preflight script added in PR #1 and record exactly which workloads /
  SDKs it reports missing.
- Install the missing components through Visual Studio Installer. Do not edit
  `.vsconfig` again unless preflight surfaces a genuinely missing entry; that
  would be a separate PR.
- Re-run preflight until it passes.

Observed on 2026-05-18:

- `.\scripts\Test-BuildPrerequisites.ps1` is blocked by the local PowerShell execution policy.
- `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Test-BuildPrerequisites.ps1` runs and reports:
  - [x] Visual Studio 2022 MSBuild present.
  - [ ] MSVC toolchain missing.
  - [ ] Windows app packaging PRI tasks missing.

### Step 2 - Verify clean restore + build

From a fresh shell at repo root:

```powershell
dotnet restore FlowFlow.sln
dotnet build FlowFlow.sln -c Debug
```

Canonical build command: `dotnet build FlowFlow.sln -c Debug`.

Capture the final lines of each command's output in the PR description. If
either command fails, stop and document the missing manual setup requirement in
README; do not silently patch source, project, solution, or preflight files with
workarounds.

Observed on 2026-05-18:

- `dotnet restore FlowFlow.sln` completed successfully with an offline NuGet vulnerability-data warning.
- `dotnet build FlowFlow.sln -c Debug` failed because WinUI PRI packaging tasks are not installed on this machine.
- A direct Visual Studio MSBuild check also failed because the MSVC toolchain is not installed.
- No `FlowFlow.exe` was produced.

### Step 3 - Manual happy-path QA

Run the built app and walk through this exact sequence, noting any defect in
the PR body. Defects become Phase 1 PRs rather than fixes in this PR.

1. Launch the app; main window opens, no exception dialog.
2. Create a new project (use a plain ASCII name such as `qa-smoke`).
3. Create a new task under that project.
4. Trigger full-screen capture via `Ctrl+Shift+1`.
5. Trigger region capture via `Ctrl+Shift+2` and select a small region.
6. Edit a screenshot memo and confirm it persists after restart.
7. Generate / refresh the resume prompt.
8. Copy resume prompt to clipboard and paste into Notepad to verify content.
9. Inspect `Documents\FlowFlow\qa-smoke\.ai-refs\tasks\<task>\` and confirm:
   - `screenshots/` contains both PNGs.
   - `screenshots.json` has matching entries.
   - `resume-prompt.md` matches the clipboard content.

Observed on 2026-05-18: items 3-1 through 3-8 were skipped because Step 2 did
not produce a runnable executable. Item 3-9 was also skipped for the same reason.

### Step 4 - README setup section

Replace the old README build instructions with a "Local Setup" section covering:

- Required OS (Windows 10/11 + minimum build).
- Required Visual Studio edition and the workloads/components the preflight script checks.
- How to run preflight.
- Restore + canonical build command.
- Where local data is stored (`Documents\FlowFlow`) and how to wipe it.
- Known limitations observed during QA.

## Files Likely To Change

- `README.md` - replace old setup instructions with a "Local Setup" section.
- `docs/plans/01-phase0-finalization.md` - record measured verification status.
- No code changes expected. If any prove unavoidable, split them into a follow-up PR.

## Verification Checklist

- [ ] Preflight script passes on a clean shell.
- [x] `dotnet restore` output captured.
- [x] `dotnet build -c Debug` output captured.
- [ ] Manual QA sequence completed; skipped until the missing VS components are installed.
- [x] README "Local Setup" section reviewed by reading top-to-bottom as a new contributor.

## Risks / Notes

- If installing components rewrites global VS state, note it in the PR so reviewers know the environment changed.
- If the QA sequence uncovers a real crash, the right move is to stop this PR at "documented defect" and let Plan 02 absorb the fix; do not balloon scope.
- The current machine cannot complete Phase 0 until MSVC and Windows app packaging PRI tasks are installed from `.vsconfig`.

## Exit Signal

Phase 0 is ready to close only after preflight passes, `dotnet build
FlowFlow.sln -c Debug` produces `FlowFlow.exe`, and the manual QA sequence above
passes or records defects for follow-up without changing source code.
