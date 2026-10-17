# Unit 14 report: confirmation review of the unit 12 fixes

Reviewer: ICON review unit, default tier. Read only; the store and repository were not changed. Scripts: session scratchpad `rev14/` (`rev14.py`, `unused.py`), run with `python -I`. They parse the markdown files directly (front matter tags, `**Tags:**` lines, heading ancestry) and read the node dictionaries from each `tags.md`.

Result: every `fix` finding of unit 12 is resolved. No block, no fix-level new finding that breaks a rule the store checks. 1 new finding at fix-low (unused dictionary terms), 3 nits.

## Pass 1: per-finding table

| Finding | Resolved | Evidence |
|---|---|---|
| F1.1 `mod.yaml` named for `supportedContent`/`minimumSupportedBuild` | yes | All six overviews (motion-sensor-range, multichannel-critter-sensor, natural-backwalls-everywhere, no-magic-fridges, refined-building, salty-surf) now read "`mod_info.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`". `git diff HEAD --word-diff` on the 17 mod overviews and the root overview: the only non-tag body change is `` `mod.yaml` `` to `` `mod_info.yaml` `` in exactly those six; nothing else in any overview body changed. |
| F3.1 `developer-tooling` boundary clause | yes | natural-backwalls-everywhere and vent-freeze-fix `tags.md`: "A record earns this term when it is about debug and developer features a mod includes for its author." No "rather than". |
| F3.2 `mod-entry-point` code-location term | yes, by the chosen route | The gloss is unchanged (a decision recorded in unit 13: tags stop going on file indexes). The 17 "Where things are" sections carry no tags (script: 0 map sections with tags, 18 nodes). The term now sits on `entities.md` entry sections and on the "<Mod> specifics" sections of `code-style.md`, which are the UserMod2 class descriptions, not folder maps. See N1 for the side effect. |
| F4.1 same text, different tags per mod | yes | Script over all 59 sections whose text is identical in at least 3 mod nodes (standards code-style, error-handling, naming-conventions, README; ignoring Id and Tags lines). Effective tags (document + enclosing headings + own) restricted to the terms every mod in the group declares: 0 sections differ (was 34 of 56 with different sets). Unrestricted, 15 sections still differ between mods; each difference is only a term that some mods do not declare (for example `mod-compatibility` on "Yielding to Other Mods": present in 6 mods, absent from the other 11 dictionaries; `game-component`, `reflection`, `transpiler`, `mod-strings`, `performance`, `exception-handling`, `mod-options`). Same canonical set everywhere, as unit 13 designed. |
| F4.2 wrong terms on shared sections | yes | "Ids and Tags": no section tag in any mod, effective set `code-style` only. "What to Include": no section tag (covered by the `logging` parent). "try/catch": `logging` (+ `exception-handling` where declared) in all 17; no `mod-compatibility`/`worldgen`/`reflection`. None of `hotkey`, `game-tag`, `building-config`, `performance`, `developer-tooling` remains on any shared section (they occur only on mod-specific sections). |
| F4.3 index sections carry subject tags | yes | 0 "Where things are" sections with tags in 18 nodes; 0 README `##` sections with tags (natural-backwalls-everywhere `domains/README.md#domains`, `standards/README.md#standards`, smart-weight-plate `standards/README.md#standards` all empty); root overview "What this is" has no `**Tags:**` line. |
| F4.4 standards README and shared document tag sets | yes | Document-level tags of the four shared standards documents, counted over 17 mods: `standards/README.md` `[code-style]` x17; `standards/code-style.md` `[code-style]` x17; `standards/naming-conventions.md` `[code-style]` x17; `standards/error-handling.md` none x17. One set each (was 11 distinct README sets). |
| F4.5 `commit-messages` on phase-architecture exit section | yes | `.context/workflows/task-plan/phase-architecture.md` "Phase Exit / Handoff" `**Tags:** phase-handoff`, identical to completion, implementation, investigation, testing. |
| F4.6 `common-submodule` on the `ks init --exclude` entry | yes | `.context/domains/tooling-friction.md` entry `2026-10-17 icon-init: --exclude ...` has no `**Tags:**` line. |
| F4.7 `unit-testing.md` misplaced terms | yes | "Framework and Tools" `[game-update-compatibility]` only; "File Organization" has no tags. (Out of scope nit F4.9 left as is: "Test File Location" `mod-build`, "Naming Conventions" `[test-assertions, screenshots]`.) |
| F4.8 `vanilla-fallback` on upgrade-planner "Business Rules" | yes | `mods/upgrade-planner/.context/domains/entities.md#business-rules` has no section tag; the document carries `building-replacement`. |

