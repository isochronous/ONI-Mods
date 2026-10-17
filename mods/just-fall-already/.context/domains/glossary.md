---
witness_digest: "sha256:49eff80a7e3de4b75083d815cb6a92a69df8f2c9f195c336a66e79ed2725449c"
witness: loose
applies_to:
  - "src/JustFallAlready/**"
doc_id: doc-ebdf75035929
description: "Glossary for just-fall-already: how door changes trigger critter fall checks"
---

# Domain Glossary

Terms used in the Just Fall Already mod (`mods/just-fall-already`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-996ae7f8d7ab

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| JFA | Just Fall Already | Prefix of the in-game test names in `Doors.cs` |
| Nav | Navigation | `NavGrid`, `NavTable`, `NavType` in the game's pathfinding |

## Terms to Avoid
**Id:** blk-07b3f3a01be2

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Poke (in player text) | Fall at once | "Poke" is the code's word for calling `brain.UpdateBrain()`; players see only the critter falling |

## Cross-Domain Terms
**Id:** blk-73b0826dedf4

### Pez dropper
**Id:** blk-62297f8e712a  
**Definition**: Two stacked pneumatic doors: a critter stands on the lower door, and the upper door closing on it drops it to the one below. The mod's standard case.  
**Used in**: `Doors.cs` (`PezDropper`), `FallPoke`

### Poke
**Id:** blk-13c1494f8af0  
**Definition**: Calling `brain.UpdateBrain()` on a critter so the game runs its chore selection, including the fall check, now instead of on the brain scheduler's round-robin.  
**Used in**: `OnGridUpdated`

### Armed cell
**Id:** blk-897798f555d4  
**Definition**: A cell near a door that just finished changing state; it stays armed until the last navigation grid has recomputed a batch containing it, or 2 seconds pass.  
**Used in**: `FallPoke.armed`

### Brain and brain scheduler
**Id:** blk-aabe9d05aeaa  
**Definition**: A critter's `Brain` decides what it does next; the game runs brains on a round-robin schedule, which is why a critter can stand on nothing for a moment.  
**Used in**: `FallPoke`

### Nav grid and nav type
**Id:** blk-7c1eabd25f0f  
**Definition**: A `NavGrid` is a navigation graph for one kind of walker; `NavType` is how a walker is currently moving (floor, climbing and so on). A critter whose current nav type is no longer valid at its cell has lost its footing.  
**Used in**: `OnGridUpdated`

### CreatureFallMonitor
**Id:** blk-e82daec3da18  
**Definition**: The state machine (`CreatureFallMonitor.Instance`) that makes a critter fall; its presence marks a brain as a critter for this mod.  
**Used in**: `CrittersAt`

### Critters Fall Through Open Doors
**Id:** blk-e47a9aee85a7  
**Definition**: Sgt_Imalas's mod (Workshop 3816086407) that makes open doors non-floor and non-ceiling for critters. With it, Just Fall Already watches the cells around doors as well.  
**Used in**: `OpenDoorsAreAir`

### Fast Track
**Id:** blk-3e0b229450b9  
**Definition**: A performance mod that queues non-duplicant brains; the mod's hooks work with it (its path cache keeps the grid update call).  
**Used in**: `FallPoke` notes

## Related
**Id:** blk-a4d3d896128f

- See also: [entities](entities.md)
