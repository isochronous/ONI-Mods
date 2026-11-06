# Unit 10 review findings: commit aa2fae7 (four mod fixes)

Reviewed `git show aa2fae7 -- mods/` against plan.md ## Decisions, with the ICON code-quality-rules passes (abbreviated 1/4/5 for the text-only Vent Freeze Fix change, all five for the code changes). The game was decompiled with ilspycmd (Assembly-CSharp and -firstpass) to check the Salty Surf and Just Fall Already claims. No tests were run (no game launch). The implementers did not run them either; only builds ran (Pass 5 gap, noted below).

No Critical findings. No finding blocks the commit.

## Ranked findings

### Moderate

1. **Vent Freeze Fix: the text doesn't use Jeremy's decision wording, and Jeremy hasn't been asked about it.** (Pass 1)
   plan.md Decisions says the text should say the watcher "does nothing on a built one". The commit says instead that a built vent's watcher runs for its first second and "has no earlier state to compare on its first tick" (README.md:18). That is the correct choice: `VentFreezeWatcher.Verdict` returns null on ticks 2-5 of a built vent when a sub-80% tile forms (VentFreezeWatcher.cs:209-221), so "does nothing" would be false (unit 06 finding 1). Failure scenario: Jeremy reads the shipped text, expects the wording he decided on, and gets something else he never approved. Action: show Jeremy the new sentence, or ask him whether he wants the code changed so a built vent really does nothing (skip the watcher when `savedVentCell` is invalid after spawn). Don't silently keep either version.

### Minor

2. **Upgrade Planner: the highlight can stay out of date for up to 1 s after the active asteroid changes, or while a drag's orders are being placed.** (Pass 3) UpgradeTool.cs:305 and :315-325.
   The cache key covers the plan, its version and the box, but not `ClusterManager.Instance.activeWorldId`. `IsMarked` state also isn't in the key. Before the fix, the cache never hit, so both updated every frame. Now:
   (a) If you switch asteroids with the tool open, the old world's cells stay coloured and the new world has no highlight for up to 1 s.
   (b) After a drag, `matcher = null` (UpgradeTool.cs:292) rebuilds the highlight once. `PlacementQueue` then places orders over the following frames, so tiles turn MarkedColor up to 1 s later.
   This fits the design ("at most once a second", README:61). Adding `activeWorldId` to the key would remove (a) at no cost. (b) can stay as it is.
3. **Vent Freeze Fix: the player text says "spawns".** (Pass 4) workshop-description.txt:13, mod.yaml:2, ModStrings.cs:30.
   "after a liquid vent spawns (on load, or when it is built)" uses a developer term. "for the first second after a load, or after a liquid vent is built" says the same thing in player words, and it's shorter.
4. **Vent Freeze Fix README: the subject of "It looks at the vent cell..." became unclear.** (Pass 4) README.md:18.
   The new sentence "A vent built in play has no earlier state to compare on its first tick." now sits right before "It looks at the vent cell...", so "It" reads as the vent instead of the watcher. Move the new sentence after "...so the first comparison after a load has a previous tick."
5. **Vent Freeze Fix: no CHANGELOG bullet, and a store note is still stale.** (Pass 5)
   The Workshop description, mod.yaml description and an option tooltip changed, but CHANGELOG.md has no Unreleased entry. Unit 06 left this for the coordinator to decide. The other three mods each got a bullet. Store note `mods/vent-freeze-fix/.context/domains/entities.md:97` ("runs only in the first second of running time after a load") is now wrong and needs a write-path update. Unit 06 also flagged this.
6. **Salty Surf: the code comment describes the separator rule imprecisely.** (Pass 4) Mod.cs:52-54.
   The comment says "The line writes ", " after every tag but the last, hidden or not". In `Element.FullDescription`, a hidden tag writes nothing. The ", " comes from the visible tag before it whenever that tag is not at the last index. So the only bad position for a hidden tag is last, and first is just one safe choice. The code is correct; only the comment's reasoning is loose.
7. **Salty Surf test: the Properties-line prefix check depends on the language.** (Pass 5) Intake.cs:35.
   The test finds the line with `ELEMENTPROPERTIES.Split('{')[0]`. In a translation where `{0}` comes first, the prefix is empty and the last description line is checked instead. The test runner uses English, so this has no effect today.
8. **Just Fall Already CHANGELOG: one bullet holds two changes.** (Pass 4) CHANGELOG.md:5.
   "No longer writes a log line for each critter it drops, and spends less time finding critters next to doors." Per the changelog style memory (few short plain bullets), split it into two bullets.
9. **Just Fall Already: no test covers the log gate itself.** (Pass 5) Under the runner, `LogPokes` is always true, so the Doors.cs suite only shows that the gated line still appears. Nothing shows it is silent in a normal launch. This can't be tested through the runner. It's acceptable as is; a short manual check of a non-runner Player.log would cover it.

## Per mod: what was checked

