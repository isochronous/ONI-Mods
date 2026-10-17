## Task: OM-002
## Branch: OM-002-store-backfills (stacked on OM-001-populate-context, PR #6)
## Objective: Bring the store from scaffold to described, covered and tagged: write the placeholder overviews and README indexes, then run the description, coverage and tag backfills over every node.
## Folder: .context/tasks/OM-002-store-backfills/

## Phase State
- Phase plan: [implementation, completion]
- implementation: in progress
- completion: pending

## Decisions
- Order is forced: overviews/READMEs first (the backfills would otherwise describe placeholders), then descriptions (the coverage survey's "subject elsewhere" gate and the tag survey both read descriptions), then coverage, then tags.
- Overview and README content is drafted by units as change-sets in this task folder; the manager plans and applies them one at a time (same pattern as OM-001).
- META.md Layout/Discipline placeholders stay: META.md belongs to the descriptor writer and was not part of the request.
- common/ is out of this task: it is a git submodule (oni-mods-common) and its own store, so its writes land in another repository.
- The description backfill re-authors every document, including the 96 OM-001 wrote; that is the skill's design (no already-described skip), accepted.
- Tags: one dictionary per node, because a node answers for its own vocabulary (tag-backfill, "What This Does Not Do").

## Key Files
- units/: specs and reports; changesets/: drafted change-sets; work/: survey, answers and journal files for the backfills

## Progress
- [ ] Phase A: overview.md for root, mods/ and 17 mods; domains/README.md and standards/README.md indexes for 17 mods ← IN PROGRESS
- [ ] Phase B: description backfill over the root store
- [ ] Phase C: coverage backfill, node by node (19 nodes)
- [ ] Phase D: tag backfill, node by node
- [ ] `ks check --all --strict`, `ks sweep`, review, retrospective, commit, PR

## Open Questions / Blockers
- None yet.

## Constraints
- Every store change goes through the write path; never hand-edit .context/ outside tasks/.
- No hedging language in documents; TODO(owner) where the repo gives no answer.
- Commit messages: one terse line.
