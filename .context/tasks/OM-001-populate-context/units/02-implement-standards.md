# Unit 02 — implement: draft standards change-sets for all 17 mod leaves

## Objective
Draft one change-set per mod leaf creating `standards/code-style.md`, `standards/error-handling.md`, `standards/naming-conventions.md`, adapted from the ICON templates. Plan step: "Units 02-05".

## Scope
- IN: the 17 nodes `mods/<name>` for: bitshifter, corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges, just-fall-already, motion-sensor-range, multichannel-critter-sensor, natural-backwalls-everywhere, no-magic-fridges, refined-building, salty-surf, smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix.
- The mods share one toolchain (`common/oni-mod.props`, `oni-mod.targets`, `.editorconfig` if any) and largely one style. Establish the shared conventions once from the code (C# version, Harmony patch class layout, PLib use, STRINGS / ModStrings, logging, null handling in patches, namespaces, file layout). Then give each mod its own copy with a short mod-specific section only where that mod genuinely differs (e.g. its namespace, its patch targets, its options class). A script that composes per-mod JSON from a shared body is fine.
- Error handling must reflect how these mods actually guard Harmony patches and log (e.g. `Debug.LogWarning` / PUtil logging, try/catch in patches), with real file references.
- Each mod already has `standards/README.md`; do not touch it.
- OUT: root, mods/, common/ nodes; domains; applying.
- Acceptance: 17 files `.context/tasks/OM-001-populate-context/changesets/standards-<mod>.json`, 3 create ops each, each accepted by `write plan` (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-001-populate-context/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-001-populate-context/units/02-implement-standards-report.md
- Must contain: the shared conventions found with file references, per-mod differences captured, TODO(owner)s left, `write plan` summary per file, friction met.
- Model tier: default
