---
doc_id: doc-62bc27b793da
description: "Dated log of friction met with ICON's ks engine, skills, hooks and agents while working in this repository."
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
