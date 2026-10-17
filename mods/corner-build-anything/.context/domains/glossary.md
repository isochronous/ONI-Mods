---
witness_digest: "sha256:0306519df41aa63a1a600c61bb32b39a0b3a095827e6365f9dd180909f6c8a3f"
witness: loose
applies_to:
  - "src/CornerBuildAnything/**"
doc_id: doc-d3898b8e3b24
description: "Glossary for the Corner Build Anything mod: ONI and OnSpawn abbreviations, the term to avoid (corner placement), and definitions of reach table, standard and corner tables, corner pocket, footprint and offset filter"
---

# Domain Glossary

Terms used in the Corner Build Anything mod (`mods/corner-build-anything`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-6df20dc29a0d

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| OnSpawn | Component spawn callback | `Constructable.OnSpawn` and `Deconstructable.OnSpawn` are the two patched methods |

## Terms to Avoid
**Id:** blk-8812a9239a96

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Corner placement | Corner reach | The mod changes who can reach a building once placed; what can be placed where is unchanged |

## Cross-Domain Terms
**Id:** blk-831d591151c9

### Reach table
**Id:** blk-2e755327158e  
**Definition**: A table of offsets that says where a duplicant may stand to work on a building. The game keeps two: standard and with corners.  
**Used in**: `OffsetGroups`, both patches

### Standard table
**Id:** blk-b85705fc1357  
**Definition**: `OffsetGroups.InvertedStandardTable`. A diagonal cell is reachable only by way of one of its two orthogonal neighbours.  
**Used in**: the game's setup for non-tile buildings; the mod's `Standard` field

### Corner table
**Id:** blk-61683cf3e067  
**Definition**: `OffsetGroups.InvertedStandardTableWithCorners`. Diagonal cells are reachable outright. The game uses it for digging and for tiles; the mod makes every building use it.  
**Used in**: the mod's `WithCorners` field

### Corner pocket
**Id:** blk-0666a699ad10  
**Definition**: A cell whose only opening is a diagonal gap. Under the standard table it is out of reach for building, so it can be dug and tiled but not laddered or piped.  
**Used in**: `Pockets.cs` in the tests

### Footprint
**Id:** blk-cc24605602b3  
**Definition**: All the cells a building occupies. A multi-cell building's reach is built from every footprint cell, using the rows of the one nearest the standing cell.  
**Used in**: bridge handling, `Reach.cs` tests

### Offset filter
**Id:** blk-9c225ac82915  
**Definition**: A building's own list of cells it refuses to be worked from (a pressure plate, for example). The mod leaves it as the game sets it.  
**Used in**: the pressure plate control test in `Reach.cs`

### Standing cell
**Id:** blk-d7404ebb59b2  
**Definition**: The cell a duplicant stands in to work on a building.  
**Used in**: reach checks

## Related
**Id:** blk-79d498f02cc9

- See also: [entities](entities.md)
