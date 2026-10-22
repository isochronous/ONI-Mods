# Unit 11 report — Sweep Zones keeps player-cancelled sweeps cancelled

## How re-marking works

- `ZoneStore.Sim1000ms` (mods/sweep-zones/src/SweepZones/ZoneStore.cs:121) calls `MarkSweepZones` (:125) once a second. It walks every sweep-zone cell's `Pickupables` stack and calls `Clearable.MarkForClear` (:161) on each item unless it is a Duplicant, already carries `GameTags.Garbage`, or is not clearable.
- Before this change, nothing remembered a cancel: `CancelClearing` removes `Garbage`, so the next pass marked the item again within a second.

## How a player cancel reaches debris (game code, Assembly-CSharp U59)

- Cancel tool: `CancelTool.OnDragTool` calls `gameObject.Trigger(GameHashes.Cancel)` (2127324410) on the first object in each layer of the cell. `Clearable.OnCancel` (subscribed in `Clearable.OnPrefabInit`) walks the pile's `objectLayerListItem` chain and calls `CancelClearing` on each item.
- Item user menu: the "Cancel Sweep" button calls private `Clearable.OnClickCancel`, which calls `CancelClearing`.
- `CancelClearing` is also called by the game itself from `OnStore` (picked up / stored) and `OnEquipped`; those are not player cancels.
- `KPrefabID.AddTag(tag, serialize: true)` puts the tag in `serializedTags`, so it is saved with the item; `RemoveTag` drops it from both sets.

## What "stays cancelled" means

A pile the player cancelled stays cancelled until a Duplicant picks it up (`Clearable.OnStore`) or it is marked again (Sweep tool, the item's button, or vanilla `OnAbsorb` re-marking it after it merges with marked debris). Chosen because each of those is a point where the game already treats the pile as changed, it needs one saved tag on the item and no per-cell bookkeeping, and redrawing the zone would not reach the item without tracking cells. Cancels by code (`CancelClearing` called directly) are not player cancels and still get re-marked.

## The change

- `ZoneStore.PlayerCancelled` tag (ZoneStore.cs:22); `MarkSweepZones` skips items carrying it (:150).
- Four Harmony postfixes in src/SweepZones/Patches.cs: `Clearable_OnCancel_Patch` (:65, tags the whole pile, saved), `Clearable_OnClickCancel_Patch` (:77, tags the item, saved), `Clearable_MarkForClear_Patch` (:87, removes the tag once marked), `Clearable_OnStore_Patch` (:98, removes the tag on pickup).
- CHANGELOG Unreleased bullet; README "Per-cell priority" bullet gets one sentence. No ModStrings changed, so no translations run needed.

## Test

- New: `Sweep Zones: debris the player cancels stays cancelled in a sweep zone until it is marked again` (`Marking.PlayerCancelSticks`, src/SweepZones.Tests/Marking.cs:70). Cancel tool event, 3 game seconds, still unmarked; mark again lifts the tag; item's Cancel Sweep button, 3 game seconds, still unmarked. Not run (no game launch in this unit).
- Retitled `ZoneLifecycle` (Marking.cs:40): its direct `CancelClearing` step now reads as a cancel by code, which still comes back; logic unchanged.

## Build output

```
dotnet build src/SweepZones/SweepZones.csproj -c Release -p:ModDeployFolder=none
Build succeeded.
    0 Warning(s)
    0 Error(s)

dotnet build src/SweepZones.Tests/SweepZones.Tests.csproj -c Release -p:ModDeployFolder=none
  Merged PLib into SweepZones.dll
  SweepZones.Tests -> C:\dev\ONI-Mods\mods\sweep-zones\src\SweepZones.Tests\bin\Release\SweepZones.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## For the coordinator

- `mods/sweep-zones/.context/domains/entities.md` Business Rules (blk-a6ab594b39d9) and the Patches table (blk-8ed739708efc) need the new skip and the four Clearable patches; this unit has no write path.
