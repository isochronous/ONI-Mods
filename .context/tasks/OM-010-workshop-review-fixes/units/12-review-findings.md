# Unit 12 — review findings: Sweep Zones keeps player-cancelled sweeps cancelled (4e444dd)

Passes run: 1 (correctness), 3 (integration), 4 (clarity), 5 (evidence). Pass 2 has no trust boundary here (no input, files or network).
Game code read from a full ilspycmd decompile of Assembly-CSharp.dll (Clearable, Pickupable, ElementSplitterComponents, CancelTool, Movable, AttackTool, WorldContainer, Storage, SolidConduitFlow) and KPrefabID from Assembly-CSharp-firstpass.dll.

## Moderate

### M1. New debris that lands on a cancelled pile is absorbed into it and never swept (Pass 1)
`mods/sweep-zones/src/SweepZones/Patches.cs:64-73` with `ZoneStore.cs:149-150`.
When a pickupable lands, `Pickupable` (cell-change handler) calls `existing.TryAbsorb(arriving)`: the pile already in the cell absorbs the newcomer and the newcomer is deleted. The surviving object keeps its own tags. Freshly dug or dropped debris is not marked yet, so `Clearable.OnAbsorb` does not call `MarkForClear`, and the tag stays.
Scenario: the player cancels the sweep of a sandstone pile in a zone. Dupes dig sandstone above it. Every chunk that falls there merges into the cancelled pile, which grows and is never swept. This contradicts README line 10, "Newly arriving debris is marked within a second".
The reverse case also changes the result: when already-marked debris falls on the cancelled pile, `OnAbsorb` → `MarkForClear` lifts the player's cancel. The code comment (`Patches.cs:85`, "merged with marked debris") says this, but README and CHANGELOG ("until a Duplicant picks it up or you mark it again") do not. A tagged item that falls onto an untagged pile loses its tag when it is absorbed (also undocumented, but harmless).
Possible fix: a prefix/postfix on `Clearable.OnAbsorb` (or `Pickupable.Absorb`) that lifts `PlayerCancelled` from the absorber when the absorbed item does not carry it.

### M2. Cancel actions that are not "cancel this sweep" also tag debris for good (Pass 3)
`Patches.cs:64-73`. The `OnCancel` postfix tags every item in the pile on any `GameHashes.Cancel` (2127324410), whether or not it was marked for sweeping. Callers that reach debris:
- **Cancel tool with its default "All" filter** (`CancelTool.OnDragTool` triggers Cancel on the first object in every layer of each dragged cell). A player who cancels dig orders, buildings or mop errands across a zone silently turns off sweeping for all debris in those cells, including debris that arrived less than a second ago and was never marked.
- **Move To on debris** (`Movable.MarkForMove` calls `Trigger(Cancel)` on the moved item; ore has `Movable`, see `EntityTemplates` base ore template). Vanilla `Clearable.OnCancel` walks from this item to the end of the pile list, so the other items in the pile are tagged as a side effect and stay unswept. If the player then cancels the Move To, the item itself also stays tagged. Before this change the zone re-marked all of them within a second.
- `WorldContainer.CancelChores` (called from `ClusterManager.DestroyWorld`) tags every pile in a world that is being destroyed. Low impact.
The tag is never shown to the player, and it is set outside zones too. Debris tagged by an old cancel drag is skipped when a zone is painted over it later.
Possible fix: tag only items that were marked when the cancel ran (read `isMarkedForClear` in a prefix and pass it through `__state`), and skip when the Cancel came from `Movable`. Or decide that this is acceptable and document it. Jeremy should decide which.

## Minor

### m1. The test marks and cancels in the same frame (Pass 5)
`mods/sweep-zones/src/SweepZones.Tests/Marking.cs:89-92`: `MarkForClear()` and then `OnClickCancel` with no game time between them. The memory rule "tests at human speed" asks for game time between steps. Add `yield return t.WaitGameSeconds(1)` after line 89.

### m2. Test coverage gaps (Pass 5)
The assertions check real results (the Garbage-backed `Marked` state over 3 game seconds, and the tag being lifted). Good. Not covered: lifting the tag on pickup (`OnStore`), merging (M1), and the tag surviving save/load. The Cancel-tool step calls `Trigger` on the head of the pile, not `CancelTool.OnDragTool`. That is close enough, but the pile has only one item, so the walk over the whole pile is not exercised. A second item dropped in the same cell (a different element, so it does not merge) would cover it.

### m3. A partial pickup leaves the remainder cancelled (Pass 1, documentation)
`ElementSplitterComponents.OnTake` spawns a fresh, untagged object for the taken part. The remainder keeps the tag. "Until a Duplicant picks it up" is true only for the part that was picked up. Fine as behaviour; the README wording could say "picks it up" for clarity, or leave it as is.

## Checked, no finding
- Player cancel paths: Cancel tool (`OnCancel`, whole pile from the head item, same walk as vanilla) and the item's Cancel Sweep button (`OnClickCancel`) are both hooked. The Priority tool does not cancel. Deconstructing a storage building drops items that were already untagged by `OnStore`. `AttackTool`, `HarvestTool`, `Moppable`, `Diggable` and `Constructable` Cancel triggers hit non-debris objects.
- Re-mark: `MarkForClear` postfix reads `isMarkedForClear` after the call, so a refused mark (entombed or stored) keeps the tag. Load of a marked item (`OnSpawn` → `MarkForClear(restoringFromSave)`) lifts it, which is consistent.
- Pickup: `OnStore` is raised by `Storage.Store` (Duplicant, Auto-Sweeper, any storage) and by `SolidConduitFlow` when an item enters a conveyor, so the tag is lifted on all of these.
- Save/load: `AddTag(serialize: true)` puts the tag in `KPrefabID.serializedTags`, restored in `OnDeserializedMethod`; `RemoveTag` also removes it from that set. A save loaded without the mod keeps an inert tag name on the item and loads with no error.
- `___pickupable` is `[MyCmpReq]`, so the postfixes cannot see null. No try/catch is needed under the error-handling standard.
- CHANGELOG "Unreleased" bullet and README sentence match each other and the code, except for the merge behaviour in M1.
