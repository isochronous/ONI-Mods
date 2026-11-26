# Unit 03: Smart Storage fix (OM-017)

Nothing was launched and no in-game test was run.

## Commits (branch OM-017-smart-storage-threshold-init)
- `f70e300` OM-017: revert temporary Smart Storage diagnostic logging. This reverts bc2e8f6.
- `69c6de2` Fridge Thresholds Extended: Smart Storage Lockers send their thresholds' signal after a load. This holds the fix and the tests.
- `48db95e` Fridge Thresholds Extended: Smart Storage in the README, changelog and Workshop text.

## Files changed
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended/ThresholdsCompat.cs`: the fix.
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests/SmartStorage.cs`: new, holds the round-trip tests.
- `mods/fridge-thresholds-extended/README.md`:
  - the opening sentence is widened
  - "What it does" has a new Smart Storage Locker bullet
- `mods/fridge-thresholds-extended/CHANGELOG.md`: a new `## Unreleased` section with one bullet.
- `mods/fridge-thresholds-extended/publish/workshop-description.txt`:
  - the first line is widened
  - the list has a new Smart Storage Locker item
- `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended/mod.yaml`: one sentence added to the description.

Translations: the mod has no strings tree and no po files. The only text that changed is in the README, the changelog, the Workshop description and the mod.yaml description, so there was nothing to translate.

## Why the locker spawns first (new finding, stock and Moono alike)
`StorageThresholds` has `[MyCmpGet] private StorageLockerSmart storageLockerSmart`. The spawn runs in this order:
1. `KMonoBehaviour.Spawn()` sets `isSpawned = true`.
2. It then calls `MyAttributes.OnStart(this)` before `OnSpawn()`. This was decompiled from `Assembly-CSharp-firstpass`, `MyAttributeManager<T>.OnStart`.
3. `MyAttributes.OnStart` calls the spawn function on every `[MyCmpGet]` target.
4. So the thresholds component's own Spawn spawns the locker first.
5. `StorageLockerSmart.OnSpawn` then calls `UpdateLogicAndActiveState`. `ThresholdsBase.Get` is still null at that point, so the vanilla body runs and sends the game's own answer.
6. Only after that does `ThresholdsBase.OnSpawn` register the component and set `LastSetFlag = null`.

That matches the log: `comp=present spawned=True registered=False`.

This order does not depend on component order or on the runtime. It happens the same way on stock and on Moono, and on a fresh build as well as on a load. A fresh build shows no wrong value only because an empty locker gives red under both answers. The round-trip test therefore reproduces on stock too.

## How the fix works
All of this is in `ThresholdsCompat.Apply`, which runs from `OnAllModsLoaded`. It binds everything by reflection and has no compile-time reference to the thresholds mod.

**Shared bindings.** `ThresholdsBase.Get` and `LastSetFlag` are now bound once in `Apply`. The Freezer part reuses them instead of binding its own copies.

**`PatchSmartStorage`** does nothing, with a warning in the log, if any of these is missing:
- `StorageRefrigeratorThresholds.StorageThresholds`
- `StorageLockerSmart.UpdateLogicAndActiveState` or `OnSpawn`
- `ThresholdsBase.OnSpawn`, `Get` or `LastSetFlag`
- a prefix on `UpdateLogicAndActiveState` whose method comes from the thresholds mod's assembly

If the thresholds mod is absent, `Apply` returns before this step, as it did before.

**The postfixes.** It binds `UpdateLogicAndActiveState` as an `Action<StorageLockerSmart>` delegate. The delegate goes through the Harmony detour, so the thresholds prefix runs. It then adds two postfixes:
- **`StorageLockerSmart.OnSpawn`:** if the thresholds are already registered, it resends.
- **`ThresholdsBase.OnSpawn`:** this is shared by both thresholds types. It acts only for a `StorageThresholds` whose locker `isSpawned`, and then resends. This is the case that happens in practice.

**The resend** sets `LastSetFlag = null`, then calls the delegate. The thresholds prefix sends only when its result differs from `LastSetFlag`, so clearing it makes the call send.

Whichever of the two OnSpawns runs second sends the thresholds' value, so the fix does not depend on which one runs first. Fridges are not affected: the `ThresholdsBase.OnSpawn` postfix filters on `StorageThresholds`.

**Reuse.** The existing `ThresholdsRefresh` component, from OM-011, works by being added after the thresholds component on the prefab. Smart Storage gets its thresholds from the thresholds mod's own config prefix, so the fix uses a pair of postfixes that does not depend on order. It reuses the reflection bindings and the logging style of `ThresholdsCompat`.

## Refrigerator, with the same mod (checked, not widened)
`RefrigeratorThresholds` also has `[MyCmpGet] Refrigerator`, so every fridge has the same mechanism:
- `Refrigerator.OnSpawn` runs `UpdateLogicCircuit` before the thresholds register.
- The vanilla `IsFull && operational` is sent.
- `LastSetFlag` stays null.

