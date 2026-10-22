# Unit 15 review findings: commit 768d2ac (review fixes in Sweep Zones and four mods)

Reviewed `git show 768d2ac -- mods/` against units/10-review-findings.md, units/12-review-findings.md and plan.md Decisions, with the ICON code-quality-rules passes: all five for Sweep Zones, 1/3/4/5 for Upgrade Planner, 1/4/5 for the text and test changes. Pass 2 has no trust boundary in any of these changes (no input, files or network). Game code read from the ilspycmd decompile of Assembly-CSharp in this session's scratchpad (Clearable, Pickupable, Movable, CancellableMove, CancelTool). No game launch and no tests run (the implementers built only, so in-game evidence is still missing: Pass 5).

No Critical or Moderate findings. Every finding from units 10 and 12 that Jeremy asked to fix is resolved. Three Minor findings below, and one open point for Jeremy that unit 13 already raised.

## Prior findings: resolved or not

### Unit 12 (Sweep Zones)
- **M2 (any Cancel tags debris): resolved.** `Clearable_OnCancel_Patch.Prefix` (Patches.cs:70-84) collects the pile items that are marked (`GameTags.Garbage`, which only `Clearable.MarkForClear` adds) and in `sweepCells` before vanilla `OnCancel` runs `CancelClearing` on them, and the postfix tags only those. It walks the same list vanilla walks (from the item that got the Cancel to the end). `OnClickCancel` does the same for one item; vanilla `OnClickCancel` calls `CancelClearing` directly, not `OnCancel`, so the two patches never both run.
  - Cancelling digs or buildings with a filter other than All, or with All over cells whose debris is unmarked, tags nothing.
  - **Move To does not swallow a real sweep cancel.** `Movable.MarkForMove` (decompile: `Trigger(Cancel)` is its first statement, synchronous) is wrapped by a `Running` counter with a Finalizer, so an exception cannot leave it stuck above 0. The counter is only non-zero for the duration of that call, and no player action sends Cancel from inside it. `MarkForMove` is also reached from `OnSplitFromChunk`, where suppressing the tag is also correct.
  - Cancelling a Move To goes through `CancellableMove.OnCancel` → `Movable.ClearMove`, which sends no Cancel to the item, as unit 13 says.
  - `WorldContainer.CancelChores` still tags marked zone debris on a world being destroyed; no effect.
- **M1 (merge keeps the cancel): resolved, both directions.** `Pickupable` (cell-change handler, decompile line ~708) always has the pile already in the cell absorb the arriving item: `existing.TryAbsorb(arriving)` → `Absorb` triggers OnAbsorb on the survivor and deletes the newcomer. The `Clearable_OnAbsorb_Patch` postfix (Patches.cs:131-138) lifts the tag from the survivor whatever was merged in:
  - Unmarked debris on a cancelled pile: the tag is lifted and the next zone pass marks the pile (the new `MergeLiftsCancel` test).
  - A cancelled item falling on another pile: the item is deleted, the survivor never had the tag (or has it lifted if it was also cancelled).
  - Marked debris on a cancelled pile: vanilla `OnAbsorb` → `MarkForClear` marks the pile, and both patches lift the tag.
  - `Storage.Store` absorbs into stacks too (Storage.cs:422); stored items have no tag (lifted on `OnStore`), so no effect.
