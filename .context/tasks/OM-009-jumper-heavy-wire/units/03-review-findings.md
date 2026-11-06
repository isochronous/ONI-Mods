# Unit 03 review findings: heavy-watt joint plates (000e870)

Diff reviewed: `git diff 0e97c6c..000e870 -- mods/jumper-bridges/`, every changed file (BridgeKind.cs, Patches.cs,
ModStrings.cs, the .pot and 3 .po files, JointPlates.cs, Power.cs, README.md, CHANGELOG.md,
publish/workshop-description.txt). I read the vanilla code fresh with `ilspycmd -t` (9.1) against
`F:/SteamLibrary/.../Managed/Assembly-CSharp.dll`, the method from units/01-explore-findings.md. The output is in
this session's scratchpad `decomp/`. Vanilla line numbers below refer to that `BuildingDef.cs` decompile.
I ran all five code-quality-rules passes. Pass 2 (security) has nothing to check: there is no trust boundary,
input parsing, secret or I/O in this diff.

**Verdict: no critical findings.** The behaviour matches the four decisions in plan.md. Two moderate findings
are test gaps. The rest are minor.

## Moderate

### M1. Plate end on another plate's end is allowed but untested (Pass 5)
- `Patches.cs:287-298` (IsValidHighWattBridgeLocation prefix) now accepts a plate end on a cell where another
  plate holds WireConnectors. Vanilla refuses any 29 occupant (decompile 1525-1529). The CHANGELOG
  (`CHANGELOG.md:5`) and README (`README.md:21`) advertise this.
- No test in `JointPlates.cs` covers it. The suite has no test of two plates meeting at one cell, the ring that
  BridgeLinks writes for them, or the circuit through them.
- Failure scenario this would catch: a regression in `HasEnd`'s `jointPlate` filter, for example the `==`
  inverted at `Patches.cs:229`. It would refuse plate-on-plate, or allow plate-on-wire-bridge, while all five
  current tests still pass for the plate-on-wire-bridge direction. Only `NotOnWireBridgeEnd` pins the filter,
  and only for the refusal side.
- Suggested test: two plates sharing a free middle cell between a battery and a lamp. Assert the lamp is
  powered and a single node sits at the shared cell.

### M2. The replacing prefix's kept vanilla refusals are untested (Pass 5)
- `Patches.cs:280-308` replaces vanilla `IsValidHighWattBridgeLocation` in full. I checked it line by line
  against the decompile (1510-1550) and it is equivalent:
  - null source or no link: valid
  - `AreCellsValid`: INVALID_CELL
  - an end on FoundationTile, `Grid.HasDoor`, or a door under construction on Building: HIGHWATT_NOT_IN_TILE
- The one intended difference: vanilla's refusal of any WireConnectors occupant is narrowed to "a wire bridge
  end".
- No test asserts any of the kept refusals. Unit 01 Q4 listed "A plate end in a tile is still refused", but it
  was not written.
- Failure scenario: a later edit drops `InTileOrDoor` or reorders its checks. Plates could then be placed with
  an end inside a wall or door, which the base game forbids, and no test fails. One representative test (a
  plate preview with an end on a Tile, expecting HIGHWATT_NOT_IN_TILE) covers the path.

## Minor

### m1. Option tooltip claims more sharing than plates allow (Pass 4, player text)
- `ModStrings.cs:24`, the same text in `JumperBridges.pot`, and `zh.po`/`ko.po`/`ru.po:27-28`: "...or Heavi-Watt
  Joint Plate can be placed with either end directly on a power port or another bridge's end".
- A plate end on a wire bridge's end is refused (decision 2), and so is the reverse. Scenario: a player reads
  the tooltip, tries a plate end on a Wire Bridge end, and gets "connector overlap". The tooltip also names only
  the base plate, while README:3 and :21 name both plates.
- `publish/workshop-description.txt:3` has the same gap. It names only "Heavi-Watt Joint Plate", and "Bridges
  can be daisy-chained, end on end" does not hold across plate and wire bridge.
- Per the Workshop style memory, state the rule plainly. Example: "...or on another bridge of the same kind's
  end" / "a joint plate's end on another joint plate's end".

