# Unit 04: review of OM-017 (Fridge Thresholds Extended, Smart Storage after a load)

Scope: `git diff main...OM-017-smart-storage-threshold-init -- mods/fridge-thresholds-extended` (6 files), the branch's commit messages, read against `Storage.cs` of Storage Refrigerator Thresholds (2959340381) and decompiled `StorageLockerSmart`, `KMonoBehaviour.Spawn` and `LogicPorts.SendSignal` from the game's Managed folder. Read-only: nothing built, launched or edited.

Verdict: the fix is correct. No critical findings. Two moderate findings (one in production reflection, one in the test), the rest minor.

## Findings

### Moderate

1. **`src/FridgeThresholdsExtended/ThresholdsCompat.cs:95-97`: the `ThresholdsBase.Get` binding is outside any try and does not check `IsStatic` (Pass 1/3).**
   Before this branch, `Delegate.CreateDelegate(typeof(Func<GameObject, object>), get)` ran inside `PatchFreezerLogic`'s try/catch. Now it runs directly in `Apply`. If a later Storage Refrigerator Thresholds turns `Get` into an instance method, or changes its parameters so the delegate does not bind, `CreateDelegate` throws `ArgumentException`. That exception leaves `Apply` and `OnAllModsLoaded`, so the Smart Storage fix, every fridge, Freezer logic and copy settings are all lost, with a stack trace instead of the "skipped: ... not found" warning the brief asks for. The `IsAssignableFrom(get.ReturnType)` check does not cover this case.
   Fix: require `get.IsStatic`, and require that the only parameter is `GameObject`. Wrap the two shared bindings (lines 95-100) in a try/catch that logs a warning and leaves the fields null. The existing `getThresholds == null` and `lastSetFlag == null` checks in `PatchSmartStorage` and `PatchFreezerLogic` then skip each part with its warning.

2. **`src/FridgeThresholdsExtended.Tests/SmartStorage.cs:119-123`: `WriteRoundTrip` installs the fix before it checks that the fix is installed (Pass 5).**
   `RestoreFix(t)` patches both postfixes onto the originals whenever they are missing. The loop after it then asserts `HasFix`, which is always true at that point. Suppose a production regression stops `PatchSmartStorage` from patching, for example by bailing at the missing-member check or at the prefix-assembly check. The test then adds the patches itself, and the "Fridge Thresholds Extended patches ..." assertion passes. Depending on which statics were left null, the round trip either NREs inside OnSpawn, which the game catches and logs, or fails on the port value. Either way the failure is far from its cause.
   Fix: restore only what `WriteRoundTripFixOff` removed. Set a process-wide marker when it unpatches, for example `AppDomain.CurrentDomain.SetData("FTE.SmartStorageFixOff", true)`, which survives a test-assembly reload. Have `RestoreFix` act only when the marker is set, then clear it. In `WriteRoundTrip`, assert `HasFix` on what production installed, and fail with a message naming `PatchSmartStorage` when a postfix is missing and no marker is set.

### Minor

3. **`src/FridgeThresholdsExtended.Tests/SmartStorage.cs:20-23` and `127-139`: the fix can stay off in a reused game, which the class doc rules out (Pass 5).**
   Only `LoadSendsThresholds`'s cleanup and the next `WriteRoundTrip` put the postfixes back. The fix stays off for the rest of the live game in two cases:
   - `-Filter` selects `WriteRoundTripFixOff` alone.
   - The load never reaches the test. The plan logs exactly this: reused game, run-20261126-152950, the game quit at the load.

   The doc's "a reused game never keeps the fix off" is therefore not true. `Fridges.cs` does not depend on these postfixes, so the leak is small.
   Fix: in `WriteRoundTripFixOff`, register a `t.Cleanup` that restores the fix when `Write` did not reach `Session.SavePath = path`, for example with a local `bool saved`. Reword the doc to "LoadSendsThresholds, or the next WriteRoundTrip, puts them back". This pairs with the marker from finding 2.

4. **`src/FridgeThresholdsExtended.Tests/SmartStorage.cs:37`: `FixPatches(TestContext t)` never uses `t` (Pass 4).** Drop the parameter. This also simplifies the call sites at 62, 122 and 132.

5. **`publish/workshop-description.txt:1` and `:9`: the Workshop text says how it behaves and contradicts its own first line (Pass 4, docs).**
   - Line 1 now says it "fixes its Smart Storage Lockers after a load". The same sentence still ends "This mod provides no content of its own, it merely adds compatibility", and a fix is more than compatibility.
   - The list item "after loading a save, its signal follows the sliders right away instead of only once something changes" is the kind of how-it-behaves clause Jeremy cut from this mod's text on 2026-11-09 (memory `workshop-writing-style.md`).
   - Line 1 and line 9 say the same thing twice.

   Fix: keep line 1 as it was on main, so it reads "work with more fridges", and make the list item a single short line, for example "[b]Smart Storage Locker[/b]: right after a load, too." Alternatively, keep the line 1 widening and drop the "merely adds compatibility" clause. Show Jeremy the wording, since he rewrites this text himself.

