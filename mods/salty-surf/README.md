# Salty Surf

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: the Mechanical Surfboard runs on Salt Water as well as Water.

## What the mod does

- The surfboard's intake accepts Water or Salt Water. Either fills the tank without damaging the building, and the board is ready once the tank holds the usual 2 kg of either.
- While a Duplicant surfs, the board spills whatever it was fed, at the usual rate, so a Salt Water board leaves Salt Water on the floor.
- Every other liquid is treated exactly as in the base game: it damages the building and is dumped.
- The building's info panel lists both accepted liquids instead of Water alone.

## How it works

The vanilla surfboard keys three things to the Water element: the pipe intake's accepted element, the readiness check on the tank, and the spill while surfing. The mod gives Water and Salt Water a shared tag when the element table loads, which the game copies onto every stored chunk of those elements, and points the three places at the tag. The intake and the readiness check use the game's own tag-based lookups; the spill draws from the tank by tag and drops the element it actually drew.

## Installing

As a local mod:

1. Download `SaltySurf-<version>.zip` from the [latest release](https://github.com/isochronous/salty-surf/releases/latest).
2. Extract it into a new folder named `SaltySurf` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\SaltySurf`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/SaltySurf`
3. Enable it in the game's Mods menu and restart.

## Translations

The mod is available in Chinese (Simplified). Other languages can be added from the template `src/SaltySurf/translations/SaltySurf.pot`: save the translated file as `translations/<locale>.po` next to it.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/salty-surf.git
dotnet build salty-surf/src/SaltySurf -c Release
```
