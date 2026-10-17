---
tags: toolbar-tool, building-replacement, building-materials, plan-presets, mod-compatibility
description: "Overview of the Upgrade Planner mod: the box-drag tool that changes building materials or replaces building types, its tabs, presets and Blueprints Expanded requirement, version, and a map of its source folders, tests and publish files"
---

# upgrade-planner

## What this is
**Id:** blk-61c64a38a275

Upgrade Planner is an Oxygen Not Included mod that adds a toolbar tool for rebuilding the buildings in a box-dragged area. It changes the material of one building type (every sandstone tile, ladder or pipe rebuilt in granite) or replaces one type with another the game lets you build over it (plain pipes to insulated, insulated to plain, modded pipes included). The mod reads the game's own replacement rules and queues the same replacement order the build tool would, once per matching building, keeping each building's orientation. It has a Change material tab, a type tab and presets. It requires Blueprints Expanded by Sgt_Imalas, which keeps an upgraded building's settings and makes every building available to the Change material tab. Version 0.5.0. Targets the current game with all DLC content (minimumSupportedBuild 744825).

## Where things are
**Id:** blk-e9942a956580  
**Tags:** mod-entry-point, harmony-patching, mod-strings, in-game-testing

- `README.md`, `CHANGELOG.md`: player-facing description and release notes.
- `src/UpgradePlanner/`: the mod. `Mod.cs` (entry point), `Patches.cs` (Harmony patches), `ModStrings.cs` (UI strings), `mod.yaml` and `mod_info.yaml` (manifests).
- `src/UpgradePlanner/Tool/`: the tool and the order queue (`UpgradeTool.cs`, `PlacementQueue.cs`, `Orders.cs`, `WindowToggle.cs`).
- `src/UpgradePlanner/Plan/`: plans and presets (`UpgradePlan.cs`, `ChangeMaterialPlan.cs`, `ReplaceTypePlan.cs`, `Presets.cs`).
- `src/UpgradePlanner/Rules/`: replacement rules, families, variants and materials.
- `src/UpgradePlanner/UI/`: the planner dialog, tabs, material picker, preset bar, hover card and tool icon.
- `src/UpgradePlanner/Compat/`: Blueprints Expanded link and settings handling for other mods (`BlueprintsCompat.cs`, `ModCompat.cs`, `OtherModSettings.cs`).
- `src/UpgradePlanner/translations/`: `.pot` template and `ko`, `ru`, `zh` po files.
- `src/UpgradePlanner.Tests/`: in-game tests, with its own `README.md`.
- `lib/PLib/`: the PLib copy the mod builds against.
- `preview/`: preview image sources; `publish/`: Workshop description, tags, preview image and workshop id.
- `.context/`: `overview.md`, `domains/` ([entities](domains/entities.md), [glossary](domains/glossary.md)), `standards/` ([code-style](standards/code-style.md), [error-handling](standards/error-handling.md), [naming-conventions](standards/naming-conventions.md)), `decisions/`, `retrospectives/`, `META.md`, `projects.md`.
