---
tags: placeholder
---

# 001 — Ceiling Ovagro is a hidden plant and vine pair reached through the vanilla seed

**Status:** Accepted  
**Date:** 2026-10-18  
**Task:** OM-006

---

## Context

Ovagro Overhead lets Ovagro Nodes grow from ceilings: Pips plant them on natural ceilings, Duplicants in flipped Farm Tiles and Hydroponic Farms, and the vines grow along the ceiling. The game gives each seed one planting direction (PlantableSeed.direction), the Ovagro Node (VineMother) is built to stand on a floor, and its vines (VineBranch) already follow any surface. A player who removes the mod keeps the save. Design unit 03 (OM-006) weighed one prefab flipped per instance against a second hanging prefab; the user chose the hanging prefab (D4), and an in-game removal check later found that vanilla vines left behind by a dropped hanging plant throw every tick.

## Decision

The ceiling form is two hidden prefabs: VineMotherCeiling (with VineMotherCeiling_preview), built like the vanilla plant plus the hanging changes Dasha Saltvine uses (Hanging tag, hanging offsets, uproot check on the cell above, art turned over), and VineBranchCeiling, the vanilla vine under its own id, which the ceiling plant and every ceiling vine name as BRANCH_PREFAB_NAME. There is no second seed: the vanilla VineMotherSeed is routed to the ceiling form for the length of one game call, by prefix and finalizer pairs on the planting, preview, side screen and Copy Settings methods, wherever the cell above is solid or the plot is a rotatable plot facing down. A postfix on VineBranch.Instance.SetupRootInformation gives a ceiling plant's first vine the ceiling as its root shape. If any patch fails at load, Mod.cs unpatches all of them, warns once and Ovagro stays floor-only (user ruling, a fifth try/catch case beyond the error-handling standard's four). The prefab ids are written into saves and are not renamed.

## Consequences

Floor planting stays vanilla, the seed and its strings stay vanilla, and no translations are needed. Removing the mod drops ceiling Ovagro and their vines together on load, with one 'Could not find prefab' warning each, no seed and no errors; a downward Hydroponic Farm comes back empty, and an Ovagro order still open on a downward Farm Tile survives, so the Workshop text tells players to cancel those first. World-gen Ovagro stay on floors. The ten patches touch game methods other farming mods may patch; the seed swap changes shared prefab fields during one synchronous call and relies on the game not saving or reading them in between.
