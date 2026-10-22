---
tags: critter-falling
witness_digest: "sha256:49eff80a7e3de4b75083d815cb6a92a69df8f2c9f195c336a66e79ed2725449c"
witness: loose
applies_to:
  - "src/JustFallAlready/**"
doc_id: doc-2b60540b470b
description: "The `FallPoke` class, the `Door.SetWorldState` patch and the entry point of Just Fall Already!, with its members, the armed cell and poke rules, key code paths and the door tests in `Doors.cs`"
---

# Just Fall Already domain

## Overview
**Id:** blk-397690c92e02

Just Fall Already makes a critter fall at once when a door takes its footing away, instead of standing on nothing until its brain next runs the fall check. It adds no buildings, options or saved state; all logic is one class. Source is in `mods/just-fall-already/src/JustFallAlready/`.

## Key Entities
**Id:** blk-846032ee71db

### FallPoke
**Id:** blk-7d0c65d332f3  
**Tags:** harmony-patching, navigation-grid

**Description**: Static class with the whole mod. A postfix on `Pathfinding.AddNavGrid` subscribes to each navigation grid's `OnNavGridUpdateComplete`. A nested patch on `Door.SetWorldState` arms cells when a door finishes a state change. When a grid batch includes an armed cell, any critter standing in that cell whose navigation type is no longer valid there has `brain.UpdateBrain()` called, which runs normal chore selection and with it the game's fall check.  
**Location**: `mods/just-fall-already/src/JustFallAlready/FallPoke.cs`

| Member | Type | Description |
|--------|------|-------------|
| armed | Dictionary<int, float> | Armed cell to the real time it was armed |
| ArmSeconds | const float (2) | Armed cells no batch touched in this time are purged |
| SlowMilliseconds | const double (2.0) | A handler run over this is logged with its numbers |
| lastGrid | NavGrid | The grid registered last; its batch for a set of dirty cells comes after every other grid's, so cells are disarmed after it |
| OpenDoorsAreAir | bool | Whether an assembly named like `CrittersFallThroughOpenDoors` is loaded; decided once and logged |
| crittersByCell | Dictionary<int, List<Brain>> | Running brains with a `CreatureFallMonitor.Instance` standing in an armed cell, gathered once per frame on the first lookup; each brain's cell is checked against the armed cells before any component lookup, lists are reused, and arming a cell clears the cache |
| LogPokes | bool | Whether the game was started by the TestRunner (a command-line argument starting with `--onitest-`); read once. Only then is each poke logged |

**Relationships**: `Door_SetWorldState_Patch` arms; `OnGridUpdated` pokes and disarms; `CrittersAt` supplies critters. Duplicants and robots have no creature fall monitor and are never poked.

### Door_SetWorldState_Patch
**Id:** blk-f5bd93c3845d  
**Tags:** harmony-patching, mod-compatibility  
**Description**: Postfix on `Door.SetWorldState`. Acts only when `updateSim` is true (the door's state machine entered open, closed or locked after the animation) and a scene is not loading. Arms the door's `PlacementCells`; with Critters Fall Through Open Doors present it also arms the cells below, above, left and right of each.  
**Location**: `mods/just-fall-already/src/JustFallAlready/FallPoke.cs`

### JustFallAlreadyMod
**Id:** blk-09140514ea2f  
**Tags:** mod-entry-point, logging  
**Description**: The `UserMod2` entry point. Calls `base.OnLoad` (which applies the patches) and logs the version.  
**Location**: `mods/just-fall-already/src/JustFallAlready/Mod.cs`

## Business Rules
**Id:** blk-4ecf7695829b  
**Tags:** navigation-grid, mod-compatibility, performance

- A critter is poked only when it stands in an armed cell, its navigator belongs to the grid reporting the batch, its brain is running, and its current nav type is not valid in that cell.
- Only a door that has finished changing state arms cells; a door still moving, other doors, and the stream of batches from digging and building never trigger a poke.
- Without Critters Fall Through Open Doors only the door cells are watched, which covers the standard pez dropper. With it, the surrounding cells are watched too, so a door opening under a critter drops it at once.
- Duplicants are left alone.
- The listener does nothing unless a cell is armed, so normal navigation updates cost nothing.
- What counts as floor stays with the game and other mods; the mod only brings the brain's fall check forward.

## Key Code Paths
**Id:** blk-9d52ccfdb3df  
**Tags:** navigation-grid, in-game-testing, logging

- **Door finishes a state change**: `Door.SetWorldState` postfix arms cells — see `mods/just-fall-already/src/JustFallAlready/FallPoke.cs`
- **Grid recomputed**: `OnGridUpdated` checks armed cells, finds critters, calls `brain.UpdateBrain()`, disarms after the last grid — same file
- **Telemetry**: when the TestRunner started the game, each poke is logged with the brain, cell and nav type (the door tests count that line); slow handlers always log their timing — same file
- **Tests**: `mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs` covers a door closing on a critter, a stacked-door pez dropper, a door opening under a critter (nothing poked without the other mod), a duplicant in a closing door (not poked), and an explicit test with Critters Fall Through Open Doors

## Related
**Id:** blk-6c7e3df05d0b

- See also: [glossary](glossary.md)
