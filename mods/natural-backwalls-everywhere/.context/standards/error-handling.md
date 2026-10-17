---
tags: exception-handling
doc_id: doc-0ad8dccd6659
description: "Error handling rules for Natural Backwalls Everywhere: guarding Harmony patches and transpilers, try/catch and the worldgen thread finalizer, logging levels and anti-patterns, plus how its worldgen, settle sim and noise-file patches fail"
applies_to:
  - "src/**"
---

# Error Handling Conventions

## Principles
**Id:** blk-d7345384307a  
**Tags:** vanilla-fallback, logging

1. **Never break the game for a mod's sake.** A patch that cannot do its job leaves the vanilla behaviour in place and says so in the log. It does not throw into the game's update loop.
2. **Check what you patch.** Game updates rename fields and move code. Every reflection lookup, transpiler match and prefab or tech lookup is checked for a miss.
3. **Say what degraded.** A warning names the thing that was not found and the consequence, so a bug report with `Player.log` is enough to diagnose.
4. **Don't swallow.** A `catch` logs and either returns a safe default or continues; it is never empty.
5. **Log at the right level.** `Debug.Log` for start-up and summaries, `Debug.LogWarning` for degraded behaviour, `Debug.LogError` for a failed player action.

There is no custom exception hierarchy. Mods are small patches over game code, so errors are handled where they occur.

## Guarding Patches
**Id:** blk-be03f1a912d6

### Missing member: warn, leave vanilla
**Id:** blk-4c1ca7ab8b1f  
**Tags:** vanilla-fallback, harmony-patching, logging

A reflection lookup that returns null is checked before use. The patch then does nothing, or returns `true` from a prefix so the game's method runs.

```csharp
if (ActiveField == null || StorageField == null)
	return true; // the game renamed its fields: leave vanilla alone rather than throw
```
(`mods/no-magic-fridges/src/NoMagicFridges/Patches.cs`; the warning is logged once at load by `SimTransferMethod` in `FridgeThermostat.cs`.)

### Transpiler: count the match
**Id:** blk-6a76d7ebba04  
**Tags:** transpiler, vanilla-fallback, logging

A transpiler counts the instructions it replaced. Zero (or more than expected) logs a warning and returns the original code, so a game update disables the change instead of breaking the method.

```csharp
if (swapped == 0)
	Debug.LogWarning("[CornerBuildAnything] " + where + " does not load OffsetGroups.InvertedStandardTable; corner reach not applied there");
```
(`mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`; same pattern in `mods/bitshifter/src/Bitshifter/Patches.cs` and `mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs`.)

### Missing data: warn, continue
**Id:** blk-ef0357ebcd1c  
**Tags:** vanilla-fallback, logging

A build-menu, tech or asset lookup that finds nothing warns and keeps the mod working:

```csharp
Tech tech = Db.Get().Techs.TryGet(TechId);
if (tech == null)
{
	Debug.LogWarning("[SmartWeightPlate] Tech '" + TechId + "' not found; building will be unlocked from the start");
	return;
}
```
(`mods/smart-weight-plate/src/SmartWeightPlate/Patches.cs`; also `mods/multichannel-critter-sensor`, `mods/storage-pod-redux`.) Missing art or a missing vanilla prefab degrades the same way (`Display.UseAnim` in `mods/bitshifter/src/Bitshifter/Patches.cs`, `mods/multichannel-critter-sensor/src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs`).

### Null checks on game objects
**Id:** blk-475f58d796c4  
**Tags:** vanilla-fallback, harmony-patching

Components and arguments that can be absent are tested before use: `if (stt == null || !Sim.IsValidHandle(stt.SimHandle)) return true;`, `if (sensor == null || GameScenePartitioner.Instance == null) return;` (`mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs`).

### Warn once
**Id:** blk-caab886a5536  
**Tags:** logging

Work that repeats every frame or tick warns once behind a flag (`warnedMissingInternals` in `RangeSlider.cs`).

## try/catch
**Id:** blk-f04d3902a855  
**Tags:** mod-compatibility, worldgen, logging

`try`/`catch (Exception e)` is used in four situations and nowhere else:

1. **Optional integration with another mod.** Patching or reading another mod's types may fail for any reason. Log the message and skip the integration (`BetterAutomationOverlayCompat.Apply` in `mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs`; `mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs`).
2. **File and JSON reads and writes.** Return an empty or null result (`mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs`, `mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs`, `mods/sweep-zones/src/SweepZones/ToolIcon.cs`).
3. **Patches that run inside worldgen or per-cell work**, where an escaping exception kills a thread or a whole pass (`mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs`, `PlanetoidPatch.cs`, `Patches.cs`, `Census.cs`).
4. **Per-job work in a queue**, so one failure does not stop the rest (`mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs`, logged with `Debug.LogError`).

