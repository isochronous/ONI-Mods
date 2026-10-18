# Unit 08 — test: Step S4a, test kit helpers and T1 (registration)

Step S4a: the test project's shared helpers (a natural ceiling made in code, and whatever S5-S9 will reuse) and test T1, which proves the mod loads with its DLC requirement and registers `VineMotherCeiling` and its preview as the design specifies.
Ticket: OM-006

## Files to create/modify
Under mods/ovagro-overhead/src/OvagroOverhead.Tests/, as the S4a section of units/04-plan-report.md lists. Mod source is out of scope: if a test shows a mod bug, report it with the evidence; do not fix mod code.

## Pattern to follow
mods/just-fall-already/src/JustFallAlready.Tests (Doors.cs) and the TestRunner kit (common/tools/TestRunner, README and Kit/). The natural ceiling: the plan's helper laying natural sandstone with `SimMessages.ReplaceElement` (read the exact arguments from the kit's Dupes.cs), because built tiles are Foundation and the seed's ground check rejects them.

## Context the packet does not carry
- Tests run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead` (fixture testbed-so.sav, all DLCs active). The user's request to make sure the vines grow covers launching the game for test runs. Never leave the game running or the user's mods.json changed afterwards; the harness restores both, so do not work around it. Never rebuild while a run is in progress.
- Pace every step to game time: wait game seconds (1-2 after building or clearing, more after sim changes such as ReplaceElement) and wait on conditions, never frames apart. The user has had to point this out several times.
- Screenshots: `yield return t.Screenshot("name")` writes a PNG into the run's artifacts folder; name each one and list the paths in the report.
- Prior reports: units/03-design-report.md (Testability, T1-T10), units/04-plan-report.md (S4a), units/06-implement-report.md and units/07-implement-report.md (what was built).
- Watch Player.log for repeating exceptions during runs; copy any exception text into the report.

## Acceptance criteria
- T1 passes in game; the report includes the results.jsonl lines for the run and the run folder path. Do not claim a pass without the output.
- The natural-ceiling helper is exercised once (a short test or part of T1) and a screenshot shows the sandstone where expected.
- Player.log shows the mod's "Loaded version" line and no OvagroOverhead warning; quote both checks.
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds before the run.
- Commit all created/modified test files in one commit on OM-006-ovagro-overhead: `OM-006: test kit and registration test`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/08-test-report.md
- Must contain: pass/fail counts, the results.jsonl lines, what was written, screenshot paths, any mod bug found (with evidence), harness friction, and a "Store reads" section.
- Model tier: default
