# Unit 09 report — tag vocabulary for the root node `.`

Output: `work/tags/vocab-root.json` (parses; every evidence id checked against `lexicon-root.json`; every gloss starts with the required words).

## Term counts

- Node `.`: 139 records (13 documents + 126 sections). 85% bar = about 118 records.
- 29 terms proposed. Widest: `in-game-testing` at about 55 records (40%), then `task-plan` at about 40 (29%). All other terms reach 18 records or fewer.
- Floor: every term reaches at least 3 records across at least 2 documents. Four sit right at or near the floor and are the first ones to cut if the check wants fewer: `screenshots` (3 records/2 docs), `assistant-attribution` (3/3), `test-pacing` (4/2), `mod-loadout` (4/2).
- Proposed from the lexicon (titles, descriptions, headings) first, then extended by reading all 13 bodies (84 KB total, so I read every one rather than a sample). Terms found only in bodies: `test-pacing`, `mod-loadout`, `player-log`, `game-update-compatibility`, `mod-compatibility`, `common-submodule`, `assistant-attribution`, `code-review`, `github-issues`.

Terms by area:
- Testing (12): in-game-testing, test-pacing, test-cleanup, test-assertions, simulated-input, generated-worlds, save-files, screenshots, mod-compatibility, mod-loadout, game-update-compatibility, player-log
- Build and environment (4): machine-setup, mod-build, common-submodule, translations
- Release (4): release-process, steam-workshop, changelog, github-issues
- Git (4): commit-messages, assistant-attribution, git-branches, task-id
- Task workflow and store (5): task-plan, phase-handoff, unit-spec, code-review, knowledge-store

Nested terms I kept on purpose (the narrower one is something people ask for by name, and its gloss is a different subject): `steam-workshop` and `changelog` inside `release-process`; `phase-handoff` and `unit-spec` inside `task-plan`; `task-id`, which sits across `git-branches`, `commit-messages` and `task-plan`. The check should confirm these are not near-synonyms.

## Closest calls dropped

- **retrospective**: 3 records (completion retrospective template, two-stage retrospective, completion exit), but all in `phase-completion.md`. `base.md` mentions the Retro Stage-1 draft in one line, which is not enough to say it is about retrospectives. Fails the 2-document clause. Those records are covered by `knowledge-store` and `task-plan`.
- **pull-requests**: branching's PR guidelines, before-merging checklist and task-work merge TODO, all in one document. Fails 2 documents.
- **change-verification** (build and test commands plus the copied evidence): about 8 records across 7 documents, so it passes the floor. Dropped because every record it gathers is already reached by `mod-build`, `in-game-testing` or `phase-handoff`. It would mostly duplicate their union. The closest call of the set.
- **harmony-patches**: 3 records/3 docs (integration scope, architecture review triggers, research spec topic list), but each one only mentions Harmony in passing. No record is about patching.
- **discord-notice**: 4 records, all in `ci-cd.md`. Fails 2 documents.
- **secrets**: only `ci-cd.md` secrets management. Fails.
- **test-duplicants** (spawning Duplicants in tests): seeding and mocking patterns, about 3 records/2 docs, but it is a helper list rather than a subject anyone would search for. Covered by `in-game-testing`.
- **troubleshooting**, **related**, **template**, **examples**: shape terms, barred.
- **workflows** / **testing** as terms: folder names whose honest meaning here is the directory. Barred as locations. `task-plan` is also a folder name, but I kept it because it names a shared concern that reaches records outside the folder (commit-conventions active task work, base and branching task IDs).

## Rules found wrong or unclear

- **Floor counted against inheritance.** The floor counts "records", but a document tag reaches every section below it by inheritance. A term on `testing/unit-testing.md` at document level gathers 21 records on paper but would be written once. I counted inherited sections in `est_records` (for example `in-game-testing` at 55), which is how the 85% bar has to be measured. For the floor, though, a document-level term on one document with sections would clear "3 records" by itself. The 2-document clause is the only thing stopping that. The rules should say which count the floor uses.
- **"Gloss: no list of sub-topics" and "one sentence" pull against precise glosses** for umbrella terms like `release-process`. I wrote "from version bump and tag to the published download", which is a span rather than a list. The checker may read it as a list.
- **Location bar against `common-submodule`.** It names a directory, but it also names the submodule relationship: commit inside first, then bump the pointer. I kept it with a gloss about that relationship. This is the term most likely to fail the location bar, so the check should look at it.
- Nothing else looked wrong.

## Handoff brief feedback (section 2 of the packet)

- The one pointer shown (`blk-c3ff9a6f1225`, a seeding lesson) did not govern this work. Nothing relevant was missing from the brief: the work is governed by the tag-backfill skill and the lexicon, and neither lives in the store.
- The warning that 5 records "declare no coverage and no tags" is correct for this node.

## Friction met

- `ks tags --glosses --store C:/dev/ONI-Mods` printed "(no terms — nothing declared, nothing carried)". That is correct before the mint and caused no problem.
- None otherwise.
