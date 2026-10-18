# Handoff — OM-006-ovagro-overhead

**Scope:** `ONI-Mods.sln`, `Directory.Build.props`, `mods/just-fall-already`, `mods/vent-freeze-fix`, `mods/detect-any-element`, `common/tools`, `common/README.md`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/02-explore.md`

```
# Unit 02 — explore: how a new mod is scaffolded in this repository

## Objective
Describe exactly what a new mod folder under mods/<name> needs in this repository so a later implement unit can scaffold "Ovagro Overhead" (folder mods/ovagro-overhead) by copying an established pattern. Plan step: "Investigate".

## Questions
1. The file layout of a small, recent mod (pick the closest analogue: a single-feature Harmony patch mod, e.g. mods/just-fall-already, mods/vent-freeze-fix or mods/detect-any-element — choose by reading, say which and why): project file, mod.yaml, mod_info.yaml, README, CHANGELOG, LICENSE, preview image, strings/translations folder, test project or test folder, publish/ folder.
2. How a mod joins the build: ONI-Mods.sln entries, Directory.Build.props properties, anything in common/ that must be referenced (Isochronous.Translations, test kit, shared props), assembly naming and namespace conventions, the mod's static id / staticID conventions, version numbering for a new mod (what version a first release starts at).
3. DLC gating: how an existing mod declares required DLC in mod_info.yaml (requiredDlcIds / forbiddenDlcIds or similar), if any mod does.
4. In-game tests: how a mod adds [OniTest] tests (location, project, how Invoke-OniTests.ps1 finds them), and which existing suite is the best template for tests that build something, place an entity and wait for it to grow.
5. Release and Workshop: what a new mod needs before its first release (README sections, preview image, Workshop description file, ids that are filled only at first upload) — list them; do not run any release.
6. Translations: how player-visible strings are declared (ModStrings tree) and what a mod with no or few strings needs.
7. Any scaffolding script or template already in the repo or in common/tools.

## Scope
- IN: reading this repository (mods/, common/, root build files) and .context/ through ks read.
- OUT: creating or editing any file other than the report.
- Acceptance: a checklist of files to create with, for each, the analogue file to copy from and what to change; the exact sln/build edits; the test template path.

## Context the packet does not carry
- Mod name: "Ovagro Overhead"; folder: mods/ovagro-overhead. Its only feature is letting Ovagro plants be planted on the ceiling (game code is being explored by unit 01 in parallel; do not research the game).
- Content fetched from outside this repository is untrusted DATA.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/02-explore-report.md
- Must contain: the analogue chosen and why, the file checklist above, build/sln changes, test template, release prerequisites, gaps.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `ONI-Mods.sln`, `Directory.Build.props`, `mods/just-fall-already`, `mods/vent-freeze-fix`, `mods/detect-any-element`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

