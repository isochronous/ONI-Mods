# Unit 13 — implement: tests delete every save file they create

## Objective
The user: "Make sure these tests are deleting the save files they create." Today's worldgen test runs left 61 colony-summary folders in `Documents\Klei\OxygenNotIncluded\RetiredColonies\<colony name>\` (each a `<colony>.json` and `00000\<dupe>_cycle_0000.png`). No test `.sav` was found under `Documents\Klei\OxygenNotIncluded` (only the user's own "The Aftermath" saves), but confirm that for every path the harness uses.

## What to do
1. **Find everything a test run writes into the player's game folders**: saves (`save_files`, `cloud_save_files`, autosaves), colony summaries (`RetiredColonies`), timelapse images, worldgen caches, anything else — for both worldgen tests (`[OniTest(Worldgen = ...)]`, which embark a new colony) and fixture tests. Read the harness (common/tools/TestRunner) and the game code that writes them (decompiled code path in units/07-implement-report.md).
2. **Make the harness delete them**, in the harness (not in each test): record exactly what each run created (by name/path from the run itself, not by date or pattern guessing), delete it when the world is done or at the end of the run, and have the warden or the next launcher delete leftovers from a crashed run, the same way the config backup is recovered (unit 05). Never delete anything the run did not create; never touch the user's own colonies or saves. If the game must not write some of these at all (e.g. a setting or patch that skips colony summaries during tests), that is acceptable if simpler and as reliable — say which you chose and why.
3. **Leftovers from today:** units/11-test-report.md §7 lists each of its runs with the colony names its worlds got (it notes some names repeat or could match the player's own colonies — so match by more than the name: creation time inside a run's window, the run's Player.log, the summary's cluster/seed). Earlier runs today (units 05, 08, 10) need the same from their run folders. Build the list of RetiredColonies folders that test runs created, from evidence (run logs, Player.log, run artifacts with colony names), and put it in your report with the evidence per folder. **Do not delete them** — the manager shows the list to the user first.
4. Prove it: run one short worldgen test (e.g. `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders -Filter '*AtDefaults*'` or the smallest worldgen test) and show the before/after listing of RetiredColonies and save folders: nothing new remains.

## Scope
- IN: common/tools/TestRunner/** (branch OM-007-smooth-borders inside common/; commit there; the manager bumps the pointer), its README and the facts for the store records it changes (in the report).
- OUT: mod code; deleting any existing folder.
- Acceptance: the proof run's summary line verbatim with run id, before/after listings, the leftovers list with evidence. The game must be free (check; never close a game you did not start); tests at human speed. Commit inside common/ with one terse subject line, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

## Report
- File: .context/tasks/OM-007-smooth-borders/units/13-implement-report.md (do not commit it)
- Model tier: complex
