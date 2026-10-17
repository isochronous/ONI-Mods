---
doc_id: doc-01b787080f8e
description: "Entities, business rules and key code paths of bitshifter: Ribbon Reader and Writer bit selection, display and NOT gate patches"
---
# Bitshifter domain

## Overview
**Id:** blk-88f758ea7235

Bitshifter lets the Automation Ribbon Reader and Ribbon Writer select any of bits 1 to 31 instead of 1 to 4, and draws the selected bit on the buildings. It adds no buildings of its own: every entity below is a vanilla game class the mod patches, or a mod class that does the patching. Source is in `mods/bitshifter/src/Bitshifter/`.

## Key Entities
**Id:** blk-0fc6620ccba9

### BitshifterMod
**Id:** blk-976bcafad5d2  
**Description**: The `UserMod2` entry point. Applies the patches in `OnAllModsLoaded` (not `OnLoad`) so the compatibility check sees every other mod's patches, and stands down when another mod already widens the reader or writer.  
**Location**: `mods/bitshifter/src/Bitshifter/Mod.cs`

| Member | Type | Description |
|--------|------|-------------|
| Bits | const int (31) | Highest selectable bit. Bit 32 is the int's sign bit and the game tests bits with `> 0`, so it never reads as on |
| YieldToWorkshopIds / YieldToStaticIds | string[] | Digital CPU: Workshop 3244925649, staticID `OniSmallCpu` |
| Contested | MethodBase[] | `GetBitDepth` and `OnSpawn` on `LogicRibbonReader` and `LogicRibbonWriter`; an existing Harmony owner on any of them makes the mod stand down |

**Relationships**: calls `OverlayLabelGuard.Apply` first (always), then `WhyYield`; if that returns a reason it logs "Disabled: ..." and returns, otherwise `harmony.PatchAll` over the mod assembly.

### Reader and Writer bit depth
**Id:** blk-d5c2d70c3f11  
**Description**: `LogicRibbonReader.GetBitDepth` and `LogicRibbonWriter.GetBitDepth` are postfixed to return `Bits`. The bit selector side screen builds one row per `GetBitDepth()`, and both buildings already shift by the selected bit with no upper bound, so this is the whole functional change.  
**Location**: `mods/bitshifter/src/Bitshifter/Patches.cs` (`LogicRibbonReader_GetBitDepth_Patch`, `LogicRibbonWriter_GetBitDepth_Patch`)

**Relationships**: the selected bit is the game's own saved `selectedBit` field, so it is saved with the building and copied by copy-settings with no mod code.

### Display
**Id:** blk-cd081718a091  
**Description**: Shared draw code for both buildings. Replaces the vanilla four bit lights with a two-digit seven-segment display of the selected bit (1-based) and one light for that bit's state.  
**Location**: `mods/bitshifter/src/Bitshifter/Patches.cs` (`Display`)

| Member | Type | Description |
|--------|------|-------------|
| ReaderAnim / WriterAnim | string | `bitshifter_ribbon_reader_kanim`, `bitshifter_ribbon_writer_kanim`, built by `tools/make_art.py` into `src/Bitshifter/anim/assets` |
| Signal, OutputLight, InputLight | KAnimHashedString | Symbols `signal_bloom`, `output_light_bloom`, `input_light_bloom` tinted with the building's on and off colours |
| Segments | KAnimHashedString[2,7] | Symbols `seg_a_1` to `seg_g_2`, shown or hidden per digit |
| WriterUpsideDown / ReaderUpsideDown | Orientation | R270 for the writer, R90 for the reader; at those orientations the digits trade places and each segment trades with its opposite |

**Relationships**: `LogicRibbonReaderConfig` and `LogicRibbonWriterConfig` `CreateBuildingDef` postfixes swap in the mod's kanims through `Display.UseAnim` (the vanilla art is kept with a warning if the kanim is missing). `LogicRibbonReader.UpdateVisuals` and `LogicRibbonWriter.UpdateVisuals` are replaced by prefixes that return false and call `Display.Update`.

