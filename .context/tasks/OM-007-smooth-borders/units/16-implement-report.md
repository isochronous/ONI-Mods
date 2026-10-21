# Unit 16: implement report. Fixes from the S7 review (code and build only)

The game was not launched and no in-game test was run. All harness checks ran in a scratch sandbox, using a fake game process and no Steam. Nothing on the player's side was written: not `Documents\Klei`, not `%LOCALAPPDATA%\IsochronousOniTests`, and not mods\local (every build used `-p:ModDeployFolder=none`).

## Commits
- common, branch OM-007-smooth-borders: **3d554ca** `OM-007: TestRunner leftovers wait for no game, warden starts before the game, prefs restored only over the run's values, plain mod names; WorldgenDriver plain -ModConfig names`.
- ONI-Mods, branch OM-007-smooth-borders: **407db04** `OM-007: flatness check sees gaps in Space zones, transpiler fail-safe test, representative Explicit tests`.
- Not committed: the `common` pointer (the manager bumps it), `plan.md`, the handoff and unit files, and this report.

## Per finding

**M1 (leftover restore under a running game; warden started too late).** Changed.
- `common/tools/TestRunner/Invoke-OniTests.ps1:210-226`: the leftover block now runs only while no `OxygenNotIncluded` process exists. It has moved after the definitions and covers config, mode, save pointer and the stale live folder. Before it, a launcher that finds a warden still running with no game waits up to 200 s for that warden to exit (`Get-Warden`, `:193`, a CIM lookup of `pwsh` processes whose command line holds `Watch-OniGame.ps1`). Such a warden belongs to a launcher killed in the launch window, so it owns the files.
- `:529-543`: the warden now starts before `Start-Process steam` and gets `-PidFile <live\game.pid> -StartTimeoutSec 150`. The launcher still writes `game.pid` once it sees the game.
- `:558-563` (finally): if a warden was started but no game was seen (the "did not start within 120 s" throw, or any failure before the game appeared), the launcher kills its warden by pid and does the restore itself. This prevents a double restore later, which could otherwise clobber the next run's backup.
- `Watch-OniGame.ps1`: `-GamePid` is now optional, and `-PidFile` and `-StartTimeoutSec` are new. `Find-Game` (`:37`) takes the pid file first. Failing that, it takes an `OxygenNotIncluded` process started after the warden, and then writes `game.pid` itself so a later launcher finds the session. With no game before the timeout it restores at once (`:68`).
- The `-KeepGame` path is unchanged: no warden, the launcher restores. If that launcher is killed, M1's "no game at all" rule now makes the next launcher's leftover restore safe.

**m1 (stale kept file restored over newer player state).** Changed: compare-and-swap in `GameMode.ps1`.
- `Restore-GameMode` (`:59`): a run only ever switches to the other mode, so the run's value follows from the kept one (kept `1` means the run set `0`; anything else means it set `1`). It restores only while the file holds the run's value. If the file already holds the kept value it reports `unchanged`. Otherwise it reports `dropped`: the kept file is removed and the file is left alone.
- `Restore-SaveKey` (`:132`): restores only while the pointer still points at a test save. `Test-TestSaveKey` (`:121`) counts a path inside `...\runs\<run>\...` or a file named `fixture.sav`. Equal to the kept value gives `unchanged`; anything else gives `dropped`.
- Both now return an object `{Action, Value, Current}`, which used to be a bare value or a bool. `Write-PrefsRestore` (`Invoke-OniTests.ps1:198`) reports it (a dropped value always as a warning), and the warden logs it.

**M2 (flatness check blind on Space-zone lines).** Changed. `mods/smooth-borders/src/SmoothBorders.Tests/Support.cs`:
- `InSpace` is deleted, and `Look` (`:359`) now records every non-neutronium cell nearer than F as a gap.
- In `ExpectFlat` (`:575-588`), a gap is excused only on a line whose edge point lies in terrain tagged `RemoveWorldBorderOverVacuum` (the existing `EdgePointSkipsVacuum`). On those lines only vacuum, gas or liquid is excused (`IsVacuumOrFluid`, `:407`): the game left those cells as vacuum and fluids flow in during the settle. A solid on such a line is still a gap.
- The excuse is off entirely when the world sets `DrawWorldBorderForce` (new `BorderPair.Force`, `:251`).
- The rule comes from the decompiled game, build U59-744825 (ilspycmd, my scratchpad):
  - `WorldGen.DrawWorldBorder` lines 1376-1380 / 1388-1393 / 1412-1416 / 1424-1429: `if (boolSetting || (elementIdx != vacuum && != void && flag) || !flag)`, where `boolSetting = GetBoolSetting("DrawWorldBorderForce")` and `flag` = the edge point lies in a `RemoveWorldBorderOverVacuum` terrain cell. Nothing in it depends on the zone type.
  - `SaveLoader` lines 1018-1026 offset each asteroid's overworld polygons by its world position, so `EdgePointSkipsVacuum`'s cluster-coordinate test matches the game's.
