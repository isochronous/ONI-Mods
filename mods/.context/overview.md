---
description: "Overview of the mods/ folder of ONI-Mods: the shared per-mod folder layout (src, tests, publish, README, CHANGELOG) and a one-line list of what each of the 17 mods does"
---

# mods

## What this is
**Id:** blk-86fbb797583e

`mods/` holds the 17 Oxygen Not Included mods of the ONI-Mods monorepo, one folder per mod. Each mod is built, versioned, tested and released on its own (tag `<mod>-v<version>`) and has its own `.context/` node. Each folder follows the same shape: `src/<ModName>/` (the mod) with a `src/<ModName>.Tests/` project, `publish/` (Workshop text and release files), `README.md` and `CHANGELOG.md`. Shared build and tooling come from `common/` at the repo root.

The mods:

- `bitshifter` — the Automation Ribbon Reader and Writer can select any of bits 1-31 instead of 1-4, and show the selected bit on the building.
- `corner-build-anything` — duplicants can build and deconstruct any building through a diagonal gap; bridges count from either end.
- `detect-any-element` — the element sensors get an Any choice that fires on any element, gas, liquid or item.
- `dont-count-eggs-on-rails` — critter sensors no longer count eggs riding a conveyor rail through the room.
- `jumper-bridges` — an Automation Wire or Ribbon Bridge, or a power bridge, can have an end directly on any port cell, which then counts as connected.
- `just-fall-already` — a critter whose footing a door just took away falls at once.
- `motion-sensor-range` — the Duplicant Motion Sensor's range is set by a slider in its side screen (2-16 tiles, default 4).
- `multichannel-critter-sensor` — adds a room-based critter and egg counter that outputs on an Automation Ribbon, with a choice of species and egg types.
- `natural-backwalls-everywhere` — gives every biome the natural backwall tiles the Aquatic Planet Pack places, with the material chosen from the biome's own solids and adjustable coverage.
- `no-magic-fridges` — a powered fridge only ever cools, its contents share heat, and food colder than 1 C is never warmed.
- `refined-building` — every refined metal can be used wherever a building asks for raw Metal Ore.
- `salty-surf` — the Mechanical Surfboard runs on Salt Water as well as Water.
- `smart-weight-plate` — adds a weight plate with a low and a high threshold, so its signal behaves like a reservoir's.
- `storage-pod-redux` — adds the 1x1 Storage Pod and Cool Pod, buildable anywhere including inside rockets; a rewrite of Skyrunner's Storage Pod.
- `sweep-zones` — adds a Sweep Zones tool: painted zones mark debris for sweeping and puddles for mopping at the painted priority.
- `upgrade-planner` — a box-drag tool that marks buildings to be rebuilt in another material or as another type, such as plain pipes to insulated.
- `vent-freeze-fix` — fixes a save/load bug with liquid vents.

## Where things are
**Id:** blk-0ff55ca8296e

- `<mod>/src/<ModName>/` — the mod's code, `mod.yaml`, `mod_info.yaml` and, where it has them, `anim/` and `assets/`.
- `<mod>/src/<ModName>.Tests/` — the mod's in-game tests (every mod has one).
- `<mod>/publish/` — Workshop description, preview and release files.
- `<mod>/README.md`, `<mod>/CHANGELOG.md` — what the mod does and what changed per version.
- `<mod>/tools/` — art or helper scripts, in `bitshifter`, `detect-any-element`, `jumper-bridges`, `natural-backwalls-everywhere` and `smart-weight-plate`.
- `.context/` — this node's [decisions](decisions/); `projects.md` lists the mod nodes.
