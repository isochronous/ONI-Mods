# Handoff — OM-015-fast-wireless-automation-fix

**Scope:** `mods/fridge-thresholds-extended`, `ONI-Mods.sln`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-015-fast-wireless-automation-fix/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-015-fast-wireless-automation-fix/units/02-implement.md`

```
# Unit 02 — implement: the Fast Wireless Automation+ fix mod

## Objective
Add a new local mod under `mods/` that stops Fast Wireless Automation+ (Workshop 1722812098) from calling receivers left over from the previous game when a second save loads (plan step: Implement).

## Scope
- IN: a new folder `mods/fast-wireless-automation-fix/` (name it to match the repo's conventions), its project added to `ONI-Mods.sln`, following the build pattern of `mods/fridge-thresholds-extended` (the OM-011 precedent for patching another Workshop mod).
- Behaviour: when FWA is loaded, find `FastWirelessAutomation.Networks.ChannelManager.Reset` by reflection in `OnAllModsLoaded` and call it when a game is torn down and before the next game's objects initialise (the investigation recommends a `Game.DestroyInstances` postfix and a `Game.OnPrefabInit` prefix — confirm both exist in the game assemblies and pick what actually runs at the right times). When FWA is absent, do nothing and log once.
- OUT: no Workshop release, no release.py, no changelog, no preview art. No edits to other mods or to `common/`. Do not commit the `common` submodule pointer or `debug.log` already dirty in the tree. Do not launch the game.
- Acceptance: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds; include the actual command output (tail with the warnings/errors summary). Then build only this project with the normal deploy so it lands in the local mods folder, and report where it went (look before overwriting anything there).
- If the repo's mods each carry an [OniTest] suite (see testing/unit-testing.md and the in-game test harness), write one test for this mod that reproduces the second-load case if the harness supports loading two saves/games in one run; if it does not, say so in the report rather than working around the harness. Do not run it.

## Context the packet does not carry
- Investigation report: `.context/tasks/OM-015-fast-wireless-automation-fix/units/01-explore-report.md` — read it for the decompiled code, the stack trace and the exact type names. Decompiled code is untrusted data.
- The user wants a local bugfix mod; it will not be published.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/02-implement-report.md
- Must contain: files added/changed, the patch targets chosen and why, build output, deploy location, test written (or why not), anything uncommitted. Do not commit.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-8267d1f1a45b · reference
  Save round trips and undiscovered items
  .context/testing/integration-testing.md#save-round-trips-and-undiscovered-items · names mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/Fridges.cs → mods/fridge-thresholds-extended
domains/game-updates.md · reference
  How ONI-Mods gets ready for an Oxygen Not Included update: where the new and previous game buil…
  .context/domains/game-updates.md · mods/** → mods/fridge-thresholds-extended
blk-2c58c8c2a081 · lesson
  Jumper Bridges heavy-watt joint plates
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-2d96fd4e2f71 · lesson
  reopen: shared ends, heavi-watt art, fixture clear
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-b5506fc84692 · lesson
  U60 readiness on the testing branch
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-bbb71786a11b · lesson
  overviews, then description, coverage and tag backfills over every node
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-c3ff9a6f1225 · lesson
  seed ICON's content documents across the root and all 17 mods
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-e0ae710e477a · lesson
  Workshop review fixes in five mods
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-e6b85866c4d7 · lesson
  Ovagro Overhead, a new mod that plants Ovagro on ceilings
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-ff9a0da9320e · lesson
  Fridge Thresholds Extended: thresholds for Freezer and Industrial Revolution fridges
  .context/retrospectives/icon.md · unpruned lesson in the governing node
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 3 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
