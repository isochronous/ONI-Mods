---
witness_digest: "sha256:860ec49a263693aff3444e09d132e9d4dfb3c373937615798cf9163271cc09f8"
witness: loose
applies_to:
  - "src/SweepZones/**"
doc_id: doc-1e206f66663b
description: "Glossary for Sweep Zones Redux: abbreviations (U59, PLib, DLC), terms to avoid such as overlay and erase, and definitions of sweep, mop, debris, priority and drag tool terms with where each is used"
---

# Sweep Zones glossary

Terms for the Sweep Zones Redux mod. For the mod's entities, rules and code paths see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-5ae33d3b4365

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game the mod is for |
| U59 | Game update 59 | The build line the rewrite targets (`minimumSupportedBuild` 744825) |
| PLib | Peter Han's mod library | Hotkey action; merged into the DLL |
| DLC | Downloadable content | The mod supports all of them (`supportedContent: ALL`) |

## Terms to Avoid
**Id:** blk-090e762eefa7

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Overlay (for the zone display) | Zone display | The mod deliberately adds no overlay; the display is the tool's own cell colouring |
| Sweep Zones (the original) | Berkays/Rubacava's Sweep Zones | This mod is Sweep Zones Redux; no code or assets are shared with the original |
| Erase (for cancelling errands) | Cancel | Erasing a zone leaves existing errands; the vanilla Cancel tool removes them |

## Cross-Domain Terms
**Id:** blk-5208ff35a917

### Sweep
**Id:** blk-144b7d3367fc  
**Definition**: The game's errand that has a Duplicant pick up loose debris and put it in storage. `Clearable.MarkForClear` creates it and adds `GameTags.Garbage`.  
**Used in**: `ZoneStore.MarkSweepZones`.

### Mop
**Id:** blk-d6420be85bee  
**Definition**: The game's errand that removes a thin liquid layer (150 kg or less) from a floor cell, made by a `MopPlacer` object.  
**Used in**: `ZoneStore.PlaceMopErrands`.

### Debris
**Id:** blk-f7a50b55fa0c  
**Definition**: Loose items on the ground, the `Pickupable` objects on the `Pickupables` object layer.  
**Used in**: `ZoneStore.MarkSweepZones`.

### PrioritySetting
**Id:** blk-4b9fe829e6b8  
**Definition**: The game's priority value (a class and a number from 1 to 9, with yellow alert above). Zones store one per cell and apply it with `Prioritizable.SetMasterPriority`.  
**Used in**: `ZoneStore.cs`, `SweepZoneTool.cs`.

### DragTool
**Id:** blk-cc60e721fd5b  
**Definition**: The game's base class for box-drag tools. `SweepZoneTool` subclasses it and implements `OnDragTool` per cell.  
**Used in**: `SweepZoneTool.cs`.

### ToolParameterMenu
**Id:** blk-4734bb46c709  
**Definition**: The game's side menu of toggle options for the active tool. It holds the four mode toggles.  
**Used in**: `SweepZoneTool.OnActivateTool`.

### Cell
**Id:** blk-c1edf9b20db2  
**Definition**: One tile of the world grid, addressed by an int index; saved zones are keyed by it.  
**Used in**: `ZoneStore.cs`.

## Related
**Id:** blk-3d3e2c8c1cc5

- See also: [entities](entities.md)
