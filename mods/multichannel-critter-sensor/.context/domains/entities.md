---
doc_id: doc-73dc9bf76df3
description: "The things Multichannel Critter Sensor adds or patches, with file locations, gameplay rules and key code paths."
applies_to:
  - "src/MultichannelCritterSensor/**"
---
# Multichannel Critter Sensor domain

## Overview
**Id:** blk-66bf60e22675

Multichannel Critter Sensor adds one building, the Multichannel Critter Sensor: a 1x1 room-based counter of critters and eggs that writes a 4-bit value to an Automation Ribbon output. Unlike the vanilla Critter Sensor it filters by species and egg type, can keep separate thresholds for critters and eggs, and pulses on every rise or fall of the count. It is unlocked by the Multiplexing tech and sits in Automation > Sensors. The art is a custom kanim by 3GuB under `src/MultichannelCritterSensor/anim/assets/critter_sensor_advanced/`.

## Key Entities
**Id:** blk-9b5f92f32e1e

### MultichannelCritterSensorConfig
**Id:** blk-47e6134bd352  
**Description**: The building definition (`IBuildingConfig`): ID `MultichannelCritterSensor`, 1x1, 50 kg Refined Metal and 50 kg Plastic, one ribbon output port at offset (0,0), logic overlay view, always operational, not overheatable, floodable or entombable.  
**Location**: `src/MultichannelCritterSensor/MultichannelCritterSensorConfig.cs`

**Relationships**: `DoPostConfigureComplete` adds the `MultichannelCritterSensor` component and the `OverlayInFrontOfConduits` tag. Registered with the logic overlay highlight list.

### MultichannelCritterSensor (component)
**Id:** blk-862f2299e270  
**Description**: The building's behaviour. Every 200 ms (`ISim200ms`) it finds the room the building is in, counts creatures and eggs in the room cavity that pass the species filters, compares the totals against the thresholds, and sends the packed value through the ribbon port.  
**Location**: `src/MultichannelCritterSensor/MultichannelCritterSensor.cs`

| Property | Type | Description |
|----------|------|-------------|
| separateThresholds | bool (saved) | Combined mode (false) or separate critter and egg thresholds (true) |
| countCritters / countEggs | bool (saved) | Whether each kind is counted at all |
| allCritters / allEggs | bool (saved) | The "All" row: every species counts, including ones discovered later |
| critterTags / eggTags | List<Tag> (saved) | Selected prefab tags when "All" is off |
| combinedThreshold, critterThreshold, eggThreshold | int (saved) | Thresholds, 0 to 64 (`MaxThreshold`) |
| combinedAbove, critterAbove, eggAbove | bool (saved) | Fire above (true) or below (false) the threshold |
| CritterCount, EggCount, InRoom, TrackedTotal, OutputValue | runtime | Last scan results; not saved |

**Relationships**: Uses `LogicPorts` to send the signal on `PortId` ("MultichannelCritterSensorOutput"), `Game.Instance.roomProber` and `Room.cavity.creatures` / `.eggs` for counting, `Game.Instance.logicCircuitManager.onLogicTick` to clear pulses, `CopyBuildingSettings` and the `CopySettings` event for copy-settings, and two `MeterController` indicators pinned to the kanim symbols `meter_target_critter` and `meter_target_egg`.

Output bits (`PrimaryBit`, `EggBit`, `RisingBit`, `FallingBit`):

| Bit (UI, 1-based) | Combined mode | Separate mode |
|-------------------|---------------|---------------|
| 1 | Combined count passes the threshold | Critter count passes its threshold |
| 2 | Off | Egg count passes its threshold |
| 3 | One-tick pulse when the tracked total rises | Same |
| 4 | One-tick pulse when the tracked total falls | Same |

### ThresholdAdapter
**Id:** blk-eeb8babf110d  
**Description**: A small `MonoBehaviour` implementing the game's `IThresholdSwitch`, one per threshold (`Kind.Combined`, `Kind.Critters`, `Kind.Eggs`). It lets a cloned vanilla `ThresholdSwitchSideScreen` read and write one of the sensor's three thresholds. Setting a value clamps it to 0..64 and calls `NotifyConfigChanged`.  
**Location**: `src/MultichannelCritterSensor/ThresholdAdapter.cs`

**Relationships**: Lives on a helper game object owned by the side screen and points at the sensor. `RegisterNoUnits` puts an empty units string in the string table at mod load.

### MultichannelCritterSensorSideScreen
**Id:** blk-e0e419cc8a18  
**Description**: The side screen (`SideScreenContent`, `IRender200ms`). The frame is PLib UI; the three threshold editors are runtime clones of the vanilla `ThresholdSwitchSideScreen` prefab; the species and egg lists are clones of the storage-filter row and element prefabs (`TreeFilterableSideScreen`) driven directly. Layout: count line, mode buttons (Combined / Separate), combined editor, then a Critters section and an Eggs section, each with a count checkbox, an "All" row, a species list and (in separate mode) its own threshold editor.  
**Location**: `src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs`

