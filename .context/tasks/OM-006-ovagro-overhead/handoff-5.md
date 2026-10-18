# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/just-fall-already`, `ONI-Mods.sln`, `README.md`, `.github`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/05-implement.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-9d52ccfdb3df · reference
  Key Code Paths
  mods/just-fall-already/.context/domains/entities.md#key-code-paths · names mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs, mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-0b8e3ef08773 · reference
  Example
  mods/vent-freeze-fix/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-0fe521f2b16f · reference
  Example
  mods/upgrade-planner/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-16cd4984fdb3 · reference
  Loops
  mods/motion-sensor-range/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-1b40623f402c · reference
  Loops
  mods/vent-freeze-fix/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-29fb02c618a8 · reference
  Loops
  mods/smart-weight-plate/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-2aba30d1f782 · reference
  Variables
  mods/corner-build-anything/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-308a838456d3 · reference
  Loops
  mods/storage-pod-redux/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-32ae0ce82848 · reference
  Example
  mods/bitshifter/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-353f3e62bd46 · reference
  Example
  mods/multichannel-critter-sensor/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-396cf026d230 · reference
  Variables
  mods/motion-sensor-range/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-3a88acdbb150 · reference
  Variables
  mods/upgrade-planner/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-431be121fc8c · reference
  Example
  mods/corner-build-anything/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-47fe26d5347a · reference
  Variables
  mods/salty-surf/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-483b9df9284d · reference
  Loops
  mods/multichannel-critter-sensor/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-486b038b51f1 · reference
  Variables
  mods/storage-pod-redux/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-5379b7234d12 · reference
  Example
  mods/motion-sensor-range/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-5b62e7507604 · reference
  Example
  mods/just-fall-already/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-6958eb513dec · reference
  Loops
  mods/sweep-zones/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-699b426eb8f6 · reference
  Variables
  mods/sweep-zones/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-6a8bea597750 · reference
  Example
  mods/detect-any-element/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-6b9561f02621 · reference
  Loops
  mods/refined-building/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-7261503b82df · reference
  Example
  mods/storage-pod-redux/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-74082ad9f366 · reference
  Variables
  mods/vent-freeze-fix/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-76e79f89b3c4 · reference
  Variables
  mods/just-fall-already/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-7bf2c80b2f8e · reference
  Loops
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-7d0c65d332f3 · reference
  Static class with the whole mod. A postfix on `Pathfinding.AddNavGrid` subscribes to each navig…
  mods/just-fall-already/.context/domains/entities.md#key-entities/fallpoke · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-7e5838a4ab04 · reference
  Loops
  mods/no-magic-fridges/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-8ce83687722f · reference
  Example
  mods/sweep-zones/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-8f46b5b1e7a3 · reference
  Loops
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-94c9c06009e9 · reference
  Example
  mods/refined-building/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-96641cb455da · reference
  Variables
  mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-9998cf9f18c6 · reference
  Variables
  mods/multichannel-critter-sensor/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-9bc5907f3229 · reference
  Variables
  mods/bitshifter/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-9d32bd324e3e · reference
  Variables
  mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-a138faf04479 · reference
  Loops
  mods/detect-any-element/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
blk-a215876c0bad · reference
  Variables
  mods/no-magic-fridges/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 27 further candidate(s) matched and are not listed above (8153 of 8192 bytes; the whole selection is 14665). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for .github ONI-Mods.sln README.md mods/just-fall-already --budget 14665

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
