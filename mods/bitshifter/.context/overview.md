---
description: "TODO(owner): one line saying what bitshifter is."
---
# bitshifter

## What this is
**Id:** blk-037b09df0318

Bitshifter is an Oxygen Not Included mod that lets the Automation Ribbon Reader and Ribbon Writer select any of bits 1 to 31 instead of 1 to 4, so one ribbon can carry up to 31 independent signals. The bit selector side screen lists Bit 1 to Bit 31 in a scroll view, both buildings show the selected bit on an amber two-digit display with a light for that bit's state, and a NOT gate on a ribbon inverts all 31 bits. The mod steps aside and logs it when Digital CPU is enabled or another mod already patches the reader's or writer's bit depth. Version 1.2.0 in `mod_info.yaml`, minimum supported build 744825, published on the Steam Workshop. Players of ribbon-based automation builds use it.

## Where things are
**Id:** blk-977e72b83a82

Top-level folders and key files:

- `src/Bitshifter/`: the mod. `Mod.cs` (entry point and compatibility checks), `Patches.cs` (bit depth, building defs, visuals, display and NOT gate patches), `SideScreen.cs` (the 31-bit selector), `OverlayLabelGuard.cs` (keeps Better Automation Overlay labels working), `anim/` (reader and writer art), `mod.yaml`, `mod_info.yaml`.
- `src/Bitshifter.Tests/`: in-game tests for compatibility, gates, readout, settings, side screen and signals.
- `tools/make_art.py`: generates the building art.
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description, tags, preview image and the packaged mod.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
