# Unit 10 findings: Power.LampOntoBridgeEnd regression

## Verdict

The cause is state leaking between tests, not the mod diff. The new test `JointPlates.WireBridgeOnPlateEnd`
(added in d95ad94/fbe882a) leaves its wire bridge alive after the fixture clear. That bridge still has a
connected link, and its end shares cell 119036 with the bridge `Power.LampOntoBridgeEnd` builds. `BridgeLinks`
puts the leftover bridge into the new bridge's group, and the directed ring gets a dead cell (119038, which has
no wire and no node). From the lamp's cell, the flood fill's only link hop goes into that dead cell. Lamp and
battery split onto two adjacent networks (21 and 22).

The code path the test exercises (port checks, MarkOverlappingPorts, node creation, UpdateArt) does not change
behaviour in this test. Confidence: high on the leak and the ring. The last step (which network the fill starts
from) depends on `HashSet<int>` iteration order and can only be confirmed in game.

## Evidence

### 1. The fixture clear cannot see a bridge whose two ends are both taken

- `common/tools/TestRunner/FixtureArea.cs:112-124`: the clear collects only `Grid.Objects[cell, layer]` for
  each cell in the rect.
- A wire bridge's ObjectLayer is WireConnectors (`WireBridgeConfig`: `obj.ObjectLayer = ObjectLayer.WireConnectors`).
  `BuildingDef_MarkArea_Patch` (`Patches.cs:172-180`) marks only its end cells, and only when they are free
  (`MarkIfFree`). If both ends sit on ports or on another bridge's slot, the bridge is on no layer, and the
  clear never destroys it. Nothing gives a freed slot back to the bridge later (no UnmarkArea or reassign code).

### 2. Which bridges leaked (object counts in the clear log, om009b Player.log)

| Test | Objects on layers | Logged | Bridge destroyed? |
|---|---|---|---|
| OnWireBridgeEnd (1350-1353) | 7 tiles + battery + lamp + WireBridge + plate (solid centre) + 3 nodes = 14 | 14 | yes |
| WireBridgeOnPlateEnd (line "cleared ... 11 object(s)") | 7 tiles + battery + lamp + nodes 119036, 119038 = 11 | 11 | **no**: WireBridge at 119037 leaked |
| BatteryBridgeLamp (line "cleared ... 9 object(s)") | 5 tiles + battery + lamp + 2 nodes = 9 | 9 | no: WireBridge at 119035 leaked (same in the passing run) |

The 11 also matches the missing log line. `WireBridgeComplete at 119037: end nodes removed` never appears. Only
the plate's removal (`WireBridgeHighWattageComplete at 119035: end nodes removed`, 13:25:33.279, from
`t.World.Destroy(plate)`) is logged.

How the WireBridgeOnPlateEnd bridge (B2) ended up on no layer: when it was built, end 119036 was held by the plate
(the test asserts `the plate holds the shared cell's connector slot`) and end 119038 was the lamp's power port.
When the plate was destroyed, its slot was cleared and not given to B2. The clear then destroyed both nodes
(on the WireTile layer) but not B2. B2's `JumperBridge` forgot its nodes (`BridgeEndNode.Release` → `Forget`)
and never makes them again, because `ScheduleCreate` runs only on spawn and repair. Its `UtilityNetworkLink`
stays connected, so it stays in `BridgeLinks.connected`.

The bridge from BatteryBridgeLamp (B1, at 119035, cells 119034/119036) leaks in both runs. On its own it does no
harm, because its cells are the same as the next test's bridge and a ring of two cells is the vanilla
two-way pair. That is why the test passed at 44f7dd9. Back then the test before it was `NotOnWireBridgeEnd`,
whose wire bridge held a free end slot and was cleared (18 objects = 10 tiles + 2 batteries + bridge + plate +
4 nodes).

### 3. How the leftover bridge splits the circuit

LampOntoBridgeEnd builds bridge B3 at 119035 (cells 119034 battery port, 119036 free; log 13:25:36.471 `end
nodes at 119034 and 119036`).

- `BridgeLinks.OnConnect` → `GroupCells` (`BridgeLinks.cs:55-96`) walks every connected power link through
  shared cells. It finds B3 and B1 (119034, 119036) and the leaked B2 (119036, 119038), so the group is
  {119034, 119036, 119038}.
