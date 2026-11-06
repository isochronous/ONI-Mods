# Unit 01 findings: heavy-watt joint plates in Jumper Bridges

Vanilla evidence comes from `ilspycmd -t <Type>` (9.1) against
`F:/SteamLibrary/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed/Assembly-CSharp.dll`,
the same `GameFolder` that `common/oni-mod.props:21-24` resolves. The same method produced the earlier
`dab-decomp` notes (`C:/dev/incubator/direct-automation-bridges/INVESTIGATION.md:5-13`). The decompiles are in the
session scratchpad (`.../scratchpad/decomp/`). They are throwaway. Vanilla member names are quoted as decompiled.

ObjectLayer indexes used below: 1 Building, 9 FoundationTile, 11 ReplacementTile, 27 WireTile, 29 WireConnectors.

## Headline: the premise in Q3 does not hold

Jumper Bridges never changes a bridge's span, width, footprint or link offsets. "Jumper" means only that a
bridge **end may sit directly on a port cell, or on another bridge's end**, and that port counts as connected
(README.md:3-19, overview `blk-2d2edc3c3db1`). No code reads or writes a span length. Both ends are always
`UtilityNetworkLink.GetCells` of the vanilla `link1`/`link2` (JumperBridge.cs:91, BridgeFootprint.cs:54,
Patches.cs:143). So a jumper heavy-watt plate keeps its single solid centre cell, the same as vanilla. Nothing
in the mod would turn more cells solid. The real work sits at the **end** cells: placement checks, layer
marking and nodes.

## Q1: how a bridge kind is defined today, and what a new kind needs

**BridgeKind** (`src/JumperBridges/BridgeKind.cs`)
- Two instances, `Logic` (19-24) and `Power` (31-36), and `All` (38). A kind holds the ConnectionSource, the
  layers (TileLayer, WireLayer, ReplacementLayer, EndLayer), the SceneLayer and the manager type. It also keeps
  per-kind `Nodes` (55) and the manager's private `items`/`links` read by reflection (58, 70-78).
- `Manager` (74) is a property chosen per instance: `logicCircuitSystem` or `electricalConduitSystem`.
- `Of(BuildingDef)` (84-95) maps the placement rule to a kind: `LogicBridge`→Logic, `WireBridge`→Power. All
  other rules return null, `HighWattBridgeTile` among them, and that is the one switch that keeps plates out.
- `OfBridge(GameObject)` (107-114) is `Of(building.Def)` plus `Enabled`.
- `NodeAnim` (120-136) is the end-piece art. The power branch switches on prefab ID:
  `WireRefinedBridgeConfig.ID`→`utilities_electric_conduct_kanim`,
  `WireRubberBridgeConfig.ID`→`utilities_electric_rubber_kanim`, and everything else gets
  `utilities_electric_kanim`.
- **Constraint:** `Nodes` and `Links` assume one manager per kind, with one item and one link partner per cell
  (54, BridgeLinks.cs:161-172). The heavy-watt plates use `electricalConduitSystem` too. A separate
  `BridgeKind` instance for them would keep a second node table and a second ring table over the same manager,
  and two nodes on one cell would fight over it. The plates have to join `Power`'s tables. They can be
  recognised in `Of()` as Power, with an option flag of their own if the user wants one. The other route is to
  refactor BridgeKind so that a "family" (enable flag, art) is separate from the "network" (manager, nodes,
  links).

**Options / Mod** (`Options.cs:10-22`, `Mod.cs:15-24`): two restart-required bools, `AutomationBridges` and
`PowerBridges`. `OnLoad` copies them into `BridgeKind.X.Enabled` *before* `base.OnLoad`, so each patch's
`Prepare()` can skip itself.

**Prefab patching** (`Patches.cs`)
- Complete prefab: `JumperBridge` + `BridgeFootprint` (Logic 10-26, Power 51-61). Under construction:
  `BridgeFootprint` only (29-44, 63-72). The power patch targets `WireBridgeConfig` alone. The conductive and
  insulated bridges inherit the method without overriding it (comment 46-50).
