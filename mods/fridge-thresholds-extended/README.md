# Fridge Thresholds Extended

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that makes the High and Low threshold sliders from [Storage Refrigerator Thresholds](https://steamcommunity.com/sharedfiles/filedetails/?id=2959340381) drive the automation port of the Mini Fridge from The Aquatic Planet Pack, the [Freezer](https://steamcommunity.com/sharedfiles/filedetails/?id=2618339179), the fridges from [Ronivan's Legacy - Industrial Revolution](https://steamcommunity.com/sharedfiles/filedetails/?id=3557584850), and the Cool Pod from [Storage Pod Redux](https://steamcommunity.com/sharedfiles/filedetails/?id=3812993268).

## What it does

- **Freezer:** the sliders already show on it, and now its port follows them instead of only reporting full. Copy settings carries the thresholds over too.
- **Industrial Revolution fridges** (the small, large, pod and both high-tech fridges): they get the threshold sliders, and their port follows them the way the Refrigerator's does.
- **Mini Fridge** (The Aquatic Planet Pack): it gets the threshold sliders, and its port follows them the way the Refrigerator's does.
- **Storage Pod Redux Cool Pod:** it gets the threshold sliders, and its port follows them the way the Refrigerator's does.

## Compatibility

- Requires Storage Refrigerator Thresholds. Works with any of Freezer, Industrial Revolution and Storage Pod Redux; whichever is missing is left out.
- Build 744825 or later.

## Installing

As a local mod:

1. Download `FridgeThresholdsExtended-<version>.zip` from the [releases](https://github.com/isochronous/ONI-Mods/releases) (tags `fridge-thresholds-extended-v<version>`).
2. Extract it into a new folder named `FridgeThresholdsExtended` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\FridgeThresholdsExtended`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/FridgeThresholdsExtended`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/ONI-Mods.git
dotnet build ONI-Mods/mods/fridge-thresholds-extended/src/FridgeThresholdsExtended -c Release
```

The build does not need any of these mods installed.
