# Unit 09 review: fbe882a, heavi-watt art on shared ends and review 07 fixes

Reviewed `git show fbe882a -- mods/` (9 files) against the current sources and a fresh `ilspycmd -t` decompile
of `KBatchedAnimController`, `KAnimControllerBase` (Assembly-CSharp-firstpass) and `KAnimGraphTileVisualizer`
(Assembly-CSharp). Read-only: nothing built, no game launch. Passes run: 1 correctness, 2 security,
3 integration, 4 clarity, 5 evidence.

**Verdict: no critical or moderate findings.** The re-art is sound on every add and remove path. Two minor
findings follow.

## What was checked

- **SwapAnims leaves the controller in the right state** (KBatchedAnimController.cs:271-293). It deregisters,
  clears `batchGroupID`, and `LoadAnims` -> `SetBatchGroup` rebuilds `curBuild` and the symbol GPU data for the
  new file. It then re-applies `hiddenSymbolsSet` (the outline stays hidden; the explicit
  `SetSymbolVisiblity(Wire.OutlineSymbol, false)` at BridgeEndNode.cs:243 does no harm but repeats what
  SwapAnims already did), re-resolves `curAnim` by name in the new file and registers again.
  - **Tint** lives in `batchInstanceData`, which is created once in `OnAwake` (line 97) and is never rebuilt
    by a swap, so the power overlay colour survives. `ApplyPowerOverlay` also sets it again on every overlay
    `Update`.
  - **Layer**: `Register` uses the current `gameObject.layer`, so an end that the overlay lifted stays lifted.
  - **Connection frame**: `Refresh()` (KAGTV.cs:80-101) calls `Play(GetVisualizerString(cell))`, and `Play`
    always does Stop + Queue, so the segment is replayed from the new file.
- **Every add and remove path reaches UpdateArt.**
  - Create -> OnSpawn (line 121).
  - Adoption in the same tick as creation: `AddOwner` returns early because `!registered`, and OnSpawn then
    picks up the adopted owner.
  - Adoption later (`EnsureNode` -> `AddOwner`).
  - Deconstruct (`JumperBridge.OnCleanUp` -> `RemoveNodes` -> `Disown`).
  - `BuildingBroken` -> `RemoveNodes`, and repair -> `ScheduleCreate` -> `AddOwner`. A broken plate hands the
    art to the wire bridge until it is repaired, which matches "while a plate touches it".
  - **Load**: either `CreateNodes` order now gives the same art, so a load no longer depends on spawn order.
  - **Under construction**: `JumperBridge` is added only in `DoPostConfigureComplete` (Patches.cs), so a queued
    bridge never owns a node and has no art to change.
  - `Release` and `Evict` need nothing: the node goes away.
- **No re-art of logic nodes.** `Kind != BridgeKind.Power` returns first (line 228). README:118 (the ribbon
  shared-end check) still describes the logic behaviour correctly.
- **Selection logic** (lines 230-238). I walked these cases:
  - A plate arriving on a node a wire bridge created: it swaps to the plate's art.
  - A second plate: the first plate's art stays.
  - The first plate leaving while a second plate and a wire bridge remain: it goes to the second plate.
  - The plate leaving: it goes to the remaining wire bridge.
  - A destroyed `artOwner`: Unity null fails the `!= null` check, and a new owner is chosen.
  - The early return when `AnimFiles[0]` already matches avoids a needless deregister and register.
- **Side effect for two wire bridges sharing an end** (for example plain and conductive): when the bridge that
  created the node leaves, the node now takes the remaining bridge's art. Before, it kept the stale art until a
  reload. The README and checklist text agreed with neither behaviour, and the new behaviour matches the stated
  rule, so this is an improvement, not a regression. One related point is out of scope and was not changed:
  the node's `PrimaryElement` still comes from the creating bridge (line 77).
