# Unit 10 — test: Steps S6 + S9, farm plot tests T6-T8 and the save round trip T9/T10

Steps S6 and S9: in-game tests proving that Duplicant-side planting works in downward-facing (FlipV) Hydroponic Farms and Farm Tiles (preview, deposit, irrigation, Copy Settings, the planter side screen), that upright plots are unchanged, and that ceiling Ovagro (wild with vines, and in a FlipV hydroponic farm) survive a save and load.
Ticket: OM-006

## Files to create/modify
mods/ovagro-overhead/src/OvagroOverhead.Tests/Plots.cs (T6 Hydroponic Farm FlipV, T7 Farm Tile FlipV + Neutral regression + Copy Settings between two downward tiles, T8 side screen) and Persistence.cs (T9/T10, second test FreshSave), as the S6 and S9 sections of units/04-plan-report.md and the design's Testability table (units/03-design-report.md) describe; extend Kit.cs where needed. Mod source is out of scope: report mod bugs with evidence, do not fix them.

## Pattern to follow
The suite as it stands (Kit.cs, Registration.cs, WildPlanting.cs, Vines.cs; units/08-test-report.md, units/09-test-report.md), mods/salty-surf/src/SaltySurf.Tests/Persistence.cs for the round trip, and the UI-test conventions of upgrade-planner's tests for T8 (widgets driven the way the input module does).

## Context the packet does not carry
- Run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` (and `-Filter` while iterating). If the harness says the game is running, stop and report it; never close a game you did not start. Never rebuild during a run. Leave the game closed and mods.json restored, as the harness does.
- Pace every step to game time; wait on conditions, never frames apart.
- If a test misbehaves and you cannot work out why after a reasonable look, stop iterating: screenshot the setup and the failing state (t.Screenshot, cropped with Pillow to the area that matters), and report the paths with what you expected versus what each shows. The user can often spot the cause from a picture.
- Take screenshots of a FlipV Hydroponic Farm and a FlipV Farm Tile each holding a grown ceiling Ovagro, of the planter side screen on a FlipV tile with Ovagro selectable, and of the loaded state after the round trip; look at them yourself and describe them.
- The whole suite (T1-T10) must pass together at the end, not only filtered runs.
- Commit trailers exactly: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9`.

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds before runs.
- The full suite run exits 0 with every test passing; the report pastes the status lines and the run folder.
- Screenshots as listed, paths in the report.
- Commit all created/modified test files on OM-006-ovagro-overhead, message `OM-006: farm plot and save tests` (one terse line). Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/10-test-report.md
- Must contain: pass/fail counts, status lines, what was written, screenshot paths with what each shows, mod bugs found (with evidence), Player.log findings, and a "Store reads" section.
- Model tier: complex