Also checked: the four `naming-conventions.md` descriptions (F2.1, nit, optionally fixed) no longer say "ModStrings keys" in dont-count-eggs-on-rails, just-fall-already, no-magic-fridges, refined-building. The mod-strings tags on the salty-surf and sweep-zones specifics sections (fix12b) are present (`reflection, harmony-patching, mechanical-surfboard, mod-strings`; `hotkey, saved-state, reflection, mod-strings`).

## Pass 2: new errors

Scanned every `.md` in all 18 nodes outside `tasks/` and `work/`.

- Term outside its node's dictionary: 0 (document level and section level). The engine check says the same (below).
- Section repeating a term its document or an enclosing heading carries: 0. Duplicate term inside one tag line: 0.
- Folder-map section with subject tags: 0.
- Broken overview sentence: none. All body diffs against HEAD outside tag lines are the six `mod_info.yaml` edits, the four description edits, the two `developer-tooling` glosses, and task bookkeeping (`plan.md`, tooling-friction text). No existing block id changed value: 85 Id lines appear in the diff only because the tag edit re-emitted the line (same id, trailing spacing), and 3 ids are new, on the new retrospective and tooling-friction entries.

New findings:

- **N1 (fix, low)** Three dictionary terms are now used by no record: `mod-entry-point` in `mods/smart-weight-plate`, `mods/sweep-zones` and `mods/vent-freeze-fix` (`tags.md` entry exists, no document or section carries it). Unit 12 reported "no unused term"; this is the effect of removing the term from "Where things are" without a specifics or entities section to take it. Fix: add `mod-entry-point` to the mod's UserMod2 entry section in `domains/entities.md` where one exists (as the other 14 mods have), or retire the term in those three dictionaries.
- **N2 (nit)** Terms now under the admission floor of two documents: `mod-entry-point` sits in one document in 11 nodes (1 section each); `vanilla-fallback` sits in one document (`standards/error-handling.md`, 5 sections) in detect-any-element, just-fall-already, multichannel-critter-sensor, no-magic-fridges, refined-building, smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner; `exception-handling` in upgrade-planner (3 sections, one document); `mod-compatibility` in natural-backwalls-everywhere carries one section only. These are the consequences of one canonical set on shared text and were accepted in unit 13 ("residual gaps by design"). Listed so the manager can decide to keep or retire; same family as unit 12 F3.6.
- **N3 (nit)** "Yielding to Other Mods" (shared error-handling section) is tagged `mod-compatibility` in 6 mods and untagged in the other 11, because those dictionaries lack the term. A tag query for `mod-compatibility` therefore returns 6 of 17 copies. Consistent with the canonical set, noted for completeness.
- **N4 (nit)** Unit 12 nits left open as decided (F1.2 to F1.5, F3.3 to F3.6, F4.9 to F4.12). Verified still present, for example the bitshifter overview still ends "Players of ribbon-based automation builds use it." (F1.3).

## Pass 3: check

`node c:/dev/ai/icon/bin/ks.mjs check --all --strict --store C:/dev/ONI-Mods`

Summary line: `27 checks · 17 ran · 10 not run · 0 block · 9 advise · 0 inform`

Tag check line: `every tag on a record names a term some dictionary declares — 0 block, 0 advise, 0 inform   (covered 149 paths)`. The 9 advise are all "linked-tree document is not reachable by following references" on root documents (`domains/tooling-friction.md`, `workflows/branching.md`, `commit-conventions.md`, `task-plan/*`); `plan.md` already records "0 block, 9 advise" before this unit, so none is new. Generated artifacts: "every generated artifact matches the sources it is generated from (4 registered)". Notes: "18 while reading: text before the first `### ` entry belongs to no record" (unchanged system-file notice).

## Handoff questions

- Pointers that did not apply: all of them (the same ~30 shared `try/catch`, `Yielding to Other Mods`, `Braces`, `Indentation` copies). I opened none.
- Governing records that failed to appear: the node `tags.md` dictionaries and the unit 12 and 13 reports (task scratch). Found by reading the files.
- Pointer reasons that did not match: the reason names source paths the text quotes, not that the 17 records are copies of one text. Same as units 12 and 13.

## Friction for the coordinator to record

- Invocation: `ks handoff` brief over `.context mods` for a review unit. Did: spent the 8192-byte budget on near-identical shared standards sections (480 candidates unlisted). Expected: governing records or one copy plus a count. Did instead: read the packet's spec and the unit reports. Third recurrence (units 12, 13, 14).
- Not ICON: none.
