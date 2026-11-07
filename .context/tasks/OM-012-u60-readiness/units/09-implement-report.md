# Unit 09 report — implement review findings

Branch OM-012-u60-only, nothing committed. Files keep CRLF working-copy endings (checked: CR count equals line count in all three).

## Changes (per finding)

- Finding 1 (held lane, commit first, alone): `mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs` ~:36-41, `Delegate.CreateDelegate(..., throwOnBindFailure: false)`; `SensorInternalsFound` now also requires `OnPickupablesChangedOpen != null`, so a signature change reaches the existing missing-internals warning.
- Findings 4 and 5 (ship-now), `mods/salty-surf/src/SaltySurf/Mod.cs`:
  - `SurfDescriptors.WarnIfNoTarget()` called from both `Prepare()` methods logs one `[SaltySurf]` warning when neither descriptor method exists.
  - `Rewrite` logs one warning when the translation has no `{amount}`.
  - `CollectDescriptors` lookup is now `AccessTools.DeclaredMethod(typeof(MechanicalSurfboard), "CollectDescriptors", {GameObject, List<Descriptor>})`.
  - Both `Find()` are `internal static` so the helper can see them.
- Finding 6 (ship-now), `mods/upgrade-planner/src/UpgradePlanner/UI/MaterialPicker.cs`:
  - New `LookUp()` resolves `ConfigureScreen`, its parameters and result type, `IsDefBuildable*` and `GetTooltipForBuildable` once into statics.
  - On a miss it logs one `[UpgradePlanner]` warning.
  - `Configure` now returns bool; `Attach` destroys the new panel and returns null (already a documented return) instead of throwing.
- No new API beyond what exists on U59 (AccessTools.DeclaredMethod, 4-arg Delegate.CreateDelegate); no U59 run was done.
- Findings 2, 3, 7, 8 untouched (out of scope).

## Build (dotnet build -c Release, each project)

SaltySurf, SaltySurf.Tests, UpgradePlanner, UpgradePlanner.Tests, MotionSensorRange, MotionSensorRange.Tests: 0 errors (SaltySurf.Tests 2 warnings, pre-existing kind; others 0).

## In-game, U60, one run

`pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SaltySurf,UpgradePlanner,MotionSensorRange -Steam 3468585385`

```
Session done: 77 passed, 1 failed, 0 errors, 0 timeouts, 0 skipped of 78; 525 s real time
FAIL UpgradePlanner.Tests/BuildingSettings.ProblemTypesKept (103.7 s): 1 problems, first: FishFeeder: not rebuilt in Cinnabar (nothing) (expected 0, got 1)
```

Per mod: SaltySurf 11/11, MotionSensorRange 9/9, UpgradePlanner 57 pass / 1 fail. Run folder: `common/tools/TestRunner/runs/run-20261107-012007`. No new player saves (runner output).

The failure is in building-settings rebuild, nothing near the picker or reflection code touched. I reran it alone (`-Mods UpgradePlanner -Filter "*ProblemTypesKept"`): `PASS ... (73.2 s)`, 1 of 1. It looks order or timing dependent (it took 103.7 s in the full run against 73.2 s alone). I did not run it against the pre-change commit, so whether it is flaky on 354c5e6 is not established. The prior "green" commit suggests it passed then.

## Packet feedback

- The brief spent its budget on 17 copies of the same try/catch standard (as unit 08 also noted).
- Friction: Bash `sleep` is blocked and a background wait loop on the wrong file hung; I stopped only that loop, not the runner.
