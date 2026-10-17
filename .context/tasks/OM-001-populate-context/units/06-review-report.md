# Unit 06 report: review of the 96 documents landed by OM-001

Reviewer: ICON review unit, default tier. Read only; nothing under `.context/` or `mods/` was modified. The scratch scripts used live in the session scratchpad, not in the repo.

## Verdict

No block findings. Eleven fix findings (one cluster of them repeated across all 17 mods), eleven nits. The documents are accurate against the repository to a high degree: every claim checked outside the findings below held. No secrets, no unreplaced template placeholders, no web/TypeScript/CSS examples, and the protocol of the six task-plan documents is intact.

## Passes run and coverage

1. **Factual accuracy.**
   - Path check: a script took every backticked token ending in a file extension or `/` in all 96 documents and resolved it against the repo root, the owning mod folder and its `src/`. All resolved except the items in findings F-4, F-10 and the nits (the rest were bare directory names such as `anim/`, `assets/`, `bin/`, and files outside the repo by design such as `mods.json`).
   - Identifier check: every backticked identifier in the 34 domain documents (17 mods, entities and glossary) was searched for in that mod's own source. Everything found; the three "misses" were file names (`Patches`, `FeaturePatch`, `PlanetoidPatch`).
   - Claim-level checks against code, git and tooling:
     - All 11 root documents: git tags (61, all annotated), tag names, the legacy `direct-automation-bridges-v*` tags, commit subjects quoted as examples (all real), `.gitmodules`, `.gitattributes`, `.gitignore`, `iconrc.json`, `Directory.Build.props`, `oni-mod.props` / `oni-mod.targets` (deploy, `GameFolder`, `ONI_GAME_FOLDER`, `ModDeployFolder=none`), `release_mods.py` and `release.py` behaviour, `label-mod.yml`, `Invoke-OniTests.ps1` parameters, defaults and exit codes, `Invoke-SaveRun.ps1` parameters, `Test-OniMods.ps1` parameters, the TestKit API (assertions, waits, `Rect`, `Cleanup`, `Skip`, `AllowLogError`, `InstantBuild`, `ResearchAll`, `Speed`, `Screenshot`, `LookAt`, `Press`, `PointAt`, `PrimeBuildMenu`, `Dupes.*`), `[OniTest]` options, fixture README, `HangSeconds = 300`, the 15 s keep-alive, the real test excerpts, the `Persistence.cs`, `Eggs.cs`, `Compatibility.cs`, `DialogDrag.cs`, `Generated.cs`, `Looks.cs` references, the 17 `<Mod>.Tests` projects.
     - Mods, all documents (entities, glossary, code-style, error-handling, naming-conventions): bitshifter, dont-count-eggs-on-rails, jumper-bridges (batch 03); natural-backwalls-everywhere, no-magic-fridges, salty-surf, smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner, vent-freeze-fix (batches 04 and 05). Also patch tables only for detect-any-element, corner-build-anything, just-fall-already. That is 14 mods, covering all three domain batches.
     - Shared standards bodies (identical across the 17 mods) were verified once against the code: `LangVersion` 9.0, net48, tabs, Allman, no interpolation in mod code, `Loaded version` log line in all 17 mods, `Prepare`/`TargetMethods` usage, `FieldRefAccess`, `[MyCmpGet]`, `[Serialize]` OptIn, patch-class names, `ModStrings` aliases, the catch-site inventory in `mods/**/src`.
