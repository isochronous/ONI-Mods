---
description: "Overview of the Don't Count Eggs on Rails mod: what it does for critter sensors, its version and release state, and a map of its source, tests, preview, publish and `.context` folders"
---

# dont-count-eggs-on-rails

## What this is
**Id:** blk-edbb0dfde74e

Don't Count Eggs on Rails is an Oxygen Not Included mod: critter sensors no longer count eggs that are riding a conveyor rail through the room. Eggs on the floor, in incubators or carried by duplicants count as before. It patches `OvercrowdingMonitor.UpdateRegion`. Version 0.1.0 in `mod_info.yaml` (released 2026-10-09), minimum supported build 744825, published on the Steam Workshop.

## Where things are
**Id:** blk-d02ae3ef8404

Top-level folders and key files:

- `src/DontCountEggsOnRails/`: the mod. `Mod.cs` holds the entry point and the `OvercrowdingMonitor.UpdateRegion` patch; `mod.yaml`, `mod_info.yaml`.
- `src/DontCountEggsOnRails.Tests/`: in-game tests (`Eggs.cs`).
- `preview/`: before and after screenshots and the preview image with its source.
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description, tags, preview image and the packaged mod.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
