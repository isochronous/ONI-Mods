---
doc_id: doc-46d7dbd5e039
description: "Entities of Vent Freeze Fix: the vent storage insulation, the repair watcher and the dig answer, with file locations, Harmony patch targets, business rules and key code paths."
applies_to:
  - "src/VentFreezeFix/**"
---
# Vent Freeze Fix domain

## Overview
**Id:** blk-209beae2da1d

Vent Freeze Fix repairs a save/load bug with liquid vents. Super-cooled liquid in flight when a save is written comes back from the load as a natural tile, either inside the vent (entombing it) or in the cell under it (absorbing the vent's debris pile). The mod stops the first and undoes the second. It adds no buildings. The mod is `mods/vent-freeze-fix`, static ID `Isochronous.VentFreezeFix`, source in `src/VentFreezeFix/`.

## Key Entities
**Id:** blk-dc00673e5995

### Insulated vent storage (patch on LiquidVentConfig)
**Id:** blk-e2a2326c0ab7  
**Description**: Gives each liquid vent's `Storage` the game's insulated storage modifiers (hide, seal, insulate), so the packet a vent is about to emit is skipped by the phase-transition handler and not temperature simulated. This prevents the tile inside the vent.  
**Location**: `LiquidVentConfig_Storage_Patch` in `src/VentFreezeFix/Mod.cs`. Active when `Options.InsulateVents` is on; needs a restart.

### VentFreezeWatcher (component)
**Id:** blk-98e7ce8731de  
**Description**: Added to every liquid vent. For the first second of running time after it spawns (five 200 ms ticks) it watches the vent cell and the landing cell, remembers what each held the previous tick, and asks the sim to dig a tile that could not have formed in normal play. Then it removes itself from the scheduler. Implements `ISim200ms`.  
**Location**: `src/VentFreezeFix/VentFreezeWatcher.cs`

| Property | Type | Description |
|----------|------|-------------|
| ventCell | int | The vent's cell, set in `OnSpawn` (`VentCell`) |
| ticks | int | Ticks run; the watcher stops after `ActiveTicks` = 5 |
| previous | Dictionary<int, Snapshot> | Element and mass each watched cell held the tick before |
| savedVentCell, savedVentElement, savedVentMass | saved | Vent-cell snapshot taken in `[OnSerializing]`, restored in `[OnDeserialized]` |
| savedLandingCell, savedLandingElement, savedLandingMass | saved | The same for the landing cell |
| RepairEnabled | static bool | The "Repair tiles after loading" option, set once per game |
| TileMassShare | const float | 0.8, the game's tile rule |

**Relationships**: reads `Grid.Element`, `Grid.Mass`, `Grid.Solid`, `Grid.Foundation`; files dig requests in `DigRequests`; the sim answers through the `WorldDamage.OnDigComplete` patch.

### DigRequests
**Id:** blk-5905b0e9316d  
**Description**: Static map of cells the watchers asked the sim to dig, and the patch that answers the sim's dig report for those cells by spawning the full mass as debris (a dug tile normally drops half). A request the sim ignores yields no report, so asking again is safe. Cleared on `Game.OnSpawn`.  
**Location**: `src/VentFreezeFix/DigRequests.cs`

### Options
**Id:** blk-63303acdea21  
**Description**: PLib options, shared config location.  
**Location**: `src/VentFreezeFix/Options.cs`

| Property | Type | Description |
|----------|------|-------------|
| InsulateVents | bool | Default true, restart required |
| RepairOnLoad | bool | Default true, takes effect on the next load |
| DebugMode | bool | Default false, category Developer; verbose logging |
| ReproButton | bool | Default false, category Developer; adds the repro-save button |

### DebugSave
**Id:** blk-7391df44fb7c  
**Description**: Debug tooling. Reads the options once per game, logs vent emits in debug mode, and with the repro button arms a save that is written at the end of the frame in which a liquid vent next emits a packet, as `VentFreezeRepro.sav` next to the current save. That moment is the only one in which the bug's conditions can be captured.  
**Location**: `src/VentFreezeFix/DebugSave.cs`

### Harmony patches
**Id:** blk-138d9e285952  
**Location**: `src/VentFreezeFix/Mod.cs`, `src/VentFreezeFix/DigRequests.cs`, `src/VentFreezeFix/DebugSave.cs`

| Target | Kind | Purpose |
|--------|------|---------|
| `LiquidVentConfig.ConfigureBuildingTemplate` | Postfix | Sets the insulated storage modifiers |
| `LiquidVentConfig.DoPostConfigureComplete` | Postfix | Adds `VentFreezeWatcher` |
| `WorldDamage.OnDigComplete` | Prefix | Answers requested digs with full-mass debris; skips the original |
| `Game.OnSpawn` | Postfix | Two patches: clears `DigRequests`; reads the options and adds the repro button (in `DebugSave`) |
| `Exhaust.EmitCommon` | Prefix and Postfix | Debug: logs the packet and, if the repro button is armed, queues the save |
| `Game.LateUpdate` | Postfix | Debug: writes the queued save at the end of the frame and updates the button text |

### Saved state
**Id:** blk-75d011587898

Only the watcher's two cell snapshots, so the first comparison after a load has a "previous tick". The mod changes nothing else in the save.

## Business Rules
**Id:** blk-839fa2ca5de6

- The game forms a natural tile from a freezing liquid only when it holds at least 80% of the element's default mass; a one- or two-kilogram packet does not.
- A tile at the vent cell or landing cell is converted only when: the cell is solid, it is not a built foundation tile, an earlier-tick state exists, and the earlier state was not a liquid of at least 80% default mass ("formed from a real pool") and not already solid.
- A converted tile is dug through the sim and dropped as debris at the full mass the sim reports.
- The landing cell is the last open cell going down from the vent before the first solid (`LandingCell`). Cells still in the snapshot are also checked, since the walk down stops one cell higher once the landing cell turns solid.
- After the sim digs a cell, its snapshot resets to vacuum, so a tile that re-forms before the next tick is judged as new.
- A dig still pending with the cell still solid is asked again each tick.
- The watcher runs only in the first second of running time after a load, and does nothing at all when `RepairOnLoad` is off.
- The mod reads public grid data and uses the game's own dig path; it does not touch the sim otherwise.

## Domain Terminology
**Id:** blk-c479fbc0ae73

| Term | Definition |
|------|-----------|
| Packet | A small parcel of liquid mass moved through pipes; the vent stores one before emitting it |
| Natural tile | A solid cell of a natural element (not a built tile); what a freezing liquid becomes |
| Vent cell / landing cell | The vent's own cell / the last open cell under it |
| Snapshot | Element and mass of a watched cell at one tick (or at save time) |
| Verdict | The reason a cell is left alone, or null when its tile is converted (`Verdict`) |

## Key Code Paths
**Id:** blk-fdc3dcfd7c26

- **Install**: `LiquidVentConfig` postfixes add the insulated modifiers and the watcher. See `src/VentFreezeFix/Mod.cs`.
- **Save and load**: `[OnSerializing]` captures both cells; `[OnDeserialized]` -> `Restore` fills `previous`. See `src/VentFreezeFix/VentFreezeWatcher.cs`.
- **Repair**: `Sim200ms` -> `Check(cell)` -> `Verdict` -> `RequestDig` (`DigRequests.Add`, `SimMessages.Dig`) -> sim reports -> `WorldDamage.OnDigComplete` prefix -> debris spawned, `watcher.OnDug`. See `src/VentFreezeFix/VentFreezeWatcher.cs`, `src/VentFreezeFix/DigRequests.cs`.
- **Reproducing the bug**: repro button -> arm -> `Exhaust.EmitCommon` postfix -> `Game.LateUpdate` writes `VentFreezeRepro.sav`. See `src/VentFreezeFix/DebugSave.cs`.
- **Tests**: in-game `[OniTest]` suites in `src/VentFreezeFix.Tests/` (`Repair.cs`, `Storage.cs`, `ReproSave.cs`, `Kit.cs`).

## Related
**Id:** blk-75447c81c599

- See also: [glossary](glossary.md)