2. **Invention.** Looked for statements with no repo evidence. Found one value stated as fact that the repo cannot confirm (nit N-8). Other apparent inventions (bitshifter removal behaviour, no-magic-fridges "a fifth") trace to the mods' own READMEs.
3. **Leftovers.** Grepped all 96 files for template placeholders, "Template", web/TypeScript/CSS/npm/REST examples, emoji. Only the intended `Template` headings of the task-plan documents (N-1) and the `ConfigureBuildingTemplate` game method name matched. No `[Project Name]`, `[language]`, `[YourFramework]`-style placeholders remain. The `[TASK-ID]` / `[OM-NNN]` brackets in the task-plan documents are the fill-in slots of the unit-spec templates and are correct.
4. **Secrets.** Grepped for webhook URLs, `discord.com/api`, `webhooks/`, tokens, keys, passwords, `C:\Users`, the `~/.config/oni-mods` path, `AppData`. Only hit: `%USERPROFILE%/AppData/LocalLow/Klei/...Player.log` in `workflows/ci-cd.md` (the game's public log location, not a secret). The Discord notice is described without URL or config path. The Steam ids that appear (Workshop item ids of other authors' mods, Mod Profile Manager) are public.
5. **Wording.** Grepped for hedges, disclaimers and caveats. The repeated "when the exception may be a reflection wrapper" (every `error-handling.md`) is a conditional instruction, not a hedge. "roughly a fifth" (no-magic-fridges) is the mod README's own figure. One leak of session machinery into content, N-3. No other findings.
6. **ICON protocol.** Diffed each of the six `workflows/task-plan/*.md` against the shipped templates. No template protocol line was removed anywhere: Phase State, Phase Handoff Log, Section Guidance, the Phase Entry and Phase Exit / Handoff blocks of all five phase documents, the entry contracts, the untrusted-data note, the Phase-Handoff trailer rules and the Retro Stage-1 draft field are all present. Changes are limited to the "Customize ..." blockquotes (removed), placeholders (replaced), example paths, and the added `## Repository Specifics`. Result: no findings.

## Findings

### `.context/workflows/commit-conventions.md`

**F-1 (fix) — Format, first paragraph.** States "Every commit is a single subject line. There is no body and no conventional-commit type" and the header says the document is "Derived from `git log`". 151 of the repo's 410 commits have a body beyond trailers (14 of the latest 40). The document presents the owner's rule as an observation of history.
Expected fix: say the rule, not the history: "Write one terse subject line and no body" (the memory-recorded owner preference), and drop "There is no body" as a statement about existing commits. The same wording appears in `branching.md` (F-2).

**F-5 (fix) — Task ID Generation / TODO(owner), and consistency with `branching.md`.** This document settles on three digits (`OM-001`, `printf '%03d'`) while still carrying a TODO(owner) to "confirm the ID width". The TODO is intended; the problem is that `branching.md` uses four digits in two places (F-3). Expected fix: keep the TODO here, make `branching.md` agree with the three-digit generator until the owner decides.

**N-6 (nit) — header line.** "409 commits" is stale by the time the documents land (410 at review time). Expected fix: drop the count or say "the history".

### `.context/workflows/branching.md`

**F-2 (fix) — Branch Structure, first paragraph.** "the history is linear (no merge commits)". `git log --merges` shows 17 merge commits, the "Bring in <mod> with its history" commits, and the same paragraph then calls those "plain commits". Same document, Commit Messages: "One terse line, no body" (see F-1). Expected fix: "History on main is linear apart from the 17 merge commits that brought each mod in with its history"; and the F-1 rewording for the commit line.

**F-3 (fix) — Branch Structure diagram and Branch Types table.** The diagram says `<prefix>-<NNNN>-<kebab description>` and the table says `OM-NNNN-short-description`; the real branch, the Naming Conventions section of the same document and `commit-conventions.md` use three digits (`OM-001-populate-context`). Expected fix: `OM-NNN` in both places.

**N-3 (nit) — Pull Request Guidelines.** "carries the attribution lines required by the session reminder". A document for the repo should not refer to a harness reminder. Expected fix: "carries the assistant's attribution lines as described in `commit-conventions.md`".

**N-9 (nit) — Git Configuration.** "`.gitattributes` forces LF under `.context/` and on tool files" is loose: it lists `.context/**`, `.claude/commands/**`, `bin/**`, `hooks/**` and similar ICON paths, not the repo's own tool scripts. Expected fix: "forces LF under `.context/` and `.claude/commands/`".

### `mods/*/.context/standards/naming-conventions.md` (all 17)

**F-4 (fix) — Code Naming > Ids and Tags, third bullet: "The `staticID` in `mod_info.yaml` is `Isochronous.<PascalName>`".** `staticID` lives in `src/<Mod>/mod.yaml` in all 17 mods; `mod_info.yaml` holds `supportedContent`, `minimumSupportedBuild`, `version`, `APIVersion` and has no `staticID`. The "Examples by Type" row ("Mod metadata: `mod.yaml`, `mod_info.yaml`") is correct. Expected fix: "The `staticID` in `mod.yaml` is `Isochronous.<PascalName>`" in all 17 files (the shared body, one change).

