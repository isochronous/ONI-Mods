# Upgrade Planner

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: a tool that marks buildings in a box-dragged area to be rebuilt. Every sandstone tile, ladder and pipe rebuilt in granite in one drag, or one type replaced with another: plain pipes to insulated, insulated back to plain, or any modded pipe the game lets you build over the old one.

Status: **first version, in use**.

Requires [Blueprints Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=3468585385) by Sgt_Imalas, which keeps an upgraded building's settings and makes every building available to the Change material tab.

## What it does

Dragging a pipe over an existing pipe in the build tool is a *replacement*: the old pipe keeps flowing until the new one is built in its place. The game decides which replacements are legal from the building definitions (a replacement layer, a set of replacement tags, and the layer it looks for candidates on), and it allows a same-type swap as long as the material changes. The mod reads those same rules and queues the same replacement order the build tool would, once per matching building in a box, with each building's own orientation.

**Using it.** Select the Upgrade Planner from the toolbar (or its hotkey, Shift+U by default, rebindable under Options, Controls, Mods). A window opens with two tabs.

**Change material** rebuilds buildings as the same type in another material. Its left column holds **Replace** (Any material, or Specific and one material in the game's own material picker) over **With** (the one material to build in, in the same picker). Its right column lists every type that can be rebuilt as itself (tiles, backwalls, ladders, pipes, rails, wires and the modded ones), grouped by family as toggle buttons with their build menu icons. Each family's header has a checkbox that picks the whole family, a chevron that folds it away, and a count of how many are picked; Select all, Expand all and Collapse all sit at the top. A type the chosen materials rule out is greyed, and its tooltip says why: not researched yet, never built from the material to replace, or unable to be built in the new one. The footer names what a drag does, listing the first four types and counting the rest. A building with a two-ingredient recipe has the ingredient that holds the material to replace swapped.

**All buildings**, a checkbox in the Buildings header, also offers the buildings the game does not let you build over (machines, sensors, automation gates, storage...), grouped as the build menu groups them. Those are deconstructed by a Duplicant and rebuilt in the new material through the game's own reconstruct (the material button in a building's side screen), so they are out of service until rebuilt; types it adds start unpicked. The select-all checkbox sits at the left of the Buildings header.

**Settings are kept.** Blueprints Expanded stores a building's settings (thresholds, filters, access, capacity, name, enabled state and the rest it knows) and applies them to the rebuilt building, whether it was built over or reconstructed, and across a save and reload. A building replaced with another type keeps none.

**Replace type** replaces one type with another, in two columns, each scrolling on its own:

- **Replace**: every replaceable building type as the build menu lists it, grouped by family (Liquid Pipes, Ventilation Pipes, Conveyor Rails, Wires, Automation Wires, Tiles, Ladders, and so on, including families and types added by mods). Pick one, then **Any** material, or **Specific** and pick the materials in the game's own material panel, one row per recipe ingredient; only buildings built from exactly those materials are touched.
- **With**: the types the game lets you build over the chosen one, filtered to what you have researched, then its materials in the same game panel (a two-ingredient recipe such as a modded pressurized pipe or a stained glass tile shows two rows). The material in stock is picked for you to start with, as in the build menu.

While the tool is active, every building the plan would touch is highlighted in the world (blue; green once a replacement is already pending on it), and the overlay switches to the family's view (pipes, vents, power, automation) the way the build tool does, when every type in the plan shares one. Box-drag over the pipes you want: each matching building gets a replacement order at the current tool priority, and a popup says how many were marked and how many were skipped (already pending, or already that type in that material). Dragging before the plan is complete shows an error instead. With instant build on (the debug toggle, or sandbox mode with its instant build setting), the buildings are replaced on the spot instead, as the build tool does.

**Apply** closes the window and keeps the tool active for dragging; an **Upgrade Planner Config** button next to the priority picker opens it again (it shows as pressed, and does nothing, while the window is open). The window's X and Escape close it the same way, leaving the tool active; a second Escape ends the tool, as does the tool's button in the toolbar. Drags use the plan as it was when you last pressed **Apply**: changes in the window take effect at the next Apply, and a tab's **Clear** takes back what was applied. Until something is applied, a drag asks you to apply the settings first. While you drag, the buildings in the box that the plan would change turn green, and the hover card lists them by type with their counts and the materials they take. While the window is open, the highlight shows the plan you are editing; once it is closed, the applied one. The plan is remembered for the session, so selecting the tool again resumes where you left off.

**Presets.** Each tab has its own presets. The row above the tabs lists the current tab's, saves that tab's plan under a name (saving under a name the tab already has replaces that preset), and loads or deletes a saved one; loading changes only that tab's choices. Presets are kept in `mods/config/UpgradePlanner/presets.json` in the game's data folder, so they last across colonies and mod updates.

**What it never does.** It does not change what can replace what: a pair the game refuses in the build tool is refused here too. Nothing is deconstructed except the buildings All buildings adds, rebuilt as the game's own reconstruct does. Cancel pending replacements with the Cancel tool, as with any build order, and a reconstruct by cancelling its deconstruction.

## Installing

As a local mod:

