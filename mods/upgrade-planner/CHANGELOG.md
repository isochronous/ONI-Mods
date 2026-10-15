# Changelog

## Unreleased

### Changed

- Closing the window with its X or Escape keeps the tool, as Apply does, so you can open it just to check your settings. A second Escape ends the tool, as does its toolbar button.
- Apply sets the plan your drags use, and it stays until you press Apply again or a tab's Clear: changes in the window take effect at the next Apply. While the window is open, the highlight shows the plan you are editing; once it is closed, the applied one.
- Until a plan is applied, the cursor shows a red "no" sign, the hover card says to apply your settings first, and a drag says so and opens the window.
- New cursor: the game's yellow tool arrow with the planner's green plate. The marker over the cells is a yellow dashed square instead of Sweep's dolly.
- The tool icon no longer has a plus in the middle.

## 0.3.3 - 2026-10-15

### Fixed

- A building rebuilt by Duplicants in its new material now and then came back with none of its settings (Blueprints Expanded handed them over at a moment the new building was not always there yet). The planner now hands them over itself.
- Replace type no longer orders a Heavi-Watt joint plate where the game's own build tool would refuse one: with its wire ends inside tiles, on a door or on another plate's ends.

## 0.3.2 - 2026-10-14

- New mod preview image.

## 0.3.1 - 2026-10-14

### Changed

- A large drag no longer freezes the game for a moment when the button is released: the buildings are marked (or, with instant build, replaced) a few at a time over the next frames, and the popup counting them comes when the last is done.

### Fixed

- Picking a material in the planner's window no longer changes the material the build menu offers first for a building. Before, the Change material tab's picks changed it for the Printing Pod, which some mods let you rebuild.

## 0.3.0 - 2026-10-14

### Added

- Chinese, Korean and Russian translations.
- With Suppress Notifications or No Manual Delivery installed, an upgraded building keeps their settings too: its suppressed notifications, status items and damage bar, and its hold on deliveries.

### Changed

- Dragging over a large area, the highlight of what the plan touches, and opening the Replace type tab are faster in a big colony.

### Fixed

- With instant build, every building a drag rebuilds is finished at once; some (storage bins, airlocks, logic gates) were left as orders.
- A wide tile rebuilt in another material (a 3-wide farm tile) is rebuilt instead of lost with nothing ordered in its place.
- With instant build, a carpeted tile built over in another material is finished with no error from the game.
- All buildings no longer offers the mini-pod, which the game will not take down to rebuild.

## 0.2.0 - 2026-10-13

- A Change material tab, now the first: pick the material to replace (any, or one) and the one to build in, in the game's own material picker, and which building types to rebuild, as build menu toggle buttons grouped by family with select-all, fold and expand/collapse-all. A drag rebuilds every picked building in the new material. Types the materials rule out are greyed, with the reason in their tooltip; the footer lists the first four types and counts the rest.
- Requires Blueprints Expanded. Upgraded buildings keep their settings: a replacement order carries them to the new building, saved with the colony.
- All buildings: a checkbox in the Buildings header also offers the buildings the game does not let you build over, grouped as the build menu groups them. They are deconstructed and rebuilt in the new material through the game's own reconstruct, keeping their settings. The select-all checkbox moved to the left of the Buildings header.
- Presets, one list per tab: save the current tab's plan under a name, and load or delete that tab's saved ones. They are kept in the game's mod config folder.
- Buildings made of two materials (refined metal and plastic, say) change only the one you pick, either of the two; the other keeps the material the building was made of, so a copper and plastium germ sensor changed to steel stays plastium.
- The window can be moved by dragging its title bar, and stays on screen.
- A new look, in the colours of the game's own management screens: each column is one card under its header, with the picker and building rows on white. The Replace type tab's material pickers no longer show the game's header bar.

## 0.1.0 - 2026-10-13

- First version: an Upgrade Planner tool. Pick what to replace (any material, or specific materials in the game's own material panel) and what to replace it with (type and materials), then box-drag; every matching building in the area gets a replacement order at the current priority. Alternatives come from the game's own replacement rules, so modded pipes appear automatically. A mod's per-material variants behind one build menu entry (Decor Pack's stained glass tiles) show as that one entry.
- The window opens when the tool is picked and closes with Apply; the Upgrade Planner button beside the priority bar opens it again while the tool is active. Clear forgets every choice.
- Power wires and automation wires can be upgraded (to another metal, or a wire to a ribbon). A wire order placed by the planner draws the connections of the wire it replaces, as the game's own does.
- With instant build on (the debug toggle, or sandbox mode with its instant build setting), a drag replaces the buildings on the spot: each order is given its materials and finished at once, the way the game finishes it when a Duplicant does, so wires keep their connections.
