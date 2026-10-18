# Unit 12 — review: pre-completion review of Ovagro Overhead

## Objective
Review the whole change on branch OM-006-ovagro-overhead against main (`git diff main...HEAD -- . ':!.context'`; the store node files under mods/ovagro-overhead/.context are scaffold placeholders, out of scope) for correctness, consistency with the repo's standards, and test quality, before the task closes.

## Scope
- IN: mods/ovagro-overhead/src/OvagroOverhead/** (mod code, mod.yaml, mod_info.yaml, csproj), mods/ovagro-overhead/src/OvagroOverhead.Tests/** (tests and kit), mods/ovagro-overhead/README.md, CHANGELOG.md, publish/*, ONI-Mods.sln, README.md, .github/ changes.
- OUT: changing any file (findings only), common/, other mods, .context/.
- Passes to run, each named in the report with what it checked: correctness of every Harmony patch against the decompiled game source (targets, signatures, the prefix+finalizer swap restoring on every path including nested and exception paths, the #9 postfix's timing assumption); the hidden prefab against vanilla VineMotherConfig/SaltPlantConfig (anything vanilla sets that the ceiling config misses); save/load and mod-removal behaviour; standards adherence (code style, error handling — including the user-approved fifth try/catch case in Mod.cs, Harmony conventions, logging); tests (assertions that check behaviour, not just that something ran; pacing to game time; cleanup; the duplicated grow helpers between Kit.cs and Vines.cs); player-facing text (no hedging, accurate claims); registration files (sln, label map, issue templates, README row).

## Context the packet does not carry
- Approved decisions are in plan.md `## Decisions` (D1-D6, the patch-failure ruling, net48). Do not re-litigate them; flag only where the code departs from them.
- Reports: units/03-design-report.md (design), units/06, 07 (implementation departures), 08, 09, 10 (test runs: 16 pass in run-20261018-003145), 11 (docs).
- Decompiled game source: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\
- Do not launch the game or run the test suite; a build is fine.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/12-review-report.md
- Must contain: findings with severity (block / fix / nit), file:line, and the fix each expects; the passes run and what each checked; "no findings" for a pass only with what it examined; and a "Store reads" section.
- Model tier: complex
