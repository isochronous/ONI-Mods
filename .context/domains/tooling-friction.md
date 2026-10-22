---
tags: knowledge-store
doc_id: doc-62bc27b793da
description: Dated entries recording friction met with ICON's ks engine, skills and hooks in the ONI-Mods repository, each giving the invocation, what happened, what was expected and the workaround
witness: none
---

# Tooling friction

Each entry is one dated section naming the task, the invocation, what it did, what was expected, and what was done instead. An entry is not a ticket.

## 2026-10-17 icon-init: an interrupted apply left a tree neither init nor integrate could plan over
**Id:** blk-f80f26653b94

An earlier `ks init --apply` run at 13:23 stopped partway: 11 of 17 mod nodes were fully written, and the root, `mods/`, `common/` and the other 6 mods held an empty `.context/` directory (the root one holding only `.ks/`). A fresh `ks init --at . --task-prefix OM` then refused ("11 .context/ stores already exist here") and routed to `ks integrate --at C:/dev/ONI-Mods --task-prefix OM`. That integrate plan classified every empty `.context/` directory as "leaf, already a node", proposed leaf files for the root and `mods/`, and could place no roster row for any node ("no connective node above it"), so applying it would have produced an unwired tree. Expected: either verb recognises an empty `.context/` as no node and plans a coherent tree, or `init` offers to resume an interrupted apply. Done instead: moved the 110 pristine leftover files and the empty directories out of the repository by hand, then ran `init` on the clean tree, which planned and applied correctly.

## 2026-10-17 icon-init: --exclude does not drop a top-level folder from the node set
**Id:** blk-37372ca436a2  

`ks init --at . --task-prefix OM --exclude common` reported `common` under "not walked — excluded at this node or an ancestor" yet still listed it in the proposed tree and in "20 node(s) to write", with `common/.context/` among the files to write. `--nodes "mods/*"` with the same exclude made no difference, and neither did a trailing slash. `common` is a git submodule (oni-mods-common), so the node written there lands inside a different repository and cannot be committed from this one. Expected: an excluded directory is neither walked nor written, or the help text says exclusion only limits the walk. Done instead: applied the plan without `--exclude` so the `common` node at least carries its real project roster, and left `common/.context/` and `common/.gitattributes` for the owner to commit in the submodule's own repository or remove.

## 2026-10-17 populate-context: every mod node is aggregator scope, so the leaf-only standards rule matches no node
**Id:** blk-bffbb3a20912

populate-context seeds `standards/` only where the node is not `root` or `aggregator` scope. `ks resolve mods/sweep-zones/src --json` reports `scope: aggregator`, declared in the node's META.md, with the signal "projects.md below the governing node": each mod's projects.md lists its csproj folders (`src/SweepZones`, `src/SweepZones.Tests`), and `ks write plan` labels every mod node `[aggregator]`. Those csproj folders are not nodes, so read literally the rule leaves no node in the store eligible for standards, while `icon-init` gave every mod node a `standards/README.md`. Expected: a node with no child nodes is a leaf for populate-context, or the skill's rule names the deepest node rather than the declared scope. Done instead: landed the three standards documents at all 17 mod nodes, the deepest nodes holding code.

## 2026-10-17 populate-context: briefs fill with near-identical sections of a body repeated across nodes
**Id:** blk-88e2ff500324  
**Tags:** unit-spec

After OM-001 landed the same shared standards body (code style, error handling, naming) at all 17 mod nodes, `ks handoff --scope .context/workflows .context/testing mods ...` composed briefs whose budget went to near-duplicate sections of those standards: the review unit reported 30 of 496 candidates listed, all of them copies of one section, and the fix unit reported the same. The documents the units actually needed, such as the ICON templates the task-plan review was measured against, never appeared. Expected: the brief ranks one copy of a repeated block and spends the rest of the budget elsewhere, or says it collapsed duplicates. Done instead: the units found what they needed by path.

## 2026-10-17 populate-context: a cover op's refusal does not name the missing witness, and a section and a cover cannot share a change-set
**Id:** blk-9ebf6bc063df

