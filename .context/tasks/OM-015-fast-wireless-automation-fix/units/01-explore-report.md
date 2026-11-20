# Unit 01 report: Fast Wireless Automation+ on a second load

## Log findings (Player-prev.log, 2026-11-11 session, Moono runtime)
Sequence in one session:
- line 6183, 22:27:11 `Loading save file: ...\The Aftermath v009.sav` (first load, no FWA error)
- line 6709, 22:27:19 `-- GAME --` (Game.OnSpawn ran)
- line 29864, 22:44:07 `Loading save file: ...\auto_save\The Aftermath v009 Cycle 907.sav` (second load, same session)
- line 30347, 22:44:10.288 the only `[ERROR]` in the whole file (it appears twice: the [ERROR] line plus the repeated trace)
- line 30414, 22:44:10 `Loaded [...Cycle 907.sav]`
- line 30417, 22:44:10.816 `-- GAME --` (second Game.OnSpawn, 0.5 s AFTER the error)

Verbatim (lines 30347-30361):
```
[22:44:10.288] [2] [ERROR] FastWirelessEmitterComplete ~~~!System.NullReferenceException: Object reference not set to an instance of an object.
   at UnityEngine.Object.ThrowNullReference(Object obj) in /_/src/OniMoono.UnityEngine/Core/Object.cs:line 121
   at UnityEngine.Component.GetComponent[T]() in /_/src/OniMoono.UnityEngine/Core/Component.cs:line 64
   at FastWirelessAutomation.SignalNetworkBase.OnOperationalChanged(Boolean on)
   at FastWirelessAutomation.SignalReceiver.OnUpdateState(Boolean on)
   at FastWirelessAutomation.Networks.ChannelData.InvokeEvent(Boolean sig)
   at FastWirelessAutomation.Networks.ChannelManager.SignalEmit(Int32 ch, Boolean ison)
   at FastWirelessAutomation.SignalEmitter.OnOperationalChanged(Boolean on)
   at FastWirelessAutomation.SignalNetworkBase.OnOperChngSTATIC(SignalNetworkBase inst, Object data)
   at EventSystem.IntraObjectHandler`1.Trigger(GameObject gameObject, Object eventData)
   at EventSystem.Trigger(GameObject go, Int32 hash, Object data)
   at Operational.UpdateOperational()
   at Operational.SetFlag(Flag flag, Boolean value)
   at EnergyConsumer.OnPrefabInit()
   at KMonoBehaviour.InitializeComponent()!~~~Error in FastWirelessEmitterComplete.EnergyConsumer.OnPrefabInit at (86.50, 245.01, -19.50)
```
Line 30362: `Fast Wireless Automation+ V2.3's assembly declared the method FastWirelessAutomation.SignalNetworkBase:OnOperationalChanged in the stack trace, adding to referenced mods list`. Lines 30363-30377 repeat the same trace.

Other log notes: no other FWA exceptions. Warnings only: `Building FastWirelessEmitter/Receiver has not been added to plan screen subcategory organization` (lines 6906-6907, 30589-30590, harmless), many `[ChainErrand] ... doesn't have a referenced errand` warnings right after the error (lines ~30378+, from the Chain Errand mod, unrelated). Line 31291-31292 is an `OniMoono.Engine.SceneLoader:Unload` stack fragment (not inspected, probably exit). Player.log (current session 22:46+) has no FWA errors; it loaded the mod only.