- I found no case the game leaves unwalled that only the `InSpace` excuse covered. The one widening over a vacuum-only excuse is gas or liquid on skip lines, which is settle drift into cells the game skipped. It is said in the code comment.
- This needs the in-game proof below, because a false positive would only show in game.

**m2 (fail-safes untested).** Changed. New `SmoothBorders.Tests/FailSafes.cs`, one NoColony test `FailSafes.ChangedMethodLeftAlone` (`:54`), in the default suite (not Explicit).
- It reads `DrawWorldBorder`'s original IL through `PatchProcessor.GetOriginalInstructions`, which is present in the game's `0Harmony.dll` (checked by decompiling).
- Control: on the real IL the transpiler turns `Border.Patched` on. The instruction count is unchanged, exactly 2 instructions change, and both become `call Border.Setting`.
- Altered IL: (a) the thickness `ldstr` and its `GetIntSetting` call duplicated; (b) both removed. In each case `Border.Patched` goes false and the listing (opcode and operand per instruction) comes back identical.
- It also asserts that `Border.Patched` was true at load, and puts the value back in `t.Cleanup`.
- ModCheck against the game's assemblies passed for the class (output below).

**m3 (redundant Explicit tests).** Changed. The full list is below. `Clusters.cs` goes from 62 to 21 tests and `Compatibility.cs` from 74 to 63; `Identity.cs` is unchanged. The `Clusters` class summary now names the set and the nightly WorldgenDriver sweep.