- `RewriteGroupAt` (`BridgeLinks.cs:43-53`) writes the sorted directed ring:
  `links[119034]=119036`, `links[119036]=119038`, `links[119038]=119034`.
- Vanilla flood fill (`UtilityNetworkManager.RebuildNetworks`, decompiled): from each visited cell it follows
  one `links[cell]` hop through `QueueCellForVisit`. That call refuses a cell where
  `Grid.Objects[dest_cell, tileLayer] == null`. Cell 119038 has nothing on WireTile, because B2's node was
  destroyed by the clear.
- 119034 and 119036 are not neighbours (119035 sits between them), and the nodes have no wire bits. So:
  - If the fill starts at 119034, it hops to 119036, and the lamp and battery share one network (pass).
  - If it starts at 119036 (the lamp's port), it hops to dead 119038 and stops. Network N = {119036}, and
    119034 then becomes network N+1 (fail).
- Network ids are `networks.Count` when each network is created (`id = networks.Count`). Start cells come from
  iterating `HashSet<int> physicalNodes`. The failure `expected 22 (battery cell), got 21 (lamp)` is the second
  case: the lamp's network was created first, and the battery's got the next id.
- `EnergyConsumer.CircuitID` is refreshed every 200 ms from `GetCircuitID(PowerCell)` (decompiled
  `EnergySim200ms`). The 0.5 s wait leaves it current, so the 21 is the lamp port's real network and not a stale
  cache.

### 4. The diff does not touch this test's path

- `git diff 44f7dd9..HEAD -- mods/jumper-bridges/src` removes `PowerPorts.HasEnd` and the wire-bridge/plate
  end refusals. It also narrows the IsValidTileLocation postfix to `BridgeFootprint.JointPlateEndAt` and adds
  `UpdateArt`.
- In LampOntoBridgeEnd no joint plate exists. `ArePowerPortsInValidPositions` and `MarkOverlappingPorts` still
  use `PowerPorts.IsBridgeEnd` unchanged. The preview check passed (`the lamp may be placed...` was asserted
  before the failure). Each node has a single owner, so `UpdateArt` returns at the `artOwner` check without
  swapping.
- The diff's only part in this failure: d95ad94 made `WireBridgeOnPlateEnd` possible (a wire bridge end on a
  plate end), and that test is what leaves an off-layer bridge next to the Power tests' cells.

## Proposed fix (not made)

1. **Root cause, harness** (`common/tools/TestRunner/FixtureArea.cs` `Clear`): also collect every building
   whose placement cells intersect the rect, not only what sits on `Grid.Objects`. For example, walk
   `Components.BuildingCompletes` (plus the under-construction list) and add each one whose
   `Building.PlacementCells` touch `rect`. Then a bridge on no layer is destroyed like everything else.
   Reach: every mod's tests that use the fixture clear. It only adds objects inside the test rect, so no
   passing test should change. It also fixes the long-standing BatteryBridgeLamp and DeconstructBetweenPorts
   leaks. This follows the memory note "harness changes welcome": fix the harness rather than add a
   workaround in the test.
2. **Defense in depth, harness**: after the clear, warn when an object from `World.Build` is still alive
   (World.Build could record what it builds). The next leak of this kind would then show in the log instead
   of as a wrong circuit id two tests later.
3. **Optional, mod** (`BridgeLinks.RewriteGroupAt`): a directed ring breaks at any group cell with nothing on
   the tile layer. In normal play every connected end has a wire or a node, so this only shows when a node is
   destroyed from outside while its bridge stays (the test clear here, and possibly the sandbox destroy tool;
   not checked). One hardening: leave cells with no tile-layer object out of the ring, and rewrite the group
   when a node spawns or releases. Reach: BridgeLinks plus BridgeEndNode `OnSpawn`/`Release`. This is not
   needed for this failure, so I would not do it in OM-009 unless asked.

Check after fix 1: run `-Filter "JumperBridges.Tests/*"` and see that WireBridgeOnPlateEnd's clear logs 12
objects, BatteryBridgeLamp's 10, and that LampOntoBridgeEnd passes.
