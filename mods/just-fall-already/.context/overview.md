---
description: "TODO(owner): one line saying what just-fall-already is."
---
# just-fall-already

## What this is
**Id:** blk-e59d34ad505d

Just Fall Already! is an Oxygen Not Included mod: a critter whose footing a door just took away falls at once, instead of standing there until it figures out it is supposed to. It arms the cells around a door when the door finishes changing state and pokes the critter's navigation on the next batch. It works on its own for pez droppers and alongside Sgt_Imalas's Critters Fall Through Open Doors. Version 0.1.1 in `mod_info.yaml` (released 2026-10-14), minimum supported build 744825, published on the Steam Workshop.

## Where things are
**Id:** blk-d2b43691167d

Top-level folders and key files:

- `src/JustFallAlready/`: the mod. `FallPoke.cs` (door-armed cells and the immediate fall check), `Mod.cs` (entry point), `mod.yaml`, `mod_info.yaml`.
- `src/JustFallAlready.Tests/`: in-game tests (`Doors.cs`).
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description, tags, preview image and the packaged mod.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
