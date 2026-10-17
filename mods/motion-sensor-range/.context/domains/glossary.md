---
witness_digest: "sha256:435a2c77cb357228132a18543ba88fd6ddc7de0e9386ad9c3366962153ac94c4"
witness: loose
applies_to:
  - "src/MotionSensorRange/**"
doc_id: doc-78ff1518516c
description: "Glossary for Motion Sensor Range: abbreviations, terms to avoid, and definitions of the Duplicant Motion Sensor, pickupRange, scene partitioner, extents, RangeVisualizer, IIntSliderControl and copy settings"
---

# Motion Sensor Range glossary

Terms for the Motion Sensor Range mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-48c7b28388ed

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| MSR | Motion Sensor Range | Prefix of the in-game test names in `src/MotionSensorRange.Tests/` |
| U59 | Game update 59 | The build the private sensor member names were verified against (744825), noted in `RangeSlider.cs` |

## Terms to Avoid
**Id:** blk-07a7f0531608

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Radius | Range | The setting and its strings say "range"; the area is a box that spans range/2 tiles to each side, not a circle |
| Odd range | Even range | Odd values are never stored; the slider snaps to even tiles |

## Cross-Domain Terms
**Id:** blk-251c889f4fbe

### Duplicant Motion Sensor
**Id:** blk-f46163b8d190  
**Definition**: The vanilla automation sensor (`LogicDuplicantSensorConfig`, component `LogicDuplicantSensor`) that sends a green signal while a Duplicant is within its detection area.  
**Used in**: `Patches.cs`, `RangeSlider.cs`

### pickupRange
**Id:** blk-24c040f93193  
**Definition**: The `LogicDuplicantSensor` field holding the sensor's range in tiles; vanilla value 4. The mod sets it from the slider.  
**Used in**: `RangeSlider.ApplyRange`

### Scene partitioner
**Id:** blk-72a80114ee4c  
**Definition**: The game's spatial index (`GameScenePartitioner`). The sensor registers a rectangular `Extents` on the `pickupablesChangedLayer` so it hears when Duplicants move in its area; the mod frees and re-registers that entry when the range changes.  
**Used in**: `RangeSlider.ApplyRange`

### Extents
**Id:** blk-c94337e951ad  
**Definition**: The game's rectangle of grid cells, built here from a centre cell and range/2.  
**Used in**: `RangeSlider.ApplyRange`

### RangeVisualizer
**Id:** blk-ebf6560d4c1c  
**Definition**: The vanilla component that draws the building's range outline on the map; the mod sets its `RangeMin` and `RangeMax` to match the range.  
**Used in**: `RangeSlider.ApplyRange`

### IIntSliderControl
**Id:** blk-444be195218b  
**Definition**: The game's current interface for a building that wants an integer slider in its side screen. An older slider interface (`ISliderControl`) changed shape and broke the earlier mod this one replaces.  
**Used in**: `RangeSlider`

### Copy settings
**Id:** blk-129a4f1ab384  
**Definition**: The vanilla tool (mirror button, drag tool) that copies a building's settings to others of its kind, via the `CopySettings` game event and a `CopyBuildingSettings` component.  
**Used in**: `RangeSlider.OnCopySettings`

### Under-construction building
**Id:** blk-7f02101a4db9  
**Definition**: The placeholder object (`BuildingUnderConstruction`) a building is while Duplicants build it; `Constructable.FinishConstruction` replaces it with the finished building.  
**Used in**: `Patches.cs`

### KSerialization
**Id:** blk-7a1f7be29747  
**Definition**: The game's save serializer; `[Serialize]` fields on a `[SerializationConfig(MemberSerialization.OptIn)]` component are saved with the building.  
**Used in**: `RangeSlider.range`

## Related
**Id:** blk-631e5fafb8e2

- See also: [entities](entities.md)