**Relationships**: Reaches vanilla private fields through `AccessTools` (`DetailsScreen.sideScreens`, `ThresholdSwitchSideScreen.currentValue`, the `TreeFilterableSideScreen*` row and element fields). Lists come from discovered baggable species and discovered egg types, and refresh on the game's discovery event.

### Harmony patches
**Id:** blk-135266b0a795  
**Location**: `src/MultichannelCritterSensor/Patches.cs`

| Patch target | Kind | Purpose |
|--------------|------|---------|
| `GeneratedBuildings.LoadGeneratedBuildings` | Prefix | `ModUtil.AddBuildingToPlanScreen("Automation", ..., "sensors", LogicCritterCountSensorConfig.ID, After)`: build menu slot after the vanilla Critter Sensor |
| `Db.Initialize` | Postfix | Adds the building to the `Multiplexing` tech's unlocked items; if the tech is missing, logs a warning and the building is available from the start |
| `DetailsScreen.OnPrefabInit` | Postfix | `PUIUtils.AddSideScreenContent<MultichannelCritterSensorSideScreen>()` |

### Mod entry and strings
**Id:** blk-7d0135468c05  
**Location**: `src/MultichannelCritterSensor/Mod.cs`, `ModStrings.cs`, `translations/`
`MultichannelCritterSensorMod` initialises PLib (`PUtil.InitLibrary`), registers `ModStrings` through `Isochronous.Translations`, and calls `ThresholdAdapter.RegisterNoUnits()`. Chinese, Korean and Russian come from `translations/<locale>.po`. Mod id `Isochronous.MultichannelCritterSensor` (`mod.yaml`). All settings are saved with the building; the mod has no options screen.

## Business Rules
**Id:** blk-9aeb31887cd9

- Counting happens only inside a room. A sensor outside any room reads 0, shows the vanilla "Not in any room" status item, and resets its previous total.
- Critters and eggs are counted from the room cavity lists and filtered by `KPrefabID.PrefabTag`. With "All" on, every entry counts and newly discovered species are included.
- Leaving the "All" state by unticking one species keeps every other currently visible species selected.
- Thresholds are 0 to 64. Above means count greater than the threshold; below means count less than it.
- In combined mode bit 1 covers critters plus eggs (each only if its kind is counted) and bit 2 stays off. In separate mode bit 1 follows critters and bit 2 follows eggs.
- A rise or fall of the tracked total between two scans sets bit 3 or bit 4 for exactly one logic tick. A hatch (one egg out, one critter in) leaves the total unchanged and fires nothing. Changing configuration, copying settings, or leaving a room never fires a pulse (`lastTotal` resets to -1).
- Consecutive pulses are separated by at least one low tick, because `OnLogicTick` clears the pulse bits right after the circuit manager samples sender values.
- The output is a ribbon signal; a plain wire overloads when more than one bit is on.
- Copy settings copies every setting, with the tag lists cloned.
- Visuals: the body plays `on` once when the sensor starts seeing a tracked critter or egg and `off` when it sees none. Each meter plays `_off`, or a looping `_on` (with an `_on_pre` flicker when it turns on after spawn); the critter meter follows bit 1, the egg meter follows bit 2 in separate mode and bit 1 in combined mode.

## Key Code Paths
**Id:** blk-e47995daa09e

- **Scan and output**: `Sim200ms` calls `Evaluate`, which counts, sets pulse flags, builds the value, and calls `SetOutput` and `UpdateVisualState` — see `src/MultichannelCritterSensor/MultichannelCritterSensor.cs`
- **Pulse clearing**: `OnLogicTick`, hooked to `logicCircuitManager.onLogicTick` in `OnSpawn` and unhooked in `OnCleanUp` — see `src/MultichannelCritterSensor/MultichannelCritterSensor.cs`
- **Configuration edits**: side screen and adapters change fields, then `NotifyConfigChanged` rebuilds tag sets and re-evaluates — see `MultichannelCritterSensor.cs`, `ThresholdAdapter.cs`
- **Species toggling**: `SetAllSpecies`, `ToggleSpecies` — see `src/MultichannelCritterSensor/MultichannelCritterSensor.cs`
- **Side screen build**: `EnsureBuilt`, `CreateThresholdEditor`, `CreateScrollList`, `RebuildList` — see `src/MultichannelCritterSensor/MultichannelCritterSensorSideScreen.cs`
- **Tests**: in-game `[OniTest]` cases in `src/MultichannelCritterSensor.Tests/` (`Config.cs` building, copy settings, save/load, thresholds; `Counting.cs` counting, pulses and no-room; `SideScreen.cs` UI; `Kit.cs` shared helpers)

## Related
**Id:** blk-d9d42360339f

- See also: [glossary](glossary.md)
