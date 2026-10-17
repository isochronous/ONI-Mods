---
doc_id: doc-79fd71f85e3b
description: "Entities of Smart Weight Plate: the Smart Weight Plate building, its two-threshold latch, and the patches that wire it into the game, with file locations, Harmony patch targets, business rules and key code paths."
applies_to:
  - "src/SmartWeightPlate/**"
---
# Smart Weight Plate domain

## Overview
**Id:** blk-15ebeec6c4c1

Smart Weight Plate adds one building, a weight plate with a low and a high threshold. Its output behaves like a Liquid or Gas Reservoir's signal: it turns on at the high threshold and stays on until the weight falls to the low threshold, instead of flipping around a single cut-off. The mod is `mods/smart-weight-plate`, static ID `Isochronous.SmartWeightPlate`, source in `src/SmartWeightPlate/`.

## Key Entities
**Id:** blk-4bd1cc9344e4

### Smart Weight Plate (building)
**Id:** blk-ca89bb6d9d7b  
**Description**: A 1x1 foundation tile cloned from the vanilla Weight Plate (`FloorSwitchConfig`). Costs 75 kg of Refined Metal, sits in Automation > Sensors after the vanilla plate, unlocked by the Advanced Automation tech (`LogicCircuits`). Has one logic output port, `SmartWeightPlateLogicPort`, at the building's own cell.  
**Location**: `src/SmartWeightPlate/SmartWeightPlateConfig.cs` (prefab ID `SmartWeightPlate`); art in `src/SmartWeightPlate/anim/assets/smart_weight_plate/`.

| Property | Type | Description |
|----------|------|-------------|
| ID | string | `SmartWeightPlateConfig.ID` = `SmartWeightPlate` |
| Tile layer | ObjectLayer | `FoundationTile`, replaceable through `ReplacementTile`; `IsFoundation = true` |
| Build rule | BuildLocationRule | `Tile`, with `ConstructionOffsetFilter_OneDown` |
| Output port | LogicPorts.Port | `SmartWeightPlate.PortId`, shown in the logic overlay |

**Relationships**: carries one `SmartWeightPlate` component; shows the `SmartWeightPlateSideScreen`; edited through the vanilla `ActiveRangeSideScreen`.

### SmartWeightPlate (component)
**Id:** blk-e3b0a80523d8  
**Description**: The building's behaviour. Measures the weight on the cell above, keeps the heavy/light latch, drives the output port, status item and animation. Implements `IActivationRangeTarget` (so the vanilla two-slider screen edits it) and `ISim200ms`.  
**Location**: `src/SmartWeightPlate/SmartWeightPlate.cs`

| Property | Type | Description |
|----------|------|-------------|
| lowThreshold | int, saved | Low threshold in kg, default 10; exposed as `DeactivateValue` |
| highThreshold | int, saved | High threshold in kg, default 100; exposed as `ActivateValue` |
| heavy | bool, saved | The latch: set at the high threshold, cleared at the low one |
| invert | bool, saved | Swaps green and red; `IsSignalOn = heavy != invert` |
| settingsVersion | int, saved | -1 for a plate saved before 0.2.0; migrated once on spawn |
| MaxMass | const float | 2000 kg, the slider maximum; whole kilograms only |
| CurrentValue | float | Sum of `massSolid`, `massPickupables` and `massActivators` |

**Relationships**: needs `LogicPorts`, `KSelectable` and `KBatchedAnimController` on the same object; adds `CopyBuildingSettings` (copies low, high and invert). Subscribes to three `GameScenePartitioner` layers for the sensed cell.

### SmartWeightPlateSideScreen (UI)
**Id:** blk-f4b4c46d8fd9  
**Description**: A small PLib panel above the vanilla slider panel: current weight (refreshed every 200 ms) and the Invert Signal checkbox.  
**Location**: `src/SmartWeightPlate/SmartWeightPlateSideScreen.cs`. Sort order 1, the vanilla range screen is 0.

### Harmony patches
**Id:** blk-b15deacafb4d  
**Location**: `src/SmartWeightPlate/Patches.cs`, `src/SmartWeightPlate/Mod.cs`

