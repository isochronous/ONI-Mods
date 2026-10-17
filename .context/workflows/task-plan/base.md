---
doc_id: doc-2dcfc935aaf7
description: "The plan.md template for ONI-Mods tasks: required sections, Phase State and Phase Handoff Log formats, guidance for each section and the repository's build and test verification commands"
---

# Plan Document Format

> The `task-plan` skill
> reads this file to determine the required `plan.md` format for this repository.
>
> Keep the core sections (Task, Branch, Objective, Folder, Decisions, Key Files,
> Progress, Open Questions/Blockers, Constraints) — agents depend on these
> headings. Add team-specific sections after Constraints.

## Template
**Id:** blk-6a573dfa5362

```markdown
## Task: [TASK-ID, e.g. OM-002]
## Branch: [BRANCH-NAME, e.g. OM-002-short-description]
## Objective: [What we're accomplishing and why]
## Folder: .context/tasks/[TASK-ID-short-description]/

## Phase State
- **Phase plan**: investigation → architecture → implementation → testing → completion
- **Completed**: investigation, architecture
- **Current**: implementation   (status: in-progress | done | blocked | pending)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: [BRANCH-NAME]
- **Attempts (current phase)**: 1

## Decisions
- [Decision made]: [Rationale — why this approach over alternatives]

## Key Files
- [path/to/file, e.g. mods/jumper-bridges/src/JumperBridges/Mod.cs]: [What it does or how it was changed]

## Phase Handoff Log

### Handoff: architecture → implementation   (commit: <trailer-marked>)
**Sub-agent outputs**:
- [lens] unit report (tasks/<id>/units/[report file]): [verbatim recommendation + rationale, or a faithful quote of the load-bearing parts — not a lossy one-line summary]
**Reviewer findings**: [each finding + resolution status, or "N/A this phase"]
**Verification evidence**: [checks run + pass/fail; smoke/test output — the actual output, copied, not "passed"]
**Decisions delta**: [decisions made this phase — also mirrored into ## Decisions]
**Key files delta**: [files touched this phase — also mirrored into ## Key Files]
**What the next phase needs**: [the warmstart synthesis, written down instead of re-derived from memory: entry inputs the next phase must have]
**Retro Stage-1 draft** (completion-phase block only): [Avoid / Repeat / Updated draft, persisted here instead of "in session state"]

## Progress
- [x] Completed step — [brief outcome or note]
- [ ] Current step ← IN PROGRESS
- [ ] Upcoming step

## Open Questions / Blockers
- [Anything the next agent or developer needs to resolve or be aware of]

## Constraints
- [Key constraint from the user, the store, or discovered during work]
```

## Section Guidance
**Id:** blk-ba5051f99876

- **Phase State** — The lean pointer a cold session (or a phase launcher) reads
  to resume without scanning the whole plan. Written when the task is created —
  the manager records the per-task **Phase plan**, an ordered subsequence of
  `investigation → architecture → implementation → testing → completion`
  (`completion` always last) — and updated at every phase boundary: advance
  `Completed`/`Current`/`Next`, set the `Current` status, record the loaded
  skill, and reset `Attempts` to `0`. **Attempts ownership**: this counts how
  many times the current phase has been *launched*. In automated single-shot /
  cron launches the **phase launcher owns the counter** — it increments and
  commits `Attempts` before each launch, so a persistently failing phase is
  bounded even when its session fails closed and commits nothing; a successful
  phase exit resets it to `0`. (In-process launchers use their own counter and
  treat persisted `Attempts` as a secondary guard; interactive runs need not
  maintain it.) `Branch` is the integrity anchor; do not store a commit SHA here
  (the branch tip is the state). Keep it lean and parsable.
- **Phase Handoff Log** — Append-only, one `###` block per phase as it completes.
  Capture the state that would otherwise live only in session memory: verbatim
  unit reports (by report file), `review` findings + resolution, verification evidence
  (copied output, not "passed"), the Decisions/Key Files deltas (also mirrored
  into their sections), and **What the next phase needs** (the warmstart the
  following phase reconstructs from). The completion block also carries the
  Retro Stage-1 draft. Never rewrite or prune earlier blocks. Phase boundaries
  are commit points: the boundary commit carries a `Phase-Handoff: <phase>`
  trailer that phase entry verifies. Uncommitted work at a boundary means an
  incomplete handoff.
- **Decisions** — Record every non-obvious choice and the reason for it.
  Future agents should not re-litigate decided questions. If you chose A over B,
  say why.
- **Key Files** — List every file touched or about to be touched. A resuming
  agent should be able to orient in the codebase from this list alone.
- **Progress** — Check off steps as they complete. Add a brief outcome note —
  not just ✓ but what the result was.
- **Open Questions / Blockers** — Ambiguities, risks, or things the next agent
  needs to decide. Do not carry these only in memory.
- **Constraints** — API limitations, backwards-compatibility requirements, user
  constraints discovered mid-task.

## Repository Specifics
**Id:** blk-b6ca8e2d3bd3

- **Task IDs** use the prefix `OM` (`local_task_id_prefix` in `.context/iconrc.json`): `OM-001`, `OM-002`, and so on. The task folder is `.context/tasks/OM-NNN-kebab-description/` and the branch has the same name (`workflows/branching.md`).
- **Phase boundary commits** are one terse line like the rest of the log, `OM-NNN: <what the phase did>`, with the `Phase-Handoff: <phase>` trailer and the assistant's `Co-Authored-By` trailer (`workflows/commit-conventions.md`).
- **Verification evidence** copied into a handoff block is the output of the repo's real commands: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` for the build, `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <Mod>` for the in-game tests of a touched mod (`testing/unit-testing.md`), and `python common/tools/Translations/translations.py <mod-folder>` when player-visible strings changed. There is no CI to wait for (`workflows/ci-cd.md`).
- **Scope of a task** is a set of folders under `mods/<name>` and, when the shared build or harness changes, `common/` (a submodule with its own history; commit inside it first, then bump the pointer).
- **Key Files** paths are written from the repository root.
