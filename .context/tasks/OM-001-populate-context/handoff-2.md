# Handoff — OM-001-populate-context

**Scope:** `mods`, `common`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-001-populate-context/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-001-populate-context/units/02-implement-standards.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:overview.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: overview.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
