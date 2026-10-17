---
tags: bridge-end
description: "Overview of the Jumper Bridges mod: what direct bridge ends on ports do for automation and power, its version, translations and release state, and a map of its source, tests, tools and publish folders"
---

# jumper-bridges

## What this is
**Id:** blk-2d2edc3c3db1  
**Tags:** mod-options, mod-strings

Jumper Bridges is an Oxygen Not Included mod: an Automation Wire Bridge, Automation Ribbon Bridge, Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge can be placed with either end directly on any automation or power port cell, and that port counts as connected. Each end acts as its own piece of wire, drawn like one, with nothing to build, deconstruct or pay for. Bridges chain end on end, and a wire dragged up to a bridge end joins it. The automation and power halves can each be turned off in the mod options. Version 0.2.0 in `mod_info.yaml` (released 2026-10-17), minimum supported build 744825, published on the Steam Workshop with Korean, Russian and Chinese translations.

## Where things are
**Id:** blk-7a7996b980da  

Top-level folders and key files:

- `src/JumperBridges/`: the mod. `JumperBridge.cs` (per-bridge component that owns the end nodes), `BridgeEndNode.cs` (the end node, a graph tile visualizer), `BridgeKind.cs` (layers and managers per automation or power kind), `BridgeLinks.cs` (link rings between nodes), `BridgeFootprint.cs` (finds a bridge by its end cells), `Patches.cs` (Harmony patches), `Options.cs` and `ModStrings.cs` (options and translatable strings), `Mod.cs`, `translations/`, `mod.yaml`, `mod_info.yaml`.
- `src/JumperBridges.Tests/`: in-game tests for deconstruction, gates, power, preview, signals and wire drags.
- `tools/make_preview.py`: builds the preview image.
- `README.md`: what the mod does, compatibility, installing and how it works.
- `CHANGELOG.md`: release notes per version.
- `publish/`: Workshop description, tags, preview image and the packaged mod (`JumperBridges.zip`, plus `DirectAutomationBridges.zip`, the mod's earlier name).

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
