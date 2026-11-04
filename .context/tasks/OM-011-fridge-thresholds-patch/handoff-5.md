# Handoff — OM-011-fridge-thresholds-patch

**Scope:** `mods/fridge-thresholds-extended`, `ONI-Mods.sln`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-011-fridge-thresholds-patch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-011-fridge-thresholds-patch/units/05-review.md`

```
Feature: Fridge Thresholds Extended — review of changes since the first review (units/03-review-report.md)
Ticket: OM-011
Lens: review (read-only; write only your report). Do not launch the game.
Diff: `git diff 1e1d658..HEAD -- mods/ ONI-Mods.sln` on branch OM-011-fridge-thresholds-patch, C:\dev\ONI-Mods.
Changed files:
  - mods/fridge-thresholds-extended/src/FridgeThresholdsExtended/ThresholdsCompat.cs: M1 fix (prefix on Psyko.Freezer.FreezerConfig.DoPostConfigureComplete adds RefrigeratorThresholds ahead of Freezer), clearer "class not found" warning
  - mods/fridge-thresholds-extended/README.md: last line
  - mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/** (new): 5 [OniTest] tests; run 4 passed 5/5 (units/04-test-report.md)
  - ONI-Mods.sln: tests project added
Review focus:
  - M1 fix: no double component with the thresholds mod's AddOrGet; absence-safety and JIT-safety still hold (no foreign type in any signature)
  - Tests: each asserts the behaviour it names (testing-discipline), not just "no exception"; reflection-only access to other mods' types; pacing in game time; no leftover diagnostics that should not ship; the DiscoveredResources workaround is commented for the next reader
  - Tests project matches other *.Tests projects in the repo (csproj shape, sln entries)
Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\05-review-report.md — findings with severity and file:line, each with the fix expected; "no findings" only with the passes run.
Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

**Coverage.** nothing in this store declares coverage of these targets, no record names one of them in its own prose, and no target's own bytes cite one, so nothing matched them BY TARGET. What matched, matched by NODE — the records and retrospective entries that sit in the nodes these targets are inside — so the same list comes back for any path inside those nodes. It is still what there is; it is not a selection for the paths you asked about.

**How that is fixed:** a record claims the paths it governs by declaring `applies_to:` in its frontmatter, and a `##`-or-deeper heading block may declare its own `**Applies to:**` — a block that does gets its own pointer, at the block rather than at the whole document.

blk-2c58c8c2a081 · lesson
  Jumper Bridges heavy-watt joint plates
  .context/retrospectives/icon.md · unpruned lesson in the governing node
blk-2d96fd4e2f71 · lesson
  reopen: shared ends, heavi-watt art, fixture clear
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
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 3 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