1. Download `UpgradePlanner-<version>.zip` from the [latest release](https://github.com/isochronous/upgrade-planner/releases/latest).
2. Extract it into a new folder named `UpgradePlanner` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\UpgradePlanner`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/UpgradePlanner`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/upgrade-planner.git
dotnet build upgrade-planner/src/UpgradePlanner -c Release
```

PLib is merged into the DLL by the shared build, so the mod ships as a single file.

## Notes for modders

- Six Harmony patches (`Patches.cs`). Three register the icon (`Db.Initialize`), add the tool to the player's tools (`PlayerController.OnPrefabInit`), and add the toolbar button (`ToolMenu.CreateBasicTools`). A postfix on `MaterialSelector.UpdateScrollBar` gives the planner's own material pickers their size back each time the game sets the build menu's (every 200 ms); pickers without the planner's `SizeKeeper` are left alone. A postfix on `Reconstructable.TryCommenceReconstruct` places the rebuilt building's order when the game's own placement was refused because the old building's cells were not clear yet (a 3-wide farm tile), half a game second after the game's own attempt (`Orders.PlaceRebuildIfMissing`). One on `MaterialSelector.OnSelectMaterial` is described with the Change material pickers below.
- Blueprints Expanded is found in `OnAllModsLoaded` by its static ID (`BlueprintsV2`, enabled for the active DLC), as its author asks of integrations, and reached through delegates made from its API (`BlueprintsCompat`). A replacement order gets the old building's settings from `API_Methods.GetAllAdditionalBuildingData` written into its `UnderConstructionDataTransfer`, which Blueprints Expanded applies to the finished building of the same type in that cell; the order itself is left alone, so its priority stays the tool's. A reconstruct goes through `Reconstructable.RequestReconstruct`, with every ingredient set (the game's button sets only the first), and Blueprints Expanded's own patch on it carries the settings. With instant build the planner finishes the deconstruction and then the new order, as instant build finishes a replacement.
- Adaptive Building Menu lays out every material picker for the build menu's side bar; when it is loaded, its two layout methods are patched (found by name, in `ModCompat`) to skip the planner's pickers, whose containers carry a `PlannerPickerHost` from before the picker is made.
- A picker's own scroll view is on only when its rows overflow: its scrollbar is set to show permanently, which Unity enforces every frame while the view is on, and the game hides it on each refresh, so it flickered.
- The Replace type tab's Replace side does not scroll as a whole: the types scroll in one white card taking two shares of the height, over the material card's one.
- The Change material tab's Materials column does not scroll: Replace and With each ask for no height and an equal flexible share, so they split it in half, and each holds its picker at full height in a scroll pane of its own. The picker's own scroll view is switched off so the wheel reaches that pane.
- A drag works from the plan compiled into a `Matcher` (a lookup of the types it takes and the ingredient it swaps), made again when the plan changes; the highlight uses the same, made again each second since research changes what is allowed.
- A drag settles what each building becomes when the button goes up, and `PlacementQueue` places them about 4 ms of work a frame, starting in the drag's own frame, so a small drag is done at once and a large one does not hold the game up (304 tiles took 120 ms in one frame as orders, 180 ms with instant build; now no frame spends more than about 5 ms). Each building is checked again as it is placed; a drag made meanwhile joins the queue, and the popup comes when the last is placed. `DragTiming` in the tests measures it.
- Queueing one replacement is the build tool's own sequence: `GetReplacementCandidate`, `IsReplacementLayerOccupied` over the footprint, `CanReplace`, the same-type-same-material skip, then `TryReplaceTile` and the write into the replacement layer, with the priority from the tool menu. See `Orders.Replace`.
- Alternatives for a type are computed in `ReplacementRules.CanReplace`: the target names a replacement layer, the source prefab carries one of the target's replacement tags, and the source's tile or object layer is one the target searches for candidates. A mod whose pipes replace vanilla pipes in the build tool satisfies all three, so it appears without being named.
- The window is a PLib `PDialog` (`UpgradeDialog`, with one view per tab), a plain (non-modal) screen laid out again when a choice changes what it shows, so button highlights follow the plan; materials are picked in the game's `MaterialSelectionPanel`, instantiated from the build menu's prefab into the window (`MaterialPicker.cs`), with its priority strip hidden and its rows widened. Closing the window deselects the tool.
- A mod that makes one def per material and lists one of them in the build menu (Decor Pack's stained glass tiles, one per dye, all from one config class) shows as one type: defs made by the same config class, of which exactly one is in the build menu, form a family under the listed one (`Families`). Matching uses the materials a building was built from (`Deconstructable.constructionElements`), and the def to build for a choice of materials is resolved by running the mods' own prefixes on `BuildTool.Activate` with those arguments, the swap the build menu goes through, falling back to a finished building of the family with the same materials (`Variants`).
- The Change material pickers are the game's `MaterialSelectionPanel` given one recipe whose single ingredient tag joins every material class the listed types use with `&`, which the game reads as "any of these"; its one selection per row is the plan's one material. The recipe names the Printing Pod, since the game looks up a real building for it (`MaterialSwap.PickerRecipe`). A prefix on `MaterialSelector.OnSelectMaterial` keeps every pick in the planner's pickers, on both tabs, out of the game's memory of the last material picked for a building (saved with the colony, and offered first in the build menu), so the planner changes no build menu default, the Printing Pod's included. A type is listed when the game lets it be built over by itself (`ReplacementRules.CanReplace` with the type as both).
