# Unit 04 — implement: resolve review findings

Step 2: Fix the findings in units/03-review-findings.md listed below. Ticket: OM-009.

Files to modify: mods/jumper-bridges/** (Patches.cs, BridgeFootprint.cs, JumperBridge.cs, ModStrings.cs, translations/*, JumperBridges.Tests/JointPlates.cs, CHANGELOG.md, README.md, publish/workshop-description.txt) as each finding needs.

Findings to resolve:
- M1: add a test that two plates meeting end to end at one cell connect and carry power.
- M2: add ONE representative test of a kept vanilla refusal (e.g. plate end inside a solid tile or a door is refused). One case per behaviour, not every refusal.
- m1: tooltip (ModStrings.cs key, .pot, zh/ko/ru .po) and workshop-description.txt must say exactly what is true: both joint plates, ends on a power port or another joint plate's end; not on a wire bridge end. Short, player-facing, no hedging. Rebuild then run `python common/tools/Translations/translations.py mods/jumper-bridges --build` and show it passes.
- m2: add a short plain CHANGELOG bullet under Unreleased for the fixed crash when placing a tile over a bridge end node.
- m3: replace the per-call list allocation with a non-allocating check.
- m5: inherit stored wire connections only when some bridge has an *end* on the cell, fixing it once for wire bridge middles and plate centres.
- m6: add the "no circuit before the plate" baseline and the portsNotOverlapping flag check to BatteryPlateLamp, matching Power.cs.
- NOT in scope: m4 (note only), m7 (store records, the manager handles them).

Pattern to follow: existing tests and patches in the same files; tests paced to game time like the rest.
Prior reports: units/01-explore-findings.md, units/02-implement-report.md, units/03-review-findings.md.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — actual output tail in the report.
  - translations.py check passes — actual output in the report.
  - Do NOT launch the game.
  - Commit all created/modified files with one terse line, e.g. `OM-009: joint plate review fixes`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/04-implement-report.md
  - Must contain: per-finding change summary, new test names, build and translations output verbatim.
  - Model tier: default
