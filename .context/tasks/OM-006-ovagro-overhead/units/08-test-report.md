# Unit 08 report: S4a test kit and T1 (PASSED in game on the second attempt)

## In-game run (supersedes the BLOCKED section below)
Command: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead`, exit 0, after the user closed the game. No test-code fixes were needed, so no further commit; the commit stays 2834d21 (trailers there are Sonnet 5.5; the coordinator asked for the Opus 5.5 line for any fix commit, none was made).
- Run folder: `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261017-235414`
- Console: `PASS Registration.Prefabs (0.0 s)`, `PASS Registration.NaturalCeilingHelper (3.1 s)`; "Session done: 2 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped".
- results.jsonl:
```
{"id":"OvagroOverhead.Tests/Registration.Prefabs",...,"layer":0,"status":"pass","message":null,"durationSeconds":0.00,"gameSeconds":0.00,"logErrors":[],"notes":[]}
{"id":"OvagroOverhead.Tests/Registration.NaturalCeilingHelper",...,"layer":0,"status":"pass","message":null,"durationSeconds":3.07,"gameSeconds":3.07,"logErrors":[],"notes":["built DevLifeSupport at 122849 (101, 193)","boxed 38x8 at (101, 186) in 96 tiles of IgneousRock; air filled, a dev life support in the corner","screenshot ...\artifacts\OvagroOverhead.Tests.Registration.NaturalCeilingHelper-natural-ceiling.png"]}
```
- Player.log (run folder): `[03:54:36.754] [1] [INFO] [OvagroOverhead] Loaded version 0.1.0.0`. No OvagroOverhead warning (grep of "warn" with "ovagro": no lines), 0 lines containing "Exception".
- Screenshot: `C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261017-235414\artifacts\OvagroOverhead.Tests.Registration.NaturalCeilingHelper-natural-ceiling.png` (2560x1440). Cropped view: two white tile rows (box top and floor) with a band of natural sandstone-textured rock between them in the upper part, its underside lit gold-edged and irregular (not a tile), open teal air below it; dark igneous rock to the right.
- Cleanup: game process gone after the run (tasklist empty); Documents mods.json identical to the run's mods.json.real (restored).
- Mod bugs: none. Harness friction: none this run.

# (earlier attempt, kept for history)

## Result
- Pass/fail counts: none. 0 passed, 0 failed, 0 run. The game was already running, so `Invoke-OniTests.ps1 -Mods OvagroOverhead` refused to start: `Invoke-OniTests.ps1:147: The game is already running; close it first.`
- The running process is `OxygenNotIncluded` PID 47828, started 2026-10-17 14:03, still responding at 23:51, Player.log last written 22:21. That is not a harness session (the harness's own session stays up only 15 s after a run). I took it to be the user's game and did not close or kill it. I never launched anything.
- No results.jsonl lines, no run folder, no screenshots, no Player.log checks. T1 and the helper test are unproven in game. Acceptance for the in-game parts is NOT met.

## Done
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`: 0 errors (289 warnings, none from the new files after switching to `IsContentSubscribed`).
- `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead -BuildOnly`: "Built and checked against the game assemblies", exit 0 (ModCheck passes).
- Committed (not run in game) as `OM-006: test kit and registration test` with the trailers from the session reminder (Claude Sonnet 5.5 plus the session URL; the spec text said Opus 5.5, the harness reminder overrides it).

## Written (mods/ovagro-overhead/src/OvagroOverhead.Tests/)
- `Kit.cs` (internal static `Kit`): `NaturalRow/NaturalCeiling/NaturalFloor(t, inside, x0, x1, y)` (coroutines: `SimMessages.ReplaceElement(cell, SandStone, CellEventLogger.Instance.SandBoxTool, element.defaultValues.mass, 293.15f)` per cell, as `Dupes.Box` calls it for air; then `WaitUntil` solid and not Foundation and element SandStone, 5 s; then `WaitGameSeconds(1)`), `IsNaturalRow`, `Spawn(t, prefabId, cell, layer)`, `Seed`, `ForceMature` (Maturity amount to max).
- `Registration.cs`:
  - `REGISTRATION: the ceiling Ovagro and its preview are registered and the vanilla plant and seed are untouched` (`NoColony`, Layer 0): DLC4 subscribed; ceiling prefab Hanging and HideFromCodex; vanilla not Hanging; OccupyArea (0,0),(0,-1); UprootedMonitor (0,1); SeedProducer seedId `VineMotherSeed`; temperature limits and irrigation tag/rate equal the vanilla prefab's; preview occupies (0,-1); seed still Top with `VineMother` / `VineMother_preview`.
  - `REGISTRATION: the natural-ceiling helper lays sandstone that is solid and not Foundation` (`Boxed`, Layer 0): `Dupes.Box()`, 2 game s, sandstone row at `inside.H - 2` from x 2 to W-3, asserts solid, not Foundation, SandStone, cell below open; `LookAt` then `Screenshot("natural-ceiling")`.
- T1 does not check the "Loaded version" line or that the mod is active in the mod manager; those come from Player.log after a run.

## Mod bugs found
None (no run).

## To finish
1. User closes the game (or says the running process is disposable).
2. `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead -Filter "*REGISTRATION*"`; paste results.jsonl and run folder; check Player.log for `[OvagroOverhead] Loaded version` and no OvagroOverhead warning; view the screenshot in `<run>/artifacts/`.
3. Fix whatever the first real run shows (likely candidates: `Grid.Element[cell].id`, the `NoColony` test sharing a run with a colony test, `DlcManager.IsContentSubscribed`). Amend in a follow-up commit.

## Harness friction
- The launcher's refusal names no process, start time or owner, so I had to find PID 47828 with Get-Process to tell a user game from a stale harness one.

## Store reads
Runs (cwd C:/dev/ONI-Mods, `node c:/dev/ai/icon/bin/ks.mjs`):
1. `read blk-960faf85db39 blk-33fb80177cba blk-db81d810e051 blk-41ffba520973 blk-3753681f851b blk-976d76b5a752 blk-a66da6f9aaa2 blk-46b28a0040e8 blk-0194e02d0fd6`: all 9 resolved in one call (7637 B).
2. `brief --store common --for common/tools/TestRunner`: 8 pointers; none opened further (the TestRunner code-style/entities records did not bear on writing a test; I read Kit/Dupes.cs, Attributes.cs, TestContext.cs, World.cs directly).
Informed the work: blk-960faf85db39 (commands, exit codes, one game at a time), blk-33fb80177cba and blk-db81d810e051 (pacing), blk-41ffba520973 (test project layout), blk-a66da6f9aaa2 (box/fixture/cleanup), blk-0194e02d0fd6 (Assert API).
Report-back: nothing in the packet failed to apply; nothing governing was missing; the packet's brief found no records for `in-game-testing && test-pacing` (its own note) and spent its budget on just-fall-already code-style copies, which did not apply. Reason lines accurate.

## ICON friction
- The handoff's brief again filled with per-mod code-style copies rather than the testing records; the root testing ids named by the caller were what mattered.
- `ks read` with nine ids worked first call. Nothing else.
