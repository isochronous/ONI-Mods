# Retrospectives

Newest first. Each entry is a `### ` block naming the task it came from, appended by the write
path — `learned:` on the change-set that produced the lesson, never a closing ceremony.


### OM-001: seed ICON's content documents across the root and all 17 mods
**Id:** blk-c3ff9a6f1225

- **Avoid**: Leaving the coverage shape to each drafting unit. Unit 03 gave its six entities.md no applies_to while units 04 and 05 used src/<Project>/**, and no unit covered glossary.md, so the review had to raise it and a second round of 18 change-sets (with a witness value nobody had chosen) fixed it. A spec that fans out the same document type across many nodes names the applies_to and witness values up front. Also: the review flagged testbed-base.sav as missing because it looked only in this repository; fixture saves live in the common submodule, which a reviewer of mod or testing documents has to check too.
- **Repeat**: Units draft one change-set per node into the task folder and prove it with write plan; the manager applies them one at a time. 55 change-sets landed with no refusal and no apply racing another, and every unit could run in parallel because none of them wrote the store.
- **Updated**: domains/tooling-friction.md gains two entries: the handoff brief filled with near-identical sections of the shared standards body, and the cover operation's refusal not naming witness while refusing a section and a cover on one document in one change-set. The testing documents already say where the fixture saves are tracked, so nothing else was promoted.
