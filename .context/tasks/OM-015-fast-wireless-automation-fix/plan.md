## Task: OM-015
## Branch: OM-015-fast-wireless-automation-fix
## Objective: A local (not published) bugfix mod for the Workshop mod "Fast Wireless Automation+". That mod keeps receivers from the previous save in a static table and calls them during the next load; the fault only shows when a second save is loaded in the same session. The fix mod clears or guards that stale state.
## Folder: .context/tasks/OM-015-fast-wireless-automation-fix/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-015-fast-wireless-automation-fix
- **Attempts (current phase)**: 1

## Decisions
- Branched from main, not from the checked-out OM-014 branch: this task is unrelated to OM-014's work.
- Local bugfix mod, as the user asked; precedent is OM-011-fridge-thresholds-patch.

## Key Files
- (to be filled by investigation: the installed mod's DLL location, the static table, the load path that calls stale receivers)

## Phase Handoff Log

### Handoff: investigation → implementation
**Sub-agent outputs**:
- explore unit report (units/01-explore-report.md): Player-prev.log has one FWA error, a NullReferenceException at 22:44:10.288 on the session's second save load: `EnergyConsumer.OnPrefabInit` → `SignalEmitter.OnOperationalChanged` → `ChannelManager.SignalEmit` → `ChannelData.InvokeEvent` → `SignalReceiver.OnUpdateState`, dying on `GetComponent` of a destroyed receiver; the emitter's `EnergyConsumer.OnPrefabInit` is aborted. First load clean. Mod: Workshop 1722812098 "Fast Wireless Automation+ V2.3", `FastWirelessAutomation_OV3.dll` via SuperComicLib ModONI loader. Static table `ChannelManager.channels`; only clear is a `Game.OnSpawn` postfix calling `ChannelManager.Reset()`, which runs after the save's objects init (error 10.288, `-- GAME --` 10.816). Recommended fix: new mod, reflect `FastWirelessAutomation.Networks.ChannelManager.Reset` in `OnAllModsLoaded` (OM-011 pattern, mods/fridge-thresholds-extended), call it from a `Game.DestroyInstances` postfix and a `Game.OnPrefabInit` prefix.
**Reviewer findings**: N/A this phase
**Verification evidence**: stack trace quoted verbatim in units/01-explore-report.md
**Decisions delta**: fix point = reset on teardown and before the next game's init
**Key files delta**: mods/fridge-thresholds-extended (pattern)
**What the next phase needs**: report file above; not proven that `Game.OnPrefabInit` runs before the save's objects init — the `DestroyInstances` postfix covers the case regardless.

## Progress
- [x] Investigate — NRE on second load; stale `ChannelManager.channels`, cleared too late (Game.OnSpawn)
- [x] Update this plan with findings and the fix approach
- [x] Implement the fix mod under mods/ — mods/fast-wireless-automation-fix (FastWirelessAutomationFix 0.1.0), sln build 0 errors (manager re-ran), deployed to mods\Local\FastWirelessAutomationFix; report units/02-implement-report.md
- [ ] Test: load a save, then a second save in the same session ← IN PROGRESS. Correction: unit 02 claimed TestRunner cannot load a second save; it can — `[OniTest(..., FreshSave = true)]` reloads mid-session (precedent: mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/Fridges.cs:346-406, round trip 1/2 and 2/2). Unit 03 wrote the pair (units/03-test-report.md): FastWirelessAutomationFix.Tests/SecondLoad.cs (WriteSave, LoadAgain with FreshSave); sln builds 0 errors. Harness change in common (uncommitted): Session.LoadCapture + TestContext.LoadErrors so load-time errors are visible. WriteSave injects a stale registration by reflection because the fixture clear unregisters receivers normally — the test simulates the leftover, it does not reproduce how the real game leaves it. Run (awaiting go-ahead): `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FastWirelessAutomationFix -Steam 1722812098 -Filter "*SecondLoad.*"` (Stock runtime; unit suggested -Runtime Moono, dropped since the bug is in the real game).
- [x] Moono check dropped: a hand launch of `OniMoono.exe --game` started Moono's window AND the stock game through Steam (Steam asked Jeremy to approve the custom launch); Jeremy killed the stock game. Direct launch is OM-014's to fix. Two OniMoono.exe processes left running at Jeremy's call.
- [x] Commit — common: af9fd71 on OM-014-onimoono-direct-launch (single -Runtime fix, belongs to OM-014's code); b7844e1 on new common branch OM-015-fast-wireless-automation-fix from 5078e10 (load-time error capture). Main repo commit points common at b7844e1. sln rebuilt against b7844e1: 0 errors. Not pushed.
- [ ] Close: review, retro (owed; Jeremy asked to commit first)

## Open Questions / Blockers
- Run 1 failed before launch: `Cannot bind argument to parameter 'Path' because it is null.` Cause: Invoke-OniTests.ps1:232 `$legs = if (...) { @(...) } else { @($Runtime) }` unrolls a one-item array to a string, so `$legs[0]` was 'S' and `$runDir` null for any single-runtime run. Fixed in common (uncommitted): `$legs = @(if ... { 'Stock', 'Moono' } else { $Runtime })`. Run 2 (Stock, runs/run-20261111-180835): `PASS SecondLoad.WriteSave (4.9 s)`, `PASS SecondLoad.LoadAgain (1.4 s)`, `Session done: 2 passed, 0 failed, 0 errors`. Fix-off run (Stock, patches commented out, Mod.cs restored from bytes afterwards and redeployed): `PASS WriteSave`, `FAIL LoadAgain: ... NullReferenceException ... KMonoBehaviour.InitializeComponent ... Error in FastWirelessEmitterComplete.EnergyConsumer.OnPrefabInit (expected 0, got 1)` — the same failure as Jeremy's log, so the test catches the bug. Moono run: `The game did not start within 120 s.` (Invoke-OniTests.ps1:568); OniMoono.exe exists, no game process left; not retried. Unit 04 (units/04-explore-report.md): player never wrote a Player.log, warden log ends "waiting up to 150 s for the game", no crash event; wrong-process-lookup ruled out; never-started vs exited-early open. Checked-out `common` is 61b048a = OM-014's unproven direct-launch implementation (main and OM-014 both record 5078e10), so this was the first direct launch ever and OM-015's harness edits sit on top of OM-014's uncommitted-pointer work. Narrowest next launch (needs Jeremy): OniMoono alone with --game and --log, no Stock session first.
- ICON friction to record in domains/tooling-friction.md through the write path: `ks handoff --scope mods --decisions` filled the brief with duplicate `error-handling.md` copies from every mod; the explore unit used none of them.
- Uncommitted `common` submodule pointer and `debug.log` were in the working tree when this task started; they belong to someone else's work and are not part of this task.

## Constraints
- Local mod only, not released to the Workshop.
- In-game test runs hold Jeremy's PC: narrowest filter, minimal mod loadout.