Adding `applies_to` to an existing document with a `cover` operation that lacked `witness`, `ks write plan` refused with text saying "`cover` is the mode that declares it", although the operation already was `cover`, and it never named `witness` as the missing field. Separately, Jumper Bridges' `domains/entities.md` needed a section edit and a coverage claim; the writer refuses two operations on one destination in one change-set, so the coverage went into a second change-set applied after the first. Expected: the refusal names the missing field, and a section edit and a coverage change on one document compose in one transaction. Done instead: set `witness: loose` on every cover op and split the Jumper Bridges change into two change-sets.

## 2026-10-17 tag-backfill: a library and a brief, but no command to run
**Id:** blk-4e42b38452ab

The tag-backfill skill describes a seven-step run and ships `lib/backfill/*.mjs` (surveyStore, chunkDocuments, taggingInput, beginPass/completePass, validateResponse, generateOperations, runBackfill) plus the tagging brief, but no CLI that drives them. Running it over 18 nodes took a 150-line driver (`.context/tasks/OM-002-store-backfills/work/tags/tagwork.mjs`): split the store-wide survey per node because each node needs its own dictionary, write one input per chunk, open and close passes, validate each chunk against its own chunk survey (a part chunk's expectation differs from its whole document's), merge, check every term against the node's `tags.md`, refuse when the node changed since its survey, then generate and run rounds. Expected: a `ks tag-backfill` verb with survey, chunk, intake and write steps, node-aware. Done instead: wrote the driver in the task folder.

## 2026-10-17 coverage-backfill: nothing to propose, and no door for documents that cover nothing
**Id:** blk-a378f3fb67b5

`coverage.mjs survey` exited 0 at all 19 nodes: 45 undeclared documents, every one no candidate (root workflow and testing documents: spread or one target only; README indexes: no resolved token). The door declares `witness: none` only when a proposal is rejected on question 2 or 3, so a practice document that never gets a proposal stays undeclared, and every sweep keeps listing it as uncovered. Hand-declaring `witness: none` would work, but no write-path mode removes a witness, so it can only be undone through git. Expected: a way to record "considered, covers no place" for a no-candidate document through the door. Done instead: left the 45 documents undeclared.

## 2026-10-17 resolve: a submodule inside the repository becomes its own store, outside the root's node graph
**Id:** blk-535873fcc26c

`common/` is a git submodule (oni-mods-common) inside this repository, and the root's `projects.md` lists it as a member. `ks resolve common --json` still reports `store_root C:/dev/ONI-Mods/common` with its own checkout key and a chain that stops at `common/` itself, and `ks map`, `ks check --all` and `ks sweep` at the root cover 19 nodes without it. Nothing in `iconrc.json` or any verb's options lets the parent store adopt a nested checkout. OM-001 and OM-002 therefore left `common/` out of populate-context and of every backfill. Expected: a directory inside the repository is a node of the repository's store, a submodule included, with writes there committed in the submodule and the pointer bumped in the root. Done instead: nothing yet; including `common/` needs a resolver change in ICON's engine.

## 2026-10-17 icon-integrate: inserting a tier relabels one roster row and leaves the rest pointing past it
**Id:** blk-19571747a73c

`ks init --at common --task-prefix OM` refused (a store already exists) and routed to `ks integrate --at common`. Its plan inserted a `tools` aggregator between `common` and its six tool projects. Applying it wrote a correct `tools/.context/projects.md`, but rewrote `common/.context/projects.md` so the row named `ModCheck` pointed at `tools` while the other five rows still pointed at `tools/SaveRunner` and the rest, through the new aggregator. `ks check --store common` reported nothing. Expected: the parent's rows for the moved members re-pointed into one `tools` row, the way the `projects` writer's `repoints` describes. Done instead: replaced `common/.context/projects.md` with a single `tools` row through a `repair` of kind `correction`, ratified by its digest. A change-set naming `common` from the root was refused as a second store, so the repair ran with `--at common`.

## 2026-10-17 write schema: the narrative section mode does not name overview.md as a target
**Id:** blk-239a99f5f959

OM-005 (in common). `ks write schema --row narrative --mode section` lists the narrative row's targets as domains, standards, architecture, testing, styling, workflows and team-home paths; `overview.md` is not among them. Both drafting units read that as a reason to doubt their overview change-sets. `ks write plan --at common` accepted every `section` op on `overview.md` under the narrative writer. Expected: the schema's target list to include `overview.md`, since the engine routes it there. Done instead: went by the plan output and applied the change-sets.

## 2026-10-17 tag-backfill brief: no answer for empty headings and link lists in an untagged document
**Id:** blk-a2457b938b06

