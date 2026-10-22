# Unit 07 review: d95ad94, plate and wire bridge ends may share a cell

Reviewed `git show d95ad94 -- mods/jumper-bridges/` (12 files) against the current sources and a fresh
`ilspycmd -t` (9.1) decompile of `BuildingDef`, `CircuitManager`, `ElectricalUtilityNetwork`,
`WireUtilityNetworkLink` and the four power bridge configs from the game's `Assembly-CSharp.dll`.
Read-only: nothing built, no game launch. Passes run: 1 correctness, 2 security, 3 integration, 4 clarity,
5 evidence.

**Verdict: no critical or moderate findings.** The change does what plan.md ## Decisions asks. Four minor
findings and one evidence gap follow.

## What was checked, with the vanilla evidence

- **Placement, both orders.** Vanilla `IsValidWireBridgeLocation` (BuildingDef.cs:1489) refuses only when
  `Grid.Objects[end, 29] != null`, and vanilla `IsValidPlaceLocation` returns its result directly for
  `WireBridge` (BuildingDef.cs:718), so the new unconditional `true` prefix (Patches.cs:228-233) drops no other
  vanilla check. It is also the same result as the pre-change prefix for every case except the plate refusal
  (a null `source_go` or a missing link gave `true` before as well). Vanilla `IsValidHighWattBridgeLocation`
  (BuildingDef.cs:1510) checks off-map, 29, FoundationTile, door and door under construction. The prefix
  (Patches.cs:247-267) keeps everything except 29. Wire bridge first: the bridge holds 29 and the plate's
  `MarkIfFree` leaves it (Patches.cs:198-202). Plate first: the plate holds 29 and the wire bridge leaves it.
  Neither order hits any other refusal; the removed test proved that each refusal came from the mod alone
  (it asserted the mod's `WIRECONNECTORS_OVERLAP` reason in both directions).
- **Under-construction bridges.** `BridgeFootprint` is on the BUC prefab, and `IsJointPlate` reads
  `Building.Def`, which a `BuildingUnderConstruction` has. So `JointPlateEndAt` (BridgeFootprint.cs:40-48)
  sees a queued plate, and a tile cannot be queued over a queued plate's end.
- **Tile refusal on a shared end.** Wire bridge holds 29: the `IsValidTileLocation` prefix (Patches.cs:293-310)
  sees `WireBridge` on 29 and lets it through, and the postfix (319-326) refuses through `JointPlateEndAt`.
  Plate holds 29: the prefix refuses on `HighWattBridgeTile`, which matches vanilla BuildingDef.cs:800-805.
  Real wire under the ends (no node): vanilla runs, and the postfix still refuses. Port holds 29: the postfix
  refuses (test `NoTileOnEnd`).
- **After either bridge is deconstructed.** Vanilla `UnmarkArea` clears 29 only where `Grid.Objects == go`
  (BuildingDef.cs:1028-1038), so removing the bridge that does not hold the slot leaves the holder in place.
  Plate removed: `BridgeFootprint.OnCleanUp` takes it out of `byCell`, and 29 is cleared or still the wire
  bridge. A tile may then cover the wire bridge's end node, as vanilla allows over a wire bridge end on a wire.
  So the cell is not wrongly blocked. Wire bridge removed: the plate is still in the registry, and the
  postfix (or, if the plate held the slot, the prefix) keeps the tile off. No end is left buried.
  `BridgeEndNode.Disown` keeps the node while the plate still owns it.
- **Port cells.** A building port placed later on a shared end: `ArePowerPortsInValidPositions` counts a
  bridge end on 29 as free (Patches.cs:354-358), and `MarkOverlappingPorts` never tags a power bridge. The
  change does not touch this; it holds for mixed pairs as it did for pairs of one bridge type.
- **BridgeLinks ring with mixed bridges.** `BridgeKind.Of` maps both `WireBridge` and `HighWattBridgeTile` to
  `Power` (BridgeKind.cs:85-97), so `GroupCells` (BridgeLinks.cs:56-95) puts both in one group. The test
  layout gives the ring 1 -> 3 -> 5 -> 1.
- **Wattage claim.** `CircuitManager` files each bridge under its `maxWattageRating` (CircuitManager.cs:245-254).
  `ElectricalUtilityNetwork.GetMaxSafeWattage` returns the lowest group that holds a wire or a bridge
  (EUN.cs:154-168). WireBridge is Max1000 and the plates are Max20000 and Max50000, so the test's `1000f` and the
  README's "the lower wattage limit of the two" are both right.
- **Removed `PowerPorts.HasEnd` and renamed `AnyPowerEndAt`.** A repository-wide grep (excluding bin/obj) finds
  no remaining caller in source, tests or docs. `JointPlateEndAt` has its one caller at Patches.cs:321.
  Dropping the `Kind == Power` filter is safe because the patch's `Prepare` already gates on Power.
- **Security (pass 2).** There is no trust boundary or input handling in this change. Nothing found.
- **Player text.** The en, ko, ru and zh tooltips match each other. README, CHANGELOG and the Workshop
  description agree with the code. The text has no hedging, and the Workshop copy is short and player-facing.

## Findings

### m1 (minor, pass 4): tooltip says "that port" when the end may be on a bridge end
`mods/jumper-bridges/src/JumperBridges/ModStrings.cs:24`, and the matching msgid and msgstr in `ko.po`, `ru.po`
and `zh.po`. The tooltip says "placed with either end directly on a power port or on the end of any of these,
and that port is connected." When the end sits on another bridge's end there is no port for "that port" to
refer to. Suggested wording: "...directly on a power port, which is then connected, or on the end of any of
these." The translations would need the same change.

### m2 (minor, pass 1, cosmetic): a shared end draws as the first owner's wire, even after it leaves
`BridgeEndNode.Create` picks the kanim from the bridge that created the node (BridgeEndNode.cs:64), and
`AddOwner`/`Disown` never change it (194-205). Before this commit, a node was only ever shared between plain,
conductive and insulated wire bridges, or between two plates. Now a plate end that shares a wire bridge's node
draws as thin plain wire. If the wire bridge is deconstructed, the plate's end keeps the plain-wire art until a
reload. After a reload, whichever bridge's `CreateNodes` runs first decides the art. README.md:21 says a plate
end "draws as a piece of heavi-watt or conductive heavi-watt wire", which is not true on a shared cell.
Options: change the art when the creating owner leaves, or let a plate's art win, or accept this and say
nothing about shared cells. Jeremy decides.

### m3 (minor, pass 5): no test for the plate-first order or for removing one bridge
`mods/jumper-bridges/src/JumperBridges.Tests/JointPlates.cs:122-150`. `OnWireBridgeEnd` covers the wire bridge
first, which is the postfix path. No test runs the `IsValidTileLocation` prefix's `HighWattBridgeTile` refusal
on a node cell (Patches.cs:299-303), the path a plate-first shared end, or any plate holding its free end's
slot, depends on. `SolidCentre` asserts that the plate holds the slot but never previews a tile there. No test
deconstructs one bridge of a shared pair either. One representative test would do: plate first, wire bridge
onto its end, tile refused, then deconstruct the plate, and a tile on the wire bridge's end is allowed. That
also exercises the prefix path.

### m4 (minor, pass 4): stale comments and store records
- Patches.cs:205-209: the `PowerPorts` summary describes the placement rules, but the class now holds only
  `IsBridgeEnd`. Its last sentence, "the lowest rating among its bridges", leaves out wires; the limit is the
  lowest among wires and bridges (EUN.cs:154-168).
- Patches.cs:281-282: "may not hold it" reads as a prohibition. "does not always hold it" says what is meant.
- Store records still state the superseded rule. `mods/jumper-bridges/.context/domains/entities.md:111`
  ("never with a wire bridge's end (refused both ways)"), `glossary.md:85` ("never on a wire bridge end") and
  `entities.md:94` (`blk-a88b0d39bc11`, "on ports and other plate ends"). These are for the plan's pending
  "store notes" step, through the write path.

### Evidence gap (pass 5, not a code finding)
Unit 06 ran the build (0 errors) and the translations check (ALL CHECKS PASSED). It did not run the in-game
tests, so `OnWireBridgeEnd` has not been seen green. One thing can only be confirmed in game: the `Tile` preview
on the shared cell must fail with `HELP_BUILDLOCATION_WIRE_OBSTRUCTION` and not with an earlier
`IsValidPlaceLocation` reason. The plan already lists the in-game run as the next step:
`-Filter "*joint plate*"`.

A pacing note for the same run: the test builds the battery, lamp and wire bridge in one frame with no game
time between them (JointPlates.cs:129-131). `NoTileOnEnd` has the same pattern, so this is not new, but the
human-speed rule would put `WaitGameSeconds` between them.
