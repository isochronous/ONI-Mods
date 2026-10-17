# Unit 08 report: review of the fix diff applied after unit 06

Reviewer: ICON review unit, basic tier. Read only. Nothing was modified, applied or committed. Checks were run against the uncommitted diff over `.context/` and `mods/*/.context/` (excluding `.context/tasks/`), the unit 06 report and the unit 07 report, and the repo files the fixed text cites.

## Verdict

Of the eleven fix findings (F-1 to F-10, with F-1 counted once), nine are resolved, one (F-1) is partly resolved, and F-9 is resolved on a different basis from the one the review assumed. No new factual error was found in the changed text. Two new items are raised: a witness class chosen without a stated basis, and a stale header in `commit-conventions.md`. Most of the nits were not in unit 07's change-sets and are unchanged.

## Passes run

1. Diff coverage: all 63 changed files in `.context/` and `mods/*/.context/` were listed. The 34 per-mod standards files were diffed across all 17 mods. The 23 domain and root files were read as hunks.
2. Per-finding evidence: each finding was checked against the working tree, and where it cites code, against that code (grep and `git` commands).
3. New-error pass: every new claim in the changed text was checked against the repository (see the list under "Checked claims" below).
4. Leftovers: grepped the changed root and mod text for `NNNN`, `no merge commits`, `no body`, `409`, `Derived from`.
5. Front matter: each of the 23 new `applies_to` blocks was checked for form. `witness` values were checked against the `ks write schema --row narrative --mode cover` enum (`strict`, `loose`, `none`). The witness digests were not recomputed; no read-only verify was run.

## Per-finding table

| Finding | Status | Evidence |
|---|---|---|
| F-1 commit-conventions "Format" and branching "Commit Messages" | Partly resolved | The Format paragraph and branching "Commit Messages" now say new commits carry no body. But the `commit-conventions.md` header still reads "Derived from `git log` ... (409 commits ...)" (line 7). The repo has 410 commits, and that header is the history framing F-1 asked to drop (also N-6). |
| F-2 branching "Branch Structure" and "Task Work" | Resolved | Line now reads "linear except for the merge commits that brought each mod in". `git log --merges` gives 17 merges, matching. "merge commits appear only where a mod's history was brought in" in "Task Work". |
| F-3 branching diagram and Branch Types | Resolved | `<NNN>` in the diagram and `OM-NNN-short-description` in the table. No `NNNN` left under `.context/workflows` or `.context/testing`. |
| F-4 naming-conventions "Ids and Tags" (all 17) | Resolved | 17 of 17 read "The `staticID` in `mod.yaml`". `staticID` is in `src/<Mod>/mod.yaml` in all 17 mods and in no `mod_info.yaml`. |
| F-5 commit-conventions "Task ID Generation" | Resolved | TODO replaced with the three-digit statement, consistent with branching now. |
| F-6 error-handling "try/catch" (all 17) | Resolved | 17 blank lines removed, each between items 3 and 4 (hunk checked in bitshifter). The list now renders as one list. |
| F-7 entities and glossary `applies_to` | Resolved | Six batch-03 `entities.md` (bitshifter, corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges, just-fall-already) gained `src/<Project>/**`. All 17 glossaries gained the same selector. See the witness note below. |
| F-8 jumper-bridges "Key Code Paths" design record | Resolved | Now states the notes are kept outside this repository. No path left that cannot be resolved. |
| F-9 fixtures (unit-testing "Fixtures", integration-testing "Launcher Options" and "Machine requirements") | Resolved, on a different basis | The review said `testbed-so.sav` is not committed. It is: `git -C common ls-files tools/TestRunner/fixtures` lists `testbed-so.sav` (commit 01aefe7), and `common/.gitignore` says fixture saves are tracked. The changed text is correct. The `common` fixtures README (line 14, "The `.sav` files are not committed") is stale; it is outside `.context/` and was not touched. |
| F-10 ci-cd "1. Build" | Resolved for `translations/` | `common/oni-mod.targets` line 125 deploys `translations/`. The 06 finding also mentioned that `DeployWorldgen` copies `worldgen/` for Natural Backwalls only (`mods/natural-backwalls-everywhere/src/NaturalBackwalls/NaturalBackwalls.csproj`). That remains unmentioned in ci-cd.md. Not a new error, but a residual gap in the same paragraph. |

### Nits