### m2. Unit 02's claimed NRE is real, but narrower than reported and missing from the changelog (Pass 1/5)
- **Verified against the decompile.**
  - Vanilla `IsValidTileLocation` (decompile 795) does `Grid.Objects[cell, 27].GetComponent<Building>().Def`
    with no null check.
  - `BridgeEndNode` registers on WireTile (27) (`BridgeEndNode.cs:114`) and has no `Building`.
  - So a Tile preview over a power end node throws a NullReferenceException from the build tool's validity
    check. A plate centre over one throws too, via the `HighWattBridgeTile` switch (decompile ~719).
- **Narrower than "any power end node".** On a cell where a building still stands (an end on a live port),
  IsAreaClear's Building-layer occupancy check (decompile 597-606) breaks out with OCCUPIED before the Tile-rule
  call at 661. So the throw needs a node on a cell with nothing on Building: the shared end of two daisy-chained
  Wire Bridges in open space, or an end whose port building was deconstructed.
- **Pre-existing since 0.2.0** for 1 kW wire bridges. The fix is user-visible (the tile could not be placed and
  the log was spammed), but `CHANGELOG.md:3-5` has no bullet for it.
- **Partly untested.** The 1 kW case is now allowed: the node counts as a wire, as a wire under a tile is in
  vanilla. No test covers it. `NoTileOnEnd` reaches the prefix only on a plate node, where the postfix then
  refuses anyway.

### m3. A list allocation on every tile validity check (Pass 1, performance)
- `Patches.cs:351-358`: the `IsValidTileLocation` postfix calls `PowerPorts.HasEnd`, which calls
  `BridgeFootprint.EndsAt` (`BridgeFootprint.cs:29-37`). That allocates a new `List<GameObject>` whenever the
  cell is in `byCell`, and does a dictionary lookup every time.
- `IsValidTileLocation` runs per cell per mouse move while dragging tiles, doors or plates. Most cells miss
  `byCell`, so the cost is a lookup. This is GC churn only near bridges. A non-allocating `AnyEndAt(cell,
  predicate)` would remove it. Low impact.

### m4. Full-method replacements drop any future vanilla check (Pass 3/4)
- `Patches.cs:280-298` (IsValidHighWattBridgeLocation) replaces the vanilla method on every call.
- `Patches.cs:325-349` (IsValidTileLocation, on node cells) re-implements vanilla checks 2 and 3.
- Scenario: a Klei update adds a fourth refusal to either method. It is skipped for plates, or on node cells,
  and nothing reports it. ModCheck verifies signatures, not bodies.
- This is the same pattern as the existing IsValidWireBridgeLocation and ArePowerPortsInValidPositions
  prefixes, so it is not a new deviation. Noted so the release checklist re-diffs these two bodies on game
  updates.

### m5. Stale inherited connections at a plate centre (Pass 3, pre-existing pattern, widened)
- `Patches.cs:600-604` with `JumperBridge.cs:58-62`. When a wire leaves a cell, `JumperBridge.At` includes
  bridges whose *middle* is on the cell, so `Inherit` stores that wire's connections for the cell. Only a node
  spawning there later consumes them (`BridgeEndNode.cs:118`).
- A plate centre is now such a middle, and a 1 kW wire can run through the plate's tile. Scenario:
  1. A wire runs through a plate centre.
  2. The wire is deconstructed, and the entry is stored.
  3. Later the plate is removed, and a Wire Bridge end is placed on that cell.
  4. Its node inherits the old wire's bits and joins neighbours no drag joined it to. That breaks the README
     rule "a bridge end connects only to cells a wire drag joined it to".
- The same path already exists for the 3x1 Wire Bridge middle. Fix it once by inheriting only when some bridge
  in `bridges` has an *end* on the cell.

### m6. BatteryPlateLamp asserts less than its wire-bridge twin (Pass 5)
- `JointPlates.cs:18-45` omits two checks that `Power.cs:55` and `:71` make:
  - the "no circuit before the bridge" baseline
  - the `InvalidPortReporter.portsNotOverlapping` operational flag
- Without the baseline, the test cannot show that the plate is what connected the lamp. Two lines would add
  both.

### m7. Store records now stale (completion phase, not code)
- `blk-e978d080073f`: BridgeKind says the plates "are left alone".
- `blk-a88b0d39bc11`: the patch list is missing the WireBridgeHighWattageConfig postfixes and
  `IsValidTileLocation`.
- `blk-852815eed994`: the tests list is missing `JointPlates.cs`.
- Unit 01 also named the glossary `blk-279f9dc39e51` and overview `blk-2d2edc3c3db1`.

