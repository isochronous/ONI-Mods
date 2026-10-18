# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/just-fall-already/src`, `ONI-Mods.sln`, `README.md`, `.github/workflows/label-mod.yml`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/04-plan.md`

```
# Unit 04 — plan: sequenced implement and test steps for Ovagro Overhead

Task: OM-006 — Ovagro Overhead (Ovagro grows from ceilings)
Objective: turn the approved design and the scaffold checklist into implement steps a sub-agent can each finish and commit, plus the test steps, so the mod builds, loads and passes in-game tests.
Affected mods and shared projects: mods/ovagro-overhead (new), ONI-Mods.sln, README.md (root table), .github/workflows/label-mod.yml; common/ is read-only for this task.
Complexity: Medium

## Context the packet does not carry
- plan.md `## Decisions` and the two handoff blocks (investigation, architecture) are the governing decisions: hidden `VineMotherCeiling` prefab, vanilla seed routed at planting time, patches #1-#9 from the design report's table, art transform, D1-D5 as the user chose them, requiredDlcIds DLC4_ID, no ModStrings.
- Reports: units/02-explore-report.md (scaffold checklist, sln blocks, test template mods/just-fall-already Doors.cs, release prerequisites), units/03-design-report.md (design, patch table, risks, tests T1-T10). Unit 01's report is superseded where unit 03 corrects it.
- Decompiled game source for reference: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\
- In-game tests run through `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead`; tests are paced at game speed (build, wait game time, act), never frames apart. Test runs launch the game, which the user has permitted for testing.
- The publish/ preview image and Workshop text are release material: plan them as a step, but the preview art is produced with the repo's preview tooling and shown to the user before release; no release runs in this task.
- After the scaffold exists, the manager integrates the mod's .context node (icon-integrate) and lands the design as a decision record; plan that as a manager step, not a unit step.

