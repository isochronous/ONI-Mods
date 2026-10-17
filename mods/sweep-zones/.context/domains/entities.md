---
tags: toolbar-tool, saved-state, cleaning-errands
doc_id: doc-f39997b0359c
description: The zone store, paint tool, toolbar icons, hotkey and Harmony patches of Sweep Zones Redux, with the rules that mark debris for sweeping and place mop errands, and the source file each lives in
applies_to:
  - "src/SweepZones/**"
---

# Sweep Zones domain

## Overview
**Id:** blk-262749aa3aad

Sweep Zones Redux adds a toolbar tool for painting persistent zones. Debris that lands in a sweep zone is marked for sweeping, and moppable puddles in a mop zone get mop errands, at the priority the zone was painted with, until the zone is erased. The mod is `mods/sweep-zones`, static ID `Isochronous.SweepZones`, source in `src/SweepZones/`. It is a from-scratch rewrite of Berkays/Rubacava's Sweep Zones and shares no code or assets with it.

## Key Entities
**Id:** blk-47f9fb177298

### ZoneStore (saved component)
**Id:** blk-d8d071a9cbe4  
**Description**: Holds the painted zones and applies them once a second. Attached to the `SaveGame` object so the zones serialize into the save file (loading without the mod drops them).  
**Location**: `src/SweepZones/ZoneStore.cs`

| Property | Type | Description |
|----------|------|-------------|
| sweepCells | Dictionary<int, PrioritySetting>, saved | Sweep zone cells and the priority each was painted with |
| mopCells | Dictionary<int, PrioritySetting>, saved | Mop zone cells and their priorities |
| Version | int | Bumped on every mutation so the tool's display can cache |
| Instance | ZoneStore | The current store, set in `OnPrefabInit` |

**Relationships**: read and written by `SweepZoneTool`; implements `ISim1000ms`.

### SweepZoneTool
**Id:** blk-8d4b11374898  
**Tags:** reflection  
**Description**: The box-drag tool, a subclass of the game's `DragTool`. Owns the mode, the zone display (cell colours, perimeter stroke, priority labels) and the toolbar-menu filters. Cursor and area-visualizer assets are cloned from the vanilla Sweep tool by reflection (field names verified against U59-744825).  
**Location**: `src/SweepZones/SweepZoneTool.cs`

| Property | Type | Description |
|----------|------|-------------|
| mode | ZoneMode | `Sweep`, `ClearSweep`, `Mop`, `ClearMop` |
| filters | ToolParameterMenu.ToggleData[] | The four mode toggles in the game's tool side menu (`ModStrings.FilterSweep`, `FilterClearSweep`, `FilterMop`, `FilterClearMop`) |
| cellColors | HashSet<ToolMenu.CellColorData> | Per-cell tint returned by `GetOverlayColorData` |

**Relationships**: uses `ToolMenu.Instance.PriorityScreen` for the painted priority; reads `ZoneStore.Instance`.

### ToolIcon
**Id:** blk-a6045495610f  
**Description**: Registers the toolbar sprite `isochronous_sweepzones_tool` from a PNG embedded in the DLL (resource `SweepZones.assets.tool_icon.png`), with a procedurally drawn fallback. Source art is in `assets-src/`.  
**Location**: `src/SweepZones/ToolIcon.cs`

### Hotkey
**Id:** blk-7d58cc65b189  
**Tags:** hotkey  
**Description**: A PLib `PAction`, `Isochronous.SweepZones.Tool`, default Shift+Z, rebindable under options > game > controls > Mods. Held in `SweepZonesMod.ToolAction`.  
**Location**: `src/SweepZones/Mod.cs`

### Harmony patches
**Id:** blk-8ed739708efc  
**Tags:** harmony-patching  
**Location**: `src/SweepZones/Patches.cs`

