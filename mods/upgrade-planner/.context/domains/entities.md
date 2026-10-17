---
tags: toolbar-tool, building-replacement, building-materials, plan-presets, mod-compatibility
doc_id: doc-5e37d1980952
description: The planner tool, plan classes, replacement rules, order queue, presets, dialog, Blueprints Expanded compatibility, hotkey and patches of Upgrade Planner, with business rules, saved state and the source file each lives in
applies_to:
  - "src/UpgradePlanner/**"
---

# Upgrade Planner domain

## Overview
**Id:** blk-e852e7d310a2

Upgrade Planner adds a toolbar tool that rebuilds the buildings in a box-dragged area: the same type in another material (Change material), or one type replaced by another the game lets you build over it (Replace type). It reads the game's own replacement rules and queues the same replacement order the build tool would. It depends on Blueprints Expanded (static ID `BlueprintsV2`) to keep an upgraded building's settings. The mod is `mods/upgrade-planner`, static ID `Isochronous.UpgradePlanner`, source in `src/UpgradePlanner/`. It adds no buildings.

## Key Entities
**Id:** blk-abde66e7508d

### UpgradeTool
**Id:** blk-7671dcf41450  
**Tags:** reflection  
**Description**: The box-drag tool, a subclass of the game's `DragTool`. Cursor and area-visualizer assets are cloned from the vanilla Sweep (`ClearTool`) tool by reflection. Owns the dialog, the compiled plan, the highlight (blue for buildings the plan would touch, green for ones already pending), the overlay switch and the hover card. A drag works from the plan as it stood at the last Apply.  
**Location**: `src/UpgradePlanner/Tool/UpgradeTool.cs`

**Relationships**: holds `UpgradePlan.Current` and an `applied` snapshot; opens `UpgradeDialog`; hands drags to `PlacementQueue`; `WindowToggle` drives the "Upgrade Planner Config" button next to the priority picker.

### UpgradePlan, ChangeMaterialPlan, ReplaceTypePlan
**Id:** blk-10be300a8170  
**Description**: The player's choices on both tabs, kept for the session. `UpgradePlan.Current` holds the active `PlanMode` (`ChangeMaterial` or `ReplaceType`) and a `Version` bumped on every change, so highlights recompile. Each tab implements `IPlanTab` (`IsComplete`, `Summary`, `TouchedDefs`, `Compile`, `Save`, `Load`).  
**Location**: `src/UpgradePlanner/Plan/UpgradePlan.cs`, `ChangeMaterialPlan.cs`, `ReplaceTypePlan.cs`

| Property | Type | Description |
|----------|------|-------------|
| Change material: picked types | HashSet<string> | Prefab IDs of picked types; null means every offered type |
| Change material: SourceMode, Source, Target | MaterialMode, Tag, Tag | `Any` or `Specific` material to replace; the one material to build in |
| Change material: AllBuildings | bool | Also offer buildings the game cannot build over (needs Blueprints Expanded) |
| Replace type: Source, SourceMode, SourceMaterials | Family, MaterialMode, Tag[] | The type to replace and its materials, one per recipe ingredient |
| Replace type: Target, TargetMaterials | Family, Tag[] | The type to build over it and its materials |

### Matcher and Upgrade
**Id:** blk-eb0cfd6059c7  
**Description**: A plan compiled for a drag or highlight. `Matcher.Matches(building)` says whether the plan takes a building; `TryPlan(building, out Upgrade)` says what it becomes. `Upgrade` holds the target `BuildingDef`, the materials and `Rebuild` (deconstruct and reconstruct instead of building over). Made again when the plan changes and once a second, since research changes what is allowed.  
**Location**: `src/UpgradePlanner/Plan/UpgradePlan.cs`

