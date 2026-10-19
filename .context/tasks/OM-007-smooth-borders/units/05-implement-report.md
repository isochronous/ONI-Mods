# Unit 05 report: S2, TestRunner and WorldgenDriver changes in common/

**Status:** S2 code is done and committed in common/ on branch `OM-007-smooth-borders` (made from main 7687b3c), commit **`ef0115e1a302398bb14c1083496ffd68a4b28064`**, subject `OM-007: TestRunner Prepare hook and mod config restore; WorldgenDriver -ModConfig` with both trailers. Both projects build with no new warnings, and the PowerShell backup and restore logic passes offline checks.

**The in-game regression run has not happened.** When it was due, the player's own game was running: `OxygenNotIncluded.exe` PID 10016, started 09:08, command line with no test arguments, no live-session folder. I waited 30 minutes (10:44 to 11:14) and it was still running, so I left it alone. Invoke-OniTests refuses to launch while another game is up, and I did not close it. **The manager still needs to run:**

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls -Filter '*Generated*'
```

Run it once the game is closed. It is the regression for the worldgen path and the first real test of the config backup and restore (NaturalBackwalls' config folder exists). Compare `mods\config\NaturalBackwalls\config.json` before and after: SHA1 before was `8151C4E6F20B60CFC69236884AC7C2608B7DC066` (2026-10-07T14:59:39). Expect three lines in Player.log: `backed up the config of NaturalBackwalls`, then `put back the config of NaturalBackwalls`, and no leftover in `%LOCALAPPDATA%\IsochronousOniTests\config-backup`. `*Generated*` also selects the Explicit `Generated.Overview` (screenshots, 120 s timeout). Use `-Filter '*Generated.StartingAsteroid;*Generated.OtherPlanetoids'` for the two plain tests only.

## Diff summary per file (common/)

| File | Change |
|---|---|
| `tools/TestRunner/Kit/Attributes.cs` | `OniTestAttribute.Prepare` (string): names a `public static` method of the test's class, taking no arguments and returning void or `System.Action`. Doc comment covers when it runs, the undo, the world key and the failure behaviour. |
| `tools/TestRunner/Session.cs` | `TestCase.Prepare` (MethodInfo) is resolved in the constructor, which runs at Discover. Discover throws, naming the test, when the method is missing or has the wrong shape, or when `Prepare` is set without `Worldgen`. That makes the run `runner-error`, the same as the existing signature check. `TestCase.WorldKey` = `Worldgen + "\|" + <assembly>/<class>.<prepare>` (null for fixture tests). `TestCase.RunPrepare()` unwraps TargetInvocationException. `WriteOff` groups by `WorldKey`. New `Session.SetUndo` / `RunUndo` (an undo that throws is logged as a warning). `Continue` sets `Worldgen.PendingTest` and runs `RunUndo` before loading the fixture. `Finish` runs `RunUndo`, then `ModConfig.Restore()`, before writing the status line (a restore problem is appended to the status `detail`). `Start` calls `ModConfig.Snapshot()` before Discover. New `--onitest-config=NAME[;NAME]` option and live-request field `config=`, documented in the Session summary. |
| `tools/TestRunner/Worldgen.cs` | `Worldgen.PendingTest`. `Begin` (at the settled main menu) runs the previous world's undo, then this test's Prepare, keeping its undo, then applies the coordinate. A Prepare that throws sets `Failure = "Prepare threw: <Type>: <msg>"` and returns without leaving the menu. The next frame's existing write-off path fails that world's tests and the run goes on. `Fail` now puts the "the world could not be generated: " prefix into `Failure` itself. `GeneratedWorld.Key` (ctor now `(coordinate, key)`). `OnColonySpawned` clears `PendingTest`. |
| `tools/TestRunner/TestBehaviour.cs` | `otherWorld` compares `test.WorldKey` with `Worldgen.Current.Key`. |
| `tools/TestRunner/Patches.cs` | The write-off uses `Worldgen.Failure` as the full message and clears `PendingTest`. |
| `tools/TestRunner/ModConfig.cs` (new, internal) | Snapshot and restore of `KMod.Manager.GetDirectory()/config/<name>` for each named mod. The backup goes to `%LOCALAPPDATA%\IsochronousOniTests\config-backup\<name>`: `files\` holds the folder as it was, an `absent` marker means it did not exist. Each backup is written to `<name>.tmp` and then renamed. It is removed only after a completed restore. A leftover backup found at snapshot time is restored first. A failed snapshot throws, so the run ends as `runner-error` before any test can write a config. |
| `tools/TestRunner/ModConfig.ps1` (new) | `Restore-ModConfigBackup <configRoot>`, dot-sourced by the launcher and the warden. It puts every backup back (`.tmp` folders are only deleted) and returns the names it restored. |
| `tools/TestRunner/Invoke-OniTests.ps1` | `Get-LoadoutKey` hashes `mods\config\<mod folder name>` instead of `mods\config\<staticID>`. This fixes the plan's finding 1: before this, a mod's config never entered the key. `$configNames = Mods + With` are passed as `--onitest-config=` and as `config=` in live requests. Leftover config backups are restored: at start when no live game is running, in the fresh-game path (followed by a recomputed `$loadoutKey` before key.txt is written), and in the no-warden `finally` once no game process remains. The help text mentions the config backup. |
| `tools/TestRunner/Watch-OniGame.ps1` | New optional `-ConfigRoot` (passed by the launcher). After the game exits, it restores any config backup the game left and logs it to warden.log. This covers a game that died mid-run. |
| `tools/TestRunner/README.md` | New paragraph "Mod settings are put back" and a `Prepare` bullet in the per-test options list. |
| `tools/WorldgenDriver/Invoke-Worldgen.ps1` | `-ModConfig 'Name=path\to\config.json'` (repeatable or comma-separated). Each entry is checked before anything is touched: Name=path form, file exists, file parses as JSON. The folder `mods\config\<Name>` is backed up to `Klei\OxygenNotIncluded\worldgen-driver\config-backup\<Name>` (`files\` or `absent`, via `.tmp`). The backup happens inside the `try`. `Set-Configs` writes the file as `config.json` before each session, and the `finally` restores. A leftover backup is restored at the next start. The session line shows the config. Help text and an example are added. |
| `tools/WorldgenDriver/README.md` | Paragraph on `-ModConfig`. |

WorldgenDriver's C# is unchanged: the config is a file the game reads, so no driver option was needed.

## Design choices that differ from the plan's wording

- **Prepare runs in `Worldgen.Begin`, not in `Session.Continue`.** Begin runs at the settled main menu, which is what the attribute doc promises. Continue can run while a colony is still loaded. The undo runs at Begin (before the next world's Prepare), in Continue before a fixture load, and in Finish. "Whenever a different key comes up" is therefore covered: any new generation undoes first. Regenerating the same key also undoes and prepares again.
- **`Prepare` requires `Worldgen`.** Discover rejects `Prepare` without `Worldgen`. Fixture worlds carry no key, and S6 needs no fixture Prepare. Allowing it later means keying fixture loads too.
- **The config backup is at a fixed path** (`%LOCALAPPDATA%\IsochronousOniTests\config-backup`) instead of the run folder (`Dir\config-backup`). With a fixed path, the next launcher and the warden can find a leftover whatever `-OutDir` was. The game computes the path itself, so no path crosses Steam's command line (which cannot take spaces).
- **The warden restores leftovers too** (the plan only had the next launcher). Without it, a crash would leave a test config in place until the next test run, and the player could start their own game with it.

## Build and offline check output (verbatim tails)

```
> dotnet build common/tools/TestRunner/TestRunner.csproj -c Release -p:ModDeployFolder=none -nologo
C:\dev\ONI-Mods\common\tools\TestRunner\Worldgen.cs(207,15): warning CS0618: 'Object.FindObjectOfType<T>()' is obsolete: ...
C:\dev\ONI-Mods\common\tools\TestRunner\TestBehaviour.cs(212,28): warning CS0618: 'Object.FindObjectsOfType<T>()' is obsolete: ...
    3 Warning(s)
    0 Error(s)