6. **`README.md:12` and `CHANGELOG.md:5`: "instead of (only) reporting full" undersells the bug (docs).**
   The diagnostic found lockers wrong in both directions: 8 green that should have been red, and 3 red that should have been green. The game's answer is its own hidden Smart Storage hysteresis (activate 100, deactivate 0), which can stay green after the fill has dropped. Players with a locker stuck green would not recognise "reporting only full".
   Fix: name the symptom without the mechanism. For example, the changelog could read "Smart Storage Lockers send the right signal for their thresholds right after a save loads". The README bullet could drop "instead of reporting only full".

7. **Commit `7646900` "OM-017: in-game tests pass, fix-off fails" also moves the `common` submodule pointer (commit style).** The message is terse, but it does not mention the submodule bump, which brings in the OM-018 TestRunner/SaveRunner work. Either split the bump into its own one-line commit, for example "common: Save Runner Moono runtime and ...", or name it in the line. All other commit messages on the branch are single terse lines with only the trailer. `d216dbc` is Jeremy's own, without a trailer, which is fine.

## Checked and found correct (no finding)

- **Double sends.**
  - `KMonoBehaviour.Spawn` sets `isSpawned = true`, then runs `MyAttributes.OnStart`, then `OnSpawn`.
  - In the real order, the thresholds' `[MyCmpGet]` spawns the locker inside the thresholds' Spawn. The locker postfix then sees `Get == null` and does nothing.
  - The thresholds postfix then sees `locker.isSpawned` and sends once.
  - In the reverse order, the thresholds postfix sees `isSpawned == false`, and the locker postfix sends once.
  - Both can fire only if the thresholds registered inside the locker's own Spawn, before its OnSpawn ran. Nothing in the locker's dependency graph references `StorageThresholds`, so that cannot happen. `LockerOnSpawnPostfix` is a guard that does nothing in practice, which is harmless.
- **Fresh construction and copy.**
  - On a fresh build, the same postfix sends the thresholds' answer. That answer equals vanilla's for an empty locker with default settings (100/99, green on high), so the port does not change. `LogicEventSender.SetValue` with the same value is a no-op for the circuit.
  - Copy settings, and building with copied settings, go through the thresholds mod's setters after spawn, which this change does not touch.
  - No misfire.
- **Load transient.** On a load, vanilla's value and then the thresholds' value are written in the same frame, before any logic tick, so no wrong value reaches the circuit.
- **Other hosts.**
  - The thresholds mod adds `StorageThresholds` only in `StorageLockerSmartConfig.DoPostConfigureComplete`.
  - A modded building with `StorageThresholds` but no locker is skipped by `locker != null`.
  - A building with `StorageLockerSmart` but no thresholds is skipped by `Get == null`.
  - A locker subclass that calls `base.OnSpawn` is covered by the declared-method patch.
- **Fridges.** The `ThresholdsBase.OnSpawn` postfix returns at once for `RefrigeratorThresholds` (`IsInstanceOfType`), so it has no interaction with `ThresholdsRefresh` or the Freezer prefix. Moving the `Get`/`LastSetFlag` bindings into `Apply` leaves the Freezer path's behaviour unchanged, apart from finding 1.
- **Harmony order.**
  - `Apply` runs in `OnAllModsLoaded`, after the thresholds mod's `PatchAll` in `OnLoad`, so its prefix check (same assembly as `ThresholdsBase`) sees the prefix.
  - The postfix on `ThresholdsBase.OnSpawn` runs after the original's `LastSetFlag = null`.
  - The `MethodDelegate` call goes through the detour, so the prefix runs.
  - A later thresholds release that fixes this itself only makes the resend a same-value send.
- **Thresholds mod absent.** `Apply` returns on `thresholdsBase == null` with a plain log line and patches nothing. Missing members each skip with a warning naming the member, apart from finding 1.
- **Hysteresis side effects.** The extra `UpdateLogicState` call at spawn uses the same fill, so the serialized `activated` cannot flip. `operational.SetActive` matches what vanilla already does at spawn.
- **Test asserts real behaviour.**
  - The test reaches a locker whose vanilla `activated` is false and whose thresholds say green: 35% on its way down from 60%, with High 50 / Low 20.
  - It asserts the port, and `LastSetFlag == true`, with nothing changed after the load.
  - The fix-off run fails as intended: run-20261126-153024, "expected 1, got 0".
  - Cleanup is registered first in `LoadSendsThresholds`, so it runs on failure too.
  - The fixture path and the round-trip files are restored the same way `Fridges.cs` does it.
- **mod.yaml.** One added sentence, plain, no hedging.

## Passes run

1. **Correctness and logic:** spawn order from decompiled `KMonoBehaviour.Spawn` and `StorageLockerSmart`, the double-send paths, null paths in both postfixes, fresh construction and copy, the hysteresis side effects.
2. **Security and trust boundaries:** no input, secrets or new data paths. Nothing found.
3. **Integration and side effects:** the fridge path and `ThresholdsRefresh`, the Freezer prefix after the binding move, other hosts of `StorageThresholds` and `StorageLockerSmart`, Harmony ordering against the thresholds mod's `PatchAll`, the thresholds mod absent or changed.
4. **Maintainability and clarity:** doc comments against behaviour, naming, the unused parameter. Docs read against `workshop-writing-style.md`, `changelog-style.md` and `no-hedging-language.md`.
5. **Evidence and completeness:** the test assertions against the fix, the fix-off proof, cleanup on failure and leaked state in reused sessions, the build and test evidence in `03-fix-report.md` and `plan.md`. Commit messages checked against `commit-message-style.md`.
