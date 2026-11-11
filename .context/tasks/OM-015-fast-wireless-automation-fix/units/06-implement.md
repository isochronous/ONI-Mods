# Unit 06 — implement: review fixes

## Objective
Fix the review findings in units/05-review-report.md (findings 1, 2, 3, 4, 5, 6; 7 at your discretion; 8 is no action).

## Scope
- Main repo (branch OM-015-fast-wireless-automation-fix):
  - Finding 1: `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix.Tests/SecondLoad.cs` — the test must not pass vacuously: the reflection lookups the injection needs are asserted in a test body, and `LoadAgain` fails unless the stale registration was actually saved/left (a marker the cleanup sets, as the reviewer suggests, or a better way).
  - Finding 2: `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/Mod.cs` — drop the `Game.DestroyInstances` postfix; keep only the `Game.OnPrefabInit` prefix. Update the class doc comment and log line to match. (Jeremy approved this.)
- common submodule, currently on its branch OM-015-fast-wireless-automation-fix at b7844e1:
  - Finding 3: end the load capture in `Session.Begin` and `Session.Finish` (Session.cs ~462) so errors don't carry into the next run in a reused game.
  - Finding 4: the `LoadErrors` doc comment says what it covers.
  - Finding 5: `SetLoadErrors` copies under the lock.
  - Finding 6: add `LoadErrors` to the TestRunner README. `entities.md:114` is a `.context/` record — do NOT edit it; put the exact sentence it should gain in your report and the manager lands it.
- OUT: no game launches, no commits, no edits under `.context/` other than your report.
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — include the summary lines; then build `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix` with the normal deploy so the local mods folder has the one-patch build, and show its output.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/06-implement-report.md
- Must contain: per-finding change with file:line, build outputs, the proposed entities.md sentence.
- Model tier: default
