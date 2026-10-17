# Unit 13 report: change-sets for the unit 12 review fixes

Implement unit, complex tier. I wrote only the change-sets listed below and this report. Nothing was applied and nothing was committed. The generator and survey scripts are in the session scratchpad under `u13fix/` (`survey.py`, `shared.py`, `gen.py`). They read `ks map --json --format narrative-block` and `--format narrative`, and the files themselves.

## Change-sets

All are in `.context/tasks/OM-002-store-backfills/changesets/`. Envelope: `task: "OM-002"`, one shared `why`, `node`.

| File | Ops | `write plan` |
|---|---|---|
| fix12-root.json | 6 | accepted |
| fix12-bitshifter.json | 12 | accepted |
| fix12-corner-build-anything.json | 5 | accepted |
| fix12-detect-any-element.json | 16 | accepted |
| fix12-dont-count-eggs-on-rails.json | 7 | accepted |
| fix12-jumper-bridges.json | 15 | accepted |
| fix12-just-fall-already.json | 18 | accepted |
| fix12-motion-sensor-range.json | 18 | accepted |
| fix12-multichannel-critter-sensor.json | 10 | accepted |
| fix12-natural-backwalls-everywhere.json | 22 | accepted |
| fix12-natural-backwalls-everywhere-dict.json | 1 | accepted |
| fix12-no-magic-fridges.json | 10 | accepted |
| fix12-refined-building.json | 16 | accepted |
| fix12-salty-surf.json | 27 | accepted |
| fix12-smart-weight-plate.json | 20 | accepted |
| fix12-storage-pod-redux.json | 11 | accepted |
| fix12-sweep-zones.json | 12 | accepted |
| fix12-upgrade-planner.json | 17 | accepted |
| fix12-vent-freeze-fix.json | 13 | accepted |
| fix12-vent-freeze-fix-dict.json | 1 | accepted |

Every file passed `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at .` (exit 0). The only notices were "no base digest given", so each was planned against the current file.

**Two nodes have two files each.** The spec asks for one change-set per node. The engine refused that for natural-backwalls-everywhere: a change-set that writes `tags.md` cannot also add a tag, because terms are checked against the dictionary as it was before the change-set. So the `developer-tooling` gloss writes are in `-dict` files. I split vent-freeze-fix the same way to keep the two nodes alike, although its file would have passed either way. The `-dict` files and the main files do not depend on each other, so they can be applied in any order.

## Canonical tag sets for the shared sections (F4.1, F4.2, F4.4)

The script found the shared sections by grouping each section's own text by document and heading address across the 17 mods, ignoring the Id and Tags lines. Every section below has the same text in all 17 mods. The exception is `standards/README.md#standards`, which is the same in 6 mods; I gave it the empty set in all 17 under F4.3. The domains `entities.md` and `glossary.md` sections that are shared already had no tags everywhere, so they needed no change.

How each mod's own set was worked out: take the canonical set, keep only the terms that mod's `tags.md` declares, then drop any term that the document or an enclosing heading's new set already carries. Each op adds what is missing and removes the rest. Because of the second step, a mod whose `error-handling.md` already carries `vanilla-fallback, logging` shows fewer section tags than a mod whose document carries none. The tags each section effectively has are still the canonical set, limited to what the mod declares, in every mod.

### standards/error-handling.md

| Section | Canonical set | Why |
|---|---|---|
| principles | vanilla-fallback, logging, exception-handling | The five rules: keep vanilla behaviour, log what degraded and at which level, never leave a catch empty. Reflection and patching are what gets checked, not the subject. |
| guarding-patches | harmony-patching | The heading covers all five children, and each one guards a Harmony patch. Its body is empty, so the term sits here once and not on every child. |
| …/missing-member-warn-leave-vanilla | reflection, vanilla-fallback | A null reflection lookup, then vanilla runs. |
| …/transpiler-count-the-match | transpiler, vanilla-fallback | A transpiler checks its match and returns the original code if it finds none. |
| …/missing-data-warn-continue | vanilla-fallback, logging | Warn and keep working. `building-unlock` came only from the tech example and is dropped. |
| …/null-checks-on-game-objects | vanilla-fallback | Components that are absent are skipped. |
| …/warn-once | logging | A once-flag on a warning. |
| try-catch | exception-handling, logging | Where the mods catch and what the message says. `mod-compatibility`, `worldgen` and `reflection` each matched one bullet or example and are dropped. |
| try-catch/finalizer-for-a-thread-with-no-handler | harmony-patching, exception-handling | A Harmony finalizer that reports the exception. `exception-handling` falls away wherever the parent carries it. |
| yielding-to-other-mods | mod-compatibility | Stepping aside for another mod. `building-registration`, `mod-entry-point` and `logging` are dropped. |
| logging | logging | Its two children inherit it. |
| logging/log-levels, logging/what-to-include | (none) | Covered by the parent. `performance` and `developer-tooling` came from one telemetry bullet. |
| anti-patterns-to-avoid | (none) | |
| …/swallowing-exceptions | exception-handling | An empty catch. `mod-strings` is dropped. |
| …/throwing-from-a-patch | harmony-patching | |
| …/assuming-the-game-s-internals | reflection, transpiler, harmony-patching | An unchecked FieldInfo and an unconfirmed instruction match. |
| …/silent-degradation | logging | Every skipped path logs. |
| …/spamming-the-log | logging | |
| project-specific-patterns, related | (none) | |

