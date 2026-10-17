---
witness_digest: "sha256:3078d537bf2fc3f461b3c463dd8356dd58d0ad38f62814a174b0ffe50091d1c3"
witness: loose
applies_to:
  - "src/NaturalBackwalls/**"
doc_id: doc-afc6c48467c3
description: "Glossary for Natural Backwalls Everywhere: abbreviations, terms to avoid, and definitions of backwall, subworld, biome group, zone type, noise tree, band table, coverage and settle sim, each with the files using it"
---

# Natural Backwalls Everywhere glossary

Terms for the Natural Backwalls Everywhere mod. For the code behind them, see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-54f649d0bc79

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod targets |
| NB | Natural Backwalls | Prefix of the in-game test names in `src/NaturalBackwalls.Tests/` |
| POI | Point of interest | A worldgen template (ruins, geysers) stamped onto the map; keeps its own backwall data |
| SO | Spaced Out | The Spaced Out DLC (`DlcManager.EXPANSION1_ID`); gates several biome and zone groups |
| Aquatic pack | Aquatic Planet Pack | DLC 5, the source of the backwall design (`DlcManager.DLC5_ID`) |
| PLib | Peter Han's mod library | Builds the options screen |
| YAML | Worldgen data format | Subworlds, biomes and noise trees are YAML files |

## Terms to Avoid
**Id:** blk-5b562a58ad46

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Background | Backwall | The game's worldgen code says "background" (`ApplyBackground`, `BiomeBackgroundElementBandConfigurations`); mod text and options say backwall |
| Biome (for a pattern) | Zone | A noise tree belongs to a subworld and its zone type, not a biome; material and coverage belong to the biome group |
| Coverage as a percent | Coverage as a fraction | Options store 0 to 1; the density figures for patterns are percentages of cells |

## Cross-Domain Terms
**Id:** blk-4fbe497263b4

### Backwall
**Id:** blk-b8fb7d2bb919  
**Definition**: A natural background tile behind the open space of a cell, written only in world generation. The game writes one only where the sampled element is a solid.  
**Used in**: `Patches.cs`, `FeaturePatch.cs`, `SettleSimPatch.cs`

### Subworld
**Id:** blk-e31f1d15e9f2  
**Definition**: A worldgen unit (`ProcGen.SubWorld`) that lists weighted biomes, declares a zone type, and may name a `backwallNoise` tree.  
**Used in**: `Patches.cs`, `Patterns.cs`

### Biome
**Id:** blk-73d7be9bdfb2  
**Definition**: A worldgen biome file with an element band table. The mod looks up the `<biome>_backwall` table by biome key.  
**Used in**: `Patches.cs`, `BiomeGroups.cs`

### Biome group
**Id:** blk-4125ee054752  
**Definition**: A family of biome files sharing one material and coverage setting (one options row). Grouped by biome file, not zone.  
**Used in**: `BiomeGroups.cs`, `Options.cs`

### Zone type
**Id:** blk-3b99ed7e6f88  
**Definition**: `SubWorld.ZoneType`, the category every subworld declares (Sandstone, Marsh, Magma, and so on). The unit a noise pattern applies to.  
**Used in**: `Patterns.cs`

### Noise tree (pattern)
**Id:** blk-5d8b183e9daf  
**Definition**: A worldgen noise YAML the backwall band is sampled with; sets the shape and density of backwall patches. Shipped: Reef, Kelp (kelp forest), Abyss, Beach, plus the base-game Strange.  
**Used in**: `Patterns.cs`, `worldgen/noise/`

### Band table
**Id:** blk-8b548e0e81c3  
**Definition**: `ElementBandConfiguration`: an ordered list of element gradients with band sizes. The noise value picks the element by band; for backwalls, a Vacuum band means no backwall.  
**Used in**: `CloneInToNewWorld_Patch.Build`

### Coverage
**Id:** blk-2b18c261e35b  
**Definition**: The band size, 0 to 1, of the backwall material in a biome's `_backwall` table. About the fraction of cells that get a backwall, shaped by the noise tree.  
**Used in**: `Options.cs`, `BiomeGroups.cs`

### Starting biome
**Id:** blk-90a62c62bac8  
**Definition**: The biomes of the world's starting subworld (`startSubworldName`); they use the separate start coverage.  
**Used in**: `CloneInToNewWorld_Patch.StartBiomes`

### Feature room
**Id:** blk-9c0189c6ee53  
**Definition**: A cave, lake or geode a subworld carves after claiming its cells (the slime biome's TallRoom and BlobRoom). The base backwall pass skips them; the mod's postfix covers them.  
**Used in**: `FeaturePatch.cs`

### Settle sim
**Id:** blk-936e79273cf9  
**Definition**: The hundred-second simulation worldgen runs to settle gases and liquids (`WorldGenSimUtil.DoSettleSim`). Backwall phase changes during it need the guard in `SettleSimPatch.cs`.  
**Used in**: `SettleSimPatch.cs`

### Census
**Id:** blk-2431d358dea8  
**Definition**: A developer log table of backwall cells per zone type, written at colony load when `LogCensus` is on; comparable with the BiomePreview tool's numbers.  
**Used in**: `Census.cs`

### Overlay (export)
**Id:** blk-9826cc006e19  
**Definition**: A mod-style worldgen YAML folder the options screen writes (`NaturalBackwalls-overlay`), for external renderers and the preview tool.  
**Used in**: `ExportOverlay.cs`

### MutatedWorldData
**Id:** blk-b8e708373c1d  
**Definition**: The per-world copy of the worldgen settings (subworlds, biomes) that `SettingsCache.CloneInToNewWorld` returns; the mod changes only this copy, never the global cache.  
**Used in**: `Patches.cs`

### Worldgen thread
**Id:** blk-7406ad5fa7c5  
**Definition**: The thread that runs `Cluster.BeginGeneration`. It has no exception handler, so an escaped error left the loading screen waiting forever; the finalizer reports it instead.  
**Used in**: `WorldgenThreadPatch.cs`

## Related
**Id:** blk-9c39658d3297

- See also: [entities](entities.md)
