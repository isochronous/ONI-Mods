# Unit 08 — review: the fix diff applied after unit 06

## Objective
Confirm the uncommitted changes (`git diff` over `.context/` and `mods/*/.context/`, excluding `.context/tasks/`) resolve the findings in `units/06-review-report.md` as `units/07-implement-fixes-report.md` claims, and introduce no new errors.

## Scope
- IN (read only): that diff; the two reports; the repo files the fixed text cites.
- Passes: each of F-1 to F-10 and the listed nits resolved or not; any new factual error, broken path, hedge or leftover in the changed text; front-matter coverage (`applies_to`, `witness`) well formed.
- OUT: fixing; writing the store.
- Acceptance: per finding resolved / not resolved with evidence; new findings with severity, location and expected fix.

## Report
- File: .context/tasks/OM-001-populate-context/units/08-review-fixes-report.md
- Must contain: the per-finding table and any new findings, with the passes run.
- Model tier: basic
