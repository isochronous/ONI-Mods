# Unit 02 report: heavy-watt joint plates as jumper bridges

## Diff summary per file

- `src/JumperBridges/BridgeKind.cs`: `Of()` maps `HighWattBridgeTile` to `Power`. `NodeAnim` adds
  `WireBridgeHighWattage` -> `utilities_electric_insulated_kanim` and `WireRefinedBridgeHighWattage` ->
  `utilities_electric_conduct_hiwatt_kanim`. New `BridgeKind.IsJointPlate(GameObject)`, which replaces
  `PowerPorts.IsJointPlate`. Doc comments on `Power`, `OfBridge` and `NodeAnim` updated.
- `src/JumperBridges/Patches.cs`:
  - New postfixes `WireBridgeHighWattageConfig.DoPostConfigureComplete` (adds `BridgeFootprint` and
    `JumperBridge`) and `DoPostConfigureUnderConstruction` (adds `BridgeFootprint`). The conductive plate
    inherits the first and calls the second through `base`.
  - `MarkArea` prefix: for `HighWattBridgeTile` it runs the vanilla footprint loop (with
    `Uprootable.CanUproot` moving a plant to the Plants layer) on whatever layer it is called with
    (Building/FoundationTile from BuildingComplete, object/replacement/tile layer from Constructable). The
    ends are then marked on WireConnectors only when free, the same as the wire bridges. The centre stays
    registered.
  - `PowerPorts.HasEnd(cell, self, jointPlate)`: looks for a power bridge or plate end at the cell, both on
    the WireConnectors layer and through `BridgeFootprint`. A footprint lookup is needed because an end on a
    port does not hold the slot.
  - `IsValidWireBridgeLocation` prefix: refuses a wire bridge end on any plate end, now also one that sits
    on a port (closes the reverse hole).
  - `IsValidHighWattBridgeLocation`: was a postfix, now a replacing prefix. Plate ends may sit on ports and on
    other plate ends, and wire bridge ends are refused. Vanilla's other refusals stay: `AreCellsValid`
    (INVALID_CELL), an end on FoundationTile, `Grid.HasDoor`, and a door under construction on Building
    (HIGHWATT_NOT_IN_TILE).
  - New `IsValidTileLocation` patch.
    - Postfix: refuses a tile, or a plate centre, on any plate end found through `BridgeFootprint`
      (WIRE_OBSTRUCTION). This closes the tile-over-end hole for plate ends whose port has gone.
    - Prefix: on a cell whose WireTile occupant is a `BridgeEndNode`, it runs vanilla's other two checks
      itself. Vanilla's first check does `GetComponent<Building>().Def` on the WireTile occupant, and a node
      has no Building. That means a NullReferenceException when hovering a tile over any power end node,
      wire bridges included (pre-existing, found by reading the decompile; not reproduced in game).
  - Comments updated where they said plates are left alone.
- `src/JumperBridges.Tests/Power.cs`: `Floor`, `Circuit` and `ValidWithPreview` changed from private to
  internal so the plate tests can reuse them.
- `src/JumperBridges.Tests/JointPlates.cs` (new): five tests (below).
- `src/JumperBridges/ModStrings.cs` + `translations/JumperBridges.pot`, `zh.po`, `ko.po`, `ru.po`: the
  POWER tooltip said "Heavy-watt joint plates are unchanged". It now lists the Heavi-Watt Joint Plate among
  the bridges. Same key, so no new string. The three translations are updated to match.
  `translations.py jumper-bridges --build` reports ALL CHECKS PASSED.
- `README.md`: intro, a joint-plate paragraph in "Power bridges", patch list, testing summary.
- `CHANGELOG.md`: new `## Unreleased` with one bullet.
- `publish/workshop-description.txt`: "An automation bridge, power bridge or Heavi-Watt Joint Plate can be
  placed…".

## New tests (all in `JointPlates.cs`; filter `-Filter "*joint plate*"`)

- `Jumper Bridges joint plate: battery to plate to lamp carries power with no wire`: one circuit, a 20 kW
  limit, no HasInvalidPorts, and the lamp powered after `Battery.AddEnergy`.
- `Jumper Bridges joint plate: the centre is a solid tile and the ends draw conductive heavi-watt wire`: uses
  the conductive plate. Checks `Grid.Solid`, `Grid.Foundation`, the plate on Building and FoundationTile at
  the centre, on WireConnectors at a free end, and the node's anim file.
- `Jumper Bridges joint plate: a plate end and a wire bridge end may not share a cell`: both directions, each
  with the first end on a battery port. Each preview is valid before the other bridge is built and refused
  after.
- `Jumper Bridges joint plate: a tile may not be built on a plate end that sat on a port`: the battery is
  destroyed, then a Tile preview on the end is refused with WIRE_OBSTRUCTION. This test also exercises the
  node prefix.
- `Jumper Bridges joint plate: a plate between two ports is marked for deconstruction from its centre`.

None have been run (the manager runs them).

## Deviations from the decisions

- None in behaviour. There are two additions outside the four listed test cases:
  - The tile-on-end test, for the new `IsValidTileLocation` code path.
  - The NRE guard prefix, which that test would otherwise hit.
- The existing tooltip text was reworded (no new key) because it stated the opposite of the new behaviour.
  Translations were reworded by me, not by a native speaker.

## Build output (verbatim tail)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`:

```
  JumperBridges -> C:\dev\ONI-Mods\mods\jumper-bridges\src\JumperBridges\bin\Release\JumperBridges.dll
  JumperBridges.Tests -> C:\dev\ONI-Mods\mods\jumper-bridges\src\JumperBridges.Tests\bin\Release\JumperBridges.Tests.dll
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.14
```

On the clean build the warnings came from other mods. None were in jumper-bridges.
`ModCheck` against the game's Managed dir reported ALL CHECKS PASSED.
