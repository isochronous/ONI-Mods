# Unit 02 report: Phase A, mods group A

Six change-sets written to `.context/tasks/OM-002-store-backfills/changesets/phaseA-<mod>.json` (generator: `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/gen_phaseA2.py`). Each has four section ops: overview.md (What this is, Where things are), domains/README.md (Domains), standards/README.md (Standards). Nothing applied, nothing committed.

## Per node
- bitshifter: ribbon reader/writer 31-bit selection, display, NOT gate, Digital CPU and patch-conflict stand-down; v1.2.0, build 744825; folders src/Bitshifter, Tests, tools/make_art.py, publish; domains and standards indexed.
- corner-build-anything: diagonal-gap build/deconstruct via reach-table swap; v0.1.0, changelog Unreleased, no workshop-id; src, Tests, publish; indexed.
- detect-any-element: Any choice on five element sensors, asterisk art, ko/ru/zh translations; v0.2.1; src, Tests, tools, preview, publish; indexed.
- dont-count-eggs-on-rails: rail eggs excluded from critter sensor counts via OvercrowdingMonitor.UpdateRegion; v0.1.0; src, Tests, preview, publish; indexed.
- jumper-bridges: bridge ends on automation/power ports, end nodes, link rings, options, translations; v0.2.0; src (key classes listed), Tests, tools/make_preview.py, publish; indexed.
- just-fall-already: door-armed cells and immediate critter fall; works with Critters Fall Through Open Doors; v0.1.1; src, Tests, publish; indexed.

## TODO(owner) left
None. Where the repo gave no answer I left the fact out (no hedging text): no release status is stated for corner-build-anything beyond "changelog lists it as Unreleased, no Workshop id".

## `write plan` summary (all six accepted, "plan only", from C:/dev/ONI-Mods, --at .)
Each file reported: overview.md What this is updated; Where things are updated; domains/README.md Domains updated; standards/README.md Standards updated. Bytes added per file (What this is / Where things are / Domains / Standards):
- bitshifter: +693 / +941 / +223 / +422
- corner-build-anything: +478 / +677 / +207 / +422
- detect-any-element: +574 / +1025 / +212 / +422
- dont-count-eggs-on-rails: +387 / +742 / +196 / +422
- jumper-bridges: +691 / +1177 / +205 / +422
- just-fall-already: +520 / +657 / +204 / +422
Only notice: "no base digest given for ... planned against it as it is now" for the three documents (expected, none supplied).

## Brief feedback (handoff section 2)
- Nothing shown turned out not to apply; the pointers were all other mods' standards sections and mod entity docs, mostly not needed for index work.
- Governing record missing: none seen.
- Pointer reasons matched.

## Friction
- Envelope key for operations is `ops`, not `operations`; `ks write schema` (no args) printed the envelope header but I only learned the key by reading phaseA-mods.json. First `write plan` was not run with the wrong key (caught before), so no failure, but the schema command head output did not show it within the first 30 lines.
- Auto-mode system text asked for Bash over dedicated tools; I used Write for the script and JSON generation per common-rules.
