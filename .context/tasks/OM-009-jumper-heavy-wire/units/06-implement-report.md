# Unit 06 report: wire bridge ends and joint plate ends may share a cell

Model tier: default. Game not launched; in-game tests not run.

## Per-file changes

- `src/JumperBridges/Patches.cs`
  - `BuildingDef_IsValidWireBridgeLocation_Patch`: the prefix now accepts every end (ports and any power bridge end); the joint plate refusal is gone. Vanilla's only refusal was "anything on WireConnectors", so nothing else is kept.
  - `BuildingDef_IsValidHighWattBridgeLocation_Patch`: the wire bridge end refusal is gone; off-map, in-tile and door refusals kept.
  - `PowerPorts.HasEnd` removed (it existed only for the no-sharing rule and the `jointPlate` filter).
  - `BuildingDef_IsValidTileLocation_Patch.Postfix` now refuses when `BridgeFootprint.JointPlateEndAt(cell, source_go)`. The registry holds every complete and queued plate, so a shared end is covered whichever bridge was built first and whoever holds the WireConnectors slot (explore Q3 item 3). When a plate holds the slot, vanilla's own 29 check also refuses; when a wire bridge holds it, the node-cell prefix lets it through and the postfix refuses.
  - Doc comments updated for the new rule.
- `src/JumperBridges/BridgeFootprint.cs`: `AnyPowerEndAt(cell, self, jointPlate)` became `JointPlateEndAt(cell, self)` (no kind/flag filter, no allocation).
- `src/JumperBridges.Tests/JointPlates.cs`: `NotOnWireBridgeEnd` replaced by `OnWireBridgeEnd`.
- `src/JumperBridges/ModStrings.cs`, `translations/JumperBridges.pot` (rebuilt), `ko.po`, `ru.po`, `zh.po`: POWER.TOOLTIP is now "... can be placed with either end directly on a power port or on the end of any of these, and that port is connected. A tile cannot be built over a joint plate's end. Needs a restart." Translations updated to match.
- `README.md`: power paragraph says wire bridge ends may sit on any power bridge end, joint plates included; plate paragraph says plate ends may sit on any power bridge end, wire bridges included, with no tile over a plate end, and a circuit joined through both takes the lower wattage limit (same plain-fact style as the existing "the circuit's wattage limit is the bridge's own"). Patch list and test summary updated.
- `CHANGELOG.md`: Unreleased plate bullet now says "on a power port or on any power bridge's end".
- `publish/workshop-description.txt`: "a joint plate end can share a cell with any power bridge's end. A tile cannot be built over a joint plate's end."

## Tests changed

- Removed: `JointPlates.NotOnWireBridgeEnd` ("a plate end and a wire bridge end may not share a cell").
- Added: `JointPlates.OnWireBridgeEnd` ("Jumper Bridges joint plate: a plate end on a wire bridge end joins its circuit and keeps a tile off"). Floor of 7; battery (1,1), lamp (5,1), Wire Bridge at (2,1) built first so it holds the WireConnectors slot on the shared cell (3,1) (asserted). Plate preview at (4,1) valid; after build, battery port, shared cell and lamp are on one circuit, limit 1000 W (the wire bridge's); a Tile preview on the shared cell is refused with HELP_BUILDLOCATION_WIRE_OBSTRUCTION. Game-time waits after each build, as in the other tests.

## Build

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` tail:

```
    288 Warning(s)
    0 Error(s)

Time Elapsed 00:00:07.92
```

(The warnings are other mods' CS0618; the JumperBridges and JumperBridges.Tests projects built.)

## Translations

`python common/tools/Translations/translations.py mods/jumper-bridges --build`:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.37
Wrote 4 strings to C:\dev\ONI-Mods\mods\jumper-bridges\src\JumperBridges\translations\JumperBridges.pot
ko.po: 4/4 translated
ru.po: 4/4 translated
zh.po: 4/4 translated
ALL CHECKS PASSED
== jumper-bridges
```

## Next

Run `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Filter "*joint plate*"` to prove `OnWireBridgeEnd` and that the other plate tests still pass.

## Brief feedback

- About 22 pointers were other mods' code-style "Line Length" and "Harmony Patches" sections; only the jumper-bridges copies apply.
- Nothing covered the test files or the player-text style; memory notes covered that.
- `blk-a88b0d39bc11` (patch list) says IsValidHighWattBridgeLocation allows ends "on ports and other plate ends"; now stale (any power bridge end). For the completion phase.