### Family and Category
**Id:** blk-edd76d9c5a3f  
**Description**: A `Family` is a building type as the build menu lists it: usually one def, or several defs made by one config class when a mod makes one def per material and lists one (Decor Pack's stained glass tiles). A `Category` is a heading of families, by replacement layer or by build menu category.  
**Location**: `src/UpgradePlanner/Rules/Families.cs`

### Rules
**Id:** blk-b0f419b32269  
**Description**: `ReplacementRules` decides what the game lets be built over what (`IsReplaceable`, `CanReplace`, `IsBuildable`, source and target categories). `MaterialSwap` decides which types the Change material tab offers and which recipe ingredient a change swaps. `Materials` reads what a building was built from (`Deconstructable.constructionElements`). `Variants.Resolve` picks the def to build for a family and materials by running other mods' `BuildTool.Activate` prefixes.  
**Location**: `src/UpgradePlanner/Rules/ReplacementRules.cs`, `MaterialSwap.cs`, `Materials.cs`, `Variants.cs`

### Orders and PlacementQueue
**Id:** blk-89e4c8708ab0  
**Tags:** performance  
**Description**: `Orders.Place` queues one upgrade: `Replace` follows the build tool's sequence (`GetReplacementCandidate`, `IsReplacementLayerOccupied` over the footprint, `CanReplace`, same-type-same-material skip, `TryReplaceTile`, write into the replacement layer, priority from the tool menu); `Reconstruct` goes through `Reconstructable.RequestReconstruct` with every ingredient set. `PlacementQueue` places the drag's buildings in slices of about 4 ms a frame, re-checks each as placed, and shows the popup when the last is done. With instant build (debug toggle or sandbox setting) buildings are replaced on the spot.  
**Location**: `src/UpgradePlanner/Tool/Orders.cs`, `src/UpgradePlanner/Tool/PlacementQueue.cs`

### Presets
**Id:** blk-82a395d5be77  
**Description**: A `PlanPreset` is one tab's plan saved by name, by prefab ID and element tag so it reads back in any colony. Each tab lists only its own presets and a name is unique within a tab. Stored in `mods/config/UpgradePlanner/presets.json` under the game's data folder (`Presets.FilePath`); tests point it elsewhere.  
**Location**: `src/UpgradePlanner/Plan/Presets.cs`, `src/UpgradePlanner/UI/PresetBar.cs`

### Window
**Id:** blk-07f5b34ed3fd  
**Description**: A PLib `PDialog` (`UpgradeDialog`), non-modal, with one view per tab (`MaterialTabView`, `TypeTabView`). Materials are picked in the game's own `MaterialSelectionPanel`, instantiated from the build menu's prefab (`MaterialPicker`). `Widgets.cs` holds shared controls, `ToolMarker` and `ToolIcon` the cursor marker and toolbar sprite, `PlannerHoverCard` the hover card.  
**Location**: `src/UpgradePlanner/UI/`

### Compat
**Id:** blk-24d2573ff9eb  
**Tags:** harmony-patching, reflection  
**Description**: `BlueprintsCompat` binds to Blueprints Expanded's `API_Methods.GetAllAdditionalBuildingData` and its `UnderConstructionDataTransfer` carrier, and sets `Loaded`. `OtherModSettings` registers Suppress Notifications' and No Manual Delivery's per-building settings with Blueprints Expanded's API. `ModCompat` patches Adaptive Building Menu's two layout appliers to skip the planner's pickers.  
**Location**: `src/UpgradePlanner/Compat/`

### Hotkey
**Id:** blk-32317980a9a4  
**Tags:** hotkey  
**Description**: PLib `PAction` `Isochronous.UpgradePlanner.Tool`, default Shift+U, in `UpgradePlannerMod.ToolAction`.  
**Location**: `src/UpgradePlanner/Mod.cs`

### Harmony patches
**Id:** blk-2fd79bc0f22b  
**Tags:** harmony-patching  
**Location**: `src/UpgradePlanner/Patches.cs`, `src/UpgradePlanner/Compat/ModCompat.cs`

| Target | Kind | Purpose |
|--------|------|---------|
| `Db.Initialize` | Postfix | Registers the tool icon |
| `PlayerController.OnPrefabInit` | Postfix | Adds `UpgradeTool` to the player's tools |
| `ToolMenu.CreateBasicTools` | Postfix | Adds the toolbar button |
| `MaterialSelector.UpdateScrollBar` | Postfix | Puts the planner's picker size back after the game sets the build menu's (pickers with a `SizeKeeper` only) |
| `MaterialSelector.OnSelectMaterial` | Prefix | Nulls the recipe for the planner's pickers so picks stay out of the game's saved last-material memory |
| `Reconstructable.TryCommenceReconstruct` | Postfix | `Orders.FollowRebuild` places the rebuilt building's order if the game's own was refused |
| Adaptive Building Menu `MaterialSelectorLayoutApplier.Apply`, `MaterialSelectionPanelLayoutApplier.Apply` | Prefix, found by name | Skips layout for the planner's pickers, only when that mod is loaded |

### Saved state
**Id:** blk-4e9607ecfcda

Presets in `presets.json`. The plan itself lives for the session only. Settings of rebuilt buildings are carried by Blueprints Expanded, not by this mod.

## Business Rules
**Id:** blk-5ec4d3dd1f4e  
**Tags:** vanilla-fallback

- The mod never changes what can replace what: a pair the game refuses in the build tool is refused here. A target qualifies when it names a replacement layer, the source carries one of its replacement tags, and the source's tile or object layer is one the target searches (`ReplacementRules.CanReplace`); utility buildings (wires, pipes, rails) of one tile layer replace each other.
- The Change material tab lists a type when the game lets it be built over by itself. With All buildings on and Blueprints Expanded loaded, any reconstructable type is also offered; those are deconstructed and rebuilt, out of service until done.
- A same-type swap is allowed only when the material changes; same type in the same materials is skipped.
- A building with a two-ingredient recipe has only the ingredient holding the material to replace swapped.
- A drag with no applied plan, or an incomplete one, shows an error instead of marking anything. Apply closes the window and keeps the tool active; Clear takes back the applied plan.
- Each marked building gets a replacement order with its own orientation at the tool's current priority. A popup counts those marked and those skipped (already pending, already that type and material).
- A building replaced with another type keeps none of its settings; a same-type rebuild keeps them through Blueprints Expanded.
- Without Blueprints Expanded, settings are lost and only buildings the game lets you build over are offered; a warning is logged.
- Picks in the planner's pickers never change the build menu's last-picked materials, including the Printing Pod's (`MaterialSwap.PickerResult` names the real building `Headquarters` the recipe needs).
- Nothing is deconstructed except the buildings All buildings adds.

## Domain Terminology
**Id:** blk-22b4459badcc

| Term | Definition |
|------|-----------|
| Replacement | The build tool's way of building over a finished building; the old one works until the new one is built |
| Reconstruct | The game's deconstruct-and-rebuild in a new material (the material button in a building's side screen) |
| Plan | The choices on a tab; compiled into a `Matcher` for drags |
| Applied plan | The plan as it stood at the last Apply; what drags use |
| Family | A building type as the build menu lists it, with any per-material variant defs |
| Change material / Replace type | The two tabs |
| All buildings | Header checkbox that also offers buildings the game cannot build over |
| Preset | A named, saved plan for one tab |

