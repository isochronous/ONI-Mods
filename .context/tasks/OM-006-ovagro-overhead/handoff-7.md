# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/ovagro-overhead/src/OvagroOverhead`, `mods/vent-freeze-fix/src`, `mods/natural-backwalls-everywhere/src`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/07-implement.md`

```
# Unit 07 — implement: Steps S3 + S4, Harmony patches #1-#9

Steps S3 and S4: the seed-side patches (#1 `PlantableCellQuery.CheckValidPlotCell`, #2 `SeedPlantingStates.CheckValidPlotCell`, #3 `PlantableSeed.TryPlant`, #9 `VineBranch.Instance.SetupRootInformation`) and the plot-side patches (#4 `PlantablePlot.SpawnOccupyingObject`, #5 `PlantablePlot.SetPreview`, #6 `ReceptacleSideScreen.Initialize`, #7 `PlanterSideScreen.SetResultDescriptions`, #8 `PlantablePlot.OnCopySettings`).
Ticket: OM-006

## Files to create/modify
As listed under S3 and S4 in units/04-plan-report.md, in mods/ovagro-overhead/src/OvagroOverhead/ (and Mod.cs if patching is applied there). The design is units/03-design-report.md, "Game methods patched (10)" and "Routing": prefix + finalizer pairs around `CeilingForm.Enter`/`Exit` for #1-#8, a postfix for #9. Use `CeilingForm` as unit 06 built it (units/06-implement-report.md).

## Pattern to follow
The repo's Harmony patch conventions per the packet's pointers and the template mod; `__state` and `Finalizer` precedents named in the design (vent-freeze-fix DebugSave.cs, natural-backwalls-everywhere WorldgenThreadPatch.cs). Confirm every target's exact signature, access and overloads in the decompiled source (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\) before writing its patch.

## Prior reports
units/03-design-report.md, units/04-plan-report.md (S3, S4), units/06-implement-report.md.

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds with no new warnings; the report shows the output tail. Do not claim success without showing it.
- Every patch target resolves at patch time: a target that is private, an overload or a nested type is named so Harmony binds it, and the report lists each target with the signature it binds.
- Every swap is restored on every path, exceptions included.
- No player-visible strings added. Do not launch the game; in-game tests are S4a-S9.
- Commit all created/modified files in one commit on OM-006-ovagro-overhead: `OM-006: route Ovagro seeds to the ceiling form`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/07-implement-report.md
- Must contain: the diff summary, the target/signature table, the build output tail verbatim, the commit hash, every departure from the design and why, anything the game code contradicted, and a "Store reads" section.
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs → mods/natural-backwalls-everywhere/src
blk-01abe9cbf8da · reference
  Conditionals
  mods/motion-sensor-range/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-090e1e892c99 · reference
  Transpiler: count the match
  mods/smart-weight-plate/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-0b1f765c56ad · reference
  Transpiler: count the match
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-101f4f9d4623 · reference
  Transpiler: count the match
  mods/corner-build-anything/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-14482d7feb95 · reference
  Conditionals
  mods/salty-surf/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-1987e6421398 · reference
  Transpiler: count the match
  mods/no-magic-fridges/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-1de4d1f2707d · reference
  Conditionals
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-21e2796e605b · reference
  Transpiler: count the match
  mods/jumper-bridges/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-24f0ce307136 · reference
  Conditionals
  mods/bitshifter/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-3f366bdebdc6 · reference
  Conditionals
  mods/no-magic-fridges/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-4300a4469703 · reference
  Transpiler: count the match
  mods/detect-any-element/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-456be1b4431d · reference
  Conditionals
  mods/vent-freeze-fix/.context/standards/code-style.md#control-structures/conditionals · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-4a6995b5e81f · reference
  Transpiler: count the match
  mods/vent-freeze-fix/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-4c8d79f7826c · reference
  Transpiler: count the match
  mods/bitshifter/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
blk-5dac2e3d8156 · reference
  Transpiler: count the match
  mods/just-fall-already/.context/standards/error-handling.md#guarding-patches/transpiler-count-the-match · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs → mods/natural-backwalls-everywhere/src
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 82 further candidate(s) matched and are not listed above (8179 of 8192 bytes; the whole selection is 30555). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/natural-backwalls-everywhere/src mods/ovagro-overhead/src/OvagroOverhead mods/vent-freeze-fix/src --budget 30555

⚠ 5 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/natural-backwalls-everywhere:tags.md, mods/ovagro-overhead:domains/README.md, mods/ovagro-overhead:overview.md, mods/ovagro-overhead:standards/README.md, mods/vent-freeze-fix:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
