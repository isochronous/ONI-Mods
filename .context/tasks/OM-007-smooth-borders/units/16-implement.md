# Unit 16 — implement: fixes from the S7 review (code and build only, NO game launch)

## Objective
Resolve the review findings in units/15-review-report.md (read it; finding ids below are its ids).

- **M1** Invoke-OniTests.ps1:181-192: leftover config/mode/save-pointer restore runs before the "game already running" check and only knows the live pid file. Restore leftovers only when no OxygenNotIncluded process exists at all (as Invoke-Worldgen.ps1:68 does); start the warden before the game, so a launcher killed during the launch window still has a warden.
- **m1** a kept mode or save-pointer file that survives a restart must not overwrite the player's newer value: restore only when the current value is still the test's value (record what the run set, or compare against it).
- **M2** SmoothBorders.Tests/Support.cs:361: drop the blanket `!InSpace(cell)` excuse; rely on the existing `EdgePointSkipsVacuum` check, which follows the game's rule. If you find a case the game itself leaves unwalled that only the InSpace excuse covered, say so with the decompiled code; do not guess.
- **m2** add one NoColony test that runs the transpiler on altered IL (e.g. the GetIntSetting read for the thickness key removed or duplicated) and checks `Border.Patched` comes back false and the code is returned unchanged.
- **m3** cut redundant Explicit tests per the user's rule (2026-10-19): "comprehensive tests don't necessarily mean 'every single one', it means 'a representative sample'". Cut what the review lists (EmptyEmpteraCamera, the redundant Clusters tests, the extra Omnificia tests, duplicate "same world twice" controls) unless a test is the only cover for a distinct behaviour or code path; keep one case per asteroid kind, mode, mod mechanism and edge value. List every test cut and every test kept with the reason. The nightly WorldgenDriver sweep is the all-clusters check.
- **m4** Invoke-Worldgen.ps1:56: reject `.`, `..`, empty names and anything with a path separator in -ModConfig names.
- **m7** Clusters.cs:87, Compatibility.cs:479: a clear assertion message when the baseline is missing, not an exception.
- **Nits**: Flatness.cs:43 "15 at most" → the game's thickest is 16; remove the empty config-backup folder after a restore; warden-start argument quoting with a trailing backslash.

## Scope
- IN: common/tools/TestRunner/**, common/tools/WorldgenDriver/** (commit in common on branch OM-007-smooth-borders; the manager bumps the pointer), mods/smooth-borders/src/SmoothBorders.Tests/** (commit in ONI-Mods on branch OM-007-smooth-borders). READMEs where behaviour changes.
- OUT: mod code under mods/smooth-borders/src/SmoothBorders (no finding needs it); store records under .context (m5 lands at completion); launching the game or in-game tests — a game that is not ours is open; never touch it.
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` Warning(s)/Error(s) lines verbatim, TestRunner and WorldgenDriver builds, PowerShell parse check of each changed .ps1 (0 errors), dry checks outside the game for M1/m1/m4 (scratchpad copies, fake processes) with output. List the in-game proof runs the manager should schedule (narrowest filters, expected time). Commits: one terse subject line each, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

## Report
- File: .context/tasks/OM-007-smooth-borders/units/16-implement-report.md (do not commit). If the Write tool refuses it, put the full report in your final message.
- Must contain: per finding, the change and file:line or "not changed" with why; tests cut/kept list; build/parse output verbatim; dry checks; proposed in-game proofs; store facts affected; ICON tooling friction (or none).
- Model tier: complex
