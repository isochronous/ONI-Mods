# Handoff — OM-011-fridge-thresholds-patch

**Scope:** `mods/vent-freeze-fix`, `mods/no-magic-fridges`, `common`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-011-fridge-thresholds-patch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-011-fridge-thresholds-patch/units/01-research.md`

```
Topic: Workshop mods Storage Refrigerator Thresholds (llunak, 2959340381), Freezer (Psyko, 2618339179), Ronivan's Industrial Revolution (find its Workshop id); Harmony patching across optional mods
Game build: whatever is installed (F:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll)
Ticket: OM-011
Lens: research (read-only). Do not edit repo files except your report. Do not launch the game.

Background (verified earlier this session):
- Storage Refrigerator Thresholds source: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs. It adds a `RefrigeratorThresholds` component (IActivationRangeTarget) and a Harmony prefix on vanilla `Refrigerator.UpdateLogicCircuit` that calls `UpdateLogicState(stored/capacity)`. `OtherMods_Patch` (385-412) adds the component to Psyko's FreezerConfig, but nothing patches `Psyko.Freezer.Freezer.UpdateLogicCircuit` (it copies Refrigerator and sends IsFull & IsOperational). Its UpdateLogicCircuit falls back to calling the modded building's own method by reflection (304-313).
- Decompiled Freezer and thresholds DLL already exist in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\freezer-src and ...\scratchpad\srt-src.

Questions:
1. Which Workshop mod is "Ronivan's Industrial Revolution" (Ronivan publishes several; look in C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\*\mod.yaml and mod_info.yaml, and the mods.json in C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods). Is it installed? Which of its buildings are fridges/cold storage (refrigerator-like: FilteredStorage + logic port)? Give config class names, building IDs, and the component class each uses (vanilla Refrigerator, a copy, or something else) with its UpdateLogicCircuit equivalent. Decompile with ilspycmd into the scratchpad.
2. Does the thresholds mod already list any Industrial Revolution buildings in OtherMods_Patch or elsewhere?
3. For each target building, what exact patch makes the sliders drive the port: what to prefix, how to reach `RefrigeratorThresholds` and `UpdateLogicState` (public? internal? need reflection/AccessTools?), how stored fraction is computed so it matches the vanilla-Refrigerator path in the thresholds mod. If a building doesn't get the component at all, what must the patch add (and does the side screen then work)?
4. How should a patch mod handle any of the three mods being absent: load order (ONI mod load order / mod_info requirements), resolving types by name at runtime (AccessTools.TypeByName) and skipping patches when missing, and patching after the other mods have loaded (e.g. in OnAllModsLoaded / Db.Initialize postfix).
5. oni-moono caveat: Jeremy runs on oni-moono (clone at scratchpad\oni-moono). Note anything in its docs (ARCHITECTURE.md D13/D19/D21 on inlining) that the patch must respect, e.g. callers of the patched method that could inline it.
6. How existing mods in this repo are structured and built (look at one small one, e.g. mods/vent-freeze-fix: csproj, mod.yaml, mod_info.yaml, Directory.Build.props, common/), so the plan can copy the pattern. Note whether any existing mod here already patches another Workshop mod's types optionally.

