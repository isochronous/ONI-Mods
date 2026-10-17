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
