# Handoff — OM-002-store-backfills

**Scope:** `.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/09-research-vocab.md`

```
# Unit 09 — research: propose the tag root node vocabulary

## Objective
Propose the tag vocabulary for the root node `.` (13 documents: overview, tooling-friction log, the ICON task-plan templates, branching, commit conventions, CI/CD, unit and integration testing). Phase D, tag-backfill step 2. The manager checks it and mints the dictionaries; you write nothing to the store.

## Scope
- IN: read `work/tags/lexicon-root.json` (paths relative to `.context/tasks/OM-002-store-backfills/`) and the documents they list (under the node's `.context/`).
- OUT: writing anything except your output and report; committing.

## Rules (ICON tag-backfill step 2; follow exactly)
- Propose from the lexicon file(s) named below (each document's title, description and section titles), then extend from the bodies of a sample of the documents (read them from disk) for terms that live only in bodies.
- **Admission floor:** a term must plausibly gather at least 3 records (documents or sections) across at least 2 distinct documents of THE SAME NODE. Count it per node; state the estimate.
- **Bar** a term that would reach more than about 85% of that node's records: it selects nothing.
- **Bar a location rather than a subject.** A term whose honest meaning is "records about this directory" is a path, not a subject. A folder-like word is fine when it names a shared concern.
- **Bar shape terms:** a term that would fit any document of that shape about anything (e.g. "index", "overview", "examples") is not a subject.
- **Gloss:** exactly one sentence saying what a record must be about to earn the term, starting "A record earns this term when it is about". No boundary clause, no "not X", no pointer to another term, no list of sub-topics.
- Terms: lower-case, hyphenated noun phrases, no commas. Somebody starting work would ask for it by name.
- Read every gloss you have written before adding another: no plurals, short forms or near-synonyms of an existing term.
- Report anything in these rules you find to be wrong.

## Output
- `work/tags/vocab-root.json`: `{"node": ".", "terms": [{"term": "...", "gloss": "...", "est_records": N, "est_documents": N, "evidence": ["<record ids>"]}]}`. Write with a script or the Write tool; validate it parses.

## Report
- File: .context/tasks/OM-002-store-backfills/units/09-research-vocab-report.md
- Must contain: term counts per node, the closest calls you dropped and why, anything in the rules you found wrong, friction met.
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

**Coverage.** nothing in this store declares coverage of these targets, no record names one of them in its own prose, and no target's own bytes cite one, so nothing matched them BY TARGET. What matched, matched by NODE — the records and retrospective entries that sit in the nodes these targets are inside — so the same list comes back for any path inside those nodes. It is still what there is; it is not a selection for the paths you asked about.

**How that is fixed:** a record claims the paths it governs by declaring `applies_to:` in its frontmatter, and a `##`-or-deeper heading block may declare its own `**Applies to:**` — a block that does gets its own pointer, at the block rather than at the whole document.

blk-c3ff9a6f1225 · lesson
  seed ICON's content documents across the root and all 17 mods
  .context/retrospectives/shared.md · unpruned lesson in the governing node
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 5 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: domains/tooling-friction.md, workflows/task-plan/base.md, workflows/task-plan/phase-implementation.md, workflows/task-plan/phase-investigation.md, workflows/task-plan/phase-testing.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