| Nit | Status | Evidence |
|---|---|---|
| N-1 unit-testing prefixes | Resolved | `PLib:` (in `mods/upgrade-planner/src/UpgradePlanner.Tests/DialogDrag.cs`) and `control:` (in `mods/jumper-bridges/src/JumperBridges.Tests/WireDrags.cs`) both exist in code. |
| N-1b task-plan "Template" titles | Not changed | Left as the protocol names, per unit 06's note. |
| N-2 glossary `applies_to` | Resolved | All 17 glossaries. |
| N-3 branching "Pull Request Guidelines" | Resolved | Session-reminder wording replaced with "as described in `commit-conventions.md`". |
| N-4 naming-conventions "Art" row | Not changed | Still backticks `anim/` and `assets/` without `src/<Mod>/`. |
| N-5 shared standards body | No change required | Per unit 06. |
| N-6 "409 commits" header | Not changed | Header still says 409; the repo has 410. |
| N-7 error-handling "four situations" | Not changed | `Variants.cs` (upgrade-planner) and `ExportOverlay.cs` (natural-backwalls-everywhere) still not named. |
| N-8 sweep-zones 150 kg | Resolved | Now "the game's `MopTool.maxMopAmt`". |
| N-9 branching `.gitattributes` | Not changed | Still "forces LF under `.context/` and on tool files". |
| N-10 natural-backwalls "Other Harmony patches" | Not changed | `DoSettleSim` row still omits the postfix in `SettleSimPatch.cs`. |
| N-11 upgrade-planner `PlanPreset` path | Not changed | Text is "under the game's data folder (`Presets.FilePath`)"; the relative path reading remains, though the new wording makes the base clearer. |
| N-12 ci-cd Discord webhook row | Not changed | Still says "Read from a local config file", omitting `ONI_RELEASE_WEBHOOK`. |

## New findings

**NF-1 (low) — witness class without a stated basis.** The 23 `cover` writes use `witness: loose` (`witness_digest` values added). `loose` is a valid enum value, so the front matter is well formed, but nothing in the unit 07 report says why `loose` and not `strict` or `none` for a domain document. Location: every `domains/entities.md` and `domains/glossary.md` changed in this diff. Expected fix: the owner confirms the class, or the reason is recorded in the write's `why`. The digests were not recomputed here.

**NF-2 (low) — stale header in `commit-conventions.md`.** Line 7 "Derived from `git log` ... (409 commits ...)" is both the F-1 history framing and N-6 (see above). Expected fix: drop the count and the "Derived from" clause, or say "from the history of `C:/dev/ONI-Mods`".

**NF-3 (process) — unit 07 report is out of step with the tree.** Unit 07 says nothing was applied, but the working tree already holds the edits for every finding in its table (34 standards files, the root documents, 23 domain files). The claim "plan only" in unit 07 no longer describes the repository. The fix diff reviewed here is the applied state, so the review rests on the tree, not on the report's "change-set" wording.

**NF-4 (residual) — ci-cd Build omits the worldgen deploy.** Noted under F-10. The paragraph says a successful build deploys a fixed list. Natural Backwalls also deploys `worldgen/` via `DeployWorldgen`. Expected fix: say "and, for Natural Backwalls, `worldgen/`", or leave it for the mod's own document.

## Checked claims (new text)

- Fixture: `testbed-so.sav` tracked in `common` (`git -C common ls-files`); base-game `.sav` not present, as stated.
- `translations/` deployed by `common/oni-mod.targets` (`DeployTranslations`, line 125).
- 17 merge commits, 410 commits total.
- `PLib:` and `control:` prefixes exist in the cited test files.
- `mod.yaml` holds `staticID`; no `mod_info.yaml` does.
- Jumper-bridges design notes: outside the repo, as stated.
- sweep-zones `MopTool.maxMopAmt`: named as the game's constant; no number asserted.
- `.gitattributes`, `iconrc.json` and the other unchanged root references were not in the changed text and were not rechecked.

## Friction (for the receiver to record)

- The brief in the packet (30 of 496 candidates shown) listed only the 17 shared `try/catch`, `Braces`, `Indentation` and `Yielding to Other Mods` sections, all of them the documents under review. None governed the review. The governing material (the witness enum and the `common/.gitignore` note that fixture saves are tracked) was reached by path, not by any pointer. The F-9 evidence sat in `common/.gitignore`, which the brief did not surface.
- No ICON friction was met in this review beyond the above.

## Answers to the packet's three questions

- Did anything above not apply once opened: the 30 pointers are sections of the mods' own standards, not guidance for the review. None shaped the work.
- Did anything governing the work fail to appear: yes, the `common/.gitignore` note that fixture saves are tracked (the basis of F-9) and the witness enum in `ks write schema`. Neither was in the brief.
- Did any pointer's one-line reason not match its record: none. Every `error-handling.md` `#try-catch` pointer names the same source list ("and 4 more"), which matches the shared body but does not discriminate between mods.