How each case comes out:
- **Fridges FTE adds**, the Buildings list including Freezer: `ThresholdsRefresh` already resends after the thresholds component spawns. It is added last, after both, so the existing fix covers them.
- **Vanilla Refrigerator**, and the Advanced Refrigeration fridges the thresholds mod adds itself: no `ThresholdsRefresh`. Fridges take power, though. At load `EnergyConsumer.IsPowered` is false, so the vanilla signal at spawn is red, the same answer the thresholds give while not operational. The first electrical tick then raises `OperationalChanged`, the prefix runs with `LastSetFlag` null, and it sends the right value. So a vanilla Refrigerator corrects itself once power is evaluated, after unpause. It is not stuck the way a Smart Storage Locker, which takes no power, is.

Left as is, per the brief.

## Build output
`dotnet build mods/fridge-thresholds-extended/src/FridgeThresholdsExtended -c Release`:
```
FridgeThresholdsExtended -> ...\bin\Release\FridgeThresholdsExtended.dll
Deployed mod to C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\local\FridgeThresholdsExtended
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
The Release build deploys, so the fixed FTE now replaces the diagnostic build in `mods\local`.

`dotnet build mods/fridge-thresholds-extended/src/FridgeThresholdsExtended.Tests -c Release`:
- Build succeeded, 0 errors.
- The FTE projects have no warnings.
- The first build showed 3 CS0618 warnings in `common/tools/TestRunner`: `Worldgen.cs` 260 and 293, `TestBehaviour.cs` 219, all `FindObjectOfType` / `FindObjectsOfType` being obsolete. They are in the submodule and were not touched.

ModCheck, run against the game's Managed folder on both DLLs: ALL CHECKS PASSED.

## Test design (`SmartStorage.cs`, class `SmartStorage`, needs `-Steam 2959340381`)
The tests follow the Fridges round-trip pattern: a save in the run folder, `Session.SavePath`, then `FreshSave`. The harness needed no change.

The fix's postfixes are found as `Harmony` patches with owner `Isochronous.FridgeThresholdsExtended`.

1. **`WriteRoundTrip`** (round trip 1/2):
   - It first puts back any fix postfix that is missing, and asserts both are present.
   - It builds a Smart Storage Locker in a `Dupes.Box()` and sets user capacity to 1000 kg.
   - It sets High 50 / Low 20, green on high, through the side screen's properties.
   - It discovers sandstone and ticks it in the filter, so `TreeFilterable` keeps it on load.
   - It fills to 60% and asserts green, then takes the fill down to 35% and asserts it stays green.
   - It saves to `fte-smart-storage.sav` in the run folder.
   - Each step is 1 game second apart, at speed 2.
2. **`WriteRoundTripFixOff`** (Explicit):
   - It removes FTE's two spawn postfixes with `Harmony.Unpatch(..., Postfix, owner)` and asserts they are gone.
   - It then writes the same save. The load therefore runs with no fix, and there is no switch in the production code.
3. **`LoadSendsThresholds`** (round trip 2/2, `FreshSave`), on the reloaded locker:
   - It asserts High 50 / Low 20 and a fill of 35%.
   - It asserts that vanilla's own `activated` is false, so the game's answer is red and differs from the green.
   - After 1 game second it takes a screenshot (`smart-storage-after-load`), asserts the port is 1 and `LastSetFlag == true`, then asserts the port is still 1 two game seconds later.
   - Nothing is stored, removed or changed after the load. With the fix off the port stays at the vanilla 0, so the test fails.
   - Its cleanup puts back any missing fix postfix by name from `ThresholdsCompat`, restores the fixture path and deletes the round-trip files. This keeps the fix from staying off in a game that is reused for the next run.

One representative case is covered: the thresholds say green and vanilla says red. The other direction, vanilla green and thresholds red at 100% green-on-low, runs the same code path, so it was left out.

## Commands (from C:\dev\ONI-Mods; not run, waiting for Jeremy's approval)
Stock:
```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381 -Filter "*SmartStorage.WriteRoundTrip;*SmartStorage.LoadSendsThresholds"
```
Moono:
```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381 -Runtime Moono -Filter "*SmartStorage.WriteRoundTrip;*SmartStorage.LoadSendsThresholds"
```
Fix-off proof. Expected result: `LoadSendsThresholds` FAILS on "sends green with no storage change", with actual 0.
```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381 -Filter "*SmartStorage.WriteRoundTripFixOff;*SmartStorage.LoadSendsThresholds"
```
The globs carry no trailing `*`. That way `*SmartStorage.WriteRoundTrip` does not also select the Explicit `...FixOff` test.

## Not done / notes
- This report is not committed.
- `debug.log`, `mods/injury-report/` and the `common` submodule pointer were already modified or untracked, and were left alone.
- Plan.md was not updated. That is left to the manager.
