---
tags: toolbar-tool, cleaning-errands, saved-state
description: "Overview of the Sweep Zones Redux mod: the toolbar paint tool with four drag modes, per-cell priority and labels, version, and a map of its source, icon generator, tests, translations and publish folders"
---

# sweep-zones

## What this is
**Id:** blk-5cf836ddb339

Sweep Zones Redux is an Oxygen Not Included mod that adds a Sweep Zones tool to the toolbar. Players paint persistent zones, and debris that lands in a sweep zone is marked for sweeping, and moppable puddles in a mop zone get mop errands, at the priority the zone was painted with, until the zone is erased. The tool has four box-drag modes (sweep zone, erase sweep zone, mop zone, erase mop zone), remembers priority per cell including yellow alert, and shows priority labels over each contiguous same-priority region. It is a ground-up rewrite of the feature set of Berkays/Rubacava's Sweep Zones and shares no code or assets with it. Version 1.6.0. Targets the current game (U59+) with all DLC content (minimumSupportedBuild 744825).

## Where things are
**Id:** blk-7ba496c215ac  

- `README.md`, `CHANGELOG.md`: player-facing description and release notes.
- `src/SweepZones/`: the mod. `Mod.cs` (entry point), `SweepZoneTool.cs` (the paint tool), `ZoneStore.cs` (the saved zone store), `ToolIcon.cs` (toolbar icons), `Patches.cs` (Harmony patches), `ModStrings.cs` (UI strings), `mod.yaml` and `mod_info.yaml` (manifests).
- `src/SweepZones/assets/`: `tool_icon.png`.
- `src/SweepZones/translations/`: `.pot` template and `ko`, `ru`, `zh` po files.
- `src/SweepZones.Tests/`: in-game tests (zones, painting, marking, display, by-hand play).
- `assets-src/IconGen/`: the program that generates the tool icons; `assets-src/captures/`: reference captures of the icons and zones.
- `publish/`: Workshop description, preview image, workshop id and the old obsolete-mod description.
- `.context/`: `overview.md`, `domains/` ([entities](domains/entities.md), [glossary](domains/glossary.md)), `standards/` ([code-style](standards/code-style.md), [error-handling](standards/error-handling.md), [naming-conventions](standards/naming-conventions.md)), `decisions/`, `retrospectives/`, `META.md`, `projects.md`.
