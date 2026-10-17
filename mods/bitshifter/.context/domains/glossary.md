---
doc_id: doc-2605b1cfcd73
description: "Glossary for bitshifter: what the Bitshifter mod patches, its rules and its terms"
---
# Domain Glossary

Terms used in the Bitshifter mod (`mods/bitshifter`). For the classes and patches behind them, see the entities document.

## Abbreviations and Acronyms
**Id:** blk-c32654e0f564

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game this mod is for |
| kanim | Klei animation | The game's sprite-animation asset (`*_kanim`); the mod ships its own for the reader and writer |
| ONICPU | Digital CPU (staticID `OniSmallCpu`, Workshop 3244925649) | A mod that already widens ribbons to 32 bits; Bitshifter yields to it |
| BAO | Better Automation Overlay (Workshop 1878896484) | Mod whose overlay labels `OverlayLabelGuard` protects |

## Terms to Avoid
**Id:** blk-cf5e7783d48c

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Bit 32 | Bits 1-31 | Bit 32 is the int's sign bit; the game tests bits with `> 0`, so it never reads as on and the mod leaves it out |
| Bit 0 | Bit 1 | The side screen and the display number bits from 1; the stored `selectedBit` is 0-based, so the display shows `selectedBit + 1` |

## Cross-Domain Terms
**Id:** blk-e56acb84a125

### Ribbon
**Id:** blk-21a5cd486094  
**Definition**: An automation wire that carries a 32-bit integer signal; the game exposes the low four bits. A ribbon carries up to 31 independent signals with this mod.  
**Used in**: Reader, Writer, NOT gate mask

### Automation Ribbon Reader
**Id:** blk-e56d7b368a41  
**Definition**: Building (`LogicRibbonReader`) that picks one bit from a ribbon, or shifts the value down by that many bits onto a ribbon output.  
**Used in**: bit depth patch, `UpdateVisuals` patch, side screen

### Ribbon Writer
**Id:** blk-0353e0e524cb  
**Definition**: Building (`LogicRibbonWriter`) that shifts a 1-bit or ribbon input up by the selected bit onto a ribbon.  
**Used in**: bit depth patch, `UpdateVisuals` patch, side screen

### Bit depth
**Id:** blk-317285d31e79  
**Definition**: What `GetBitDepth()` returns: how many bits the selector side screen lists. Vanilla returns 4; the mod returns 31.  
**Used in**: `Patches.cs`, `Mod.cs` (`Bits`)

### Bit selector side screen
**Id:** blk-64dad4f78a5e  
**Definition**: The details-panel screen (`LogicBitSelectorSideScreen`) with one row per bit. The mod puts its rows in a scroll view.  
**Used in**: `SideScreen.cs`

### Selected bit
**Id:** blk-a6c85b35c760  
**Definition**: The building's saved bit choice (`selectedBit`, 0-based internally, shown 1-based). Saved with the building and copied by copy-settings.  
**Used in**: `Display.Update`, tests in `Settings.cs`

### Port lights
**Id:** blk-b5a06fba0850  
**Definition**: The input and output lights on a logic building that show whether a wire is connected and its state; the mod keeps them and tints them with the building's on and off colours.  
**Used in**: `Display`

### Stand down
**Id:** blk-ce9d17157555  
**Definition**: The mod applies no patches (apart from the overlay guard) and logs why, because another mod already handles the widening.  
**Used in**: `Mod.cs` (`WhyYield`)

## Related
**Id:** blk-e9bc1a155ba9

- See also: [entities](entities.md)
