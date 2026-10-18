# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/12-review.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 3 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
