# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead`, `mods/just-fall-already/.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/14-implement.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 4 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
