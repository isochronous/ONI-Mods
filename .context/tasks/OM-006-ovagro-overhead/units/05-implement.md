# Unit 05 — implement: Step S1, scaffold mods/ovagro-overhead

Step S1: scaffold the mod so it builds as an empty mod with its test project, and register it everywhere a new mod is registered.
Ticket: OM-006

## Files to create/modify
As listed under "### S1 - Scaffold the mod" in units/04-plan-report.md (line 24 on), which follows the checklist in units/02-explore-report.md:
- mods/ovagro-overhead/: csproj, mod.yaml, mod_info.yaml (with `requiredDlcIds` for `DLC4_ID`, per plan.md Decisions D3), Mod.cs, the Tests csproj, and whatever else the S1 section lists for this step (README/CHANGELOG/publish material are later steps S7, S8; leave them out unless S1 lists a stub).
- ONI-Mods.sln: the solution folder, mod project and Tests project entries.
- README.md (root): one table row.
- .github/workflows/label-mod.yml: the label map entry.
- .github/ISSUE_TEMPLATE/bug_report.yml and feature_request.yml: "Ovagro Overhead" in the Mod dropdown (plan report A1).

## Pattern to follow
mods/just-fall-already (single-feature Harmony mod, no strings). Assembly/namespace OvagroOverhead, staticID Isochronous.OvagroOverhead, version 0.1.0.

## Prior reports
units/02-explore-report.md, units/03-design-report.md (DLC gating section), units/04-plan-report.md (S1).

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds with the two new projects in it; the report includes the actual build output tail (warnings and errors count). Do not claim success without showing it.
- mod_info.yaml's `requiredDlcIds` syntax is confirmed against the game's mod loader code in the decompiled source (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\ and ...\fp\); cite the class and line.
- No game patches or feature code in this step: Mod.cs only does what the template's entry point does.
- `git status --short` is clean after the commit; commit all created/modified files on branch OM-006-ovagro-overhead in one commit, message one terse line `OM-006: scaffold Ovagro Overhead` with the trailers:
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
- Do not launch the game, run releases, or touch .context/ beyond the report file.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/05-implement-report.md
- Must contain: the diff summary (files and what each holds), the build output verbatim (tail), the commit hash, any deviation from the S1 section and why, and a "Store reads" section.
- Model tier: default
