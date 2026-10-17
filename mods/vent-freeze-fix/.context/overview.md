---
tags: liquid-vent, liquid-freezing, tile-repair
description: "Overview of the Vent Freeze Fix mod: the save/load liquid vent bug it addresses, its two options for insulating vent storage and repairing tiles after a load, version, and a map of its source, tests and publish folders"
---

# vent-freeze-fix

## What this is
**Id:** blk-50ab94beb967

Vent Freeze Fix is an Oxygen Not Included mod for a save/load bug with liquid vents. A save written while super-cooled liquid is in flight comes back from the load as a natural tile, either inside the vent (entombing it) or on the floor under it (taking the debris pile with it), because the game only applies its 80%-of-default-mass rule to tiles formed in running play. The mod insulates each liquid vent's storage so the packet it holds is not solidified, and for the first second after a load a watcher digs up any tile at or under a vent formed from less liquid than a tile needs, dropping it as debris at full mass. Both fixes are options, on by default; developer options add a reproduction button and debug logging. Version 0.2.0. Targets the current game with all DLC content (minimumSupportedBuild 744825).

## Where things are
**Id:** blk-9a8933671047  

- `README.md`, `CHANGELOG.md`: player-facing description and release notes.
- `src/VentFreezeFix/`: the mod. `Mod.cs` (entry point and the vent storage patch), `VentFreezeWatcher.cs` (post-load watcher), `DigRequests.cs` (the dig answer), `Options.cs` (settings), `DebugSave.cs` (the save-on-next-vent-emit reproduction button), `ModStrings.cs` (UI strings), `mod.yaml` and `mod_info.yaml` (manifests).
- `src/VentFreezeFix/translations/`: `.pot` template and `ko`, `ru`, `zh` po files.
- `src/VentFreezeFix.Tests/`: in-game tests (storage, repair, repro save).
- `publish/`: Workshop description, tags, preview image (with its xcf source) and workshop id.
- `.context/`: `overview.md`, `domains/` ([entities](domains/entities.md), [glossary](domains/glossary.md)), `standards/` ([code-style](standards/code-style.md), [error-handling](standards/error-handling.md), [naming-conventions](standards/naming-conventions.md)), `decisions/`, `retrospectives/`, `META.md`, `projects.md`.