## Checked and correct (focus items)

- **MarkArea prefix** (`Patches.cs:155-196`):
  - For `HighWattBridgeTile`, `MarkFootprint` reproduces vanilla's footprint loop (decompile 829-843) on
    whatever layer the caller passes, `Uprootable` move to Plants included.
  - BuildingComplete calls it with Building, then FoundationTile (`BuildingComplete.OnSpawn` 162/165).
    Constructable calls it with Object or Replacement, then TileLayer if free (`Constructable.MarkArea`
    449/456). So the centre is registered on both layers as in vanilla.
  - The plate's ends are marked if free. The plate has no conduit or power ports and no ISecondary* ports, so
    skipping the rest of vanilla loses nothing.
  - Vanilla's `MarkOverlappingPorts` tagging of the port building is the one intentional omission.
  - `Constructable.FinishConstruction` unmarks (260) before `Def.Build` (261), so the complete plate gets the
    free end slots.
  - `UnmarkArea` clears 29 only when `== go` (decompile 1027-1037), so mark-if-free is safe on removal.
- **IsValidHighWattBridgeLocation**: equivalent to vanilla except the intended narrowing (M2 above).
  `AreCellsValid` runs before any grid access.
- **No sharing, both directions**:
  - The wire bridge prefix (`Patches.cs:243-261`) and the plate prefix both use `HasEnd`. It checks the
    WireConnectors holder and `BridgeFootprint.EndsAt`, so an end that gave its slot to a port is still found.
  - Under-construction bridges and plates carry `BridgeFootprint`, so queued ones count.
  - `self` excludes the source.
  - Tile and plate centre over a plate end whose slot holder is gone: refused by the postfix.
  - A plate centre over a plate end on 29: refused as in vanilla.
- **Deconstruct, cancel, prioritize**:
  - A plate's centre is on Building, so vanilla `DeconstructCell` (all 45 layers) reaches it there.
  - `SweptBridges.Missed` adds only ends whose slot is held by something else, and `EndsAt` excludes the
    middle, so nothing is marked twice.
  - The filter layer comes from `GetFilterLayerFromGameObject` (plate: Building, so "Buildings"), the same as
    vanilla.
- **Existing wire-bridge behaviour**:
  - Unchanged except two places: the new refusal of a wire bridge end on a plate end that sits on a port (an
    intended reverse fix), and the NRE fix (m2).
  - `Of()`/`OfBridge` callers (`JumperBridge`, `BridgeFootprint`, the OnConnect/OnDisconnect patches,
    `WireKinds`) need no other change.
  - The node anims match vanilla: `HighWattageWire` uses `utilities_electric_insulated_kanim`, and
    `WireRefinedHighWattage` uses `utilities_electric_conduct_hiwatt_kanim`.
- **Prefab patches**: `WireRefinedBridgeHighWattageConfig` overrides only `DoPostConfigureUnderConstruction`
  (it calls base first) and `AddNetworkLink`. So the two postfixes on `WireBridgeHighWattageConfig` cover both
  plates, and the link exists when they run.
- **Tests**:
  - Pacing: game-time waits after each build or destroy, and the same-frame setup builds match the existing
    `Power.cs` precedent.
  - Assertions check outcomes: grid layers, circuit ID and limit, `IsPowered`, exact fail-reason strings, anim
    file, `IsMarkedForDeconstruction`.
  - `DeconstructFromCentre` correctly does not copy `Power.cs:141`.
  - I traced each test's cell layout against the vanilla footprints and offsets. The plate is 1x1 with ends at
    plus or minus 1, and the Wire Bridge's ends are plus or minus 1 from its origin.
- **Player text**: no hedging. Names match vanilla STRINGS ("Heavi-Watt Joint Plate", "Heavi-Watt Conductive
  Joint Plate"). The changelog bullet is short and plain. Accuracy issues are in m1.

## Brief feedback (handoff-3 section 2)
- About 22 of the pointers shown were other mods' `code-style.md` "Line Length" and "Harmony Patches" sections.
  They do not apply. Only `blk-7cc833c742eb` and `blk-27d655025249` do.
- The glossary joint-plate record `blk-279f9dc39e51` did not appear, the same miss unit 01 reported.
- `blk-e978d080073f`'s line "the heavy-watt joint plates ... are left alone" no longer matches the code (m7).
