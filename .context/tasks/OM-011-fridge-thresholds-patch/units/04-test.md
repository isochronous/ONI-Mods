Task: OM-011 — in-game tests for Fridge Thresholds Extended
Lens: test. Branch OM-011-fridge-thresholds-patch, C:\dev\ONI-Mods. Do not commit. Jeremy has approved ONE in-game run of this suite (2026-11-04); ask before any second run beyond one fix-and-rerun of your own test code.

Build:
1. Add mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests following the TestRunner kit pattern used by other mods (see common/tools/TestRunner/README.md and an existing *.Tests project, e.g. the one added in commit 88214fb). Add it to ONI-Mods.sln like the others.
2. Representative tests only (one per behaviour/code path):
   a. Freezer: build a Freezer (Psyko.Freezer building), set the thresholds component to High 50 / Low 20, put food in to ~60% of capacity -> FULL port is green (1); drop to ~10% -> red (0); back to ~35% -> still matches hysteresis state. Compare with a vanilla Refrigerator under the same settings as a control.
   b. One IR fridge (e.g. SimpleFridge): the thresholds component exists on the built building and the same High/Low behaviour drives its port.
   c. Copy settings Freezer -> Freezer carries SendGreenOnLow / ActivateValue / DeactivateValue.
   d. First signal after load for the Freezer (review finding M1): if the harness supports save/reload (see the harness README), cover it; otherwise note it as untested.
   Reach other mods' types by reflection only in the test project too.
   Pace steps in game time (memory: tests at human speed; never build/destroy frames apart).
3. Run: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850` (check the exact -Steam parameter syntax in the script's help). Never pass -KeepGame. Never stop the runner mid-run. If a test fails for an unclear reason, stop, take screenshots of setup and failure as the harness allows, and report rather than retrying blindly.
4. If a harness change is needed, make it in common/ and report it (do not commit inside the submodule).

Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md — files created, the exact command, the runner's pass/fail output copied verbatim, relevant Player.log lines (patch-applied log lines from FridgeThresholdsExtended, any exceptions), and what remains unverified.
Model tier: complex
