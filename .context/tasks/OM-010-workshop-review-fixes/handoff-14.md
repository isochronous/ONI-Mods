# Handoff — OM-010-workshop-review-fixes

**Scope:** `mods/upgrade-planner/src`, `mods/vent-freeze-fix`, `mods/salty-surf/src`, `mods/just-fall-already/CHANGELOG.md`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-010-workshop-review-fixes/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-010-workshop-review-fixes/units/14-implement.md`

```
# Unit 14 — implement: minor review fixes in four mods
Ticket OM-010. Read units/10-review-findings.md (minor findings). Touch only mods/upgrade-planner, mods/vent-freeze-fix, mods/salty-surf, mods/just-fall-already.
- Upgrade Planner: add the active world id to the highlight cache key so switching asteroids refreshes at once.
- Vent Freeze Fix: player text says "spawns": use player words instead (e.g. "is built or a save is loaded") in README, Workshop description, mod.yaml, tooltip + translations; fix the README sentence where "It" now reads as the vent; add a short CHANGELOG bullet under Unreleased saying the description now matches what the watcher does. Text as is otherwise (Jeremy kept the behaviour).
- Salty Surf: make the separator comment exact; make the test's Properties-line check language-independent (no English prefix).
- Just Fall Already: split the CHANGELOG bullet into two, one per change. No test for the log gate is needed.

Acceptance criteria:
  - Build only the projects of the mods you touch: `dotnet build <csproj> -c Release -p:ModDeployFolder=none`. Output tail in report. Another unit edits other mods at the same time: do not build the whole solution, do not stage or commit, do not launch the game.
  - Player text short, plain, no hedging. If player-visible strings change, run `python common/tools/Translations/translations.py <mod-folder> --build` and update the translations (write .po with Write/Edit).

Report: File .context/tasks/OM-010-workshop-review-fixes/units/14-implement-report.md. Must contain: per-mod change summary, build and translations output verbatim. Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/upgrade-planner/src/UpgradePlanner/Compat/BlueprintsCompat.cs, mods/upgrade-planner/src/UpgradePlanner/Plan/Presets.cs, mods/upgrade-planner/src/UpgradePlanner/Tool/PlacementQueue.cs → mods/upgrade-planner/src
blk-0b8e3ef08773 · reference
  Example
  mods/vent-freeze-fix/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-0fe521f2b16f · reference
  Example
  mods/upgrade-planner/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-32ae0ce82848 · reference
  Example
  mods/bitshifter/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-353f3e62bd46 · reference
  Example
  mods/multichannel-critter-sensor/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-431be121fc8c · reference
  Example
  mods/corner-build-anything/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-5379b7234d12 · reference
  Example
  mods/motion-sensor-range/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-5b62e7507604 · reference
  Example
  mods/just-fall-already/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-6a8bea597750 · reference
  Example
  mods/detect-any-element/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-7261503b82df · reference
  Example
  mods/storage-pod-redux/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-8ce83687722f · reference
  Example
  mods/sweep-zones/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-94c9c06009e9 · reference
  Example
  mods/refined-building/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
blk-b243414742bb · reference
  Example
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf/src
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 55 further candidate(s) matched and are not listed above (8109 of 8192 bytes; the whole selection is 23194). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/just-fall-already/CHANGELOG.md mods/salty-surf/src mods/upgrade-planner/src mods/vent-freeze-fix --budget 23194

⚠ 4 record(s) in these 4 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, mods/salty-surf:tags.md, mods/upgrade-planner:tags.md, mods/vent-freeze-fix:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
