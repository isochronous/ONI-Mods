---
tags: detection-range
doc_id: doc-160512696e47
description: "The `RangeSlider` component, Harmony patches, strings and saved range of Motion Sensor Range, with the 2 to 16 tile range rules, key code paths and tests for range, detection and persistence"
applies_to:
  - "src/MotionSensorRange/**"
---

# Motion Sensor Range domain

## Overview
**Id:** blk-59db01f0a30a

Motion Sensor Range makes the detection range of the vanilla Duplicant Motion Sensor (`LogicDuplicantSensorConfig`) adjustable from a slider in the building's side screen. It adds no building of its own: it bolts one component onto the vanilla prefab and rewrites the sensor's detection area at runtime. The range runs from 2 to 16 tiles in steps of 2, with 4 as the vanilla default.

## Key Entities
**Id:** blk-6d0612f31e83

### RangeSlider
**Id:** blk-1d5bba6bb234  
**Tags:** game-component, side-screen, saved-state, reflection  
**Description**: The component that holds the chosen range, exposes it to the game's int-slider side screen, and applies it to the sensor.  
**Location**: `src/MotionSensorRange/RangeSlider.cs`

| Property | Type | Description |
|----------|------|-------------|
| range | int (`[Serialize]`) | The chosen range in tiles; saved with the building (opt-in `KSerialization`) |
| MinRange / MaxRange / DefaultRange | const int | 2, 16 and 4 |
| copyBuildingSettings | `CopyBuildingSettings` (`[MyCmpAdd]`) | Puts the motion sensor into the vanilla copy-settings tool, which the vanilla building lacks |
| sensor | `LogicDuplicantSensor` (`[MyCmpGet]`) | The vanilla sensor whose `pickupRange` and scene-partitioner entry the component rewrites |
| visualizer | `RangeVisualizer` (`[MyCmpGet]`) | The range overlay, resized to the new area |
| rotatable | `Rotatable` (`[MyCmpGet]`) | Rotates the detection offset for a sensor facing sideways |

**Relationships**: Implements `IIntSliderControl` (the game's current slider interface). Sits beside `LogicDuplicantSensor` on the same game object. Reads and writes the sensor's private `pickupableExtents`, `pickupablesChangedEntry`, `pickupablesDirty`, `OnPickupablesChanged` and `RefreshReachableCells` through cached Harmony `AccessTools` reflection.

### Harmony patches
**Id:** blk-c493a6cf7733  
**Tags:** harmony-patching, under-construction-building  
**Location**: `src/MotionSensorRange/Patches.cs`

| Patch target | Kind | Purpose |
|--------------|------|---------|
| `LogicDuplicantSensorConfig.DoPostConfigureComplete` | Postfix | `AddOrGet<RangeSlider>()` on the finished building prefab |
| `GeneratedBuildings.LoadGeneratedBuildings` | Postfix | Adds `RangeSlider` to the sensor's under-construction prefab, so the range can be preset before the building is finished |
| `Constructable.FinishConstruction` | Postfix | Copies the range from the construction site's `RangeSlider` to the finished building found in the same cell on the building's object layer |

### Mod entry and strings
**Id:** blk-4592ab5d1abc  
**Tags:** mod-entry-point, mod-strings  
**Location**: `src/MotionSensorRange/Mod.cs`, `src/MotionSensorRange/ModStrings.cs`, `src/MotionSensorRange/translations/`
`MotionSensorRangeMod` (a `UserMod2`) registers `ModStrings` through `Isochronous.Translations` (see `common/`). The three strings (title, tooltip, slider tooltip) live under `STRINGS.UI.UISIDESCREENS.MOTION_SENSOR_RANGE_SIDE_SCREEN`; Chinese, Korean and Russian come from `translations/<locale>.po`.

### Saved state and settings
**Id:** blk-ef8a1b1dc448  
**Tags:** saved-state

There is one saved value, `RangeSlider.range`, stored with each motion sensor. The mod has no options screen and no config file. Mod id: `Isochronous.MotionSensorRange` (`src/MotionSensorRange/mod.yaml`).

## Business Rules
**Id:** blk-129735a5fffc  
**Tags:** saved-state, copy-settings, under-construction-building, vanilla-fallback

- The range is 2 to 16 tiles; any value the slider sends is rounded to the nearest even number and clamped (`SetSliderValue`). The scan area spans range/2 tiles to each side and is only symmetric for even ranges.
- The default is 4, the vanilla range, so an untouched sensor behaves exactly as in the base game.
- A new range takes effect at once: the sensor's `pickupRange`, its scene-partitioner registration, its reachable-cell refresh and the range overlay all follow it.
- The range is saved with the building and re-applied on spawn and after deserialization, because `LogicDuplicantSensor.OnSpawn` registers with the default range first.
- Copy settings between motion sensors copies the range. A range set on the building under construction carries over when construction completes.
- If a game update renames the sensor's private members, the mod logs one warning and the range setting has no effect; it does not throw.

## Key Code Paths
**Id:** blk-63f43343ddcb  
**Tags:** copy-settings, under-construction-building, in-game-testing

- **Slider change**: side screen calls `SetSliderValue`, which snaps and clamps, then `ApplyRange` — see `src/MotionSensorRange/RangeSlider.cs`
- **Applying a range**: `ApplyRange` mirrors the extents math of `LogicDuplicantSensor.OnSpawn`, frees and re-adds the `pickupablesChangedLayer` partitioner entry, marks pickupables dirty, refreshes reachable cells and resizes the visualizer — see `src/MotionSensorRange/RangeSlider.cs`
- **Construction carry-over**: `Constructable.FinishConstruction` postfix calls `CopyRangeFrom` — see `src/MotionSensorRange/Patches.cs`
- **Copy settings**: `GameHashes.CopySettings` event handled by `OnCopySettings` — see `src/MotionSensorRange/RangeSlider.cs`
- **Tests**: in-game `[OniTest]` cases in `src/MotionSensorRange.Tests/` (`Range.cs` for defaults, snapping, detection area, rotation and copy settings; `Detection.cs` for a Duplicant at distance and the side screen; `Persistence.cs` for save/load and construction carry-over)

## Related
**Id:** blk-eebe4207ea1d

- See also: [glossary](glossary.md)
