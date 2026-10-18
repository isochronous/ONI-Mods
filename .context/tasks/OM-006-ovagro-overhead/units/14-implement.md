# Unit 14 — implement: draft the Ovagro Overhead store node's documents as change-sets

## Objective
Draft, as change-set JSON files in this task folder, the documents the new store node mods/ovagro-overhead/.context needs to match its sibling mods' nodes. Do not apply them: the manager plans and applies each through `ks write`. Plan step: Completion, store update.

## Scope
- IN: change-sets under .context/tasks/OM-006-ovagro-overhead/changesets/node-*.json for the node `mods/ovagro-overhead`:
  - overview.md: fill its TODO(owner) sections (section writes; `ks write schema --row narrative --mode section`), the way mods/just-fall-already/.context/overview.md reads.
  - domains/README.md and standards/README.md: fill their TODO(owner) placeholders as the siblings' READMEs read (index of the node's documents).
  - New documents (narrative create; `ks write schema --row narrative --mode create`, each with a fresh one-line description): standards/code-style.md, standards/error-handling.md, standards/naming-conventions.md, domains/entities.md, domains/glossary.md, each adapted from mods/just-fall-already/.context's document of the same name to this mod's code (mods/ovagro-overhead/src/**). Every example and file reference names this mod's real code; no invented facts; TODO(owner) where the code gives no answer.
  - error-handling.md must name the user-approved fifth try/catch case: Mod.cs's all-or-nothing patch catch (unpatch everything, warn once, Ovagro stays floor-only), with why (plan.md Decisions, "Patch failure at load").
  - entities.md covers VineMotherCeiling, VineMotherCeiling_preview, VineBranchCeiling, the routed vanilla VineMotherSeed and the CeilingForm swap, and what removing the mod does (plan.md Decisions; units/03-design-report.md).
- OUT: META.md, iconrc.json, decisions/ (the manager files the decision record), retrospectives/, tags and tags.md, any file outside the task folder. No game launch.
- Acceptance: for every change-set, `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at .` (from C:\dev\ONI-Mods) exits 0 and its plan output is pasted in the report; nothing applied (`git status --short` shows only new files under the task folder). Do not claim a clean plan without the output.

## Context the packet does not carry
- plan.md (`## Decisions`, `## Key Files`) and the reports units/03, 06, 07, 12 describe the code and the rulings.
- A section heading containing `/` is read as a nested address by `ks write`; avoid slashes in headings you create.
- No hedging language in documents.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/14-implement-report.md
- Must contain: each change-set file and what it writes, the `write plan` output for each, open TODO(owner) markers left and why, and a "Store reads" section.
- Model tier: default