### Upgrade Planner: no correctness findings (finding 2 is a minor edge case)
- Cause confirmed: `colorsBoxVersion` (UpgradeTool.cs:74) was never assigned, so the cache never hit. The fix assigns it at :311, alongside the matcher fields.
- Invalidation paths:
  - Plan edits: every mutator in `ChangeMaterialPlan` and `ReplaceTypePlan` calls `changed()`, which bumps `Version`. That includes material mode (SetSourceAny/Specific/SetSource/SetTarget/Pick/SetAllBuildings/Clear/Load) and the type tab (SetSource/SetSourceAny/SetSourceSpecific/SetSourceMaterials/SetTarget/SetTargetMaterials/Clear/Load). `Mode` bumps it on a tab change. `SetAllBuildings` and `ReplaceTypePlan.SetSource` return early only when nothing changed.
  - Apply: it creates a new `applied` object, so the reference check on `matcherPlan` catches it.
  - Window open/close: this switches `Shown` between `plan` and `applied`, which is a reference change.
  - Box: `OnMouseMove` bumps `boxVersion` (:221), and so does `ClearBox` (:255). `ClearBox` runs before the cache check (:302-303).
  - Drag end and deactivate: both set `matcher = null`.
  - Research or building changes: picked up by the 1 s timer.
- The window's search/filter text isn't part of `Compile()`, so it doesn't need to invalidate the cache.
- Test `HighlightIsCached` is meaningful. It fails on the old code, because every call cleared the set and the marker was lost. It is deterministic because `Time.unscaledTime` doesn't change within a frame. The existing `BoxMarksAsItMoves` test now also exercises box invalidation (grow, then shrink back to one tile).
- The magenta marker the test adds stays in the tool's set for up to 1 s. The tool is torn down by cleanup, so this has no side effects.
- README:61 and the CHANGELOG bullet are accurate and plain.

### Just Fall Already: no correctness findings
- Log gate: `static readonly bool LogPokes = StartedByTestRunner()` is set once, in the type initializer. It uses an ordinal `StartsWith("--onitest-")`. The runner always starts the game with `--onitest-dir=... --onitest-save=...` (TestRunner entities.md:19), and live-session reuse keeps the same process and arguments. The Doors.cs `WatchPokes` handler matches the unchanged text. The once-per-load line and the slow-handler line are not gated, as the decision says.
- `CrittersAt` gives the same results as before. It is only called for cells in `armed` (FallPoke.cs:120), so dropping critters in unarmed cells at gather time removes nothing a lookup could reach:
  - Arming in the middle of a frame resets `cacheFrame` (:77). This is stricter than before: the old cache was built before the arm and found critters in the new cell only at their earlier positions.
  - Disarming (:142-143) leaves stale entries that are never looked up. Purge only runs inside the door patch, which also resets the cache.
  - Multi-cell critters: both versions key on `Grid.PosToCell(brain)`, the origin cell only. That limitation is unchanged.
  - Critters that move during a frame: both versions snapshot positions at the first lookup in the frame, so they behave the same.
- Allocation: none per frame. Lists come from `spareLists`. `Dictionary.Values` is cached by the dictionary and enumerates with a struct enumerator, and so does `Components.Brains.Items`. The gather is lazy and only happens on an armed hit.
- Checked whether a pooled list could be reused while `OnGridUpdated` is iterating it. `brain.UpdateBrain()` would have to reset `cacheFrame` through a door arm, which needs `Door.SetWorldState(true)`. The decompile shows that only happens on entering open, closed or locked after the door's animation (Door.cs:132-147), never synchronously from `Door.Open()`. So it can't happen.
- The slow-handler log's "critter cells" count now counts only armed critter cells. That is fine for diagnostics.

### Salty Surf: no correctness findings
- `GameTags.HiddenElementTags` has one reader in Assembly-CSharp, `Element.FullDescription` (Element.cs:252), and none in -firstpass. `FullDescription` is called from `CodexEntryGenerator_Elements` and from `SimpleInfoScreen` (twice), which matches the CHANGELOG ("codex page and info panel").
- Reordering `oreTags` doesn't affect anything else. Every other reader is order-independent: `Element.HasTag` (IndexOf), `Recipe` (IndexOf), `PrimaryElement` (adds all), `CodexEntryGenerator_Elements:84` and `SandboxToolParameterMenu:465` (scan for HideFromCodex/HideFromSpawnTool). `CreateOreTags` always adds the phase tag, so Water (AnyWater, Liquid) still has visible tags and the Properties line isn't left empty.
- Adding the tag twice is harmless: `TagSet.Add` is a set, and the `!HasTag` guard is kept.
- The test asserts results: the tag is still present, the text has no `MISSING` and no tag name, a Properties line exists, and it has no trailing comma. It would fail on the old code (MISSING). `OniAssert` throws, so `line != null` guards the `TrimEnd`.

### Vent Freeze Fix: text only (findings 1, 3, 4, 5)
- The code confirms a watcher on every liquid vent prefab (Mod.cs `DoPostConfigureComplete`), active for 5 ticks of running time (VentFreezeWatcher.cs:39, :114).
- "Takes effect on the next load" is still true: `RepairEnabled` is read in DebugSave's `Game.OnSpawn` (DebugSave.cs:54), and built vents use that value.
- The new text describes ticks 2-5 on a built vent accurately, because it says what the watcher does without saying "nothing".
- The .po files were ignored, as instructed.

## Brief feedback (handoff section 2)
- None of the pointers listed applied. They were 16 copies of the same `error-handling.md#try-catch` block from 16 mod nodes, matched only because that block names Upgrade Planner files, plus generic code-style blocks (Loops, Imports example) from other mods. None of them govern this review.
- What actually governed the review, and was missing from the pointers: the Just Fall Already `entities.md#key-code-paths` member table and Telemetry note (blk-9d52ccfdb3df appeared; blk-7d0c65d332f3 did not), the Vent Freeze Fix `entities.md` watcher notes (line 97), and the TestRunner `--onitest-*` contract in `common/tools/TestRunner/.context/domains/entities.md`.
