# Unit 11 — implement: FixtureArea.Clear misses buildings on no layer

Ticket: OM-009. Work in the `common/` submodule (its own git repo, already on branch OM-009-jumper-heavy-wire). Jeremy's standing rule: fix the TestRunner harness when a test needs it; no workarounds in test code.

Root cause from units/10-debug-findings.md (read it): `FixtureArea.Clear` (common/tools/TestRunner/.../FixtureArea.cs:112-124) collects only objects on `Grid.Objects`. A bridge whose ends were given up to other owners and whose middle sits on no layer survives the clear and leaks into the next test (Jumper Bridges' BridgeLinks rings it into the next bridge's group).

Do:
- Make Clear also destroy every building (complete and under construction) whose placement cells touch the rect, found generically — the harness must not know about any mod's types.
- After the clear, log a warning naming anything built through World.Build in the cleared area that is still alive.
- Keep existing behaviour for everything else; follow the harness's existing code style and logging.
- Update the TestRunner README/docs if they describe Clear.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` from C:\dev\ONI-Mods succeeds — output tail in report.
  - Do NOT launch the game.
  - Commit inside common/ with one terse line, e.g. `TestRunner: fixture clear removes buildings on no layer`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`. Do not commit in the outer repo; do not push.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/11-implement-report.md (outer repo; leave it uncommitted, the manager commits it)
  - Must contain: diff summary, the common/ commit hash, build output verbatim.
  - Model tier: default
