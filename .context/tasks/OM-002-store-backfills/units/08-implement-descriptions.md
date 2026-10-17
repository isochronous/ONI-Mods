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
