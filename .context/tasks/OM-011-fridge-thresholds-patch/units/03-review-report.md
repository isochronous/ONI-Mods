# OM-011 review report: Fridge Thresholds Extended (b257266..1e1d658)

Read-only review, game not launched. Passes 1-5 of code-quality-rules were run over every changed file: ThresholdsCompat.cs, Mod.cs, csproj, mod.yaml, mod_info.yaml, README, CHANGELOG, .gitignore/.gitattributes, sln, root README, .github templates and label workflow.

**Verdict: nothing blocking.** The Freezer prefix and copy-settings postfix match the thresholds mod's Refrigerator code. The IR targets are correct, and absence safety holds. One moderate item (7, the first signal after load) is real but small, and has a one-method fix. The rest are minor.

## Findings

### Moderate

**M1. Freezer: the first signal after a load can be Freezer's own IsFull (spec item 7). Real, low impact.** (Pass 1)
- Where: ThresholdsCompat.cs:167-169. The cause is outside this mod: FreezerConfig.cs:75 (`go.AddOrGet<Freezer>()`) runs before the thresholds mod's postfix adds `RefrigeratorThresholds` (Storage.cs:388, 405, 409-412).
- Verified: on the Freezer prefab the component comes after `Freezer`. `Freezer.OnSpawn` calls `UpdateLogicCircuit` directly (Freezer.cs:73). `ThresholdsBase.Get` reads `fastMap`, which `ThresholdsBase.OnSpawn` fills (Storage.cs:124). `ThresholdsBase.OnSpawn` then sets `LastSetFlag = null` and does not call `UpdateLogicCircuit` (Storage.cs:121-127). After that, Freezer recomputes only on OnStorageChange (-1697596308) or OperationalChanged (-592767678) (Freezer.cs:76-77). `Sim4000ms` recomputes only inside rocket interiors (Storage.cs:183).
- Inferred: that OnSpawn runs in component order. The thresholds mod relies on the same thing when it uses a prefix for the vanilla fridge.
- Failure scenario: a powered Freezer between the thresholds when the save is loaded, for example 60% full with High 80 / Low 40 while the port is green and the hysteresis is on its way down. After the load it sends IsFull, which is red. It stays red until a dupe stores or takes food or the power state changes. With "green on low" the wrong colour is more likely. The case where a Freezer is unpowered at spawn self-corrects, because the later power-on raises OperationalChanged.
- Is the proposed fix right? Yes. Add `RefrigeratorThresholds` in our own prefix on `Psyko.Freezer.FreezerConfig.DoPostConfigureComplete` (null-checked, the same `AddThresholdsPrefix`). This puts it ahead of `Freezer`, which mirrors Storage.cs:369-377 for the vanilla fridge and our own IR prefix. The thresholds mod's postfix uses `AddOrGet` (Storage.cs:411), so no duplicate component appears. The patch is applied in OnAllModsLoaded, before `LoadGeneratedBuildings` runs the configs. `RefrigeratorThresholds.OnSpawn` touches only `LogicPorts.outputPortInfo[0]`, which already exists from `BuildingDef.LogicOutputPorts` (FreezerConfig.cs:48), exactly as on the vanilla fridge.
- Alternative that does not depend on spawn order: in the prefix, when `Get` returns null, fall back to `__instance.GetComponent(refrigeratorThresholds)`. Serialized fields are already restored before OnSpawn. The later `LastSetFlag = null` in its OnSpawn only causes one redundant resend. The config prefix is the more consistent choice.
- Suggested fix: add the FreezerConfig prefix inside `PatchFreezerLogic` (or its own try block). Drop or reword the comment at line 169.

**M2. No test project.** (Pass 5)
- Where: mods/fridge-thresholds-extended/src. Scaffold commit 88214fb added `SmoothBorders.Tests`, and most mods here have a `<Name>.Tests` (TestRunner kit). The implement report lists this as a deviation.
- Scenario: none of the "needs an in-game check" items are covered by a repeatable test. Those are Harmony accepting `Component __instance` and the `___` injection, sliders driving the port, copy settings, the first signal after load, and each mod missing in turn.
- Suggested fix: the plan's testing phase is the place for this. If the in-game test is done through TestRunner, add `FridgeThresholdsExtended.Tests` with one case per behaviour (Freezer port follows the thresholds, Freezer copy settings, one IR fridge port, first signal after load). Run it with `-Steam` for the three Workshop ids. Not blocking for review.

