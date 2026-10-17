---
tags: detection-range
description: "Overview of the Motion Sensor Range mod: the range slider it adds to the Duplicant Motion Sensor, and a map of its source, tests, preview art, publish, README and `.context` folders"
---

# motion-sensor-range

## What this is
**Id:** blk-38f9329ac7bb

Motion Sensor Range is an Oxygen Not Included mod that makes the detection range of the vanilla Duplicant Motion Sensor adjustable from a slider in the building's side screen: 2 to 16 tiles in steps of 2, default 4 (the vanilla range). It adds no building of its own. A `RangeSlider` component is attached to the sensor prefab by a Harmony postfix and rewrites the sensor's detection area at runtime; the range is saved with the building, works with the vanilla copy-settings tool, and carries over from an under-construction building. It is a from-scratch rewrite of Cairath's Configurable Motion Sensor Range for the current game. Version 1.2.0 is released on the Steam Workshop (id 3804025857); `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`, so it loads with every DLC.

## Where things are
**Id:** blk-2ddb429ef268

- `src/MotionSensorRange/`: the mod. `Mod.cs` (entry point), `Patches.cs` (Harmony patches on the motion sensor config and construction), `RangeSlider.cs` (the component), `ModStrings.cs` (text registered under the game's STRINGS tree), `mod.yaml` and `mod_info.yaml` (game manifests), `translations/` (`.pot` template plus `ko.po`, `ru.po`, `zh.po`).
- `src/MotionSensorRange.Tests/`: in-game tests for detection, persistence and range (`Detection.cs`, `Persistence.cs`, `Range.cs`).
- `assets-src/`: preview art sources and `PreviewGen/`, the small program that renders the Workshop preview.
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`, `preview.png`, and `obsolete-description.txt` for the retired earlier upload).
- `README.md`, `CHANGELOG.md`: user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