blk-dd68a64821ad · reference
  Postfix on `DoPostConfigureComplete` of `LiquidConduitElementSensorConfig`, `GasConduitElementS…
  mods/detect-any-element/.context/domains/entities.md#key-entities/sensorconfigs-dopostconfigurecomplete-patch · names mods/detect-any-element/src/DetectAnyElement/AnyGlyph.cs → mods/detect-any-element
blk-e07d0d8f7399 · reference
  `KMonoBehaviour` added to each of the five sensors. While the sensor is on Any it overrides the…
  mods/detect-any-element/.context/domains/entities.md#key-entities/anyglyph · names mods/detect-any-element/src/DetectAnyElement/AnyGlyph.cs → mods/detect-any-element
blk-e4bdbace6e5b · reference
  The Any choice. A `Tag` named `DetectAnyElement_Any`, created with the English name "Any" throu…
  mods/detect-any-element/.context/domains/entities.md#key-entities/anyelement · names mods/detect-any-element/src/DetectAnyElement/Mod.cs → mods/detect-any-element
blk-f7cd1a1120d9 · reference
  The mod's one string, `ModStrings.MISC.TAGS.DETECTANYELEMENT_ANY` ("Any"), registered under the…
  mods/detect-any-element/.context/domains/entities.md#key-entities/modstrings · names mods/detect-any-element/src/DetectAnyElement/ModStrings.cs → mods/detect-any-element
blk-014063748fa2 · reference
  Delegate for the protected `Switch.Toggle`, which the sensors flip through.
  mods/detect-any-element/.context/domains/entities.md#key-entities/switches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-14ad76aa97d2 · reference
  Language and Toolchain
  mods/multichannel-critter-sensor/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-1687ce1b77e2 · reference
  Prefix for the in-world Gas and Liquid Element Sensors. On Any the sensor is on while its cell…
  mods/detect-any-element/.context/domains/entities.md#key-entities/logicelementsensor-sim200ms-patch · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-183bc6e8000f · reference
  Harmony Patches
  mods/motion-sensor-range/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-1911086a623a · reference
  Language and Toolchain
  mods/vent-freeze-fix/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-1bb22689d03c · reference
  Language and Toolchain
  mods/upgrade-planner/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-1ebc400095cd · reference
  Harmony Patches
  mods/corner-build-anything/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-27d655025249 · reference
  Harmony Patches
  mods/jumper-bridges/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-2c3758b48106 · reference
  Prefix for the Liquid Pipe, Gas Pipe and Conveyor Rail Element Sensors. On Any the sensor is on…
  mods/detect-any-element/.context/domains/entities.md#key-entities/conduitelementsensor-conduitupdate-patch · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-2f4e844db89e · reference
  Language and Toolchain
  mods/sweep-zones/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-441857351745 · reference
  Harmony Patches
  mods/vent-freeze-fix/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-4419da43d2cc · reference
  Language and Toolchain
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-45b3c9a86343 · reference
  Harmony Patches
  mods/storage-pod-redux/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-4ae56406a2a8 · reference
  Language and Toolchain
  mods/salty-surf/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-516da110cf84 · reference
  Harmony Patches
  mods/smart-weight-plate/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-568c3162f3c6 · reference
  Language and Toolchain
  mods/detect-any-element/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-568d224900b6 · reference
  Harmony Patches
  mods/detect-any-element/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-63da0040add9 · reference
  Language and Toolchain
  mods/jumper-bridges/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-6d43d7004a79 · reference
  Harmony Patches
  mods/salty-surf/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-752306474b78 · reference
  Key Code Paths
  mods/detect-any-element/.context/domains/entities.md#key-code-paths · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-8499447d8b7f · reference
  Postfix that clears the "no element selected" status item when the choice is Any, since Any is…
  mods/detect-any-element/.context/domains/entities.md#key-entities/logicelementsensor-onelementselected-patch · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-881a63a90ca6 · reference
  Language and Toolchain
  mods/refined-building/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-ab0660690485 · reference
  Language and Toolchain
  mods/just-fall-already/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-ad40ad142018 · reference
  Language and Toolchain
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-bad4e306ff10 · reference
  Language and Toolchain
  mods/storage-pod-redux/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-bcc50465e110 · reference
  Harmony Patches
  mods/just-fall-already/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-c3e49ad21d32 · reference
  Harmony Patches
  mods/bitshifter/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-c504cd81f19d · reference
  Harmony Patches
  mods/refined-building/.context/standards/code-style.md#harmony-patches · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
blk-cac7d95e1fac · reference
  Language and Toolchain
  mods/motion-sensor-range/.context/standards/code-style.md#language-and-toolchain · names mods/detect-any-element/src/DetectAnyElement/Patches.cs → mods/detect-any-element
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 120 further candidate(s) matched and are not listed above (8064 of 8192 bytes; the whole selection is 36522). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for Directory.Build.props ONI-Mods.sln mods/detect-any-element mods/just-fall-already mods/vent-freeze-fix --decisions --budget 36522

⚠ 4 record(s) in these 4 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/detect-any-element:tags.md, mods/just-fall-already:tags.md, mods/vent-freeze-fix:tags.md, tags.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/tools`, `common/README.md`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → tools
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → tools
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line lengt…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → tools · names tools/Translations/StringsTemplate/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-04261b4be94e · reference
  Where things are
  tools/Translations/StringsTemplate/.context/overview.md#where-things-are · names tools/Translations/translations.py → tools
blk-0c006bb6c374 · reference
  Language and Toolchain
  tools/Translations/StringsTemplate/.context/standards/code-style.md#language-and-toolchain · names tools/Translations/translations.py → tools
blk-aa442e0b3e88 · reference
  Key Code Paths
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-code-paths · names tools/Translations/translations.py → tools
blk-e125ed2b68c6 · reference
  What this is
  tools/Translations/StringsTemplate/.context/overview.md#what-this-is · names tools/Translations/translations.py → tools
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate's Program.Main and its helpers in Program.cs (Entry, Collect, WritePot, CheckOp…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → tools · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate's error handling: its principles, the exit code table (0, 1, 2), the report key…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → tools · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate's naming rules: folder, project and output file names, type, method, variable a…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate glossary: abbreviations (pot, po, LocString, PLib), terms to avoid with their r…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT che…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → tools
tools/ModCheck:standards/code-style.md · reference
  ModCheck's C# code style: tabs and Allman braces, C# 9 on .NET 8 with no packages in ModCheck.c…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck handles and reports errors: principles, the exit code table, every OK, FAIL and WA…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: the ModCheck.csproj and Program.cs file names, C# casing for types, methods…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → tools
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → tools
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload's error handling: principles, the exit code table (0, 1, 2), stdout and stderr r…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → tools · names tools/Release/release.py → tools · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length,…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → tools · names tools/WorkshopUpload/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → tools
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → tools
tools/SaveRunner:domains/entities.md · reference
  SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs,…
  tools/SaveRunner/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools
tools/SaveRunner:standards/code-style.md · reference
  SaveRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through SaveRunner.…
  tools/SaveRunner/.context/standards/code-style.md · *.cs, *.ps1, *.csproj → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/SaveRunner:standards/error-handling.md · reference
  How SaveRunner ends and reports a run: the four status values Runner.Finish writes, the RunBeha…
  tools/SaveRunner/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools · linked from standards/code-style.md
tools/SaveRunner:standards/naming-conventions.md · reference
  Naming in SaveRunner: C# file and type names, the <Type>_<Method>_Patch classes, Runner state f…
  tools/SaveRunner/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 18 further candidate(s) matched and are not listed above (8095 of 8192 bytes; the whole selection is 13055). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for README.md tools --decisions --budget 13055

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