## Key Code Paths
**Id:** blk-ff3c8a3f00bf  
**Tags:** hotkey, mod-entry-point, in-game-testing

- **Select and configure**: toolbar button or Shift+U -> `UpgradeTool.OnActivateTool` -> window opens; choices change `UpgradePlan` -> `Version++` -> highlight recompiles. See `src/UpgradePlanner/Tool/UpgradeTool.cs`, `src/UpgradePlanner/UI/UpgradeDialog.cs`.
- **Apply and drag**: Apply snapshots the plan; button-up settles each building's `Upgrade` through the `Matcher`, then `PlacementQueue.Add` -> `Orders.Place`. See `src/UpgradePlanner/Tool/PlacementQueue.cs`, `src/UpgradePlanner/Tool/Orders.cs`.
- **Replacement vs rebuild**: `Upgrade.Rebuild` (from `MaterialSwap.Rebuilds`) -> `Orders.Reconstruct`, else `Orders.Replace`; `Reconstructable.TryCommenceReconstruct` patch -> `Orders.FollowRebuild`.
- **Settings carry-over**: `BlueprintsCompat.CarrySettings(oldBuilding, order)` in `Orders.Replace`. See `src/UpgradePlanner/Compat/BlueprintsCompat.cs`.
- **Startup**: `OnLoad` registers translations and the hotkey; `OnAllModsLoaded` runs `ModCompat.Apply` and `BlueprintsCompat.Init`. See `src/UpgradePlanner/Mod.cs`.
- **Presets**: `PresetBar` -> `Presets` -> `presets.json`. See `src/UpgradePlanner/UI/PresetBar.cs`, `src/UpgradePlanner/Plan/Presets.cs`.
- **Tests**: in-game `[OniTest]` suites in `src/UpgradePlanner.Tests/` (`MaterialChange.cs`, `TypeTab.cs`, `AllBuildings.cs`, `BuildingSettings.cs`, `WireReplace.cs`, `ToolParity.cs`, `DragTiming.cs`, `Closing.cs`, `Looks.cs`, and the `ByHand` walkthroughs); `src/UpgradePlanner.Tests/README.md` describes them.

## Related
**Id:** blk-de8d2d185a04

- See also: [glossary](glossary.md)