- **m1 (test not paced): resolved.** `PlayerCancelSticks` now waits 1 game second after mark, cancel and re-mark, and before each check.
- **m2 (coverage gaps): partly addressed, as specified.** The merge rule now has a test. The multi-item pile walk, pickup and save/load are still not covered; Jeremy's instruction was one test per behaviour, so that is acceptable.
- **m3 (partial pickup): left as is**, per Jeremy.
- **SetSweep lifts cancels on paint (unit 13's addition): correct and within the decision.** `ZoneStore.SetSweep` (ZoneStore.cs:54-67) has one caller, `SweepZoneTool.OnDragTool` in Sweep mode (SweepZoneTool.cs:201), so it never runs on load or during pruning. Without it, a cancelled pile under a zone that is erased and painted again, or one that fell into another zone, would stay skipped, against "a zone painted later sweeps everything in it". Side effect: dragging Sweep zone over an existing zone only to change its priority also lifts every cancel there. README and CHANGELOG say so ("or you paint the zone over it again").
- **The three tests are paced and assert results.**
  - `PlayerCancelSticks`: now ends by painting the zone again and waiting for the mark; would fail without the SetSweep change.
  - `MergeLiftsCancel`: fails on 4e444dd (the tag survives the merge and the pile is never marked). The arriving drop is in the cell above the zone cell, which is not in `sweepCells`, so it lands unmarked and the test checks the case M1 described. The mass check (>59 kg) shows the merge happened.
  - `OtherCancelKeepsSweep`: the first half fails on 4e444dd (Move To tagged the item). The second half (Cancel Move To button) passes on the old code too, since no Cancel reaches the item; it checks the debris stays marked, so it still asserts a result. A cleanup cancels the move.

### Unit 10 (four mods)
- **2. Upgrade Planner world id: resolved.** `colorsWorld` is part of the cache check and stored on refresh (UpgradeTool.cs:349, 358-366); `world` is read once and still used in the building loop. Part (b) (orders placed after a drag) stays as is, as unit 10 suggested.
- **3. Vent Freeze Fix "spawns": resolved** in README, Workshop description, mod.yaml, tooltip and the .pot/.po msgids. The text is accurate: the watcher is added in `DoPostConfigureComplete` and starts on every vent's spawn, that is a build or a load.
- **4. README "It": resolved** by naming "The watcher" instead of moving the sentence. Reads clearly.
- **5. Vent Freeze Fix CHANGELOG: resolved** (Unreleased bullet, accurate, plain). The store note `mods/vent-freeze-fix/.context/domains/entities.md:97` ("runs only in the first second of running time after a load") is still stale; plan.md lists store notes as a later step.
- **.po msgid change: translations still accurate.** Only the msgids changed; the ko/ru/zh msgstr say "after the vent is created/appears (on load or when built)", which is the same meaning as the new English. `translations.py --build` reported 13/13 for each.
- **6. Salty Surf comment: resolved.** Mod.cs:55-57 now states the rule correctly (a hidden tag writes nothing; the separator comes from the visible tag before it; only last place is unsafe).
- **7. Salty Surf test language: resolved.** Intake.cs:34-35 checks every description line for a trailing "," with no localized prefix. See m2 below for what it gave up.
- **8. Just Fall Already CHANGELOG: resolved** (two bullets).
- **1 (VFF wording) and 9 (JFA log gate test)**: settled by Jeremy (text kept, no test needed); nothing to do in this commit.

## New findings

### Minor

**m1. README and CHANGELOG say "more debris lands on the pile", but only debris that merges lifts the cancel.** (Pass 1, documentation) README.md:10, CHANGELOG.md:5.
`Pickupable` absorbs only when the newcomer can merge (same material) and the cell below is solid. Scenario: the player cancels a sandstone pile's sweep in a zone; copper ore falls into the same cell. It lands on the pile but does not merge, so the sandstone stays cancelled (the copper is marked by the zone). The player reads "more debris lands on the pile" and expects the sandstone to be swept. The ZoneStore doc comment ("other debris merges into it") is exact. Suggested text: "more of the same debris lands on the pile".

**m2. The Salty Surf test no longer checks that a Properties line exists.** (Pass 5) Intake.cs:34-35.
The old test asserted a Properties line was present; the new loop only checks that no line ends in ",". If a change hid every tag of Water or Salt Water, the line would disappear and the test would still pass. Water always keeps its phase tag (unit 10), so this has no effect today. A language-independent check is possible, for example that the text contains `ELEMENTPROPERTIES` formatted with the first visible tag's name, or that the line count is unchanged from vanilla.

**m3. The pile walk is now written three times in ZoneStore/Patches.** (Pass 4) ZoneStore.cs:54-67 (`SetSweep`), ZoneStore.cs:143-160 (`MarkSweepZones`), Patches.cs:76-83.
`SetSweep` repeats `MarkSweepZones`' "first pickupable in the cell, then walk `objectLayerListItem`" lookup line for line. A small `PileAt(int cell)` helper would keep the two in step. Optional.

### Open point for Jeremy (not a defect; unit 13 raised it)
The Cancel tool's default filter is All (`CancelTool` adds only CLEANANDCLEAR and DIGPLACER as extra, off by default; the base filter set has All on). A player who drags Cancel with the default filter over a sweep zone to cancel digs also cancels the sweep of the zone's marked debris there, and that cancel is now remembered. Under the rule Jeremy set ("marked for sweeping at that moment and in a sweep zone") this is a cancelled sweep, and the vanilla game cancels those sweeps in the same drag. The mod cannot tell this drag apart from a drag meant to cancel the sweep. Jeremy's "cancelling digs ... must not tag debris" holds for every filter except All. Ask whether that is fine as is.

## Still open (outside this commit)
- No in-game run of the three Sweep Zones tests, the Upgrade Planner test or the Salty Surf test. Plan.md lists the in-game run as the next step.
- Store notes: VFF entities.md:97 (above) and Sweep Zones entities.md (Business Rules blk-a6ab594b39d9, Patches table blk-8ed739708efc, no mention of the cancel rule or the six Clearable/Movable patches).

## Brief feedback (handoff section 2)
- None of the pointers shown applied: 16 copies of `error-handling.md#try-catch` from 16 mod nodes, matched only because that block names Upgrade Planner and Sweep Zones files, plus `code-style.md#imports-includes/example` copies from other mods. Same pattern unit 10 reported.
- Missing: the Sweep Zones `entities.md` Business Rules and Patches table (blk-a6ab594b39d9, blk-8ed739708efc) and the VFF `entities.md` watcher notes, which govern this change directly.
- Pointer reasons were accurate as stated (they say "names <file>"), just irrelevant.