- For the plates, `WireRefinedBridgeHighWattageConfig` does not override `DoPostConfigureComplete`. It
  overrides `DoPostConfigureUnderConstruction` but calls `base` first. So postfixes on
  `WireBridgeHighWattageConfig.DoPostConfigureComplete` and `DoPostConfigureUnderConstruction` cover both plates,
  following the same pattern.

**End cells**: always vanilla `link.GetCells`. Nodes go only on ends with nothing on `Kind.TileLayer`
(JumperBridge.cs:101-115). Creation is deferred one UI frame (79-95).

**BridgeFootprint** (`BridgeFootprint.cs`) is a static `byCell` registry of end1, the origin cell and end2
(51-58). `EndsAt` (29-37) serves the tools and placement. `At` (40-48) includes the middle and serves the wire
`OnCleanUp` path through `JumperBridge.At` (JumperBridge.cs:120-130). `Kind` comes from `OfBridge` (53).

**BridgeLinks** (`BridgeLinks.cs:17-103`): after any connect or disconnect, the distinct end cells of a group
are rewritten as a sorted ring, then `ForceRebuildNetworks`. The group is per kind. If plates join Power, a
plate and a 1 kW bridge sharing a cell form one ring.

**BridgeEndNode** (`BridgeEndNode.cs:58-121`): art from `kind.NodeAnim(bridge)` (64), element copied from the
bridge (73-75), registration on `Kind.TileLayer` and `AddToNetworks` (114-115). It is not a `Wire`, so the
circuit limit comes from the bridges (24-26).

**Patches that mention joint plates today**
- `PowerPorts.IsJointPlate` (Patches.cs:174-178).
- `IsValidWireBridgeLocation` prefix refuses a power bridge end on a plate end (181-204, line 192).
- `IsValidHighWattBridgeLocation` postfix refuses a plate end on a power bridge end (206-229).
- The `UtilityNetworkLink.OnConnect`/`OnDisconnect` postfixes leave plates out only because `OfBridge` returns
  null (74-112).

**Adding the plates, file by file**
- `BridgeKind.cs`: `Of()` (84-95) gains `HighWattBridgeTile`. Fix the doc comments (26-31, 106). `NodeAnim`
  (127-135) gains `WireBridgeHighWattageConfig.ID`→`utilities_electric_insulated_kanim` (the anim
  `WireHighWattageConfig` gives "HighWattageWire") and
  `WireRefinedBridgeHighWattageConfig.ID`→`utilities_electric_conduct_hiwatt_kanim` (from
  `WireRefinedHighWattageConfig`). If the plates get their own option, `Enabled` has to be per family. Today a
  `Prepare()` per kind gates everything.
- `Patches.cs`:
  - Add two postfixes on `WireBridgeHighWattageConfig`.
  - The **MarkArea prefix (121-156) must change**. It returns `false` and skips the whole vanilla method on the
    grounds that "the vanilla method does nothing else for these two rules: no footprint marks" (129-130). That
    is false for `HighWattBridgeTile`. Vanilla `MarkArea` *does* run the PlacementOffsets footprint loop for that
    rule, because only `Conduit`/`WireBridge`/`LogicBridge` are excluded. `BuildingComplete.OnSpawn` calls it
    twice, once with `Def.ObjectLayer` (Building) and once with `Def.TileLayer` (FoundationTile).
    `Constructable.MarkArea` calls it with ObjectLayer, or ReplacementLayer when replacing, then with TileLayer
    if that is free. A plate routed through today's prefix would lose its Building and FoundationTile marks, and
    the centre tile would be registered nowhere. For plates, either reproduce the footprint loop (with
    `Uprootable.CanUproot`) or let vanilla run and repair layer 29 and the `HasInvalidPorts` tags afterwards.
  - `IsValidHighWattBridgeLocation` (210-229) has to become a check that lets ends sit on ports and power
    bridge ends. It must keep vanilla's other refusals: `AreCellsValid`, an end on FoundationTile (9),
    `Grid.HasDoor`, and a door under construction on layer 1.
  - Decide on 181-204 and 206-229: may a wire bridge end and a plate end share a cell?
  - `PowerPorts.IsBridgeEnd` (166-172) becomes true for plates on its own once their footprint's Kind is Power.
    That lets a building's port land on a plate end through `ArePowerPortsInValidPositions` (236-261) and
    `MarkOverlappingPorts` (269-281).
