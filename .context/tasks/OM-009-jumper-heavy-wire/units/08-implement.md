# Unit 08 — implement: shared-end art and review 07 fixes

Ticket: OM-009 (branch OM-009-jumper-heavy-wire). Fix units/07-review-findings.md m1, m2, m3, m4 (code comments only — store records are the manager's).

- m2, per Jeremy's decision: a bridge end node shared by a joint plate and a wire bridge draws heavi-watt art (plain or conductive, per the plate) while any joint plate has an end on that cell, whichever was built first. When the plate is removed, the node switches to the remaining wire bridge's art; when the wire bridge is removed, it keeps/gets the plate's art. Use the existing NodeAnim mapping; re-art the existing node rather than recreating it if that is how the code already handles changes. README.md:21 must then be true.
- m1: tooltip (ModStrings.cs:24 + .pot + zh/ko/ru .po) must be accurate when the end is on another bridge's end. Short, plain, no hedging. Rebuild, then `python common/tools/Translations/translations.py mods/jumper-bridges --build`.
- m3: add ONE test: plate built first, then a wire bridge end on its end — valid, one circuit, tile on the shared end refused, end draws heavi-watt art; then deconstruct the plate and assert the end remains on the wire bridge's circuit with the wire bridge's art. Paced to game time like the other tests.
- m4: fix the stale/ambiguous comments at Patches.cs:205-209 and 281-282.

Prior reports: units/01-, 06-implement-report.md, 07-review-findings.md.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — output tail in report.
  - translations check passes — output in report.
  - Do NOT launch the game; do not push.
  - Commit all modified files, one terse line e.g. `OM-009: heavi-watt art on shared ends`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/08-implement-report.md
  - Must contain: per-finding change summary, new test name, build and translations output verbatim.
  - Model tier: default