## Open questions the breakdown must address
- Step granularity: which steps can run in parallel (scaffold vs. prefab vs. patches vs. tests) and which must be sequential; each step ends in a successful `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` and a commit.
- How the wild-planting tests make a natural ceiling in code (design report "Testability"), and which test steps need the testbed save that exists (testbed-so.sav) given testbed-base.sav is missing.
- Which of T1-T10 are automated and which stay manual (mod-removal load).
- The tier for each step (basic / default / complex) by the routing guide's signals.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/04-plan-report.md
- Must contain: the sequenced step list with a dependency map; for each step the files it creates or modifies (paths from the repo root), the pattern or analogue to copy, acceptance criteria (command output for build and tests, "commit all created/modified files"), and a tier; open questions and assumptions; and a "Store reads" section listing every ks brief and ks read run and which pointers informed the plan.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-9d52ccfdb3df · reference
  Key Code Paths
  mods/just-fall-already/.context/domains/entities.md#key-code-paths · names mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs, mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-0b8e3ef08773 · reference
  Example
  mods/vent-freeze-fix/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-0fe521f2b16f · reference
  Example
  mods/upgrade-planner/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-16cd4984fdb3 · reference
  Loops
  mods/motion-sensor-range/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-1b40623f402c · reference
  Loops
  mods/vent-freeze-fix/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-29fb02c618a8 · reference
  Loops
  mods/smart-weight-plate/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-2aba30d1f782 · reference
  Variables
  mods/corner-build-anything/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-308a838456d3 · reference
  Loops
  mods/storage-pod-redux/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-32ae0ce82848 · reference
  Example
  mods/bitshifter/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-353f3e62bd46 · reference
  Example
  mods/multichannel-critter-sensor/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-396cf026d230 · reference
  Variables
  mods/motion-sensor-range/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-3a88acdbb150 · reference
  Variables
  mods/upgrade-planner/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-431be121fc8c · reference
  Example
  mods/corner-build-anything/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-47fe26d5347a · reference
  Variables
  mods/salty-surf/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-483b9df9284d · reference
  Loops
  mods/multichannel-critter-sensor/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-486b038b51f1 · reference
  Variables
  mods/storage-pod-redux/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-5379b7234d12 · reference
  Example
  mods/motion-sensor-range/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-5b62e7507604 · reference
  Example
  mods/just-fall-already/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6958eb513dec · reference
  Loops
  mods/sweep-zones/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-699b426eb8f6 · reference
  Variables
  mods/sweep-zones/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6a8bea597750 · reference
  Example
  mods/detect-any-element/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6b9561f02621 · reference
  Loops
  mods/refined-building/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7261503b82df · reference
  Example
  mods/storage-pod-redux/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-74082ad9f366 · reference
  Variables
  mods/vent-freeze-fix/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-76e79f89b3c4 · reference
  Variables
  mods/just-fall-already/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7bf2c80b2f8e · reference
  Loops
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7d0c65d332f3 · reference
  Static class with the whole mod. A postfix on `Pathfinding.AddNavGrid` subscribes to each navig…
  mods/just-fall-already/.context/domains/entities.md#key-entities/fallpoke · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7e5838a4ab04 · reference
  Loops
  mods/no-magic-fridges/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-8ce83687722f · reference
  Example
  mods/sweep-zones/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-8f46b5b1e7a3 · reference
  Loops
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-94c9c06009e9 · reference
  Example
  mods/refined-building/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-96641cb455da · reference
  Variables
  mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9998cf9f18c6 · reference
  Variables
  mods/multichannel-critter-sensor/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9bc5907f3229 · reference
  Variables
  mods/bitshifter/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9d32bd324e3e · reference
  Variables
  mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-a138faf04479 · reference
  Loops
  mods/detect-any-element/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-a215876c0bad · reference
  Variables
  mods/no-magic-fridges/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-a6588337dcaa · reference
  Variables
  mods/jumper-bridges/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-ab3898ffc38d · reference
  Loops
  mods/salty-surf/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-b243414742bb · reference
  Example
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-b8840ab84c08 · reference
  Variables
  mods/detect-any-element/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-c1b2341c69e8 · reference
  Variables
  mods/refined-building/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-c6278ac1655d · reference
  Loops
  mods/corner-build-anything/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-ca4a91a387a7 · reference
  Loops
  mods/upgrade-planner/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-cb4940771b30 · reference
  Example
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-cd84394283fa · reference
  Example
  mods/no-magic-fridges/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-d21c0d25275e · reference
  Variables
  mods/smart-weight-plate/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-d5ef39d406e3 · reference
  Loops
  mods/jumper-bridges/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-dffe3109ac36 · reference
  Loops
  mods/just-fall-already/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-e155218a57bf · reference
  Example
  mods/salty-surf/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-ea3b63724204 · reference
  Example
  mods/smart-weight-plate/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-eb6b4b01a219 · reference
  Loops
  mods/bitshifter/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-f3f1f9eab24b · reference
  Example
  mods/jumper-bridges/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-f5bd93c3845d · reference
  Postfix on `Door.SetWorldState`. Acts only when `updateSim` is true (the door's state machine e…
  mods/just-fall-already/.context/domains/entities.md#key-entities/door-setworldstate-patch · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src · tagged mod-compatibility → mod-build
blk-09140514ea2f · reference
  The `UserMod2` entry point. Calls `base.OnLoad` (which applies the patches) and logs the versio…
  mods/just-fall-already/.context/domains/entities.md#key-entities/justfallalreadymod · names mods/just-fall-already/src/JustFallAlready/Mod.cs → mods/just-fall-already/src · tagged mod-entry-point → mod-build
mods/just-fall-already:domains/entities.md · reference
  The `FallPoke` class, the `Door.SetWorldState` patch and the entry point of Just Fall Already!,…
  mods/just-fall-already/.context/domains/entities.md · src/JustFallAlready/** → mods/just-fall-already/src · linked from domains/glossary.md
mods/just-fall-already:domains/glossary.md · reference
  Glossary for Just Fall Already!: abbreviations, a term to avoid, and definitions of pez dropper…
  mods/just-fall-already/.context/domains/glossary.md · src/JustFallAlready/** → mods/just-fall-already/src · linked from domains/entities.md
mods/just-fall-already:standards/code-style.md · reference
  Code style guide for Just Fall Already!: C# formatting, imports, comments, control structures,…
  mods/just-fall-already/.context/standards/code-style.md · src/** → mods/just-fall-already/src · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/just-fall-already:standards/error-handling.md · reference
  Error handling conventions for Just Fall Already!: guarding patches, try/catch cases, logging l…
  mods/just-fall-already/.context/standards/error-handling.md · src/** → mods/just-fall-already/src · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/just-fall-already:standards/naming-conventions.md · reference
  Naming conventions for Just Fall Already!: files, namespaces, variables, methods, patch classes…
  mods/just-fall-already/.context/standards/naming-conventions.md · src/** → mods/just-fall-already/src · linked from standards/code-style.md · linked from standards/error-handling.md
blk-3e0b229450b9 · reference
  Fast Track
  mods/just-fall-already/.context/domains/glossary.md#cross-domain-terms/fast-track · tagged mod-compatibility → mod-build
blk-4ecf7695829b · reference
  Business Rules
  mods/just-fall-already/.context/domains/entities.md#business-rules · tagged mod-compatibility → mod-build
blk-b684bb2fb3e0 · reference
  Yielding to Other Mods
  mods/just-fall-already/.context/standards/error-handling.md#yielding-to-other-mods · tagged mod-compatibility → mod-build
blk-e47a9aee85a7 · reference
  Critters Fall Through Open Doors
  mods/just-fall-already/.context/domains/glossary.md#cross-domain-terms/critters-fall-through-open-doors · tagged mod-compatibility → mod-build
blk-e59d34ad505d · reference
  What this is
  mods/just-fall-already/.context/overview.md#what-this-is · tagged mod-compatibility → mod-build
blk-fc0b4aab1de9 · reference
  Just Fall Already! specifics
  mods/just-fall-already/.context/standards/error-handling.md#project-specific-patterns/just-fall-already-specifics · tagged mod-compatibility → mod-build
blk-386e66e27501 · reference
  6. Issue labelling (the only GitHub Action)
  .context/workflows/ci-cd.md#stage-details/6-issue-labelling-the-only-github-action · names .github/workflows/label-mod.yml → .github/workflows/label-mod.yml · tagged mod-build → mod-build
blk-c8ecd5229a93 · reference
  Additional Architecture Review Triggers
  .context/workflows/task-plan/phase-architecture.md#additional-architecture-review-triggers · names .github/workflows/label-mod.yml → .github/workflows/label-mod.yml · tagged mod-build, mod-compatibility → mod-build · tagged release-process → release-process
blk-0194e02d0fd6 · reference
  Assertion Patterns
  .context/testing/unit-testing.md#assertion-patterns · tagged in-game-testing → in-game-testing · tagged test-assertions → test-pacing
blk-0272c27e23d2 · reference
  Settings and environment
  .context/testing/unit-testing.md#mocking-patterns/settings-and-environment · tagged in-game-testing → in-game-testing · tagged test-cleanup → test-pacing
tags: read as `(in-game-testing && test-pacing) || (release-process && steam-workshop) || mod-build` (`&&` binds tighter than `||`), which selected 60 record(s). 33 more carry one of its terms but do not satisfy it and were NOT selected by the expression.

**Truncated.** 50 further candidate(s) matched and are not listed above (16282 of 16384 bytes; the whole selection is 26338). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for .github/workflows/label-mod.yml ONI-Mods.sln README.md mods/just-fall-already/src --tag "(in-game-testing && test-pacing) || (release-process && steam-workshop) || mod-build" --budget 26338

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
