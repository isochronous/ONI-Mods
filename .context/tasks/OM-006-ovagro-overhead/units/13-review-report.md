# Unit 13 — review report: Ovagro Overhead change since unit 12 (OM-006)

Reviewed: `git diff 896238b..HEAD -- . ':!.context' ':!mods/ovagro-overhead/.context'` (14 files) and `git -C common diff 93d2890..a0fe078` (5 files). Read every changed file whole, plus the unchanged `BranchPatch.cs`, `Session.cs` discovery and `TestBehaviour.RunAll`. Reference for game claims: the decompiled `VineBranchConfig.cs`, `VineBranch.cs`, `VineMother.cs` and a grep over the whole `decomp\` folder. Changed no file except this report. Did not launch the game or run the suite (spec).

**Verdict: approve.** No block findings. 1 fix, 5 nit.

## Findings

| # | Sev | Where | Finding | Expected fix | Pass |
|---|-----|-------|---------|--------------|------|
| 1 | fix | `common/tools/TestRunner/Kit/Attributes.cs:22-27` vs `TestBehaviour.cs:171-173` | The KeepSave summary says the fixture area is not cleared "after the test before it, when that one keeps the save too". The code skips the clear after any test whose next test has `KeepSave && !FreshSave`, whether or not that test keeps the save. A reader following the doc would expect a plain test followed by a KeepSave test to be cleared; it is not. | Reword to "nor after the test before it". Keep the README line (`README.md:104-107`) as is: it already says "run it alone or first after a load". | 1, 4 |
| 2 | nit | `common/tools/TestRunner/.context/domains/entities.md:110`, `.../standards/naming-conventions.md:37` | Both records list the `OniTest` options (`Timeout`, `FreshSave`, `Explicit`, `Boxed`, `NoColony`, `Worldgen`) and now miss `KeepSave`. `entities.md:155` describes FreshSave's reload and has no line for KeepSave. These are common-store records, so I could not edit them. | The manager adds `KeepSave` to both lists and one line to the entities behaviour list through the write path, at completion. | 4 |
| 3 | nit | `OvagroOverhead.Tests/Plots.cs:366` | `VinesFromDownwardFarm` has `Timeout = 150` (real seconds) but waits up to 90 s in `Kit.GrowMother` and another 90 s in `Kit.GrowChains`, plus about 10 game seconds of waits. The sibling ceiling test with the same two calls has `Timeout = 200` (`Vines.cs:38`). A slow machine would end in a timeout, not in the named "did not grow" failure. | `Timeout = 220`, or lower the two inner caps. | 5 |
| 4 | nit | `OvagroOverhead.Tests/Removal.cs:44`, `Preview.cs:139-166` | Both repeat the "wait for a branch on each side" wait that `Kit.GrowMother` now holds (the Vines.cs copy was removed for unit 12 finding 2). Preview's loop adds a diagnostic log and a "stuck" screenshot, which is a reason; Removal's has none. The hanging-farm set-up (tiles row, Hydroponic Farm flipped, pipe, seed, order, deposit, water) is now written four times (Plots, Removal, Preview, `VinesFromDownwardFarm`). | Removal: call `Kit.GrowMother`. Optional: a `Kit.HangingFarm(t, ...)` helper for the set-up. | 4 |
| 5 | nit | `common/tools/MakePreview/title_preview.py:231-232` | `gap` is the value `line_masks` returned for the line gap; the split branch overwrites it with the ink gap to the image edge. It is only read in the else branch, so nothing breaks, but the name now means two things. With three or more lines and `--position split`, `zip(masks, tops)` still draws only the first and second lines (pre-existing: `tops` had two entries before this change too). | Rename the new local `edge_gap`. The split help text could say "two lines". | 4 |
| 6 | nit | `common/tools/TestRunner/TestBehaviour.cs:172` | A KeepSave test that is not first after a load runs on whatever the previous test left, since the clear before it is skipped. The doc says to run it first, but nothing logs when it is not. | Optional: `Debug.Log` a line when a KeepSave test runs with `Session.SaveIsFresh` false. | 1 |

Carry-over, no change in status: unit 12 finding 6 (copied literals in `VineMotherCeilingConfig`) is partly covered now. `Registration.Prefabs` compares the vine's mass, crop, fiber amount, seed id and drop chance, and both temperature warnings, but not `max_age`, `max_radiation`, the pressure bound or decor. Still a nit.

## Passes run

### Pass 1 — Correctness: VineBranchCeilingConfig against vanilla

I diffed `VineBranchCeilingConfig.CreatePrefab` against the decompiled `VineBranchConfig.CreatePrefab`, call by call. Everything matches: the `CreatePlacedEntity` arguments (size 1x1, TIER0, `Creature`, 308.15 temperature), the tags, the `ExtendEntityToBasicPlant` bounds and flags (273.15, 298.15, 318.15, 378.15; no pressure sensitivity; `max_age` 2400; radiation 0 to 2200), the components, the trait modifiers (`GROWING_DURATION_CYCLES` is 3f), the `SeedProducer` (seed id and `HarvestOnly` and 1/6 drop chance), `WiltCondition` with both delays 0, and `OnPrefabInit` (monitor cells empty, the icon offset).

The differences are the intended ones:

- The prefab id, the `RegisterWithOverlay` id and `VineBranch.Def.BRANCH_PREFAB_NAME`, all `VineBranchCeiling`.
- The trait id `VineBranchCeilingOriginal`, which has to differ from vanilla's.
- The second `AddOrGet<HarvestDesignatable>()` dropped, a no-op in vanilla.
- The `IHasDlcRestrictions` members, written out for net48 as in `VineMotherCeilingConfig`.

Nothing vanilla sets is missing.

**Code that finds vines by the "VineBranch" id.** I grepped all of `decomp\` for the literal `"VineBranch"` and for `VineBranchConfig`. The only hits are `VineBranchConfig.cs` itself and `VineMotherConfig.cs:56`. Nothing in the game compares a prefab id to `VineBranch` or reads `VineBranchConfig.ID`, so critters, harvesting, overlays and the codex do not look vines up by that id. The things that matter are reached by other routes:

- The harvestable overlay: the config registers its own id with `OverlayScreen.HarvestableIDs`.
- The codex: `HideFromCodex` plus `CodexEntryRedirector` → `VineMother`, as vanilla.
- Harvest and fiber: `Harvestable`, `PlantFiberProducer`, `Crop`.
- Branch spawning: `VineMother.cs:153` and `VineBranch.cs:513` instantiate `Assets.GetPrefab(def.BRANCH_PREFAB_NAME)`, which is now the ceiling id for a ceiling mother and for each ceiling vine. The mother's own def is set in `VineMotherCeilingConfig.cs:91`.
- Every `PlantBranch` grep hit in the game belongs to the forest tree, not to vines.

Result: no findings.

### Pass 1b — BranchPatch (#9)

The postfix keys on the mother's prefab id (`CeilingForm.PlantTag`), not on the branch's, so it still runs for the first branch of a ceiling mother, which is now a `VineBranchCeiling`. Later branches copy `root.MyShape`. `Vines.cs` asserts both the prefab id and `RootShape == Top` on every branch of the ceiling chain, and `VinesFromDownwardFarm` does it for the plot case, so a break in either would fail. The floor test asserts the vanilla id on a floor mother's branch. Result: no findings.

### Pass 3 — KeepSave in RunAll

I traced each path in `TestBehaviour.cs:136-178` and `Session.cs:450-470`:

- **First test of the run** (`NextIndex` 0): `keepFirst` is true when that test sets KeepSave, so the opening clear is skipped. The bounds check on `NextIndex` is there.
- **Resume after a reload** (the driver restarts at `NextIndex = i`): `keepFirst` is read again at the resume index, so a KeepSave test right after a FreshSave or next-mod reload still gets no opening clear.
- **Between tests**: the clear after test `i` is skipped when test `i+1` is KeepSave and not FreshSave. With FreshSave, the clear runs and the reload follows, which is harmless. The `Boxed` branch comes after, so a box stays up across the skip (see finding 6 for the not-first case).
- **FreshSave test**: unchanged; the reload condition did not change.
- **Boxed tests**: the skip applies before the boxed-clearing choice, so a KeepSave test never gets `ClearInside` either.
- **Next mod**: `nextMod` forces a reload when the next assembly differs, so the skipped clear costs nothing there.
- **Generated worlds**: `generated` makes the new code unreachable.
- **Tests that do not set it**: `KeepSave` is false, so `keepFirst` is false and the `continue` is not taken. The control flow is identical to before.

Behaviour matches the attribute doc except the parenthetical in finding 1. Explicit tests: `Session.cs:157` leaves out `Explicit` tests unless a filter selects them, which covers `Preview.Scene` and `Removal.SaveForRemoval`. Findings: 1, 6.

### Pass 3b — title_preview.py split placement

The bottom line sits on the margin by its box. `gap` is the distance from the bottom line's ink bottom (`getbbox()[3]`) to the image bottom. The top line is placed at `gap - masks[0].getbbox()[1]`, so its ink top lands `gap` from the top, the same as the bottom's. Checked:

- One-line titles: `split` is false when `len(lines) == 1`, so they take the unchanged else branch.
- Fonts: the arithmetic uses each mask's own ink bounds, so descent and ascent differences between fonts are absorbed. A tall first-line ascent only makes `tops[0]` smaller or negative, and the paste still puts the ink at `gap`.
- An empty mask: `getbbox()` returns None for a blank line, which would raise. Titles are required, so it cannot occur.

Finding: 5.

### Pass 4 — standards adherence

Code-style checked: Formatting (tabs, Allman, trailing commas), Imports (System first, sorted), Comments (summaries on the new config and test classes), Control Structures (the `var` rule), Strings (no interpolation), Harmony Patches (unchanged), Game Components. The unit 12 `var` finding is fixed in `CeilingForm.cs:53` and `VineMotherCeilingConfig.cs:61`, and `Registration.cs` now writes types out too. `var` remains in `Removal.cs` and `Preview.cs`, which the standard allows in test projects. Error-handling: the new mod code has no try/catch and no logging, and no patch was added. The TestRunner standards (code-style: Allman, tabs, comments; error-handling: the five statuses) hold for the `KeepSave` additions.

Unit 12 findings 2 and 3 are resolved (`Vines.cs` uses `Kit.GrowMother` and `Kit.GrowChains`; one `AssertSeedIsVanilla` in Kit). Finding 1 is resolved by `VinesFromDownwardFarm`. Findings: 1, 2, 4, 5.

### Pass 5 — tests

- `VinesFromDownwardFarm` asserts behaviour: both first branches take `Top` as `RootShape`; for three branches on each side the cell is exactly the one under the tiles beside the plot, `MyShape` is `Top`, the prefab is `VineBranchCeiling`, and the branch is not Uprooted. The row of tiles beside the plot is the set-up that unit 12 finding 1 asked for.
- `Registration.Prefabs` compares the vine prefab to the vanilla one for the def, tags, mass, crop, fiber, seed and temperatures, and asserts the vanilla mother and vanilla vine ids unchanged.
- `Vines.cs` asserts the prefab id of every branch, in both the ceiling and floor tests.
- `Removal.SaveForRemoval` and `Preview.Scene` are Explicit and are fixtures for a manual check and a screenshot. Neither asserts a feature, but `Removal.Build` asserts its own set-up (`Kit.Is`, plant at cell, open order with no plant) before saving, so a bad scene fails.
- Evidence: the handoff cites full suite 17/17 (run-20261018-013757) and the removal check run-20261018-014051. I did not re-read those folders; I did not run the suite.

Findings: 3, 4.

### Pass 6 — player-facing text

`README.md` and `publish/workshop-description.txt` read plainly, with no hedging. "Ceiling Ovagro and their vines simply vanish; no seed is dropped. Cancel any Ovagro orders on upside-down farm tiles first." matches the code (the removal check showed 0 errors, and the order is the case from unit 12 finding 7, now covered). "Vines will grow along the ceiling just like they grow on the floor" holds per `Vines.cs`. Noted only: the new Workshop text no longer says "Requires the Prehistoric Planet Pack" (the README keeps it, and `mod_info.yaml` requires the DLC). The text is user-approved, so I did not raise it as a finding.

### Pass 8 — security

No file I/O in mod code. `Removal.cs` writes `ovagro-removal.sav` and `.txt` under `Session.Dir` (the run folder), deleting an existing save there first. Nothing else. Result: no findings.

### Build evidence

Not run (spec: change no file; the first review's build is the baseline). The test project compiled for the cited suite run.

## Store reads

- Handoff packet `handoff-13.md`: the brief for `mods/ovagro-overhead/src` was empty because the node has no records. The common-store brief gave the TestRunner records `tools/TestRunner:domains/entities.md`, `standards/code-style.md`, `standards/error-handling.md`, `standards/naming-conventions.md`, `domains/glossary.md` and `blk-6c2bbf052519` (MakePreview location).
- `ks map --document mods/just-fall-already/.context/standards/code-style.md` (22 records) and `.../error-handling.md` (23 records).
- `ks read` on the code-style sections #formatting (blk-6b50367224f0, with blk-e0ff5de8ea26, blk-9f56da6c281c, blk-60755d5ff761), #comments (blk-9edc9a83a58d, blk-00af2e4182a4, blk-fe4a12277682, blk-c293c5529a69), #control-structures (blk-2b2f52d515f0, with Loops blk-dffe3109ac36 and Strings blk-786743155fe7) and #game-components (blk-9fead247823f). The error-handling sections were read in unit 12 and were not changed by this diff.
- `ks read --store common` on `tools/TestRunner:standards/code-style.md` and `standards/error-handling.md`, applied to the KeepSave change.
- Grep (not a store read) of the TestRunner README, `.context/domains/entities.md` and `.context/standards/naming-conventions.md` for the option lists, for finding 2.
- The brief's common-store pointers all applied. One mismatch: `blk-6c2bbf052519` matched `tools/MakePreview` only through a one-line mention in the tools overview, and nothing in it governs `title_preview.py` itself. Nothing that governs this work was missing from the brief. No `ks brief --tag` widening was needed.

## Friction with ICON tooling

- `ks read` takes `<path>.md#section` addresses, but the section map prints them with a store prefix (`mods/just-fall-already:standards/...`); I used the printed form and it resolved. `ks read --store common <doc-id>` printed the whole document body (7 KB) where I wanted one section; sections of TestRunner docs have ids only inside the file text, not in a map I ran. No blocking problem.
- The packet's "next hop" text says to read with `--store C:/dev/ONI-Mods/common`; I used the shortcut `--store common`, which worked from the repo root.
