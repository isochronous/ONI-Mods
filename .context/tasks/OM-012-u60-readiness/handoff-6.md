# Handoff — OM-012-u60-readiness

**Scope:** `common/tools/TestRunner`, `common/tools/ModTest`, `mods/salty-surf`, `mods/upgrade-planner`, `mods/corner-build-anything`, `mods/bitshifter`, `mods/jumper-bridges`, `mods/motion-sensor-range`, `mods/multichannel-critter-sensor`, `mods/smart-weight-plate`, `mods/ovagros-overhead`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/06-test.md`

```
# Unit 06 — test: in-game test suites of every touched mod on the U60 testing branch

## Objective
Plan step "In-game tests per touched mod" of OM-012. Run the in-game suites on the installed U60 testing build (762230) from the tip of branch `OM-012-u60-only` (superset of the ship-now fixes), and report pass/fail per test with evidence. Jeremy is asleep and has said to use the PC as necessary, so full suites and sequential runs are fine tonight.

## Scope
- IN, in this order:
  0. **Save safety first.** On U60 the one irreversible action is writing over an existing save (a U60 save won't open in the live build). Before any run, read how TestRunner/ModTest handle saves (NoColony tests, fixture saves, autosave, any save-on-exit) and confirm no test run writes to a save in Jeremy's save folder that existed before. Also check autosave can't fire during a run onto a pre-existing save. If any path could, stop and report it — don't run.
  1. Confirm the ONI-moono launch parameters being gone doesn't break the runner (it may have relied on them).
  2. TestRunner itself: confirm the new CapturePng patch keeps timelapse/preview images out of the player's folder (check the folder before and after a run).
  3. Suites, each with a minimal mod loadout (the mod under test plus what it needs): salty-surf, upgrade-planner, corner-build-anything, bitshifter, jumper-bridges, motion-sensor-range, multichannel-critter-sensor, smart-weight-plate, ovagros-overhead. Use `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <Mod>` (see the testing pointers in the packet for the exact flags).
  4. The in-game checks the fix reports list (units/02..05 reports): Salty Surf info panel text, Upgrade Planner material picker preselect + rebuild paused/unpaused (rebuild now on next tick), Jumper Bridges overlay tint, Ovagro's Overhead yield/growth/harvest/wilt animations, event subscription behaviour changes in the five mods. Where an existing test covers it, cite the test; where none does and a test is quick to add in the mod's test project following existing patterns, add it; otherwise list it as untested.
- OUT: changing mod code to make a failing test pass (report the failure with evidence instead — screenshot the setup and failure state as the stuck-tests rule says); releases; switching Steam branches; commits.
- Never use -KeepGame; never stop a runner mid-run; pace test steps to game time (no frames-apart build/destroy).
- Acceptance: per-suite pass/fail counts with the runner's actual output; save-folder listing before/after showing no pre-existing save changed (timestamps + sizes); timelapse-folder before/after. Do not claim success without showing evidence.

## Context the packet does not carry
- Fix reports: .context/tasks/OM-012-u60-readiness/units/02-implement-report.md, 03-implement-report.md, 04-implement-report.md, 05-implement-report.md.
- Game install: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (testing branch). Copy Player.log aside before each launch if the previous run crashed.
- Scratch files only in C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad, never the repo root.
- Harness changes are welcome if a test needs something new (no workarounds in test code); say what you changed in common/.
- Screenshots: save them in the scratchpad and list their paths in the report.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/06-test-report.md
- Must contain: save-safety finding; per-suite results table with output excerpts; each in-game check → test name / result / untested; failures with screenshots and Player.log excerpts; any test or harness files added/changed.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/salty-surf`, `mods/upgrade-planner`, `mods/corner-build-anything`, `mods/bitshifter`, `mods/jumper-bridges`, `mods/motion-sensor-range`, `mods/multichannel-critter-sensor`, `mods/smart-weight-plate`, `mods/ovagros-overhead`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner
blk-00af2e4182a4 · reference
  When to Comment
  mods/just-fall-already/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-016318d101d7 · reference
  Yielding to Other Mods
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-064a58168248 · reference
  Braces
  mods/sweep-zones/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-0eb8c73385d0 · reference
  Yielding to Other Mods
  mods/no-magic-fridges/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 283 further candidate(s) matched and are not listed above (8119 of 8192 bytes; the whole selection is 85161). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/bitshifter mods/corner-build-anything mods/jumper-bridges mods/motion-sensor-range mods/multichannel-critter-sensor mods/ovagros-overhead mods/salty-surf mods/smart-weight-plate mods/upgrade-planner --budget 85161

⚠ 12 record(s) in these 9 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/bitshifter:tags.md, mods/corner-build-anything:tags.md, mods/jumper-bridges:tags.md, mods/motion-sensor-range:tags.md, mods/multichannel-critter-sensor:tags.md and 7 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools/TestRunner`, `common/tools/ModTest`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj → tools/TestRunner · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/ModTest/Test-OniMods.ps1, tools/ModTest/disabled.txt, tools/ModTest/included.txt → tools/ModTest · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