- **Test `WireBridgeOnPlateEnd`** (JointPlates.cs:152-196).
  - It is paced: `WaitGameSeconds(0.5)` comes after every build and after the destroy.
  - It asserts outcomes: the slot holder, a single circuit, the tile refusal with WIRE_OBSTRUCTION (this is the
    `IsValidTileLocation` prefix path that review 07 m3 asked for), the art by `AnimFiles[0]` reference, node
    identity after the destroy, and the circuit after the destroy.
  - `Plate` is `WireBridgeHighWattage`, so the expected `utilities_electric_insulated_kanim` is right.
- **Comments (m4).** The `PowerPorts` summary now matches the class: it holds only `IsBridgeEnd`.
  "whatever was built there first does" is accurate, because a port or bridge placed later never takes slot 29
  (`MarkIfFree`). The `UpdateArt` summary states the rule, including the case without a plate.
- **Tooltip and translations (m1).** The en, ko, ru and zh texts say the same thing. "which is then connected"
  now refers only to the port. The text has no hedging.
- **Security (pass 2).** There is no trust boundary or input handling. Nothing found.
- **Evidence (pass 5).** The unit 08 report shows the build (0 errors) and the translations check
  (ALL CHECKS PASSED). The in-game tests were not run, so `WireBridgeOnPlateEnd` has not been seen green yet.

## Findings

### m1 (minor, pass 4): README still says a wire bridge end always draws as its own wire
`mods/jumper-bridges/README.md:19` says a wire bridge end "draws as a piece of the wire the bridge is the bridge
of (plain, conductive or insulated)". After this commit that is false on a cell it shares with a joint plate's
end: the end draws as heavi-watt wire. README:21 was updated for the plate's side only.

Failure scenario: a player puts a Wire Bridge end on a plate end, sees heavi-watt art, and the README says
otherwise.

Fix: add the counterpart clause to line 19, for example "...(plain, conductive or insulated), or as the plate's
heavi-watt wire where it shares a cell with a joint plate's end". The store record `blk-a0f2a41a1467`
(BridgeEndNode) also lacks the art rule; that belongs to the plan's store-notes step.

### m2 (minor, pass 5): the swap on adoption, and a non-empty segment after a swap, are untested
`WireBridgeOnPlateEnd` builds the plate first, so the node is created with plate art, and the first art
assertion (JointPlates.cs:175) passes without any swap. The only swap it exercises is the one on `Disown`. Three
things have no test:

- **The swap on adoption.** This is the `AddOwner` -> "plate present, artOwner is not a plate" branch
  (BridgeEndNode.cs:231-242), reached when the wire bridge is built first. `OnWireBridgeEnd`
  (JointPlates.cs:122-150) covers exactly that order but asserts no art.
- **The replayed segment.** Neither test drags a wire into the shared end, so the end's connections are empty
  and `Refresh` replays the empty-connection anim. Nothing checks that a real segment ("L", "LR") is replayed
  from the new file after `SwapAnims`.
- **The tile after the plate is removed.** Review 07 m3 also suggested checking that a tile is allowed on the
  wire bridge's end once the plate is gone; the new test stops before that.

Failure scenario: a regression in the adoption branch (for example inverting the early-return condition at line
231) leaves a wire-bridge-first shared end drawn as thin wire, and every test still passes.

Fix (one representative case per path): in `OnWireBridgeEnd`, after the plate is built, assert
`AnimFiles[0] == utilities_electric_insulated_kanim`. Optionally, in `WireBridgeOnPlateEnd`, drag a wire into
the shared end before the destroy and assert that the controller's current anim name equals
`GetVisualizerString(sharedCell)` afterwards, then assert that a tile preview on the shared cell is valid.

## Brief feedback
- The 20 or so "Line Length" and "Harmony Patches" pointers from other mods did not apply; only the
  jumper-bridges copies did.
- `blk-a0f2a41a1467`, `blk-c5d470758241` and `blk-e978d080073f` were the useful pointers.
- Missed: nothing covered the test project (`src/JumperBridges.Tests`) or the vanilla anim controller
  behaviour; the decompile covered both.
