---
tags: mechanical-surfboard
doc_id: doc-4e6a503ac17c
description: The SurfWater tag helper, Harmony patches, mod strings and translations of Salty Surf in src/SaltySurf, with the accepted-liquid, readiness, spill and info panel rules, file locations, code paths, tests and saved state
applies_to:
  - "src/SaltySurf/**"
---

# Salty Surf domain

## Overview
**Id:** blk-5c09c48ae91d  
**Tags:** harmony-patching, game-tag

Salty Surf makes the Mechanical Surfboard run on Salt Water as well as Water. The vanilla building keys its pipe intake, its readiness check and its spill to the Water element alone. The mod tags Water and Salt Water with one shared tag when the element table loads, then points those three places, and the info panel text, at the tag. The mod adds no building and no options, and saves nothing of its own: the tank contents are ordinary stored liquid.

## Key Entities
**Id:** blk-6c906d447ea5

### SurfWater
**Id:** blk-eab76ffa269c  
**Tags:** game-tag, mod-strings  
**Description**: The static holder of the shared tag and the accepted elements.  
**Location**: `src/SaltySurf/Mod.cs`

| Property | Type | Description |
|----------|------|-------------|
| Tag | `Tag` ("SaltySurfWater") | Created with `TagManager.Create`; copied by the game onto every stored chunk of a tagged element |
| Elements | SimHashes[] | `Water` and `SaltWater` |
| Names() | string | "Water or Salt Water" using the game's element names joined by the translated `ModStrings.SALTYSURF.ELEMENT_LIST` ("{0} or {1}") |

### Harmony patches
**Id:** blk-0d5a13c73107  
**Tags:** harmony-patching, game-tag, info-panel-text

**Location**: `src/SaltySurf/Mod.cs`

| Patch target | Kind | Purpose |
|--------------|------|---------|
| `ElementLoader.Load` | Postfix | Puts `SurfWater.Tag` first in the `oreTags` of Water and Salt Water and adds it to `GameTags.HiddenElementTags`, so the elements' Properties line does not list it (the tag has no name string) |
| `MechanicalSurfboardConfig.ConfigureBuildingTemplate` | Postfix | Sets the `ConduitConsumer.capacityTag` to `SurfWater.Tag`, so the intake accepts either liquid |
| `MechanicalSurfboard.States.IsReady` | Prefix (replaces) | Ready when `Storage.GetMassAvailable(SurfWater.Tag)` reaches `minOperationalWaterKG` (2 kg) |
| `MechanicalSurfboardWorkable.OnWorkTick` | Prefix (replaces) | Draws `waterSpillRateKG x dt` from the tank by tag with `ConsumeAndGetDisease` and drops the element actually drawn through `FallingWater.AddParticle`, at a random cell across the board's width |
| `MechanicalSurfboard` `IGameObjectEffectDescriptor.GetDescriptors` (explicit interface method, found by `TargetMethod`) | Postfix | Replaces the vanilla water requirement line and tooltip with the two-liquid wording |

### Mod entry and strings
**Id:** blk-858e2835d046  
**Tags:** mod-entry-point, mod-strings  
**Location**: `src/SaltySurf/Mod.cs`, `src/SaltySurf/ModStrings.cs`, `src/SaltySurf/translations/`
`SaltySurfMod` (a `UserMod2`) registers `ModStrings` through `Isochronous.Translations`. The one string is `SALTYSURF.ELEMENT_LIST`; Chinese, Korean and Russian come from `translations/<locale>.po`. Mod id `Isochronous.SaltySurf` (`mod.yaml`).

### Saved state
**Id:** blk-99d39a8ff721  
**Tags:** game-tag

None of the mod's own. A board saved with Salt Water in its tank loads with the Salt Water still there, counts as ready and spills it, because the stored chunk carries the shared tag from the element definition.

## Business Rules
**Id:** blk-cabaabc55d71  
**Tags:** info-panel-text, vanilla-fallback

- The intake accepts Water or Salt Water; either fills the tank without damaging the building.
- The board is ready at 2 kg of either liquid, or of both together; less than 2 kg, or other liquids, is not ready.
- While a Duplicant surfs, the board spills whatever it was fed, at the usual rate (`waterSpillRateKG`): a Salt Water board spills Salt Water, a Water board spills Water. A tank holding no accepted liquid spills nothing.
- Every other liquid is treated as in the base game: it damages the building and is dumped.
- The info panel lists both accepted liquids in place of Water alone and keeps its other lines.
- The vanilla descriptor line is matched by its exact text, so a changed line is left alone.

## Key Code Paths
**Id:** blk-ecc935e6ebf6  
**Tags:** in-game-testing, harmony-patching, game-tag

- **Tagging the elements**: the `ElementLoader.Load` postfix — see `src/SaltySurf/Mod.cs`
- **Intake**: `capacityTag` set in the `ConfigureBuildingTemplate` postfix — see `src/SaltySurf/Mod.cs`
- **Readiness**: `MechanicalSurfboard_IsReady_Patch` — see `src/SaltySurf/Mod.cs`
- **Spill**: `MechanicalSurfboardWorkable_OnWorkTick_Patch` — see `src/SaltySurf/Mod.cs`
- **Tests**: in-game `[OniTest]` cases in `src/SaltySurf.Tests/` (`Intake.cs` tag and intake; `Board.cs` readiness and info panel; `Spill.cs` spill element and rate; `Persistence.cs` save and load round trip; `Surfing.cs` a Duplicant surfing on Salt Water)

## Related
**Id:** blk-161e1e339eb1

- See also: [glossary](glossary.md)
