# Handoff — OM-002-store-backfills

**Scope:** `mods/storage-pod-redux/.context`, `mods/sweep-zones/.context`, `mods/upgrade-planner/.context`, `mods/vent-freeze-fix/.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/08-implement-descriptions.md`

```
# Unit 08 — implement: author descriptions for 32 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents, each read in full from disk (paths are relative to `C:/dev/ONI-Mods`):
- `mods/storage-pod-redux::domains/README.md`  (file: `mods/storage-pod-redux/.context/domains/README.md`)
- `mods/storage-pod-redux::domains/entities.md`  (file: `mods/storage-pod-redux/.context/domains/entities.md`)
- `mods/storage-pod-redux::domains/glossary.md`  (file: `mods/storage-pod-redux/.context/domains/glossary.md`)
- `mods/storage-pod-redux::overview.md`  (file: `mods/storage-pod-redux/.context/overview.md`)
- `mods/storage-pod-redux::standards/README.md`  (file: `mods/storage-pod-redux/.context/standards/README.md`)
- `mods/storage-pod-redux::standards/code-style.md`  (file: `mods/storage-pod-redux/.context/standards/code-style.md`)
- `mods/storage-pod-redux::standards/error-handling.md`  (file: `mods/storage-pod-redux/.context/standards/error-handling.md`)
- `mods/storage-pod-redux::standards/naming-conventions.md`  (file: `mods/storage-pod-redux/.context/standards/naming-conventions.md`)
- `mods/sweep-zones::domains/README.md`  (file: `mods/sweep-zones/.context/domains/README.md`)
- `mods/sweep-zones::domains/entities.md`  (file: `mods/sweep-zones/.context/domains/entities.md`)
- `mods/sweep-zones::domains/glossary.md`  (file: `mods/sweep-zones/.context/domains/glossary.md`)
- `mods/sweep-zones::overview.md`  (file: `mods/sweep-zones/.context/overview.md`)
- `mods/sweep-zones::standards/README.md`  (file: `mods/sweep-zones/.context/standards/README.md`)
- `mods/sweep-zones::standards/code-style.md`  (file: `mods/sweep-zones/.context/standards/code-style.md`)
- `mods/sweep-zones::standards/error-handling.md`  (file: `mods/sweep-zones/.context/standards/error-handling.md`)
- `mods/sweep-zones::standards/naming-conventions.md`  (file: `mods/sweep-zones/.context/standards/naming-conventions.md`)
- `mods/upgrade-planner::domains/README.md`  (file: `mods/upgrade-planner/.context/domains/README.md`)
- `mods/upgrade-planner::domains/entities.md`  (file: `mods/upgrade-planner/.context/domains/entities.md`)
- `mods/upgrade-planner::domains/glossary.md`  (file: `mods/upgrade-planner/.context/domains/glossary.md`)
- `mods/upgrade-planner::overview.md`  (file: `mods/upgrade-planner/.context/overview.md`)
- `mods/upgrade-planner::standards/README.md`  (file: `mods/upgrade-planner/.context/standards/README.md`)
- `mods/upgrade-planner::standards/code-style.md`  (file: `mods/upgrade-planner/.context/standards/code-style.md`)
- `mods/upgrade-planner::standards/error-handling.md`  (file: `mods/upgrade-planner/.context/standards/error-handling.md`)
- `mods/upgrade-planner::standards/naming-conventions.md`  (file: `mods/upgrade-planner/.context/standards/naming-conventions.md`)
- `mods/vent-freeze-fix::domains/README.md`  (file: `mods/vent-freeze-fix/.context/domains/README.md`)
- `mods/vent-freeze-fix::domains/entities.md`  (file: `mods/vent-freeze-fix/.context/domains/entities.md`)
- `mods/vent-freeze-fix::domains/glossary.md`  (file: `mods/vent-freeze-fix/.context/domains/glossary.md`)
- `mods/vent-freeze-fix::overview.md`  (file: `mods/vent-freeze-fix/.context/overview.md`)
- `mods/vent-freeze-fix::standards/README.md`  (file: `mods/vent-freeze-fix/.context/standards/README.md`)
- `mods/vent-freeze-fix::standards/code-style.md`  (file: `mods/vent-freeze-fix/.context/standards/code-style.md`)
- `mods/vent-freeze-fix::standards/error-handling.md`  (file: `mods/vent-freeze-fix/.context/standards/error-handling.md`)
- `mods/vent-freeze-fix::standards/naming-conventions.md`  (file: `mods/vent-freeze-fix/.context/standards/naming-conventions.md`)
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
- `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/work/desc-answers-08.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 32 keys.

## Report
- File: .context/tasks/OM-002-store-backfills/units/08-implement-descriptions-report.md
- Must contain: the key count check output, any null with its reason, anything in these rules you found to be wrong, friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 12 record(s) in these 4 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/storage-pod-redux:domains/README.md, mods/storage-pod-redux:overview.md, mods/storage-pod-redux:standards/README.md, mods/sweep-zones:domains/README.md, mods/sweep-zones:overview.md and 7 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
