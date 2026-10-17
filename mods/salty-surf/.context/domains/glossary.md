---
tags: mechanical-surfboard
witness_digest: "sha256:a6f9104ca76161d0586dd54ea09e5be2e8ef2c86ad283569ec95283149f30aef"
witness: loose
applies_to:
  - "src/SaltySurf/**"
doc_id: doc-8bc9e6c6b6ef
description: "Glossary for Salty Surf: abbreviations, terms to avoid, and definitions of the Mechanical Surfboard, intake capacity tag, shared SaltySurfWater tag, readiness, spill, wrong liquid and descriptor, with the code using each"
---

# Salty Surf glossary

Terms for the Salty Surf mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-0a94ff963f30

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| kg | Kilogram | Tank and spill amounts in the game's mass unit |

## Terms to Avoid
**Id:** blk-d2a22b0683e9

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Saltwater | Salt Water | The element's in-game name is "Salt Water" (`SimHashes.SaltWater`); the code and README use it |
| Surf board | Mechanical Surfboard | The vanilla building's name |

## Cross-Domain Terms
**Id:** blk-6982f99e63a4

### Mechanical Surfboard
**Id:** blk-765d6b119222  
**Definition**: The vanilla recreation building (`MechanicalSurfboardConfig`) a Duplicant surfs on. It takes liquid from a pipe into a tank, spills it while in use, and gives the surfer a morale effect.  
**Used in**: `Mod.cs`

### Intake / capacity tag
**Id:** blk-63dbbc9820b9  
**Tags:** game-tag  
**Definition**: The pipe intake is a `ConduitConsumer`; its `capacityTag` names which element it accepts. Vanilla uses Water; the mod uses the shared tag.  
**Used in**: `MechanicalSurfboardConfig_Patch`

### Shared tag (SaltySurfWater)
**Id:** blk-7c1deca7e340  
**Tags:** game-tag  
**Definition**: The tag the mod adds to Water and Salt Water. The game copies an element's tags onto every stored chunk when the chunk is created, so one tag on the element covers the pipe check, the storage mass check and the spill.  
**Used in**: `SurfWater`, `ElementLoader_Load_Patch`

### oreTags
**Id:** blk-ac3b96f5c9c1  
**Tags:** game-tag, harmony-patching  
**Definition**: The tag array on an `Element` that `ElementLoader` fills; the mod appends the shared tag.  
**Used in**: `ElementLoader_Load_Patch`

### Readiness (IsReady)
**Id:** blk-8bc9bf639fb2  
**Tags:** harmony-patching  
**Definition**: The board's state-machine check that its tank holds `minOperationalWaterKG` (2 kg); only then can a Duplicant surf.  
**Used in**: `MechanicalSurfboard_IsReady_Patch`

### Spill
**Id:** blk-29c423cd1692  
**Tags:** harmony-patching  
**Definition**: The liquid the board drops on the floor while in use, at `waterSpillRateKG` per second of work, through `FallingWater`.  
**Used in**: `MechanicalSurfboardWorkable_OnWorkTick_Patch`

### Wrong liquid
**Id:** blk-79266a9a9f87  
**Tags:** vanilla-fallback, in-game-testing  
**Definition**: Any liquid other than Water or Salt Water: it damages the building and is dumped, as in the base game.  
**Used in**: `Intake.cs` tests

### Descriptor
**Id:** blk-90e71a732047  
**Tags:** info-panel-text, harmony-patching  
**Definition**: A line in a building's info panel (`Descriptor` from `IGameObjectEffectDescriptor`); the mod rewrites the water requirement line.  
**Used in**: `MechanicalSurfboard_GetDescriptors_Patch`

## Related
**Id:** blk-c5eab6144ce4

- See also: [entities](entities.md)
