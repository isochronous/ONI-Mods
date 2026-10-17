# Handoff — OM-002-store-backfills

**Scope:** `mods/natural-backwalls-everywhere/.context`, `mods/no-magic-fridges/.context`, `mods/refined-building/.context`, `mods/salty-surf/.context`, `mods/smart-weight-plate/.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/07-implement-descriptions.md`

```
# Unit 07 — implement: author descriptions for 40 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents, each read in full from disk (paths are relative to `C:/dev/ONI-Mods`):
- `mods/natural-backwalls-everywhere::domains/README.md`  (file: `mods/natural-backwalls-everywhere/.context/domains/README.md`)
- `mods/natural-backwalls-everywhere::domains/entities.md`  (file: `mods/natural-backwalls-everywhere/.context/domains/entities.md`)
- `mods/natural-backwalls-everywhere::domains/glossary.md`  (file: `mods/natural-backwalls-everywhere/.context/domains/glossary.md`)
- `mods/natural-backwalls-everywhere::overview.md`  (file: `mods/natural-backwalls-everywhere/.context/overview.md`)
- `mods/natural-backwalls-everywhere::standards/README.md`  (file: `mods/natural-backwalls-everywhere/.context/standards/README.md`)
- `mods/natural-backwalls-everywhere::standards/code-style.md`  (file: `mods/natural-backwalls-everywhere/.context/standards/code-style.md`)
- `mods/natural-backwalls-everywhere::standards/error-handling.md`  (file: `mods/natural-backwalls-everywhere/.context/standards/error-handling.md`)
- `mods/natural-backwalls-everywhere::standards/naming-conventions.md`  (file: `mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md`)
- `mods/no-magic-fridges::domains/README.md`  (file: `mods/no-magic-fridges/.context/domains/README.md`)
- `mods/no-magic-fridges::domains/entities.md`  (file: `mods/no-magic-fridges/.context/domains/entities.md`)
- `mods/no-magic-fridges::domains/glossary.md`  (file: `mods/no-magic-fridges/.context/domains/glossary.md`)
- `mods/no-magic-fridges::overview.md`  (file: `mods/no-magic-fridges/.context/overview.md`)
- `mods/no-magic-fridges::standards/README.md`  (file: `mods/no-magic-fridges/.context/standards/README.md`)
- `mods/no-magic-fridges::standards/code-style.md`  (file: `mods/no-magic-fridges/.context/standards/code-style.md`)
- `mods/no-magic-fridges::standards/error-handling.md`  (file: `mods/no-magic-fridges/.context/standards/error-handling.md`)
- `mods/no-magic-fridges::standards/naming-conventions.md`  (file: `mods/no-magic-fridges/.context/standards/naming-conventions.md`)
- `mods/refined-building::domains/README.md`  (file: `mods/refined-building/.context/domains/README.md`)
- `mods/refined-building::domains/entities.md`  (file: `mods/refined-building/.context/domains/entities.md`)
- `mods/refined-building::domains/glossary.md`  (file: `mods/refined-building/.context/domains/glossary.md`)
- `mods/refined-building::overview.md`  (file: `mods/refined-building/.context/overview.md`)
- `mods/refined-building::standards/README.md`  (file: `mods/refined-building/.context/standards/README.md`)
- `mods/refined-building::standards/code-style.md`  (file: `mods/refined-building/.context/standards/code-style.md`)
- `mods/refined-building::standards/error-handling.md`  (file: `mods/refined-building/.context/standards/error-handling.md`)
- `mods/refined-building::standards/naming-conventions.md`  (file: `mods/refined-building/.context/standards/naming-conventions.md`)
- `mods/salty-surf::domains/README.md`  (file: `mods/salty-surf/.context/domains/README.md`)
- `mods/salty-surf::domains/entities.md`  (file: `mods/salty-surf/.context/domains/entities.md`)
- `mods/salty-surf::domains/glossary.md`  (file: `mods/salty-surf/.context/domains/glossary.md`)
- `mods/salty-surf::overview.md`  (file: `mods/salty-surf/.context/overview.md`)
- `mods/salty-surf::standards/README.md`  (file: `mods/salty-surf/.context/standards/README.md`)
- `mods/salty-surf::standards/code-style.md`  (file: `mods/salty-surf/.context/standards/code-style.md`)
- `mods/salty-surf::standards/error-handling.md`  (file: `mods/salty-surf/.context/standards/error-handling.md`)
- `mods/salty-surf::standards/naming-conventions.md`  (file: `mods/salty-surf/.context/standards/naming-conventions.md`)
- `mods/smart-weight-plate::domains/README.md`  (file: `mods/smart-weight-plate/.context/domains/README.md`)
- `mods/smart-weight-plate::domains/entities.md`  (file: `mods/smart-weight-plate/.context/domains/entities.md`)
- `mods/smart-weight-plate::domains/glossary.md`  (file: `mods/smart-weight-plate/.context/domains/glossary.md`)
- `mods/smart-weight-plate::overview.md`  (file: `mods/smart-weight-plate/.context/overview.md`)
- `mods/smart-weight-plate::standards/README.md`  (file: `mods/smart-weight-plate/.context/standards/README.md`)
- `mods/smart-weight-plate::standards/code-style.md`  (file: `mods/smart-weight-plate/.context/standards/code-style.md`)
- `mods/smart-weight-plate::standards/error-handling.md`  (file: `mods/smart-weight-plate/.context/standards/error-handling.md`)
- `mods/smart-weight-plate::standards/naming-conventions.md`  (file: `mods/smart-weight-plate/.context/standards/naming-conventions.md`)
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
- `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/work/desc-answers-07.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 40 keys.

## Report
- File: .context/tasks/OM-002-store-backfills/units/07-implement-descriptions-report.md
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

⚠ 15 record(s) in these 5 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/natural-backwalls-everywhere:domains/README.md, mods/natural-backwalls-everywhere:overview.md, mods/natural-backwalls-everywhere:standards/README.md, mods/no-magic-fridges:domains/README.md, mods/no-magic-fridges:overview.md and 10 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