### Minor

**m1. Misleading warning when an IR class is missing.** (Pass 4)
- Where: ThresholdsCompat.cs:89-93. If one of the five config types does not resolve while others do (for example after an IR rename), the warning says "DoPostConfigureComplete not found" when the class itself is missing.
- Fix: use a separate message for `configs[i] == null` ("... class not found").

**m2. Per-call allocations in the Freezer prefix.** (Pass 1, performance)
- Where: ThresholdsCompat.cs:172-177. Each call allocates two `object[]`, plus boxing for `Invoke` and `SetValue`. It runs on every storage change of every Freezer, which is cheap but not free.
- Optional fix: bind `UpdateLogicState`, `IsActuallyOperational` and the `LastSetFlag` accessors once with `AccessTools.MethodDelegate<...>(method, delegateArgs: ...)` or a small `DynamicMethod` taking `object`. The current form is correct, so fine to leave.

**m3. The llunak check depends on order.** (Pass 3)
- Where: ThresholdsCompat.cs:141-146. The implement report already notes this. If a future thresholds release patches Freezer in its own OnAllModsLoaded after ours runs, the check misses it and both prefixes are installed.
- Verified: `UpdateLogicState` is idempotent for a repeated input (Storage.cs:105-119), and the `LastSetFlag` guard stops a second send. The effect is a redundant computation, not a wrong signal.
- No change needed. A comment at line 140 saying this would help the next reader.

**m4. README "Building" closing line explains mechanics.** (Pass 4)
- Where: README.md:32 ("Harmony only, no PLib. None of the three mods is referenced at build time..."). It is harmless in a repo README, but no other mod README carries such a line. Keep it out of the Workshop text when that is written (workshop-writing-style).

No commented-out code, debug statements, swallowed exceptions or secrets. Pass 2 (security) found nothing applicable: there is no input and no I/O.

## Spec checks

1. **Freezer prefix vs Storage.cs:346-366. Verified, identical.**
   - Fraction: `GetAmountStored / GetMaxCapacityMinusStorageMargin` through `MethodDelegate<Func<FilteredStorage,float>>`, as Storage.cs:321-326 does. Both methods are private instance members in vanilla FilteredStorage.cs:174-185.
   - Operational: `IsActuallyOperational(___operational)`.
   - `flag = UpdateLogicState(stored/capacity) && operational`, with the same evaluation order (both are computed before the `&&`, so the side effect on `activated` always happens, as upstream).
   - Change detection: `!(LastSetFlag is bool last) || last != flag` is the same as the lifted `flag != (bool?)LastSetFlag`, so a null sends.
   - Port and meter: `FilteredStorage.FULL_PORT_ID`, then `SetLogicMeter(flag)`, then `return false`.
   - Fallback: `return true` when `Get` returns null.
   - Skipping the original loses nothing, because Freezer.cs:112-117 only sends and sets the meter.
   - Members, checked against the decompile: `Freezer.UpdateLogicCircuit` is private instance (DeclaredMethod), and the fields `filteredStorage`/`operational`/`ports` have exactly the types the prefix expects (Freezer.cs:12, 15, 20). `ThresholdsBase.Get(GameObject)` is public static and returns `ThresholdsBase`, and `Delegate.CreateDelegate` to `Func<GameObject,object>` is valid through reference return covariance. `UpdateLogicState(float)` returns bool and `IsActuallyOperational(Operational)` returns bool, both public instance. `LastSetFlag` is a `bool?` auto-property. The type checks at lines 125-133 enforce all of this, so a changed member makes the part skip rather than mis-bind.
   - Inferred, needs the game: that Harmony accepts `Component __instance` and the `___` injection on a manual patch. SmartWeightPlate's manual postfix already uses `Component __instance`, and the `___` names resolve against the original's declaring type.
