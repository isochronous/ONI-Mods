# Handoff — OM-011-fridge-thresholds-patch

**Scope:** `mods/vent-freeze-fix`, `mods/smart-weight-plate/src/SmartWeightPlate`, `common/oni-mod.props`
**Stores:** `C:/dev/ONI-Mods` (the task's own store), `C:/dev/ONI-Mods/common` — one brief each, for the stores this scope reached.
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-011-fridge-thresholds-patch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-011-fridge-thresholds-patch/units/02-implement.md`

```
Task: OM-011 — Fridge Thresholds Extended (new mod)
Lens: implement. Work on branch OM-011-fridge-thresholds-patch in C:\dev\ONI-Mods. Do not commit; do not launch the game; do not touch other mods or common/.

Design: read C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\plan.md (Decisions + the investigation handoff block) and units\01-research-report.md. Decompiles referenced there are in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad.

Build:
1. New mod at mods/fridge-thresholds-extended, copying the skeleton of mods/vent-freeze-fix (csproj importing $(OniCommon) props/targets, mod.yaml with staticID "Isochronous.FridgeThresholdsExtended", mod_info.yaml, Mod.cs, ModStrings.cs if the pattern requires it, README.md, CHANGELOG.md with a 0.1.0 entry). Version 0.1.0. Add the project to ONI-Mods.sln the way the others are. Skip PLib options (no settings). Only add translations/ if the skeleton requires it; there are no player-visible strings beyond mod.yaml.
2. Compat class applied from UserMod2.OnAllModsLoaded, shaped like mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs:22-58: resolve thresholds types with AccessTools.TypeByName; if missing, log once and return. Group A (IR fridges): for the five DupesRefrigeration config classes, prefix DoPostConfigureComplete(GameObject go) adding RefrigeratorThresholds when absent. Group B (Freezer): prefix Psyko.Freezer.Freezer.UpdateLogicCircuit as described in the report (skip if a prefix owned by a harmony id containing "llunak" already exists), plus OnCopySettings postfix copying SendGreenOnLow/ActivateValue/DeactivateValue from the source object's thresholds component (check the thresholds source C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs:328-344 for the exact members). Try/catch per group, warn on any missing member and leave that target alone. No compile-time reference to any of the three mods; no foreign type in any patch signature or field type. Bind reflection once.
3. README: follow the store's Workshop writing style (short, player-facing: what it does, compatibility; requires Storage Refrigerator Thresholds; works with Freezer and Ronivan's Legacy - Industrial Revolution). No hedging.
4. Match the surrounding code's naming, comment density and idioms.

Verify: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` must succeed with no new warnings from the new project. Copy the relevant output tail into the report.
Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\02-implement-report.md — files created/changed, build output, anything that deviates from the design and why, and what still needs an in-game check.
Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

**2 briefs**, one per store this scope reaches. Each is the selection a brief over that store's share of the scope returns, and each carries its own notices. The budget (8192 bytes) applies to EACH brief on its own, so this section can be as many times that as there are stores.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

### Store `C:/dev/ONI-Mods`

Scope here: `mods/vent-freeze-fix`, `mods/smart-weight-plate/src/SmartWeightPlate`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods`: an id here is this store's own, and the same id can name a different record in another.

blk-00af2e4182a4 · reference
  When to Comment
  mods/just-fall-already/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-15a99617bbd0 · reference
  When to Comment
  mods/refined-building/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-29493d7a7b8a · reference
  When to Comment
  mods/upgrade-planner/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-3d37bcce99a4 · reference
  When to Comment
  mods/storage-pod-redux/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-40ac288e819d · reference
  When to Comment
  mods/motion-sensor-range/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-430105ebb58b · reference
  When to Comment
  mods/vent-freeze-fix/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-4351aee3774b · reference
  When to Comment
  mods/multichannel-critter-sensor/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-5b85e9aaa8d8 · reference
  When to Comment
  mods/smart-weight-plate/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-6090624fb2d7 · reference
  When to Comment
  mods/sweep-zones/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-6e6c7ccc6bba · reference
  When to Comment
  mods/detect-any-element/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-764a3de83887 · reference
  When to Comment
  mods/bitshifter/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-812bb51869b0 · reference
  When to Comment
  mods/salty-surf/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-93242b225403 · reference
  When to Comment
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-9dbadebf4997 · reference
  When to Comment
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-b19160e035af · reference
  When to Comment
  mods/corner-build-anything/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-f3272b570c51 · reference
  When to Comment
  mods/jumper-bridges/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-f989724353cb · reference
  When to Comment
  mods/no-magic-fridges/.context/standards/code-style.md#comments/when-to-comment · names mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods/smart-weight-plate/src/SmartWeightPlate
blk-081c15a29a2f · reference
  Strings (`ModStrings`)
  mods/motion-sensor-range/.context/standards/naming-conventions.md#code-naming/strings-modstrings · names mods/smart-weight-plate/src/SmartWeightPlate/ModStrings.cs → mods/smart-weight-plate/src/SmartWeightPlate
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 77 further candidate(s) matched and are not listed above (8190 of 8192 bytes; the whole selection is 28466). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/smart-weight-plate/src/SmartWeightPlate mods/vent-freeze-fix --budget 28466

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/smart-weight-plate:tags.md, mods/vent-freeze-fix:tags.md. Give them `applies_to` or tags through the write path.

### Store `C:/dev/ONI-Mods/common`

Scope here: `common/oni-mod.props`

Open this store's pointers with `ks read --store C:/dev/ONI-Mods/common`: an id here is this store's own, and the same id can name a different record in another.

**Coverage.** nothing in this store declares coverage of these targets, no record names one of them in its own prose, and no target's own bytes cite one, so nothing matched them BY TARGET. What matched, matched by NODE — the records and retrospective entries that sit in the nodes these targets are inside — so the same list comes back for any path inside those nodes. It is still what there is; it is not a selection for the paths you asked about.

**How that is fixed:** a record claims the paths it governs by declaring `applies_to:` in its frontmatter, and a `##`-or-deeper heading block may declare its own `**Applies to:**` — a block that does gets its own pointer, at the block rather than at the whole document.

blk-2fd7612e80b0 · lesson
  overviews and the description, coverage and tag backfills over common
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-a823ce0beb55 · lesson
  populate-context over common's six tool nodes
  .context/retrospectives/icon.md · unpruned lesson in the governing node
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
