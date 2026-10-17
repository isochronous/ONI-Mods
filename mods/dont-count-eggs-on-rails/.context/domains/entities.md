---
doc_id: doc-3255e485bb79
description: "Entities, business rules and key code paths of dont-count-eggs-on-rails: rail eggs excluded from critter sensor counts"
---
# Don't Count Eggs on Rails domain

## Overview
**Id:** blk-233337a30535

Don't Count Eggs on Rails keeps critter sensors from counting eggs that are riding a conveyor rail. It adds no buildings, options or saved state: one Harmony prefix on the egg's room tracking. Source is in `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs`.

## Key Entities
**Id:** blk-be406fb6be06

### OvercrowdingMonitor.UpdateRegion patch
**Id:** blk-e83dca4a52f5  
**Description**: A prefix on `OvercrowdingMonitor.UpdateRegion`, the once-a-second step in which an egg lists itself in the room its position falls in. For an egg on a rail it removes the egg from the room it was in and skips the original; for everything else it returns true and the game runs as usual.  
**Location**: `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs` (`OvercrowdingMonitor_UpdateRegion_Patch`)

| Member | Type | Description |
|--------|------|-------------|
| Prefix | bool | False when the egg is on a rail; also calls `smi.RemoveFromCavity()`, `roomProber.UpdateRoom(cavity)` and clears `smi.cavity` |
| OnRail | bool | The egg has `GameTags.Stored` and its `Pickupable.storage` is null |

**Relationships**: reads `OvercrowdingMonitor.Instance` (`IsEgg`, `cavity`, `kpid`); critter sensors count the eggs the room prober lists for their room, so unlisting the egg removes it from every sensor's count. Sensors, rails and the room prober are not patched.

### DontCountEggsOnRailsMod
**Id:** blk-549ca7135900  
**Description**: The `UserMod2` entry point. Calls `base.OnLoad` (which applies the patch) and logs the version.  
**Location**: `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs`

## Business Rules
**Id:** blk-6376f81fca35

- An egg on a conveyor rail is not counted by any critter sensor.
- A rail marks the item it carries as stored without giving it a storage (it fires the store event with a plain true). Nothing else does: an egg in an incubator or in a duplicant's hands has a real storage, and an egg on the floor is not stored. That combination is how the mod recognizes a rail egg.
- Eggs on the floor, in incubators, in storage or carried by duplicants count exactly as before.
- When an egg leaves the rail the game's own update lists it again within a second; the mod keeps no state.

## Key Code Paths
**Id:** blk-ee826a4ccee7

- **Unlisting a rail egg**: `Prefix` then `OnRail` — see `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs`
- **Tests**: `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails.Tests/Eggs.cs` covers a floor egg (counted), a rail egg (not counted, whether counted before or never, counted again off the rail) and an incubator egg (counted)

## Related
**Id:** blk-e8208f2a5ef0

- See also: [glossary](glossary.md)