| Target | Kind | Purpose |
|--------|------|---------|
| `GeneratedBuildings.LoadGeneratedBuildings` | Prefix | Adds the plate to the plan screen (Automation, sensors, after `FloorSwitchConfig`) |
| `Db.Initialize` | Postfix | Adds the building to the `LogicCircuits` tech's unlocked items; if the tech is missing, logs a warning and the building is available from the start |
| `DetailsScreen.OnPrefabInit` | Postfix | Registers the PLib side screen with `PUIUtils.AddSideScreenContent` |
| `ActiveRangeSideScreen.SetTarget` | Postfix | Resets the text boxes' min, max and character limit for every target; the vanilla screen sets them once, so typed values clamped to 100 |
| `BetterLogicOverlay.LogicSettingDisplay.ActivationRangeTargetSetting.GetSetting` | Postfix, applied in `OnAllModsLoaded` | Only when Better Automation Overlay is installed: labels the plate as `low - high kg` instead of percentages |

### Saved state
**Id:** blk-960f86eec7c8

Everything marked "saved" above is serialized with the building (`[SerializationConfig(MemberSerialization.OptIn)]`). The mod has no options file and no world-level state.

## Business Rules
**Id:** blk-24c523bf6253

- The plate weighs the cell above it, three ways, as the vanilla `LogicMassSensor` does: a solid tile (`Grid.Solid` mass), pickupables (loose items, debris), and `FloorSwitchActivator` objects (Duplicants, critters). Gas and liquid never count.
- A critter is counted as a pickupable only if it is not a Creature, or is a Creature that does not walk, hover or flop; walkers, hoverers and floppers count through their activator.
- The latch turns heavy when the weight is at or above the high threshold and light when it is at or below the low threshold. Both comparisons are inclusive.
- If high <= low there is no band, so the plate acts as a plain "at or above" threshold (`heavy = mass >= high`), which stops an equal pair from flipping every tick.
- Default signal: green while heavy, red while light, like the vanilla plate. Invert Signal swaps them.
- Thresholds are whole kilograms from 0 to 2000; the setters round and clamp. The vanilla screen keeps upper >= lower.
- Saves from before 0.2.0 sent green while light. On first spawn, a plate loaded from such a save has its invert flag flipped once so it keeps sending the same signal.
- The signal is only sent when it changes (or on the first refresh); the status item and animation follow it.

## Domain Terminology
**Id:** blk-d3fec039b462

| Term | Definition |
|------|-----------|
| High Threshold | Weight at or above which the latch turns heavy; the upper slider |
| Low Threshold | Weight at or below which the latch turns light; the lower slider |
| Latch / heavy state | Remembered state between the thresholds; what makes the plate a band, not a cut-off |
| Sensed cell | `Grid.CellAbove(this.NaturalBuildingCell())`, the cell weighed |
| Green / Red signal | Logic signal 1 / 0 on the output port, rendered with `FormatAsAutomationState` |

## Key Code Paths
**Id:** blk-474d5d572fe9

- **Weighing**: scene-partitioner callbacks `OnSolidChanged`, `OnPickupablesChanged`, `OnActivatorsChanged` update the three masses; each is also called once in `OnSpawn` because items already in the cell raise no event. See `src/SmartWeightPlate/SmartWeightPlate.cs`.
- **Latch and signal**: `Sim200ms` -> `Refresh` re-evaluates the latch, sends the signal on `PortId` when it changed, sets the Power-category status item, then `UpdateVisualState`. See `src/SmartWeightPlate/SmartWeightPlate.cs`.
- **Animation**: `UpdateVisualState` plays `on_`/`off_` + `down`/`up`; the `_pre` transition (knob motion) plays only when the pressed state changes. The kanim's animations are reworked by `tools/knobs_follow_pressure.py`.
- **Threshold editing**: the building qualifies for the vanilla `ActiveRangeSideScreen` through `IActivationRangeTarget`; the `SetTarget` patch fixes its range. See `src/SmartWeightPlate/Patches.cs`.
- **Port text**: `UpdatePortDescriptions` rewrites the instance's port descriptions for the invert setting; strings live in `src/SmartWeightPlate/ModStrings.cs`.
- **Tests**: in-game `[OniTest]` suites in `src/SmartWeightPlate.Tests/` (`Weighing.cs`, `Signal.cs`, `Settings.cs`, `SideScreen.cs`, `Compatibility.cs`).

## Related
**Id:** blk-d52f1ffb33aa

- See also: [glossary](glossary.md)
