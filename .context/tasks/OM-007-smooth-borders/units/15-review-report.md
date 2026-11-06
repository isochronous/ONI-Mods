# Unit 15 — review report: OM-007 Smooth Borders + TestRunner/WorldgenDriver harness

Review only. No file changed apart from this report. The game was not launched and no in-game test was run.

## Verdict

No critical findings. I found no path where the mod's worldgen patch leaves a hole, doubles a rectangle, leaks the flat state into another world or thread, or crashes another mod's worldgen. The two moderate findings are one ordering bug in the launcher's leftover restore, which can lose the player's mode and save pointer, and one flatness check that is blind on Space-zone lines. The rest are minor or nits: store drift, test trimming and fail-safe coverage.

## Findings

### Critical
None.

### Moderate

**M1. The launcher uses up leftover backups while a game is running. That game then overwrites what was restored.** (Pass 3: integration and side effects.)
- Where: `common/tools/TestRunner/Invoke-OniTests.ps1:181-187` (`if (-not (Get-LiveGame)) { Restore-LeftoverConfig; Restore-GameMode; Restore-SaveKey }`) runs before the "game is already running" check at `:190-192`. The check only looks for the live session's pid file, not for any `OxygenNotIncluded` process.
- Scenario A: a run dies with no warden (machine restart, or both processes killed), which leaves `kplayerprefs.yaml.onitest-mode` / `.onitest-savekey` and possibly `config-backup\<Mod>`. The player then starts their own game, which boots with the test mode. An agent starts a run, as happened today while "a game that is not ours is open". The launcher writes the player's mode and pointer back into `kplayerprefs.yaml` and deletes the kept files, then throws "already running". The player's game still holds the switched values in memory and writes `kplayerprefs.yaml` when it saves its prefs. The player's mode and last-save pointer are lost, and so are the kept copies.
- Scenario B (orphan game): the launcher is killed between `Start-Process steam` (`:490`) and `Start-Warden` (`:497`). That window can last up to 120 s while it waits for the process. The game is ours, but `game.pid` was never written, so the next launcher sees no live game. It restores `config-backup\<Mod>` while the orphan game is still running its tests. Any later config write by a test or Prepare in that game has no backup left, so `ModConfig.Restore` at its `Finish` finds nothing and the test value stays in the player's config. The mode and save pointer are restored under a running game, as in A.
- Fix expected: run the leftover block only when no `OxygenNotIncluded` process exists at all, after the running-game check, the same way `Invoke-Worldgen.ps1:68` checks before its own restore. The launcher's `finally` block already guards this way. Also start the warden before launching the game: have it wait for the pid, or pass it the run's `game.pid` path. That way no window exists in which the launcher is the only restorer.

**M2. The flatness check cannot see a missing wall on lines in a Space zone.** (Pass 5: evidence. Priority 1, "a hole".)
- Where: `mods/smooth-borders/src/SmoothBorders.Tests/Support.cs:361`. In `Look`, `!InSpace(cell)` drops every non-neutronium cell of a `SubWorld.ZoneType.Space` zone from `Gaps`, whatever the element.
- The game's rule (`WorldGen.DrawWorldBorder`, decompiled lines 1376-1380) skips only vacuum or void, and only on lines whose edge point lies in terrain tagged `RemoveWorldBorderOverVacuum`. On every other line it walls any cell. The test already models that rule exactly (`EdgePointSkipsVacuum`, used in `ExpectFlat` at `:575`), so the blanket Space excuse is both redundant and too wide. The top edges of Spaced Out asteroids are mostly Space zones. A regression that drew no wall, or too thin a wall, there over regolith, gas or liquid would pass `ExpectFlat`. Only the "Deep" half of the check would still catch anything there.
- Fix expected: remove the `InSpace` excuse from `Look`. In `ExpectFlat`, excuse a gap only when its line is one the game leaves unwalled over vacuum (`EdgePointSkipsVacuum`), keeping it for any element, because gas can drift into those cells during the settle. Re-run `Identity.TerraniaFlat` and one mini cluster to confirm there are no new false positives.

