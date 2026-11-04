# OM-011 test report: Fridge Thresholds Extended in-game tests

**Result: the suite is written and builds, but it did not run.** The game never started. The approved run ended at launch with "The game did not start within 120 s." No test executed: 0 passed, 0 failed, 5 not run. Nothing was committed and nothing under common/ was changed.

## Why the game did not start (verified)

Steam's launch options for Oxygen Not Included now run the game through oni-moono. In `C:\Program Files (x86)\Steam\userdata\...\config\localconfig.vdf` the option is:

```
"LaunchOptions"		"\"C:\\Users\\thegr\\AppData\\Local\\oni-moono\\player\\OniMoono.exe\" -- %command%"
```

1. Steam started `OniMoono.exe -- ...\OxygenNotIncluded.exe "--onitest-dir=..." "--onitest-save=..." ...` as pid 19200 at 13:01:41. Steam removed that process at 13:01:44 (Steam `logs\console_log.txt`).
2. The process wrote no Player.log. The last write to Player.log was 12:51:09, from Jeremy's own session that ended before the run. The Windows Application log has no crash entry.
3. The moono host's `Options.ParseInto` (decompiled from `oni-moono\player\OniMoono.dll`) handles `--` by jumping to the game executable and then carries on parsing the arguments that follow it. `--onitest-dir=...` is neither a moono option nor a Unity option. It hits `Console.Error.WriteLine("Unknown option " + args[i])`, then `Parse` returns null and `Main` returns 2. That happens before the engine log is opened, which is why nothing was written.
4. On top of that, the launcher only looks for a process named `OxygenNotIncluded` (Invoke-OniTests.ps1:154, 215-232, 542, 576). Under moono the game runs as `OniMoono`, so the launcher would not find it even if moono accepted the arguments.

The last good TestRunner run in the warden log was 2026-10-22. That was before the launch option pointed at moono.

Cleanup after the failed launch was verified. The player's `mods.json` is byte-identical to the run's `mods.json.real` backup (SHA-256 350F34FA...6592). No warden process is left running. The mode was not changed, because the run was in the player's own Spaced Out mode.

## How to run it (Jeremy's call; I changed nothing)

- **A. One run on the vanilla player.** Clear the game's Steam launch options, run the command below, then put the options back. This tests the mod on Mono, not on moono.
- **B. Make the harness work under moono.** Moono would need to pass arguments that follow the game executable through to the game, instead of rejecting them. The launcher would also need to accept an `OniMoono` process, and its warden and pid file would have to follow it. That is a moono change plus a TestRunner change, outside this unit.

Either way the suite needs a fresh approval for its run.

## Files

Created:
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/FridgeThresholdsExtended.Tests.csproj`: a copy of the VentFreezeFix.Tests csproj shape. It references the mod and TestRunner, `ModDeployFolder none`.
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/Fridges.cs`: five tests, described below.

Changed:
- `ONI-Mods.sln`: project `FridgeThresholdsExtended.Tests` {4E1C7B2A-93D5-4F86-A0B7-6D2E58C9F314} under the `fridge-thresholds-extended` folder, with Debug/Release configs. 7 lines added, CRLF and BOM kept.

The other mods' types are reached only by name: `AccessTools.TypeByName` for `StorageRefrigeratorThresholds.RefrigeratorThresholds` and `Psyko.Freezer.Freezer`, and `AccessTools.Property` for `SendGreenOnLow`, `ActivateValue`, `DeactivateValue` and `LastSetFlag`.

## The tests (one per behaviour)

Every rig follows NoMagicFridges: a `t.Dupes.Box()`, a copper wire dragged with the real tool, and a `DevGenerator` for power. Fills store real MushBar. Emptying goes through `Storage.ConsumeIgnoringDisease`, the call a Duplicant's eating uses, so the hysteresis never passes through an empty fridge. The port is read with `LogicPorts.GetOutputValue(FilteredStorage.FULL_PORT_ID)`. Steps are a game second apart.

