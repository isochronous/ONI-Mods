# Bitshifter

An [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mod that lets the **Automation Ribbon Reader** and **Ribbon Writer** select any of bits 1–31 instead of 1–4, and shows the selected bit on the buildings.

## Why

A ribbon's signal is a 32-bit integer; the game only exposes the low four bits. The Reader and Writer already work by shifting the whole value (`value >> bit` and `value << bit`), with no upper bound on the bit, so letting their side screen offer more bits is enough to put up to 31 independent signals on one ribbon. Bit 32 is the sign bit, and the game tests bits with `> 0`, so it would never read as on; it is left out.

## What it does

- The bit selector side screen of both buildings lists Bit 1 … Bit 31. The panel is a fixed height, so the list sits in a scroll view one and a half times the height the four vanilla rows had, with a scrollbar; scroll it with the wheel or the bar.
- A Reader set to a high bit outputs that bit on a 1-bit wire, or the value shifted down by that many bits on a ribbon, exactly as it does for bits 1–4. A Writer shifts its input up by the selected bit.
- Both buildings show the selected bit on an amber two-digit display, with one light beside it for that bit's state (the vanilla art only has lights for bits 1–4). The port lights work as before.
- A NOT gate with a ribbon on its input inverts all 31 bits instead of only bits 1–4. The other gates already work on every bit, except the Buffer and Filter, which output a single bit (on if any input bit is on) in the base game and still do.
- Copy-settings works as before; the selected bit is saved with the building. If the mod is removed, a building set to a bit above 4 keeps working, but the vanilla side screen will not show its selection until you pick a bit again.

## Compatibility

The mod stands down (and says so in the log) when something else already handles this:

- when [Digital CPU](https://steamcommunity.com/sharedfiles/filedetails/?id=3244925649) (ONICPU) is enabled, since it already expands the ribbons, Reader, and Writer to 32 bits;
- when any other mod has already Harmony-patched `GetBitDepth` or `OnSpawn` on `LogicRibbonReader` or `LogicRibbonWriter` by the time all mods are loaded (Digital CPU, for instance, sets the bit depth from an `OnSpawn` postfix).

So whichever mod widens the reader and writer, this one yields to it rather than doubling up.

## Installing

As a local mod:

1. Download `Bitshifter-<version>.zip` from the [latest release](https://github.com/isochronous/bitshifter/releases/latest).
2. Extract it into a new folder named `Bitshifter` inside the game's local mods folder, so that `mod.yaml` ends up directly inside it (create `local` if it does not exist):
   - Windows: `Documents\Klei\OxygenNotIncluded\mods\local\Bitshifter`
   - Linux: `~/.config/unity3d/Klei/Oxygen Not Included/mods/local/Bitshifter`
   - macOS: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/local/Bitshifter`
3. Start the game, enable the mod under **Mods** in the main menu, and let the game restart.

## Building

Requires the .NET SDK (8+). Shared build configuration lives in the [oni-mods-common](https://github.com/isochronous/oni-mods-common) submodule, so clone with `--recurse-submodules` (or run `git submodule update --init`). The game DLLs are referenced directly from the game install; override the path if yours differs:

```
dotnet build src/Bitshifter -c Release -p:GameFolder="<path-to>\OxygenNotIncluded"
```

A successful build deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\Bitshifter` (disable with `-p:ModDeployFolder=none`).

## Art

The display is drawn by `tools/make_art.py`, which starts from the game's own Ribbon Reader and Writer kanims and writes `src/Bitshifter/anim/assets/bitshifter_ribbon_reader` and `..._writer`. It needs Python with the packages in `common/tools/requirements.txt` and the game installed; `--preview` renders both buildings and opens the image, and `--check` fails when the committed kanims differ from a fresh build (after a game update, say).

```
python tools/make_art.py [--game "<path-to>\OxygenNotIncluded"] [--preview] [--check]
```

## Implementation notes

Eight Harmony patches, applied in `OnAllModsLoaded` (after every other mod has loaded, so the compatibility check sees their patches): `GetBitDepth` on `LogicRibbonReader` and `LogicRibbonWriter` returns 31; `CreateBuildingDef` on both configs swaps in the mod's kanims; `UpdateVisuals` on both is replaced by one that plays the pose for the wired ports (the vanilla `<ports>_<bit>` poses without the bit lights), shows the selected bit's segments and tints its light with the building's own on and off colours; and `LogicBitSelectorSideScreen.SetTarget` moves the rows' container into the game's own `KScrollRect` (with `RectMask2D` and a `ContentSizeFitter`) the first time the screen is shown, 1.5x the row list's original height, with a scrollbar cloned from the first vanilla side screen that has one (a flat-colour bar is the fallback). The same patch clamps the screen's layout widths to the details panel's header (the vanilla screen asks for 316 px against a 280 px header, which would leave a blank strip beside the Options title). A transpiler on `LogicGate.UpdateState` widens the NOT gate's four-bit mask (`& 0xF`, the method's only one; left alone with a warning if the game ever has more or none) to bits 1–31. Whenever Better Automation Overlay is enabled, and even when the rest of the mod stands down, every `GetSetting` on its overlay labels gets a prefix: a label that has not spawned yet reads blank instead of throwing. Its fields are filled in `OnSpawn`, and Blueprints Expanded shows a building in the overlay before then. No PLib, no options, no strings.
