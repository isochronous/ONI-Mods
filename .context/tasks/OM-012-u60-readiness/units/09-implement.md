# Unit 09 — implement: review findings from unit 08

## Objective
Address the review findings in .context/tasks/OM-012-u60-readiness/units/08-review-report.md before the PRs open, then rerun the touched suites in game on U60.

## Scope
- IN:
  - Finding 1 (moderate, held lane): motion-sensor-range RangeSlider.cs:36-41 — bind the open delegate without throwing (`throwOnBindFailure: false` or equivalent) and fold its null check into `SensorInternalsFound`, so a future signature change reaches the existing missing-internals warning. This file's change lives on branch OM-012-u60-only — **you are on OM-012-u60-only; make only this edit there first**, then tell the coordinator in your report (the coordinator commits it).
  - Minor findings, ship-now lane (salty-surf Mod.cs, upgrade-planner UI/MaterialPicker.cs): Salty Surf logs one warning when neither descriptor target exists and when a translation lacks `{amount}`; use a declared-only lookup for `CollectDescriptors`; Upgrade Planner caches its reflection lookups and logs one warning naming the mod on a miss. These must keep working on both U59 744825 and U60 762230 (check against F:\ONI-Frozen\APP).
  - Build the three mods + their test projects; then one in-game run on U60: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SaltySurf,UpgradePlanner,MotionSensorRange -Steam 3468585385` (UpgradePlanner's Blueprints Expanded tests need that Workshop mod; one call so the game isn't relaunched per mod; never -KeepGame; don't stop a run mid-run).
- OUT: other findings (CBA test DLL on U59, submodule push, publish/ yaml — coordinator's); commits; branch switches beyond what's stated; releases.
- Acceptance: build output 0 errors; in-game suite counts with the runner's actual output. Do not claim success without showing evidence.

## Context the packet does not carry
- The salty-surf and upgrade-planner edits are ship-now-lane files; making them on OM-012-u60-only is fine — the coordinator moves those commits to OM-012-u60-readiness. Keep each mod's edits confined to its own files so they split cleanly.
- Unit 06 confirmed TestRunner never writes over Jeremy's saves; don't change that.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad. Keep files' existing line endings.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/09-implement-report.md
- Must contain: each finding → what changed (file:line); build output; in-game output.
- Model tier: default
