# Handoff — OM-001-populate-context

**Scope:** `.`, `common`, `.github`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-001-populate-context/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-001-populate-context/units/01-implement-root.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `.`, `.github`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 2 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: domains/tooling-friction.md, overview.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: overview.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
