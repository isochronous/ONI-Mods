# Don't Count Eggs on Rails

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod: critter sensors no longer count eggs that are riding a conveyor rail through the room.

## The problem

A critter sensor counts the eggs the game lists for its room, and an egg lists itself in whatever room its position falls in, once a second. An egg on a conveyor rail moves through rooms like anything else on the rail, so while its packet crosses open cells of a room, every critter sensor in that room counts it, and automation built on the count misfires.

## What the mod does

A rail marks the item it carries as stored without giving it a storage, which nothing else does: an egg in an incubator or carried by a duplicant has a real storage behind it, and an egg on the floor is not stored at all. While an egg is in that state it is kept out of any room's list, and taken out of the one it was in. Once it leaves the rail, the game's own update lists it again within a second. Eggs on the floor, in incubators, in storage, or in a duplicant's hands count exactly as before.

One patch on the egg's room tracking; sensors, rails, and the room prober are not touched.

## Installing

As a local mod:

1. Download `DontCountEggsOnRails-<version>.zip` from the [latest release](https://github.com/isochronous/dont-count-eggs-on-rails/releases/latest).
2. Extract it into a new folder named `DontCountEggsOnRails` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\DontCountEggsOnRails`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/DontCountEggsOnRails`
3. Enable it in the game's Mods menu and restart.

## Building

```
git clone --recurse-submodules https://github.com/isochronous/dont-count-eggs-on-rails.git
dotnet build dont-count-eggs-on-rails/src/DontCountEggsOnRails -c Release
```
