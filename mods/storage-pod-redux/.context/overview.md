---
tags: storage-building
description: "Overview of the Storage Pod Redux mod: what the Storage Pod and Cool Pod do, their capacity, cost and power figures, version, and a map of its source, art, translations, tests and publish folders"
---

# storage-pod-redux

## What this is
**Id:** blk-756ddf9d7bdd  
**Tags:** refrigerator, mod-options, kanim-art

Storage Pod Redux is an Oxygen Not Included mod that adds two compact 1x1 storage buildings that can be built anywhere, rockets included. The Storage Pod is a Storage Bin in one cell: 5,000 kg of solids (configurable), no power, 100 kg of Refined Metal, unlocked by Refined Renovations. The Cool Pod is a fridge in one cell: 50 kg of food (configurable), 60 W dropping to 10 W once everything inside is cold, satisfies the Kitchen room's refrigerator requirement. It is a from-scratch rewrite of the code of Skyrunner's Storage Pod, whose Cool Pod crashes on completion since the Sweet Dreams update; art by 3GuB, used with permission. Each pod can be turned off in the options. Version 1.1.0. Targets the current game with all DLC content (minimumSupportedBuild 744825).

## Where things are
**Id:** blk-8a97644fd6fe  

- `README.md`, `CHANGELOG.md`: player-facing description and release notes.
- `src/StoragePodRedux/`: the mod. `Mod.cs` (entry point), `StoragePodConfig.cs` and `CoolPodConfig.cs` (building definitions), `Buildings.cs` (registration gate), `Options.cs` (settings), `ModDetection.cs` (detects other enabled mods and reads their config files), `Patches.cs` (Harmony patches), `ModStrings.cs` (UI strings), `mod.yaml` and `mod_info.yaml` (manifests).
- `src/StoragePodRedux/anim/`: the kanim art.
- `src/StoragePodRedux/translations/`: `.pot` template and `ko`, `ru`, `zh` po files.
- `src/StoragePodRedux.Tests/`: in-game tests (pods, registration, decisions).
- `publish/`: Workshop description, preview image and workshop id.
- `.context/`: `overview.md`, `domains/` ([entities](domains/entities.md), [glossary](domains/glossary.md)), `standards/` ([code-style](standards/code-style.md), [error-handling](standards/error-handling.md), [naming-conventions](standards/naming-conventions.md)), `decisions/`, `retrospectives/`, `META.md`, `projects.md`.