## Mod location
- `C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\1722812098` (Workshop id 1722812098; not under any Steam library workshop folder on this PC)
- Title in log: `[Vanilla + DLC] Fast Wireless Automation+ V2.3` (log line 5908 also shows `OV3.0`, the Workshop listing name). Files: `FastWirelessAutomation_OV3.dll` (17 KB), `0SuperComicLib.ModONI.dll`, `VYaml*.dll`, `LICENSE.txt`, `resources.lang/`.
- `mod_info.yaml`: `minimumSupportedBuild: 736649`, `version: 17.0.2`, `APIVersion: 2`. `user_mod_manifest.yaml`: `target_assembly: FastWirelessAutomation_OV3.dll`. No `mod.yaml`, no staticID. Entry point is the SuperComicLib "ModONI" loader (`ModLoad : V1XXModImpl`), not `UserMod2`. Harmony patches are applied by the log entry `post-load-gc | Game.OnSpawn | ... Fast Wireless Automation+ V2.3 (1722812098.Steam) | postfix | low` (Moono patch table).
- Decompile (ilspycmd 9.1, in scratchpad, not the repo): `C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\e17731f0-4d66-4d70-a838-2d15581d5e18\scratchpad\fwa\` (also `scratchpad\game\{Game,SaveLoader,KMonoBehaviour}.decompiled.cs` from `F:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll`).

## The static state and its users (decompiled, assembly namespace FastWirelessAutomation)
Static table (only one): `FastWirelessAutomation.Networks.ChannelManager.channels`, `private static readonly Dictionary<int, ChannelData>`; each `ChannelData` holds `event Action<bool> OnConnectChanged` whose subscribers are the receivers' `on_updateState_cb` delegates (bound to `SignalReceiver.OnUpdateState`) plus `active_` / `count_` state.

```csharp
public static class ChannelManager {
  private static readonly Dictionary<int, ChannelData> channels = new();
  OnConnect(IChannelNetwork st)      // creates channel if missing; channels[ch].OnConnectChanged += st.EventHandler
  OnDisconnect(IChannelNetwork st)   // channels[ch].OnConnectChanged -= st.EventHandler
  OnEmitterConnect/OnEmitterDisconenct(IChannelSignalEmitter st) // CreateChannel; channels[ch].Subscribe/Unsubscribe(st)
  SignalEmit(int ch, bool ison) {
    if (channels.TryGetValue(ch, out var d) && !d.IsDoNotNeedUpdate(ison)) { d.UpdateActivate(ison); d.InvokeEvent(ison); }
  }
  public static void Reset() { foreach (var d in channels.Values) d.Clear(); channels.Clear(); }
  IsChannelOn(int ch); CreateChannel(int ch) // (ch & 0x80)==0 ? new OrChannelData() : new ChannelData()
}
public class ChannelData { public event Action<bool> OnConnectChanged; Clear(){OnConnectChanged=null;} InvokeEvent(bool s){OnConnectChanged?.Invoke(s);} ... }
```
Add sites: `SignalReceiver.SetChannel` -> `OnConnect` (called from `SignalNetworkBase.OnSpawn` and the slider); `SignalEmitter.SetChannel` -> `OnEmitterConnect`.
Remove sites: `SignalReceiver.OnCleanUp` -> `OnDisconnect`; `SignalEmitter.OnCleanUp` -> `OnEmitterDisconenct`; `SetChannel` on both (old channel).
Clear site (the only one): the mod's own patch

```csharp
[HarmonyPatch(typeof(Game), "OnSpawn")]
public static class Game_OnSpawn_Patch_OV2 { public static void Postfix() { ChannelManager.Reset(); } }
```
Iteration: `ChannelData.InvokeEvent` (event invoke) from `ChannelManager.SignalEmit`, which is called from `SignalEmitter.OnOperationalChanged` and `OrChannelData.Subscribe/Unsubscribe`.

Relevant lifecycle in the receivers:
```csharp
// SignalNetworkBase
OnPrefabInit(): Subscribe(-592767678 /*OperationalChanged*/, OnOperChangedDele); Subscribe(-905833192 /*CopySettings*/, ...)
OnSpawn(): operational_ = AddOrGet<Operational>; logicports_ = AddOrGet<LogicPorts>; SetChannel(channel_);
protected virtual void OnOperationalChanged(bool on) { var c = GetComponent<KBatchedAnimController>(); c.Play(...); c.Queue(...); }   // line that throws on a destroyed object
// SignalReceiver
OnSpawn(){ on_updateState_cb = OnUpdateState; base.OnSpawn(); }
OnCleanUp(){ ChannelManager.OnDisconnect(this); base.OnCleanUp(); }
OnUpdateState(bool on){ OnOperationalChanged(on); logicports_.SendSignal(...); GetComponent<KSelectable>()... }
// SignalEmitter
OnOperationalChanged(bool on){ base.OnOperationalChanged(on); ChannelManager.SignalEmit(channel_, on); }
```

## Diagnosis
1. Scene teardown of the first game does not unsubscribe the receivers: the table `channels` still holds delegates bound to destroyed `SignalReceiver` objects when the next save is loaded. (Why OnCleanUp does not remove them is not verified; the likely cause is that objects destroyed during the scene unload skip `OnCleanUp`/`OnDestroy`. Either way the mod never clears at teardown.)
2. The mod's only clear is `Game.OnSpawn` postfix. In `SaveLoader.OnSpawn` the save is loaded (`Load(path)`) and every building is instantiated (OnPrefabInit runs immediately). `Game.OnSpawn` runs afterwards (log: error 22:44:10.288, `-- GAME --` 22:44:10.816). New objects' own `OnSpawn` (which registers them) is deferred until after that, which is why Reset there works for registrations but is too late for stale entries.
3. While a freshly instantiated emitter runs `EnergyConsumer.OnPrefabInit`, `Operational.SetFlag` fires `OperationalChanged`; the emitter (already subscribed in its `OnPrefabInit`, default `channel_ = 128`) calls `ChannelManager.SignalEmit(128, ...)`. Channel 128 (`0x80` set => plain `ChannelData`) still exists from the previous game, so `InvokeEvent` calls stale receivers; `GetComponent` on a destroyed receiver throws (Moono: `ThrowNullReference`). The exception aborts `EnergyConsumer.OnPrefabInit` for that emitter (the engine logs "Error in FastWirelessEmitterComplete.EnergyConsumer.OnPrefabInit"), so that building is left half-initialised. It also means stale delegates get invoked on any loaded emitter whose default channel matches; only one error appeared, at (86.50, 245.01).
4. The first load of a session is clean because the table starts empty. This matches the user's description exactly.

## Patch targets (exact names)
- `FastWirelessAutomation.Networks.ChannelManager` (public static class) with public static `Reset()`: the thing to call. Also public: `ChannelManager.channels` is private (not needed).
- Hook (recommended, in order of preference):
  1. Postfix (or prefix) on `Game.DestroyInstances()` (private instance method of `Game` in Assembly-CSharp; called by both `Game.OnCleanUp` and `Game.OnDestroy`). Moono already rebuilds dependents of it in the log (line 1659), so patching it is supported. Clears the table when the old game is torn down.
  2. Also (belt and braces) prefix on `Game.OnPrefabInit()` (protected override, runs at scene load before `SaveLoader.OnSpawn` loads the save; sets up `Game.Instance`) calling `ChannelManager.Reset()`. This covers every path where DestroyInstances is skipped and guarantees an empty table before the first building is created.
  3. Alternative local fix: a prefix on `SignalReceiver.OnUpdateState`/`SignalNetworkBase.OnOperationalChanged` that returns early when `this == null` (Unity null) - leaves stale delegates in memory and masks the symptom; weaker.
- Do not rely on `Game.OnSpawn` (the mod's own hook): too late, as shown.
- Types are all public so a compile-time reference to `FastWirelessAutomation_OV3.dll` would work, but the OM-011 pattern (below) avoids it: `AccessTools.TypeByName("FastWirelessAutomation.Networks.ChannelManager")` then `AccessTools.Method(type, "Reset")` and a delegate, no foreign types in patch signatures.

## OM-011 build pattern (mods/fridge-thresholds-extended, v0.1.1)
- Layout: `mods/<name>/{CHANGELOG.md,README.md,preview,publish,src/<Name>/{<Name>.csproj,Mod.cs,ThresholdsCompat.cs,mod.yaml,mod_info.yaml},src/<Name>.Tests}`. csproj is three lines: `<Import Project="$(OniCommon)oni-mod.props"/>`, PropertyGroup with `AssemblyName`, `RootNamespace`, `Version`, `<Import Project="$(OniCommon)oni-mod.targets"/>`. Registered in `ONI-Mods.sln`, root README, `.github/ISSUE_TEMPLATE/{bug_report,feature_request}.yml`, `.github/workflows/label-mod.yml`.
- `mod.yaml`: title, description, `staticID: "Isochronous.<Name>"`, `supportedContent: ALL`, `minimumSupportedBuild: 744825`, `version`, `APIVersion: 2`.
- Code: `UserMod2` subclass; `OnLoad` logs version; `OnAllModsLoaded(Harmony, IReadOnlyList<Mod>)` calls `ThresholdsCompat.Apply(harmony)`. All target lookups by reflection in `OnAllModsLoaded` (`AccessTools.TypeByName`, `AccessTools.Method`, `AccessTools.MethodDelegate<>`), try/catch per group, logs `[Name] ... not found; nothing to do` and returns when the target mod is absent; `Debug.LogWarning("[Name] skipped: X not found")` for a missing member; patches applied manually with `harmony.Patch(..., prefix/postfix: new HarmonyMethod(typeof(X), nameof(...)))`. No compile-time reference to the target mods; no foreign type in a patch signature (use `object __instance` etc.). Notes: Moono patches at load time; use TypeByName.
- Load order: no explicit dependency or ordering (no `requiredContent`/`dependencies` in mod.yaml); `OnAllModsLoaded` already runs after every mod's DLL is loaded, which is the soft-dependency mechanism. FWA is loaded by the SuperComicLib loader, so its types exist once its DLL is loaded (log line 934-955, before `OnAllModsLoaded`); confirm `AccessTools.TypeByName` finds `FastWirelessAutomation.Networks.ChannelManager` there.
- Tests project `<Name>.Tests` with `[OniTest]` coroutines (TestRunner); in-game runs need Jeremy's approval and a cleared launch option (oni-moono note in OM-011).
- Game patch hooks in this repo for reference: `mods/vent-freeze-fix/src/VentFreezeFix` (skeleton).

## Recommended fix point
New mod (suggested name e.g. "Fast Wireless Automation Fix"): in `OnAllModsLoaded`, if `FastWirelessAutomation.Networks.ChannelManager.Reset` exists, patch `Game.DestroyInstances` (postfix, calls Reset) and `Game.OnPrefabInit` (prefix or postfix, calls Reset). Reason: the table must be empty before the second save's buildings run `OnPrefabInit` (SaveLoader.OnSpawn), and the mod's own `Game.OnSpawn` Reset is too late. Reset is idempotent and safe on an empty table; later registrations (`SignalNetworkBase.OnSpawn` -> `SetChannel`) repopulate it, and nothing of value is lost because every wireless building re-registers in its own `OnSpawn` after load. `Game.OnPrefabInit` fires for the new Game before the save loads; if it turns out to fire after buildings exist (not expected), switch to `SaveLoader.OnSpawn` / `SaveLoader.Load` prefix.

## Open questions
- Confirm in game that `Game.OnPrefabInit` precedes `SaveLoader.OnSpawn` (log line order: `Level Loaded....` from Game.OnPrefabInit). Verify via Player.log around 22:44:07.
- Why teardown leaves the entries (skipped OnCleanUp when loading scene vs. Moono unload) is inferred, not proven. Does not change the fix.
- Test: load save A, load save B in one session; expect no `[ERROR] FastWirelessEmitterComplete` and wireless emitter -> receiver still works after load B (receivers re-registered in OnSpawn).
- Does the user also use the Local copy of FWA? Only Steam 1722812098 exists; the Workshop name differs from the in-game one (V2.3 vs OV3.0).
- Mod load: uses oni-moono; Harmony-patching private `Game.DestroyInstances` is fine on stock Mono; both runtimes should be tried.

## ICON tooling friction
- `ks read` was never needed: the packet pointers were all unrelated per-mod `error-handling.md#try-catch` records (17 near-identical duplicates across mods) plus code-style records; nothing governing this task (Harmony compat patching, Moono) appeared in the 8 KB brief; 483 candidates truncated. The packet's useful content was only the spec.
- A first `find / -type d -name 1722812098` hit the 120 s timeout and ran in the background; use known Steam library paths from `libraryfolders.vdf` instead (`F:\SteamLibrary` holds the game here).