Decision this research will inform: the patch design and the new mod's skeleton.
Constraints: patch only at runtime; never copy other authors' code into the repo; must not error when a target mod is missing.
Report: write to C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\01-research-report.md — findings with file:line citations, versions checked, a concrete recommended patch per building, and what you verified vs inferred. Keep it under ~150 lines.
Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/vent-freeze-fix`, `mods/no-magic-fridges`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

blk-0e2beaa499b3 · reference
  Game Components
  mods/multichannel-critter-sensor/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-3c9f317e1942 · reference
  Game Components
  mods/corner-build-anything/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-6da88cee9129 · reference
  Game Components
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-86e1c7e9ad24 · reference
  Game Components
  mods/jumper-bridges/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-9213f30d4abe · reference
  Game Components
  mods/motion-sensor-range/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-9703c0ddcb30 · reference
  Game Components
  mods/bitshifter/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-9fead247823f · reference
  Game Components
  mods/just-fall-already/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-c603763d457b · reference
  Game Components
  mods/sweep-zones/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-c8c4aaafedc9 · reference
  Game Components
  mods/detect-any-element/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-cad0cc439cdc · reference
  Game Components
  mods/salty-surf/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-d12bc1aff49d · reference
  Game Components
  mods/no-magic-fridges/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-e214e0eaebc5 · reference
  Game Components
  mods/upgrade-planner/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-ed35ebb87372 · reference
  Game Components
  mods/vent-freeze-fix/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-f75c36d39e17 · reference
  Game Components
  mods/smart-weight-plate/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-f8667089c191 · reference
  Game Components
  mods/storage-pod-redux/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-f9f0a392308b · reference
  Game Components
  mods/refined-building/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-fd0855e66b70 · reference
  Game Components
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#game-components · names mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs → mods/no-magic-fridges
blk-0490b4fe2728 · reference
  Missing member: warn, leave vanilla
  mods/motion-sensor-range/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-154cbec92734 · reference
  Missing member: warn, leave vanilla
  mods/jumper-bridges/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-22a4f096f436 · reference
  Missing member: warn, leave vanilla
  mods/salty-surf/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-249b3eb5bb20 · reference
  Missing member: warn, leave vanilla
  mods/just-fall-already/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-2fe59699dd8e · reference
  Missing member: warn, leave vanilla
  mods/refined-building/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-3246f83ea12b · reference
  Missing member: warn, leave vanilla
  mods/corner-build-anything/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-45da075bd1a7 · reference
  Missing member: warn, leave vanilla
  mods/storage-pod-redux/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-49d91e817362 · reference
  Missing member: warn, leave vanilla
  mods/detect-any-element/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-4c1ca7ab8b1f · reference
  Missing member: warn, leave vanilla
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-4c5784f27e28 · reference
  Missing member: warn, leave vanilla
  mods/upgrade-planner/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-7e940ba69d8c · reference
  Missing member: warn, leave vanilla
  mods/smart-weight-plate/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-8f52e2f92d48 · reference
  Missing member: warn, leave vanilla
  mods/sweep-zones/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-b63008570ba1 · reference
  Missing member: warn, leave vanilla
  mods/no-magic-fridges/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-ca37f1585a91 · reference
  Missing member: warn, leave vanilla
  mods/vent-freeze-fix/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-d4e18f40f032 · reference
  Missing member: warn, leave vanilla
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
blk-f0e4c750c725 · reference
  Missing member: warn, leave vanilla
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#guarding-patches/missing-member-warn-leave-vanilla · names mods/no-magic-fridges/src/NoMagicFridges/Patches.cs → mods/no-magic-fridges
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 45 further candidate(s) matched and are not listed above (8015 of 8192 bytes; the whole selection is 19320). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/no-magic-fridges mods/vent-freeze-fix --decisions --budget 19320

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/no-magic-fridges:tags.md, mods/vent-freeze-fix:tags.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → .
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → .
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line lengt…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → . · names tools/Translations/StringsTemplate/Program.cs → . · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-04261b4be94e · reference
  Where things are
  tools/Translations/StringsTemplate/.context/overview.md#where-things-are · names tools/Translations/translations.py → .
blk-0c006bb6c374 · reference
  Language and Toolchain
  tools/Translations/StringsTemplate/.context/standards/code-style.md#language-and-toolchain · names tools/Translations/translations.py → .
blk-aa442e0b3e88 · reference
  Key Code Paths
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-code-paths · names tools/Translations/translations.py → .
blk-e125ed2b68c6 · reference
  What this is
  tools/Translations/StringsTemplate/.context/overview.md#what-this-is · names tools/Translations/translations.py → .
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate's Program.Main and its helpers in Program.cs (Entry, Collect, WritePot, CheckOp…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → . · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate's error handling: its principles, the exit code table (0, 1, 2), the report key…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → . · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate's naming rules: folder, project and output file names, type, method, variable a…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → . · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate glossary: abbreviations (pot, po, LocString, PLib), terms to avoid with their r…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT che…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → .
tools/ModCheck:standards/code-style.md · reference
  ModCheck's C# code style: tabs and Allman braces, C# 9 on .NET 8 with no packages in ModCheck.c…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → . · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck handles and reports errors: principles, the exit code table, every OK, FAIL and WA…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → . · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: the ModCheck.csproj and Program.cs file names, C# casing for types, methods…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → . · linked from domains/entities.md · linked from standards/code-style.md
blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → .
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → .
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → .
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload's error handling: principles, the exit code table (0, 1, 2), stdout and stderr r…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → . · names tools/Release/release.py → . · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → .
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → .
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length,…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → . · names tools/WorkshopUpload/Program.cs → . · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → .
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → .
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → .
tools/SaveRunner:domains/entities.md · reference
  SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs,…
  tools/SaveRunner/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → .
tools/SaveRunner:standards/code-style.md · reference
  SaveRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through SaveRunner.…
  tools/SaveRunner/.context/standards/code-style.md · *.cs, *.ps1, *.csproj → . · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/SaveRunner:standards/error-handling.md · reference
  How SaveRunner ends and reports a run: the four status values Runner.Finish writes, the RunBeha…
  tools/SaveRunner/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → . · linked from standards/code-style.md
tools/SaveRunner:standards/naming-conventions.md · reference
  Naming in SaveRunner: C# file and type names, the <Type>_<Method>_Patch classes, Runner state f…
  tools/SaveRunner/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → . · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → .
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 19 further candidate(s) matched and are not listed above (7967 of 8192 bytes; the whole selection is 13003). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for . --decisions --budget 13003

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
