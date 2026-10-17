---
doc_id: doc-dfa34259ff9b
description: The FridgeThermostat component, Harmony patches and mod entry of No Magic Fridges in src/NoMagicFridges, with the heat-exchange, compressor and insulation rules, file locations, code paths and tests
applies_to:
  - "src/NoMagicFridges/**"
---

# No Magic Fridges domain

## Overview
**Id:** blk-341e7f0208d5

No Magic Fridges changes what a powered fridge does to its contents. Vanilla attaches a simulated 1 degree C heat reservoir (400 kJ/K, 1000 W/K) to every stored item, which cools warm food but also drags deep-frozen food up to 1 degree C. The mod switches that reservoir off for fridge contents and runs its own model once a second: the fridge is an insulated box with a thermostat that only ever cools. Unpowered fridges, and items outside a fridge, are vanilla. The mod adds no building, no options and no saved state.

## Key Entities
**Id:** blk-0d6d8f812e66

### FridgeThermostat
**Id:** blk-eb78596783d5  
**Description**: The component that runs the thermal model on a fridge building. It samples every stored item each second, moves each toward one shared interior temperature, and insulates cold items from a warmer room.  
**Location**: `src/NoMagicFridges/FridgeThermostat.cs`

| Property | Type | Description |
|----------|------|-------------|
| InsulationFactor | const float 0.01 | Heat-exchange scale for an insulated item, the factor an Insulated Tile uses |
| BaseHeatCapacity | const float 25 x 3.47 | The compressor's thermal mass in kDTU/K, about 25 kg of food; larger cools a full warm fridge faster |
| InsulateBand / ReleaseBand | const float 0.25 | Kelvin hysteresis before an item is insulated or released |
| MinimumStep | const float 0.0005 | Smaller temperature changes are not sent to the sim |
| MinimumTemperature | const float 1 | Never drive an item below this many kelvin |
| items | Dictionary<int, Tracked> | Per item (by instance id): whether it is insulated and its original `SurfaceArea` and `GroundTransferScale` |
| def | `RefrigeratorController.Def` | Source of the setpoint (`simulatedInternalTemperature`) and conductivity (`simulatedThermalConductivity`) |

**Relationships**: Implements `ISim1000ms`. Uses the building's `Storage` and `Operational` (`[MyCmpGet]`), each item's `PrimaryElement` and `SimTemperatureTransfer`, and subscribes to `GameHashes.OnStorageChange`. Calls the protected `SimTemperatureTransfer.SimRegister` / `SimUnregister` through cached delegates.

### Harmony patches
**Id:** blk-6075d509d0c8  
**Location**: `src/NoMagicFridges/Patches.cs`

| Patch target | Kind | Purpose |
|--------------|------|---------|
| `SimulatedTemperatureAdjuster.OnItemSimRegistered` | Prefix | For an active adjuster whose storage has a `FridgeThermostat`, sends `ModifyElementChunkTemperatureAdjuster(handle, 0, 0, 0)` and skips vanilla, so the reservoir never attaches; any other user of the adjuster runs vanilla |
| `GeneratedBuildings.LoadGeneratedBuildings` | Postfix | Adds `FridgeThermostat` to every building definition whose complete prefab has a `RefrigeratorController.Def` (Refrigerator, Mini Fridge, modded fridges) and logs the count |

The prefix reads the adjuster's private `active` and `storage` fields by reflection and falls back to vanilla if either is missing.

### Mod entry
**Id:** blk-e480792c47bc  
**Location**: `src/NoMagicFridges/Mod.cs`
`NoMagicFridgesMod` (a `UserMod2`) only logs the version. Harmony only, no PLib, no strings. Mod id `Isochronous.NoMagicFridges` (`mod.yaml`). Nothing is saved with the game; the mod can be added to or removed from an existing save.

## Business Rules
**Id:** blk-4b1cf2b79fa1

- Everything applies only while the fridge is powered (`Operational.IsActive`). When it is not, insulation is restored on every item and nothing equalises.
- Contents share heat in both directions: each item closes the fraction `1 - exp(-k*dt/C)` of its gap to the interior temperature, with `k` the def's conductivity read as W/K (1 kDTU/s/K at 1000) and `C` = mass x specific heat. Heat is conserved.
- The compressor only cools. While the contents' mean is above the setpoint (1 degree C), interior = (BaseHeatCapacity x setpoint + sum of C x T) / (BaseHeatCapacity + sum of C), which lies between the setpoint and the mean. At or below the setpoint the compressor is off and the interior is the coupling-weighted mean, so nothing pulls food up.
- A single warm item cools about as fast as in vanilla; a fridge crammed with warm food cools at roughly a fifth of the vanilla rate.
- An item colder than its room by more than 0.25 K is insulated: its `SurfaceArea` and `GroundTransferScale` are scaled by 0.01 and its sim chunk re-registered. It is released once the room is 0.25 K colder than the item. An item warmer than the room keeps exchanging with it, so a fridge in a cold room still deep-freezes food.
- Insulation is undone when the fridge loses power, the item leaves storage, or the fridge is removed.
- Power draw, the cooling and energy-saver states, storage, the full signal and spoilage rules are untouched.

## Key Code Paths
**Id:** blk-0872d6109a15

- **Per-second step**: `Sim1000ms` samples items, decides insulation, then calls `Exchange` — see `src/NoMagicFridges/FridgeThermostat.cs`
- **Heat exchange**: `Exchange(setpoint)` picks compressor-on or compressor-off interior temperature and writes `PrimaryElement.Temperature` — see `src/NoMagicFridges/FridgeThermostat.cs`
- **Insulation**: `Insulate`, `RestoreInsulation`, `Reregister` — see `src/NoMagicFridges/FridgeThermostat.cs`
- **Reservoir suppression**: the `OnItemSimRegistered` prefix — see `src/NoMagicFridges/Patches.cs`
- **Item leaves the fridge**: `OnStorageChange` restores its exchange immediately — see `src/NoMagicFridges/FridgeThermostat.cs`
- **Tests**: in-game `[OniTest]` cases in `src/NoMagicFridges.Tests/Fridges.cs` (thermostat on every fridge and nothing else, frozen food not warmed, shared temperature with heat conserved, Mini Fridge setpoint, crammed fridge rate, unpowered fridge is vanilla, insulation and release, a non-fridge keeps the vanilla reservoir)

## Related
**Id:** blk-8be45c9d80c9

- See also: [glossary](glossary.md)
