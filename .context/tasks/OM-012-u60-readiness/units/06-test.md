# Unit 06 — test: in-game test suites of every touched mod on the U60 testing branch

## Objective
Plan step "In-game tests per touched mod" of OM-012. Run the in-game suites on the installed U60 testing build (762230) from the tip of branch `OM-012-u60-only` (superset of the ship-now fixes), and report pass/fail per test with evidence. Jeremy is asleep and has said to use the PC as necessary, so full suites and sequential runs are fine tonight.

## Scope
- IN, in this order:
  0. **Save safety first.** On U60 the one irreversible action is writing over an existing save (a U60 save won't open in the live build). Before any run, read how TestRunner/ModTest handle saves (NoColony tests, fixture saves, autosave, any save-on-exit) and confirm no test run writes to a save in Jeremy's save folder that existed before. Also check autosave can't fire during a run onto a pre-existing save. If any path could, stop and report it — don't run.
  1. Confirm the ONI-moono launch parameters being gone doesn't break the runner (it may have relied on them).
  2. TestRunner itself: confirm the new CapturePng patch keeps timelapse/preview images out of the player's folder (check the folder before and after a run).
  3. Suites, each with a minimal mod loadout (the mod under test plus what it needs): salty-surf, upgrade-planner, corner-build-anything, bitshifter, jumper-bridges, motion-sensor-range, multichannel-critter-sensor, smart-weight-plate, ovagros-overhead. Use `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <Mod>` (see the testing pointers in the packet for the exact flags).
  4. The in-game checks the fix reports list (units/02..05 reports): Salty Surf info panel text, Upgrade Planner material picker preselect + rebuild paused/unpaused (rebuild now on next tick), Jumper Bridges overlay tint, Ovagro's Overhead yield/growth/harvest/wilt animations, event subscription behaviour changes in the five mods. Where an existing test covers it, cite the test; where none does and a test is quick to add in the mod's test project following existing patterns, add it; otherwise list it as untested.
- OUT: changing mod code to make a failing test pass (report the failure with evidence instead — screenshot the setup and failure state as the stuck-tests rule says); releases; switching Steam branches; commits.
- Never use -KeepGame; never stop a runner mid-run; pace test steps to game time (no frames-apart build/destroy).
- Acceptance: per-suite pass/fail counts with the runner's actual output; save-folder listing before/after showing no pre-existing save changed (timestamps + sizes); timelapse-folder before/after. Do not claim success without showing evidence.

## Context the packet does not carry
- Fix reports: .context/tasks/OM-012-u60-readiness/units/02-implement-report.md, 03-implement-report.md, 04-implement-report.md, 05-implement-report.md.
- Game install: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (testing branch). Copy Player.log aside before each launch if the previous run crashed.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad, never the repo root.
- Harness changes are welcome if a test needs something new (no workarounds in test code); say what you changed in common/.
- Screenshots: save them in the scratchpad and list their paths in the report.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/06-test-report.md
- Must contain: save-safety finding; per-suite results table with output excerpts; each in-game check → test name / result / untested; failures with screenshots and Player.log excerpts; any test or harness files added/changed.
- Model tier: default
