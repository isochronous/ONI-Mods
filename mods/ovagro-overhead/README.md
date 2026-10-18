# Ovagro Overhead

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: Ovagro grows from ceilings. Pips plant Ovagro seeds on natural ceilings, and Duplicants plant them in Farm Tiles and Hydroponic Farms rotated to face down. The vines grow along the ceiling.

## What it does

- Pips plant Ovagro seeds on natural ceilings, as they plant them on floors.
- Duplicants plant Ovagro seeds in a Farm Tile or Hydroponic Farm that is flipped to face down. The plant hangs from the plot and its vines grow along the ceiling.
- Floor planting is unchanged.

Requires the Prehistoric Planet Pack, which adds Ovagro.

## Before removing the mod

Dig up or clear ceiling Ovagro first, both wild ones and those in downward-facing farm tiles. Without the mod the game drops them when the save loads.

## Installing

As a local mod:

1. Download `OvagroOverhead-<version>.zip` from the [releases page](https://github.com/isochronous/ONI-Mods/releases?q=ovagro-overhead).
2. Extract it into a new folder named `OvagroOverhead` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\OvagroOverhead`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/OvagroOverhead`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone https://github.com/isochronous/ONI-Mods.git
dotnet build ONI-Mods/mods/ovagro-overhead/src/OvagroOverhead -c Release
```

The build deploys the mod into the game's local mods folder. See `common/README.md` for the shared build and release tooling.
