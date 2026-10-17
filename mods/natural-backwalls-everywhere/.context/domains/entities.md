---
doc_id: doc-5d8e44113c75
description: The worldgen patches, options, biome groups, noise patterns, export overlay and BiomePreview tool of Natural Backwalls Everywhere in src/NaturalBackwalls, with placement rules, file locations, code paths and saved state
applies_to:
  - "src/NaturalBackwalls/**"
---

# Natural Backwalls Everywhere domain

## Overview
**Id:** blk-6a8da1feb411

Natural Backwalls Everywhere gives every biome the natural backwall tiles the Aquatic Planet Pack places behind its own biomes. A backwall is plain worldgen data: a subworld opts in with a `backwallNoise` tree, and a biome band table named `<biome>_backwall` picks the element from that noise (a non-solid, usually Vacuum, means no backwall). The mod adds both pieces to every other subworld and biome on the per-world copy of the worldgen settings, right before the game computes band thresholds. Material and coverage are configurable per biome family, the noise pattern per subworld zone. It acts only during world generation, so it affects new worlds only and can be added or removed without touching a save. Aquatic Planet Pack is not required: the mod ships copies of the pack's four noise trees.

## Key Entities
**Id:** blk-66f7350a3837

### CloneInToNewWorld_Patch
**Id:** blk-e0426cc98c9b  
**Description**: The core patch, a postfix on `SettingsCache.CloneInToNewWorld`. `Apply` loads the options, calls `Plan` for every subworld, writes the resulting band tables into `MutatedWorldData.biomes.BiomeBackgroundElementBandConfigurations` and sets each chosen `SubWorld.backwallNoise`. Any exception is caught and logged and worldgen continues untouched. `Plan` decides without changing anything and is reused by the YAML export.  
**Location**: `src/NaturalBackwalls/Patches.cs`

| Member | Type | Description |
|--------|------|-------------|
| FallbackNoise | const string | `noise/SandstoneStrange`, the base-game tree used when a shipped tree is missing |
| Suffix | const string | `_backwall`, the band-table key suffix |
| ModdedBiomes | HashSet<string> | Biome keys whose table the mod built for the world being generated |
| FeatureBackwalls / OtherPlanetoids | bool | Option values captured per world |
| EnabledSubworlds | Dictionary<World, List<SubWorld>> | Subworlds the mod enabled, per world, so the starting-world check can undo them |
| SubworldPlan | class | Name, subworld, tree to set, band tables built, enabled flag |

**Relationships**: Uses `Options`, `BiomeGroups`, `Patterns`. `Build` makes the band table (the chosen material at the coverage, plus Vacuum for the rest); `KeepVanilla` leaves a game-made table alone while its group is at the defaults; `IsSurface` excludes space and surface subworlds; `DominantSolid` picks a biome-from-another-mod's commonest solid (skipping Unobtanium and Katairite); `StartBiomes` finds the biomes of the world's start subworld.

### Options
**Id:** blk-ced10beb987e  
**Description**: The mod's settings, a PLib `IOptions` class stored in the shared config location and read again for each world generated, so changes need no restart.  
**Location**: `src/NaturalBackwalls/Options.cs`

| Property | Type | Description |
|----------|------|-------------|
| FeatureBackwalls | bool, default true | Backwalls behind carved caves and lakes |
| OtherPlanetoids | bool, default false | Backwalls on worlds other than the starting one |
| OtherModBiomes | bool, default true | Give biomes from other mods their commonest solid |
| OtherModCoverage | float 0..1, default 0.4 | Coverage for those biomes |
| StartCoverage | float 0..1, default 1 | Coverage in the starting subworld's biomes |
| LogCensus | bool, default false | Developer option: log the backwall census at each colony load |
| Materials | Dictionary<string,string> | Material element name per biome group id |
| Coverages | Dictionary<string,float> | Coverage per biome group id |
| PatternIds | Dictionary<string,string> | Pattern id per zone id |
| ExportOverlayButton | Action | Runs `ExportOverlay.Run` |

**Relationships**: `CreateOptions` yields a help text block, one `BiomeOptionsEntry` per available `BiomeGroup`, a second help block, one `PatternOptionsEntry` per available `PatternGroup`, and the census checkbox last. Titles and tooltips are string-table keys resolved by PLib.

