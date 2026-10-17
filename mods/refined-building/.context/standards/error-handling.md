---
doc_id: doc-2600cab43ce9
description: "Error handling rules for Refined Building: guarding Harmony patches and transpilers, try/catch, logging levels and anti-patterns, plus how its element-load postfix skips ineligible elements and logs one summary"
applies_to:
  - "src/**"
---

# Error Handling Conventions

## Principles
**Id:** blk-f63338fe6158  
**Tags:** vanilla-fallback, logging

1. **Never break the game for a mod's sake.** A patch that cannot do its job leaves the vanilla behaviour in place and says so in the log. It does not throw into the game's update loop.
2. **Check what you patch.** Game updates rename fields and move code. Every reflection lookup, transpiler match and prefab or tech lookup is checked for a miss.
3. **Say what degraded.** A warning names the thing that was not found and the consequence, so a bug report with `Player.log` is enough to diagnose.
4. **Don't swallow.** A `catch` logs and either returns a safe default or continues; it is never empty.
5. **Log at the right level.** `Debug.Log` for start-up and summaries, `Debug.LogWarning` for degraded behaviour, `Debug.LogError` for a failed player action.

There is no custom exception hierarchy. Mods are small patches over game code, so errors are handled where they occur.

## Guarding Patches
**Id:** blk-e7dec4a6cf45  
**Tags:** harmony-patching

### Missing member: warn, leave vanilla
**Id:** blk-2fe59699dd8e  
**Tags:** vanilla-fallback

A reflection lookup that returns null is checked before use. The patch then does nothing, or returns `true` from a prefix so the game's method runs.

```csharp
if (ActiveField == null || StorageField == null)
	return true; // the game renamed its fields: leave vanilla alone rather than throw
```
(`mods/no-magic-fridges/src/NoMagicFridges/Patches.cs`; the warning is logged once at load by `SimTransferMethod` in `FridgeThermostat.cs`.)

### Transpiler: count the match
**Id:** blk-7b0920208dad  
**Tags:** vanilla-fallback

A transpiler counts the instructions it replaced. Zero (or more than expected) logs a warning and returns the original code, so a game update disables the change instead of breaking the method.

```csharp
if (swapped == 0)
	Debug.LogWarning("[CornerBuildAnything] " + where + " does not load OffsetGroups.InvertedStandardTable; corner reach not applied there");
```
(`mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`; same pattern in `mods/bitshifter/src/Bitshifter/Patches.cs` and `mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs`.)

### Missing data: warn, continue
**Id:** blk-e842b95ec3ff  
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
**Id:** blk-379c7f4d853c  
**Tags:** vanilla-fallback

Components and arguments that can be absent are tested before use: `if (stt == null || !Sim.IsValidHandle(stt.SimHandle)) return true;`, `if (sensor == null || GameScenePartitioner.Instance == null) return;` (`mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs`).

### Warn once
**Id:** blk-ce40a7d4834c  
**Tags:** logging

Work that repeats every frame or tick warns once behind a flag (`warnedMissingInternals` in `RangeSlider.cs`).

## try/catch
**Id:** blk-a14958d9cba8  
**Tags:** logging

`try`/`catch (Exception e)` is used in four situations and nowhere else:

1. **Optional integration with another mod.** Patching or reading another mod's types may fail for any reason. Log the message and skip the integration (`BetterAutomationOverlayCompat.Apply` in `mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs`; `mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs`).
2. **File and JSON reads and writes.** Return an empty or null result (`mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs`, `mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs`, `mods/sweep-zones/src/SweepZones/ToolIcon.cs`).
3. **Patches that run inside worldgen or per-cell work**, where an escaping exception kills a thread or a whole pass (`mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs`, `PlanetoidPatch.cs`, `Patches.cs`, `Census.cs`).
4. **Per-job work in a queue**, so one failure does not stop the rest (`mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs`, logged with `Debug.LogError`).

