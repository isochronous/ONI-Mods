# Unit 07 — implement: draft fix change-sets for the review findings

## Objective
Draft change-sets that correct the landed documents per `units/06-review-report.md`. Plan step: "Review fixes".

## Scope
- IN: fix findings F-1 to F-10, and these nits: give every `glossary.md` (all 17 mods) the same `applies_to` its mod's `entities.md` uses; add the missing test prefixes (`PLib:`, `control:`) to the prefix list; remove the "session reminder" mention from `branching.md`; the sweep-zones 150 kg mop limit: cite where it comes from in the game or reword it as a game value without a number.
- Decisions already made:
  - F-1: state the rule. Commit subjects are one terse line; new commits carry no explanatory body. Do not claim the log has no bodies.
  - F-2: say merge commits appear only where a mod's history was brought in.
  - F-3/F-5: task ids are three digits (`OM-001`), matching the real branch `OM-001-populate-context`. Make `branching.md` agree; the commit-conventions ID-width TODO(owner) can be resolved to three digits.
  - F-7: add `applies_to` to the six `entities.md` that lack it, the same shape the other 11 use (`src/<Project>/**`).
  - F-8: drop the out-of-repo path; name it as investigation notes kept outside this repository.
  - F-9: name the fixture that exists on disk.
  - The repeated shared body in standards stays as is.
- Edits are `section` ops (replace one heading block's body; body excludes the heading and its `**Id:**` line). Changing `applies_to` on an existing document: find the operation for it with `node c:/dev/ai/icon/bin/ks.mjs write schema` (list rows/modes) and use it. Never `create` over an existing document.
- One change-set per node: `.context/tasks/OM-001-populate-context/changesets/fix-<node-name>.json` (`fix-root.json` for `.`). Two ops on one destination in one change-set are refused: merge edits to the same section into one op.
- OUT: anything else; applying; committing.
- Acceptance: every change-set accepted by `write plan --changeset <file> --at .` from `C:/dev/ONI-Mods` (show output). If a plan says an op is protected, say so in the report with the confirm digest it printed.

## Context the packet does not carry
- Read `.context/tasks/OM-001-populate-context/units/common-rules.md` (its "Change-set format" and "Verification" parts apply; the template parts do not) and `units/06-review-report.md`.

## Report
- File: .context/tasks/OM-001-populate-context/units/07-implement-fixes-report.md
- Must contain: per finding, which change-set and op fixes it; the `write plan` summary per file; any protected ops with their digest; friction met.
- Model tier: default
