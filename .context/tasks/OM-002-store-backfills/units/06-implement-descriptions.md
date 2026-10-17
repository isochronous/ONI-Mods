# Unit 06 — implement: author descriptions for 40 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents, each read in full from disk (paths are relative to `C:/dev/ONI-Mods`):
- `mods/dont-count-eggs-on-rails::domains/README.md`  (file: `mods/dont-count-eggs-on-rails/.context/domains/README.md`)
- `mods/dont-count-eggs-on-rails::domains/entities.md`  (file: `mods/dont-count-eggs-on-rails/.context/domains/entities.md`)
- `mods/dont-count-eggs-on-rails::domains/glossary.md`  (file: `mods/dont-count-eggs-on-rails/.context/domains/glossary.md`)
- `mods/dont-count-eggs-on-rails::overview.md`  (file: `mods/dont-count-eggs-on-rails/.context/overview.md`)
- `mods/dont-count-eggs-on-rails::standards/README.md`  (file: `mods/dont-count-eggs-on-rails/.context/standards/README.md`)
- `mods/dont-count-eggs-on-rails::standards/code-style.md`  (file: `mods/dont-count-eggs-on-rails/.context/standards/code-style.md`)
- `mods/dont-count-eggs-on-rails::standards/error-handling.md`  (file: `mods/dont-count-eggs-on-rails/.context/standards/error-handling.md`)
- `mods/dont-count-eggs-on-rails::standards/naming-conventions.md`  (file: `mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md`)
- `mods/jumper-bridges::domains/README.md`  (file: `mods/jumper-bridges/.context/domains/README.md`)
- `mods/jumper-bridges::domains/entities.md`  (file: `mods/jumper-bridges/.context/domains/entities.md`)
- `mods/jumper-bridges::domains/glossary.md`  (file: `mods/jumper-bridges/.context/domains/glossary.md`)
- `mods/jumper-bridges::overview.md`  (file: `mods/jumper-bridges/.context/overview.md`)
- `mods/jumper-bridges::standards/README.md`  (file: `mods/jumper-bridges/.context/standards/README.md`)
- `mods/jumper-bridges::standards/code-style.md`  (file: `mods/jumper-bridges/.context/standards/code-style.md`)
- `mods/jumper-bridges::standards/error-handling.md`  (file: `mods/jumper-bridges/.context/standards/error-handling.md`)
- `mods/jumper-bridges::standards/naming-conventions.md`  (file: `mods/jumper-bridges/.context/standards/naming-conventions.md`)
- `mods/just-fall-already::domains/README.md`  (file: `mods/just-fall-already/.context/domains/README.md`)
- `mods/just-fall-already::domains/entities.md`  (file: `mods/just-fall-already/.context/domains/entities.md`)
- `mods/just-fall-already::domains/glossary.md`  (file: `mods/just-fall-already/.context/domains/glossary.md`)
- `mods/just-fall-already::overview.md`  (file: `mods/just-fall-already/.context/overview.md`)
- `mods/just-fall-already::standards/README.md`  (file: `mods/just-fall-already/.context/standards/README.md`)
- `mods/just-fall-already::standards/code-style.md`  (file: `mods/just-fall-already/.context/standards/code-style.md`)
- `mods/just-fall-already::standards/error-handling.md`  (file: `mods/just-fall-already/.context/standards/error-handling.md`)
- `mods/just-fall-already::standards/naming-conventions.md`  (file: `mods/just-fall-already/.context/standards/naming-conventions.md`)
- `mods/motion-sensor-range::domains/README.md`  (file: `mods/motion-sensor-range/.context/domains/README.md`)
- `mods/motion-sensor-range::domains/entities.md`  (file: `mods/motion-sensor-range/.context/domains/entities.md`)
- `mods/motion-sensor-range::domains/glossary.md`  (file: `mods/motion-sensor-range/.context/domains/glossary.md`)
- `mods/motion-sensor-range::overview.md`  (file: `mods/motion-sensor-range/.context/overview.md`)
- `mods/motion-sensor-range::standards/README.md`  (file: `mods/motion-sensor-range/.context/standards/README.md`)
- `mods/motion-sensor-range::standards/code-style.md`  (file: `mods/motion-sensor-range/.context/standards/code-style.md`)
- `mods/motion-sensor-range::standards/error-handling.md`  (file: `mods/motion-sensor-range/.context/standards/error-handling.md`)
- `mods/motion-sensor-range::standards/naming-conventions.md`  (file: `mods/motion-sensor-range/.context/standards/naming-conventions.md`)
- `mods/multichannel-critter-sensor::domains/README.md`  (file: `mods/multichannel-critter-sensor/.context/domains/README.md`)
- `mods/multichannel-critter-sensor::domains/entities.md`  (file: `mods/multichannel-critter-sensor/.context/domains/entities.md`)
- `mods/multichannel-critter-sensor::domains/glossary.md`  (file: `mods/multichannel-critter-sensor/.context/domains/glossary.md`)
- `mods/multichannel-critter-sensor::overview.md`  (file: `mods/multichannel-critter-sensor/.context/overview.md`)
- `mods/multichannel-critter-sensor::standards/README.md`  (file: `mods/multichannel-critter-sensor/.context/standards/README.md`)
- `mods/multichannel-critter-sensor::standards/code-style.md`  (file: `mods/multichannel-critter-sensor/.context/standards/code-style.md`)
- `mods/multichannel-critter-sensor::standards/error-handling.md`  (file: `mods/multichannel-critter-sensor/.context/standards/error-handling.md`)
- `mods/multichannel-critter-sensor::standards/naming-conventions.md`  (file: `mods/multichannel-critter-sensor/.context/standards/naming-conventions.md`)
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
- `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/work/desc-answers-06.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 40 keys.

## Report
- File: .context/tasks/OM-002-store-backfills/units/06-implement-descriptions-report.md
- Must contain: the key count check output, any null with its reason, anything in these rules you found to be wrong, friction met.
- Model tier: default
