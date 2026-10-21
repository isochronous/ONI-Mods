# Unit 13 report: test runs no longer leave saves or colony summaries in the player's folders

(Saved by the manager from the unit's hand-back message; the unit's Write tool refused .md report files. Condensed; the full leftover evidence table is in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\676240ed-29f0-48be-b5c6-6462cdcaf87f\scratchpad\leftovers\out.md, built by evidence.py in the same folder.)

## Fix
common 91b1602 on branch OM-007-smooth-borders, "OM-007: TestRunner writes no colony summary or timelapse image; summary checks the player's folders".
- Patches.cs: `RetireColonyUtility_SaveColonySummaryData_Patch` (skips, logs "skipped the colony summary of <name>"); `Timelapser_WriteToPng_Patch` (skips unless the preview of a save inside the run folder; logs "skipped a timelapse image"); `SaveLoader_Save_Patch` uses `Session.InRunFolder`.
- Session.cs: `InRunFolder(path)` with a trailing separator (fixes `StartsWith(Dir)` matching a sibling run folder).
- Reflect.cs: `TimelapserPreviewScreenshot`, `TimelapserPreviewSaveGamePath` with SelfCheck.
- Invoke-OniTests.ps1: lists save_files, cloud_save_files, RetiredColonies before and after; prints "Player's folders: nothing new in ..." or names new entries; never deletes. README updated.
- Why stop the writes instead of deleting afterwards: generated colony names collide with the player's (today's proof run generated "Crashpad", one of the player's 2023 colonies), and the fixture colony is "Mod Test", the player's own name; deleting cannot undo an overwrite. A fixture run on 2026-10-18 (run-20261018-020806) overwrote the player's RetiredColonies\Mod Test\Mod Test.json at 02:09:05 — not recoverable.

## What the game writes (from decompiled code)
- Saves: `SaveLoader.Save` — already blocked outside the run folder.
- RetiredColonies\<colony>\<colony>.json: `RetireColonyUtility.SaveColonySummaryData`, called from SaveLoader.Save and also from achievement checks (`ColonyAchievementTracker`), retire/victory/video screens and the Retired Colonies screen — now skipped.
- RetiredColonies\<colony>\<world>\<asteroid>_cycle_0000.png: `Timelapser` on a new colony's first cycle — now skipped.
- WorldGenDataSave.worldgen: game scratch, deleted before each generation — left alone.
- save_files\testsavefile: `InitializeCheck.Awake` writes and deletes it on every main-menu load — vanilla, left alone.
- kplayerprefs.yaml SaveFilenameKey/: left pointing at the last test save (open item).

## Proof (real game)
- `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -Filter '*Identity.TerraniaVanilla;*Identity.TerraniaFlat;*Identity.TerraniaSame' -Label sb-nosave`
  "Session done: 3 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; ... of 3; 59.8 s real time" / "Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies" (runs\sb-nosave-20261019-144519). Player.log: blocked saves to Crashpad and Suave Rat's Nest, two "skipped a timelapse image". Listing of 3064 entries identical apart from steam_autocloud.vdf and the save_files folder date.
- `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagrosOverhead -Label oo-nosave`
  "Session done: 17 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; ... of 17; 135.6 s real time" / "Player's folders: nothing new in ..." (runs\oo-nosave-20261019-144726). Player.log: "skipped the colony summary of Mod Test". Mod Test.json keeps its 2026-10-18 02:09:05 date.
- Build: TestRunner "3 Warning(s) 0 Error(s)" (pre-existing CS0618); PowerShell parse errors 0.

## Leftovers (nothing deleted)
110 RetiredColonies folders, 133 files, every file matched to a run's Player.log (screenshot line within 5 s, or inside a colony folder the run created within 120 s of its blocked first save); 0 folders mix test and other files. 108 from today (13 runs, 12:14:29–14:09:37), 2 from 2026-10-15 (gen8: Beyond; gen9: Data Abyss). Per-run folder lists and per-file evidence: out.md.
- Not listed: Tragic Sanctuary (2026-10-15 22:39:26, probably gen7-20261015-223838, weaker evidence); Mod Test (the player's, overwritten 2026-10-18 — do not delete).
- Left alone, no run linked: Citadel, Crashpad, Crew, Dangerous Spacecamp, Devland, Frigidaire, March 2023 Test Map, Meteor, Mod Test, ModTest, Spaced In, Swanky Lab, The Aftermath, The Perfect Dreamland, Tragic Sanctuary.

## Store facts to land (common store, TestRunner node)
entities.md Patches blk-6e02b4b02569 (two new patches), Session blk-7f9302c43486 (InRunFolder), Reflect blk-741c81a8a7c1 (two fields), Invoke-OniTests blk-5b0c52216ccc (Player's folders line), business rules blk-ad9c4a3b39af ("No save, colony summary or timelapse image goes to disk outside the run folder"), files blk-aabf5834e2e4 (summary.txt line), error-handling principle 3 blk-23a64c67c430 (colony summaries and timelapses; names collide incl. Mod Test), naming-conventions blk-dbf5ebc0be89 (summary.txt). New fact: the game writes RetiredColonies from achievement checks and Timelapser, not only saves; testsavefile on every main-menu load; WorldGenDataSave is scratch.

## Open item
kplayerprefs.yaml SaveFilenameKey/ points at the last test save after a run (Resume Game uses the newest save file, WorldGenScreen clears the key, so the effect is small). Could be saved and restored like the game mode.

## Friction
Write tool refused the .md report (sub-agent guard; harness, not ks). ks read worked.
