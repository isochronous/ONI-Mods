# Unit 11 — research: normalise the round-one tag gaps into new terms (tag-backfill step 6)

## Objective
Turn the round-one `no-good-match` suggestions of the 17 mod nodes into a small set of new dictionary terms per node, or into a recorded "no term" for each cluster. The manager checks and mints them; a second tagging round then re-tags only records that still carry no tag.

## Inputs (paths relative to this task folder)
- `work/tags/gaps-<mod>-r1.json` for the 17 mods: each gap's record id and suggestion.
- `work/tags/vocab-mods.json`: the round-one vocabulary per node (already minted as `mods/<mod>/.context/tags.md`).
- The records themselves: read them in `mods/<mod>/.context/` to see what a suggestion's records actually hold.

## Rules (ICON tag-backfill step 6; follow exactly)
- A suggestion is an honest report that the vocabulary missed a record, not a term waiting to be minted. Normalise: suggestions collapse heavily.
- Triage out, and record why: a suggestion naming a document's SHAPE rather than its subject (index of documents, cross-references to siblings, a glossary's abbreviations list, "Related" links); and records that hold source paths beside purposes (that is coverage, not a tag).
- **Admission floor, per node, unchanged:** a new term must gather at least 3 records across at least 2 distinct documents OF THAT NODE. Count records that would honestly earn it; untagged records are the ones round two asks about, so say how many of the 3+ are currently untagged.
- Bar a term reaching more than about 85% of a node's records; bar location terms.
- **One spelling, one gloss** for a concept across every node that admits it. An existing term (e.g. `mod-compatibility`, `exception-handling`) that fits a gap may be admitted to a node that lacks it, with its existing gloss verbatim.
- Gloss: one sentence starting "A record earns this term when it is about", positive rule only, no list of sub-topics, no boundary clause. A gloss that has to enumerate (formatting AND imports AND comments) is too coarse: either find the single concept or split.
- The round-one gaps are dominated by the shared standards body: code style (formatting, using order, comments, control flow, toolchain), naming (files, identifiers, string keys), try/catch rules, null guards, yielding to other mods. Decide honestly which of these are real subjects someone would ask for by name and which meet the floor in each node.
- A large share of suggestions should resolve to no term. Report the residue with reasons; do not chase 100%.
- Report anything in these rules you find wrong.

## Output
- `work/tags/vocab-gap.json`: `{"<node e.g. mods/bitshifter>": [{"term": "...", "gloss": "...", "new_or_existing": "new|existing", "est_records": N, "est_untagged": N, "est_documents": N, "evidence": ["<record ids>"]}], ...}`, all 17 nodes as keys (an empty list is valid).
- Write with a script or the Write tool; validate it parses.

## Report
- File: units/11-research-gap-terms-report.md
- Must contain: the clusters found with their dispositions (term / triaged out / no term, and why), terms per node, anything wrong in the rules, friction.
- Model tier: complex