OM-005 (in common). Eight of ten tagging passes over common's tool nodes reported the same hole in `skills/tag-backfill/tagging-brief.md`. `inherits` is defined as the document's tags or an enclosing heading covering the section, and `no-good-match` requires a missing concept. A heading with no body of its own, or a Related section holding only links, inside a document that got no document tags, fits neither: passes split between `inherits` (against its definition) and `no-good-match` with invented suggestions ("links to related documents"), which then reach the gap list. The same passes split on sub-topics of a term the document carries (Indentation, Braces, Comments under a `code-style` document): the `inherits` rule and the "vocabulary too coarse" warning give opposite answers. The brief's Output section also says to append in batches, which does not fit one JSON object; the manager's note said to rewrite the whole object. Expected: the brief to name the empty-heading and link-list case, and to say which rule wins for sub-topics of a document term. Done instead: triaged every such gap to no new term by hand.

## 2026-10-17 tags and sweep: term counts include records matched through a shared word
**Id:** blk-6cad7a7c6aea

OM-005 (in common), review unit 06. `ks tags --store C:/dev/ONI-Mods/common --node tools/WorkshopUpload` printed 17 for `item-description`, which 3 records carry; the count includes records carrying any term that shares a word with it (`item-visibility`, `item-download`). `ks sweep --store C:/dev/ONI-Mods/common` flagged `screenshots` and `player-log` in TestRunner as single-carrier terms but not `mod-entry-point`, which one record carries, apparently because other terms share the word `mod`. tag-discipline reads the count to check the 3-record floor, so a shared word hides a term under it. Expected: the count of records that declare the term. Done instead: parsed the `**Tags:**` lines with a script and grep.

## 2026-10-17 write plan: a describe and a document tag on one document cannot share a change-set
**Id:** blk-222fa626090f

OM-005 (in common), fix unit 07. `ks write plan --at common` refused a change-set holding a `describe` and a document-level `tag` op on the same document, since both replace its frontmatter, and one holding a `section` rewrite and a `tag` removal on the same heading block, since both replace that block (a section write's `tags` field adds terms but cannot remove them). Expected: two edits to different lines of one region to compose in one change-set. Done instead: split each node's fixes into two files (`fix07-<node>.json` and `fix07-b-<node>.json`) applied one after the other.

## 2026-10-17 write plan: a protected plan's confirm digest goes stale when an earlier change-set edits the same file
**Id:** blk-1c65d7dab6e8

OM-005 (in common). The fix unit planned `fix07-rmterm-TestRunner.json` (a protected `remove` on `tools/TestRunner/.context/tags.md`) while `fix07-mint-TestRunner.json`, which edits the same file, was still unapplied. The digest the plan printed could not ratify the removal after the mint landed, and the plan output gave no warning that a pending write to the same target would invalidate it. Expected: a note in the plan that the confirm digest binds the target's current bytes. Done instead: planned the removal again after the mint and applied it with the new digest.

## 2026-10-17 brief: tag dictionaries listed as records that declare no coverage and no tags
**Id:** blk-31711a20c7d2

OM-005 (in common), confirmation review unit 08. `ks brief --at C:/dev/ONI-Mods/common` over the eight nodes warned that the six `tags.md` dictionaries declare no coverage and no tags, and suggested giving them `applies_to` or tags through the write path. A dictionary is read through `ks tags`, and the tag-backfill skill excludes it from its own backfill by destination. Expected: no coverage warning for a `tags.md`. Done instead: left them undeclared, as the OM-002 dictionaries are, and recorded the decision in OM-005's plan.

## 2026-10-17 resolve: the highest task id ignores the same prefix in the common submodule store
**Id:** blk-c75a87585b6c

Opening OM-006, `ks resolve mods --explain` printed `task ids prefix OM · highest OM-002 (.context/tasks/OM-002-store-backfills)`. The `common/` submodule's own store already holds `OM-003-integrate-common`, `OM-004-populate-common` and `OM-005-common-backfills` under the same `OM` prefix, so taking the next number from resolve would have minted OM-003 a second time. This is the same boundary as the earlier entry on the submodule becoming its own store (the submodule is its own store, outside the root's node graph), seen from the task-id side. Expected: the highest id counts every store that shares the prefix inside the checkout, or resolve warns that a nested store uses the prefix too. Done instead: listed `common/.context/tasks/` by hand and took OM-006.