### BiomeGroup and BiomeGroups
**Id:** blk-dbaa08df895f  
**Description**: A family of worldgen biome files sharing one material and coverage setting (Sandstone, Forest, Frozen, Caustic, Marsh, Ocean, Oil, Rust, Magma, Barren; Swamp, Wasteland, Metallic, Moo, Radioactive, Aquatic for Spaced Out; CarrotQuarry, IceCaves, SugarWoods for Frosty Planet; Garden, Raptor, Wetlands for Prehistoric Planet; Beach, Reef, KelpForest, Abyss for Aquatic Planet Pack).  
**Location**: `src/NaturalBackwalls/BiomeGroups.cs`

| Property | Type | Description |
|----------|------|-------------|
| Id | string | Options key |
| Dlc | string | DLC that must be active, or null for base game |
| BiomeFiles / ExtraBiomes | string[] | Biome files and individual biome keys in the group |
| Materials | string[] | Solid elements found in the group's band tables (the dropdown); unknown or non-solid names dropped at runtime |
| DefaultMaterial | string | The biome's filler rock |
| DefaultCoverage | float | 0.4, or 0.15 for biomes whose zone defaults to the kelp pattern |
| Vanilla | bool | True for the Aquatic pack's biomes, whose own data is kept while settings are at defaults |

