# Refined Building

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that lets every refined metal be used wherever a building asks for raw **Metal Ore**: wire, pipes, doors, tiles, ladders, batteries and so on.

It does the same job as Cairath's [Refined Metals Usable As Raw Metals](https://steamcommunity.com/sharedfiles/filedetails/?id=1729816134), but instead of a fixed list of six metals it finds the refined metals at runtime, so it covers everything the game actually loaded: the base game, whichever DLCs are enabled, and refined metals added by other mods.

## What it does

After the game loads its element table, every solid element tagged `RefinedMetal` that does not already carry the `Metal` tag gets it. Building recipes that accept "Metal Ore" (`MATERIALS.RAW_METALS`) match on that tag, so the refined metals appear in those buildings' material lists alongside the ores. The refined-metal recipes are untouched.

On the current game with all DLCs that adds Copper, Iron, Gold, Lead, Aluminum, Tungsten, Cobalt, Nickel, Zinc, Solid Mercury and Depleted Uranium; Steel, Niobium, Thermium and Iridium already have both tags in vanilla and are left alone. The list is logged at startup (`[RefinedBuilding] Tagged N refined metal(s) ...`).

Discovery works as it always does: a metal shows up in the build menu once it has been discovered and is available, exactly like the ores.

Consequences worth knowing, all of which vanilla Steel already has:

- Refined metal debris now satisfies deliveries that ask for any "Metal", chiefly the Oxygen Mask Station. Storage bins are unaffected, since their filters key on the material category, and the refined metals stay under **Refined Metal** there.
- The Metal Refinery and Rock Crusher build their recipe lists from the same tag but skip anything that does not have an ore-to-metal transition, so no new recipes appear.

## Installing

As a local mod:

1. Download `RefinedBuilding-<version>.zip` from the [latest release](https://github.com/isochronous/refined-building/releases/latest).
2. Extract it into a new folder named `RefinedBuilding` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\RefinedBuilding`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/RefinedBuilding`
   - macOS: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/local/RefinedBuilding`
3. Start the game, enable the mod under **Mods** in the main menu, and let the game restart.

Safe to add to or remove from an existing save: it only changes which materials the build menu offers.

## Building

Requires the .NET SDK (8+). Shared build configuration lives in the [oni-mods-common](https://github.com/isochronous/oni-mods-common) submodule, so clone with `--recurse-submodules` (or run `git submodule update --init`). The game DLLs are referenced directly from the game install; override the path if yours differs:

```
dotnet build src/RefinedBuilding -c Release -p:GameFolder="<path-to>\OxygenNotIncluded"
```

A successful build deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\RefinedBuilding` (disable with `-p:ModDeployFolder=none`).

## Implementation notes

A single Harmony postfix on `ElementLoader.Load`. `ElementLoader` only creates elements whose `dlcId` is enabled, and folds `materialCategory: RefinedMetal` into `oreTags`, so `element.IsSolid && element.HasTag(GameTags.RefinedMetal)` is the whole filter. No PLib, no configuration.
