# Handoff — OM-012-u60-readiness

**Scope:** `mods/ovagros-overhead`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-012-u60-readiness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-u60-readiness/units/03-implement.md`

```
# Unit 03 — implement: Ovagro's Overhead on U60

## Objective
Plan step "Fix per mod" of OM-012. Make mods/ovagros-overhead build and behave correctly against the U60 testing branch (build 762230).

## Scope
- IN: mods/ovagros-overhead (mod and test project). U60 replaced `Crop.CropVal`'s `cropId`/`numProduced` with a `products` list and renamed `PlantAttributes.YieldAmount` to `YieldFactor` (VineBranchCeilingConfig.cs:68,78,82). The ceiling vine should follow vanilla U60 `VineBranchConfig` (the floor vine it mirrors) — compare vanilla's U59 vs U60 VineBranchConfig/VineMotherConfig and carry every relevant change over, not just enough to compile. Then fix any further compile errors in the mod and its tests. Set `minimumSupportedBuild: 762230` in its mod_info.yaml.
- OUT: other mods; common/; launching the game; commits; version/CHANGELOG.
- Acceptance: `dotnet build` of the mod and its test project with zero errors; a short table of vanilla VineBranchConfig U59→U60 changes and how each is reflected in the ceiling vine. Your report must include the actual command output — do not claim success without it.

## Context the packet does not carry
- Decision: U60-only, no U59 compatibility — the crop types changed shape, so this change waits on the held branch until U60 releases (other mods' both-builds fixes ship now).
- Unit 01 report: .context/tasks/OM-012-u60-readiness/units/01-explore-u60-breakage.md (ovagros-overhead section).
- Game DLLs: F:\SteamLibrary\steamapps\common\OxygenNotIncluded (via common/oni-mod.props). Frozen U59 copy: F:\ONI-Frozen\APP. Unit 01's decompile/IL diff tooling and outputs are in the scratchpad: C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\4d0d9dd9-872e-488b-ba91-85a8597f8d13\scratchpad (ILDiff\). Write scripts with the Write tool.
- Build only your projects with `-p:ModDeployFolder=none`; other units edit other mods concurrently.

## Report
- File: .context/tasks/OM-012-u60-readiness/units/03-implement-report.md
- Must contain: files changed with why; the vanilla-change table; build output; what needs an in-game check (yield, harvest, growth).
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-9a58d8528c61 · reference
  Checking what removing a mod leaves in a save
  .context/testing/integration-testing.md#checking-what-removing-a-mod-leaves-in-a-save · names mods/ovagros-overhead/src/OvagrosOverhead.Tests/Removal.cs → mods/ovagros-overhead
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 4 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/ovagros-overhead:domains/README.md, mods/ovagros-overhead:overview.md, mods/ovagros-overhead:standards/README.md, mods/ovagros-overhead:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
