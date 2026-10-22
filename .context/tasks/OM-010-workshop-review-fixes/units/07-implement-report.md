# Unit 07 report: Just Fall Already log gate and critter scan

## Changes
- `mods/just-fall-already/src/JustFallAlready/FallPoke.cs`
  - New `private static readonly bool LogPokes = StartedByTestRunner();` read once at class init: true when any command-line argument starts with `--onitest-` (`System.Environment.GetCommandLineArgs()`, ordinal compare).
  - The per-poke "... told it to fall" line is written only when `LogPokes` is true; its text is unchanged, so Doors.cs `WatchPokes` counts it as before (tests run under the runner). The Critters Fall Through Open Doors found/not-found line and the slow-handler line are unchanged.
  - `CrittersAt`: the once-per-frame walk over `Components.Brains` now checks `Grid.PosToCell` + `armed.ContainsKey` first and only runs `IsRunning`/`GetComponent<StateMachineController>`/`GetSMI<CreatureFallMonitor.Instance>` for brains in an armed cell, so the dictionary holds armed cells only. Cell lists are recycled through a `spareLists` stack instead of allocating new lists each frame. Still lazy (only on the first armed hit in a frame).
  - `Door_SetWorldState_Patch` sets `cacheFrame = -1` after arming, so a lookup later in the same frame sees critters in newly armed cells (the old cache could miss them; behaviour otherwise identical).
  - Doc comments: class summary Telemetry paragraph, `LogPokes`, `crittersByCell`, `CrittersAt`.
- `mods/just-fall-already/CHANGELOG.md`: new `## Unreleased` heading with "- No longer writes a log line for each critter it drops, and spends less time finding critters next to doors."
- Doors.cs untouched; no new or changed tests.

## Store follow-ups (for the coordinator)
- blk-9d52ccfdb3df "Telemetry: each poke is logged" should become "each poke is logged when started by the test runner (--onitest-*)".
- blk-7d0c65d332f3 member table: `crittersByCell` is now "critters in armed cells, gathered at most once per frame"; add `LogPokes`.

## Build output
```
dotnet build mods/just-fall-already/src/JustFallAlready/JustFallAlready.csproj -c Release -p:ModDeployFolder=none
  All projects are up-to-date for restore.
  JustFallAlready -> C:\dev\ONI-Mods\mods\just-fall-already\src\JustFallAlready\bin\Release\JustFallAlready.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:00.67

dotnet build mods/just-fall-already/src/JustFallAlready.Tests/JustFallAlready.Tests.csproj -c Release -p:ModDeployFolder=none
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.05
```
Not run in game (no launch per spec); the Doors.cs suite is the check for the gated line and the new lookup.
