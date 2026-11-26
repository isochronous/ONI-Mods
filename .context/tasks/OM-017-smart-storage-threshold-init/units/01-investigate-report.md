# OM-016 unit 01: investigation report

Read-only, code reading only. Nothing run, game not launched.

Decompiled sources are in
`C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\04af19fb-3ec8-4afc-a540-f94f8f19cc3a\scratchpad\om016\`:
- `acs\`: all of Assembly-CSharp
- top-level `*.cs`: the single types (StorageLockerSmart, FilteredStorage, LogicPorts and others)
- `srt\`: Storage Refrigerator Thresholds DLL (matches `Source\Storage.cs`)
- `sq\`: Storage Quotas
- `cb\`: Customize Buildings
- `pe\`: Piped Everything
- `ir\`: Industrial Revolution
- `jb\`: Jumper Bridges
- `re\`: Rocketry Expanded
- `ft\`: Fixes and Tweaks
- `bp\`: Blueprints
- `blo\`: Better Logic Overlay
- `mth\`: Move This Here
- `spr\`: Storage Pod Redux
- `moono\`: oni-moono start-order sources, fetched from GitHub

## Summary

- Smart Storage does not have the OM-011 fridge bug. The thresholds component is added before `StorageLockerSmart`, so it is registered before the locker sends its first signal after load.
- No other enabled mod writes the Smart Storage port or patches its logic. Storage Quotas is not the cause.
- From code, the only time a loaded locker shows a different output from the one it saved is the window before power is evaluated. During that window the port reads red on every locker, vanilla included (inferred: the window lasts while the game is paused after load).
- Because the mod sends only when its result changes, any refresh that corrects the port must have changed one of its inputs: fill across a threshold, power, the threshold values, the mode or the capacity.

## 1. How Storage Refrigerator Thresholds drives Smart Storage

Source: `mods\Steam\2959340381\Source\Storage.cs`. The shipped DLL decompiles to the same code.

**Component.** The mod adds `StorageThresholds : ThresholdsBase` (a KMonoBehaviour that implements `IActivationRangeTarget` and `ISim4000ms`) in a prefix on `StorageLockerSmartConfig.DoPostConfigureComplete`:

```csharp
[HarmonyPrefix] [HarmonyPatch(nameof(DoPostConfigureComplete))]
public static void DoPostConfigureComplete(GameObject go) { go.AddOrGet<StorageThresholds>(); }   // Storage.cs:271-275
```

It is a prefix, so the component comes before the `go.AddOrGet<StorageLockerSmart>()` in the vanilla body. The component order on the prefab is:
1. the `BuildingLoader.CreateBuildingComplete` components, including `Operational`, `EnergyConsumer` and, last, `LogicPorts` from `GeneratedBuildings.InitializeLogicPorts`
2. `StorageThresholds`
3. `Storage`
4. `StorageLocker` / `StorageLockerSmart`

Storage Quotas also adds `StorageQuotaController` before all of these.

**Serialized state** (`ThresholdsBase`, Storage.cs:23-38):
- `sendGreenOnLow`
- `activateValue`, default 100
- `deactivateValue`, default 99
- `activated`: the hysteresis state

KSerialization walks base types (`SerializationTemplate` uses `type.BaseType`), so these private base fields are saved.

**Not serialized:** `public bool? LastSetFlag` (Storage.cs:40). It is the last value the mod sent to the port. `OnSpawn` resets it:

```csharp
protected override void OnSpawn() { base.OnSpawn(); fastMap[gameObject] = this; LastSetFlag = null; UpdateLogicPortTooltip(); }  // Storage.cs:121-127
```

`OnSpawn` does not send a signal. The first signal after load comes from vanilla `StorageLockerSmart.OnSpawn`, which calls `UpdateLogicAndActiveState()` after it subscribes to `OnStorageChange` (-1697596308) and `OperationalChanged` (-592767678).

**Signal.** A prefix on the private `StorageLockerSmart.UpdateLogicAndActiveState` replaces the vanilla body (Storage.cs:245-264):

```csharp
ThresholdsBase component = ThresholdsBase.Get(__instance.gameObject);   // static fastMap, filled in OnSpawn
if (component == null) return true;                                     // vanilla body runs
float stored = getAmountStoredMethod(___filteredStorage);              // storage.MassStored()
float capacity = getMaxCapacityMethod(___filteredStorage);             // min(capacityKg, UserMaxCapacity) - storageFullMargin
bool isOperational = component.IsActuallyOperational(___operational);
bool flag = component.UpdateLogicState(stored / capacity) && isOperational;
if (flag != component.LastSetFlag) { ___ports.SendSignal(FilteredStorage.FULL_PORT_ID, flag ? 1 : 0); component.LastSetFlag = flag; }
___filteredStorage.SetLogicMeter(flag);
___operational.SetActive(isOperational);
return false;
```

`UpdateLogicState` (Storage.cs:105-119) rounds the fill to a whole percent:
- Normal mode: `activated` flips on at the High value or above, and off at the Low value or below.
- Green-on-low mode: the same with the comparisons reversed.

Every call updates `activated`, including calls made while the locker is unpowered.

The mod sends only when its result differs from `LastSetFlag`. Vanilla sends on every call: StorageLockerSmart.cs:129, `ports.SendSignal(...)` with no comparison.

**Entry points into the prefix.** All of these call `UpdateLogicAndActiveState`, so all go through the prefix:
- `StorageLockerSmart.OnSpawn`
- `UpdateLogicCircuitCB`, on `OnStorageChange` and `OperationalChanged`
- the `UserMaxCapacity` setter: the capacity slider, or Copy Settings
- the vanilla `ActivateValue` / `DeactivateValue` setters
- the mod's setters, through `StorageThresholds.UpdateLogicCircuit`: `SendGreenOnLow`, `ActivateValue`, `DeactivateValue`
- `ThresholdsBase.Sim4000ms`, inside rocket interiors only

**Which sliders the player sees.** Vanilla `StorageLockerSmart` now implements `IActivationRangeTarget` too. It has its own `activateValue`, `deactivateValue` and `activated`, which the prefix makes unused. `ActiveRangeSideScreen.SetTarget` takes `GetComponent<IActivationRangeTarget>()`. `StorageThresholds` comes first on the prefab, so the sliders are the mod's. Selecting the building writes nothing: `SetTarget` removes the listeners before it sets the slider values, and `CapacityControlSideScreen.SetTarget` does not write `UserMaxCapacity`.

## 2. The load path, and what "refresh" can be

**Load sequence for a powered locker on the ground**, from code:

1. **Deserialize.**
   - `Storage.Deserialize` stores every item with `block_events: true`.
   - `PrimaryElement` units load.
   - `StorageLocker.userMaxCapacity` loads.
   - `ThresholdsBase.activated` and the thresholds load.
   - `LogicPorts.serializedOutputValues` loads the port value from save time.
2. **`EnergyConsumer.OnPrefabInit`** (line 103) runs `IsPowered = false`. The powered flag is false and `Operational.IsOperational` is false.
3. **`LogicPorts.OnSpawn`.** It calls `CreatePhysicalPorts()`, which restores the saved value to the `LogicEventSender`.
4. **`StorageThresholds.OnSpawn`.** It registers in `fastMap` and sets `LastSetFlag = null`.
5. **`StorageLockerSmart.OnSpawn`.** It subscribes to the two events, then the prefix runs:
   - The fill is correct: the items are already in `items`.
   - `IsActuallyOperational` is false: the only false flag is `powered`.
   - So the flag is false. `LastSetFlag` is null, so the mod sends 0 and `LastSetFlag` becomes false.
   - The port is now red whatever `activated` holds.
6. **First electrical sim tick.** `EnergyConsumer` sets `IsPowered = true` and `Operational.UpdateOperational` raises `OperationalChanged` (Operational.cs:191). The prefix computes `activated && true`. If that is true it differs from `LastSetFlag`, so the mod sends 1.

Each step re-evaluates the saved `activated` against the same fill, so `activated` does not change. The output after step 6 is the output at save time.

The mod is ordered correctly. Even if the locker spawned before the thresholds component, the reverse of OM-011, the vanilla body would send 0 at spawn (unpowered). The first mod update at step 6 would still send, because `LastSetFlag` is null.

**Start order on oni-moono.** The source on GitHub was checked: `StartOrderTests.cs` and `BehaviourManager.RunStarts`. Behaviours without `Update` start in the order they were created and enabled, which on one GameObject is component order. Behaviours with `Update` start first. None of `LogicPorts`, `StorageThresholds`, `StorageLockerSmart` or `Operational` declares `Update`, so the order above holds on Moono. Moono documents this as measured against stock Unity.

**What can leave the output wrong, from code:**

- **(A) The window before power is evaluated.** Between steps 5 and 6, every Smart Storage Locker reports red. The value restored from the save is overwritten at step 5. Vanilla lockers behave the same way: `activated && operational.IsOperational`, sent at spawn.
  - Inferred: the game resumes paused after a load and the electrical sim does not run while paused, so the window lasts until the player unpauses. Unpausing would look like "a refresh".
  - Downstream automation that latches can keep the wrong state after the locker recovers: memory toggles, filter or buffer gates, and the installed Edge Detectors and Diode mod. The mod's own comment mentions this false alarm on game start (Storage.cs:17-20).
- **(B) Another writer on the port after step 6.** The mod would not repair this, because it resends only when its own result changes.
  - `LaunchableRocketCluster.UpdateLogicPorts` and `ReorderableBuilding.OnMove` recreate ports, but only on rocket modules.
  - The vanilla body would be another writer if `ThresholdsBase.Get` returned null, but it does not after spawn.
  - No mod writes this port (section 3).
  - So no path for (B) exists in code for a locker on the ground. If (B) happens, the cause is outside code reading, such as runtime inlining.
- **(C) Rocket interiors.** `IsActuallyOperational` ignores `rocketUsageAllowed`. A change in power while the locker is restricted raises no `OperationalChanged`, and only `Sim4000ms` repairs it: a wrong output that lasts at most 4 seconds.

**Deduction about "refresh".** Each entry point re-runs `UpdateLogicState` and sends only on a change. So a refresh can correct the port only if the computed result changed. The inputs are:
- the fill crossing a threshold
- power or operational state, for example unpausing so that step 6 runs
- High, Low or the green-on-low mode, through the slider, the number input or the checkbox
- the capacity slider
- Copy Settings

Re-selecting the locker or opening its side screen changes nothing. A small delivery between Low and High corrects the port only if power state also changed. To pin the mechanism, ask which refresh the player used:
- "unpause" or "wait a moment" means (A)
- "moved a slider" or "toggled the checkbox" means something changed the computed result

## 3. Other enabled mods (Spaced Out list in mods.json)

DLLs were searched for `StorageLockerSmart`, `UpdateLogicAndActiveState`, `FULL_PORT_ID`, `CreatePhysicalPorts`, `IActivationRangeTarget` and `StorageLockerSmartConfig`. The suspects were then decompiled. The `[Moono] mod overlaps` table in Player.log lists only overlaps between Moono tweaks and mods, and none of them touch these methods. Moono logged no inlining rebuild for `UpdateLogicAndActiveState`.

| Mod | What it patches | Effect on the Smart Storage port |
|---|---|---|
| Storage Quotas 0.1.38 (3781549526) | Prefix on `BuildingLoader.CreateBuildingComplete` (adds `StorageQuotaController`), prefix and postfix on `FilteredStorage.OnFilterChanged` (fetch routing), and a prefix on `FilteredStorage.IsFull` (PatchBootstrap.cs:22-42). The `IsFull` prefix is labelled the "smart storage signal". | None. In this game build only `Refrigerator.UpdateLogicCircuit` calls `FilteredStorage.IsFull` (Refrigerator.cs:104). Neither vanilla `StorageLockerSmart` nor the thresholds prefix calls it, so the quota "smart signal" never reaches a locker port. The controller never sends signals. It drops items only on explicit drop requests. Not the cause. |
| Customize Buildings (1818138009) | Prefix on `Assets.AddBuildingDef`. `LockerSmartKG`, default 400,000 kg, sets `Storage.capacityKg` on the prefab. | Changes the base of the percentage, but in the same way before and after a load. Not load-related. |
| Piped Everything (3033409170) | Adds a solid output port (`ConduitDispenserOptionalSolid`) to StorageLockerSmart. | Removing items raises `OnStorageChange`, a normal recompute. Not load-related. |
| Jumper Bridges (local) | Prefix on `LogicPorts.CreatePhysicalPorts`. | Skips only its own bridge buildings. |
| Rocketry Expanded (2837919908) | `UpdateLogicAndActiveState` belongs to its own `CargoBayStatusMonitor`. | None. |
| Industrial Revolution (3557584850) | Adds `RefrigeratorThresholds` to its fridges. `CabinetFrozen` uses `StorageLockerSmart` without thresholds. | None on vanilla lockers. |
| Fixes and Tweaks, Blueprints, Better Logic Overlay | Copy-settings groups, applying blueprint values, overlay display. | None at load. |
| Move This Here, Storage Pod Redux | Reference `FULL` only on their own buildings. | None. |

## 4. OM-011 compared, and a fix proposal

**Not the same mechanism.**
- OM-011: the thresholds component was added to the finished prefab after the fridge's component. The fridge's first `UpdateLogicCircuit` ran with `ThresholdsBase.Get == null`, so it sent the vanilla `IsFull` value until the next fill or power change.
- Fridge Thresholds Extended fixed it with `ThresholdsRefresh`, a component added after the thresholds component whose `OnSpawn` calls `UpdateLogicCircuit` again.
- For Smart Storage the thresholds component is already ahead, and `LastSetFlag == null` forces a correct send at the first update that has power. A `ThresholdsRefresh`-style component would only repeat what step 5 already does.

**Proposal.** First confirm which case it is, then make the matching change.

1. **Confirm with a short diagnostic build**, logs only, not shipped. Add a postfix on `StorageLockerSmart.UpdateLogicAndActiveState`. Postfixes run even when the thresholds prefix skips the original. For every locker with a `ThresholdsBase`, log:
   - fill / capacity and the rounded percent
   - `activated` and `LastSetFlag`
   - `IsOperational` and `EnergyConsumer.IsPowered`
   - `ports.GetOutputValue(FilteredStorage.FULL_PORT_ID)`
   - the calling event: spawn, storage change or operational change

   Load Jeremy's save on Moono and record the paused state, the state after unpausing and the state after his refresh.
2. **If it is (A)**, the red before power, keep the saved port value until power has been evaluated. Add a Harmony prefix on `StorageLockerSmart.UpdateLogicAndActiveState` with a priority above the thresholds prefix, using `HarmonyPriority`.
   - When it skips: `ThresholdsBase.Get(go)?.LastSetFlag == null`, the building has an `EnergyConsumer` whose `IsPowered` is false, and the only false `Operational` flag is `EnergyConsumer.PoweredFlag`.
   - What it does then: `return false`, so neither the thresholds body nor vanilla runs. The port keeps the value `LogicPorts.CreatePhysicalPorts` restored from `serializedOutputValues`, and `LastSetFlag` stays null.
   - The next `OperationalChanged` sends the real value.
   - A new building has no saved value, so its port stays at the 0 that `CreatePhysicalPorts` sets.
   - The same prefix on `Refrigerator.UpdateLogicCircuit` gives fridges the same benefit.
3. **If it is (B)**, a port that differs from `LastSetFlag`, add a self-repair to the same postfix: when `ThresholdsBase.Get(go)` exists and `LastSetFlag` has a value that differs from `ports.GetOutputValue(FULL_PORT_ID) != 0`, call `ports.SendSignal(FULL_PORT_ID, LastSetFlag ? 1 : 0)`. Optionally also schedule one recompute one frame after spawn, with `GameScheduler.Instance.ScheduleNextFrame` setting `LastSetFlag = null` and calling `UpdateLogicAndActiveState`.

**Where it goes.** Either works:
- Fridge Thresholds Extended, which already binds `ThresholdsBase` by reflection in `OnAllModsLoaded`. Reuse `getThresholds` / `lastSetFlag` from `ThresholdsCompat.Apply`, `mods\fridge-thresholds-extended\src\FridgeThresholdsExtended\ThresholdsCompat.cs`. That widens the mod's scope beyond fridges.
- A new small patch mod with the same reflection-only shape. `UpdateLogicAndActiveState` is private, so patch it by name: `AccessTools.Method(typeof(StorageLockerSmart), "UpdateLogicAndActiveState")`.

## Inferred, not read from code

- The game resumes paused after a load and the electrical sim does not run while paused, so the red window lasts until unpause. This is general game knowledge; neither `SpeedControlScreen` nor `CircuitManager` was traced.
- Moono's start order for these components matches stock Unity. This comes from Moono's own tests, not from a run.
- Storage Quotas was written for an older build where the Smart Storage signal called `FilteredStorage.IsFull`.
- Downstream latching gates as the reason the wrong state seems to last.
- Which refresh Jeremy used is unknown.

## Follow-up 2026-11-26

Jeremy's input: moving a slider is what fixes the wrong output, and Smart Storage Lockers have taken no power since U60. Still code reading only.

### Build checked
- `KleiVersion.ChangeList = 764025u` in the decompiled `Assembly-CSharp.dll`.
- Jeremy's Player.log line 165: `Build: U60-764025-SCRPAN (release)`.
- `appmanifest_457140.acf` shows BetaKey `public`.
- So the decompile is the build he runs: U60 on the public branch.

### Corrections to the original report
- **Smart Storage takes no power in this build.** `StorageLockerSmartConfig.CreateBuildingDef` sets neither `RequiresPowerInput` nor `EnergyConsumptionWhenActive`, so `BuildingLoader` adds no `EnergyConsumer`. I had assumed power without checking.
  - Inferred: the thresholds mod's `ReducedSmartStorageBinRequiredPower` option, which sets `EnergyConsumptionWhenActive = 20`, has no effect.
- **What is withdrawn:** section 2 steps 2 and 6, case (A) "red until power is evaluated", the summary line about it, and fix option 2.
- **What is replaced:** the "Deduction about refresh" paragraph, and fix options 1 and 3. They are restated below.

### 1. Operational flags without power
- `Operational.OnPrefabInit` runs `UpdateOperational()` on an empty flag set. That sets `IsOperational = true`.
- The only flags a ground locker gets:
  - `Structure.notEntombedFlag`: true unless the locker is entombed.
  - `BuildingHP` `healthy`: true unless the locker is broken.
  - `RocketUsageRestriction.rocketUsageAllowed`: only in rocket interiors.
- No `LogicOperationalController`: `AddLogicPowerPort = false`.
- So `IsActuallyOperational` is true at spawn and stays true. After load, the result is `UpdateLogicState(fill)` alone.

### 2. What a slider change does
- **The call chain:**
  1. `ActiveRangeSideScreen.OnActivateValueChanged` sets `target.ActivateValue = new_value`. The target is `StorageThresholds`, the first `IActivationRangeTarget` on the object. The Low slider and the number inputs work the same way through `DeactivateValue` and `onEndEdit`.
  2. The setter is `ThresholdsBase.ActivateValue`: `activateValue = (int)value; UpdateLogicCircuit();` (Storage.cs:56-67).
  3. `StorageThresholds.UpdateLogicCircuit` calls the original `StorageLockerSmart.UpdateLogicAndActiveState` through `AccessTools.MethodDelegate`, so it reaches the Harmony prefix.
  4. The prefix runs the same code as at spawn and on a storage change.
- **It does not reset `activated`.** It re-runs the hysteresis against the new threshold. A drag passes through every intermediate value:
  - dragging High down past the fill turns `activated` on, and dragging back leaves it on
  - dragging Low up past the fill turns it off
- **It does not force a send.** It sends only when the result differs from `LastSetFlag`.
- **Nothing else runs on a slider change.** There is no other side effect.
- **Inputs at spawn:** the saved `activated` and the fill. The prefix writes the port at spawn (`LastSetFlag` is null) and keeps it in step after that.
- **Every other path** runs the same prefix with the same send condition. Those paths are a storage change, the capacity setter, Copy Settings, and the green-on-low checkbox, which changes the mode.
- **What a slider fix proves.** For a slider to fix the output, the slider path must send. It sends in exactly two cases:
  - **(a)** `LastSetFlag` is still null, so any slider nudge sends, even one that changes nothing else.
  - **(b)** The drag changes `activated`, which only happens when the slider passes the current fill.

### 3. Which state stays wrong after load
- **Writers on the port:** in U60, vanilla `StorageLockerSmart` writes the FULL port only in `UpdateLogicAndActiveState`. It has no other method, no sim or logic update, and no OnSpawn path of its own that sends.
  - `StorageController` is animation only.
  - Ports are recreated only for rocket modules.
  - No mod writes the port (section 3 still holds).
  - So "LastSetFlag matches but the port holds something else" has no writer in code.
- **Candidate 1: the vanilla body runs at spawn.** This happens if, when `StorageLockerSmart.OnSpawn` runs, `ThresholdsBase.Get(go)` returns null, or the prefix is bypassed.
  - What the vanilla body does:
    - It sends unconditionally, using vanilla's own U60 hysteresis: `activateValue` (default 100), `deactivateValue` (default 0) and `activated`. That is a plain "full" signal unless those vanilla fields were changed.
    - It leaves the mod's `LastSetFlag` null.
  - With no power events, the mod runs next only on a storage change or a mod setter. That first mod call always sends, because `LastSetFlag` is null.
  - Result: wrong until a slider nudge, or until any delivery or removal. Lockers whose contents do not change stay wrong. This matches what Jeremy describes, and it is the OM-011 fridge symptom.
  - How it could happen:
    - The thresholds component spawns after the locker. This contradicts the prefab order and Moono's start-order rules for components without `Update`, so it would need a runtime difference.
    - The .NET JIT on Moono inlines the original body into `StorageLockerSmart.OnSpawn`. This is inferred as unlikely given the size of the method. Moono logged no dependent rebuild for it, and Player.log has no line naming `UpdateLogicAndActiveState`.
  - Neither cause is proven by code.
- **Candidate 2: `activated` is wrong after load,** with the fill between Low and High. This fits case (b).
  - From code, `activated` is saved and restored correctly. `SaveLoadRoot` saves every component that is not `[SkipSaveFileSerialization]`, matched by type name, and KSerialization's OptIn template walks base classes.
  - At spawn it is re-evaluated only against the loaded fill, and `Storage.Deserialize` has restored the items by then.
  - Code shows no path that would make it wrong: no transient fill, capacity or mass at spawn.
  - Remaining unverified causes:
    - a save snapshot taken while a threshold was being crossed (Moono background save)
    - a fill at spawn that differs from what code shows
- **How to tell them apart:** ask Jeremy whether any small slider nudge fixes it, or only dragging High below or Low above the current fill.
  - Any nudge means `LastSetFlag` was null, which is candidate 1.
  - Only past the fill means `activated` was wrong, which is candidate 2.

### 4. Updated fix proposal
1. **Diagnostic build first:** logs only, not shipped.
   - A postfix on `StorageLockerSmart.OnSpawn` logs `ThresholdsBase.Get(go) != null`, `LastSetFlag`, `activated`, the fill percent, both components' `isSpawned` and the port value.
     - `LastSetFlag == null` right after the locker's OnSpawn proves the prefix did not run at spawn.
   - A postfix on `UpdateLogicAndActiveState` logs the same values on every call.
   - Load one of Jeremy's saves on Moono, then nudge a slider.
2. **If candidate 1: send again once both have spawned.**
   - A postfix on `StorageLockerSmart.OnSpawn`: if `ThresholdsBase.Get(go)` exists, set `LastSetFlag = null` and call `UpdateLogicAndActiveState` through `AccessTools.MethodDelegate`, which goes to the patched method.
   - A postfix on `ThresholdsBase.OnSpawn`, bound by reflection: if the locker has already spawned (`StorageLockerSmart.isSpawned`), call the same.
   - Whichever spawns second sends the thresholds value. This is `ThresholdsRefresh` from Fridge Thresholds Extended, made independent of order. The same pair works for `Refrigerator`.
   - If the cause is JIT inlining, the delegate call still goes through the detoured method, so the second send fixes it.
3. **If candidate 2:** a refresh cannot fix a wrong `activated`. The diagnostic log has to show where it flips first. If it flips at the spawn evaluation, find which input differs at that moment, and fix it by deferring the first evaluation one frame with `GameScheduler.Instance.ScheduleNextFrame` until that input is settled.
4. **Where it goes:** unchanged. Either Fridge Thresholds Extended (reuse the reflection bindings in `ThresholdsCompat.Apply`) or a small new patch mod. Patch the private `UpdateLogicAndActiveState` by name.