| Target | Kind | Purpose |
|--------|------|---------|
| `Db.Initialize` | Postfix | Registers the tool icon |
| `SaveGame.OnPrefabInit` | Postfix | Adds the `ZoneStore` component |
| `PlayerController.OnPrefabInit` | Postfix | Creates the `SweepZoneTool` object and appends it to `tools` |
| `ToolMenu.CreateBasicTools` | Postfix | Adds the toolbar button, bound to the hotkey action |

## Business Rules
**Id:** blk-a6ab594b39d9

- A zone cell must be an open cell directly above solid ground: not `Grid.Solid`, with a valid solid cell below (`ZoneStore.IsValidZoneCell`). `Grid.Solid` is the sim's material solidity, so surfaces only Duplicants can stand on (open pneumatic doors) are not ground.
- Painting skips invalid cells; erasing is never restricted.
- Each zone cell remembers the priority (including yellow alert) selected when painted.
- Every second, debris in a sweep zone is marked for clearing, unless it is a Duplicant (`GameTags.BaseMinion`, `MinionIdentity`), already marked (`GameTags.Garbage`), or not clearable (critters). A priority already set on an item is never overridden, so manual changes stick.
- Mop zones follow the vanilla mop rules: a liquid cell on a valid zone cell with no placer yet and mass not over the game's `MopTool.maxMopAmt`.
- Each time the tool is activated or its mode switched, `PruneInvalidCells` drops zone cells that no longer qualify (floor dug out, cell built over).
- Erasing a zone keeps errands already created; the vanilla Cancel tool removes them.
- Only the zones of the selected mode's kind are shown (sweep modes show sweep zones, mop modes show mop zones). The display uses the tool's own cell-colouring hook, never an overlay, so overlay-adding mods are unaffected.

## Domain Terminology
**Id:** blk-400a3cb8e2b1

| Term | Definition |
|------|-----------|
| Sweep zone | Painted cells where debris is marked for sweeping |
| Mop zone | Painted cells where moppable puddles get mop errands |
| Region | A 4-connected group of zone cells of one priority; shows one centred priority label |
| Zone cell | A grid cell in `sweepCells` or `mopCells` |

## Key Code Paths
**Id:** blk-a257f96c9ea1  
**Tags:** harmony-patching, in-game-testing

- **Painting**: `DragTool.OnDragTool(cell, ...)` -> `ZoneStore.SetSweep/SetMop/RemoveSweep/RemoveMop` according to `mode`, priority from `PriorityScreen.GetLastSelectedPriority()`. See `src/SweepZones/SweepZoneTool.cs`.
- **Applying zones**: `ZoneStore.Sim1000ms` -> `MarkSweepZones` (walks `Grid.Objects[cell, Pickupables]` and its `objectLayerListItem` chain, `Clearable.MarkForClear`, `Prioritizable.SetMasterPriority`) and `PlaceMopErrands` (instantiates the `MopPlacer` prefab). See `src/SweepZones/ZoneStore.cs`.
- **Display**: `GetOverlayColorData` tints cells; `LateUpdate` -> `RefreshPriorityLabels` rebuilds the outline mesh and region labels (`FindRegions`) whenever `ZoneStore.Version` changes. See `src/SweepZones/SweepZoneTool.cs`.
- **Mode switching**: `OnActivateTool` fills the `ToolParameterMenu`; `OnParametersChanged` sets `mode`; `RefreshModePresentation` prunes, shows or hides the priority screen and sets the hover-card action text.
- **Tool registration**: `PlayerController.OnPrefabInit` patch, then `ToolMenu.CreateBasicTools` patch. See `src/SweepZones/Patches.cs`.
- **Tests**: in-game `[OniTest]` suites in `src/SweepZones.Tests/` (`Zones.cs`, `Marking.cs`, `Painting.cs`, `Display.cs`, `ByHand.cs`).

## Related
**Id:** blk-7c53bf2eeb5b

- See also: [glossary](glossary.md)
