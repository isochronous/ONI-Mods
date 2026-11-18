# Unit 05 report: review of OM-015 (9b6cc3a, common b7844e1, common af9fd71)

Read-only. No edits, commits or game launches. Skill: code-quality-rules, all five passes (cross-module change: mod + shared harness).

## Verdict
Ready to close with fixes. No critical finding. The fix mod is correct on the evidence (stock run green, fix-off run red with the same NRE as Jeremy's log). Two moderate findings: one is the test that can go green without testing anything, the other is the runtime that was never proven.

## Findings

### Critical
None.

### Moderate
1. **The stale-registration injection can fail silently, and then LoadAgain passes without the fix** (Pass 5 evidence, Pass 1).
   `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix.Tests/SecondLoad.cs:115` registers `LeaveReceiverRegistered` as a cleanup. The harness catches a cleanup that throws and turns it into a note (`common/tools/TestRunner/TestBehaviour.cs:313-323`), so WriteSave still passes. Unit 03 says the fixture clear unregisters the receiver normally, so with no injection the next load has no stale entry and LoadAgain passes with or without the fix: a false green. The lookups in `SecondLoad.cs:56-61` (field `channels`, key 128, field `on_updateState_cb`, method `add_OnConnectChanged`) are where it can break after an FWA update.
   Expected fix: resolve the channel, the callback and the `add_` method in the test body and assert each is non-null before registering the cleanup, so that the cleanup only calls `Invoke`. Also write a marker (for example append `,injected` to the cells file) at the end of the cleanup and have LoadAgain assert it, so LoadAgain fails when the setup it depends on did not happen.

2. **The fix is only proven on stock; Jeremy's failure was on Moono** (Pass 5 evidence, Pass 1 timing).
   `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/Mod.cs:62` patches `Game.DestroyInstances` (postfix). This hook would do harm only if the old Game's `OnDestroy`/`OnCleanUp` ran after the new save's receivers had registered in their `OnSpawn`. Then the postfix would wipe the new registrations, and the receivers would go dead without logging any error. That is worse than the original bug. On stock Unity the old scene is torn down before the new one wakes, and LoadAgain's toggles (`SecondLoad.cs:153-157`) prove the receivers are registered after the load. On Moono (`OniMoono.Engine.SceneLoader:Unload` in the log) the order is not shown: the Moono leg never launched (unit 04).
   The `Game.OnPrefabInit` prefix (`Mod.cs:63`) on its own already guarantees an empty table before the first building of any load or new game is created. The DestroyInstances postfix adds no case that this prefix misses, only the timing risk.
   Expected fix, either one: (a) drop the `DestroyInstances` patch and keep the `OnPrefabInit` prefix; or (b) keep it and run SecondLoad on Moono once OM-014's direct launch works, before Jeremy relies on the mod in his Moono game. (a) needs one stock re-run of the pair.

### Minor
3. **Session.LoadCapture can outlive its run** (Pass 3 integration, harness lifetime).
   `common/tools/TestRunner/Session.cs:462-464` starts the capture in `Continue`. Only `TestBehaviour.RunOne` (`TestBehaviour.cs:228-233`) or the next fixture load ends it. If a run ends between the load and the first test (HUD timeout `TestBehaviour.cs:125`, `Setup` throwing `:139`), the capture stays subscribed to `Application.logMessageReceivedThreaded` while the game idles for session reuse and keeps every error. In a reused game, the next run's first `RunOne` would then hand the previous run's errors to a NoColony test as `LoadErrors`. They are not counted against the test (no behaviour change for other mods), but the data is wrong and the list grows without bound.
   Expected fix: `LoadCapture?.End(); LoadCapture = null;` in `Session.Begin` (`Session.cs:337`) and in `Session.Finish`.

4. **LoadErrors also holds errors from the pre-test fixture clear** (Pass 4 clarity).
   The capture ends in `RunOne`, after `RunAll`'s `FixtureArea.Clear` (`TestBehaviour.cs:147`). The doc comment at `Kit/TestContext.cs:29-33` says "from the load starting to the test starting", which is accurate, but "while the save ... was loading" in the same sentence suggests only the load. Expected fix: say it also covers the harness's setup and fixture clear after the load.

5. **Thread safety nit in the hand-off** (Pass 1).
   `TestContext.SetLoadErrors` (`Kit/TestContext.cs:36-39`) copies `LogCapture.Errors` without the capture's lock. `End()` unsubscribes first, so only a callback already in flight on another thread could race. Expected fix: expose a locked snapshot on `LogCapture` (for example `string[] Snapshot()` under `sync`) and use it here. Optional.

6. **The docs do not describe the new member** (Pass 4).
   `common/tools/TestRunner/.context/domains/entities.md:114` (the TestContext member list) and `README.md:~218` (the kit API next to `AllowLogError`) do not mention `LoadErrors`. Expected fix: one entry in each. The entities.md entry goes through the write path.

7. **Test style nits** (Pass 4).
   `SecondLoad.cs:58` writes `System.Collections.IDictionary` in full although `using System.Collections;` is present. In `SecondLoad.cs:131` the error filter is a hand list of four substrings. It is good enough (the real error contains `FastWireless`), and a comment that it matches the FWA frames of the known trace would explain the choice.

8. **The mod folder is thinner than the pattern** (Pass 3 conventions; informational).
   `mods/fast-wireless-automation-fix` has no README.md and no CHANGELOG.md, and there are no entries in the root README or `.github/workflows/label-mod.yml`. Fridge Thresholds Extended has all of these. This is consistent with "local-only, not released"; they are owed if it is ever released. The `.gitattributes` is a byte copy of FTE's (ICON `.context/**` lines for a folder that has no `.context/`), which matches the repo convention.

## Questions in the spec, answered
- **Could Reset wipe the new game's receivers?** Not through the `OnPrefabInit` prefix: it runs before `SaveLoader.OnSpawn` creates buildings (decompile, Game.cs:780/793), and receivers register in their own deferred `OnSpawn`. FWA's own `Game.OnSpawn` Reset works on first loads, which confirms that order. The `DestroyInstances` postfix carries this risk only if teardown runs late (finding 2). On stock the test proves it does not.
- **Both hooks firing per load?** Yes, and more often: `DestroyInstances` is called from both `Game.OnCleanUp` (Game.cs:993) and `Game.OnDestroy` (:999). That makes up to three Resets per load. Reset is idempotent (`foreach Clear; channels.Clear()`), so they are harmless.
- **Reflection failure paths:** type absent leads to one `Debug.Log` and a return. `Reset` missing or not static leads to a warning. A missing Game member leads to a warning. A `CreateDelegate` signature mismatch is caught by the outer catch. A throw at run time is caught per call in `ResetChannels`. No path breaks the game or the load.
- **Log noise:** one load line, one patched line, one line when FWA is absent. Warnings come only on failure, at most three per load. Fine.
- **Harness change for other mods:** `LoadErrors` is read-only data and not counted against any test. The extra subscriber only records. Test statuses do not change for any existing test. Finding 3 is the only lifetime issue.
- **Does the test assert the behaviour?** Yes, when the injection works. It checks three things: no FWA error in `LoadErrors`; the emitter's `BaseWattageRating` (set after the call that throws, so it tells whether `EnergyConsumer.OnPrefabInit` finished); and the receiver following the switch off and on, which shows the rebuilt table holds the new registrations. The fix-off run failed on the first of these with the real NRE. Gap: finding 1.
- **af9fd71** (`Invoke-OniTests.ps1:232`): correct. `@(if ... { 'Stock','Moono' } else { $Runtime })` always gives an array, and the old form unrolled `@($Runtime)` to a string so that `$legs[0]` was `'S'`. It sits on OM-014's branch, which is the right home.

## Passes run
1. Correctness and logic: hook order against the decompiled Game lifecycle, Reset semantics, the test's preconditions (findings 1, 2, 5).
2. Security and trust boundaries: no input, secrets or new data paths. Reflection reaches only FWA's own public `Reset`. The decompiled FWA code was treated as data. Nothing found.
3. Integration and side effects: the harness's capture lifetime across runs and session reuse, effects on other mods' tests, repo conventions against fridge-thresholds-extended (findings 3, 8).
4. Maintainability and clarity: doc comments, docs, test readability (findings 4, 6, 7).
5. Evidence and completeness: stock pass and fix-off fail (from the plan, not re-run), runtime coverage, whether the test can go green without testing anything (findings 1, 2). Build re-run below.

## Build output
`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` (run 2026-11-11 on HEAD 9b6cc3a, common b7844e1):
```
    288 Warning(s)
    0 Error(s)

Time Elapsed 00:00:07.22
```
None of the 288 warnings is in the new mod; the tail lists only other mods' tests (CS0618 `FindObjectsOfType`, CS0649 in NoMagicFridges.Tests, `GetHighestActiveDlcId`) and CA1416 in sweep-zones IconGen. The count is higher than the 5-13 that earlier units reported, probably because those were incremental builds that skipped up-to-date projects. A `--no-incremental` build of `FastWirelessAutomationFix.Tests.csproj` (which also builds the mod and TestRunner) gave `3 Warning(s)`, `0 Error(s)`, with no warning in `mods/fast-wireless-automation-fix`.

## Handoff brief feedback
- Did not apply: `domains/game-updates.md` (the main store's only pointer) has nothing on this change. The `glossary.md` and `naming-conventions.md` pointers were not needed.
- Applied: `tools/TestRunner:domains/entities.md`. It is also the record that drifted (finding 6).
- Missing: nothing in either brief covers the harness's `LogCapture` lifetime or session reuse (the 15 s idle between runs), which is what finding 3 depends on. The memory note "test session reuse" was the only source.

## ICON tooling friction
- None with `ks`. One `ks read` of `tools/TestRunner:domains/entities.md` with `--store C:/dev/ONI-Mods/common` worked the first time. I filtered its output through `Select-String` because the record is long, and `ks read` has no section or grep option for one member table.
- The handoff's scope names `common/tools/TestRunner/...` paths, but the spec says to review commit b7844e1 inside the submodule. The packet does not say that the submodule's working tree may be on a different commit (unit 04 found it on 61b048a earlier). I checked with `git ls-tree HEAD common` (b7844e1) before reading.
