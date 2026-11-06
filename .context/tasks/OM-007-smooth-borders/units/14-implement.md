# Unit 14 — implement: TestRunner harness fixes (code and build only, NO game launch)

## Objective
Three harness changes in common/tools/TestRunner, plus one small test fix. Write and build them; do not run any in-game test.

1. **Restore the last-save pointer.** After a run, kplayerprefs.yaml's `SaveFilenameKey/` points at the run's last test save. Save it before the run and put it back after, the same way the game mode is saved and restored (GameMode.ps1, the launcher's and the warden's restore paths, the leftover recovery for a run that died). User approved: "Restore it".
2. **NoColony tests run at the main menu first.** User, 2026-10-19: "mod config screen test is loading into the game to open the mod config screen". `Session.Start` (Session.cs:378) skips the colony only when every selected test is NoColony; in a mixed run (e.g. the SmoothBorders default suite) the Dialog tests run inside the fixture colony. Run the selected NoColony tests at the main menu before any worldgen or fixture test, then go on with the rest of the run. Keep the all-NoColony path working.
3. **The warden must survive the launcher being stopped.** Stopping a run (the launching pwsh killed with its process tree) also killed the warden (Watch-OniGame.ps1), so mods.json and the mod config were not restored until the manager ran the warden by hand. Start the warden so it outlives the launcher's process tree, or make the next launcher's recovery restore mods.json from `mods.json.onitest-backup` when the live game is gone (config and mode recovery already exist there). Pick one, say why.
4. **Test fix:** `SmoothBorders.Tests/Compatibility.EmptyEmpteraCamera` fails in base game ("the cluster has a second asteroid (was null)"): base-game clusters have one asteroid. Make it skip or not apply in base mode, following how the file handles mode elsewhere.

## Scope
- IN: common/tools/TestRunner/** (branch OM-007-smooth-borders inside common/; commit there), its README; mods/smooth-borders/src/SmoothBorders.Tests/Compatibility.cs (commit in ONI-Mods on branch OM-007-smooth-borders; do not stage the common pointer, the manager bumps it).
- OUT: mod code; launching the game; running Invoke-OniTests.ps1 except `-BuildOnly` if it does not deploy into the player's mods\local while the game runs (check that the game is not running first; if unsure, use dotnet build only).
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` output (Warning(s)/Error(s) lines) and the TestRunner build output, verbatim; PowerShell parse check of every changed .ps1 (0 errors); a dry check of change 1 and 3 outside the game where possible (e.g. call the restore functions on a copy of kplayerprefs.yaml in the scratchpad and show before/after). List the in-game proof runs the manager should run later (narrowest filters, expected runtime). Commit with one terse subject line and trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

## Context the packet does not carry
- units/13-implement-report.md (what the game writes; open item SaveFilenameKey), units/12-test-report.md §2.1 (latest harness changes), units/05 report (config backup/restore pattern).
- The user's desktop is taken over by every in-game run: do not launch the game.

## Report
- File: .context/tasks/OM-007-smooth-borders/units/14-implement-report.md (do not commit it). If the Write tool refuses the .md file, put the full report in your final message instead.
- Must contain: the diff summary per file, the build/parse output verbatim, the dry checks, the proposed in-game proof runs, store facts the change affects, ICON tooling friction (or none).
- Model tier: default
