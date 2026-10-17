# Handoff — OM-002-store-backfills

**Scope:** `mods`, `.context`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-002-store-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-002-store-backfills/units/14-review-fixes.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/natural-backwalls-everywhere/src/NaturalBackwalls/FeaturePatch.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/ModDetection.cs and 4 more → mods
blk-00af2e4182a4 · reference
  When to Comment
  mods/just-fall-already/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods
blk-016318d101d7 · reference
  Yielding to Other Mods
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/Mod.cs → mods
blk-064a58168248 · reference
  Braces
  mods/sweep-zones/.context/standards/code-style.md#formatting/braces · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/Options.cs → mods
blk-0eb8c73385d0 · reference
  Yielding to Other Mods
  mods/no-magic-fridges/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/Mod.cs → mods
blk-13c88b17e86e · reference
  Indentation
  mods/salty-surf/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods
blk-15a99617bbd0 · reference
  When to Comment
  mods/refined-building/.context/standards/code-style.md#comments/when-to-comment · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs, mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs → mods
blk-175c1c6cba09 · reference
  Yielding to Other Mods
  mods/jumper-bridges/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/Mod.cs → mods
blk-183bc6e8000f · reference
  Harmony Patches
  mods/motion-sensor-range/.context/standards/code-style.md#harmony-patches · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/bitshifter/src/Bitshifter/Patches.cs, mods/detect-any-element/src/DetectAnyElement/Patches.cs and 1 more → mods
blk-19b851b334fa · reference
  Yielding to Other Mods
  mods/salty-surf/.context/standards/error-handling.md#yielding-to-other-mods · names mods/bitshifter/src/Bitshifter/Mod.cs, mods/storage-pod-redux/src/StoragePodRedux/Mod.cs → mods
blk-1d2cc304e9de · reference
  Indentation
  mods/detect-any-element/.context/standards/code-style.md#formatting/indentation · names mods/bitshifter/src/Bitshifter/Mod.cs → mods
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 480 further candidate(s) matched and are not listed above (8026 of 8192 bytes; the whole selection is 133258). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for .context mods --budget 133258

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:overview.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