**m4 (`-ModConfig` accepts `.` and `..`).** Changed.
- `common/tools/WorldgenDriver/Invoke-Worldgen.ps1:56-60`: the name must match `^[A-Za-z0-9_-]+(\.[A-Za-z0-9_-]+)*$`, so `.`, `..`, `...`, leading or trailing dots, and any `\` or `/` are refused before anything is touched.
- The same hazard existed in TestRunner, where `-With ..` reached the game's `ModConfig` backup and restore of `mods\config\..`. It is closed in two places with the same rule:
  - the launcher refuses such a `-Mods`/`-With` name (`Invoke-OniTests.ps1:77`);
  - `ModConfig.SetNames` (`ModConfig.cs:42`, `IsFolderName` `:51`) leaves an unsafe name out with a warning.
- Both READMEs state the rule.

**m7 (missing baseline throws).** Changed. `Clusters.cs:76` and `Compatibility.cs:446` assert `File.Exists` with "run the baseline test with this one" before reading.

**Nits**
- n1: `Flatness.cs:43` now says the game's thickest is T + R = 16, which the walk seldom reaches.
- n3: the empty `config-backup` folder is removed after a restore in three places:
  - the game's `ModConfig.Restore` (`ModConfig.cs:105`, `RemoveEmptyRoot` `:110`);
  - `Restore-ModConfigBackup` (`ModConfig.ps1:32`);
  - WorldgenDriver `Restore-Configs` (`Invoke-Worldgen.ps1:148`). There the empty folder made every following run warn "Leftover config backup".
- n4: `ConvertTo-CommandLineArgument` (`Invoke-OniTests.ps1:185`) applies the Windows argv rules: backslashes are doubled before a quote and before the closing quote, and an empty value becomes `""`.
  - Used for the WMI command line and for the `Start-Process` fallback. The fallback used to pass an unquoted array, so a path with a space would have split.
  - `Start-Warden` now returns the warden's pid.
- n2 (plan wording) and n5 (OM-008 pointer): not changed. n2 is the plan's text (out of scope), and n5 is information only.

**m5 (store drift) and m6 (Harmony ordering).** Not changed. m5 lands at completion, per the packet. m6 is mod code, which is OUT of scope and was not in the unit's list.

## Tests cut and kept (m3)

**Clusters.cs: kept 21**, the review's representative set:

| Tests | Why kept |
|---|---|
| CeresBase Vanilla/Flat/Same | the base-game mode with a worldgen-DLC cluster |
| SkewedBase Vanilla/Flat/Same/AtMaxima | the skewed asteroid (fixed seed 7, volcano quirk) in the base game, at defaults and maxima |
| Ceres Vanilla/Flat/Same | a new-layout Spaced Out DLC cluster, every kind |
| CeresClassic Vanilla/Flat/Same | a classic Spaced Out DLC cluster |
| Skewed Vanilla/Flat/Same/AtMaxima | the skewed asteroid in Spaced Out, at both edge values |
| MiniFlipped Vanilla/Flat/Same/AtMaxima | the upside-down asteroid (MiniFlippedStart), and the mini cluster at maxima |

**Clusters.cs: cut 41.** Each cluster here runs the same merge rule and the same flat draw as a kept one, and the nightly sweep checks every cluster flat at defaults and maxima.
- Named in the review's cut list (28 tests):
  - CeresShatteredBase, PrehistoricShatteredBase, AquaticBase, CeresMini, PrehistoricClassic, AquaticClassic, MiniFrozen, MiniSwampy (Vanilla/Flat/Same each);
  - MiniBadlands (Vanilla/Flat/Same/AtMaxima).
- Outside its named list but outside its representative set too (13 tests). The review's set has no place for them, and none is the only cover of a kind, mode, mechanism or edge value. **The manager may want to restore these:**
  - PrehistoricBase, Prehistoric and Aquatic (Vanilla/Flat/Same each): other worldgen DLCs on the same code path as Ceres.
  - MiniRadioactive (Vanilla/Flat/Same/AtMaxima): MiniFlippedWarp, the same upside-down mechanism as the kept MiniFlippedStart.

**Compatibility.cs: cut 11.**
- EmptyEmpteraCamera, together with its helper CameraDiagnosis. It cannot fail on anything the mod does; it diagnosed a camera problem that has since been fixed.
- EmptyEmpteraAloneAgain and EmptyIslandsAloneAgain: duplicate "same world twice" controls.
- Omnificia Forest, Garden, IceCaves and Swamp, Flat and AtMaxima each (8 tests): the same mod's worlds on the same code path as Sandstone.

**Compatibility.cs: kept 63.**

| Tests | Why kept |
|---|---|
| MiniBase Alone/Flat/Same/Plays/AtMaxima, and the same five for MiniBaseBase | the only cover of a mod that builds its own asteroids (RenderOffline), in both modes |
| MiniBaseAloneAgain | the determinism control that documents MiniBase's unseeded geyser pick (`ExpectSameSpots`) |
| EmptyEmptera / EmptyIslands Alone/Flat/Same/Plays/AtMaxima | the only cover of a mod that draws from the game's `rnd` before the probe (the `CopyOf` fix) |
| Voidria, VoidriaMini, VoidriaBase (×5 each) | vacuum edges in `RemoveWorldBorderOverVacuum` terrain, the case M2 now judges more strictly, in both modes and in a mini variant |
| Baator and BaatorMoonlet (×5 each) | another mod's own pairs and moonlet cluster |
| Omnificia (Sandstone) Alone/Flat/Same/Plays/AtMaxima | XL asteroids: identity at defaults and maxima |
| OmnificiaBeachFlat, OmnificiaBeachAtMaxima | the review's "keep one besides Sandstone"; Beach is an arbitrary pick among equals |
| Together* (10) | the only cover of all five mods enabled at once |

**Identity.cs: unchanged.** TerraniaVanillaAgain is kept as the one vanilla determinism control the review asked to keep. Terrania200/300 and the default-suite tests are not in the review's list.

**New: FailSafes.ChangedMethodLeftAlone** (m2), in the default suite.

## Build, parse and check output (verbatim)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none -nologo --no-incremental`:
```
    291 Warning(s)
    0 Error(s)
```
- None of the warnings is in SmoothBorders or SmoothBorders.Tests (grep for `smooth|TestRunner|WorldgenDriver` in the warning lines shows only TestRunner's three existing CS0618).
- The rest are other mods' existing CS0618/CS0649/CA1416.
- An incremental build before this one showed `5 Warning(s)` / `0 Error(s)`. A first build failed with `Clusters.cs(162,123): error CS1513: } expected`: my section cut had dropped the class's closing braces. I fixed that before the build above.

`dotnet build common/tools/TestRunner/TestRunner.csproj -c Release -p:ModDeployFolder=none -nologo --no-incremental`:
```
    0 Error(s)
    3 Warning(s)
C:\dev\ONI-Mods\common\tools\TestRunner\TestBehaviour.cs(219,28): warning CS0618: 'Object.FindObjectsOfType<T>()' is obsolete ...
C:\dev\ONI-Mods\common\tools\TestRunner\Worldgen.cs(259,14): warning CS0618: 'Object.FindObjectOfType<T>()' is obsolete ...
C:\dev\ONI-Mods\common\tools\TestRunner\Worldgen.cs(292,15): warning CS0618: 'Object.FindObjectOfType<T>()' is obsolete ...
```
These are the three existing warnings, the same ones the review recorded.

`dotnet build common/tools/WorldgenDriver -c Release -p:ModDeployFolder=none -nologo --no-incremental`:
```
    0 Error(s)
    0 Warning(s)
```

ModCheck (`dotnet run --project common/tools/ModCheck -- <game Managed> <scratch copy of SmoothBorders.Tests.dll + TestRunner.dll + SmoothBorders.dll>`):
```
OK SmoothBorders.Tests.FailSafes
OK SmoothBorders.Tests.FailSafes+<>c
OK SmoothBorders.Tests.FailSafes+<>c__DisplayClass6_0
OK SmoothBorders.Tests.FailSafes+<>c__DisplayClass7_0
OK SmoothBorders.Tests.FailSafes+<ChangedMethodLeftAlone>d__7
ALL CHECKS PASSED
```

PowerShell parse (`[Parser]::ParseFile`). The launcher was re-parsed after its last edit with the same result.
```
Invoke-OniTests.ps1: 0 error(s)
Watch-OniGame.ps1: 0 error(s)
GameMode.ps1: 0 error(s)
ModConfig.ps1: 0 error(s)
Invoke-Worldgen.ps1: 0 error(s)
```

## Dry checks (outside the game)

The sandbox is `scratchpad\dry\dry.ps1`. It copies the four TestRunner scripts and redirects:
- Documents\Klei to `box\klei`;
- `$env:LOCALAPPDATA` to `box\lad`;
- Player.log to a missing scratch file;
- the run mutex to `Global\IsochronousOniTestsDry`;
- the game's process name to `OniFakeGame`, a copy of PING.EXE started hidden.

The Steam launch line starts the fake game for 20 s (D2), or a plain PING that is no game (D1). The leftovers are a dead run's: mode kept `1` with the file at the run's `0`, the pointer kept as the player's with the file at a `runs\...\fixture.sav`, and a config-backup holding `{"Primary":9}` with `{"Primary":16}` live.

- **A, with a game running (not ours); old scripts (HEAD):** `-BuildOnly` restored everything under the running game. The kept files are gone, the mode is set to 1, and the pointer and config are put back. This reproduces the review's M1.
- **A, new scripts:** `-BuildOnly` touched nothing: mode 0, test pointer, both kept files present, config `{"Primary":16}`, backup present. A run attempt threw "The game is already running" and still touched nothing.
- **C, no game and no warden:** leftovers restored with warnings. Mode 1, the player's pointer, config `{"Primary":9}`. The config-backup folder is gone (n3).
- **C2, no game, but the player changed both since the dead run (m1):** the kept mode `none` was dropped because the file held 0, which the player chose since. The kept pointer was dropped because the file pointed at the player's NewColony.sav. Both kept files were removed and the player's values stay. Warnings:
  - `A kept game mode (none) left by an unfinished run was not put back: kplayerprefs.yaml holds 0, not the mode the run set, so it was chosen since.`
  - `A kept last-save pointer left by an unfinished run was not put back: kplayerprefs.yaml points at ...NewColony.sav, not a test save, so it was changed since.`
- **B, a warden in its launch window, no game ever:** the launcher printed `A test session's warden is still running with no game; waiting for it to finish.` and waited 11 s. The warden logged `waiting up to 12 s` → `no game started within 12 s; restoring` → mods.json, config, mode and pointer restored → `live session closed`. The launcher then found nothing left and made no second restore.
- **B2, the launcher killed after the game started but before `game.pid`:** the warden found the fake game by discovery and wrote `game.pid` itself. A `-BuildOnly` during the game touched nothing. After the game exited, the warden restored everything and removed the live folder.
- **D2, full launch path:** the warden was started through WMI before the launch (`waiting up to 150 s`, then `watching` the game's pid). The launcher reported no status (exit 2). After the game exited, the warden restored mods.json and the mode, and the live folder is gone.
- **D1, full launch path where no game comes up:** after 122 s `The game did not start within 120 s.` The launcher killed its warden (0 wardens left, and the log stops at `waiting up to 150 s`) and restored mods.json and the mode itself.
- **Compare-and-swap table** (`dry\cas.ps1`): 10/10 OK.
  - Mode: restored 1; unchanged; restored none; dropped 0; dropped none.
  - Pointer: restored; restored absent; unchanged; dropped (a new save); dropped (no line). The last one is the case that would have thrown in my first draft of the message helper; fixed before commit.
- **n4 quoting** (`dry\quoting.ps1`): 9/9 values round-trip through a child `pwsh -File` argv, including `C:\dir\`, `C:\with space\`, `say "hi"`, `a\"b`, `ends\\`, an empty value and a tab.
- **m4** (`dry\modconfig-names.ps1`, the real `Invoke-Worldgen.ps1` with no coordinates):
  - Rejected: `..`, `.`, `...`, `a\b`, `a/b`, `Smooth..Borders`, `SmoothBorders.` and `.hidden`. `' '` fails the Name=path form.
  - Accepted, stopping at "No coordinates given" before any file: `SmoothBorders`, `Mod.Name` and `my-mod_2`.
  - The TestRunner launcher copy rejects `-With ..`, `a\b` and `Mod.`.
- Afterwards: 0 OniFakeGame processes and 0 wardens running.

## Proposed in-game proof runs (for the manager to schedule)

1. **M2, m2 and the harness live path**, Spaced Out, about 5-6 min:
   `Invoke-OniTests.ps1 -Mods SmoothBorders -Filter "*FailSafes.*;*Identity.TerraniaFlat;*Flatness.AtMaxima"`
   - Expect FailSafes to pass (NoColony, a few seconds).
   - Expect TerraniaFlat and AtMaxima to pass with no new gaps. Space-zone top edges are now measured; any skipped cells are logged as "vacuum, gas or liquid cell(s) ... (RemoveWorldBorderOverVacuum)".
   - Afterwards check `warden.log`: `waiting up to 150 s for the game` → `watching` → `restored mods.json` → `live session closed`, and `kplayerprefs.yaml` back to the player's.
2. **M2 on a mini cluster and m7**, Spaced Out, about 5 min:
   `-Filter "*Clusters.MiniFlippedVanilla;*Clusters.MiniFlippedFlat;*Clusters.MiniFlippedAtMaxima"`
3. **M2 where the skip rule matters most** (vacuum edges in tagged terrain), about 4 min:
   `-Steam 3430682737 -Filter "*Compatibility.VoidriaAlone;*Compatibility.VoidriaFlat"`
4. Optional, to see m7's message, about 2 min: `-Filter "*Clusters.SkewedAtMaxima"` alone. Expect a fail with "run the baseline test with this one" instead of an error.

## Store facts affected (for m5 at completion)

- **common `tools/TestRunner`:**
  - error-handling blk-73f72ee0a0b7 "Mods.json and the warden": the warden starts before the game (pid file or discovery, 150 s start timeout), and a launcher that saw no game ends its warden and restores. Leftovers are restored only with no game process and no game-less warden, and kept mode and pointer go back only over the run's values.
  - Principle 3 (blk-23a64c67c430) as the review noted.
  - entities: `Watch-OniGame.ps1` has `-GamePid` optional and new `-PidFile` and `-StartTimeoutSec`. The launcher has new helpers `Get-Warden`, `ConvertTo-CommandLineArgument` and `Write-PrefsRestore`; `Restore-GameMode`/`Restore-SaveKey` return `{Action, Value, Current}`; `Test-TestSaveKey` is new. Mod names are checked in the launcher and in `ModConfig.SetNames`. The config-backup folder is removed when empty.
- **common `tools/WorldgenDriver`:** the `-ModConfig` name rule, and the empty `config-backup` folder is removed after a restore (blk-2c4e50d4b9fc, blk-6b5e188da28e, blk-64bfa31b7823).
- **ONI-Mods testing records:** Smooth Borders' Explicit sets shrank (Clusters' representative set, five fewer Compatibility controls and worlds), and there is a new default-suite NoColony test `FailSafes.ChangedMethodLeftAlone`. Any record listing the Clusters or Compatibility filters or counts is stale.

## Packet feedback

- **Pointers that did not apply once opened:**
  - TestRunner code-style and error-handling, and WorldgenDriver entities blk-2c4e50d4b9fc, applied: they are the records this change makes stale.
  - The ModCheck overview (blk-3ca89d57dfde), the tools overviews (blk-647210c6a78a, blk-6c2bbf052519) and the two glossaries were not needed.
- **What governs this work but did not appear:** nothing beyond what unit 15 already reported. The main store matched nothing for `mods/smooth-borders` (no `.context/` node), and the main-store testing records were reachable only by tag.
- **Pointer reasons that did not match their records:** none.

## ICON tooling friction
None. `ks read --store C:/dev/ONI-Mods/common <id>` worked first time for a block id and two document addresses.
