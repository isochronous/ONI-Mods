---
witness_digest: "sha256:0306519df41aa63a1a600c61bb32b39a0b3a095827e6365f9dd180909f6c8a3f"
witness: loose
applies_to:
  - "src/CornerBuildAnything/**"
doc_id: doc-5047cbea51fc
description: "The game tables and Harmony transpilers of the Corner Build Anything mod (src/CornerBuildAnything): the two OffsetGroups reach tables, the Constructable and Deconstructable OnSpawn patches and SwapTable, with locations"
---

# Corner Build Anything domain

## Overview
**Id:** blk-5eaa0c4a7e12

Corner Build Anything lets duplicants build and deconstruct any building through a diagonal gap, the way the game already lets them build tiles. It adds no buildings, options or saved state; it changes which reach table two game methods use. Source is in `mods/corner-build-anything/src/CornerBuildAnything/`.

## Key Entities
**Id:** blk-649a6e358fea

### OffsetGroups reach tables
**Id:** blk-21f0d61743aa  
**Description**: The game keeps two tables of offsets that say where a duplicant may stand to work on a building. `InvertedStandardTable` reaches a diagonal cell only by way of one of its two orthogonal neighbours. `InvertedStandardTableWithCorners` reaches diagonal cells outright. The corner table is a strict superset of the standard one: every standing cell the standard table reaches, it reaches, by the same or a shorter row of intermediate cells.  
**Location**: game class `OffsetGroups` (not in this repo); the mod reads both as static fields in `mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`

**Relationships**: `OffsetGroups.BuildReachabilityTable` caches by table reference, so the swapped table gets its own cache entries and does not collide with a mod that still asks for the standard one.

### Constructable.OnSpawn patch
**Id:** blk-b8025016d218  
**Description**: Transpiler that replaces every `ldsfld OffsetGroups.InvertedStandardTable` in `Constructable.OnSpawn` with a load of the corner table. The game picks the corner table here for tiles only; after the patch every building gets it.  
**Location**: `mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs` (`Constructable_OnSpawn`)

### Deconstructable.OnSpawn patch
**Id:** blk-ba96c6ad3eb0  
**Description**: The same transpiler on `Deconstructable.OnSpawn`, so a building put up through a corner can be taken down through the same corner.  
**Location**: `mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs` (`Deconstructable_OnSpawn`)

### SwapTable
**Id:** blk-1b351a36e13a  
**Description**: The shared instruction rewrite. Counts the swapped loads, logs `corner reach table in place` per method, or logs a warning and leaves the method unchanged when it no longer loads the standard table.  
**Location**: `mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`

### CornerBuildAnythingMod
**Id:** blk-b1cf9481a19b  
**Description**: The `UserMod2` entry point. Calls `base.OnLoad` (which applies all patches) and logs the version.  
**Location**: `mods/corner-build-anything/src/CornerBuildAnything/Mod.cs`

## Business Rules
**Id:** blk-5fd39b686cad

- Duplicants may build and deconstruct any building from a corner standing cell, not only tiles.
- Multi-cell buildings, bridges included, are covered by the same swap: the game builds their reach from every footprint cell, taking the rows of the nearest footprint cell to the standing cell, so a bridge is reachable through a corner from either end and its middle. No bridge-specific patch exists.
- Placement rules are unchanged: what can be placed where is exactly as before. Only who can reach it once placed changes.
- Rotation, a building's own offset filter (the cells it refuses to be worked from, for example a pressure plate) and the delivery storage that shares the construction table keep the game's values, so materials arrive through the corner too.
- The change only adds reachable positions; it never removes one.
- A game update that stops either method loading the standard table does not fail the patch; the method is left as is and a warning is logged.

## Key Code Paths
**Id:** blk-3ccd0caf0b17

- **Build through a corner**: `Constructable.OnSpawn` reads the corner table (patched) — see `mods/corner-build-anything/src/CornerBuildAnything/ReachTablePatches.cs`
- **Deconstruct through a corner**: `Deconstructable.OnSpawn` reads the corner table (patched) — see the same file
- **Tests**: in-game tests in `mods/corner-build-anything/src/CornerBuildAnything.Tests/`: `Reach.cs` (table loads, reach checks, controls, superset check, placement unchanged), `Pockets.cs` (pocket fixtures), `ByHand.cs` (duplicants build a switch, a 2x2 generator and a bridge end through corners and take them down)

## Related
**Id:** blk-01a371e4fdff

- See also: [glossary](glossary.md)
