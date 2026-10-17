# Unit 12 report: review of the OM-002 store changes

Reviewer: ICON review unit, default tier. Read only; nothing in the store or the repository was changed except this report. Scripts live in the session scratchpad under `rev12/`.

Governing skills read for the passes: `tag-discipline` (gloss rules, term worth minting), `tag-backfill` step 2 (admission floor of 3 records across 2 documents, 85% bar, location-not-subject, one-sentence gloss, no boundary clause), `description-backfill` (what the file contains, not why it matters; one line).

Severity: block (must change before merge), fix (change before the PR), nit (optional).

Result: no block findings. 11 fix findings, 14 nits. The fix findings cluster in pass 4 (tag placement), plus one factual error in pass 1.

---

## Pass 1: overview and README accuracy

Coverage: every concrete claim of the root `overview.md`, `mods/.context/overview.md` and all 17 mod overviews (more than the eight required). Checked against the repository: every backticked file or folder exists (script, 17 of 17), every stated version equals `mod_info.yaml` (17 of 17), the 17 mod folders, `common/` contents and tool list, `.github/` files, `.claude/commands/release-mods.md`, `.gitmodules`, `$(OniCommon)` in `Directory.Build.props`, the "separate repositories until October 2026" claim (git log: 17 "Bring in ... with its history" commits on 2026-10-15), release dates against each CHANGELOG, all six Workshop ids against `publish/workshop-id.txt`, translations folders and languages, the `retired.txt` claim for refined-building, README facts (Storage Pod figures, Sweep Zones four modes, Upgrade Planner and Blueprints Expanded, Vent Freeze Fix 80% rule and one-second watcher, motion sensor 2 to 16 / default 4, NBE four shipped noise trees, work-in-progress status, 3GuB and Cairath credits). Every link in every `.md` outside `tasks/`: 311 links, 0 broken. Every `domains/README.md` and `standards/README.md` lists exactly the sibling documents in its folder (no unlisted, no dangling).