### standards/code-style.md

The document carries `code-style` in every mod, so that term never appears on a section.

| Section | Canonical set | Why |
|---|---|---|
| formatting (and its 3 children), language-and-toolchain, imports-includes (and 2), comments (and 3), control-structures, control-structures/conditionals, related | (none) | Pure style. The old tags (`harmony-patching` and `mod-compatibility` on when-to-comment, `logging` and `mod-strings` on imports) each came from one bullet or example. |
| control-structures/loops | performance | The first bullet is about keeping per-tick code free of LINQ and allocations. |
| control-structures/strings | mod-strings | Player-visible text lives in `ModStrings`. "Log text is built with +" is about how strings are built, not what is logged, so `logging` is dropped. |
| harmony-patches | harmony-patching | `mod-entry-point`, `transpiler`, `reflection`, `mod-options` and `vanilla-fallback` each matched one bullet. |
| game-components | game-component, saved-state | How a KMonoBehaviour is written, plus `[Serialize]` fields. `building-config` and `harmony-patching` are dropped. |
| project-specific-rules | mod-options, logging, in-game-testing | One bullet each. `mod-strings` came from a half-sentence about option titles and is dropped. |

### standards/naming-conventions.md

| Section | Canonical set | Why |
|---|---|---|
| files-and-directories, general-rules, examples-by-type | (none) | A table of file-naming rules. Each old term (`mod-entry-point`, `side-screen`, `kanim-art`, `building-config` and others) matched one row. |
| code-naming, variables, booleans, project-specific-conventions, related | (none) | |
| code-naming/functions-methods, code-naming/classes-types-interfaces | harmony-patching | How Harmony methods and patch classes are named. This kept what 15 of 17 mods already had. |
| code-naming/strings-modstrings | mod-strings | `mod-options` came from the option-key bullet and is dropped. |
| code-naming/ids-and-tags | (none) | Id and prefix rules. The review's preferred answer: `in-game-testing` matched one bullet, and `hotkey`, `game-tag`, `building-config`, `mod-compatibility` and `mod-entry-point` were example-driven. |

### standards/README.md (F4.4)

- **Document tags:** `code-style` only, in all 17. The README lists the three standards documents, and `code-style` ("the house style in which a mod's C# source code is written") covers all three. `logging`, `vanilla-fallback`, `harmony-patching` and `exception-handling` describe what the linked documents contain, not the index. 11 mods change; the 6 that already had `[code-style]` need nothing.
- **"Standards" section:** no tags, in all 17. This changes natural-backwalls-everywhere and smart-weight-plate (F4.3).

## Findings and the change-sets that resolve them

- **F1.1:** a `section` write on `overview.md` "What this is" changes "`mod.yaml` sets" to "`mod_info.yaml` sets". The rest of the body is taken byte for byte from the file. The plan shows a 1-line change of +5 bytes in each, with the Id line and tags kept. In fix12-motion-sensor-range, -multichannel-critter-sensor, -natural-backwalls-everywhere, -no-magic-fridges, -refined-building and -salty-surf.
- **F3.1:** a `section` write on `tags.md#developer-tooling` with the body "A record earns this term when it is about debug and developer features a mod includes for its author." In fix12-natural-backwalls-everywhere-dict and fix12-vent-freeze-fix-dict.
- **F3.2 / F4.3:**
  - All tags are removed from `overview.md#where-things-are` in all 17 mod files and in fix12-root.
  - `github-issues` is removed from the root `overview.md#what-this-is` (fix12-root). The review names it under F4.3: one clause about issues.
  - All tags are removed from `domains/README.md#domains` in natural-backwalls-everywhere.
  - All tags are removed from `standards/README.md#standards` in natural-backwalls-everywhere and smart-weight-plate.
  - The `mod-entry-point` term and its gloss are unchanged.