2. **OnCopySettings postfix vs Storage.cs:328-344. Verified.**
   - `data` is the source GameObject, and the parameter name matches `Freezer.OnCopySettings(object data)` (Freezer.cs:94). The source and target `RefrigeratorThresholds` are read as upstream does.
   - Same three properties in the same order. Each setter re-runs `UpdateLogicCircuit`, which reaches our prefix through Storage.cs:304-313.
   - Using `as` instead of a cast is safe. The postfix runs after Freezer copies `UserMaxCapacity`.
3. **IR prefix. Verified.**
   - The five class names and the namespace match ir-src/RonivansLegacy_ChemicalProcessing.Content.Defs.Buildings.DupesRefrigeration/*.cs, and the ID swap on the Hightech pair is handled by matching on class.
   - Each class declares `DoPostConfigureComplete(GameObject go)`, so the parameter name `go` matches the prefix, and calls `go.AddOrGet<Refrigerator>()` in its body (lines 81/81/81/88/102). The prefix therefore puts the component ahead of `Refrigerator`.
   - `GetComponent(type) == null` prevents a double add if a future thresholds release lists IR with `AddOrGet`.
   - The thresholds mod's `OnSpawn` and `UpdateLogicPortTooltip` read `outputPortInfo[0]`. Each of the five has exactly one output port, `FilteredStorage.FULL_PORT_ID` (lines 51/51/51/60/62), so port 0 is FULL.
   - `RefrigeratorThresholds.refrigerator` (`[MyCmpGet]`) finds the vanilla `Refrigerator`, so the thresholds mod's own prefix and copy postfix drive these fridges.
4. **Absence safety. Verified.**
   - The built DLL (bin/Release, 12:46 today) references only mscorlib, Assembly-CSharp, Assembly-CSharp-firstpass, 0Harmony, UnityEngine.CoreModule and System.Core, read from its metadata.
   - Patch signatures use only `GameObject`, `Component`, `object`, `FilteredStorage`, `Operational` and `LogicPorts`. Static fields are `Type`/`MethodInfo`/`PropertyInfo`/delegates over vanilla types.
   - Thresholds missing: one `Debug.Log`, then return. Freezer missing: silent. IR missing: silent (`configs.All(null)`). Each part has its own try/catch using `GetBaseException().Message`. Apply runs once, so each line logs once.
5. **oni-moono (research Q5). Verified against the code.**
   - Patching happens in OnAllModsLoaded, before any world exists (D34).
   - Lookups use `AccessTools.TypeByName` only, with no bare `Type.GetType` (D49).
   - No `GetCallingAssembly` and no stack indices (D19).
   - No generics or value-type overrides (D13).
   - D21 rebuilding is handled by the host.
6. **Repo conventions. Verified, consistent.**
   - The csproj is the no-magic-fridges no-PLib shape, line for line. Mod.cs has the same OnLoad log line as NoMagicFridges and the OnAllModsLoaded → `Compat.Apply(harmony)` shape of SmartWeightPlate. `[FridgeThresholdsExtended]` log prefix. Comment density is the same as SmartWeightPlate's compat (an XML summary naming the other mods with Workshop ids).
   - mod.yaml uses staticID `Isochronous.FridgeThresholdsExtended`. mod_info.yaml has ALL / 744825 / 0.1.0 / APIVersion 2. Files are CRLF.
   - CHANGELOG has one plain bullet. The README sections match vent-freeze-fix, minus options and translations, which this mod does not have. Release links point at the monorepo with `<name>-v<ver>` tags (repo-layout-2026-10).
   - The sln, root README row and `.github` additions match 88214fb, in alphabetical position. The GitHub label still has to be created at release, as the implement report says.
   - Deviations: no PLib, translations or Tests project (M2). All are justified except tests.
7. **First signal after load.** Real. See M1. The proposed fix is right.

## Verified vs inferred
- **Verified** by reading the code: everything with a file:line above, and the DLL's assembly references from metadata.
- **Inferred**, needs the in-game test:
  - Harmony accepting `Component __instance` and the `___` fields on the manual patches.
  - That OnSpawn order follows component order, and so the exact M1 window.
  - That a double prefix (m3) behaves as described.
  - That sliders drive the port on all six buildings.

## Friction for the coordinator to record
None with ICON in this unit. The handoff brief said "nothing matched", which was accurate. The governing pointers came through the research report.
