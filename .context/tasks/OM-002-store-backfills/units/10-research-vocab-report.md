# Unit 10 report: tag vocabularies for the 17 mod nodes

Output: `work/tags/vocab-mods.json`. All 17 nodes are keys. It holds 199 term entries and 56 distinct terms. The file parses. A generator script checked every entry: each evidence id exists in that node's lexicon, every term meets the floor (at least 3 records in at least 2 documents of the same node), no term passes 85% of its node's records, and a term shared by several nodes has the same gloss in each.

## Method

- I read all 17 lexicons. The three standards documents have identical section trees in all 17 nodes (one hash matched across all of them), so I read their bodies once in full (bitshifter). For every node I then read the overview "What this is", the whole entities document, the three mod-specific sections in the standards, and the glossary definitions.
- I built the shared core first (one spelling, one gloss), then admitted each core term node by node, then added each mod's own terms.
- **Extra admission condition** (see "Rules found wrong", point 1): the standards text is the same in every node, so I admitted a core term only where the node's own records discuss it. Those are its entities, glossary, overview or the `*-specifics` sections of its standards. The shared standards text alone was not enough.
- Two glosses match the root dictionary that is already minted (`.context/tags.md`), word for word: `in-game-testing` and `mod-compatibility`. A term should not mean two things in two nodes of the same store.

## Term counts per node

| Node | Terms | Records in node |
|---|---|---|
| mods/bitshifter | 13 | 91 |
| mods/corner-build-anything | 7 | 89 |
| mods/detect-any-element | 12 | 94 |
| mods/dont-count-eggs-on-rails | 8 | 85 |
| mods/jumper-bridges | 13 | 97 |
| mods/just-fall-already | 9 | 88 |
| mods/motion-sensor-range | 13 | 90 |
| mods/multichannel-critter-sensor | 16 | 95 |
| mods/natural-backwalls-everywhere | 15 | 103 |
| mods/no-magic-fridges | 9 | 91 |
| mods/refined-building | 6 | 86 |
| mods/salty-surf | 10 | 89 |
| mods/smart-weight-plate | 15 | 89 |
| mods/storage-pod-redux | 15 | 92 |
| mods/sweep-zones | 11 | 90 |
| mods/upgrade-planner | 15 | 99 |
| mods/vent-freeze-fix | 12 | 93 |

The widest term is `harmony-patching` at about 13 records, roughly 14% of a node, so nothing comes near the 85% bar.

**Shared core and how many nodes admit each term:**
- harmony-patching, logging, in-game-testing, mod-entry-point: all 17 nodes.
- vanilla-fallback: 15 nodes (all except refined-building and vent-freeze-fix).
- mod-strings: 11 nodes.
- saved-state: 8 nodes.
- game-component: 7 nodes.
- mod-compatibility: 6 nodes.
- reflection: 6 nodes.
- side-screen: 5 nodes.
- kanim-art: 5 nodes.
- game-tag: 4 nodes.
- mod-options: 4 nodes.
- transpiler: 3 nodes.
- automation-ribbon: 3 nodes.
- building-config: 3 nodes.
- building-unlock: 3 nodes.
- copy-settings: 2 nodes.
- critter-sensor, signal-threshold, refrigerator, building-materials, toolbar-tool, hotkey, exception-handling, performance, developer-tooling: 2 nodes each.

**Each mod's own terms:**
- bitshifter: bit-depth, not-gate
- corner-build-anything: reach-table
- detect-any-element: element-sensor
- dont-count-eggs-on-rails: conveyor-rail
- jumper-bridges: bridge-end, utility-network, wire-tools
- just-fall-already: critter-falling, navigation-grid
- motion-sensor-range: detection-range, under-construction-building
- natural-backwalls-everywhere: worldgen, biome-group, noise-pattern, settle-sim
- no-magic-fridges: heat-exchange
- salty-surf: mechanical-surfboard, info-panel-text
- smart-weight-plate: weight-sensing
- storage-pod-redux: building-registration, storage-building
- sweep-zones: cleaning-errands
- upgrade-planner: building-replacement, plan-presets
- vent-freeze-fix: liquid-vent, liquid-freezing, tile-repair

Two nodes go over the "roughly 6–15" range:
- multichannel-critter-sensor has 16. Every term there has real evidence in the mod's own documents.
- refined-building has 6, because the mod is a single postfix.

## Closest calls dropped, and why

- **vanilla-fallback in refined-building and vent-freeze-fix.** In refined-building the only record of its own is "skips ineligible elements", which is guarding, not falling back to vanilla. In vent-freeze-fix it is null checks only. Both meet the floor from the standards text alone, which is the case point 1 below rules out.
- **saved-state in salty-surf and natural-backwalls-everywhere.** Both have a "Saved state" section, and both sections say "none of the mod's own". A record whose answer is "nothing" is not about the subject. salty-surf was the closest: its section does say tank contents survive a load, and `Persistence.cs` exists.
- **saved-state in jumper-bridges, no-magic-fridges, dont-count-eggs-on-rails and upgrade-planner.** In those mods nothing is saved, or the data lives in a config file. upgrade-planner's `presets.json` got `plan-presets` instead.
- **copy-settings in bitshifter, detect-any-element, smart-weight-plate and storage-pod-redux.** Each has only 2 records of its own (a member table line plus a business rule). The term was kept where the copy-settings tool has its own glossary entry: motion-sensor-range and multichannel-critter-sensor.
- **reflection in detect-any-element, corner-build-anything and bitshifter.**
  - detect-any-element: one record (`Switches`, a delegate for a protected method).
  - corner-build-anything: `FieldInfo` lookups of fields that are probably public.
  - bitshifter: the reflection there is mod detection, which `mod-compatibility` already covers.
