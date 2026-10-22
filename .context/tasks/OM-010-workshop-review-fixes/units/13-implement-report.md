# Unit 13 report: Sweep Zones review fixes (M1, M2, m1)

Game code read from an ilspycmd decompile of Assembly-CSharp (Clearable, Movable, CancellableMove, Cancellable, CancelTool, Pickupable).

## Changes

- **M2: only a cancelled sweep in a zone is remembered** (`src/SweepZones/Patches.cs`, `ZoneStore.cs`).
  - New `ZoneStore.IsMarkedInSweepZone(pickupable)`: the item carries `GameTags.Garbage` (marked) and its cell is in `sweepCells`.
  - `Clearable_OnCancel_Patch`: the prefix collects the pile items that are marked and in a sweep zone before vanilla cancels them (`__state` list). The postfix tags only those. A Cancel that reaches unmarked debris (for example a Cancel drag over digs or buildings, with debris that was not marked) tags nothing.
  - `Clearable_OnClickCancel_Patch`: same prefix/postfix rule for the item's Cancel Sweep button.
  - New `Movable_MarkForMove_Patch` (prefix `Running++`, finalizer `Running--`): `Movable.MarkForMove` triggers Cancel on the item, and `OnCancel` skips tagging while it runs. Cancelling a Move To goes through `CancellableMove.OnCancel` → `ClearMove` and never sends Cancel to the item, so nothing else is needed for it.
  - "A zone painted later sweeps everything in it": `ZoneStore.SetSweep` now lifts the tag from every item in the painted cell. Without this, a tagged pile that fell out of its zone (floor dug), or one under a zone that was erased and painted again, would stay skipped.
- **M1: merging lifts the cancel.** New `Clearable_OnAbsorb_Patch` postfix removes `PlayerCancelled` from the absorbing pile whatever was merged in. Vanilla `OnAbsorb` already marks the pile when the merged debris was marked. The next zone pass marks the whole pile.
- **Docs:** README "Per-cell priority" and the CHANGELOG Unreleased bullet now say: debris in a sweep zone whose sweep you cancel stays cancelled until a Duplicant picks it up, you mark it again, more debris lands on the pile, or you paint the zone over it again. README adds: "Other orders on that debris, such as Move To, do not count as cancelling its sweep." README Implementation notes now list the Clearable and `Movable.MarkForMove` patches; it said "Four Harmony patch points", which was already out of date.
- No ModStrings changed, so no translations run.

## Tests (src/SweepZones.Tests/Marking.cs; built, not run: no game launch in this unit)

- `PlayerCancelSticks`, retitled "Sweep Zones: debris the player cancels stays cancelled in a sweep zone until it is marked again or the zone is painted over it". It is now paced: 1 game second after marking, after the cancel and after the re-mark, before each check. It now ends by painting the zone again and waiting for the mark.
- New `MergeLiftsCancel`: "Sweep Zones: debris landing on a pile whose sweep the player cancelled merges into it, and the whole pile is swept again". The pile is cancelled, unmarked sandstone is dropped one cell above it, the test waits for the absorb, then for the pile to be marked, and checks that the mass is over 59 kg.
- New `OtherCancelKeepsSweep`: "Sweep Zones: giving and cancelling a Move To on debris in a sweep zone leaves it swept". Move To on marked zone debris leaves no tag and the zone marks it again. The item's Cancel Move To button then leaves no tag, and the debris is still marked after 3 game seconds. A cleanup cancels the move if the test stops midway.

## For Jeremy / coordinator

- The Cancel tool with its "All" filter, dragged over a zone, still cancels the sweeps of marked debris there. That is a cancelled sweep under the rule, so it is remembered.
- Existing behaviour I did not change: the zone marks debris that has a Move To order, so it carries both orders.
- `mods/sweep-zones/.context/domains/entities.md` (Business Rules blk-a6ab594b39d9, Patches table blk-8ed739708efc) still needs the skip rule and the six Clearable/Movable patches; this unit has no write path.

## Build output

```
dotnet build src/SweepZones/SweepZones.csproj -c Release -p:ModDeployFolder=none
INFO: Wrote C:\dev\ONI-Mods\mods\sweep-zones\src\SweepZones\bin\Release\SweepZones.dll
  Merge succeeded in 0.6737853 s
  Merged PLib into SweepZones.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.33

dotnet build src/SweepZones.Tests/SweepZones.Tests.csproj -c Release -p:ModDeployFolder=none
  SweepZones.Tests -> C:\dev\ONI-Mods\mods\sweep-zones\src\SweepZones.Tests\bin\Release\SweepZones.Tests.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.88
```
