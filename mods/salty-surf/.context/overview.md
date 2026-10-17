---
tags: mechanical-surfboard
description: "Overview of the Salty Surf mod: how the Mechanical Surfboard accepts Salt Water as well as Water, its Workshop version, supported content and languages, and a map of its source, tests and publish folders"
---

# salty-surf

## What this is
**Id:** blk-11a37a51f75f  
**Tags:** info-panel-text, mod-strings, vanilla-fallback

Salty Surf is an Oxygen Not Included mod that lets the Mechanical Surfboard run on Salt Water as well as Water. Either liquid fills the tank without damaging the building, the board is ready once the tank holds the usual 2 kg of either, and while a Duplicant surfs the board spills whatever it was fed. Every other liquid is treated as in the base game: it damages the building and is dumped. The building's info panel lists both accepted liquids. Version 0.2.0 is on the Steam Workshop (id 3816726470); `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`. Available in English, Chinese (Simplified), Korean and Russian.

## Where things are
**Id:** blk-16065c0a902a  
**Tags:** mod-entry-point, harmony-patching, mod-strings, in-game-testing

- `src/SaltySurf/`: the mod. `Mod.cs` (entry point, the accepted-liquid helper and the Harmony patches on the element load and the surfboard config), `ModStrings.cs`, `mod.yaml`, `mod_info.yaml`, `translations/` (`.pot` template plus `ko.po`, `ru.po`, `zh.po`).
- `src/SaltySurf.Tests/`: in-game tests (`Board.cs`, `Intake.cs`, `Persistence.cs`, `Spill.cs`, `Surfing.cs`).
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`, `workshop-tags.txt`, `preview.jpg`); `preview/` holds the preview image.
- `README.md`, `CHANGELOG.md`: user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
