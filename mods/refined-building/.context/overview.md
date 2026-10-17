---
description: "TODO(owner): one line saying what refined-building is."
---
# refined-building

## What this is
**Id:** blk-d87d99e6bbcc

Refined Building is an Oxygen Not Included mod that lets every refined metal be used wherever a building asks for raw Metal Ore: wire, pneumatic doors and airlocks, mesh tiles, batteries, pumps, valves, vents, coal generators, the electrolyzer, sinks and showers. After the game loads its element table, every solid element tagged `RefinedMetal` that lacks the `Metal` tag gets it, so the refined metals appear in those buildings' material lists. It finds the metals at runtime, so it covers the base game, the enabled DLCs and refined metals added by other mods. Buildings that ask for Refined Metal are untouched. It does the same job as Cairath's Refined Metals Usable As Raw Metals, without a fixed list. Version 1.0.1 is built; `publish/retired.txt` records that the mod is unlisted on the Workshop (id 3806989781) because an existing mod already does this job. `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`.

## Where things are
**Id:** blk-eecf05761bd9

- `src/RefinedBuilding/`: the mod. `Mod.cs` (entry point), `Patches.cs` (the postfix on `ElementLoader.Load` that tags the metals), `mod.yaml`, `mod_info.yaml`.
- `src/RefinedBuilding.Tests/`: in-game tests (`RefinedMetals.cs`).
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`, `retired.txt`).
- `README.md`, `CHANGELOG.md`: user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
