# Retrospectives

Newest first. Each entry is a `### ` block naming the task it came from, appended by the write
path — `learned:` on the change-set that produced the lesson, never a closing ceremony.


### OM-009: Jumper Bridges heavy-watt joint plates
**Id:** blk-2c58c8c2a081

- **Avoid**: The task was framed as a long-span bridge with a solid middle; Jumper Bridges never changes a bridge's span, so the plate keeps its one vanilla solid cell and the real work was at the end cells. Read the mod's overview record (`blk-2d2edc3c3db1`) before writing an explore spec's questions, so the spec does not carry a wrong premise. Implement units also widened scope (tooltip rewording, self-written zh/ko/ru lines, an extra crash guard); a spec should say whether player-visible strings may change.
- **Repeat**: Explore and review units decompiled the vanilla methods (`ilspycmd -t <Type>` against the game's Assembly-CSharp.dll) and checked each patch against them; that caught the `BuildingDef.MarkArea` prefix that would have stripped the plate's footprint marks, a real pre-existing NullReferenceException in `IsValidTileLocation` on bridge end node cells, and a tooltip that overclaimed what may share a cell. Running the whole mod suite after a change to shared code (20/20 in one 40 s session) confirmed the wire bridges were untouched.
- **Updated**: mods/jumper-bridges `domains/entities.md` (BridgeKind, Harmony patches, Business Rules, Key Code Paths), `domains/glossary.md` (Joint plate), `overview.md` (What this is) now describe the plates as Power-kind jumper bridges with a solid centre; root `domains/tooling-friction.md` records the brief crowding by other mods' code-style sections.


### OM-006: Ovagro Overhead, a new mod that plants Ovagro on ceilings
**Id:** blk-e6b85866c4d7

- **Avoid**: Designing around facts nobody checked against the user's words or the game. The plan said Duplicants plant in natural tiles when the user had written Pips; unit 01 read only the Pip's spawn nav type and concluded Pips walk floors, when their nav grid includes walls and ceilings; the user caught both. A floating sandstone strip in a test box could never be reached by a Pip spawned elsewhere, which the user also caught. Separately, the manager logged its own ungrouped `--tag 'a && b' 'c'` as engine friction without reading `ks brief --help`, which documents the grouping, and had to correct two entries. Read the user's sentence for who does what, check a unit's game-code reading against how the game plays before building on it, and read a verb's --help before calling it broken.
- **Repeat**: Checking in game what removing the mod does before writing a removal claim: an Explicit test saved ceiling Ovagro in every state, a KeepSave probe in another mod's suite loaded that save with the mod off, and it showed vanilla vines left behind by the dropped hanging plant throwing every tick, which the design had missed; giving the ceiling vines their own prefab made removal clean (two prefab warnings, no errors). When a wait stalls, logging the object's state every few seconds and screenshotting with the HUD on (status icons) found the cause in one run: a Hydroponic Farm with no pipe reports NeedLiquidIn and its plant wilts. Measuring screenshot crops and title spacing by pixels, not by camera or font metrics, got the preview placement right.
- **Updated**: testing/integration-testing.md gains "Checking what removing a mod leaves in a save" (Explicit save test, -Fixture, a KeepSave probe in another mod's suite) and "Plant scenes" (a Hydroponic Farm needs a pipe; the Ovagro Node has no maturity amount; a thin warm fill in the vacuum test map; Pips cannot cross a gap; stalled waits log state and screenshot). mods:domains/game-interfaces.md (new) records the net48 rule for entity configs and why the game can use default interface methods. mods/ovagro-overhead:decisions/001 files the ceiling form design (hidden plant and vine pair, routed vanilla seed, all-or-nothing patching), under the node's first tag term, placeholder, in a new tags.md. In common, TestRunner's domains/entities.md and standards/naming-conventions.md now list the KeepSave option.


### OM-002: overviews, then description, coverage and tag backfills over every node
**Id:** blk-bbb71786a11b

- **Avoid**: Tagging 17 nodes in independent passes over one standards body copied into each of them. Identical sections came back with different tags from mod to mod (34 of 56 shared sections), round two then put terms on documents and parent headings whose sections already carried them (about 90 nested-duplicate advisories), and the per-node two-document floor admitted a term like exception-handling in 2 nodes and not 15 for the same text. When a body is shared across nodes, choose its tags once per section and stamp them into every copy, and give the tagging passes the round-one tags of a document before asking for its document tag.
- **Repeat**: Freezing each node's survey and refusing a write when the node changed since; opening each tagging pass's partial file before dispatch and closing it only on the harness's completion notice; persisting every round before applying it. 18 dictionaries, two tagging rounds and every correction landed with no refusal and nothing to replay. Splitting description authoring across four units and merging into one bound answers file also ran clean: 150 distinct descriptions, 0 declined.
- **Updated**: domains/tooling-friction.md gains two entries: tag-backfill shipping a library and no runnable command, and coverage-backfill having no door for practice documents. Nothing else was promoted: the tagging lessons concern ICON's bulk tools rather than this repository's code.


### OM-001: seed ICON's content documents across the root and all 17 mods
**Id:** blk-c3ff9a6f1225

- **Avoid**: Leaving the coverage shape to each drafting unit. Unit 03 gave its six entities.md no applies_to while units 04 and 05 used src/<Project>/**, and no unit covered glossary.md, so the review had to raise it and a second round of 18 change-sets (with a witness value nobody had chosen) fixed it. A spec that fans out the same document type across many nodes names the applies_to and witness values up front. Also: the review flagged testbed-base.sav as missing because it looked only in this repository; fixture saves live in the common submodule, which a reviewer of mod or testing documents has to check too.
- **Repeat**: Units draft one change-set per node into the task folder and prove it with write plan; the manager applies them one at a time. 55 change-sets landed with no refusal and no apply racing another, and every unit could run in parallel because none of them wrote the store.
- **Updated**: domains/tooling-friction.md gains two entries: the handoff brief filled with near-identical sections of the shared standards body, and the cover operation's refusal not naming witness while refusing a section and a cover on one document in one change-set. The testing documents already say where the fixture saves are tracked, so nothing else was promoted.
