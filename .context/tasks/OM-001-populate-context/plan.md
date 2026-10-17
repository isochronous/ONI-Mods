## Task: OM-001
## Branch: OM-001-populate-context
## Objective: Seed the scaffolded .context/ tree with ICON's content documents (populate-context skill), each adapted to ONI-Mods and landed through the write path.
## Folder: .context/tasks/OM-001-populate-context/

## Phase State
- Phase plan: [implementation, completion]
- implementation: in progress
- completion: pending

## Decisions
- Task-plan templates (base + five phases) land at the root node only: one git repository, and the manager reads them from the root.
- Workflows (branching, commit conventions, CI/CD) land at the root only: one repository, one history, one release flow.
- Standards (code style, error handling, naming) land at the 17 mod leaves; root, mods/ and common/ are root/aggregator scope and are skipped per the skill.
- Testing guides (unit, integration) land at the root: the in-game test harness is shared across mods (common/tools/TestRunner).
- Domains (entities, glossary) land at the 17 mod leaves: their domains/ holds only the scaffold README. Root is skipped because it already has domains/tooling-friction.md.
- Architecture skipped: not requested. Styling skipped: the template is CSS/SCSS for web frontends; the mods have no such UI.
- common/ gets nothing: aggregator scope with no wired leaves.
- Units draft change-sets and run `write plan` only; the manager applies them one at a time, so no two applies race on the store.

## Key Files
- .context/tasks/OM-001-populate-context/units/: unit specs and reports
- .context/tasks/OM-001-populate-context/changesets/: one change-set per node, drafted by units

## Progress
- [x] Resolve store, read templates, decide placement
- [ ] Unit 01 implement: root change-set (task-plan x6, workflows x3, testing x2) ← IN PROGRESS (dispatched)
- [ ] Units 02-05 implement: 02 standards for all 17 mods; 03-05 domains, six, six and five mods ← IN PROGRESS (dispatched in parallel)
- [ ] Apply every change-set sequentially; `ks check --store .`
- [ ] Review unit over landed documents
- [ ] Retrospective, commit, PR

## Open Questions / Blockers
- None yet.

## Constraints
- Never overwrite an existing document; never write decisions/, retrospectives/, META.md, iconrc.json, overview.md.
- Where the repository gives no answer, leave TODO(owner) rather than inventing.
- User memory: commit messages are one terse line; no hedging language in docs.