### Pattern, PatternGroup and Patterns
**Id:** blk-9fdaa6a295e8  
**Description**: A `Pattern` is a noise tree (Reef, Kelp, Abyss, Beach under `noise/NaturalBackwalls*`, plus the base-game `Strange`, plus `Vanilla` meaning the subworld's own tree). A `PatternGroup` is a subworld zone type (`SubWorld.ZoneType`) with a default pattern: Kelp for ToxicJungle, BoggyMarsh, Swamp, SugarWoods, PrehistoricGarden, PrehistoricWetlands; Abyss for OilField, Rust, MagmaCore, Barren, Wasteland, Metallic, Radioactive; Reef elsewhere; the Aquatic pack's own zones default to Vanilla.  
**Location**: `src/NaturalBackwalls/Patterns.cs`

### Options entries
**Id:** blk-91bd1415587e  
**Description**: Custom PLib option rows: `BiomeOptionsEntry` (material dropdown plus coverage text box per biome group) and `PatternOptionsEntry` (pattern dropdown showing each tree's density per zone).  
**Location**: `src/NaturalBackwalls/BiomeOptionsEntry.cs`, `src/NaturalBackwalls/PatternOptionsEntry.cs`

### Worldgen noise trees
**Id:** blk-966cbf23a002  
**Description**: Four YAML noise trees shipped with the mod (`NaturalBackwallsReef`, `Kelp`, `Abyss`, `Beach`). `mod.yaml` and the worldgen folder are deployed with the mod.  
**Location**: `src/NaturalBackwalls/worldgen/noise/*.yaml` (copies for the Workshop package in `publish/content/worldgen/noise/`)

### Other Harmony patches
**Id:** blk-e61211aa02df  
**Location**: `src/NaturalBackwalls/`

| Patch target | File | Purpose |
|--------------|------|---------|
| `TerrainCell.ApplyBackground` (postfix) | `FeaturePatch.cs` | Runs the backwall placement over feature-room cells (the base pass covers only unclaimed cells), for biomes in `ModdedBiomes`, when `FeatureBackwalls` is on |
| `WorldGen.GenerateOffline` (prefix) | `PlanetoidPatch.cs` | For a world that is not the starting world, clears the noise the mod enabled when `OtherPlanetoids` is off |
| `WorldGenSimUtil.DoSettleSim` (transpiler) | `SettleSimPatch.cs` | Replaces the one call to `BackwallManager.UpdateFromSim` with `SettleSimBackwalls.UpdateFromSim`, a worldgen-safe twin |
| `Cluster.BeginGeneration` (finalizer) | `WorldgenThreadPatch.cs` | Turns an exception that escapes the worldgen thread into the game's "not all seeds germinate" failure and logs it |
| `Game.OnSpawn` and `SubworldZoneRenderData.OnSpawn` (postfixes) | `Census.cs` | Writes the backwall census per zone type once per load when `LogCensus` is on |

### ExportOverlay
**Id:** blk-48d469661fa6  
**Description**: Writes the current settings as a mod-style worldgen YAML folder, `NaturalBackwalls-overlay` in the game's root folder (`Util.RootFolder()`), for external renderers and the preview tool. Reuses `Plan`.  
**Location**: `src/NaturalBackwalls/ExportOverlay.cs`

### BiomePreview tool
**Id:** blk-48a2302aeae3  
**Description**: A .NET 8 Windows tool that runs the game's noise trees and band tables for one biome and writes an HTML preview. Not part of the mod DLL.  
**Location**: `tools/BiomePreview/` (see `tools/BiomePreview/README.md`)

### Mod entry and strings
**Id:** blk-80646e2a7680  
**Location**: `src/NaturalBackwalls/Mod.cs`, `ModStrings.cs`, `translations/`
`NaturalBackwallsMod` initialises PLib, registers `ModStrings` through `Isochronous.Translations`, and registers `Options` with `POptions`. Chinese, Korean and Russian come from `translations/<locale>.po`. Mod id `Isochronous.NaturalBackwallsEverywhere` (`mod.yaml`).

### Saved state
**Id:** blk-6a6e975cc857

None in saves. The only persistent state is the options file (shared config location).

## Business Rules
**Id:** blk-0897bf42bb22

- A subworld is enabled only if at least one of its biomes resolves to a solid material. A biome without one gets a Vacuum-only table, so a subworld with backwall noise never falls back to its terrain bands (which would give gas and ore backwalls).
- Space, surface and rocket-interior subworlds are never touched: a backwalled cell is not open to space, which would break solar panels, rockets and meteors.
- Default materials are the biome's filler rock, so a backwall never yields a resource the biome lacks.
- Coverage is the band fraction of the chosen material; the rest of the table is Vacuum. A coverage of 0, material None, or a non-solid material leaves the biome without backwalls and its subworld off.
- The starting biome (the biomes of `startSubworldName`) uses the separate start coverage, default 1.
- A subworld that already has a backwall noise tree keeps it, and a biome that already has a `_backwall` table keeps it, until its group's material or coverage differs from the vanilla defaults (the beach's algae pockets stay Dirt).
- Biomes from other mods get their commonest solid at `OtherModCoverage`, or nothing if `OtherModBiomes` is off.
- Other planetoids get no backwalls unless the option is on; only the starting asteroid is walled by default.
- POIs and geysers keep their holes: stamping a template overwrites the backwall under its footprint, and the mod leaves that alone.
- Every patch is guarded: a failure in the mod's pass is logged and skipped, leaving worldgen as the game and other mods set it up.
- During the settle sim a backwall past its melting point becomes the liquid in its cell, applied by the mod's twin with a logged count.

## Key Code Paths
**Id:** blk-eaf00ac88258

- **Planning**: `CloneInToNewWorld_Patch.Plan` per subworld, then `Apply` writes bands and noise — see `src/NaturalBackwalls/Patches.cs`
- **Feature rooms**: `TerrainCell.ApplyBackground` postfix — see `src/NaturalBackwalls/FeaturePatch.cs`
- **Other planetoids**: `WorldGen.GenerateOffline` prefix calls `ClearBackwallNoise` — see `src/NaturalBackwalls/PlanetoidPatch.cs`, `Patches.cs`
- **Settle guard**: `DoSettleSim` transpiler and `SettleSimBackwalls.UpdateFromSim` — see `src/NaturalBackwalls/SettleSimPatch.cs`
- **Thread guard**: `Cluster.BeginGeneration` finalizer — see `src/NaturalBackwalls/WorldgenThreadPatch.cs`
- **Options dialog**: `Options.CreateOptions` — see `src/NaturalBackwalls/Options.cs`
- **YAML export**: `ExportOverlay.Run` — see `src/NaturalBackwalls/ExportOverlay.cs`
- **Census**: `Game_OnSpawn_Census.Write` — see `src/NaturalBackwalls/Census.cs`
- **Tests**: in-game `[OniTest]` cases in `src/NaturalBackwalls.Tests/` (`Planning.cs` plan and options behaviour on the game's data; `Guards.cs` patch targets, settle guard, finalizer, census, options defaults; `Generated.cs` full cluster worldgen tests; `Looks.cs` screenshot cases; `Support.cs` helpers)

## Related
**Id:** blk-5c32e4c730c0

- See also: [glossary](glossary.md)
