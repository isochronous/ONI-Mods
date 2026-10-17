# Handoff — OM-002-store-backfills

**Scope:** `.context`, `mods/.context`, `mods/bitshifter/.context`, `mods/corner-build-anything/.context`, `mods/detect-any-element/.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/05-implement-descriptions.md`

```
# Unit 05 — implement: author descriptions for 38 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents, each read in full from disk (paths are relative to `C:/dev/ONI-Mods`):
- `.::domains/tooling-friction.md`  (file: `.context/domains/tooling-friction.md`)
- `.::overview.md`  (file: `.context/overview.md`)
- `.::testing/integration-testing.md`  (file: `.context/testing/integration-testing.md`)
- `.::testing/unit-testing.md`  (file: `.context/testing/unit-testing.md`)
- `.::workflows/branching.md`  (file: `.context/workflows/branching.md`)
- `.::workflows/ci-cd.md`  (file: `.context/workflows/ci-cd.md`)
- `.::workflows/commit-conventions.md`  (file: `.context/workflows/commit-conventions.md`)
- `.::workflows/task-plan/base.md`  (file: `.context/workflows/task-plan/base.md`)
- `.::workflows/task-plan/phase-architecture.md`  (file: `.context/workflows/task-plan/phase-architecture.md`)
- `.::workflows/task-plan/phase-completion.md`  (file: `.context/workflows/task-plan/phase-completion.md`)
- `.::workflows/task-plan/phase-implementation.md`  (file: `.context/workflows/task-plan/phase-implementation.md`)
- `.::workflows/task-plan/phase-investigation.md`  (file: `.context/workflows/task-plan/phase-investigation.md`)
- `.::workflows/task-plan/phase-testing.md`  (file: `.context/workflows/task-plan/phase-testing.md`)
- `mods::overview.md`  (file: `mods/.context/overview.md`)
- `mods/bitshifter::domains/README.md`  (file: `mods/bitshifter/.context/domains/README.md`)
- `mods/bitshifter::domains/entities.md`  (file: `mods/bitshifter/.context/domains/entities.md`)
- `mods/bitshifter::domains/glossary.md`  (file: `mods/bitshifter/.context/domains/glossary.md`)
- `mods/bitshifter::overview.md`  (file: `mods/bitshifter/.context/overview.md`)
- `mods/bitshifter::standards/README.md`  (file: `mods/bitshifter/.context/standards/README.md`)
- `mods/bitshifter::standards/code-style.md`  (file: `mods/bitshifter/.context/standards/code-style.md`)
- `mods/bitshifter::standards/error-handling.md`  (file: `mods/bitshifter/.context/standards/error-handling.md`)
- `mods/bitshifter::standards/naming-conventions.md`  (file: `mods/bitshifter/.context/standards/naming-conventions.md`)
- `mods/corner-build-anything::domains/README.md`  (file: `mods/corner-build-anything/.context/domains/README.md`)
- `mods/corner-build-anything::domains/entities.md`  (file: `mods/corner-build-anything/.context/domains/entities.md`)
- `mods/corner-build-anything::domains/glossary.md`  (file: `mods/corner-build-anything/.context/domains/glossary.md`)
- `mods/corner-build-anything::overview.md`  (file: `mods/corner-build-anything/.context/overview.md`)
- `mods/corner-build-anything::standards/README.md`  (file: `mods/corner-build-anything/.context/standards/README.md`)
- `mods/corner-build-anything::standards/code-style.md`  (file: `mods/corner-build-anything/.context/standards/code-style.md`)
- `mods/corner-build-anything::standards/error-handling.md`  (file: `mods/corner-build-anything/.context/standards/error-handling.md`)
- `mods/corner-build-anything::standards/naming-conventions.md`  (file: `mods/corner-build-anything/.context/standards/naming-conventions.md`)
- `mods/detect-any-element::domains/README.md`  (file: `mods/detect-any-element/.context/domains/README.md`)
- `mods/detect-any-element::domains/entities.md`  (file: `mods/detect-any-element/.context/domains/entities.md`)
- `mods/detect-any-element::domains/glossary.md`  (file: `mods/detect-any-element/.context/domains/glossary.md`)
- `mods/detect-any-element::overview.md`  (file: `mods/detect-any-element/.context/overview.md`)
- `mods/detect-any-element::standards/README.md`  (file: `mods/detect-any-element/.context/standards/README.md`)
- `mods/detect-any-element::standards/code-style.md`  (file: `mods/detect-any-element/.context/standards/code-style.md`)
- `mods/detect-any-element::standards/error-handling.md`  (file: `mods/detect-any-element/.context/standards/error-handling.md`)
- `mods/detect-any-element::standards/naming-conventions.md`  (file: `mods/detect-any-element/.context/standards/naming-conventions.md`)
- OUT: any other document; writing the store; committing.

## Rules (from the ICON description-backfill skill; follow them exactly)
- A description says **what the file contains**, not what it says in compressed form and not why it matters. Read the whole document; name what a reader finds in it (e.g. "The Harmony patch targets, components, options and saved state of Jumper Bridges, with the gameplay rules each enforces and the file each lives in").
- Never lift or paraphrase the document's first paragraph; never describe from the headings alone.
- One line, no line break, no trailing period required; aim for 15–35 words.
- Name the mod in every mod document's description: the standards documents share a body across 17 mods, so a description that does not name its mod is indistinguishable.
- Code names in a description (paths, file names, identifiers) must be true: a later coverage pass reads them to decide which directory a document is about. Name a mod's source folder (e.g. `src/Bitshifter`) only where the document is about that code.
- Plain wording, no hedges.
- If a document genuinely cannot be described, give the value `null` and say why in the report; that stops the whole run, so use it only when true.

## Output
- `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/work/desc-answers-05.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 38 keys.

## Report
- File: .context/tasks/OM-002-store-backfills/units/05-implement-descriptions-report.md
- Must contain: the key count check output, any null with its reason, anything in these rules you found to be wrong, friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

**Coverage.** nothing in this store declares coverage of these targets, no record names one of them in its own prose, and no target's own bytes cite one, so nothing matched them BY TARGET. What matched, matched by NODE — the records and retrospective entries that sit in the nodes these targets are inside — so the same list comes back for any path inside those nodes. It is still what there is; it is not a selection for the paths you asked about.

**How that is fixed:** a record claims the paths it governs by declaring `applies_to:` in its frontmatter, and a `##`-or-deeper heading block may declare its own `**Applies to:**` — a block that does gets its own pointer, at the block rather than at the whole document.

blk-c3ff9a6f1225 · lesson
  seed ICON's content documents across the root and all 17 mods
  .context/retrospectives/shared.md · unpruned lesson in the governing node
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 15 record(s) in these 5 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: domains/tooling-friction.md, mods/bitshifter:domains/README.md, mods/bitshifter:overview.md, mods/bitshifter:standards/README.md, mods/corner-build-anything:domains/README.md and 10 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
