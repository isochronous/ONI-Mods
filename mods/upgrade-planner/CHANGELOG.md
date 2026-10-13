# Changelog

## 0.1.0 - 2026-10-13

- First version: an Upgrade Planner tool. Pick what to replace (any material, or specific materials in the game's own material panel) and what to replace it with (type and materials), then box-drag; every matching building in the area gets a replacement order at the current priority. Alternatives come from the game's own replacement rules, so modded pipes appear automatically. A mod's per-material variants behind one build menu entry (Decor Pack's stained glass tiles) show as that one entry.
- The window opens when the tool is picked and closes with Apply; the Upgrade Planner button beside the priority bar opens it again while the tool is active. Clear forgets every choice.
- Power wires and automation wires can be upgraded (to another metal, or a wire to a ribbon). A wire order placed by the planner draws the connections of the wire it replaces, as the game's own does.
- With instant build on (the debug toggle, or sandbox mode with its instant build setting), a drag replaces the buildings on the spot: each order is given its materials and finished at once, the way the game finishes it when a Duplicant does, so wires keep their connections.