> dotnet build common/tools/WorldgenDriver/WorldgenDriver.csproj -c Release -p:ModDeployFolder=none -nologo
    0 Warning(s)
    0 Error(s)
```

The 3 CS0618 warnings are not new. The same build of unchanged main (git stash) gives the same three, at Worldgen.cs(151), Worldgen.cs(184) and TestBehaviour.cs(212): `3 Warning(s) 0 Error(s)`. The line numbers moved only because of the added code. S2's "0 warnings" cannot hold for TestRunner without touching unrelated Embark/Setup code, so I left them.

Parse check of the four scripts: `0 parse errors` each (Invoke-OniTests.ps1, Watch-OniGame.ps1, ModConfig.ps1, Invoke-Worldgen.ps1).

`Restore-ModConfigBackup` on a scratch tree (A was present and changed by a test, B was absent and created by a test, C has an unfinished `.tmp`):
```
restored: A,B
A: {"original":true} sub: True nested-files-dir: False
B exists: False
C: {"c":1}
backup root left: 0 entries
second call: 0 restored
```

WorldgenDriver `Backup-Configs` / `Set-Configs` / `Restore-Configs`, taken from the script by AST, on a scratch tree:
```
backups: \Absent \Present \Absent\absent \Present\files \Present\files\config.json \Present\files\other.txt
during: Present={"sweep":16} Absent={"sweep":16}
Restored the config of Absent.
Restored the config of Present.
after: Present={"player":1} other=keep Absent exists=False
backup entries left: 0
```

`-ModConfig` validation (throws before anything is touched; no `mods.json.worldgen-backup` created):
```
threw: -ModConfig SmoothBorders: file not found: C:\nope\missing.json
threw: -ModConfig 'nofile' is not Name=path\to\config.json
False
```

The in-game paths of Prepare, undo and the game-side ModConfig are proved only by a game run: the regression above, and S6 as their first user.

## Store facts that changed (for the manager to land)

Main store (`C:/dev/ONI-Mods`):
- **blk-c8a18ea4088d** (testing/unit-testing.md, per-test options table): add the row `Prepare = nameof(M)` | "With `Worldgen`: a `public static` method of the test's class (void or `System.Action`) run at the main menu before the coordinate is applied; the returned action undoes it before the next world or the fixture, and at the end of the run; different Prepare = different world". Add a config note: "each `-Mods`/`-With` mod's `mods\config\<assembly name>` is backed up at run start and put back at the end, so a test may write it".

common store (`C:/dev/ONI-Mods/common`), TestRunner node:
- `domains/entities.md`:
  - Session blk-7f9302c43486: `RunUndo`/`SetUndo`; `--onitest-config`; Start snapshots configs; Finish undoes and restores before writing the status.
  - TestCase blk-18187e3cb8c6: `Prepare`, `WorldKey`, `RunPrepare`.
  - Worldgen blk-9e3323c536e4: "consecutive tests with one coordinate **and Prepare** share the world"; Begin runs undo and Prepare; `GeneratedWorld.Key`; `PendingTest`.
  - Kit table blk-c6e1619a47bb: add `Prepare` to the OniTestAttribute options.
  - Watch-OniGame blk-5f4f98f50aec: it also restores leftover config backups.
  - Files blk-aabf5834e2e4: add `ModConfig.cs` and `ModConfig.ps1`.
  - Business rules blk-ad9c4a3b39af: the loadout key's "shared configs" are now actually `mods\config\<folder name>`. The old code looked under staticID and never matched.
- `standards/code-style.md` Layout blk-3eafd0a33a08: add rows `ModConfig.cs` ("backup/restore of the mods' shared config folders") and `ModConfig.ps1` ("leftover-config restore shared by launcher and warden").
- `standards/error-handling.md` (around the generated-worlds paragraph): a Prepare that throws writes off its world's tests as `fail` "Prepare threw: ..."; an ill-shaped Prepare is a Discover error (`runner-error`); an undo that throws is a warning only; a config restore failure is appended to the session status `detail` and the backup is left.

WorldgenDriver node:
- `domains/entities.md` Invoke-Worldgen.ps1 blk-2c4e50d4b9fc: the `-ModConfig` parameter and its backup at `worldgen-driver\config-backup`.
- `standards/error-handling.md`: `-ModConfig` validation throws before any file is touched.

## Report back on the packet

- **Did not apply:** blk-3ca89d57dfde (ModCheck overview) and blk-647210c6a78a / blk-6c2bbf052519 (tools overview). They only name the script paths. The WorldgenDriver Driver.cs and Mod.cs records (blk-cb417a1e3041, blk-1e7cd48f4614) did not apply either: no C# change was needed there.
- **Applied:** the TestRunner code-style record (tabs, Allman, "options documented in three places", internal for harness-only types, the layout table) and the entities record. The plan report did most of the steering.
- **Missing:** nothing on PLib's config folder naming (same gap unit 03 found). Nothing on the fact that the game's `Action` type shadows `System.Action` inside `Isochronous.TestRunner` (the first build failed with CS0037/CS0077; `System.Action` must be written in full). That is a good one-line fact for the TestRunner code-style record.
- **Mismatched reasons:** none.

## ICON tooling friction

- `ks read --store C:/dev/ONI-Mods/common tools/TestRunner:standards/code-style.md` and `ks read --store C:/dev/ONI-Mods blk-c8a18ea4088d` worked first time.
- None otherwise.

## Other friction (not ICON)

- A Python heredoc through Bash turned `\t` in `path\to` into a tab: the memory note on backslashes again. I reverted the file and redid the edit with the Edit tool. `sed -i` on Session.cs also turned CRLF into LF; I restored it with unix2dos, and git normalises it anyway.

## Next steps

1. Manager: once the player's game is closed, run the regression command above and check the config hash and the Player.log lines.
2. M2: bump the submodule pointer to `ef0115e` (or later) in the main repo.
