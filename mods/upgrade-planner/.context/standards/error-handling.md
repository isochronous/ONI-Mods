---
doc_id: doc-ec7a8d0f341f
description: "Error handling and logging rules for the Upgrade Planner mod: guarding patches, try/catch use, yielding to other mods, log levels, anti-patterns, plus how its Blueprints link, presets, variants and placement queue fail"
applies_to:
  - "src/**"
---

# Error Handling Conventions

## Principles
**Id:** blk-e04e91cfb788

1. **Never break the game for a mod's sake.** A patch that cannot do its job leaves the vanilla behaviour in place and says so in the log. It does not throw into the game's update loop.
2. **Check what you patch.** Game updates rename fields and move code. Every reflection lookup, transpiler match and prefab or tech lookup is checked for a miss.
3. **Say what degraded.** A warning names the thing that was not found and the consequence, so a bug report with `Player.log` is enough to diagnose.
4. **Don't swallow.** A `catch` logs and either returns a safe default or continues; it is never empty.
5. **Log at the right level.** `Debug.Log` for start-up and summaries, `Debug.LogWarning` for degraded behaviour, `Debug.LogError` for a failed player action.

There is no custom exception hierarchy. Mods are small patches over game code, so errors are handled where they occur.

## Guarding Patches
**Id:** blk-b193e73a84af

### Missing member: warn, leave vanilla
**Id:** blk-4c5784f27e28

A reflection lookup that returns null is checked before use. The patch then does nothing, or returns `true` from a prefix so the game's method runs.

```csharp
if (ActiveField == null || StorageField == null)
	return true; // the game renamed its fields: leave vanilla alone rather than throw
```
(`mods/no-magic-fridges/src/NoMagicFridges/Patches.cs`; the warning is logged once at load by `SimTransferMethod` in `FridgeThermostat.cs`.)

### Transpiler: count the match
**Id:** blk-8bf05515feca

A transpiler counts the instructions it replaced. Zero (or more than expected) logs a warning and returns the original code, so a game update disables the change instead of breaking the method.

```csharp
if (swapped == 0)
	Debug.LogWarning("[CornerBuildAnything] " + where + " does not load OffsetGroups.InvertedStandardTable; corner reach not applied there");
```
(`mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`; same pattern in `mods/bitshifter/src/Bitshifter/Patches.cs` and `mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs`.)

### Missing data: warn, continue
**Id:** blk-9a05a76074e8

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
**Id:** blk-08ab5b72b198

Components and arguments that can be absent are tested before use: `if (stt == null || !Sim.IsValidHandle(stt.SimHandle)) return true;`, `if (sensor == null || GameScenePartitioner.Instance == null) return;` (`mods/motion-sensor-range/src/MotionSensorRange/RangeSlider.cs`).

### Warn once
**Id:** blk-102610fa711e

Work that repeats every frame or tick warns once behind a flag (`warnedMissingInternals` in `RangeSlider.cs`).

## try/catch
**Id:** blk-dcc0a4ff3762

`try`/`catch (Exception e)` is used in four situations and nowhere else:

1. **Optional integration with another mod.** Patching or reading another mod's types may fail for any reason. Log the message and skip the integration (`BetterAutomationOverlayCompat.Apply` in `mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs`; `mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs`).
2. **File and JSON reads and writes.** Return an empty or null result (`mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs`, `mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs`, `mods/sweep-zones/src/SweepZones/ToolIcon.cs`).
3. **Patches that run inside worldgen or per-cell work**, where an escaping exception kills a thread or a whole pass (`mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs`, `PlanetoidPatch.cs`, `Patches.cs`, `Census.cs`).
4. **Per-job work in a queue**, so one failure does not stop the rest (`mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs`, logged with `Debug.LogError`).

The message form is `"[<Name>] <what was skipped>: " + e.Message`. Use `e.GetBaseException().Message` when the exception may be a reflection wrapper, and the full `e` when the stack trace is needed.

### Finalizer for a thread with no handler
**Id:** blk-9655db8a0ecd

