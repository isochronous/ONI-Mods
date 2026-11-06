# Unit 05 review findings: review-fix commit 44f7dd9

Diff reviewed: `git diff 04e800d..44f7dd9 -- mods/jumper-bridges/`, all 11 files, against units/03-review-findings.md.
All five passes run. Pass 2 (security): no trust boundary in the diff. No code run (read-only); relied on the supplied 20/20 in-game run.

**Verdict: all seven findings resolved. No critical or moderate findings. Two minor wording notes.**

## Per-finding

- **M1 resolved.** `JointPlates.cs` `PlatesEndToEnd`: plate at (2,1) has ends at 1 and 3, plate at (4,1) has ends at 3 and 5, so cell 3 is shared. Asserts the second preview is valid, a node on the lamp port, battery circuit == shared-cell circuit == lamp CircuitID, no invalid ports, and `IsPowered` after charging. These are outcomes, not existence checks. An inverted `jointPlate` filter would fail the second `ValidWithPreview`.
- **M2 resolved.** `NotInTile`: a Tile at the plate's end cell, asserts invalid and the exact `HELP_BUILDLOCATION_HIGHWATT_NOT_IN_TILE` string. Covers the `InTileOrDoor` path (tile case only; the door cases stay untested, which one representative test per path allows).
- **m1 resolved.** Tooltip (ModStrings.cs:24, .pot, zh/ko/ru) now names both plates, drops "or another bridge's end" and states the two sharing rules. Workshop text says "plain or conductive", "Bridges of one kind", and that a plate end shares only with a plate end. README:3 and :21 already agree. All statements match the code rule (`HasEnd` filters on `IsJointPlate ==`). No hedging. The three translations match the English sentence by sentence.
- **m2 resolved.** CHANGELOG bullet added: "Fixed a crash when placing a tile over a bridge end node." It is short and plain. See minor note N1.
- **m3 resolved.** `AnyPowerEndAt` is equivalent to the old loop. Old: `EndsAt(cell)` (null-safe, `IsEnd(cell)`), then `go != self`, Kind == Power, `IsJointPlate(go) == jointPlate`. New: same four conditions on the same `byCell` list, in the same combination. `Kind` is set in `OnSpawn` before `Add`, so it is never unset for a registered footprint. No allocation. The `HasEnd` call site just returns it.
- **m5 resolved, WireUnderEnd intact.** `Inherit` now runs only when some bridge in `JumperBridge.At(kind, cell)` has `IsEnd(cell)`. Every bridge that reaches `At` has `BridgeFootprint` (Patches.cs:23-24, 58-59, 86-87 add both together), so `GetComponent<BridgeFootprint>()` cannot be null there. A wire under a wire bridge end (the WireUnderEnd case) has an end on that cell, so `endHere` is true and inheritance is unchanged. A middle-only cell (wire bridge middle, plate centre) no longer stores bits. `ScheduleCreate` is still called for all bridges, as before. If a middle cell also holds another bridge's end, `endHere` is true and it inherits, which is correct.
- **m6 resolved.** `BatteryPlateLamp` now has the "no circuit before the plate" baseline and the `portsNotOverlapping` flag check, matching `Power.cs:55,70`. `PlatesEndToEnd` has the baseline too.

## New findings

### N1. Minor (Pass 4): "crash" overstates the m2 fix
`CHANGELOG.md:6`. The failure was a NullReferenceException in the placement check and log spam, with the tile not placeable. The game did not crash. Suggested: "Fixed placing a tile over a bridge end node failing with an error." Optional.

### N2. Minor (Pass 4): README:19 still says "another power bridge's end" without the plate split
`README.md:19` is about the three wire bridges and says an end may sit on "another power bridge's end". Line 21 and the new tooltip give the split, so a reader of the earlier paragraph alone is not misled about plates (plates are named only from line 21). No change needed unless the paragraph is reworded.

## Checked and fine
- `PlatesEndToEnd` and `NotInTile` cell layouts traced against plate footprint (1x1, ends at +/-1) and `Power.Floor` (row 0 tiles, row 1 free). Pacing uses game-time waits after each build.
- `.pot` msgid equals `ModStrings.cs` text exactly, and the three `.po` msgids equal the `.pot`.
- m4, m7 were not part of the fix commit's scope. m7 (stale store records `blk-e978d080073f`, `blk-a88b0d39bc11`, `blk-852815eed994`, glossary/overview) is still for the completion phase; the commit touched no store records.

## Brief feedback (handoff-5 section 2)
- About 22 of the pointers were other mods' `code-style.md` "Line Length" and "Harmony Patches" sections. They do not apply here. Only the jumper-bridges ones (`blk-7cc833c742eb`, `blk-27d655025249`) do.
- No pointer covered the test files or the translations/Workshop copy rules; the review used the memory style notes instead. No pointer's stated reason mismatched its record.
- `BridgeKind` record `blk-e978d080073f` still says plates are "left alone" (the m7 staleness).

## ICON tooling friction
None. The `ks read` calls were unnecessary: the handoff listed only pointers into code I read directly through the diff, so I did not run any.
