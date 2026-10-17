# Unit 07 report: fix change-sets for the review findings

19 change-set files in `.context/tasks/OM-001-populate-context/changesets/`: `fix-root.json`, `fix-<mod>.json` for the 17 mods, and one extra, `fix-jumper-bridges-coverage.json`. All 19 are accepted by `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at .` run from `C:/dev/ONI-Mods` (exit 0, "plan only", no `refused`, no protected op, so no confirm digest). Nothing was applied or committed. Full plan output for all 19: `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/plan.out`. The generator is `.../scratchpad/gen.mjs` in the same folder (run with `WITNESS=loose`).

## Finding to change-set and op

| Finding | Change-set | Op |
|---|---|---|
| F-1 | fix-root | `section` `workflows/commit-conventions.md` "Format": states "A commit subject is one terse line. New commits carry no explanatory body, and there is no conventional-commit type." |
| F-1 (branching copy) | fix-root | `section` `workflows/branching.md` "Commit Messages": "One terse subject line; new commits carry no explanatory body." |
| F-2 | fix-root | `workflows/branching.md` "Branch Structure": linear "except for the merge commits that brought each mod in with its history". Also "Task Work": "merge commits appear only where a mod's history was brought in" (that section repeated "no merge commits") |
| F-3 | fix-root | `workflows/branching.md` "Branch Structure" (diagram `<NNN>`) and "Branch Types" (`OM-NNN-short-description`) |
| F-5 | fix-root | `workflows/commit-conventions.md` "Task ID Generation": TODO(owner) replaced with the three-digit statement |
| F-4 | fix-`<mod>` x17 | `section` `standards/naming-conventions.md` "Ids and Tags": `mod_info.yaml` becomes `mod.yaml` |
| F-6 | fix-`<mod>` x17 | `section` `standards/error-handling.md` "try/catch": blank line before item 4 removed. The body carries the nested "Finalizer for a thread with no handler" subsection unchanged, since a section write replaces nested blocks too; plan reports 1 byte removed, 0 added |
| F-7 | fix-bitshifter, -corner-build-anything, -detect-any-element, -dont-count-eggs-on-rails, -just-fall-already, fix-jumper-bridges-coverage | `cover` `domains/entities.md` add `src/<Project>/**` |
| F-8 | fix-jumper-bridges | `section` `domains/entities.md` "Key Code Paths": "the investigation notes (section 6 covers the design) are kept outside this repository" |
| F-9 | fix-root | `testing/unit-testing.md` "Fixtures"; `testing/integration-testing.md` "Launcher Options" (`-Fixture` row) and "Machine requirements" |
| F-10 | fix-root | `workflows/ci-cd.md` "1. Build": adds `translations/` |
| Nit: glossary `applies_to` | fix-`<mod>` x17 | `cover` `domains/glossary.md` add `src/<Project>/**` (same selector as that mod's entities.md) |
| Nit: prefixes | fix-root | `testing/unit-testing.md` "Naming Conventions": adds `PLib:` and `control:` |
| Nit: session reminder | fix-root | `workflows/branching.md` "Pull Request Guidelines": "carries the assistant's attribution lines as described in `commit-conventions.md`" |
| Nit: 150 kg | fix-sweep-zones | `section` `domains/entities.md` "Business Rules": "mass not over the game's `MopTool.maxMopAmt`" |

## Things to know before applying

- **F-9 is not what the review assumed.** `testbed-so.sav` is tracked in the `common` submodule (`git -C common ls-files` lists it; commit 01aefe7 "TestRunner: track the fixture save"; `common/.gitignore` says fixture saves are tracked). The fixtures README is stale ("not committed"). The documents now say: `testbed-so.sav` is tracked in `common`; a base-game run needs a `testbed-base.sav` copied in. The README in `common` still says the opposite; it is outside this repo's `.context/` and was not touched.
- **`cover` needs a witness.** Without one the plan refuses ("does not say how closely it tracks"). The 11 existing entities documents carry `applies_to` and no witness fields in their frontmatter, so no precedent was visible; the ops use `witness: "loose"`. Change to `strict` or `none` if the owner wants another class (edit the `witness` field in the 17 glossary ops and 6 entities ops, or re-run gen.mjs with `WITNESS=...`).
- **Jumper Bridges needs two change-sets.** Its `entities.md` takes a section write (F-8) and a `cover` write (F-7); the writer refuses two ops on one destination when one is a coverage claim. Apply order: `fix-jumper-bridges.json` first, then `fix-jumper-bridges-coverage.json`, so the coverage digest is computed over the corrected bytes.
- A section write puts a blank line between the `**Id:**` line and the body. Where the original had none (ci-cd "1. Build"), the plan counts `+2` lines for a one-line edit. Cosmetic.
- The `control:` prefix is described as "a control case that runs beside the behaviour it checks", citing `mods/jumper-bridges/src/JumperBridges.Tests/WireDrags.cs` (the only user). The 7 `PLib:` tests are in upgrade-planner.
- Left alone, not in scope: N-4, N-5, N-6, N-7, N-9, N-10, N-11, N-12, N-1b.

## `write plan` summary per file (all accepted)

- fix-root: 12 section ops over 5 documents. Byte deltas: Format -89/+117, Task ID -114/+104, Branch Structure -438/+416, Branch Types -53/+52, Task Work -209/+239, Commit Messages -106/+144, PR Guidelines -270/+289, unit-testing Naming -309/+456, Fixtures -408/+519, integration Launcher Options -97/+133, Machine requirements -196/+242, ci-cd Build -495/+513.
- fix-bitshifter, -corner-build-anything, -detect-any-element, -dont-count-eggs-on-rails, -just-fall-already: 4 ops each; naming -67/+62 bytes, try/catch -1/+0, entities and glossary "applies_to (+1) recorded".
- fix-jumper-bridges: 4 ops (naming, try/catch, Key Code Paths -142/+108, glossary coverage). fix-jumper-bridges-coverage: 1 op (entities coverage).
- fix-sweep-zones: 4 ops (naming, try/catch, Business Rules -144/+146, glossary coverage).
- fix-motion-sensor-range, -multichannel-critter-sensor, -natural-backwalls-everywhere, -no-magic-fridges, -refined-building, -salty-surf, -smart-weight-plate, -storage-pod-redux, -upgrade-planner, -vent-freeze-fix: 3 ops each (naming, try/catch, glossary coverage).

No op was protected; no confirm digest was printed.

## Friction (for the receiver to record)

- `ks write plan` on a `cover` op without `witness`: refused with "does not say how closely it tracks the work", naming `cover` as the mode that fixes it, though the op already was `cover`. The refusal never says `witness` is the missing field (only the schema does), and the existing documents' `applies_to` without a witness shows no precedent for a value. Expected the selector alone to be enough, as the review's "same shape the other 11 use" implied. Did instead: read `ks write schema --row narrative --mode cover`, set `witness: "loose"`.
- Two ops on one destination (a `section` and a `cover`) are refused as an unknown-region write. Split into two change-sets.
- The handoff packet's brief (8 KB of 128 KB) listed 30 near-identical `try/catch`/`Braces`/`Indentation` sections of the mods' own standards, which were the documents being edited, not guidance. The tooling the unit needed (`ks write schema`, `common-rules.md`) was reached by the spec's own pointers, not the brief.
- Auto mode says prefer Bash over Read/Write; the owner's memory says write scripts with Write. Used Write for `gen.mjs`.
