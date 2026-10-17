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
