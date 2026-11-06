Task: OM-011 — one in-game run of the Fridge Thresholds Extended suite through oni-moono
Lens: test. Branch OM-011-fridge-thresholds-patch, C:\dev\ONI-Mods. Do not commit. Do not edit Steam settings, the mod, or the tests.
Jeremy approved ONE run (2026-11-04) with his Steam launch options set to start the game through oni-moono. He reports the latest Moono passes command-line arguments through.

Context: units/04-test-report.md (runs 1-4; run 1 is the earlier Moono failure: Moono rejected --onitest-* args, and the launcher only watches for an `OxygenNotIncluded` process). .context/testing/integration-testing.md "Launching with oni-moono".

Steps:
1. Before running, read the launcher (common/tools/TestRunner/Invoke-OniTests.ps1 and the warden) to see how it detects the game process, and check what process name a Moono launch produces (read the oni-moono clone at C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\oni-moono, and note it may be older than Jeremy's installed version). Report what you expect to happen.
2. Run once: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850`. No -KeepGame, never stop the runner mid-run, no foregrounding or polling the game window, no rerun whatever the result.
3. Confirm from Player.log that the run was really on Moono (first line "Moono starting ... .NET=...").

If the launcher can't follow the Moono process, do NOT change the harness: report what change it needs (Jeremy welcomes harness changes, but that is a separate approval).

Report: append a "Run 5 (oni-moono)" section to C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md with the verbatim runner output, the Moono first log line, the FridgeThresholdsExtended log lines, any exceptions, whether mods.json was restored, and what (if anything) the harness needs. Reply with a 3-line summary with counts.
Model tier: default