- **F1.1 (fix)** Six overviews attribute `supportedContent: ALL` and `minimumSupportedBuild: 744825` to `mod.yaml`. Both keys are in `mod_info.yaml`; `mod.yaml` holds title, description and `staticID` only. Locations: `mods/motion-sensor-range`, `multichannel-critter-sensor`, `natural-backwalls-everywhere`, `no-magic-fridges`, `refined-building`, `salty-surf`, each `.context/overview.md`, section "What this is" (the sentence "`mod.yaml` sets ..."). Fix: write `mod_info.yaml` in that sentence. The other 11 overviews state it correctly or without a file name.
- **F1.2 (nit)** `mods/.context/overview.md` says each folder "follows the same shape" and lists `tools/` as the only extra. Seven mods carry other folders: `preview/` (detect-any-element, dont-count-eggs-on-rails, natural-backwalls-everywhere, salty-surf, upgrade-planner), `assets-src/` (motion-sensor-range, sweep-zones), `lib/` (upgrade-planner). Fix: add one bullet naming them.
- **F1.3 (nit)** `mods/bitshifter/.context/overview.md`, "What this is", last sentence: "Players of ribbon-based automation builds use it." Nothing in the repository states it; it is filler. Fix: delete the sentence.
- **F1.4 (nit)** The `error-handling` bullet in every `standards/README.md` says the document covers "reflection lookups and file reads". Four mods have neither in their code (dont-count-eggs-on-rails, just-fall-already, refined-building, vent-freeze-fix) and only two read files (storage-pod-redux, upgrade-planner). The bullet describes the shared text, not the mod. Fix: drop "reflection lookups and file reads" or limit it to the mods that have them. (Related and outside this diff: the shared standards bodies written by OM-001 cite other mods' files in every mod, for example `dont-count-eggs-on-rails/.../error-handling.md` points at upgrade-planner files, so a mod node's standards read as repository-wide rather than the mod's own.)
- **F1.5 (nit)** Two README styles: 6 mods (bitshifter through just-fall-already) open with "Documents in this folder:" and capitalise the bullet text; the other 11 do not and use lower case. Fix: pick one.

No other inaccuracy found in the overviews.

## Pass 2: descriptions

Coverage: automated checks over all 167 documents carrying front matter, covering root and 17 mods: single line, YAML-safe quoting (0 risks), no CRLF, mod name present in every mod-node description (0 misses), no description over 300 characters, every backticked or CamelCase code name looked up in that mod's source tree. Read in full or in part about 120 descriptions (all 8 root documents, all overview, domains, standards and README descriptions of corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges, just-fall-already, multichannel-critter-sensor, motion-sensor-range, natural-backwalls-everywhere, refined-building, smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix, no-magic-fridges, salty-surf, bitshifter). Spot-verified code claims by reading source: `OvercrowdingMonitor.UpdateRegion` is a Prefix, corner-build-anything has two Transpilers over `SwapTable`, `OnRail`, `ThresholdAdapter`, `RangeSlider`, `FridgeThermostat`, `OffsetGroups` all exist. Descriptions say what the file contains, not why it matters. The 72 files without a description are `META.md`, `projects.md`, `decisions/README.md` and `retrospectives/shared.md` at each of 18 nodes (system files, outside the 150).

- **F2.1 (nit)** The `naming-conventions.md` descriptions of dont-count-eggs-on-rails, just-fall-already, no-magic-fridges and refined-building name "`ModStrings` keys". None of the four has a `ModStrings` class; the phrase describes the shared section, not the mod. Fix: drop the phrase in those four.
- **F2.2 (nit)** The 34 README descriptions follow one frame ("Index of the ... each with a one-line summary of ...") and carry a subject phrase that is the mod's topic, not the document's content ("the door and critter fall subject it covers"). Acceptable for an index; shorter wording would read better. No change required.

No fix-level finding.

## Pass 3: dictionaries

Coverage: all 18 `tags.md` (root plus 17), 249 term entries, 86 distinct terms. Checked by script and by reading every distinct gloss.

Passed: every gloss is one sentence starting "A record earns this term when it is about"; no enumerated sub-topic list in any gloss; 29 terms are shared across nodes and every shared gloss is byte-identical across nodes (0 mismatches, including `mod-compatibility` and `in-game-testing` against the root); no unadmitted tags on any record (0 tags outside their node's dictionary, across 749 tagged sections); no term reaches 85% of its node (maximum 67%, `harmony-patching` in natural-backwalls-everywhere); no unused term.

- **F3.1 (fix)** `developer-tooling` (natural-backwalls-everywhere, vent-freeze-fix) ends "... for its author rather than for players". That is a boundary clause, the shape the gloss rule forbids. Fix: "A record earns this term when it is about the debug and developer features a mod includes for its author." Land it as a `section` write per node.
- **F3.2 (fix)** `mod-entry-point` (all 17 dictionaries) is a code-location term: "a mod's UserMod2 class and what it does when the game loads the mod". It is applied to the "Where things are" file-index section of most of the 17 overviews, which shows the location reading in use. Fix: reword the gloss to the subject (what a mod does as the game loads it) and stop tagging file indexes (see F4.3), or retire the term.
- **F3.3 (nit)** `reach-table` (corner-build-anything) names a data shape ("tables of offsets"). Fix: "the rules that decide where a duplicant may stand to build or deconstruct a building", or leave.
- **F3.4 (nit)** Glosses that join two aspects with "and": `heat-exchange` (three places heat moves between), `mechanical-surfboard` ("takes in, holds and spills"), `building-materials` ("which materials ... accepts and how ... is chosen"). Single subject each; reword only if a split is wanted later.
- **F3.5 (nit)** `mod-strings` (11 nodes) includes "and the translations of that text", which overlaps the root `translations` term ("translating a mod's player-visible strings"). Different nodes, so no clash today.
- **F3.6 (nit)** Eight terms live in one document only, under the admission floor of 2 documents: root `test-assertions` and `simulated-input` (both only in one testing document), detect-any-element `reflection`, jumper-bridges `automation-ribbon`, multichannel-critter-sensor `pulse-output`, `vanilla-fallback` in no-magic-fridges, refined-building and storage-pod-redux. The floor is an admission rule, and these may have met it before the dedupe pass; decide whether to keep or retire.

## Pass 4: tag placement

Coverage: all 749 tagged sections were screened mechanically (term versus the section's own text; 132 flagged); about 330 tagged sections were read by hand: every tagged section in the 13 root documents, and the domains, overview and standards documents of bitshifter, jumper-bridges, detect-any-element, upgrade-planner, storage-pod-redux, multichannel-critter-sensor, sweep-zones, natural-backwalls-everywhere, vent-freeze-fix and no-magic-fridges. Also checked by script: 0 duplicates of a document-level tag on a section of the same document, 0 child sections repeating a parent heading's tag, every documented-with-prose file carries at least one tag.

- **F4.1 (fix)** The same text carries different tags in different mods. Of 56 section bodies that are byte-identical across at least 3 mod nodes (almost all across 17), 34 carry a different tag set per mod. Examples (distinct tag sets over 17 copies): "Examples by Type" 14, "Principles" 12, "Yielding to Other Mods" 11, "Harmony Patches" 10, "try/catch" 10, "Ids and Tags" 8, "Assuming the game's internals" 8, "Transpiler: count the match" 8, "Missing member: warn, leave vanilla" 8. The sets follow whatever each mod's tagging unit noticed in the cross-mod examples quoted inside the shared text, so a query for a term returns an arbitrary subset of identical sections. Fix: choose one tag set per shared section from what the text is about (`harmony-patching`, `vanilla-fallback`, `logging`, `reflection`, `code-style`) and apply it to all copies; remove terms that exist only because an example names a feature (`hotkey`, `game-tag`, `building-config`, `building-unlock`, `worldgen`, `building-registration`, `developer-tooling`).
- **F4.2 (fix)** Wrong terms on shared sections (specifics of F4.1):
  - `naming-conventions.md` "Ids and Tags": `hotkey` (sweep-zones, upgrade-planner), `game-tag` (detect-any-element, dont-count-eggs-on-rails, refined-building, salty-surf), `building-config` (multichannel-critter-sensor, smart-weight-plate, storage-pod-redux), `mod-compatibility` (just-fall-already, natural-backwalls-everywhere), `mod-entry-point` (salty-surf, smart-weight-plate). The section lists id and prefix rules; none of those terms describes it. Expected: `in-game-testing` only if kept at all (one bullet is about test names), otherwise no section tag.
  - `error-handling.md` "What to Include": `performance` (just-fall-already, upgrade-planner) and `developer-tooling` (vent-freeze-fix). The section is about what a log line carries; one bullet mentions per-tick telemetry. Expected: `logging` only.
  - `error-handling.md` "try/catch": `mod-compatibility` in several mods reflects bullet 1 of 4 only, and `worldgen` in natural-backwalls-everywhere reflects bullet 3. Expected: `logging` (and `exception-handling` where the node has it), not the example-driven terms.
- **F4.3 (fix)** Index-style sections carry subject tags taken from the files they list. All 17 overviews tag "Where things are" with 2 to 4 subject terms (for example `in-game-testing`, `mod-entry-point`, `kanim-art`, `mod-strings`), though the section is a folder map. Same in the root overview: "Where things are" `[github-issues, knowledge-store, translations]` and "What this is" `[github-issues]` (one sentence about issues). Also `natural-backwalls-everywhere/.context/domains/README.md` "Domains" `[biome-group, noise-pattern, mod-options, harmony-patching]` and `.../standards/README.md` "Standards" `[harmony-patching, exception-handling, logging, mod-strings]`, and `smart-weight-plate/.context/standards/README.md` `[code-style, logging]` (a list of links is about nothing in those glosses). Fix: remove section tags from "Where things are" and README index sections; the document-level tags carry the subject.
- **F4.4 (fix)** The 17 `standards/README.md` index the same three documents but carry 11 different document-level tag sets: code-style absent in just-fall-already, refined-building and upgrade-planner and moved to a section in smart-weight-plate and natural-backwalls-everywhere; `exception-handling` only in upgrade-planner; `harmony-patching` in four. Fix: one set across all 17 (or none, per F4.3).
- **F4.5 (fix)** `workflows/task-plan/phase-architecture.md` "Phase Exit / Handoff (run LAST ...)" carries `[commit-messages, phase-handoff]`; the same section in `phase-implementation`, `phase-investigation` and `phase-testing` carries `[phase-handoff]` (completion likewise). The text is the same boundary-commit step in all five. Fix: drop `commit-messages` from the architecture copy (the OM-002 corrections already did this for siblings).
- **F4.6 (fix)** `domains/tooling-friction.md`, entry "2026-10-17 icon-init: --exclude does not drop a top-level folder from the node set" is tagged `common-submodule` (about the git submodule and keeping the root pointer in step). The entry is about a `ks init` flag. Fix: remove; the document tag `knowledge-store` covers it.
- **F4.7 (fix)** `testing/unit-testing.md`: "Framework and Tools" `mod-build` (nothing compiles or deploys there; the table lists test tools) and `translations` (one table row); "File Organization" `test-assertions` (class and method layout, not what a test checks). Fix: remove `mod-build` and `test-assertions` from these; keep `game-update-compatibility`.
- **F4.8 (fix)** `upgrade-planner/.context/domains/entities.md` "Business Rules" `[vanilla-fallback]`: the rules say the mod never changes what the game lets replace what, which is `building-replacement`, not the fallback when something is missing (only the Blueprints Expanded bullet fits). Fix: replace with `building-replacement`, or leave the section to its document tags.
- **F4.9 (nit)** `unit-testing.md` "Test File Location" `mod-build`, "Naming Conventions" `[test-assertions, screenshots]`; `integration-testing.md` "Launcher Options" `[save-files, mod-build]` and "Screenshots" `steam-workshop`: each term matches one row or clause of the section. Remove the weakest.
- **F4.10 (nit)** Glossary entries with borderline terms: natural-backwalls-everywhere "Backwall" `settle-sim`, "Feature room" `biome-group`; jumper-bridges "Overload" `automation-ribbon` (a wire bridge breaking when a ribbon passes it).
- **F4.11 (nit)** Root overview document tags `mod-build, release-process, steam-workshop, common-submodule`: the overview mentions release and Workshop in single sentences. Drop two, or accept as a map of the repository.
- **F4.12 (nit)** `phase-architecture.md` "Additional Architecture Review Triggers" `[mod-build, translations, mod-compatibility, release-process]`: each term fits one bullet of a trigger list; acceptable for a list section.

## Pass 5: wording

Coverage: every overview, domains README, standards README and tags.md (all 18 nodes), scanned by pattern for hedges (may, might, typically, generally, probably, likely, usually, seems, appears, and similar) and read for invented facts.

- No hedge or disclaimer in any overview, README, description or gloss (the only hits are mod names such as "Just Fall Already!" and the tags.md frame "each term that records here may carry").
- No invented fact found in the claims listed under pass 1, apart from F1.1 (wrong file) and F1.3 (unsupported sentence).
- Credits are stated plainly (3GuB, Cairath, Skyrunner, Berkays/Rubacava, Sgt_Imalas).

No findings beyond F1.1 and F1.3.

---

## Answers to the handoff questions

- **Did anything in the pointers not apply?** All of it. The brief returned about 30 near-identical copies of shared `standards/` sections (`try/catch`, `Yielding to Other Mods`, `Braces`, `Indentation`) because their text cites source paths under `mods/`. Nothing in them bears on reviewing store documents. I opened none. This is the same failure the 2026-10-17 tooling-friction entry "briefs fill with near-identical sections" already records: it recurred, and the budget (8192 of 133258 bytes) never reached a single record that governs this work.
- **Did anything that governs this work fail to appear?** Yes: the rules the review is measured against did not appear: `tag-discipline` (one-sentence gloss, no boundary clause, enumerating gloss is too coarse), `tag-backfill` step 2 (admission floor of 3 records across 2 documents, 85% bar, location-not-subject) and `description-backfill` (contents not summary, one line). I found them by reading `C:/dev/ai/icon/skills/`. The root `tags.md` and the 17 node dictionaries also never surfaced, since they carry no coverage and no tags (the brief's own warning lists `tags.md` and the `mods` overview).
- **Did a pointer's stated reason not match its record?** The reasons said "names mods/.../Mod.cs ..." and that is true of the text, but the match is the shared text quoting another mod's file, which is why the same pointer appeared 17 times; the stated reason does not say the records are copies.

## Friction for the coordinator to record (a sub-agent does not write it)

- Invocation: `ks handoff` brief for scope `.context mods` on a review of store documents. Did: filled the budget with 30 near-identical `try/catch`, `Yielding to Other Mods` and formatting sections from the 17 mod standards; 480 further candidates not listed. Expected: pointers to the governing tag, description and store-discipline records, or one copy of the repeated text with a count. Did instead: read the packet's spec and the ICON skill files directly. This repeats the 2026-10-17 populate-context entry.
- Not ICON: `python -I` ignores `PYTHONIOENCODING`, so printing a store file containing an arrow character crashed one of my scripts (cp1252 stdout); fixed with `sys.stdout.reconfigure`.

## Suggested order for the fixes

1. F1.1 (six one-sentence edits).
2. F3.1, F3.2 gloss changes first (a dictionary change-set is separate from tag application), then F4.1 to F4.8 as one tagging pass per node that applies one tag set to shared sections, removes tags from index sections, and clears F4.5 to F4.8.
3. Nits as wanted.
