---
tags: critter-sensor, conveyor-rail
witness_digest: "sha256:3def3e9e12d6f3c6606d6c6558901f245679b4e000b973436070a67748ac0a6d"
witness: loose
applies_to:
  - "src/DontCountEggsOnRails/**"
doc_id: doc-55731fdf28b0
description: "Glossary for Don't Count Eggs on Rails: abbreviations, a term to avoid, and definitions of critter sensor, room and cavity, room prober, overcrowding monitor, conveyor rail and the stored tag"
---

# Domain Glossary

Terms used in the Don't Count Eggs on Rails mod (`mods/dont-count-eggs-on-rails`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-77b4fbda4643

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| DCEOR | Don't Count Eggs on Rails | Prefix of the in-game test names in `Eggs.cs` |
| SMI | State machine instance | `OvercrowdingMonitor.Instance smi`, the patch's argument |

## Terms to Avoid
**Id:** blk-fa9a4522cadc  
**Tags:** game-tag

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Stored egg | Egg on a rail | A rail marks its item stored with no storage; an egg in an incubator is also stored but has a real storage and is still counted |

## Cross-Domain Terms
**Id:** blk-661bdc636455

### Critter sensor
**Id:** blk-4a5e82831748  
**Definition**: Automation sensor that counts the critters and eggs the game lists for its room and switches on a threshold.  
**Used in**: the problem the mod fixes

### Room and cavity
**Id:** blk-37f904dc661d  
**Definition**: A room is a prober-detected enclosed area; the egg's `cavity` is the room list it is currently in. `RemoveFromCavity` takes it out.  
**Used in**: `OvercrowdingMonitor_UpdateRegion_Patch`

### Room prober
**Id:** blk-f818fb057849  
**Definition**: `Game.Instance.roomProber`, the game system that tracks rooms and their contents.  
**Used in**: `UpdateRoom(cavity)` call in the prefix

### Overcrowding monitor
**Id:** blk-bd3bc3efff75  
**Definition**: `OvercrowdingMonitor`, the state machine on critters and eggs that lists them in their room once a second (`UpdateRegion`).  
**Used in**: the patch target

### Conveyor rail
**Id:** blk-1f8734576039  
**Definition**: The solid conduit network (shipping rail) that moves packets of items through the world and so through rooms.  
**Used in**: the rail egg condition

### Stored tag
**Id:** blk-85612028f708  
**Tags:** game-tag  
**Definition**: `GameTags.Stored`, set on an item inside a storage, and set by a rail on its packet's item without any storage behind it.  
**Used in**: `OnRail`

## Related
**Id:** blk-a89b35e148f6

- See also: [entities](entities.md)
