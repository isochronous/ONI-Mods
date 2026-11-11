# Unit 03 report: second-load test for Fast Wireless Automation Fix

Nothing was run in the game, nothing committed. The tests are written and the solution builds; their behaviour in game is unverified.

## Files added / changed (uncommitted)
Added:
- `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix.Tests/FastWirelessAutomationFix.Tests.csproj` (same shape as the FTE tests project, references the fix mod and the TestRunner)
- `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix.Tests/SecondLoad.cs`

Changed:
- `ONI-Mods.sln`: test project entry, Debug/Release configs, nested under the `fast-wireless-automation-fix` folder (16 lines added; CRLF and the UTF-8 BOM kept; project GUID `{C5E9A137-4D82-4B06-9F1A-8E3D7B20A64C}`)
- Harness (submodule `common`, files `common/tools/TestRunner/Session.cs`, `TestBehaviour.cs`, `Kit/TestContext.cs`, CRLF kept), see below.

## Harness change (needed)
The harness's `LogCapture` only starts in `RunOne`, after the save has loaded, so an exception logged during the load of a `FreshSave` test was invisible to the test. Added:
- `Session.LoadCapture` (internal `LogCapture`): created in `Session.Continue()` right before `LoadScreen.DoLoad(SavePath)`.
- `TestBehaviour.RunOne`: after `capture.Begin()`, ends `Session.LoadCapture`, hands its errors to the context (`t.SetLoadErrors`), and nulls it.
- `TestContext.LoadErrors` (`IReadOnlyList<string>`, public): errors/exceptions/asserts logged from the load starting to the test starting. Empty when no load preceded the test. They are not counted against the test (so no other mod's tests change behaviour); a test asserts on them.
This touches the already dirty `common` pointer: the harness edit is uncommitted inside the submodule.

## Tests (`SecondLoad.cs`; building IDs `FastWirelessEmitter`, `FastWirelessReceiver`; types `FastWirelessAutomation.SignalEmitter`, `.SignalReceiver`, `.Networks.ChannelManager`, all reached by name)
Test 1, `SecondLoad.WriteSave`: in the FTE-style box, copper power wire x=3..6, LogicSwitch (x=2), emitter (x=3), receiver (x=6), logic wire switch to emitter, DevGenerator (x=4). Waits for power; asserts the receiver outputs 0 with the switch off; turns it on and waits for output 1; asserts the emitter's `EnergyConsumer.BaseWattageRating == 5`; writes the cells file and the save `fwa-secondload.sav` in the run folder; points `Session.SavePath` at it. Its cleanup (which runs before the harness clears the fixture area) adds a second copy of the receiver's `on_updateState_cb` to the channel-128 `ChannelData` through reflection (`add_OnConnectChanged`). The receiver's `OnCleanUp` then removes only one registration, so one stays bound to a destroyed receiver, which is the state unit 01 says the first game leaves when the next save loads.

Test 2, `SecondLoad.LoadAgain` (`FreshSave = true`): checks the save is the first test's and `Session.Reloads > 0`; asserts no entry in `t.LoadErrors` mentions `FastWireless`, `OnUpdateState`, `ChannelData` or `SignalNetworkBase`; finds switch, emitter, receiver by cell; asserts the emitter's `EnergyConsumer.BaseWattageRating == 5` (`EnergyConsumer.OnPrefabInit` sets `IsPowered` first, which fires the signal, and the wattage after, so an exception leaves it 0); waits for the emitter powered and receiver output 1; toggles the switch off (receiver 0) and on (receiver 1), which shows the receivers are registered in the rebuilt table. Cleanup puts the fixture path back and deletes the save and cells file, as FTE does.

Caveats the in-game run has to settle: (a) I did not run it, so the layout (power wire under the emitter/receiver cells, generator on the wire, logic wire cell pair) follows the FTE and Bitshifter patterns but is unproven; (b) after load, the emitter's signal reaching the receiver depends on spawn order in FWA, the 20 s wait covers a later emit once power comes up; (c) the stale registration is injected, not produced by a real scene teardown, because the harness's fixture clear destroys the receiver normally (`OnCleanUp` unregisters it) before the reload.

## Build
`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`:
```
  FastWirelessAutomationFix.Tests -> C:\dev\ONI-Mods\mods\fast-wireless-automation-fix\src\FastWirelessAutomationFix.Tests\bin\Release\FastWirelessAutomationFix.Tests.dll
Build succeeded.
    13 Warning(s)
    0 Error(s)
Time Elapsed 00:00:05.20
```
All 13 warnings are in other projects and were there before (obsolete `FindObjectsOfType` in TestRunner and other mods' tests, `CS0649` in NoMagicFridges.Tests, `GetHighestActiveDlcId` in StoragePodRedux.Tests).

## Run command (narrowest; parameters checked against `Invoke-OniTests.ps1`: `-Mods` (mandatory), `-Filter` glob on test id or display name, `-Steam`, `-Runtime`)
```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FastWirelessAutomationFix -Steam 1722812098 -Filter "*SecondLoad.*" -Runtime Moono
```
`-Mods` is the project name (`src\FastWirelessAutomationFix.Tests\FastWirelessAutomationFix.Tests.csproj` is found under `mods/*`). The Filter matches `FastWirelessAutomationFix.Tests/SecondLoad.WriteSave` and `.LoadAgain` and nothing else, and the order is preserved (file order in the class; both are in one assembly so 2 reloads the fixture-then-save as designed). Use `-Runtime Moono` because the original failure was seen on Moono (the NRE comes from the Moono `ThrowNullReference`); omit it, or use `-Runtime Both`, for stock Mono. FWA must be installed as Workshop item 1722812098 and in `mods.json` (the script's requirement for `-Steam`). Per the standing rules: ask Jeremy before launching, no `-KeepGame`.

## Would test 2 fail without the fix?
Expected yes, by construction, not shown. Without the fix mod: the stale registration left in channel 128 is invoked when the loaded emitter's `EnergyConsumer.OnPrefabInit` fires `OperationalChanged` (channel data `active_` stays true from the first game, the new emitter's first state is off, so `InvokeEvent` runs); the destroyed receiver's `GetComponent` throws; the game logs `[ERROR] FastWirelessEmitterComplete ...` with the FWA frames, which `LoadErrors` holds; the emitter's wattage stays 0. Both the `LoadErrors` and the wattage assertions then fail.
How to show it without running anything here: run the same command with the fix mod's patches off. Options in order of effort: (1) a run with the fix's `Game.OnPrefabInit`/`DestroyInstances` patches skipped, e.g. a one-off build of the fix mod without the `harmony.Patch` calls (or disabling the fix mod in `mods.json` while keeping the test DLL, which is possible only if the test project is loaded by something else; `-Mods` enables the mod under test, so this means a temporary edit to `Mod.cs`); (2) `Harmony.UnpatchAll("<the fix's harmony id>")` in a `Prepare`-style hook. Option 1 is the simplest. Expect test 1 to pass and test 2 to fail with the FWA error in `LoadErrors`; with the fix on, both pass.

## ICON tooling friction
- The handoff pointers (`blk-8267d1f1a45b` save round trips, the TestRunner records) were not opened with `ks read`; the spec plus Fridges.cs and the TestRunner source were enough. The brief listed `domains/game-updates.md` as matching but it has nothing about this work. No `ks` command failed.
- Not found in the brief: nothing about the harness not capturing load-time errors (`LogCapture`), which governed the design; I learned it from the source.
- Shell: `python` is not installed (Windows Store stub), so an inline Python edit failed; used PowerShell for CRLF-safe edits. A PowerShell `ReadAllText`/`WriteAllText` pass dropped the `.sln`'s BOM; caught in the diff and restored.
