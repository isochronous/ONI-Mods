# Unit 07 — implement + test: Smooth Borders on U60

## Objective
Unit 06's smoke test found Smooth Borders logging "DrawWorldBorder has changed; borders stay as the game draws them" on U60 762230: its patch detects the change and no-ops, so border smoothing is off on U60 while its tests still pass. Unit 01's IL/patch check missed it. Make Smooth Borders smooth borders on U60, and close the test gap so a no-op patch fails a test.

## Scope
- IN: mods/smooth-borders (mod and test project). Compare vanilla `DrawWorldBorder` (and whatever it calls) between U59 (F:\ONI-Frozen\APP) and U60 (F:\SteamLibrary\steamapps\common\OxygenNotIncluded); update the patch. Prefer a change that works on both builds; if that isn't reasonably possible, say so and make it U60-only (then raise `minimumSupportedBuild` to 762230 in the source mod_info.yaml and tell the coordinator — that change belongs on the held branch). Add or strengthen a test in its test project so that the suite fails when the patch is inactive (follow the existing tests' patterns).
- Run the Smooth Borders suite in game on U60: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders` (one call; never -KeepGame; don't stop a run mid-run). Also confirm the new test fails with the patch disabled if that's cheap to show (e.g. against the unfixed build), otherwise explain why it would.
- Save safety: unit 06 confirmed TestRunner never writes over Jeremy's saves; don't change that.
- OUT: other mods; commits; releases; Steam branch switches.
- Acceptance: build output with 0 errors; the in-game suite output showing pass counts; Player.log no longer carrying the "DrawWorldBorder has changed" line. Your report must include the actual output — do not claim success without it.

## Context the packet does not carry
- Branch: OM-012-u60-only checked out (the both-builds lane is OM-012-u60-readiness; the coordinator will move commits). Don't switch branches.
- Unit 06 report: .context/tasks/OM-012-u60-readiness/units/06-test-report.md. Unit 01 report (IL diff method): units/01-explore-u60-breakage.md; its tooling is in the scratchpad.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad, never the repo root. Keep files' existing line endings.
- If a test fails for a reason you can't explain, stop and screenshot the setup and the failure state; don't loop.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/07-implement-report.md
- Must contain: what changed in vanilla; files changed and why; BOTH-BUILDS or U60-ONLY; build + in-game output; why unit 01's check missed it (one paragraph, so the method can be fixed).
- Model tier: complex
