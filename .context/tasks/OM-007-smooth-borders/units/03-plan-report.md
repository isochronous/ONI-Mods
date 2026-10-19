# Unit 03 report: plan for the Smooth Borders build

Inputs: units/02-design-report.md with the user's overrides (handoff-3 section 1), the OM-006 plan, mods/ovagros-overhead (scaffold), mods/natural-backwalls-everywhere (PLib options, ModStrings, Generated.cs and Looks.cs tests), common/tools/TestRunner (Session.cs, Worldgen.cs, TestBehaviour.cs, Patches.cs, Invoke-OniTests.ps1), common/tools/WorldgenDriver, store records blk-87272da04dee and blk-c8a18ea4088d (read by id). Nothing was run and no file other than this report was written.

Tiers used: **light** (mechanical, copies a pattern), **default**, **complex** (touches game internals or the harness state machine; give it a stronger model or a review). "Manager" means the coordinator does it directly (store skills, pointer bump, user review).

## 0. Findings that change the design report

1. **Config folder name.** PLib's shared config for a mod lives in `Documents\Klei\OxygenNotIncluded\mods\config\<assembly name>\config.json` (on disk: `config\NaturalBackwalls\config.json`, no `Isochronous.*` folder anywhere). The design (section 4, harness item 2) says `mods/config/<staticID>`; that is wrong, and `Get-LoadoutKey` in Invoke-OniTests.ps1 (lines 282-286) has the same error, so a mod's config never enters the loadout key today. For Smooth Borders the folder is `config\SmoothBorders`. S2 fixes both.
2. **Translations path.** The `.pot` is written to `src/SmoothBorders/translations/SmoothBorders.pot` (translations.py docstring), not `translations/` under the mod folder.
3. **Ranges changed by the user** (1 to T+R): Primary 1-16, Small 1-7, Moonlets 1-6, Tiny moonlets 1-3; labels "Primary asteroids (1-16)", "Small asteroids (1-7)", "Moonlets (1-6)", "Tiny moonlets (1-3)". The design's claims that the max stays within vanilla's walk now read: at the top of the range the flat wall equals the thickest wall vanilla draws on the group's widest world (T+R) and exceeds it on the other worlds of a merged group (Tiny moonlets at 3 on the (1,0) dev worlds, Small at 7 on (4,1) worlds whose walk tops at 5). Those edges' POI rectangles grow by the section 2.4 rule, so the identity guarantee holds up to the default and edge by edge above it. Tests 4 and 6 now run at the new maxima.
4. **Tooltips** are the game default only: four full strings "Game default: 4." (3, 2, 1), one per group, no placeholders and no asteroid lists.
5. **The vanilla-baseline switch can live in test code** (answer to the packet's question, details in S6): the test assembly unpatches the mod's `DrawWorldBorder` patches by Harmony id from a `Prepare` hook and re-applies them from the hook's undo. No `Border.Active` in the shipped mod. Fallback if the Harmony unpatch round trip misbehaves: the public static bool from the design, set only by tests.
6. **Parallel-game constraint.** Every step that verifies in the game launches it; only one game runs at a time. So "parallel" below means parallel editing without a shared branch, and verification runs are serialised by the manager.

## 1. Steps

### S1 Scaffold (light) — depends on nothing

Pattern: mods/ovagros-overhead (OM-006 commits bea076e and 791f19c), but with PLib and translations like natural-backwalls-everywhere.

| File | Change |
|---|---|
| `mods/smooth-borders/src/SmoothBorders/SmoothBorders.csproj` | Import `$(OniCommon)oni-mod.props`/`.targets`; AssemblyName/RootNamespace `SmoothBorders`, Version 0.1.0; `<UsePLib>true`, `<UseTranslations>true`, `<ReferenceUnityUI>true`, Newtonsoft.Json reference `Private=false` (copy natural-backwalls' csproj lines) |
| `src/SmoothBorders/mod.yaml` | title "Smooth Borders", staticID `Isochronous.SmoothBorders`, one-line description from design section 5 |
| `src/SmoothBorders/mod_info.yaml` | `supportedContent: ALL`, `minimumSupportedBuild: 744825`, `version: 0.1.0`, `APIVersion: 2` |
| `src/SmoothBorders/Mod.cs` | `SmoothBordersMod : UserMod2` with the Ovagros all-or-nothing try/catch around `base.OnLoad(harmony)` (unpatch all, warn once), then `PUtil.InitLibrary(false)`, `Translations.Register`, `POptions.RegisterOptions`, "Loaded version" log. Registration lines stay commented out or guarded until S3 adds the types so the project builds |
| `src/SmoothBorders.Tests/SmoothBorders.Tests.csproj` | Copy of OvagrosOverhead.Tests.csproj (ReferenceUnityUI, project refs to the mod and TestRunner, `ModDeployFolder=none`); add a PLib reference only if Looks needs it (natural-backwalls' Tests csproj shows how) |
| `mods/smooth-borders/.gitignore`, `.gitattributes` | Copies from ovagros-overhead |
| `mods/smooth-borders/CHANGELOG.md` | `## Unreleased` with the first-version bullets (final wording in S9) |
| `ONI-Mods.sln` | Solution folder, two projects, 3 GUIDs, config lines, NestedProjects (see how OvagrosOverhead appears at lines 86 and 132) |
| `README.md` | Table row `Smooth Borders | [mods/smooth-borders](mods/smooth-borders)` |
| `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml` | Option "Smooth Borders" |
| `.github/workflows/label-mod.yml` | `"Smooth Borders": "smooth-borders"` |

Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` gives 0 warnings, 0 errors; `git status` shows only those paths. Commit: one terse line.

**M1 (manager, after S1):** icon-integrate for `mods/smooth-borders`, then the `mods/.context/projects.md` row. The decision record goes in at completion through adr-discipline. No node population, as for OM-006.

### S2 TestRunner and WorldgenDriver changes in common/ (complex) — depends on nothing; runs beside S1

common/ is a submodule: work and commit inside `common/` on its own branch first (as OM-006 did, common PR style: one PR for the repo), then the manager bumps the pointer in the main repo (M2) so no unit commits in the main repo concurrently.

Files and changes:

1. `Kit/Attributes.cs`: `OniTestAttribute.Prepare` (string, the name of a `public static` method of the test's own class, void or returning `Action`). Doc comment: runs once, at the main menu, before the coordinate is applied; the returned action undoes it.
2. `Session.cs` (`TestCase`): resolve the method at Discover (a missing or ill-shaped method is a Discover error naming the test, like the existing signature check); a `WorldKey` = `Worldgen + "|" + Prepare method id`. Replace the three places that compare coordinates by key: `Session.WriteOff` (grouping loop), `Session.Continue`, and `TestBehaviour.RunAll` line 150 (`otherWorld`), plus `GeneratedWorld`/`Worldgen.Current` carrying the key. Tests with different `Prepare` never share a world, same coordinate or not.
3. `Session.Continue` (just before `Worldgen.Pending = next.Worldgen`): run the test's Prepare (inside try/catch: an exception writes off that world's tests as `fail` "Prepare threw: ...", as a generation failure does), keep the returned undo. Run the previous world's undo first, whenever a different key comes up, and at `Session.Finish`.
4. Config snapshot and restore (the design's item 2, corrected by finding 1): the run request carries the mod folder names (`-Mods` and `-With`); `Session.Start` copies `mods\config\<name>` for each into `Dir\config-backup\<name>` (or records "absent"), `Session.Finish` restores or deletes. Crash safety: Invoke-OniTests.ps1 restores a leftover `config-backup` of an earlier run at the start of the next run, as it does for the `mods.json` leftover (lines 361-377).
5. `Invoke-OniTests.ps1`: `Get-LoadoutKey` uses the config folder `mods\config\<mod folder name>` (the assembly name; local mod folder names equal assembly names in this repo) instead of staticID; pass the names in the request; leftover restore.
6. `common/tools/WorldgenDriver/Invoke-Worldgen.ps1`: new `-ModConfig 'Name=path\to\config.json'` (repeatable) that backs up `mods\config\<Name>\config.json`, writes the given one before each session and restores at the end, the way it handles `mods.json` and `kplayerprefs.yaml`. Needed by S8 (the max-thickness sweep) so that no sweep edits a player's config by hand.
7. READMEs (`TestRunner/README.md` per-test options; `WorldgenDriver/README.md` parameter) and the common store's testing records (`ks`-side, manager) updated: Prepare row in the per-test options table of record blk-c8a18ea4088d, and the config note.

Pattern: the existing `Worldgen` / `KeepSave` plumbing in Session.cs and TestBehaviour.cs; style per the TestRunner and WorldgenDriver code-style standards (tabs, braces).

Acceptance: `dotnet build common/tools/TestRunner/TestRunner.csproj -c Release -p:ModDeployFolder=none` and the WorldgenDriver csproj, 0 warnings, 0 errors. Regression of the worldgen path with an unchanged mod: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls -Filter '*Generated*'` still passes. The Prepare and config behaviours get their real proof in S6 (a harness-only check would need a throw-away test project; S6's tests are the harness's first user). The manager runs the game launches.

**M2 (manager):** `git add common` pointer bump commit in the main repo, after S2 and between other units.

### S3 Groups, options, strings (default) — depends on S1

Files (all `mods/smooth-borders/src/SmoothBorders/` unless noted):

| File | Change |
|---|---|
| `Groups.cs` | The one group table: id, default (4/3/2/1), min 1, max (16/7/6/3), key into ModStrings; the `(T,R) -> group` mapping from design 2.1 (merge by equal floor: (10,6) Primary; (5,2),(4,1) Small; (4,2),(3,1) Moonlets; (2,1),(1,0) Tiny moonlets); unknown pair gives `max(1, T-R)` flat and no setting; T <= 0 gives 0. Pure functions, no game types, so NoColony tests can call them. |
| `Options.cs` | `[JsonObject(MemberSerialization.OptIn)]`, `[ConfigFile(SharedConfigLocation: true)]`, four `[JsonProperty] public int` (Primary, Small, Moonlets, TinyMoonlets), `Load() => POptions.ReadSettings<Options>() ?? new Options()` clamped per group table, `IOptions.CreateOptions()` yielding the intro entry then four `IntOptionsEntry(nameof(X), new OptionAttribute(NAME + " (1-16)", TOOLTIP), new LimitAttribute(min, max))` (range suffix from the table). Pattern: `mods/natural-backwalls-everywhere/src/NaturalBackwalls/Options.cs` lines 13-15, 91-115. No static `[Option]` properties (keeps the yield order). |
| `TopLeftTextOptionsEntry.cs` | ~15-line subclass of PLib's `TextBlockOptionsEntry` (design 2.5); `CreateUIEntry` override adds the `PLabel` with `UpperLeft`, `FlexSize = Vector2.right`, word wrap, `ColumnSpan = 2`. The implementer checks the 4.25.0 base members by decompiling the package DLL (`~/.nuget/packages/plib/4.25.0/lib/net48/PLib.dll`). |
| `ModStrings.cs` | `SMOOTHBORDERS.OPTIONS.INTRO` (text from design section 1), and `PRIMARY`, `SMALL`, `MOONLETS`, `TINY_MOONLETS` each `.NAME` ("Primary asteroids", "Small asteroids", "Moonlets", "Tiny moonlets") and `.TOOLTIP` ("Game default: 4." ...). No hedging language. Pattern: NaturalBackwalls `ModStrings.cs`. |
| `Mod.cs` | Enable the `Translations.Register` and `RegisterOptions` lines from S1. |
| `src/SmoothBorders/translations/SmoothBorders.pot` | Written by `python common/tools/Translations/translations.py smooth-borders --build`. |

Acceptance: solution build clean; `python common/tools/Translations/translations.py smooth-borders --build` exits 0 and writes the pot; the options screen itself is proved by test T1 in S5.

### S4 Border patch (complex) — depends on S3 (needs `Groups` and `Options.Load`)

Files in `src/SmoothBorders/`:

| File | Change |
|---|---|
| `Border.cs` | `public static int Setting(WorldGenSettings settings, string name)`: reads real T and R through `settings.GetIntSetting`; for `"WorldBorderThickness"` returns F (group setting, unknown-pair floor, or 0) when this world's thread-static "flat" flag is set, else the real value; for `"WorldBorderRange"` returns 0 when flat, else real. Holds the `Patched` flag and the per-thread state (flat flag, vanilla rectangles). |
| `WorldGen_DrawWorldBorder_Patch.cs` | `[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]`. **Transpiler**: replaces exactly one `ldstr "WorldBorderThickness"` + `callvirt GetIntSetting` and one for `"WorldBorderRange"` with `call Border.Setting`; any other match count returns the IL unchanged, logs "DrawWorldBorder has changed; borders stay as the game draws them" and leaves `Patched` false. **Prefix**: if `Patched`, calls `Options.Load()` once, clones `simData.cells`, runs the vanilla reverse patch on the copy with a fresh `new SeededRandom(0)`, a throwaway `borderCells` and a no-op progress callback, moves the four rectangles it added to `WorldGen.POIBounds` into thread-static storage, sets the flat flag (stays false when the probe throws; log `"[SmoothBorders] <what was skipped>: " + e.Message`). **Postfix**: merges per edge (larger of vanilla and flat: left wider, right smaller x, bottom taller, top smaller y) only when exactly four rectangles were added; otherwise logs and leaves the list; logs one line per world (world, group, F) and one per edge whose rectangle grew; clears the flag. |
| `WorldGen_DrawWorldBorder_Vanilla.cs` | `[HarmonyReversePatch]` stub with the method's exact signature (the implementer reads it from the decompile: `ilspycmd` on Assembly-CSharp.dll; the unit-01/02 scratchpad `decomp\` folder may be gone, re-decompile if so). |
| `Mod.cs` | Unchanged apart from what S1 set (the all-or-nothing catch covers a failed reverse patch or transpiler). |

Pattern: failure handling per store record blk-f04d3902a855 (case 3: catch, log, leave vanilla) and "Yielding to Other Mods" (blk-016318d101d7); Harmony patch class naming `<Type>_<Method>_Patch` (code-style "Harmony Patches"). Thread rule: worldgen thread, touch no Unity objects.

Acceptance: solution build clean (0 warnings, 0 errors). Behaviour is proved by S6; as a quick pre-check the manager can run one `Invoke-Worldgen.ps1 -Mods SmoothBorders SNDST-C-100-0-0-0` after S4 and read Player.log for the per-world lines (the driver is existing tooling, no S2 needed).

Risks to watch: (a) the reverse patch must see the original IL, not the transpiled one (Harmony's `HarmonyReversePatchType.Original`); (b) `Options.Load()` must read the file each call (PLib `ReadSettings` caching is unchecked; if it caches, S6 test 4 will show it and the fix is to read through `POptions.ReadSettings` after clearing or to read the JSON directly); (c) other transpilers on `DrawWorldBorder`.

### S5 Tests without a colony (default) — depends on S3 (S4 not needed)

Files: `src/SmoothBorders.Tests/Options.cs` and `Mapping.cs` (names free), `Looks.cs`.

- **T1** (`NoColony = true`): open `POptions.ShowDialog(typeof(Options))` as natural-backwalls' `Looks.cs` does; assert the first entry is the text block, its label alignment is UpperLeft, four sliders/fields with labels "Primary asteroids (1-16)", "Small asteroids (1-7)", "Moonlets (1-6)", "Tiny moonlets (1-3)", defaults 4/3/2/1, tooltips exactly "Game default: N."; screenshot (`t.Screenshot`), shown to the user via SendUserFile.
- **T2** (`NoColony = true`): every world in `SettingsCache.worlds` maps per the design 2.1 table (the table is a data array in the test); unknown pair (12,3) gives 9; (0,0) gives 0; a hand-written config with 0, 99 and negative values loads clamped to 1..max. Uses `Groups` directly, so it needs no worldgen.

Pacing: no building or destroying, so the pacing records (test-pacing tag) only apply to the screenshot waits (`t.WaitRealSeconds(1.5)` before the capture, as Looks.cs).

Acceptance: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -Filter '*Options*','*Mapping*'` (the filter takes globs of id or name; use the actual test names) all pass; screenshot reviewed by the manager. Because both tests are NoColony and the run holds no other test, the run loads no save.

### S6 Worldgen tests (complex) — depends on S4, S5 (shared Support) and M2 (Prepare, config restore)

Files in `src/SmoothBorders.Tests/`: `Support.cs`, `Flatness.cs`, `Identity.cs`.

**Vanilla-baseline switch (in test code).** `Support.VanillaOn()` (called from a test's `Prepare`) finds the mod's Harmony instance by id (`KMod`'s UserMod2 id is the mod's staticID, `Isochronous.SmoothBorders`; read `harmony.Id` rather than hard-coding if it is reachable), calls `Harmony.UnpatchAll`-style `harmony.Unpatch(AccessTools.Method(typeof(WorldGen), "DrawWorldBorder"), HarmonyPatchType.All, id)` and returns an undo that re-applies the mod's patch classes (`harmony.CreateClassProcessor(typeof(WorldGen_DrawWorldBorder_Patch)).Patch()` and the same for the reverse stub). This makes the baseline the unmodified game with the mod still loaded; nothing about it ships. The undo runs from the harness (S2 item 3), so a live session reused for another run starts with the mod's patches on. If Harmony cannot restore the transpiler cleanly (check: a world generated after the undo is flat again), fall back to a `public static bool Border.Active` in the mod, set only by tests.

**Log capture in test code.** `Prepare` subscribes `Application.logMessageReceivedThreaded` (worldgen logs arrive on the worldgen thread) and collects `[SmoothBorders]` lines; the undo unsubscribes. The mod gets no diagnostics API for tests.

Tests (design section 4, ranges updated):

| # | Test | Coordinate and Prepare | Checks |
|---|---|---|---|
| T3 | Flat at defaults | `V-SNDST-C-<seed>-0-0-0`, `SNDST-C-<seed>-0-0-0`, `SNDST-A-<seed>-0-0-0`; `Prepare` writes the default config | Per design "How test 3 checks flatness": every non-interior `WorldContainer`, F from the mod's own mapping fed with `SettingsCache.worlds.GetWorldData(world.worldName)`, every row/column depth < F is Unobtanium except Space-zone cells, the count of Unobtanium at depth F is zero outside natural neutronium (small threshold, each hit photographed); log shows no "rectangle grew" line |
| T4a | Flat at the maxima | `SNDST-C`, config Primary 16, Small 7, Moonlets 6, Tiny moonlets 3 | Same flatness; no template cell inside depth < F that is not neutronium (no template holes); "rectangle grew" lines expected on merged-group worlds and recorded |
| T4b | Flat at a mixed set | `SNDST-C`, config Primary 7, Small 1, Moonlets 5, Tiny moonlets 2 | Same flatness, both directions around the defaults; also proves the options were re-read for the new world |
| T5 | Vanilla identity at defaults | Baseline world: `Prepare = VanillaOn`; mod world: default config, same coordinate | The baseline test writes the `WorldGenSpawner` spawnables of building type and geyser prefabs (id, cell) per world to a file in the session folder (`Session.Dir`); the second test (declared after it in the same class, so Discover order puts it later) reads and compares: identical lists; no "rectangle grew" lines. One seed per cluster in the default run, two more `Explicit` |

Ordering: tests run by assembly, class full name, declaration order; T5's two methods live in one class, baseline first. A missing baseline file fails the compare test with a clear message (it can only happen under a filter).

Acceptance: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -TimeoutSec 1200` (raise TimeoutSec: several worlds, about 20 s each plus embark); all pass with the config restored afterwards (compare `config\SmoothBorders` before and after). Any test failing for an unknown reason: stop, screenshot setup and failure, bring them to the user (stuck-tests record). Keep `Worldgen.Current`-based tests out of `t.Area`.

Open assumption carried here: two generations of one coordinate in one session give identical spawnables (the premise of T5). The first baseline-vs-baseline run settles it; T5 includes a self-check variant (two vanilla worlds compared) as an `Explicit` test if T5 fails.

### S7 Review (default) — depends on S2-S6

A reviewer unit over the main-repo diff and the common diff (as OM-006 units 12, 13, 15). Findings fixed by short units; the manager decides.

### S8 WorldgenDriver sweep (default) — depends on S4, S2 item 6, S6

Run (manager launches, one at a time, background):

- `pwsh common/tools/WorldgenDriver/Invoke-Worldgen.ps1 -ListFile <all-cluster-coordinates.txt> -Mixing all -Stories all -Label vanilla` (no mod: baseline)
- the same with `-Mods SmoothBorders -Label sb-default`
- the same with `-Mods SmoothBorders -ModConfig 'SmoothBorders=<max-config.json>' -Label sb-max` (every group at its maximum)

The list holds one seed per cluster coordinate prefix of both modes (coordinatePrefix from the cluster files; a short script, or a `Select-String` over `StreamingAssets` cluster files, builds it). Compare the three JSONL files: no coordinate that succeeds in the vanilla run fails or hangs in the other two. Failures are checked against design 4 test 6 (start-template overlap, unplaceable guaranteed template). Output stays under `common/tools/WorldgenDriver/runs/` (untracked). Estimated 20 s per coordinate, 25 per session; a few dozen clusters per sweep, so each sweep is tens of minutes.

Acceptance: a three-column table (coordinate, vanilla, default, max statuses) in the unit report, with zero regressions or each regression explained.

### S9 Docs and Workshop material (default) — last; depends on S7 and S8

| File | Content |
|---|---|
| `mods/smooth-borders/README.md` | What it does, the four settings and ranges, new worlds only, how the groups follow vanilla; pattern: ovagros-overhead README |
| `mods/smooth-borders/CHANGELOG.md` | Few short plain bullets (changelog style): flat border at a thickness per asteroid kind; four settings; new worlds only |
| `mods/smooth-borders/publish/workshop-description.txt` | Plain statements, no hedging language; credit sources plainly |
| `mods/smooth-borders/publish/workshop-tags.txt` | Category (`tweaks`), then every DLC tag the mod supports (supportedContent ALL: Base Game, Spaced Out!, The Frosty Planet Pack, The Bionic Booster Pack, The Prehistoric Planet Pack, The Aquatic Planet Pack), as in ovagros-overhead |
| `mods/smooth-borders/publish/preview.png` | 512x512 through `common/tools/MakePreview/` (`title_preview.py`, "Graystroke is the game font, local only"). Source image: a before/after of one edge; the "after" from an S6 screenshot (`t.Screenshot`) of a generated world's bottom edge, the "before" from `.context/tasks/OM-007-smooth-borders/before-border.png`. Shown to the user before it is used (no content changes to Workshop images without showing them). |

Release (zip, GitHub release, Workshop upload, Discord notice) is the release-mods skill, run on the user's word, not part of this plan.

Acceptance: release prerequisites present (CHANGELOG `## Unreleased`, preview 512x512, description, tags); user approves the preview and text.

## 2. Dependency map

```
S1 -> M1
S1 -> S3 -> S4
S2 (common) ----> M2 ------------------------\
S3 -> S5 ---------------------------------> S6 -> S7 -> S8 -> S9
S4 ------------------------------------------^
S2 (item 6) ------------------------------------> S8
```

Plain list: S1; S2 ∥ S1; S3 after S1; S4 after S3; S5 after S3; M1 after S1; M2 after S2; S6 after S4, S5, M2; S7 after S6; S8 after S4, S6, M2; S9 after S7, S8.

**Parallel without two units committing on one branch at once:**
- S1 and S2 may run together: S1 commits in the main repo, S2 commits only inside `common/` (own branch). M2 (pointer bump) waits until no main-repo unit is mid-commit.
- S3, S4, S5 all commit in the main repo's branch: strictly one at a time (S3, then S4 and S5 in either order). S5 is the natural slot while S4's reviewer or the user waits; they cannot run simultaneously.
- Any step that launches the game (S2 verification, S5, S6, S8) runs alone.
- S8's list-building and S9's text drafting may be drafted beside S6 only as files outside the repo or without committing; simplest to keep them sequential.

Suggested order: S1, S2 (together) -> M1 -> S3 -> S5 -> S4 -> M2 -> S6 -> S7 -> fixes -> S8 -> S9.

## 3. Open questions and assumptions

Questions for the user:
1. **Chinese translation** for 0.1.0? Nine mods shipped one; the string set is small (intro plus eight names and tooltips). Default assumed: English only, pot file in place.
2. **Preview art:** the before/after composition in S9 is the plan; confirm or give another.
3. **Workshop tags:** all six DLC tags assumed from `supportedContent: ALL`.

Assumptions (each checked in the named step):
- PLib 4.25.0 `TextBlockOptionsEntry` can be subclassed as the design says, and a wrapping PLabel lays out in the two-column grid (T1 screenshot).
- `POptions.ReadSettings` reads the file at each call (S4 risk b; T4b shows it).
- Harmony can unpatch and re-apply the mod's `DrawWorldBorder` patches in a running game (S6; fallback `Border.Active`).
- Two generations of one coordinate in one session are identical in spawnables (S6, T5).
- The decompile folder of units 01/02 may be gone from the scratchpad; S4 re-decompiles `WorldGen` (`DrawWorldBorder` signature, `POIBounds` type) with ilspycmd if so.
- A hand-written config.json for T4 is written by `Prepare` into `Documents\Klei\OxygenNotIncluded\mods\config\SmoothBorders\` and restored by the S2 snapshot; if S2 slips, T4 must not be run.

## 4. Report back on the packet

- **Pointers that did not apply:** the 16 extra copies of the try/catch record and the comment, braces, indentation, harmony-patches records in the `mods` brief (read about as much as the one copy needed); `tools/ModCheck` overview in the `common` brief.
- **Pointers that applied:** blk-87272da04dee and blk-c8a18ea4088d (read by id with `ks read --store C:/dev/ONI-Mods`), TestRunner and WorldgenDriver entity records (used through the source), the try/catch and Yielding records (S4).
- **Missing from the briefs:** the two generated-worlds records again (tag-only), and anything on PLib's config folder naming (found on disk, not in the store).
- **Reasons that did not match:** none.

## 5. ICON tooling friction

- `ks read --store C:/dev/ONI-Mods blk-87272da04dee blk-c8a18ea4088d` worked as written (first try).
- The `mods` brief in the packet again spent its budget on 17 copies of one try/catch record (third unit in a row to see this; the store-level fix is in the open items of plan.md). I read the generated-worlds records by id because the packet's section 1 named the tag route in the prompt, not the brief.