- **game-tag in multichannel-critter-sensor and storage-pod-redux.** The prefab tag and the kitchen-refrigerator tag each have one glossary entry plus a passing mention. Dropping them also brought multichannel down from 18 terms.
- **exception-handling in storage-pod-redux, smart-weight-plate and sweep-zones.** Each has one try/catch, described only in its error-handling specifics section. It was admitted only where the mod's entities document describes catching: natural-backwalls-everywhere (the worldgen thread) and upgrade-planner (the placement queue).
- **mod-compatibility in sweep-zones, no-magic-fridges and refined-building.**
  - sweep-zones: one business rule ("overlay-adding mods are unaffected").
  - no-magic-fridges: "modded fridges" is about coverage, not coexistence.
  - refined-building: the duplicate Workshop mod is the reason it is retired, not a compatibility subject.
- **plib, as a core term.** It recurs (side screens, options, hotkeys, upgrade-planner's vendored PLib) and the owner works on PLib. In practice it would tag nearly every UI and options record that the narrower terms already cover. Worth minting later if a gap round asks for it.
- **room or cavity tracking** in dont-count-eggs-on-rails and multichannel-critter-sensor. It fits under `critter-sensor` in both nodes, so it would be a near-synonym there.
- **corner-pocket and building-footprint** in corner-build-anything: 2 records each, both inside `reach-table`.
- **logic-signal or logic-pulse** in smart-weight-plate and multichannel-critter-sensor: it is spread thin across the signal-threshold records.
- **door** in just-fall-already: it is inside `critter-falling`.
- **bit-selection** in bitshifter: a near-synonym of `bit-depth`. I kept `bit-depth` because the glossary uses that word.
- **code formatting, comments and identifier naming**: each lives in one document (code-style or naming-conventions), so none passes the two-document floor. They would also just restate the document's title.

## Rules found wrong

1. **The floor does not hold up when 17 nodes share one standards text.** In every node, the identical code-style, error-handling and naming documents already supply 3 records in 2 documents for `transpiler`, `mod-options`, `mod-strings`, `reflection` and `saved-state`. That is true even in mods with no transpiler, no options or no strings, such as bitshifter ("No PLib, no options, no strings"). Applied as written, the floor would admit those terms in all 17 nodes, and in most of them the term would select only boilerplate. I added a condition: the node's own records must carry the subject. The rule should say this for any store whose nodes copy one template body.
2. **Shared spelling stops at the node.** The spec requires one spelling and one gloss across the mod nodes, but says nothing about the root, whose dictionary is already minted. Two terms overlap with root terms, and the manager should decide each before minting:
   - `saved-state` vs root's `save-files`. Mine means what a mod puts in a save. The root's means save files as such.
   - `mod-strings` vs root's `translations`. Mine also covers ModStrings key layout.

   If the manager wants one term each, swap in the root spelling and gloss. The evidence would carry over unchanged.
3. **The "no list of sub-topics" rule is ambiguous for two-part concepts.** `building-unlock` ("placed in the build menu and unlocked by research") and `exception-handling` ("where a mod catches exceptions and what it does once it has caught one") each read as one concept to me, but a strict reader could call either a list.
4. **The location bar sits awkwardly with overview sections.** `overview.md#where-things-are` is a location record by nature. I used it as evidence for `in-game-testing` only, because it names the tests project. A tagger may well answer `inherits` there instead.

## Friction (ICON)

- The handoff brief (`ks brief`, the packet's section 2) spent its whole 8 KB budget on 17 copies of the same standards sections ("try/catch" ×17, then "When to Comment", "Yielding to Other Mods", "Braces"…), with 479 more cut off. A brief over `mods` returns the template once per node, so for this unit it showed nothing past the first copy. I expected near-identical records across sibling nodes to be collapsed, or at least ranked as one. Instead I read the lexicons and the documents directly.
- The packet did not surface the root's minted tag dictionary (`.context/tags.md`). That record governs cross-node spelling for this unit, and the brief did not reach it at all. I found it only because `vocab-root.json` was sitting beside my output.

## Answers to the packet's questions

- **Did anything listed fail to apply?** All the listed pointers are standards sections. I read their bodies anyway as the shared core's evidence. As pointers, they gave this unit nothing past the first copy.
- **Did anything that governs this work fail to appear?** Yes: the root dictionary `.context/tags.md`, and the `tag-backfill` and `tag-discipline` skills.
- **Did any pointer's stated reason not match its record?** No. The "names <file>" reasons are accurate.
