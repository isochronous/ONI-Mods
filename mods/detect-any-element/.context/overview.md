---
description: "TODO(owner): one line saying what detect-any-element is."
---
# detect-any-element

## What this is
**Id:** blk-d74e1e7b3ec3

Detect Any Element is an Oxygen Not Included mod that adds an Any choice, right under None, to the element sensors. The Liquid Pipe, Gas Pipe and Conveyor Rail Element Sensors go green for any packet passing through, and the Gas and Liquid Element Sensors for any gas or liquid in their cell. Picking an element switches the sensor back to normal. The Any choice carries an asterisk drawn by a small kanim. Version 0.2.1 in `mod_info.yaml` (released 2026-10-12), minimum supported build 744825, published on the Steam Workshop with Korean, Russian and Chinese translations.

## Where things are
**Id:** blk-a4375e886775

Top-level folders and key files:

- `src/DetectAnyElement/`: the mod. `Mod.cs` (entry point and the Any tag), `Patches.cs` (side screen, pipe and world sensor patches), `AnyGlyph.cs` (the asterisk behaviour), `ModStrings.cs` (translatable strings), `anim/` (asterisk art), `translations/` (`.pot` and `.po` files), `mod.yaml`, `mod_info.yaml`.
- `src/DetectAnyElement.Tests/`: in-game tests for the glyph, persistence, pipe sensors, side screen and world sensors.
- `tools/make_art.py`: generates the asterisk art; `tools/` also holds the Liberation Sans font and its licence.
- `preview/`: preview image source.
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description, tags, preview image and the packaged mod.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
