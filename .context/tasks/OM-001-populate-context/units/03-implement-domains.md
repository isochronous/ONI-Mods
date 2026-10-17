# Unit 03 — implement: draft domains change-sets for mods:bitshifter, corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges, just-fall-already

## Objective
Draft one change-set per listed mod leaf creating `domains/entities.md` and `domains/glossary.md`, adapted from the ICON templates. Plan step: "Units 02-05".

## Scope
- IN: nodes `mods/<name>` for:bitshifter, corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges, just-fall-already.
- Read each mod's source, README, mod.yaml / mod_info.yaml, strings and tests to learn what it does.
- entities.md: the mod's real things — buildings, components, Harmony patch targets in the game (classes and methods patched), options/settings, saved state — each with its file location and relationships; business rules = the gameplay rules the mod enforces; key code paths with file references; state transitions only if the mod has states (otherwise cut that section). Title it after the mod's subject (e.g. "Jumper Bridges domain").
- glossary.md: ONI and mod-specific terms a newcomer needs (game terms the mod relies on, abbreviations, terms to avoid if the code or strings show one), scoped to this mod.
- Each mod already has `domains/README.md`; do not touch it.
- OUT: other mods, root, mods/, common/; standards; applying.
- Acceptance: one file per mod at `.context/tasks/OM-001-populate-context/changesets/domains-<mod>.json`, 2 create ops each, each accepted by `write plan` (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-001-populate-context/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-001-populate-context/units/03-implement-domains-report.md
- Must contain: per mod, one line on what it does and what the two documents cover; TODO(owner)s left; `write plan` summary per file; friction met.
- Model tier: default