The message form is `"[<Name>] <what was skipped>: " + e.Message`. Use `e.GetBaseException().Message` when the exception may be a reflection wrapper, and the full `e` when the stack trace is needed.

### Finalizer for a thread with no handler
**Id:** blk-77b6fb0b3e9b  
**Tags:** harmony-patching

`Cluster.BeginGeneration` is the worldgen thread body and has none; an exception ends the thread and the loading screen waits forever. A finalizer reports the failure through the game's own error path and logs the exception (`Cluster_BeginGeneration_Patch` in `mods/natural-backwalls-everywhere/src/NaturalBackwalls/WorldgenThreadPatch.cs`).

## Yielding to Other Mods
**Id:** blk-55dfe09fb614

When another mod already does the job or shares an id, the mod steps aside and logs why:
- Bitshifter disables itself when Digital CPU is enabled or the contested methods already have Harmony owners (`WhyYield` in `mods/bitshifter/src/Bitshifter/Mod.cs`).
- Storage Pod Redux does not register its buildings while the original Storage Pod is enabled, and logs a warning telling the player to disable one (`mods/storage-pod-redux/src/StoragePodRedux/Mod.cs`).

## Logging
**Id:** blk-2af835f93a55  
**Tags:** logging

### Log Levels
**Id:** blk-b9c496483ce4

| Level | Use For | Example |
|-------|---------|---------|
| `Debug.LogError` | A player action failed and was abandoned | `PlacementQueue` in `mods/upgrade-planner` |
| `Debug.LogWarning` | Degraded or skipped behaviour | the examples above |
| `Debug.Log` | Load line, a one-line summary of what a pass changed | `Tagged N refined metal(s)` in `mods/refined-building` |

### What to Include
**Id:** blk-c266aa00bc5c

- The `[<ModName>] ` prefix on every line; `JumperBridgesMod.LogPrefix` holds it for that mod.
- What was expected, what was found, and what the mod does instead.
- Cell numbers, building names and element ids when they identify the case (`mods/vent-freeze-fix`).
- Per-tick or per-frame telemetry only behind a debug option (`VentFreezeWatcher.Verbose`, `DebugSave.Enabled`) or capped (`DetailLines` in `SettleSimPatch.cs`).

PLib's `PUtil.Log*` helpers are not used; all mods log through `Debug`.

## Anti-Patterns to Avoid
**Id:** blk-20083d3a3660

### Swallowing exceptions
**Id:** blk-1ae72c9768ca

An empty `catch` hides a broken patch. The one `catch` that returns a fallback without a log is `GuessLanguageCode` in `common/src/Translations.cs`, because a missing language preference is the normal case before the game has started.

### Throwing from a patch
**Id:** blk-fc2862131c67  
**Tags:** harmony-patching

An exception in a prefix or postfix on a per-frame or sim method repeats every frame. Guard, log once, return.

### Assuming the game's internals
**Id:** blk-d77fd0f5ca2e  
**Tags:** harmony-patching

Reading a private field by name without a null check on the `FieldInfo`, or replacing an instruction without confirming it was found, breaks on the next game update.

### Silent degradation
**Id:** blk-6300248d4ab1  
**Tags:** logging

Doing nothing without a log line leaves a player and the author guessing. Every skipped path logs.

### Spamming the log
**Id:** blk-84ce178e9e4f  
**Tags:** logging

A warning inside an update method fires thousands of times. Use a once-flag.

## Project-Specific Patterns
**Id:** blk-e8aea02be7e9

### Refined Building specifics
**Id:** blk-11c814061f9e  
**Tags:** building-materials, game-tag

- The postfix skips null elements, non-solids, elements without `RefinedMetal` and elements that already carry `Metal`; so running it twice changes nothing.
- It logs one summary line listing the elements it tagged.

## Related
**Id:** blk-0abe0a51246d

- See also: [code style](code-style.md)
- See also: [naming conventions](naming-conventions.md)
