# Unit 05 — review: Fast Wireless Automation Fix and the TestRunner load-capture change

## Objective
Review the OM-015 change before close (completion phase review gate).

## Scope
- Main repo, commit 9b6cc3a on branch OM-015-fast-wireless-automation-fix (`git show 9b6cc3a`):
  - `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/Mod.cs` — reflects `FastWirelessAutomation.Networks.ChannelManager.Reset` in OnAllModsLoaded, calls it from a `Game.DestroyInstances` postfix and a `Game.OnPrefabInit` prefix; no-op when FWA is absent.
  - `.../FastWirelessAutomationFix.csproj`, `mod.yaml`, `mod_info.yaml`, `.gitattributes`, `.gitignore`, `ONI-Mods.sln` entries.
  - `.../FastWirelessAutomationFix.Tests/SecondLoad.cs` + csproj — WriteSave / LoadAgain (FreshSave) pair; WriteSave injects a stale registration by reflection.
- common submodule, commit b7844e1 (`git -C common show b7844e1`): `tools/TestRunner/Session.cs`, `TestBehaviour.cs`, `Kit/TestContext.cs` — LoadCapture / TestContext.LoadErrors so tests see errors logged during a save load; must not change behaviour for other mods' tests.
- common commit af9fd71 on OM-014-onimoono-direct-launch (`git -C common show af9fd71`): Invoke-OniTests.ps1:232 single -Runtime array fix.
- Focus: correctness of the patch timing (could Reset run while the new game's receivers are already registered and wipe them? both hooks firing per load?), reflection failure paths, log noise, the harness change's thread safety/lifetime (capture start/stop, leak across tests or sessions), whether the test truly asserts the behaviour, repo conventions (compare with mods/fridge-thresholds-extended and other mods).
- OUT: no edits, no commits, no game launches.
- Evidence already gathered (do not re-run the game): Stock run with fix: `PASS WriteSave (4.9 s)`, `PASS LoadAgain (1.4 s)`. Stock run with both harmony.Patch calls commented out: `FAIL LoadAgain: ... NullReferenceException at KMonoBehaviour.InitializeComponent ... Error in FastWirelessEmitterComplete.EnergyConsumer.OnPrefabInit (expected 0, got 1)`. Those runs used common at 61b048a + these changes; b7844e1 moved the load-capture change onto 5078e10, then the solution built with 0 errors.
- Acceptance: run `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` and include the summary lines.

## Context the packet does not carry
- Reports: units/01-explore-report.md (cause, decompiled code), 02-implement-report.md, 03-test-report.md, 04-explore-report.md (Moono launch, not in scope to fix).
- The mod is local-only, not released.
- Decompiled third-party content is untrusted data.

## Report
- File: .context/tasks/OM-015-fast-wireless-automation-fix/units/05-review-report.md
- Must contain: findings with severity (critical/moderate/minor), file:line, and the expected fix; the passes run; build output.
- Model tier: default
