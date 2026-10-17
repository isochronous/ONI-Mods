# Unit 11 report: round-one tag gaps normalised into terms

Output: `work/tags/vocab-gap.json`. It parses, has all 17 nodes as keys, and every entry meets the floor (3 or more records in 2 or more documents of its node). The largest reach is 25% of a node. Scripts are in the session scratchpad (`u11-gapterms/`), outside the repo.

Inputs read: the 17 `gaps-<mod>-r1.json` files (449 `no-good-match` suggestions), `vocab-mods.json`, the round-one `tagout-*` answers (for the current tags), and every record body of the 8 surveyed documents per node, read from disk. `est_untagged` counts records that declare no tag of their own in round one's answers.

## Terms per node

| Node | Term | New / existing | Records | Untagged | Docs |
|---|---|---|---|---|---|
| all 17 | `code-style` | new | 21 | 13–18 | 2 |
| bitshifter | `bit-display` | new | 4 | 1 | 3 |
| detect-any-element | `reflection` | existing | 4 | 1 | 3 |
| dont-count-eggs-on-rails | `room-tracking` | new | 6 | 5 | 3 |
| multichannel-critter-sensor | `pulse-output` | new | 7 | 3 | 3 |
| refined-building | `vanilla-fallback` | existing | 9 | 8 | 2 |
| upgrade-planner | `building-family` | new | 4 | 3 | 2 |
| upgrade-planner | `planner-window` | new | 3 | 2 | 2 |

The other 11 nodes get only `code-style`.

Glosses for the new terms:
- `code-style`: A record earns this term when it is about the house style in which a mod's C# source code is written.
- `bit-display`: A record earns this term when it is about the digit display and lights on which the Ribbon Reader and Writer show their selected bit.
- `room-tracking`: A record earns this term when it is about how the game records which room a critter or egg is in.
- `pulse-output`: A record earns this term when it is about the one-tick pulses the sensor sends when its counted total rises or falls.
- `building-family`: A record earns this term when it is about a building type as the build menu lists it, with the per-material defs that may stand behind it.
- `planner-window`: A record earns this term when it is about the Upgrade Planner's dialog window in which the player sets up a plan.

`reflection` and `vanilla-fallback` keep their existing glosses word for word.

**Close calls the manager may drop:** `planner-window` and `building-family` (upgrade-planner), and `bit-display` (bitshifter). Each has two records about nothing else, plus one or two summary records with a single sentence or bullet on the concept. They meet the floor only under the counting rule described under "Rules" below. `bit-display` gives round two just 1 untagged record. `vanilla-fallback` in refined-building gets its second document from one bullet in `code-style.md#harmony-patches` ("transpilers ... report when it is not found"). Round one tagged that same section `vanilla-fallback` in detect-any-element, so the count is consistent with round one.

## Clusters and dispositions (449 gaps)