### LogicBitSelectorSideScreen scroll view
**Id:** blk-3546b9b4bcd4  
**Description**: A postfix on `LogicBitSelectorSideScreen.SetTarget` wraps the rows' container in a `KScrollRect` (with `RectMask2D`, `LayoutElement` and `ContentSizeFitter`) the first time the screen is shown. The viewport is 1.5 times the height of the vanilla four rows, with a permanent scrollbar cloned from a vanilla side screen (a flat-colour bar is the fallback). The same postfix clamps layout widths to the details panel header (280 px against the vanilla 316 px ask).  
**Location**: `mods/bitshifter/src/Bitshifter/SideScreen.cs`

**Relationships**: `IsWrapped` checks the row list's direct parent so reopening the screen does not nest a second scroll view.

### NOT gate mask
**Id:** blk-5f2b3567543d  
**Description**: A transpiler on `LogicGate.UpdateState` replaces the single `& 15` (four-bit mask) with `Mask` = `(1 << 31) - 1`, so a NOT gate with a ribbon input inverts all 31 bits. If the method has no such mask or more than one, it is left alone and a warning is logged.  
**Location**: `mods/bitshifter/src/Bitshifter/Patches.cs` (`LogicGate_UpdateState_Patch`)

### OverlayLabelGuard
**Id:** blk-4919a4967bb1  
**Description**: When Better Automation Overlay (Workshop 1878896484) is enabled, prefixes every `GetSetting` on its `LogicLabelSetting` labels so a label that has not spawned yet returns blank text instead of throwing a NullReferenceException every frame. Applies even when the rest of the mod stands down.  
**Location**: `mods/bitshifter/src/Bitshifter/OverlayLabelGuard.cs`

## Business Rules
**Id:** blk-de8c66751b82

- The reader and writer offer bits 1 to 31 and no more.
- A reader set to a high bit outputs that bit on a 1-bit wire, or the value shifted down by that many bits on a ribbon; a writer shifts its input up by the selected bit. This is the game's own shifting; the mod changes no arithmetic.
- The NOT gate inverts 31 bits. Other gates already work on every bit; Buffer and Filter output a single bit, as in the base game.
- The mod stands down and logs the reason when Digital CPU is enabled, or when any other mod has Harmony-patched `GetBitDepth` or `OnSpawn` on the reader or writer by the time all mods have loaded.
- Removing the mod leaves a building set above bit 4 working; the vanilla side screen shows no selection for it until a bit is picked again.
- No PLib, no options, no strings.

## Key Code Paths
**Id:** blk-2fe104dbf053

- **Startup and yield check**: `OnAllModsLoaded` runs the guard, `WhyYield`, then `PatchAll` — see `mods/bitshifter/src/Bitshifter/Mod.cs`
- **Drawing the selected bit**: `UpdateVisuals` prefix, `Display.Update`, `SetDigit` — see `mods/bitshifter/src/Bitshifter/Patches.cs`
- **Scrolling bit list**: `SetTarget` postfix, `Wrap`, `FitUnderHeader` — see `mods/bitshifter/src/Bitshifter/SideScreen.cs`
- **Overlay crash guard**: `OverlayLabelGuard.Apply` — see `mods/bitshifter/src/Bitshifter/OverlayLabelGuard.cs`
- **Art build**: `tools/make_art.py` writes the two kanims from the game's own reader and writer kanims; `--check` fails when committed kanims differ from a fresh build
- **Tests**: in-game tests in `mods/bitshifter/src/Bitshifter.Tests/` (`Compatibility.cs`, `Gates.cs`, `Readout.cs`, `Settings.cs`, `SideScreen.cs`, `Signals.cs`)

## Related
**Id:** blk-d6aecc1ac010

- See also: [glossary](glossary.md)
