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
