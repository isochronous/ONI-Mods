# Corner Build Anything

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: duplicants can build and deconstruct any building through a diagonal gap, the way the game already lets them build tiles. Bridges of every kind count from either end, not just their middle.

## What it does

Where a duplicant may stand to work on something is a table of reach offsets, and the game keeps two of them. In the standard table a diagonal cell can be reached only by way of one of its two orthogonal neighbours, so a cell whose only opening is a corner is out of reach. In the table "with corners" the diagonal cells are reachable outright. Digging uses the corner table for everything; construction and deconstruction use it for tiles only, and the standard table for every other building. That is why a corner pocket can be dug and tiled but not laddered or piped.

The mod swaps the table in those two places, so every building is set up with the corner table. Nothing else in that setup is touched: rotation, a building's own offset filter (the cells it refuses to be worked from), and the delivery storage that shares the construction table all keep the game's values, so materials arrive through the corner as well.

**Bridges.** A multi-cell building's reach is built from every cell of its footprint, taking the rows of whichever footprint cell is nearest the standing cell. With the standard table, a bridge whose middle sits in the corner pocket works when one of the pocket's orthogonal neighbours is the bridge's own open end cell; a bridge with only an end in the pocket fails, because both of that cell's neighbours are rock. With the corner table the end cell's diagonal row needs no neighbour, so the same swap covers both cases and no bridge-specific patch is needed.

**Placement is unchanged.** What you can place where is exactly as before. The mod only changes who can reach it once placed.

**Deconstruction too.** A building put up through a corner can be taken down through the same corner. Without this, a corner-built building would be stuck until a side was dug open.

## Installing

As a local mod:

1. Download `CornerBuildAnything-<version>.zip` from the [latest release](https://github.com/isochronous/corner-build-anything/releases/latest).
2. Extract it into a new folder named `CornerBuildAnything` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\CornerBuildAnything`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/CornerBuildAnything`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/corner-build-anything.git
dotnet build corner-build-anything/src/CornerBuildAnything -c Release
```

## Notes for modders

- Two transpilers, on `Constructable.OnSpawn` and `Deconstructable.OnSpawn`, replace each load of `OffsetGroups.InvertedStandardTable` with `OffsetGroups.InvertedStandardTableWithCorners`. If a game update stops either method from loading that field, the method is left as is and a warning is logged, rather than the patch failing.
- The corner table is a strict superset of the standard one: every standing cell the standard table reaches, it reaches, by the same or a shorter row of intermediate cells. So the change can only add reachable positions, never remove one.
- The reach-table cache in `OffsetGroups.BuildReachabilityTable` is keyed on the table reference, so the swapped table gets its own cache entries and never collides with a mod that still asks for the standard one.
