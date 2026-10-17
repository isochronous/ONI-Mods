---
tags: critter-sensor, automation-ribbon
witness_digest: "sha256:c4faee29e43d2b01855c3dcaaa107087f0eafb53ee94a8d86275f214e698eb99"
witness: loose
applies_to:
  - "src/MultichannelCritterSensor/**"
doc_id: doc-cb45b2283a1a
description: "Glossary for Multichannel Critter Sensor: abbreviations, terms to avoid, and definitions of critter sensor, room and cavity, ribbon port, logic tick, Sim200ms, threshold, modes, tracked total and Multiplexing"
---

# Multichannel Critter Sensor glossary

Terms for the Multichannel Critter Sensor mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-6e62c0f3760f  
**Tags:** kanim-art, in-game-testing

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| MCS | Multichannel Critter Sensor | Prefix of the in-game test names in `src/MultichannelCritterSensor.Tests/` |
| PLib | Peter Han's mod library | Builds the side screen frame; merged into the mod DLL at build time |
| kanim | Klei animation | The building's art format, under `anim/assets/critter_sensor_advanced/` |

## Terms to Avoid
**Id:** blk-e2771d8b92cc  
**Tags:** signal-threshold

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Bit 0 / bit 1 in player-facing text | Bit 1 / bit 2 | The UI and README count bits from 1; the code constants (`PrimaryBit` = 0) count from 0 |
| Count (alone) for a species filter | Species filter / All row | "Count" is also the threshold value name; the filter picks what is counted |
| Wire for the output | Automation Ribbon | A plain automation wire overloads when more than one bit is on |

## Cross-Domain Terms
**Id:** blk-99f829403aed

### Critter Sensor
**Id:** blk-58d1e4fb66c1  
**Definition**: The vanilla `LogicCritterCountSensor` (Automation > Sensors) that counts critters and eggs in a room and sends one green or red signal. This mod's building follows its counting method and sits right after it in the build menu.  
**Used in**: `Patches.cs`, `MultichannelCritterSensor.cs`

### Room and cavity
**Id:** blk-a7c784698924  
**Definition**: A room is a recognised enclosed area (`Room`); its cavity (`Room.cavity`) is the game's list of the creatures and eggs inside it. The sensor counts only entries in the cavity lists.  
**Used in**: `MultichannelCritterSensor.Evaluate`

### Automation Ribbon / ribbon port
**Id:** blk-3d9352b59ea0  
**Definition**: A four-bit automation cable. The sensor's `RibbonOutputPort` writes all four bits; a Ribbon Reader splits it back into single wires.  
**Used in**: `MultichannelCritterSensorConfig.cs`

### Logic tick
**Id:** blk-4e6873004416  
**Definition**: The fixed 100 ms step at which the circuit manager samples every sender's value. A pulse bit lasts exactly one logic tick.  
**Used in**: `MultichannelCritterSensor.OnLogicTick`

### Sim200ms
**Id:** blk-28e2f9fd0cb1  
**Definition**: The game's `ISim200ms` callback, called every 200 ms of game time. The sensor scans on it, so a pulse fires once per scan in which the total changed.  
**Used in**: `MultichannelCritterSensor.Sim200ms`

### Threshold (above / below)
**Id:** blk-449051416ace  
**Tags:** signal-threshold, side-screen  
**Definition**: The count a signal is compared against, 0 to 64, with a direction. The side screen uses the vanilla threshold editor (slider, number field, +/- buttons).  
**Used in**: `ThresholdAdapter.cs`

### Combined and separate mode
**Id:** blk-a2cbe7d177ab  
**Tags:** signal-threshold  
**Definition**: Combined: one threshold on critters plus eggs, output on bit 1. Separate: one threshold for critters (bit 1) and one for eggs (bit 2).  
**Used in**: `MultichannelCritterSensor.separateThresholds`

### Tracked total
**Id:** blk-029731d32878  
**Definition**: The critter count plus the egg count, each included only if its kind is ticked. Rises and falls of this value drive the pulse bits.  
**Used in**: `MultichannelCritterSensor.TrackedTotal`

### Prefab tag
**Id:** blk-953f60378848  
**Tags:** saved-state  
**Definition**: `KPrefabID.PrefabTag`, the tag naming a species or egg type. Species selections are stored as these tags.  
**Used in**: `critterTags`, `eggTags`

### IThresholdSwitch
**Id:** blk-89218f16b429  
**Tags:** signal-threshold, side-screen  
**Definition**: The game's interface for anything the threshold side screen can edit. `ThresholdAdapter` implements it so a cloned vanilla screen drives the sensor's thresholds.  
**Used in**: `ThresholdAdapter.cs`

### Multiplexing
**Id:** blk-0701cca2bd9a  
**Tags:** building-unlock  
**Definition**: The research node that unlocks the building, two tiers past Animal Control, where the vanilla Critter Sensor unlocks.  
**Used in**: `Patches.cs`

### Copy settings
**Id:** blk-48503f178c60  
**Tags:** copy-settings  
**Definition**: The vanilla tool that copies a building's settings to others of its kind via the `CopySettings` event and `CopyBuildingSettings`.  
**Used in**: `MultichannelCritterSensor.OnCopySettings`

## Related
**Id:** blk-9b410f0c4294

- See also: [entities](entities.md)
