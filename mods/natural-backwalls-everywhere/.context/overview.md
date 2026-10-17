---
description: "Overview of the Natural Backwalls Everywhere mod: what it does to worldgen, its Workshop version and supported content, and a map of its source, tests, preview tool, publish and context folders"
---

# natural-backwalls-everywhere

## What this is
**Id:** blk-6327e8e4c1c0

Natural Backwalls Everywhere is an Oxygen Not Included mod that gives every biome the natural backwall tiles the Aquatic Planet Pack places behind its own biomes. Each biome gets a configurable material chosen from that biome's own solids and a configurable coverage, and each subworld zone picks the noise pattern its backwall patches follow. It works on new worlds only: backwalls are written during world generation, by adding the pack's `backwallNoise` and `<biome>_backwall` band data to every other subworld and biome on the per-world copy of the worldgen settings. It does not need the Aquatic Planet Pack; it ships its own copies of the pack's four noise trees. Version 0.3.0 is on the Steam Workshop (id 3816744732); the README marks it work in progress, with defaults still being tuned. `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`. Available in English, Chinese (Simplified), Korean and Russian.

## Where things are
**Id:** blk-db3477ac8cc6

- `src/NaturalBackwalls/`: the mod. `Mod.cs` (entry point), `Patches.cs` (clones the backwall data into each new world's settings), `PlanetoidPatch.cs` and `FeaturePatch.cs` (worldgen ordering and feature cells), `SettleSimPatch.cs` and `WorldgenThreadPatch.cs` (guards for the settle sim and the worldgen thread), `Options.cs`, `BiomeOptionsEntry.cs` and `PatternOptionsEntry.cs` (options screen), `BiomeGroups.cs` and `Patterns.cs` (biome families and noise patterns), `Census.cs` (backwall counts logged per game load), `ExportOverlay.cs` (writes the settings as a plain worldgen overlay), `ModStrings.cs`, `mod.yaml`, `mod_info.yaml`.
- `src/NaturalBackwalls/worldgen/noise/`: the four shipped noise trees (reef, kelp, abyss, beach); `translations/` holds the `.pot` template and `.po` files.
- `src/NaturalBackwalls.Tests/`: in-game and worldgen tests (`Generated.cs`, `Guards.cs`, `Looks.cs`, `Planning.cs`, shared `Support.cs`).
- `tools/BiomePreview/`: a Windows program that previews one biome's backwalls outside the game with the game's own noise trees and band tables.
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`, `preview.jpg`, `content/` and `NaturalBackwalls.zip`, the packaged mod).
- `preview/`, `README.md`, `CHANGELOG.md`: preview art, user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
