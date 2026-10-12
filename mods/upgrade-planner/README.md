# Upgrade Planner

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: a tool that marks every building of one type in a box-dragged area to be replaced with another type and material. Plain pipes to insulated, sandstone to steel, insulated back to plain, or any modded pipe the game lets you build over the old one.

Status: **first version, in use**.

## What it does

Dragging a pipe over an existing pipe in the build tool is a *replacement*: the old pipe keeps flowing until the new one is built in its place. The game decides which replacements are legal from the building definitions (a replacement layer, a set of replacement tags, and the layer it looks for candidates on), and it allows a same-type swap as long as the material changes. The mod reads those same rules and queues the same replacement order the build tool would, once per matching building in a box, with each building's own orientation.

**Using it.** Select the Upgrade Planner from the toolbar (or its hotkey, Shift+U by default, rebindable under Options, Controls, Mods). A window opens with two columns, each scrolling on its own:

- **Replace**: every replaceable building type as the build menu lists it, grouped by family (Liquid Pipes, Ventilation Pipes, Conveyor Rails, Wires, Automation Wires, Tiles, Ladders, and so on, including families and types added by mods). Pick one, then **Any** material, or **Specific** and pick the materials in the game's own material panel, one row per recipe ingredient; only buildings built from exactly those materials are touched.
- **With**: the types the game lets you build over the chosen one, filtered to what you have researched, then its materials in the same game panel (a two-ingredient recipe such as a modded pressurized pipe or a stained glass tile shows two rows). The material in stock is picked for you to start with, as in the build menu.

While the tool is active, every building the plan would touch is highlighted in the world (blue; green once a replacement is already pending on it), and the overlay switches to the family's view (pipes, vents, power, automation) the way the build tool does. Box-drag over the pipes you want: each matching building gets a replacement order at the current tool priority, and a popup says how many were marked and how many were skipped (already pending, or already that type in that material). Dragging before both columns are filled in shows an error instead.

**Apply** closes the window and keeps the tool active for dragging; an **Upgrade Planner Config** button next to the priority picker opens it again (it shows as pressed, and does nothing, while the window is open). Closing the window with its X or Escape ends the tool. The plan is remembered for the session, so selecting the tool again resumes where you left off.

**What it never does.** It does not change what can replace what, and it never deconstructs. A pair the game refuses in the build tool is refused here too. Cancel pending replacements with the Cancel tool, as with any build order.

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

- No game logic is patched. The three Harmony patches register strings and the icon (`Db.Initialize`), add the tool to the player's tools (`PlayerController.OnPrefabInit`), and add the toolbar button (`ToolMenu.CreateBasicTools`).
- Queueing one replacement is the build tool's own sequence: `GetReplacementCandidate`, `IsReplacementLayerOccupied` over the footprint, `CanReplace`, the same-type-same-material skip, then `TryReplaceTile` and the write into the replacement layer, with the priority from the tool menu. See `UpgradeTool.TryQueue`.
- Alternatives for a type are computed in `ReplacementRules.CanReplaceDef`: the target names a replacement layer, the source prefab carries one of the target's replacement tags, and the source's tile or object layer is one the target searches for candidates. A mod whose pipes replace vanilla pipes in the build tool satisfies all three, so it appears without being named.
- The window is a PLib `PDialog`, a plain (non-modal) screen rebuilt when a type or the any/specific choice changes so button highlights follow the plan; materials are picked in the game's `MaterialSelectionPanel`, instantiated from the build menu's prefab into the window (`MaterialPanel.cs`), with its priority strip hidden and its rows widened. Closing the window deselects the tool.
- A mod that makes one def per material and lists one of them in the build menu (Decor Pack's stained glass tiles, one per dye, all from one config class) shows as one type: defs made by the same config class, of which exactly one is in the build menu, form a family under the listed one (`ReplacementRules.Family`). Matching uses the materials a building was built from (`Deconstructable.constructionElements`), and the def to build for a choice of materials is resolved by running the mods' own prefixes on `BuildTool.Activate` with those arguments, the swap the build menu goes through, falling back to a finished building of the family with the same materials.
