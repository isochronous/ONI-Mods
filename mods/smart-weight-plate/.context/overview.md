---
description: "TODO(owner): one line saying what smart-weight-plate is."
---
# smart-weight-plate

## What this is
**Id:** blk-a357f3b4017a

Smart Weight Plate is an Oxygen Not Included mod that adds one building, a weight plate with a low and a high threshold. It weighs what sits on the tile above it like the vanilla Weight Plate (solid tiles, loose items and debris, Duplicants and critters; gases and liquids do not count). It sends a green signal once the weight reaches the High Threshold and holds it until the weight drops to the Low Threshold, then sends red, the way a Liquid or Gas Reservoir's signal behaves. Thresholds run 0 to 2000 kg, an Invert Signal option swaps the colours, and settings copy with the copy-settings tool. Art by 3GuB. Version 0.3.0, status work in progress. Targets the current game with all DLC content (minimumSupportedBuild 744825).

## Where things are
**Id:** blk-fb799be8fbf0

- `README.md`, `CHANGELOG.md`: player-facing description and release notes.
- `src/SmartWeightPlate/`: the mod. `Mod.cs` (entry point), `SmartWeightPlateConfig.cs` (building definition), `SmartWeightPlate.cs` (the two-threshold latch component), `SmartWeightPlateSideScreen.cs` (the panel), `Patches.cs` (Harmony patches), `ModStrings.cs` (UI strings), `mod.yaml` and `mod_info.yaml` (mod manifests).
- `src/SmartWeightPlate/anim/assets/smart_weight_plate/`: the kanim art.
- `src/SmartWeightPlate/translations/`: `.pot` template and `ko`, `ru`, `zh` po files.
- `src/SmartWeightPlate.Tests/`: in-game tests (weighing, signal, settings, side screen, compatibility).
- `tools/knobs_follow_pressure.py`: reworks the animations in place so the knobs and button follow the plate's pressure.
- `publish/`: Workshop description, preview image and workshop id.
- `.context/`: `overview.md`, `domains/` ([entities](domains/entities.md), [glossary](domains/glossary.md)), `standards/` ([code-style](standards/code-style.md), [error-handling](standards/error-handling.md), [naming-conventions](standards/naming-conventions.md)), `decisions/`, `retrospectives/`, `META.md`, `projects.md`.
