# Unit 02 report: Fast Wireless Automation Fix mod

## Files added / changed (all uncommitted)
- `mods/fast-wireless-automation-fix/.gitignore`, `.gitattributes` (copied from fridge-thresholds-extended)
- `mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/FastWirelessAutomationFix.csproj` (same three-part pattern, v0.1.0)
- `.../Mod.cs`, `.../mod.yaml` (staticID `Isochronous.FastWirelessAutomationFix`, minimumSupportedBuild 744825), `.../mod_info.yaml`
- `ONI-Mods.sln`: solution folder + project + Debug/Release configs + nesting (9 added lines, CRLF kept)
- Not touched: `common` (still dirty), `debug.log`, other mods, README/issue templates/workflow (out of scope).

## Patch targets
In `OnAllModsLoaded`: `AccessTools.TypeByName("FastWirelessAutomation.Networks.ChannelManager")`; absent -> logs `... not found; nothing to do` once and returns. `Reset()` is bound to a `System.Action` delegate. Both hooks exist in Assembly-CSharp (confirmed in the decompile: `Game.OnPrefabInit` protected override at line 780, private `Game.DestroyInstances` at line 2040, called from `OnCleanUp`/`OnDestroy`):
- postfix `Game.DestroyInstances`: clears the table when the old game is torn down.
- prefix `Game.OnPrefabInit`: clears it before `SaveLoader.OnSpawn` builds the new save's buildings (FWA's own `Game.OnSpawn` Reset is too late, per unit 01).
Missing members log a warning and skip; the patch body is wrapped in try/catch. No compile-time reference to FWA. Not verified in game: that `Game.OnPrefabInit` precedes building creation (unit 01 open question).

## Build output
`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` (final run):
```
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:05.04
```
Earlier run produced `FastWirelessAutomationFix -> ...\bin\Release\FastWirelessAutomationFix.dll`. The 5 warnings are pre-existing (other mods' tests). Two fixes were needed on the way: ONI has a global `Action` type that shadows `System.Action` (used `System.Action` explicitly); and the sln edit initially had a form-feed from a `\f` escape in my Python script (fixed, bytes verified).

Deploy: `dotnet build mods/fast-wireless-automation-fix/src/FastWirelessAutomationFix/FastWirelessAutomationFix.csproj -c Release` -> 0 warnings, 0 errors.
Landed in `C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Local\FastWirelessAutomationFix\` (FastWirelessAutomationFix.dll 7168 B, mod.yaml, mod_info.yaml). Checked beforehand: no folder of that name existed there (only an unrelated `FastTrack`), so nothing was overwritten.

## Test
None written. The TestRunner harness has no way to load a second save or game in one run (`Kit/SaveFile.cs` only captures/restores a single building; `Worldgen.cs` uses `App.LoadScene("frontend")` for worldgen only). Per the spec I did not work around it. The second-load case must be checked by hand: load save A, then save B in one session, expect no `[ERROR] FastWirelessEmitterComplete` and the log lines `[FastWirelessAutomationFix] Patched ...`. Adding a `SaveLoader`-driven "load another save" helper to the harness would be the prerequisite for a test.

## Uncommitted
Everything above, plus the pre-existing `common` pointer and `debug.log`.

## ICON feedback
- Handoff brief pointers: none governed this work (the per-mod retro lessons were unrelated; `game-updates.md` not needed). Nothing in the brief covered Harmony compat patching, which was in the OM-011 code instead.
- Tooling friction: none with `ks`; I did not need `ks read`.