## 2026-10-17 write plan: a slash in a new section heading is read as a nested address
**Id:** blk-b32f774e33ce

Recording the task-id entry, `ks write plan` refused a `create_section` op whose heading contained `common/ submodule`: it read the `/` as a block address separator and asked for a parent section named everything before the slash. Expected: `create_section` takes the heading text literally, or the schema says a heading may not contain `/` and how to escape one. Done instead: reworded the heading without the slash.

## 2026-10-17 handoff: a scope spanning several mod nodes fills the brief with per-mod copies of one record
**Id:** blk-80598ce7fbc3

OM-006 unit 02 (how a new mod is scaffolded) got a packet from `ks handoff --scope ONI-Mods.sln Directory.Build.props mods/just-fall-already mods/vent-freeze-fix mods/detect-any-element common/tools common/README.md --decisions`. Its 47 pointers were mostly each mod's own `standards/code-style.md` and `domains/entities.md` sections (nine from detect-any-element's `entities.md` alone, two code-style sections from each of eight mods); none pointed at `workflows/ci-cd.md`, `testing/unit-testing.md` or common's TestRunner and release records, which answer the unit's questions. The unit also reported that the code-style pointers gave one-line reasons naming detect-any-element's `Patches.cs` for records belonging to other mods. The unit read four pointers with `ks read` and answered the rest from files. Expected: records that are near-identical across sibling nodes collapse to one pointer, so the budget reaches the records the scope's own question needs. Unit 01, scoped to `Directory.Build.props mods` for a question answered only by decompiled game code, got 27 of 508 candidates, all per-mod error-handling and code-style standards, and read none. Done instead: nothing in the tool; the manager's scope was also wider than the question (three sample mods and all of `common/tools`), so later packets name only the folders a unit will touch and pass `--tag` for subject records.

## 2026-10-17 brief: a tag disjunction widens through shared words, and a path packet reaches no subject records
**Id:** blk-6c43aeeca587

OM-006 design unit 03 ran `ks brief --tag 'mod-compatibility || save-files || game-update-compatibility'`. `game-update-compatibility` pulled in every `in-game-testing` record, so the answer was the same broad testing set the unit already had. That is documented behaviour, not a defect: `ks brief --help` says a term matches any record whose tags share a token with it. The cost is that a hyphenated term sharing a common word (`game`, `testing`) reaches far wider than its dictionary entry; narrowing takes an `&&` with a second term, not a longer `||` list. The unit read only the records whose own tags fit.

The same unit's packet (`ks handoff --scope mods/just-fall-already/src .github/workflows/label-mod.yml --decisions`) spent its 8 KB on per-mod copies of the code-style sections and returned no decision records; the records that governed the design (the new-mod architecture trigger `blk-c8ecd5229a93`, the testing records) came only from tag queries the unit ran itself. Expected: a packet for a design unit reaches the subject records its spec names. Done instead: the manager passes a grouped `--tag` expression into later packets and names governing record ids in the unit's prompt.

## 2026-10-17 handoff and brief: several --tag arguments are refused once one of them holds &&
**Id:** blk-0d476b3d9191

Not an engine fault: a caller error, kept here with its correction. `ks handoff ... --tag 'in-game-testing && test-pacing' 'mod-build' 'release-process && steam-workshop'` was refused as malformed, correctly. `ks brief --help` documents the grammar: terms union unless `&&` conjoins them, `||` spells the union, parentheses group, and `&&` binds tighter than `||`. Ungrouped, those arguments read as one expression with two terms side by side. The right form is one grouped expression, `--tag '(in-game-testing && test-pacing) || mod-build || (release-process && steam-workshop)'`, which the next run used and which composed. Lesson: read the verb's `--help` before composing a `--tag` expression, and group every `&&` clause with parentheses when it sits beside other terms.

