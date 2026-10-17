---
doc_id: doc-88e47a3aca23
description: The ElementLoader.Load patch and mod entry of Refined Building in src/RefinedBuilding that tag refined metals as Metal, with the tagging rules, affected buildings, file locations, code path and tests
applies_to:
  - "src/RefinedBuilding/**"
---

# Refined Building domain

## Overview
**Id:** blk-0fa48a39c914

Refined Building lets every refined metal stand in wherever a building recipe asks for raw Metal Ore. It does this by adding the `Metal` tag to every solid element that carries `RefinedMetal` but not `Metal`, once, right after the game loads its element table. It adds no building, no component, no options and no saved state, so it can be added to or removed from an existing save. The mod is retired on the Workshop: another mod there already does the same job (`publish/retired.txt`).

## Key Entities
**Id:** blk-4255333b0ff3

### ElementLoader_Load_Patch
**Id:** blk-e1d4c3dc435f  
**Description**: The mod's only code of substance: a Harmony postfix on `ElementLoader.Load`. It walks `ElementLoader.elements`, skips non-solids, skips elements that lack `GameTags.RefinedMetal` or already have `GameTags.Metal`, and appends `GameTags.Metal` to the element's `oreTags`. It logs `Tagged N refined metal(s) as raw metal: ...`.  
**Location**: `src/RefinedBuilding/Patches.cs`

| Property | Type | Description |
|----------|------|-------------|
| element.oreTags | Tag[] | The tag array the patch extends with `Metal` |

**Relationships**: `ElementLoader` only creates elements whose DLC is enabled and folds `materialCategory: RefinedMetal` into `oreTags`, so the loaded table covers the base game, enabled DLCs and elements other mods define. The material selector in the build menu matches recipes on tags, so the tagged metals appear beside the ores in the lists of `MATERIALS.RAW_METALS`, `MATERIALS.ALL_METALS` and the `BuildableRaw&Metal` group.

### Mod entry
**Id:** blk-e6f82a0713e3  
**Location**: `src/RefinedBuilding/Mod.cs`
`RefinedBuildingMod` (a `UserMod2`) only logs the version. No PLib, no strings, no translations. Mod id `Isochronous.RefinedBuilding` (`mod.yaml`).

## Business Rules
**Id:** blk-68956ddc3d8d

- Every loaded solid element tagged `RefinedMetal` ends up with the `Metal` tag, exactly once.
- Elements that already carry both tags in vanilla (Steel, Niobium, Thermium, Iridium) are left alone. On the current game with all DLCs the mod adds the tag to Copper, Iron, Gold, Lead, Aluminum, Tungsten, Cobalt, Nickel, Zinc, Solid Mercury and Depleted Uranium.
- Minerals, fuels, liquids, gases and raw ores are never retagged; raw ores keep `Metal`.
- Buildings that ask for Refined Metal or for raw minerals only (regular pipes, ladders, plain tiles) are unchanged.
- Discovery is unchanged: a metal appears in the build menu once it is discovered and available, like an ore.
- Refined metal debris now satisfies deliveries that ask for any Metal, such as the Oxygen Mask Station; storage filters key on the material category, so refined metals stay under Refined Metal there.
- The Metal Refinery and Rock Crusher build recipes from the same tag but skip anything without an ore-to-metal transition, so no new recipes appear.

## Key Code Paths
**Id:** blk-f6b9d46b071d

- **Retagging at load**: the `ElementLoader.Load` postfix — see `src/RefinedBuilding/Patches.cs`
- **Tests**: in-game `[OniTest]` cases in `src/RefinedBuilding.Tests/RefinedMetals.cs` (every solid refined metal has `Metal` once; minerals, fuels, liquids and gases do not gain it; a loose refined pile counts as Metal for deliveries; wire and gas pipe offer refined metals; mineral-only and Refined Metal buildings untouched; the refinery and crusher gain no self-recipe)

## Related
**Id:** blk-8ef648f9082a

- See also: [glossary](glossary.md)
