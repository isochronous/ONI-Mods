# Handoff — OM-010-workshop-review-fixes

**Scope:** `mods/sweep-zones`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-010-workshop-review-fixes/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-010-workshop-review-fixes/units/13-implement.md`

```
# Unit 13 — implement: Sweep Zones review fixes
Ticket OM-010. Read units/11-implement-report.md and units/12-review-findings.md. Touch only mods/sweep-zones/.
Jeremy's decisions:
- M2: set ZoneStore.PlayerCancelled only when the cancelled debris was marked for sweeping at that moment AND sits in a sweep zone. Cancelling digs, buildings, Move To and so on must not tag debris. A zone painted later sweeps everything in it.
- M1: any debris merging into a cancelled pile lifts the cancel (patch the merge step, e.g. Clearable.OnAbsorb or wherever the decompile shows it), so the whole pile is swept again.
Also: pace Marking.PlayerCancelSticks to game time between mark, cancel and check; add ONE test for the merge rule and ONE that cancelling a non-sweep order on zone debris leaves it swept (one case per behaviour). Make README and CHANGELOG state the rule exactly. Partial pickup leaving the rest cancelled is fine as is.

Acceptance criteria:
  - Build only the projects of the mods you touch: `dotnet build <csproj> -c Release -p:ModDeployFolder=none`. Output tail in report. Another unit edits other mods at the same time: do not build the whole solution, do not stage or commit, do not launch the game.
  - Player text short, plain, no hedging. If player-visible strings change, run `python common/tools/Translations/translations.py <mod-folder> --build` and update the translations (write .po with Write/Edit).

Report: File .context/tasks/OM-010-workshop-review-fixes/units/13-implement-report.md. Must contain: per-change summary, test names, build output verbatim. Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0a2a9b61cd78 · reference
  try/catch
  mods/detect-any-element/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-0b448c6c8b2a · reference
  try/catch
  mods/storage-pod-redux/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-0ed545257e4e · reference
  try/catch
  mods/no-magic-fridges/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-4a4a879b8ab3 · reference
  try/catch
  mods/smart-weight-plate/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-5933d2ff5362 · reference
  try/catch
  mods/multichannel-critter-sensor/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-5938243ee768 · reference
  try/catch
  mods/just-fall-already/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-6a2fff2949c6 · reference
  try/catch
  mods/bitshifter/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-96ea55e00f65 · reference
  try/catch
  mods/motion-sensor-range/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-a14958d9cba8 · reference
  try/catch
  mods/refined-building/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-b0bee0097cdc · reference
  try/catch
  mods/salty-surf/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-dc492987de9b · reference
  try/catch
  mods/dont-count-eggs-on-rails/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-dcc0a4ff3762 · reference
  try/catch
  mods/upgrade-planner/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-e2901713e50b · reference
  try/catch
  mods/vent-freeze-fix/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-e5aba18fd731 · reference
  try/catch
  mods/corner-build-anything/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-ee3d8ea21939 · reference
  try/catch
  mods/sweep-zones/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-f04d3902a855 · reference
  try/catch
  mods/natural-backwalls-everywhere/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-f2a71236c859 · reference
  try/catch
  mods/jumper-bridges/.context/standards/error-handling.md#try-catch · names mods/sweep-zones/src/SweepZones/ToolIcon.cs → mods/sweep-zones
blk-0e2beaa499b3 · reference
  Game Components
  mods/multichannel-critter-sensor/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-3c9f317e1942 · reference
  Game Components
  mods/corner-build-anything/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-6da88cee9129 · reference
  Game Components
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-86e1c7e9ad24 · reference
  Game Components
  mods/jumper-bridges/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-9213f30d4abe · reference
  Game Components
  mods/motion-sensor-range/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-9703c0ddcb30 · reference
  Game Components
  mods/bitshifter/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-9fead247823f · reference
  Game Components
  mods/just-fall-already/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-c603763d457b · reference
  Game Components
  mods/sweep-zones/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-c8c4aaafedc9 · reference
  Game Components
  mods/detect-any-element/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-cad0cc439cdc · reference
  Game Components
  mods/salty-surf/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-d12bc1aff49d · reference
  Game Components
  mods/no-magic-fridges/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-e214e0eaebc5 · reference
  Game Components
  mods/upgrade-planner/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-ed35ebb87372 · reference
  Game Components
  mods/vent-freeze-fix/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-f75c36d39e17 · reference
  Game Components
  mods/smart-weight-plate/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-f8667089c191 · reference
  Game Components
  mods/storage-pod-redux/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-f9f0a392308b · reference
  Game Components
  mods/refined-building/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
blk-fd0855e66b70 · reference
  Game Components
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#game-components · names mods/sweep-zones/src/SweepZones/ZoneStore.cs → mods/sweep-zones
mods/sweep-zones:domains/entities.md · reference
  The zone store, paint tool, toolbar icons, hotkey and Harmony patches of Sweep Zones Redux, wit…
  mods/sweep-zones/.context/domains/entities.md · src/SweepZones/** → mods/sweep-zones · linked from domains/glossary.md
mods/sweep-zones:domains/glossary.md · reference
  Glossary for Sweep Zones Redux: abbreviations (U59, PLib, DLC), terms to avoid such as overlay…
  mods/sweep-zones/.context/domains/glossary.md · src/SweepZones/** → mods/sweep-zones · linked from domains/entities.md
mods/sweep-zones:standards/code-style.md · reference
  C# code style for the Sweep Zones Redux mod: formatting, imports, comments, Harmony patch and g…
  mods/sweep-zones/.context/standards/code-style.md · src/** → mods/sweep-zones · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/sweep-zones:standards/error-handling.md · reference
  Error handling and logging rules for the Sweep Zones Redux mod: guarding patches, try/catch use…
  mods/sweep-zones/.context/standards/error-handling.md · src/** → mods/sweep-zones · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/sweep-zones:standards/naming-conventions.md · reference
  Naming rules for the Sweep Zones Redux mod: files, namespaces, patch classes, variables, ModStr…
  mods/sweep-zones/.context/standards/naming-conventions.md · src/** → mods/sweep-zones · linked from standards/code-style.md · linked from standards/error-handling.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/sweep-zones:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
