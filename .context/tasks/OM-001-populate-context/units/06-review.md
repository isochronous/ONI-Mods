# Unit 06 — review: the 96 documents landed by OM-001

## Objective
Review the store documents this task landed for accuracy against the repository and for fitness as ICON content. Plan step: "Review unit over landed documents".

## Scope
- IN (read only): the untracked files `git status --short` lists outside `.context/tasks/`: root `.context/workflows/**` and `.context/testing/**`; `mods/*/.context/domains/entities.md`, `glossary.md`; `mods/*/.context/standards/code-style.md`, `error-handling.md`, `naming-conventions.md`.
- Passes to run and name in the report:
  1. Factual accuracy: for every root document and for at least six mods chosen across all three domain batches, check each concrete claim (file paths, class/method names, Harmony targets, commands, tags, branch names) against the code. Check that every file path cited in every document exists (a script is fine).
  2. Invention: any claim with no evidence in the repo that should be TODO(owner) instead.
  3. Leftovers: unreplaced template placeholders (`[Project Name]`, `[language]` and the like), "Template" in titles, web/TypeScript/CSS examples that do not apply.
  4. Secrets: no webhook URLs, tokens, or local paths to secret files.
  5. Wording: hedges, disclaimers, caveats (the owner forbids them).
  6. ICON protocol: the six `workflows/task-plan/*.md` keep Phase State, Phase Handoff Log and the entry contracts.
- OUT: fixing anything; writing the store.
- Acceptance: every finding has severity (block / fix / nit), the document path and heading, and the fix it expects. "No findings" for a pass only with what that pass checked.

## Context the packet does not carry
- Unit reports for what each drafter claimed: `units/01-implement-root-report.md`, `02-implement-standards-report.md`, `03-`, `04-`, `05-implement-domains-report.md` in this task folder.
- Known open TODO(owner)s in the root documents are intended; do not flag them.
- Read a document with `node c:/dev/ai/icon/bin/ks.mjs read <path>` or directly; both are fine for review.

## Report
- File: .context/tasks/OM-001-populate-context/units/06-review-report.md
- Must contain: findings grouped by document, each with severity, location, expected fix; the passes run and their coverage.
- Model tier: default