| # | Test | Spec item | Oracle |
|---|---|---|---|
| 1 | `FreezerFollowsThresholds` | a | A Freezer and a vanilla Refrigerator (the control) sit on one circuit, both set to High 50 / Low 20. Fills go 60 → 35 → 10 → 35 → 60 and the expected port is 1, 1, 0, 0, 1 on both. Then the generator is removed and both send 0 at 60%. A screenshot is taken at 60%. Without this mod the Freezer sends 0 at 60%, because it is not full. |
| 2 | `IndustrialRevolutionFridge` | b | `SimpleFridge` is registered and has `RefrigeratorThresholds`. With High 50 / Low 20, fills 60 → 35 → 10 give 1, 1, 0. The component order is logged. |
| 3 | `FreezerCopySettings` | c | The source Freezer is set to green on low, High 70, Low 30. `CopyBuildingSettings.ApplyCopy` returns true (the tool's own call), and the target then reads true / 70 / 30. |
| 4 | `WriteRoundTrip` (1/2) | d | On the built Freezer, the thresholds component comes before Psyko's `Freezer` (this is the M1 mechanism). Fills 60 → 35 give 1, 1, so the hysteresis is active. The colony is then saved to the run folder. This follows the VentFreezeFix / Ovagro round-trip pattern. |
| 5 | `RoundTripLoad` (2/2, `FreshSave`) | d | The first look after the load, before the test touches anything: the port is 1 at 35%, High and Low are kept, and the thresholds component is still ahead of `Freezer`. The port is still 1 two game seconds later. Port, operational state, `LastSetFlag` and fill are logged. |

The harness supports save and reload (`SaveLoader.Instance.Save` under `Session.Dir`, then `Session.SavePath` and `FreshSave`), so item d is covered and no harness change was needed.

One limit of test 5: after a load the Freezer's power flag is probably set after `OnSpawn`. If so, the OperationalChanged event recomputes the port anyway, and the green-after-load check would pass even without the M1 fix. The component-order assertions in tests 4 and 5 are the checks that tell the fix apart from no fix. The first-look log line would show which case the game is in.

## Exact command and runner output

Build check, which passed:

```
pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850 -BuildOnly
...
Build succeeded.
    0 Warning(s)
    0 Error(s)
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
```

The approved run (no -KeepGame), verbatim:

```
pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850

Building C:\dev\ONI-Mods\mods\fridge-thresholds-extended\src\FridgeThresholdsExtended.Tests\FridgeThresholdsExtended.Tests.csproj

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.19
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Registered new local mod 'FridgeThresholdsExtended' (Fridge Thresholds Extended)
Loadout: TestRunner + FridgeThresholdsExtended + Workshop 2959340381, 2618339179, 3557584850; mode Spaced Out; fixture C:\dev\ONI-Mods\common\tools\TestRunner\fixtures\testbed-so.sav
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130136 --onitest-save=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-130136\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
Restored mods.json.
Exception: C:\dev\ONI-Mods\common\tools\TestRunner\Invoke-OniTests.ps1:543
Line |
 543 |  …      if (-not $proc) { throw 'The game did not start within 120 s.' }
     |                           ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
     | The game did not start within 120 s.
```

Exit code 1. Run folder: `common/tools/TestRunner/runs/run-20261104-130136` (no results.jsonl, no Player.log). Full console capture: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\om011-run1.txt`.

## Player.log

None from this run: the game process exited before any log was opened. No FridgeThresholdsExtended patch-applied lines and no exceptions to report.

## What remains unverified

All of it in game:
- Harmony accepting `Component __instance` and the `___` field injection on the manual Freezer prefix and postfix.
- Freezer: the port following the thresholds, and red without power.
- The IR SimpleFridge port.
- Copy settings between Freezers.
- The first signal after a load, and the component order.
- The four other IR fridges, and each target mod missing in turn. These were not in this suite, which is representative by design.

The test code itself has not run, so a fix-and-rerun of it may still be needed once the game can start.

## Brief feedback

- The `common` store's TestRunner pointers (entities, error-handling, naming) applied in principle, but the TestRunner README held everything this work needed. The README was not among the pointers; it was reached through the spec.
- Missed: nothing in either brief records that the game now launches through oni-moono, or that the harness cannot launch through it. That is the fact that decided this unit's outcome. It belongs in the TestRunner node, for example as a known limit next to "Steam must be running".
- No ICON friction in this unit.
