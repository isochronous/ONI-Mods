# Unit 01 — implement: draft the root node change-set (task-plan, workflows, testing)

## Objective
Draft one change-set for the root node (`node: "."`) creating 11 documents adapted from ICON templates. Plan step: "Unit 01 implement".

## Scope
- IN: templates `workflows/task-plan/base.md`, `phase-investigation.md`, `phase-architecture.md`, `phase-implementation.md`, `phase-testing.md`, `phase-completion.md` landing at `workflows/task-plan/`; `workflows/branching.md`, `workflows/commit-conventions.md`, `workflows/ci-cd.md`; `testing/unit-testing.md`, `testing/integration-testing.md`.
- Read for facts: repo root (README, CLAUDE.md / .claude, scripts like release.py, .github/), git history and tags (`git log`, `git tag`), `common/` (build props/targets, `common/tools/TestRunner`, ModCheck, WorkshopUpload), and a couple of mods' test folders.
- The task-plan documents are ICON process documents: keep their protocol (Phase State, Phase Handoff Log, entry contracts) intact; adapt only repo-specific bits (build/test commands, branch naming, task prefix `OM`).
- Commit conventions: derive from `git log` (the owner writes one terse line, e.g. "Jumper Bridges: new preview ..."; mod name prefix). Branching: derive from actual branches and tags (tags look like `<mod>-v<version>`).
- CI/CD: describe what actually exists (GitHub Actions if any, local release script, Steam Workshop upload, Discord notice). Never write the Discord webhook URL; say only that it is read from a local config file outside the repo.
- Testing: describe the real harness (in-game `[OniTest]` coroutines run by TestRunner in the real game, and any plain unit tests if present). Commands must be real ones found in the repo.
- OUT: anything under mods/*/.context or common/.context; applying.
- Acceptance: one JSON file at `.context/tasks/OM-001-populate-context/changesets/root.json` with 11 create ops; `write plan` accepts it (output shown).

## Context the packet does not carry
- Read `.context/tasks/OM-001-populate-context/units/common-rules.md` first; it binds this unit.

## Report
- File: .context/tasks/OM-001-populate-context/units/01-implement-root-report.md
- Must contain: the change-set path, one line per document saying what repo facts went in, every TODO(owner) left and why, the `write plan` output summary verbatim, friction met.
- Model tier: default
