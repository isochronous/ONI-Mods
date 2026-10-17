---
witness_digest: "sha256:5b3ee5d757a17cd91783a48a109e5d7802c20f030dd499f27722647cdbea5893"
witness: loose
applies_to:
  - "src/SmartWeightPlate/**"
doc_id: doc-3b2699889607
description: "Glossary for Smart Weight Plate: abbreviations such as kanim and BAO, terms to avoid, and definitions of the vanilla Weight Plate, reservoir semantics, logic signal, FloorSwitchActivator, Pickupable and settings version"
---

# Smart Weight Plate glossary

Terms for the Smart Weight Plate mod. For the mod's entities, rules and code paths see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-32c544c5e56e

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game the mod is for |
| kanim | Klei animation | The building's art: `src/SmartWeightPlate/anim/assets/smart_weight_plate/` |
| PLib | Peter Han's mod library | Builds the side screen; merged into the DLL by the shared build |
| BAO | Better Automation Overlay | Optional mod whose range label the plate patches (`Mod.cs`); the code calls it `BetterLogicOverlay` |

## Terms to Avoid
**Id:** blk-656af224f2ba

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Activate / deactivate threshold | High / Low Threshold | `IActivationRangeTarget` names the sliders for the reservoirs: `ActivateValue` is the upper slider (high) and `DeactivateValue` the lower (low). The UI and docs use High and Low |
| Pressure plate | Weight plate | The game and the strings call the building a Weight Plate |

## Cross-Domain Terms
**Id:** blk-b3323161da54

### Weight Plate
**Id:** blk-7932dd8bb974  
**Definition**: The vanilla building `FloorSwitchConfig`, a foundation tile that sends a logic signal when something heavy enough sits on the cell above. The mod's plate copies its tile setup and animation set.  
**Used in**: `SmartWeightPlateConfig.cs`, `Patches.cs`, `SmartWeightPlate.cs`.

### Reservoir semantics
**Id:** blk-9930ee1af08a  
**Definition**: The two-threshold latch the Liquid and Gas Reservoirs (and smart batteries) use for their signal: on at the high value, off at the low value, held in between. The mod's `SmartReservoir`-style latch is the same rule applied to mass.  
**Used in**: `SmartWeightPlate.cs`.

### Logic signal (green / red)
**Id:** blk-1b45c5463b81  
**Definition**: The automation port's two states. Green is the active signal (1), red the inactive (0) in the game's wire colours.  
**Used in**: `ModStrings.cs`, `SmartWeightPlate.cs`.

### FloorSwitchActivator
**Id:** blk-1e43d779ad2e  
**Definition**: The game component that makes an object (a Duplicant, a walking critter) press weight plates. The plate adds its `PrimaryElement.Mass` to the weight.  
**Used in**: `SmartWeightPlate.OnActivatorsChanged`.

### Pickupable
**Id:** blk-391d9b7596d6  
**Definition**: A loose item or debris on the floor. Its mass counts; one that `wasAbsorbed` does not.  
**Used in**: `SmartWeightPlate.OnPickupablesChanged`.

### Settings version
**Id:** blk-b624aed199bb  
**Definition**: The saved `settingsVersion` integer. A plate with no version in its save is from before 0.2.0, when green meant light, and has its invert flag flipped once.  
**Used in**: `SmartWeightPlate.OnSpawn`.

## Related
**Id:** blk-1580b13e6f1e

- See also: [entities](entities.md)
