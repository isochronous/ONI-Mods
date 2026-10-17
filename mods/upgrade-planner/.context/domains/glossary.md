---
tags: building-replacement, building-materials
witness_digest: "sha256:e76f9e8279fb4f9b360438528479b314547a025bcabce57dba07c31edbc4bbd2"
witness: loose
applies_to:
  - "src/UpgradePlanner/**"
doc_id: doc-ff4db6770074
description: "Glossary for Upgrade Planner: abbreviations (BE, ABM, PLib, def), terms to avoid such as upgrade and reconstruct, and definitions of replacement layers, tags, variants and material terms with where each is used"
---

# Upgrade Planner glossary

Terms for the Upgrade Planner mod. For the mod's entities, rules and code paths see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-41dd8d370f1a  
**Tags:** mod-compatibility

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game the mod is for |
| BE | Blueprints Expanded | Sgt_Imalas's mod, static ID `BlueprintsV2`; keeps settings of upgraded buildings. The code calls it Blueprints or `BlueprintsCompat` |
| PLib | Peter Han's mod library | Window (`PDialog`) and hotkey; merged into the DLL |
| ABM | Adaptive Building Menu | Optional mod whose picker layout the planner opts out of (`ModCompat.cs`) |
| def | `BuildingDef` | The game's building definition, one per prefab |

## Terms to Avoid
**Id:** blk-65df537bac20

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Upgrade (for the game action) | Replace, rebuild | The game has no upgrade action; the tool queues replacement orders or reconstructs. "Upgrade" is only in the mod's name and the `Upgrade` struct |
| Reconstruct for building over | Replace | Reconstruct means deconstruct then rebuild, with downtime; replacement keeps the old building working until the new one is built |

## Cross-Domain Terms
**Id:** blk-8b803e4f169a

### Replacement layer
**Id:** blk-b457904aa82e  
**Definition**: The `ObjectLayer` a `BuildingDef` names (`ReplacementLayer`) for the order it places over an existing building. The game's rule for what may be built over what comes from it, `ReplacementTags` and `ReplacementCandidateLayers`.  
**Used in**: `ReplacementRules.cs`, `Orders.cs`.

### Replacement tags
**Id:** blk-fe6a05f87425  
**Definition**: Tags a building must carry for a def to be built over it. Most come from the prefab config; some, like `GameTags.Ladders`, are added by a component on the finished building.  
**Used in**: `ReplacementRules.CarriesReplacementTag`.

### Replacement candidate
**Id:** blk-55d85070c060  
**Definition**: The finished building `BuildingDef.GetReplacementCandidate(cell)` finds in a cell. The planner places an order only when it is the building being upgraded.  
**Used in**: `Orders.Replace`.

### Utility building
**Id:** blk-6195032777b7  
**Definition**: A wire, pipe, rail or other building dragged as a path with the utility build tool (`BuildingDef.isUtility`). These replace each other by tile layer, with no tag check.  
**Used in**: `ReplacementRules.CanReplace`, `Orders.Replace`.

### Reconstructable
**Id:** blk-3e707cb5548c  
**Definition**: The game component behind the material button in a building's side screen; it deconstructs the building and rebuilds it in other materials.  
**Used in**: `Orders.Reconstruct`, `Patches.cs`.

### MaterialSelectionPanel
**Id:** blk-22853fcb8417  
**Definition**: The game's material picker under a building in the build menu, one row per recipe ingredient. The planner instantiates it inside its window.  
**Used in**: `MaterialPicker.cs`, `MaterialTabView.cs`, `TypeTabView.cs`.

### Instant build
**Id:** blk-17388add876b  
**Definition**: The debug toggle, or sandbox mode with its instant build setting, that finishes buildings on the spot. The planner replaces buildings immediately when it is on.  
**Used in**: `Orders.InstantBuild`.

### Material / element tag
**Id:** blk-4e9f18b6b5ff  
**Tags:** plan-presets  
**Definition**: The tag of a building material (an element such as `Granite`), as used in a recipe ingredient or `constructionElements`. Presets save them by name.  
**Used in**: `Materials.cs`, `Presets.cs`.

### Variant (family member)
**Id:** blk-9a1c4ba535a3  
**Tags:** mod-compatibility  
**Definition**: One of several defs made by one config class that the build menu lists as a single type, one per material (Decor Pack stained glass tiles). `Variants.Resolve` picks the right one.  
**Used in**: `Families.cs`, `Variants.cs`.

## Related
**Id:** blk-70af1cd54055

- See also: [entities](entities.md)