**N-4 (nit) — Examples by Type, Art row.** Backticks `anim/` and `assets/` as folders without saying where; they sit under `src/<Mod>/` (and `assets-src/` for sources in some mods). Expected fix: write `src/<Mod>/anim/`.

### `mods/*/.context/standards/error-handling.md` (all 17)

**F-6 (fix) — try/catch, list formatting.** Item 3 and item 4 of the "four situations" list are separated by a blank line, so item 4 renders as a new list starting at 1 in most renderers. Expected fix: remove the blank line before item 4.

**N-7 (nit) — try/catch, "four situations and nowhere else".** Two catch sites do not fit the four labels cleanly: `mods/upgrade-planner/src/UpgradePlanner/Rules/Variants.cs` (dry-run of other mods' build-tool prefixes, closest to situation 1) and `mods/natural-backwalls-everywhere/src/NaturalBackwalls/ExportOverlay.cs` (file write, situation 2, but not listed). Expected fix: name `Variants.cs` under situation 1 and `ExportOverlay.cs` under situation 2, or say "in these situations".

**N-5 (nit) — shared body in leaf nodes.** Every mod's `standards/` carries the same ~150-line body that cites other mods' source paths (for example the Bitshifter document cites `mods/upgrade-planner/...`, `mods/storage-pod-redux/...`). This is accurate but makes each leaf node's `applies_to: src/**` document describe the whole repo, and a change to a convention needs 17 edits. Expected fix: acceptable as landed; if the owner wants the standard once, promote the shared body to a root `standards/` document and keep only the "<Mod> specifics" sections in the mods. No change required for this task.

### `mods/*/.context/domains/entities.md` and `glossary.md`

**F-7 (fix) — missing `applies_to` on the batch-03 entities documents.** `entities.md` of bitshifter, corner-build-anything, detect-any-element, dont-count-eggs-on-rails, jumper-bridges and just-fall-already declare no `applies_to`; the other 11 mods' `entities.md` declare `src/**`-style coverage. Without it a brief over the mod's code does not reach these documents (the packet's own warning lists this kind of record). Expected fix: add the same `applies_to` the other eleven carry (`src/<Mod>/**`).

**N-2 (nit) — `glossary.md` of all 17 mods declare no `applies_to`.** Same reach problem as F-7 for the glossaries. Expected fix: add `applies_to` (or decide glossaries are reached by tag only and say so).

**F-8 (fix) — `mods/jumper-bridges/.context/domains/entities.md`, Key Code Paths, "Design record".** Cites `incubator/direct-automation-bridges/INVESTIGATION.md, section 6` as "in the ONI-Mods workspace". The file exists, but at `C:/dev/incubator/direct-automation-bridges/INVESTIGATION.md`, outside this repository (it is the mod README's own wording). A reader in the repo cannot resolve the path. Expected fix: say it is outside the repository, give the path relative to the workspace root (`../incubator/direct-automation-bridges/INVESTIGATION.md`), or mark it TODO(owner) to bring the notes into the mod folder.

**N-8 (nit) — `mods/sweep-zones/.context/domains/entities.md`, Business Rules: "mass not over `MopTool.maxMopAmt` (150 kg)".** The 150 kg value is a game constant that is not in the repo (the code compares against `MopTool.maxMopAmt`). Expected fix: drop the number or say "the game's `MopTool.maxMopAmt`".

**N-10 (nit) — `mods/natural-backwalls-everywhere/.context/domains/entities.md`, Other Harmony patches.** The `WorldGenSimUtil.DoSettleSim` row lists only the transpiler; `SettleSimPatch.cs` also has a postfix (`SettleSimBackwalls.LogSummary()`). Expected fix: add "(and a postfix that logs the summary)" to the row.

**N-11 (nit) — `mods/upgrade-planner/.context/domains/entities.md`, PlanPreset.** "Stored in `mods/config/UpgradePlanner/presets.json`" is relative to the game's user mods folder (`KMod.Manager.GetDirectory()`), and reads as a repo path next to `mods/<name>`. Expected fix: "`<Documents>/Klei/OxygenNotIncluded/mods/config/UpgradePlanner/presets.json`".

### `.context/testing/unit-testing.md`

**F-9 (fix) — Test Data > Fixtures.** Says the fixture is `testbed-so.sav` "or `testbed-base.sav`" and that the `.sav` files are not committed. On disk only `testbed-so.sav` exists beside the README (the README states the same loader rule). Expected fix: "`testbed-so.sav` (Spaced Out) or `testbed-base.sav`; neither is committed, copy one in (only `testbed-so.sav` exists on this machine)" or drop the machine note and keep the loader rule. The same sentence in `integration-testing.md` (Launcher Options, `-Fixture` row; Machine requirements) is consistent with it and needs the same wording.

**N-1 (nit) — Naming Conventions, prefix list.** Lists eleven prefixes; the code also has `PLib:` (7 tests in upgrade-planner) and `control:` (1 test). Expected fix: add both, or say "for example".

### `.context/workflows/task-plan/*.md` (six documents)

**N-1b (nit) — titles.** "Architecture Phase Templates", "Completion Phase Templates", etc. keep the word "Template" from the shipped files, and base.md keeps `## Template`. These are the protocol's own names for the unit-spec templates and match the shipped titles, so they were left alone by the drafter; the pass 3 rule flags "Template" in titles, and this is the only instance class. Expected fix: none needed if the protocol names are to stay verbatim; otherwise retitle to "Architecture Phase" etc.

### `.context/workflows/ci-cd.md`, `.context/testing/integration-testing.md`

**F-10 (fix) — ci-cd.md, Stage 1 Build.** "A successful build deploys the DLL, `mod.yaml`, `mod_info.yaml`, `assets/` and `anim/`". `oni-mod.targets` `DeployToLocalMods` also copies `translations/` (and the mod-specific `DeployWorldgen` target copies `worldgen/` for Natural Backwalls). Expected fix: add `translations/`.

**N-12 (nit) — ci-cd.md, Pipeline Configuration, Secrets table.** The Discord webhook row says "Read from a local config file outside the repository". `release.py` reads the `ONI_RELEASE_WEBHOOK` environment variable first, then the config file. Expected fix: "an environment variable or a local config file outside the repository" (still no URL, no path).

Notes that are not findings: the intended TODO(owner)s (task ID width, task branch return path, PR template, ruleset, WorkshopUpload run path, rollback, coverage target, agent-instructions file, `.editorconfig`) were left as intended. `domains/README.md` and `standards/README.md` in each mod still carry their scaffold `TODO(owner)` text; they are outside this review's scope (not among the 96 files).

## Answers to the packet's three questions

- Anything above that did not apply: the 30 listed pointers are all `try/catch`, `Braces`, `Indentation`, `When to Comment`, `Yielding to Other Mods` and `Harmony Patches` sections of the mods' own standards. They were the documents under review, not guidance for the review, so none shaped the work; they did resolve and were consistent with each other.
- Anything governing this work that did not appear: the shipped templates (`c:/dev/ai/icon/skills/populate-context/templates/`) are what pass 6 is measured against, and no pointer led to them; the reviewer found them by path. The `Persistence.cs`/TestRunner README and the release scripts were also reached by path, not by pointer.
- Pointer reasons that did not match the record: the pointer lines name the same set of source paths for every `error-handling.md` `#try-catch` pointer (`FeaturePatch.cs`, `Mod.cs`, `ModDetection.cs`, "and 4 more"), because the 17 mod documents carry the same shared body. They match what the records say. Nothing mismatched.

## Friction (for the receiver to record)

- `ks brief` / the handoff packet's budget (8192 bytes of 128463) listed 30 of 496 candidates, all of them near-identical sections of one shared body repeated across 17 nodes; the shipped templates and the repo's tooling that a review would check against did not rank. The coverage warning at the end of the brief (7 root records with no `applies_to`, tags or source path, among them `workflows/task-plan/*.md`) is the same finding as F-7/N-2 for the root documents: base.md and the phase documents cannot be reached by a brief over code paths.
- Auto mode told the agent to prefer Bash over Read/Edit/Write for file work, while the owner's memory says to write scripts with Write; Write was used for the two scratch scripts.
