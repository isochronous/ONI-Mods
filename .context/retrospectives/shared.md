# Retrospectives

Newest first. Each entry is a `### ` block naming the task it came from, appended by the write
path — `learned:` on the change-set that produced the lesson, never a closing ceremony.


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
