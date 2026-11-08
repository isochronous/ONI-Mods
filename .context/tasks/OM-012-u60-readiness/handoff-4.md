# Handoff — OM-012-u60-readiness

**Scope:** `common/tools/TestRunner`, `mods/bitshifter`, `mods/jumper-bridges`, `mods/motion-sensor-range`, `mods/multichannel-critter-sensor`, `mods/smart-weight-plate`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/04-implement.md`

```
# Unit 04 — implement: TestRunner timelapse patch and obsolete overloads on U60

## Objective
Plan step "Fix per mod" of OM-012, two parts:
1. common/tools/TestRunner: the Timelapser patch no longer takes effect — U60 writes timelapse/preview images through a new private `CapturePng` that bypasses `WriteToPng`, so test runs would write timelapse images into the player's folder. Retarget it so no image reaches the player's folder.
2. The `Subscribe`/`Unsubscribe` and `GameScenePartitioner.Add` overloads that U60 marks obsolete: move every use in bitshifter, jumper-bridges, motion-sensor-range, multichannel-critter-sensor and smart-weight-plate to the replacement U60 recommends (the Obsolete attribute message names it). Set `minimumSupportedBuild: 762230` in each of those mods' mod_info.yaml if the replacement doesn't exist in U59 (check F:\ONI-Frozen\APP).

## Scope
- IN: common/tools/TestRunner (the submodule is on branch `u60`); the five mods listed.
- OUT: no-magic-fridges (retired — leave its obsolete calls alone); other mods; launching the game; commits (the manager commits, inside common/ first); version/CHANGELOG.
- Acceptance: TestRunner and the five mods (and their test projects) build with zero errors and no remaining CS0618 obsolete warnings for those APIs; show the Timelapser patch's target resolves against the U60 DLLs. Your report must include the actual command output — do not claim success without it.

## Context the packet does not carry
- Decision (revised by Jeremy mid-task): fixes that can work on both the live U59 build (744825) and U60 ship now; only changes that need U60-only API wait on the held branch. So write every fix here to work on both builds where that's possible (e.g. resolve a patch target by name, pick whichever method exists, avoid U60-only types), and say per change in the report whether it is BOTH-BUILDS or U60-ONLY. Verify both-builds claims against the frozen U59 DLLs at F:\ONI-Frozen\APP as well as the U60 ones. Don't raise minimumSupportedBuild for a both-builds change.
- Unit 01 report: .context/tasks/OM-012-u60-readiness/units/01-explore-u60-breakage.md (common/ and obsolete-warnings sections).
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Scratchpad with unit 01's PatchCheck host: C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad. Write scripts with the Write tool.
- Changing the obsolete overloads changes event subscription semantics if the new API differs (e.g. handler signatures, returned handles that must be kept for unsubscribe) — read the new overloads' implementations before converting, and say in the report where behaviour could differ.
- Build only your projects with `-p:ModDeployFolder=none`; other units edit salty-surf, upgrade-planner, corner-build-anything and ovagros-overhead concurrently.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/04-implement-report.md
- Must contain: files changed with why; build output; patch resolution evidence; behaviour differences to check in game.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/bitshifter`, `mods/jumper-bridges`, `mods/motion-sensor-range`, `mods/multichannel-critter-sensor`, `mods/smart-weight-plate`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

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
blk-13c88b17e86e · reference
  Indentation
  mods/salty-surf/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-15a99617bbd0 · reference
  When to Comment
  mods/refined-building/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-175c1c6cba09 · reference
  Yielding to Other Mods
  mods/jumper-bridges/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-183bc6e8000f · reference
  Harmony Patches
  mods/motion-sensor-range/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-19b851b334fa · reference
  Yielding to Other Mods
  mods/salty-surf/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-1d2cc304e9de · reference
  Indentation
  mods/detect-any-element/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-1d9ad5f30869 · reference
  Indentation
  mods/sweep-zones/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-1ebc400095cd · reference
  Harmony Patches
  mods/corner-build-anything/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-24b49007faf3 · reference
  Braces
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-27d655025249 · reference
  Harmony Patches
  mods/jumper-bridges/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-29493d7a7b8a · reference
  When to Comment
  mods/upgrade-planner/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-29c3d03bda81 · reference
  Braces
  mods/upgrade-planner/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-2fe104dbf053 · reference
  Key Code Paths
  mods/bitshifter/.context/domains/entities.md#key-code-paths · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/OverlayLabelGuard.cs, mods/bitshifter/src/Bitshifter/Patches.cs and 1 more → mods/bitshifter
blk-32ca28598b1a · reference
  Indentation
  mods/multichannel-critter-sensor/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-345de95fe26c · reference
  Indentation
  mods/no-magic-fridges/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-38682c142e7f · reference
  Yielding to Other Mods
  mods/vent-freeze-fix/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-3b9868193a7b · reference
  Braces
  mods/refined-building/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-3caa04f3cdc5 · reference
  Indentation
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-3d37bcce99a4 · reference
  When to Comment
  mods/storage-pod-redux/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-40ac288e819d · reference
  When to Comment
  mods/motion-sensor-range/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-430105ebb58b · reference
  When to Comment
  mods/vent-freeze-fix/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-4351aee3774b · reference
  When to Comment
  mods/multichannel-critter-sensor/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate
blk-441857351745 · reference
  Harmony Patches
  mods/vent-freeze-fix/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-45b3c9a86343 · reference
  Harmony Patches
  mods/storage-pod-redux/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-4fdb0f5fa329 · reference
  Indentation
  mods/bitshifter/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-516da110cf84 · reference
  Harmony Patches
  mods/smart-weight-plate/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-55dfe09fb614 · reference
  Yielding to Other Mods
  mods/refined-building/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-5658a6640d6f · reference
  Braces
  mods/detect-any-element/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
blk-568d224900b6 · reference
  Harmony Patches
  mods/detect-any-element/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs → mods/bitshifter · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges
blk-5a4d3f42fdd9 · reference
  Braces
  mods/multichannel-critter-sensor/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs → mods/bitshifter
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 214 further candidate(s) matched and are not listed above (8189 of 8192 bytes; the whole selection is 65277). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/bitshifter mods/jumper-bridges mods/motion-sensor-range mods/multichannel-critter-sensor mods/smart-weight-plate --budget 65277

⚠ 5 record(s) in these 5 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/bitshifter:tags.md, mods/jumper-bridges:tags.md, mods/motion-sensor-range:tags.md, mods/multichannel-critter-sensor:tags.md, mods/smart-weight-plate:tags.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools/TestRunner`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

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
  tools/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