### Minor

**m1. A stale kept file is restored over newer player state.** (Pass 1: correctness.)
- Where: `common/tools/TestRunner/GameMode.ps1` `Restore-GameMode` / `Restore-SaveKey`, called from `Invoke-OniTests.ps1:184-186` and `:473-474`.
- Scenario: after a restart mid-run, the kept files survive while the player keeps playing for days (a new colony, a mode switch). The next run "restores" the old pointer and mode over the player's current ones. The mods.json leftover-backup path has the same shape, but that one is older behaviour.
- Fix expected: compare-and-swap. Restore the pointer only while the current value still points at a test save (a path under a `runs\` folder or named `fixture.sav`). Restore the mode only while the current value is still the one the run set. Otherwise drop the kept file with a warning.

**m2. The mod's fail-safes have no test in the suite.** (Pass 5. Store best practice 4, blk-46b28a0040e8: "Test error paths".)
- Where: `WorldGen_DrawWorldBorder_Patch.cs:50-68` (transpiler mismatch sets `Patched` false), `:105-113` (probe exception falls back to the game's border and removes the probe's rectangles), and `:143-150` / `:199-205` (rectangle count other than 4). Unit 07 checked the transpiler path offline, and that check is not in the repo.
- Fix expected: one `NoColony` test that calls `WorldGen_DrawWorldBorder_Patch.Transpiler` on a copy of the real method's instructions with one `ldstr "WorldBorderRange"` duplicated. It asserts `Border.Patched == false` and that the list is unchanged, then re-runs the transpiler on the original IL (or restores `Patched`) in a `t.Cleanup`. That one test covers the path most likely to fire after a game update. The other two paths can stay untested.

**m3. Redundant Explicit tests worth cutting (user rule 2026-10-19: a representative sample).** (Pass 5.)
- `Compatibility.EmptyEmpteraCamera` / `CameraDiagnosis` (`Compatibility.cs:435`, `:535`) cannot fail on anything the mod does. It only asserts that a second asteroid exists, and is otherwise logs and screenshots of a camera problem that has been fixed (`ShowAsteroid`). Cut it.
- `Clusters.cs` has 62 tests over 18 clusters. The vanilla-identity property comes from one merge rule that is the same for every cluster, and the night sweep already proved flatness on all 43 clusters at defaults and maxima. A representative set would be:
  - one base-game DLC cluster (Ceres);
  - the Skewed Asteroid in both modes (fixed seed, volcano quirk);
  - MiniFlipped (upside-down);
  - one new and one classic Spaced Out DLC cluster;
  - one mini cluster at maxima.
  The candidates to cut are CeresShatteredBase, PrehistoricShatteredBase, AquaticBase, CeresMini, PrehistoricClassic, AquaticClassic, MiniFrozen, MiniSwampy and MiniBadlands (about 28 tests).
- Omnificia: Beach, Forest, Garden, IceCaves and Swamp × {defaults, maxima} (10 tests) are the same mod's worlds on the same code path as Sandstone. Keep one besides Sandstone.
- The determinism controls `Identity.TerraniaVanillaAgain`, `EmptyEmpteraAloneAgain`, `EmptyIslandsAloneAgain` and `MiniBaseAloneAgain` each answer "is the baseline deterministic". Keep `MiniBaseAloneAgain`, which documents the unseeded-geyser exception, and one vanilla one.
- Real gap (besides m2): none beyond M2.

**m4. WorldgenDriver `-ModConfig` accepts `.` and `..` as the mod name.** (Pass 2: trust boundary, the operator's own input.)
- Where: `common/tools/WorldgenDriver/Invoke-Worldgen.ps1:56`. The regex `^([^=\\/]+)=(.+)$` rejects slashes but not dots.
- Scenario: `-ModConfig '..=x.json'` makes `$live` equal to `mods\config\..`, which is the `mods` folder. `Restore-Configs` then does `Remove-Item mods -Recurse -Force` and copies back what it backed up. `.` does the same to all of `mods\config`. Each needs a typo, but the cost is every local mod and `mods.json`.
- Fix expected: require `^[A-Za-z0-9_.-]+$` and reject `.` and `..`, or require the name to equal an existing `mods\local\<Name>` folder.

**m5. Store records left stale by the harness changes. Land them at completion through the write path.** (Pass 4.)
- common, `tools/TestRunner`:
  - `standards/error-handling.md` Principle 3 and "Mods.json and the warden" (blk-23a64c67c430, blk-73f72ee0a0b7) cover only mods.json. They leave out the config backup, mode, SaveFilenameKey, colony-summary and timelapse blocks, and the WMI-started warden.
  - `domains/entities.md`: the Files table, Watch-OniGame and the Invoke-OniTests options leave out `-Mode`, `config-backup`, `kplayerprefs.yaml.onitest-*` and `ModConfig`.
  - `standards/naming-conventions.md` leaves out `--onitest-config`, the launcher's `-Mode` and the `OniTest` option `Prepare`. The new dot-sourced `GameMode.ps1` and `ModConfig.ps1` break its "Scripts use Verb-Noun" rule, so either name them that way or add a rule for helper scripts.
  - `standards/code-style.md` Layout table leaves out `ModConfig.cs`, `GameMode.ps1` and `ModConfig.ps1`.
- common, `tools/WorldgenDriver`: `standards/error-handling.md` Principle 4 (blk-6b5e188da28e) and the launcher notes (blk-64bfa31b7823), plus the entities function table (blk-2c4e50d4b9fc), leave out `-ModConfig` and `Backup-Configs`/`Restore-Configs`.
- ONI-Mods:
  - testing blk-c8a18ea4088d (per-test options) has no `Prepare` row.
  - blk-87272da04dee still says "Consecutive tests with one coordinate share the world". It is now the coordinate and the Prepare. It also does not mention `-Mode` for base-game coordinates.
  - blk-1b45a760667b (launcher options) has no `-Mode`.
- `mods/smooth-borders` has no `.context/` node and no README, unlike its sibling mods. That is why the main store's brief matched nothing for it. The README is S9 work; the node (plus the plan's Decisions) should land at completion.

**m6. Harmony ordering assumptions are not quite what the comments say.** (Pass 3.)
- `WorldGen_DrawWorldBorder_Patch.cs:90-91`: `Priority.Last` is not "after every other prefix". Another mod's `Priority.Last` prefix registered after this one runs after it, and may draw from `rnd` after the copy is taken. The vanilla rectangles would then differ and identity would be lost silently (the Empty Worlds case again). Correct the comment, or log in `Merge` when the probe's rectangles and the drawn rectangles disagree at a tie depth.
- The postfix (`:177`) has the default priority. A postfix from another mod that runs before it sees the flat rectangles rather than the merged ones. `[HarmonyPriority(Priority.First)]` on the postfix makes the merge come straight after the original.
- Checked and not a finding: when an earlier prefix returns false, Harmony in the game's `0Harmony.dll` skips this prefix too, because its `ref WorldgenSimData` parameter counts as "affects original" (`MethodCreator.AddPrefixes` / `AffectsOriginal`). `__state` is then null and the postfix only calls `Border.End()`.

**m7. A missing baseline errors instead of failing with a message.** (Pass 4.)
- Where: `Clusters.cs:87` and `Compatibility.cs:479`. `File.ReadAllLines(Support.PlacementFile(...))` throws `FileNotFoundException` when the maxima test is filtered without its baseline. The other helpers assert `File.Exists` with "run the baseline test with this one".
- Fix: use the same assert.

### Nits
- n1. `Flatness.cs:43`: "deeper than the game's thickest on every edge (15 at most)". The game's thickest is T+R = 16, the same as the slider's maximum. The walk's step `{-2..1}` makes +6 rare, which is why it held in practice. Reword to "the game's wall seldom reaches 16".
- n2. `plan.md` Progress (unit 10) and the unit 15 spec say the four ints are `[Option]` + `[Limit]`. The code is `[DynamicOption(typeof(RangedIntOptionsEntry))]` + `[Limit]` with no `[Option]`, which is correct for PLib's `ExecuteConstructor`. Only the plan's wording is off.
- n3. An empty `%LOCALAPPDATA%\IsochronousOniTests\config-backup\` folder stays after every run (seen now). It is harmless and already noted by unit 08.
- n4. `Invoke-OniTests.ps1:170`: `Start-Warden` quoting escapes inner `"` but not a trailing backslash, so `"C:\dir\"` would swallow the closing quote. No current argument ends in `\`.
- n5. Information, not a defect here: `kplayerprefs.yaml` `SaveFilenameKey/` right now points at `C:\dev\ONI-Mods-OM-008\common\tools\TestRunner\runs\om008-art2-panel-20261021-101650\fixture.sav`. The OM-008 checkout runs the old harness, which does not restore it. This branch's harness will keep that test path as "the player's" value. Bumping `common` in OM-008 after this merges stops it.

## Areas with no findings (and the passes behind them)
- **Flat draw**: compared against the decompiled `WorldGen.DrawWorldBorder`, game build U59-744825. Transpiler `FindRead` needs exactly one `ldstr` per key followed by the `GetIntSetting` call. Swapping `callvirt GetIntSetting(string)` for `call Border.Setting(WorldGenSettings,string)` keeps the stack shape. With range 0, the clamp `Mathf.Max(-0, Min(num+step, 0))` is 0, so every line is exactly F deep. The game's `rnd` is a fresh `SeededRandom(0)` local (decompiled line 945) and is consumed the same way flat or not.
- **Probe**: the reverse patch is `Original` (unpatched IL). The cell copy is a `Clone()` of `simData.cells`; nothing else in `simData` is written. `GetTerrainCellsForTag` is pure. `borderCells` is a throwaway. The original writes `this.POIBounds` (not the `ref` parameter), which is the same list. The probe's four rectangles are removed again, and on an exception the prefix removes whatever the probe added. `CopyOf`: `KRandom` fields `inext`, `inextp`, `SeedArray` and `SeededRandom.seed` confirmed by decompiling firstpass. `SeedArray` is cloned.
- **Merge**: depth is width (left, right) or height (bottom, top). A tie keeps the game's rectangle. A count other than 4 leaves the list alone. I found no path that doubles a rectangle.
- **Thread and world state**: `[ThreadStatic]` flat and thickness, `End()` at the start of the prefix and in the postfix, `Border.Setting` reachable only from the patched method. If the original throws, the next prefix on that thread resets it. I found no leak path.
- **All or nothing**: `Mod.cs` unpatches by its Harmony id on any `PatchAll` failure.
- **Groups and options**: every (T,R) pair in the game's StreamingAssets worldgen yaml (grep plus a per-file tally): (10,6), (5,2), (4,1), (4,2), (3,1), (2,1), (1,0), (0,0). All map in `Groups.ForWorld`. Ranges are 1..T+R (16, 7, 6, 3), defaults T−R (4, 3, 2, 1), and `[Limit]` agrees with `Groups`. The intro `[DynamicOption]` is declared first. `Options.Load()` is per world and clamps. `[ConfigFile(SharedConfigLocation: true)]` gives `mods\config\SmoothBorders`, named by the assembly (`POptions.GetConfigPath`, PLib source), which matches the names the launcher passes.
- **Harness, other paths**:
  - `ModConfig.cs` writes a `.tmp` backup, renames it, and deletes the backup only after the restore.
  - `Session.Finish` runs the undo and the config restore before the status line.
  - Prepare/WorldKey separation; NoColony tests run first with a hand-off to `Continue`.
  - Save, summary and timelapse blocks (`Session.Active` guards, `Reflect.cs` handles in `SelfCheck`).
  - WMI warden: `warden.log` shows no "cannot restore" lines across the new-harness runs (2026-10-20 01:22–01:25), so `$env:LOCALAPPDATA` resolves in the WMI-started process.
  - `Stop-LiveSession` waits for the warden to remove `live`, which it does last, so a fresh launch does not race the old warden except past the 30 s deadline.
  - WorldgenDriver: backup and restore in `try`/`finally`, and a leftover is restored only after its own running-game check.
- **README, CHANGELOG, `.github`, sln**: the TestRunner README matches the code (`-Mode`, Prepare, config backup, Workshop registration). The label map and issue forms are consistent. The sln has both projects nested.

## Passes run
- **Read**:
  - the full diff of both repos: mod (all 13 source files), tests (all 8 files), sln, README, `.github`; common (all 15 files);
  - `plan.md` Decisions and Progress;
  - the packet's records through `ks read`: blk-87272da04dee, blk-c8a18ea4088d, blk-33fb80177cba, blk-3ca89d57dfde, blk-cb417a1e3041, blk-2c4e50d4b9fc, blk-1e7cd48f4614, TestRunner code-style, error-handling, naming and entities, WorldgenDriver error-handling and code-style;
  - widened through a `ks brief --tag 'mod-compatibility && game-update-compatibility'`, then `ks read` blk-1b45a760667b, blk-46b28a0040e8, blk-4ed03be05da6.
- **Decompiled** (ilspycmd, scratchpad): `ProcGenGame.WorldGen` (DrawWorldBorder, RenderToMap call site, GetTerrainCellsForTag), `KRandom`, `SeededRandom`, and `0Harmony` `MethodCreator` (prefix skip rule).
- **Game data**: border pairs tallied from `StreamingAssets/**/*.yaml`.
- **PLib source** (`incubator/plib`): `OptionsEntry.CreateDynamicOption` / `ExecuteConstructor`, and `POptions.GetConfigPath`.
- **Built**:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`: 0 errors.
  - `dotnet build SmoothBorders.Tests.csproj -c Release -p:ModDeployFolder=none --no-incremental`: 0 errors, 3 warnings, all the existing CS0618 warnings in TestRunner (`TestBehaviour.cs:219`, `Worldgen.cs:259`, `:292`). Neither SmoothBorders project has a warning.
- **Read-only checks of the player's files**: `kplayerprefs.yaml` (line format, current `SaveFilenameKey`) and `warden.log`. I wrote nothing there.
- **Not run**: the game and in-game tests, per the unit's limits.

## Packet feedback
- **Pointers that did not apply once opened**:
  - blk-3ca89d57dfde (ModCheck overview) matched only because it names `Invoke-OniTests.ps1`. It says nothing this change touched.
  - blk-647210c6a78a and blk-6c2bbf052519 (tools overview) were not needed.
  - The glossaries were not opened.
- **What governs this work but did not appear**: the main store's testing records that this change makes stale. blk-1b45a760667b (launcher options) and blk-46b28a0040e8 (best practices, "Test error paths") were reachable only by tag. The packet's main-store brief matched nothing for `mods/smooth-borders` because that mod has no `.context/` node (m5).
- **Pointer reasons that did not match their records**: none. Each stated reason matched its record.

## ICON tooling friction
None. `ks read --store ... <id>` and `ks brief --store ... --tag` worked first time. The decompiler miss (`ilspycmd -t HarmonyLib.MethodPatcher`: no such type in the game's Harmony build) is the project's toolchain, not ICON.
