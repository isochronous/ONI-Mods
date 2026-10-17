---
witness_digest: "sha256:d10283b86caf5c7d8c6a6af6939de06a4a749d28c59b86155c47c2117d87d519"
witness: loose
applies_to:
  - "src/RefinedBuilding/**"
doc_id: doc-a642a3738d44
description: "Glossary for Refined Building: abbreviations, terms to avoid, and definitions of the Metal and RefinedMetal tags, oreTags, material category lists, ElementLoader and the Metal Refinery, with the code using each"
---

# Refined Building glossary

Terms for the Refined Building mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-2daabcea9b32

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| DLC | Downloadable content | Spaced Out, Frosty Planet and others; `ElementLoader` loads only elements of enabled DLCs |

## Terms to Avoid
**Id:** blk-199a729f1e95

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Raw metal (for any metal) | Metal Ore / the `Metal` tag | "Raw" means unrefined ore; the mod makes refined metals count as that tag, they do not become ore |
| Pipes, ladders, plain tiles as affected buildings | Wire, pneumatic doors, mesh tiles, batteries, pumps, valves, vents | The early description named buildings that are built from raw minerals and never took metal (fixed in 1.0.1) |

## Cross-Domain Terms
**Id:** blk-594839f51b52

### Metal (tag)
**Id:** blk-d237716673b4  
**Definition**: `GameTags.Metal`. The tag building recipes match when they ask for "Metal Ore". Raw metal ores carry it; the mod adds it to refined metals.  
**Used in**: `Patches.cs`

### RefinedMetal (tag)
**Id:** blk-830e38f6d68e  
**Definition**: `GameTags.RefinedMetal`. The tag of refined metal elements (from `materialCategory: RefinedMetal`); the mod's filter.  
**Used in**: `Patches.cs`

### oreTags
**Id:** blk-501ed9940ebc  
**Definition**: The tag array on an `Element` that `ElementLoader` fills from its material category. The mod appends `Metal` to it.  
**Used in**: `Patches.cs`

### MATERIALS.RAW_METALS / ALL_METALS
**Id:** blk-d2a4e6688b06  
**Definition**: The game's material-category lists a building recipe names. Despite its name `ALL_METALS` is also just `{"Metal"}`; the `BuildableRaw&Metal` group (gas pipes, storage bins, wash basins) also matches the tag.  
**Used in**: `README.md`

### ElementLoader
**Id:** blk-17f70d2804d2  
**Definition**: The game class that loads the element table; `ElementLoader.Load` is patched and `ElementLoader.elements` is the loaded list.  
**Used in**: `Patches.cs`

### Refined Metal (material)
**Id:** blk-5e045b8ba0c0  
**Definition**: The material category buildings such as the Metal Refinery's outputs belong to (Copper, Iron, Steel and so on). Buildings that ask for Refined Metal are untouched.  
**Used in**: `README.md`

### Metal Refinery and Rock Crusher
**Id:** blk-45c15c79ef6f  
**Definition**: Buildings that build recipe lists from the same tag. They skip anything without an ore-to-metal transition, so the mod adds no recipe to them.  
**Used in**: `RefinedMetals.cs` tests

## Related
**Id:** blk-fdc9cbe0e3b5

- See also: [entities](entities.md)