- **F4.1 / F4.2:** the shared-section tag ops in all 17 mod files, per the tables above.
- **F4.4:** the `standards/README.md` document-tag ops in bitshifter, dont-count-eggs-on-rails, just-fall-already, multichannel-critter-sensor, refined-building, salty-surf, smart-weight-plate, sweep-zones, upgrade-planner and vent-freeze-fix. upgrade-planner also loses `exception-handling` there. natural-backwalls-everywhere's README already carried `[code-style]` at document level.
- **F4.5:** `commit-messages` is removed from `workflows/task-plan/phase-architecture.md#phase-exit-handoff-run-last-at-the-phase-boundary` (fix12-root). That leaves `phase-handoff`, which matches the four sibling phases.
- **F4.6:** `common-submodule` is removed from the tooling-friction entry `2026-10-17-icon-init-exclude-does-not-drop-a-top-level-folder-from-the-node-set` (fix12-root).
- **F4.7:** fix12-root. `testing/unit-testing.md#framework-and-tools` loses `mod-build` and `translations` and keeps `game-update-compatibility`. `#test-structure/file-organization` loses `test-assertions`. I removed `translations` too, because the finding calls it misplaced (one table row) and the fix says to keep only `game-update-compatibility`.
- **F4.8:** `vanilla-fallback` is removed from `domains/entities.md#business-rules` (fix12-upgrade-planner). I did not add `building-replacement` because the document already carries it.
- **F2.1 (nit):** the schema offers `describe`, so the four `standards/naming-conventions.md` descriptions are rewritten without "`ModStrings` keys". The rest of each line is unchanged. In fix12-dont-count-eggs-on-rails, -just-fall-already, -no-magic-fridges and -refined-building. I checked first that none of the four has `ModStrings` in its `src/`.
- **One extra op I added, not named in the spec:** in salty-surf, the shared `error-handling.md#project-specific-patterns` heading carried `vanilla-fallback`. Its own body is empty, so that tag belonged to its only child, "Salty Surf specifics", whose text describes a missing element being skipped and vanilla text left alone. The canonical set for that shared parent is empty, so fix12-salty-surf removes the tag from the parent and adds it to `project-specific-patterns/salty-surf-specifics`. Without this, the specifics section would lose `vanilla-fallback` entirely, since that document carries no document tags in salty-surf.

## Unresolved, for the manager

- **mod-strings lost on six specifics sections.** Removing `mod-strings` from the shared `code-style.md#project-specific-rules` heading means these "<Mod> specifics" child sections no longer inherit it: jumper-bridges, motion-sensor-range, multichannel-critter-sensor, salty-surf, smart-weight-plate, sweep-zones. The spec says specifics keep their own tags, so I added nothing. Two of them are about `ModStrings`, though: salty-surf (`ModStrings.SALTYSURF.ELEMENT_LIST` formatting) and sweep-zones (a `ModStrings` branch alias). A `tag` add of `mod-strings` on `project-specific-rules/salty-surf-specifics` and `project-specific-rules/sweep-zones-redux-specifics` would bring them back, if that is wanted. The other four mention only `UseTranslations` and do not need it.
- **Document tags differ on the shared standards documents themselves.** The spec did not ask me to change these, and I did not:
  - `code-style.md` also carries `harmony-patching` in smart-weight-plate and upgrade-planner.
  - `error-handling.md` carries anywhere from no tags to `vanilla-fallback, logging, harmony-patching`. It has none in jumper-bridges, motion-sensor-range, salty-surf and vent-freeze-fix, and has `exception-handling` alone in natural-backwalls-everywhere.

  Under the rule "a term an ancestor carries is not repeated", the section sets above work with any document set. But in smart-weight-plate and upgrade-planner every `code-style.md` section effectively carries `harmony-patching`, and in dont-count-eggs-on-rails and refined-building every `error-handling.md` section does. A later pass could give the three shared documents one document tag set, as F4.4 does for the README.
- **No change, as the spec decided:** the rest of the nits (F1.2 to F1.5, F2.2, F3.3 to F3.6, F4.9 to F4.12) and the F3.2 gloss wording.

## Handoff questions

- **Pointers that did not apply:** all 25 listed pointers are copies of shared `standards/` sections (`try/catch`, `Yielding to Other Mods`, `Braces`, `Indentation` and others), matched because their text cites `mods/` source paths. They were relevant only as examples of the shared sections this unit retags, and I reached those through `ks map` anyway.
- **What governed the work but did not appear:** the 18 `tags.md` dictionaries (the brief itself warns they have no coverage), and the write schemas. The spec named the schemas, so they were not missed in practice.
- **Pointer reasons that did not match:** none. The reasons were accurate but did not say that the 17 records are copies of one text.

## Friction (for the coordinator to record)

- **Invocation:** `ks write plan` on a single change-set that held a `tags.md` `section` write together with `tag` adds (fix12-natural-backwalls-everywhere.json, first version). **What it did:** refused the whole change-set. The `developer-tooling` gloss rewrite neither adds nor retires a term, but because the change-set wrote `tags.md`, the engine would not admit adds in the same envelope. **Expected:** a gloss-only body change to an existing term should not block adds of other, already declared terms. **What I did instead:** moved the dictionary write into a separate `-dict` change-set, which breaks the spec's "one change-set per node".
- **Invocation:** `ks map --json --node mods/bitshifter --document standards/naming-conventions.md`. **What it did:** refused with "names no document in this store". `--document` seems to want the store-relative path (`mods/bitshifter/.context/standards/...`) even when `--node` is given, although the help says "with or without its printed `.context/` prefix, or a narrative's bare path". **What I did instead:** dumped the whole store with `--format narrative-block` and filtered it in Python.
- **Invocation:** the handoff brief. **What it did:** the same near-identical shared-section flooding the unit 12 report already describes; it recurred.