- `JumperBridge.cs`, `BridgeFootprint.cs`, `BridgeLinks.cs`, `BridgeEndNode.cs`: no structural change needed.
- `Options.cs`, `Mod.cs`, `ModStrings.cs`, the translations, `README.md`, `CHANGELOG.md`: see Q5.

## Q2: how vanilla makes the plate's centre solid

From `WireBridgeHighWattageConfig`, with the conductive plate inheriting through
`WireRefinedBridgeHighWattageConfig`:
- `CreateBuildingDef(id, width 1, height 1, "heavywatttile_kanim", …, BuildLocationRule.HighWattBridgeTile)`. The
  plate is **1×1**. Its footprint is the centre cell only.
- `BuildingTemplates.CreateFoundationTileDef(def)` sets:
  - `IsFoundation = true`
  - `TileLayer = FoundationTile`
  - `ReplacementLayer = ReplacementTile`
  - `ReplacementCandidateLayers = {FoundationTile, LadderTile, Backwall}`
  - `ReplacementTags = {FloorTiles, Ladders, Backwall}`
  - `EquivalentReplacementLayers = {ReplacementLadder}`

  So the plate can replace an existing tile or ladder.
- `ObjectLayer = Building`, `SceneLayer = WireBridgesFront` (the conductive plate uses `WireBridges`),
  `ForegroundLayer = TileMain`, `PermittedRotations = R360`, `Entombable/Floodable/Overheatable = false`,
  `BaseTimeUntilRepair = -1`. No `DragBuild` is set.
- `ConfigureBuildingTemplate`:
  - `IgnoreDefaultKComponent(RequiresFoundation)`, so the plate needs no floor.
  - `SimCellOccupier` with `doReplaceElement = true` and `movementSpeedMultiplier = PENALTY_3`. Its OnSpawn
    `SimMessages.ReplaceElement`s the centre cell to the building's element, so the cell is solid sim matter.
    `notifyOnMelt` is set.
  - `BuildingHP.destroyOnDamaged = true` and `TileTemperature`.
- `BuildingComplete.OnSpawn` sets `Grid.Foundation[cell] = true` for each PlacementCell (`IsFoundation`). It
  marks Building and FoundationTile at the centre.
- `WireUtilityNetworkLink` sets `link1 = (-1,0)`, `link2 = (1,0)` and `maxWattageRating` Max20000 (the
  conductive plate: Max50000).
- **Link offsets are identical to the Wire Bridge's** (`WireBridgeConfig.AddNetworkLink`: (-1,0)/(1,0), Max1000).
  The difference is the footprint. The Wire Bridge is **3×1**, `ObjectLayer = WireConnectors`, with ends inside
  its footprint, and `MarkArea` skips its footprint marks. The plate is 1×1 with its ends **outside** the
  footprint, one cell either side. `MarkArea`'s `WireBridge || HighWattBridgeTile` branch marks both end cells on
  WireConnectors (29) and tags whatever was there with `HasInvalidPorts`.
