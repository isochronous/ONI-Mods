---
tags: reach-table
description: What the Corner Build Anything mod does (building and deconstructing through diagonal gaps, bridges from either end), its version and release state, and where its source, tests, README and publish folder sit
---

# corner-build-anything

## What this is
**Id:** blk-80ead4e4d699

Corner Build Anything is an Oxygen Not Included mod that lets duplicants build and deconstruct any building through a diagonal gap, the way the game already lets them build tiles. Bridges of every kind count from either end, not just their middle. It swaps the reach table that Constructable and Deconstructable use when a building spawns. Version 0.1.0 in `mod_info.yaml`, minimum supported build 744825; the changelog lists it as Unreleased and the node holds no Workshop id.

## Where things are
**Id:** blk-775902750f85  
**Tags:** mod-entry-point, in-game-testing

Top-level folders and key files:

- `src/CornerBuildAnything/`: the mod. `Mod.cs` (entry point), `ReachTablePatches.cs` (the Constructable and Deconstructable reach-table swap), `mod.yaml`, `mod_info.yaml`.
- `src/CornerBuildAnything.Tests/`: in-game tests (`Reach.cs`, `Pockets.cs`, `ByHand.cs`).
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description and tags.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
