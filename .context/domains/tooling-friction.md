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