`Cluster.BeginGeneration` is the worldgen thread body and has none; an exception ends the thread and the loading screen waits forever. A finalizer reports the failure through the game's own error path and logs the exception (`Cluster_BeginGeneration_Patch` in `mods/natural-backwalls-everywhere/src/NaturalBackwalls/WorldgenThreadPatch.cs`).

## Yielding to Other Mods
**Id:** blk-aa972d3701f9

When another mod already does the job or shares an id, the mod steps aside and logs why:
- Bitshifter disables itself when Digital CPU is enabled or the contested methods already have Harmony owners (`WhyYield` in `mods/bitshifter/src/Bitshifter/Mod.cs`).
- Storage Pod Redux does not register its buildings while the original Storage Pod is enabled, and logs a warning telling the player to disable one (`mods/storage-pod-redux/src/StoragePodRedux/Mod.cs`).

## Logging
**Id:** blk-2bc51bf35a83

### Log Levels
**Id:** blk-e3fc753da293

| Level | Use For | Example |
|-------|---------|---------|
| `Debug.LogError` | A player action failed and was abandoned | `PlacementQueue` in `mods/upgrade-planner` |
| `Debug.LogWarning` | Degraded or skipped behaviour | the examples above |
| `Debug.Log` | Load line, a one-line summary of what a pass changed | `Tagged N refined metal(s)` in `mods/refined-building` |

### What to Include
**Id:** blk-491326ff88e4

- The `[<ModName>] ` prefix on every line; `JumperBridgesMod.LogPrefix` holds it for that mod.
- What was expected, what was found, and what the mod does instead.
- Cell numbers, building names and element ids when they identify the case (`mods/vent-freeze-fix`).
- Per-tick or per-frame telemetry only behind a debug option (`VentFreezeWatcher.Verbose`, `DebugSave.Enabled`) or capped (`DetailLines` in `SettleSimPatch.cs`).

PLib's `PUtil.Log*` helpers are not used; all mods log through `Debug`.

## Anti-Patterns to Avoid
**Id:** blk-e4325c74d374

### Swallowing exceptions
**Id:** blk-707db46802de

An empty `catch` hides a broken patch. The one `catch` that returns a fallback without a log is `GuessLanguageCode` in `common/src/Translations.cs`, because a missing language preference is the normal case before the game has started.

### Throwing from a patch
**Id:** blk-a79c1180fc5c

An exception in a prefix or postfix on a per-frame or sim method repeats every frame. Guard, log once, return.

### Assuming the game's internals
**Id:** blk-baf501e66502

Reading a private field by name without a null check on the `FieldInfo`, or replacing an instruction without confirming it was found, breaks on the next game update.

### Silent degradation
**Id:** blk-3983b476e44f

Doing nothing without a log line leaves a player and the author guessing. Every skipped path logs.

### Spamming the log
**Id:** blk-5f9246cf1510

A warning inside an update method fires thousands of times. Use a once-flag.

## Project-Specific Patterns
**Id:** blk-3df0b44c102f

### Upgrade Planner specifics
**Id:** blk-b8e68616a6cc

- `BlueprintsCompat` binds to Blueprints Expanded methods by name and return type, logs `<Type>.<Method> not found` or `returns <T>, not <U>`, and treats a failed bind as integration off. Read and copy failures log `e.GetBaseException().Message` and return null or the partial result.
- `Presets` read and save inside `try`/`catch`, log the file path and message, and return an empty list on a read failure.
- `Variants` asks the build tool's patches for a dry run; a refusal is logged with `Debug.Log` and an unexpected exception with a warning.
- `PlacementQueue` catches per job and logs `Debug.LogError` with the building name and the full exception, so the queue continues.
- Missing donor tool fields warn in `Tool/UpgradeTool.cs`, as in Sweep Zones.

## Related
**Id:** blk-ab7dc9291edf

- See also: [code style](code-style.md)
- See also: [naming conventions](naming-conventions.md)
