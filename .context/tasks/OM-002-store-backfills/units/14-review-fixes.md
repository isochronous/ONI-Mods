# Unit 14 — review: confirm the unit 12 findings are resolved

## Objective
Confirm that the change-sets applied after unit 12 (changesets/fix12-*.json, fix12b-*.json, fix14-*.json) resolve every `fix` finding in `units/12-review-report.md` and introduce no new error.

## Scope (read only)
- The store as it is now (`mods/*/.context/`, `.context/`), the review report, and `units/13-implement-review-fixes-report.md`.
- Passes:
  1. Per finding F1.1, F3.1, F3.2, F4.1–F4.8: resolved or not, with evidence (for F4.1/F4.4, a script comparing every shared standards section's and shared standards document's tags across the 17 mods, reporting any section or document whose effective tags differ between mods that share its text).
  2. New errors: a term outside its node's dictionary, a section repeating a term its document or an enclosing heading carries, a folder-map section with subject tags, a broken overview sentence.
  3. `node c:/dev/ai/icon/bin/ks.mjs check --all --strict --store C:/dev/ONI-Mods`: quote the summary line.
- OUT: fixing; writing the store.

## Report
- File: units/14-review-fixes-report.md
- Must contain: the per-finding table with evidence, new findings with severity and location, the check summary.
- Model tier: default