The message form is `"[<Name>] <what was skipped>: " + e.Message`. Use `e.GetBaseException().Message` when the exception may be a reflection wrapper, and the full `e` when the stack trace is needed.

### Finalizer for a thread with no handler
**Id:** blk-2cda0e3a8dd6  
**Tags:** harmony-patching

`Cluster.BeginGeneration` is the worldgen thread body and has none; an exception ends the thread and the loading screen waits forever. A finalizer reports the failure through the game's own error path and logs the exception (`Cluster_BeginGeneration_Patch` in `mods/natural-backwalls-everywhere/src/NaturalBackwalls/WorldgenThreadPatch.cs`).

## Yielding to Other Mods
**Id:** blk-016318d101d7  
**Tags:** mod-compatibility, logging

When another mod already does the job or shares an id, the mod steps aside and logs why:
- Bitshifter disables itself when Digital CPU is enabled or the contested methods already have Harmony owners (`WhyYield` in `mods/bitshifter/src/Bitshifter/Mod.cs`).
- Storage Pod Redux does not register its buildings while the original Storage Pod is enabled, and logs a warning telling the player to disable one (`mods/storage-pod-redux/src/StoragePodRedux/Mod.cs`).

## Logging
**Id:** blk-8bb4d3c90a73

### Log Levels
**Id:** blk-dc07f4a8abe8  
**Tags:** logging

| Level | Use For | Example |
|-------|---------|---------|
| `Debug.LogError` | A player action failed and was abandoned | `PlacementQueue` in `mods/upgrade-planner` |
| `Debug.LogWarning` | Degraded or skipped behaviour | the examples above |
| `Debug.Log` | Load line, a one-line summary of what a pass changed | `Tagged N refined metal(s)` in `mods/refined-building` |

### What to Include
**Id:** blk-63783d2b239e  
**Tags:** logging

- The `[<ModName>] ` prefix on every line; `JumperBridgesMod.LogPrefix` holds it for that mod.
- What was expected, what was found, and what the mod does instead.
- Cell numbers, building names and element ids when they identify the case (`mods/vent-freeze-fix`).
- Per-tick or per-frame telemetry only behind a debug option (`VentFreezeWatcher.Verbose`, `DebugSave.Enabled`) or capped (`DetailLines` in `SettleSimPatch.cs`).

PLib's `PUtil.Log*` helpers are not used; all mods log through `Debug`.

## Anti-Patterns to Avoid
**Id:** blk-2ab6d7b3e16c

### Swallowing exceptions
**Id:** blk-f23be92b9d7d  
**Tags:** logging

An empty `catch` hides a broken patch. The one `catch` that returns a fallback without a log is `GuessLanguageCode` in `common/src/Translations.cs`, because a missing language preference is the normal case before the game has started.

### Throwing from a patch
**Id:** blk-dd5003fe429c  
**Tags:** harmony-patching, logging

An exception in a prefix or postfix on a per-frame or sim method repeats every frame. Guard, log once, return.

### Assuming the game's internals
**Id:** blk-4d7cded4dc3b  
**Tags:** vanilla-fallback, harmony-patching, transpiler

Reading a private field by name without a null check on the `FieldInfo`, or replacing an instruction without confirming it was found, breaks on the next game update.

### Silent degradation
**Id:** blk-ee5834add906  
**Tags:** logging, vanilla-fallback

Doing nothing without a log line leaves a player and the author guessing. Every skipped path logs.

### Spamming the log
**Id:** blk-072cd61b19ea  
**Tags:** logging

A warning inside an update method fires thousands of times. Use a once-flag.

## Project-Specific Patterns
**Id:** blk-a4d21297c79b

### Natural Backwalls Everywhere specifics
**Id:** blk-7423d5bfd4b5  
**Tags:** worldgen, settle-sim, vanilla-fallback, developer-tooling

- Worldgen runs on a thread the game does not guard, so the patches wrap their work in `try`/`catch` and log `[NaturalBackwalls] ... skipped` with the exception message, leaving worldgen untouched (`Patches.cs`, `FeaturePatch.cs`, `PlanetoidPatch.cs`, `Census.cs`).
- `Cluster_BeginGeneration_Patch.Finalizer` turns an escaped worldgen exception into the game's failure path with a logged stack trace (`WorldgenThreadPatch.cs`).
- `WorldGenSimUtil_DoSettleSim_Patch` replaces `BackwallManager.UpdateFromSim` in the offline settle sim with `SettleSimBackwalls.UpdateFromSim`, warns when the call is not found, and logs a capped number of detail lines (`DetailLines`) plus one summary.
- Missing noise files log a warning and fall back to `FallbackNoise`.
- `ExportOverlay` and `Census` are developer features behind options; `ExportOverlay` logs `Export failed` with the exception.

## Related
**Id:** blk-35a33d4ae18c

- See also: [code style](code-style.md)
- See also: [naming conventions](naming-conventions.md)