The real friction from that run: with `--budget 16384`, the packet still gave 66 of its 70 pointers to per-mod `code-style.md` and `entities.md` sections (each mod's dictionary tags its code-style sections `mod-build`) and 4 to the root's testing and release records. The manager named the governing root ids in the unit's prompt.

## 2026-10-17 integrate: a root run re-adds the tool rows the common roster repair removed
**Id:** blk-f52a5dcb40e8

To wire the new mods/ovagro-overhead node, `ks integrate --at .` planned and applied (confirm digest `sha256:4b26549a...`). It did the expected work: a new `mods/ovagro-overhead/.context/` node and one row in `mods/.context/projects.md`. It also appended `ModCheck`, `SaveRunner`, `TestRunner`, `StringsTemplate`, `WorkshopUpload` and `WorldgenDriver` rows to `common/.context/projects.md` — the rows the OM-003 repair (entry "icon-integrate: inserting a tier relabels one roster row") replaced with a single `tools` row — although the same plan listed `common` under "not walked — a nested git repository is a different store". The plan also listed `.gitattributes` and `.gitignore` under every existing node, none of which changed on disk. Expected: a root run leaves a nested store's roster alone, and the plan lists only bytes it will change. Done instead: restored `common/.context/projects.md` with `git -C common checkout`, kept the two intended writes.

## 2026-10-18 write: a retrospective log cannot be renamed
**Id:** blk-513a68d2b02d

The root's retrospective entries (OM-001, OM-002, OM-006) had been appended to `retrospectives/shared.md`, the log `retro append` writes when no `log` is named; this repository's log is `retrospectives/icon.md`. A narrative `rename` from `retrospectives/shared.md` to `retrospectives/icon.md` was refused: "retro has no `rename` mode" (its modes are append, remove, repair, replay, revise, scaffold). The only write-path route was re-appending each entry to the `icon` log and removing it from `shared` (protected), which mints new entry ids and drops the old ones. Expected: a rename of a whole log that keeps its bytes and ids, or a node setting naming the default log. Done instead: `git mv` of the file at the user's direction, bytes unchanged; `ks map --kind lesson` lists all three entries under `retrospectives/icon.md` and `ks check` reports no block. Later retro writes name `log: "icon"`.

## 2026-10-18 renaming a project: no door for a node's folder, and rename --check refuses on a submodule
**Id:** blk-df578066c6b1

Renaming the unreleased mod `mods/ovagro-overhead` to `mods/ovagros-overhead` (OM-006) moved its store node with the folder. No verb moves a node: `projects` `repoint` rewrites a row's path cell but keeps its name cell, so the parent's row would have kept the name `ovagro-overhead`, and the node's own scaffold names itself in `META.md`, `overview.md`, the README descriptions and `tags.md`. Done instead: the folder moved with `git mv`, then one protected `repair` (kind `correction`) per file in each node (`mods/ovagros-overhead`, `mods`, root), each the file with only the names changed, ratified by its digest; the decision record kept its text and gained an addendum. Afterwards `ks rename --check ovagro-overhead` refused: "could not read 1 file: common (EISDIR: illegal operation on a directory, read)" — the submodule's gitlink is a tracked path the sweep tries to read as a file. Expected: a node move that repoints the parent row (name and path) and renames the node's own self-references, and a sweep that skips gitlinks. Done instead: `git grep` over the tracked files, history excluded.

## 2026-10-21 brief: other mods' code-style sections crowd out the target mod's records
**Id:** blk-2dfcc5b0839b

`ks brief --for mods/jumper-bridges` (8192-byte budget) returned 36 of 49 candidates; 22 of the shown pointers were other mods' `standards/code-style.md` "Line Length" and "Harmony Patches" sections, matched by source-mention because each names a jumper-bridges file as an example. They spent the budget and the brief was truncated before the jumper-bridges glossary record on joint plates (`blk-279f9dc39e51`), the record most relevant to the task. Expected: the target node's own records to rank above sibling nodes' mentions of its paths. Done instead: the explore unit grepped the glossary to find it. `ks read` with several ids in one call worked cleanly.

## 2026-10-22 write: a section body starting with a field line reshapes the block's metadata
**Id:** blk-f755cc056946

OM-010 store updates used `write plan`/`apply` section ops whose bodies began with the block's `**Description**:` line (the `**Id:**` and `**Tags:**` lines left out, as the skill says). The writer kept the Tags line but dropped its trailing two-space line break and inserted a blank line before `**Description**`, so the metadata lines now render as separate paragraphs (seen in mods/just-fall-already `domains/entities.md` FallPoke). Expected: the field lines under the heading to keep their shape. Left as written; it renders and `ks check` reports no block. Also recurring: handoff briefs for any mod path fill with each sibling mod's copy of the same standards sections (try/catch, Game Components), crowding out the target mod's `entities.md` (see the 2026-10-21 entry).