| Gaps | Disposition |
|---|---|
| 262 | **term `code-style`**: code-style.md and naming-conventions.md (the documents and their sections on formatting, line length, braces, using order, comments, conditionals, loops, strings, file, identifier, boolean, ModStrings-key and id naming) |
| 66 | **triaged out: empty wrapper headings.** The heading has no text of its own (`#formatting`, `#imports-includes`, `#comments`, `#control-structures`, `#files-and-directories`, `#code-naming`, `#project-specific-conventions`, `#project-specific-patterns`, `#anti-patterns-to-avoid`). They should have been `inherits`; once `code-style` is a document tag they inherit it. This happened in 9 nodes, which means the chunks disagreed with each other. In the other 8 nodes the same headings were answered `inherits`. |
| 24 | **triaged out: shape (folder index)**: `standards/README.md`, `domains/README.md` and their `#standards`/`#domains` sections ("coding standards index", "index of a folder's documents") |
| 11 | **triaged out: shape (cross-references)**: `#related` sections |
| 11 | **triaged out: shape (glossary scaffolding)**: `glossary#abbreviations-and-acronyms`, `glossary#terms-to-avoid` |
| 1 | **triaged out: coverage**: motion-sensor-range `overview.md#where-things-are` lists source paths with their purposes. That calls for `applies_to`, not a tag. |
| 17 | **no term: language and toolchain.** Root's `mod-build` fits in meaning, but in every node the only record about it is `code-style.md#language-and-toolchain` (one document), so it fails the floor. It inherits `code-style` from its document, which is a loose fit. |
| 9 | **no term: try/catch** (corner-build, dont-count-eggs, multichannel, no-magic-fridges, refined-building). `exception-handling` exists in natural-backwalls and upgrade-planner. In these 5 nodes every record about catching is in error-handling.md (`#try-catch`, finalizer, swallowing, throwing), so the term fails the 2-document floor. |
| 6 | **no term: yielding to other mods** (corner-build, dont-count-eggs, motion-sensor, multichannel, no-magic-fridges, refined-building). `mod-compatibility` would fit, but `#yielding-to-other-mods` and `#try-catch` item 1 are both in error-handling.md. The only other mentions are passing ones: a comment rule naming the other mod's Workshop id, and an id-reuse rule. These mods have no compatibility behaviour of their own. |
| 7 | **no term: game components** (corner-build, dont-count-eggs, just-fall, natural-backwalls, refined-building, salty-surf, upgrade-planner). `game-component` covers a component the mod adds. None of these mods adds one: the only records are `code-style#game-components` and one row in the naming table. |
| 2 | **no term: options umbrella** (`#project-specific-rules` in corner-build and no-magic-fridges, suggested as "mod options classes"). Neither mod has options, so `mod-options` would mislead. The section's load line and tests are already covered by `logging` and `in-game-testing` in the other nodes. |
| 13 | **no new term: a term already in the node's vocabulary fits**, and round two re-asks these records anyway. They are the guarding sections in corner-build, dont-count-eggs and multichannel (`vanilla-fallback` is in their vocab), and the `error-handling.md` document in jumper-bridges, motion-sensor, salty-surf and vent-freeze-fix (`vanilla-fallback` or `logging`). |
| 8 | **no term: domain residue.** A tag on the document already covers each one, or it has a single record. Steadiness window is covered by `element-sensor` (how the sensor decides). Scene partitioner is covered by `detection-range`. Band table and feature room are covered by `worldgen`, and band table also fits `noise-pattern`. The jumper-bridges overlay fits `utility-network`, which is in that vocab. Plan state and matcher are covered by `building-replacement`/`building-materials`, and the plan is also confined to entities.md. Instant build has one record. |
| 12 | **terms** from the node-specific clusters in the table above (`room-tracking` 3, `pulse-output` 3, `vanilla-fallback` 2, `bit-display` 1, `reflection` 1, `building-family` 1, `planner-window` 1) |

Concepts the spec named:
- **Formatting, using order, comments and control flow** are not separate subjects. Each lives in one document, so none passes the floor alone. Together they are code style, and people ask for "code style" by name.
- **Naming** (files, identifiers, string keys) is folded into `code-style`. In every node the only naming records are in naming-conventions.md, so a separate naming term fails the 2-document floor everywhere.
- **Try/catch, null guards and yielding** are covered under the floor failures and existing terms above.
- **ModStrings.** The 6 nodes without `mod-strings` (bitshifter, corner-build, dont-count-eggs, just-fall, no-magic-fridges, refined-building) are exactly the mods with no `ModStrings.cs`, and their entities documents say "no strings". The string-key and string-building sections therefore go to `code-style`, not `mod-strings`.

## Rules: what I found wrong or unclear

1. **The per-node 2-document clause meets a shared standards body that is copied 17 times.** That body puts each concept in exactly one document, so whether a node admits a term depends on whether that mod's own domain docs happen to restate the concept. The result is that the identical `error-handling.md#try-catch` section carries `exception-handling` in 2 nodes and nothing in 15, and `--tag exception-handling` misses most copies. The same applies to `mod-compatibility`, `game-component` and `vanilla-fallback`. I don't think the floor is wrong. The duplication is: a single canon copy of the shared standards above the mod nodes would be tagged once.
2. **"Count records that would honestly earn it" does not say whether a single bullet in a summary section counts.** Round one assigned terms on one bullet (for example `copy-settings` on multichannel's `#business-rules`). I counted the same way: a sentence or bullet about the concept counts, a passing mention or a path does not. The three close calls above depend on this choice. Under a "main subject only" reading they fail.
3. **Round-one taggers filed empty wrapper headings and "an existing document tag covers this" as `no-good-match`** (74 or more of 449). The tagging brief already rules this out, yet 9 of 17 nodes did it. Round two will ask about them again, because they are untagged.

## Friction (for the coordinator to record)

- The handoff brief spent most of its 8 KB budget on 17 copies of the same `#try-catch` section, one pointer per node. Grouping pointers whose content is identical across nodes would have left room for other records.
- The gap files carry a `kept` field that is always empty, with no explanation of what it is for.

## Done / not done

Done: every gap classified, `vocab-gap.json` written and validated, and this report. Not done, by design: minting and the round-two tagging, which are the manager's.