- Placement (`BuildingDef.IsValidBuildingLocation`, `case HighWattBridgeTile`): `IsValidTileLocation` on the
  centre, then `IsValidHighWattBridgeLocation`, then the general power/conduit/logic port checks.
  - `IsValidTileLocation` refuses a centre on a `NotInTiles` building on WireTile (27), or on a
    **HighWattBridgeTile on 29** (so no plate centre on another plate's end), or on a NotInTiles backwall.
  - `IsValidHighWattBridgeLocation` refuses:
    - ends off-grid (`AreCellsValid`)
    - anything on 29 at either end
    - FoundationTile (9) at either end
    - a door at either end, or a door under construction
- `UnmarkArea` clears the plate's end cells on 29 only if they still hold the plate (`== go`). A mark-if-free
  scheme is therefore safe on removal.
- Deconstruct (`Deconstructable.OnCompleteWork`): with a SimCellOccupier and a TileLayer, it nulls Building and
  FoundationTile at the centre, clears `Grid.Foundation`, then runs `SimCellOccupier.DestroySelf`, which
  `ReplaceElement`s the centre to Vacuum. Construction waits for fetches before digging
  (`Constructable`: `waitForFetchesBeforeDigging` when the complete building has a SimCellOccupier). The
  replacement path is `Constructable` lines ~380-392 and ~554.

## Q3: what a jumper span does to the solid centre

The mod keeps the vanilla footprint and vanilla link ends (headline above). So:
- **Solid cells:** only the plate's own cell, in every case. The ends are one cell out and are never solid,
  because vanilla refuses an end on FoundationTile.
- **Can a single solid cell be kept?** Yes. No other outcome exists unless the mod deliberately strips the
  solidity.

What breaks or needs care:
1. **MarkArea prefix** (Patches.cs:121-156). Admitting plates without reworking it drops their footprint and
   tile marks (Q1). This is the main breakage.
2. **End placement.** Vanilla refuses ends on any 29 occupant. The jumper check must keep the
   tile, door and edge checks.
3. **Tile-over-end hole.** Vanilla stops a later solid tile or plate centre landing on a plate end only because
   the plate holds slot 29 there (`IsValidTileLocation` checks 29 for `HighWattBridgeTile`). Under
   mark-if-free, a plate end that shares a cell with a wire bridge end does not hold 29. A Tile or another
   plate's centre could then be built onto that end and bury the end node in solid. Port cells are safe,
   because the owning building is on the Building layer. Needs either a `BridgeFootprint`-aware check in
   `IsValidTileLocation`, or a ban on plate ends sharing a cell with bridge ends.
4. **Deconstruct / cancel / prioritize.** Vanilla reaches the plate at its **centre** (Building layer) and at
   its ends (29). That differs from wire bridges, which are reachable at their ends only. `SweptBridges.Missed`
   (Patches.cs:291-299) only adds ends that the sweep missed, so the centre keeps working as in vanilla. Tests
   must not copy the wire-bridge "middle does not mark" assertion (Power.cs:141).
5. **Digging and sim replacement on deconstruct:** centre only and handled by vanilla. The mod touches neither.
   A node never sits in the solid cell.
6. **Mixed-rating rings.** `ElectricalUtilityNetwork.GetMaxSafeWattage` returns the rating of the lowest group
   that has any wire or bridge. If plate ends may share cells with wire bridges, the circuit drops to 1 kW or
   2 kW, and overload hits the weakest bridge. That matches what a wire between them would do in vanilla.
   Today the mod refuses that sharing (Patches.cs:192, 221).

**Design options for the solid centre** (not chosen; for the user)
- **A. Keep the vanilla solid centre and only make the ends jumper.** No change to solidity, seals, replacement
  or deconstruct. The smallest change. Consequences:
  - The centre cell must be free and tile-able. Two ports two cells apart in a row with an empty cell between
    them is the case that fits.
  - The plate can still replace an existing tile, so a plate can be set into a wall with its ends on ports
    either side.
  - Needs item 1, items 2-3 and a decision on item 6.
- **B. Strip the solid centre for every plate:** drop SimCellOccupier and IsFoundation, and move ObjectLayer to
  WireConnectors. This changes a vanilla building for every player who has the option on.
  - Gas and liquid seals made with plates stop sealing.
  - On existing saves, the sim cell already holds solid metal that no SimCellOccupier will clear. It stays
    behind as natural tile, or the save desyncs.
  - Removing the mod later reverses it.
  - Contradicts "unchanged" wording everywhere.
- **C. Make the centre non-solid only when an end sits on a port.** Placement-dependent solidity. The state
  flips when a port building is built or removed later, which needs runtime SimCellOccupier add and remove and
  re-digging. Complex, and the same seal and save issues as B on a smaller scale.
- **D. A separate "jumper joint plate" building** (cloned def, non-solid or solid). Leaves the vanilla plates
  alone. Costs a new building ID, art, research entry, strings and translations, and save dependence on the mod.
  It does not fit the mod's "nothing extra to build" pattern.

## Q4: tests

`src/JumperBridges.Tests/` uses `Isochronous.TestKit` `[OniTest]` coroutines (csproj references TestRunner).
The files are Signals, Gates, WireDrags, WireUnderEnd, Deconstruct, Power and Preview (explicit). The power
suite (`Power.cs`) is the template:
- `BatteryBridgeLamp` (46-71)
- `LampOntoBridgeEnd` (74-93)
- `DragFromEnd` (95-121)
- `DeconstructBetweenPorts` (124-145)

`ValidWithPreview` (31-44) runs the placement check with a live preview object. This matters because
`World.Build` passes no source object and so skips the port checks the mod changes.

Plate tests, representative, one per behaviour:
- **Battery to plate to lamp.** Plate centre at `r.Cell(2,1)`, no floor needed. Assert one circuit, no
  `HasInvalidPorts`, and that `Grid.Solid`/`Grid.Foundation` are true at the centre.
- **Lamp placed onto a plate end.**
- **`HighWattageWire` drag from a plate end.**
- **Deconstruct.** Marked from the centre and from an end. After completion the centre is no longer solid and
  no node is left.
- **A plate end in a tile is still refused.** Plus, if option 3 is kept, a Tile on a plate end that shares a
  cell with a bridge end is refused.
- **Conductive plate.** Its `requiredSkillPerk` only matters when it is not instant-built. One test with
  `WireRefinedBridgeHighWattage` for the art and the 50 kW rating covers the second prefab.
- **Mixed-rating limit.** If plate and bridge ends may share a cell, check that `GetMaxSafeWattageForCircuit`
  falls to the lower rating.

Run with `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Filter "*joint plate*"`
(README.md:56-60).

## Q5: options, strings, translations that list the kinds

- `ModStrings.cs:21-25`, POWER.TOOLTIP: "…Heavy-watt joint plates are unchanged…". It needs rewording, plus a
  new `OPTIONS.<X>.NAME/TOOLTIP` pair if the plates get their own option.
- `Options.cs:10-22` (a new bool if separate). `Mod.cs:15-24` (Enabled wiring and the log line at 23-24).
- `translations/JumperBridges.pot:19-27`, and `ko.po`, `ru.po`, `zh.po` line 27 hold the same msgid, which will
  go stale.
- `README.md`: lines 3, 19, 23 and 29 (the patch list mentions joint plates twice), plus the testing summary at
  line 62.
- `CHANGELOG.md:6` says that joint plates are unchanged.
- `BridgeKind.cs` doc comments 26-31 and 106. Patches.cs comments 46-50, 78, 159-161 and 206-209.
- `publish/workshop-description.txt` says only "automation or power bridge", so no list needs changing.

## Store records checked against the code

- No pointer record is wrong against the code. These were all accurate: `blk-e978d080073f` (BridgeKind,
  including "the heavy-watt joint plates have their own rule and are left alone"), `blk-a88b0d39bc11`
  (patch list), `blk-f00f2e956606`, `blk-67d61b61d321`, `blk-a0f2a41a1467`, `blk-c5d470758241`,
  `blk-45611b9a4ffc`, `blk-f92ca7ddeb23`, `blk-852815eed994`, and the glossary's `blk-279f9dc39e51`
  ("joint plate").
- All of those become stale once the plates are added: BridgeKind's `Of` and NodeAnim, the patch list, Options,
  the glossary entry, and the overview `blk-2d2edc3c3db1`.
- A code comment, not a store record, is the one wrong-in-general claim: Patches.cs:129-130 says MarkArea
  "does nothing else" for bridge rules. That holds for `WireBridge`/`LogicBridge` but would be false for
  `HighWattBridgeTile`.
- Brief quality: 22 of the 39 pointers were other mods' `code-style.md` "Line Length" and "Harmony Patches"
  sections that name jumper-bridges files. Only the jumper-bridges copies (`blk-7cc833c742eb`,
  `blk-27d655025249`) apply. The glossary's joint-plate record `blk-279f9dc39e51`, which is the most relevant
  record to this task, did not appear in the shown part of the brief.
