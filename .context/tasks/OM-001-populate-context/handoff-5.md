# Handoff — OM-001-populate-context

**Scope:** `mods/smart-weight-plate`, `mods/storage-pod-redux`, `mods/sweep-zones`, `mods/upgrade-planner`, `mods/vent-freeze-fix`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-001-populate-context/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-001-populate-context/units/05-implement-domains.md`

```
# Unit 05 — implement: draft domains change-sets for mods:smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix

## Objective
Draft one change-set per listed mod leaf creating `domains/entities.md` and `domains/glossary.md`, adapted from the ICON templates. Plan step: "Units 02-05".

## Scope
- IN: nodes `mods/<name>` for:smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix.
- Read each mod's source, README, mod.yaml / mod_info.yaml, strings and tests to learn what it does.
- entities.md: the mod's real things — buildings, components, Harmony patch targets in the game (classes and methods patched), options/settings, saved state — each with its file location and relationships; business rules = the gameplay rules the mod enforces; key code paths with file references; state transitions only if the mod has states (otherwise cut that section). Title it after the mod's subject (e.g. "Jumper Bridges domain").
- glossary.md: ONI and mod-specific terms a newcomer needs (game terms the mod relies on, abbreviations, terms to avoid if the code or strings show one), scoped to this mod.
- Each mod already has `domains/README.md`; do not touch it.
- OUT: other mods, root, mods/, common/; standards; applying.
- Acceptance: one file per mod at `.context/tasks/OM-001-populate-context/changesets/domains-<mod>.json`, 2 create ops each, each accepted by `write plan` (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-001-populate-context/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-001-populate-context/units/05-implement-domains-report.md
- Must contain: per mod, one line on what it does and what the two documents cover; TODO(owner)s left; `write plan` summary per file; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 15 record(s) in these 5 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/smart-weight-plate:domains/README.md, mods/smart-weight-plate:overview.md, mods/smart-weight-plate:standards/README.md, mods/storage-pod-redux:domains/README.md, mods/storage-pod-redux:overview.md and 10 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
