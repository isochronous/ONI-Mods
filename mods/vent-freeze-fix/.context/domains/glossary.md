---
witness_digest: "sha256:6ebc8845a2b258051bf9210868d7f042fe8594560c2a88d50f03ea26282696c7"
witness: loose
applies_to:
  - "src/VentFreezeFix/**"
doc_id: doc-c11388d0a842
description: "Glossary for Vent Freeze Fix: abbreviations (sim, 200 ms tick, repro), the term to avoid (fix versus repair), and definitions of liquid vent, super-cooled liquid, the 80% rule, dig and debris terms with where each is used"
---

# Vent Freeze Fix glossary

Terms for the Vent Freeze Fix mod. For the mod's entities, rules and code paths see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-7e02fc6a16a9

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game the mod is for |
| sim | The game's native simulation | Owns cell mass, temperature and phase changes; the mod asks it to dig |
| 200 ms tick | `ISim200ms` update | The watcher's schedule |
| PLib | Peter Han's mod library | Options screen |
| repro | Reproduction | The debug save (`VentFreezeRepro.sav`) that captures the bug's conditions |

## Terms to Avoid
**Id:** blk-10ea847ff6a2

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Fix (for the tile conversion) | Repair | The option is "Repair tiles after loading"; "Insulate vent storage" is the prevention |

## Cross-Domain Terms
**Id:** blk-1697ed7ff0e5

### Liquid vent
**Id:** blk-e8a479abb601  
**Definition**: The vanilla building `LiquidVentConfig` that drips liquid from a pipe into a cell. The mod patches its prefab to add insulation and the watcher.  
**Used in**: `Mod.cs`.

### Super-cooled liquid
**Id:** blk-7dd126b0becf  
**Definition**: Liquid below its freezing point that has not frozen; small packets pushed through pipes can be in this state without breaking the pipe.  
**Used in**: `VentFreezeWatcher.cs`, mod description.

### Phase transition handler
**Id:** blk-4cd539c00174  
**Definition**: `SimTemperatureTransfer.DoOreMeltTransition`, the game's handler that converts an item past its phase transition into cell mass of the new phase with no mass rule. The cause of the tile inside the vent.  
**Used in**: comment on `LiquidVentConfig_Storage_Patch` in `Mod.cs`.

### Insulated storage modifiers
**Id:** blk-e1d60e038ed5  
**Definition**: `Storage.StandardInsulatedStorage`: hide, seal, insulate. Sealed items are skipped by the transition handler; insulated items are not temperature simulated.  
**Used in**: `Mod.cs`.

### 80% rule
**Id:** blk-dc6818cbd533  
**Definition**: The game forms a natural tile from freezing liquid only if it holds at least 80% of the element's default mass (`TileMassShare`).  
**Used in**: `VentFreezeWatcher.Verdict`.

### Dig
**Id:** blk-fca4205bc8e0  
**Definition**: The game's way of turning a natural tile into debris: `SimMessages.Dig` makes the sim remove the tile, then `WorldDamage.OnDigComplete` spawns what it removed (normally half the mass).  
**Used in**: `VentFreezeWatcher.RequestDig`, `DigRequests.cs`.

### Debris pile
**Id:** blk-b2d84a74cd66  
**Definition**: The loose items a vent's frozen packets leave on the floor; automation is often built on it. A tile forming in its cell absorbs it.  
**Used in**: README problem statement.

### Foundation tile
**Id:** blk-6c63c7a6f934  
**Definition**: A built tile (`Grid.Foundation`), as opposed to a natural one. The watcher never converts it.  
**Used in**: `VentFreezeWatcher.Verdict`.

## Related
**Id:** blk-4f4b397e2db2

- See also: [entities](entities.md)
