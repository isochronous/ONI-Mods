---
description: "TODO(owner): one line saying what multichannel-critter-sensor is."
---
# multichannel-critter-sensor

## What this is
**Id:** blk-3ed615c5605e

Multichannel Critter Sensor is an Oxygen Not Included mod that adds a room-based critter and egg counter for the Automation Ribbon. It counts critters and eggs in its room like the vanilla Critter Sensor, lets the player choose which species and egg types to count, and writes a 4-bit value to its ribbon output port, with combined or separate critter/egg thresholds and rising/falling count pulses. The building has its own kanim art by 3GuB, used with permission, and a side screen built from PLib UI plus clones of the vanilla threshold editors. Version 0.3.0 is released on the Steam Workshop (id 3804039425); `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`. The mod is available in English, Chinese (Simplified), Korean and Russian.

## Where things are
**Id:** blk-c1548a895d61

- `src/MultichannelCritterSensor/`: the mod. `Mod.cs` (entry point), `MultichannelCritterSensorConfig.cs` (building definition), `MultichannelCritterSensor.cs` (the counting component), `MultichannelCritterSensorSideScreen.cs` (the side screen), `ThresholdAdapter.cs` (exposes a threshold to the cloned vanilla editor), `Patches.cs` (build menu, research and details-screen patches), `ModStrings.cs`, `mod.yaml`, `mod_info.yaml`.
- `src/MultichannelCritterSensor/anim/` and `anim-original/`: the sensor's kanim art and the original art it came from; `translations/` holds the `.pot` template and `.po` files.
- `src/MultichannelCritterSensor.Tests/`: in-game tests (`Config.cs`, `Counting.cs`, `SideScreen.cs`, shared `Kit.cs`).
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`, `logo.jpg`, `preview.gif`).
- `README.md`, `CHANGELOG.md`: user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
