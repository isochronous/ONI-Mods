# Unit 07 report: S4 border patch

Commit **04e1d27** `OM-007: flat border draw and POI bounds` on OM-007-smooth-borders. Three new files; `Mod.cs` unchanged. Nothing in common/ was touched, and the common pointer was not staged.

## Files

| File | What it does |
|---|---|
| `mods/smooth-borders/src/SmoothBorders/Border.cs` | `Border.Setting(WorldGenSettings, string)` stands in for the two `GetIntSetting` reads. It always reads the real value first. While this thread's world is flat it returns the stored thickness F for `"WorldBorderThickness"` and 0 for `"WorldBorderRange"`; otherwise it returns the real value. The class also holds `public static bool Patched` (set by the transpiler) and the `[ThreadStatic]` flat flag and thickness (`BeginFlat`, `End`). |
| `mods/smooth-borders/src/SmoothBorders/WorldGen_DrawWorldBorder_Patch.cs` | `[HarmonyPatch(typeof(WorldGen), "DrawWorldBorder")]`. **Transpiler:** see "IL match" below. **Prefix:** does nothing unless `Patched` is set. It reads T and R. A world with T ≤ 0 is left to the game, with one log line. Otherwise it calls `Options.Load()` once and works out F with `Groups.Thickness` (the group setting, or `max(1, T-R)` for an unknown pair). It clones `simData.cells` and runs the reverse-patched vanilla method on the copy, with `new SeededRandom(0)`, a throwaway `borderCells` and a no-op progress callback. Unless exactly 4 rectangles were added, it removes them, logs, and leaves the world vanilla. With 4, it moves them out of `POIBounds` into `__state` and calls `Border.BeginFlat(F)`. Any exception removes the partly added rectangles, clears the flag and logs `"[SmoothBorders] Flat border skipped, this world keeps the game's border: " + e.Message`. **Postfix:** always clears the flag. If the original did not add exactly 4 rectangles, it logs and leaves the list. Otherwise it logs `"<world filePath>: group <Id>, flat wall F tiles"` and, for each edge, keeps the deeper rectangle (width for left and right, height for bottom and top). A tie keeps the vanilla rectangle, so the list matches vanilla when nothing grew. For each edge where the flat rectangle is kept, it logs `"<world>: <edge> POI rectangle grew from V to F tiles"`. |
| `mods/smooth-borders/src/SmoothBorders/WorldGen_DrawWorldBorder_Vanilla.cs` | `[HarmonyReversePatch(HarmonyReversePatchType.Original)]` stub `DrawWorldBorder(WorldGen instance, ref WorldgenSimData, Chunk, SeededRandom, ref HashSet<int>, ref List<RectInt>, WorldGen.OfflineCallbackFunction)`, which matches the decompiled signature (DrawWorldBorder.il lines 2-9). |

**Deviation from the plan text:** the vanilla rectangles go to the postfix through Harmony's `__state`, not thread-static storage. `__state` is per call, so a world can never see another world's rectangles. Only the flat flag and F are thread-static, because `Border.Setting` is called from inside the method and has no other way to get them. The prefix also clears the flag at the start of every call.

## IL match the transpiler relies on

Game build U59-744825, `ProcGenGame.WorldGen.DrawWorldBorder`, 556 instructions:

```
IL_0024: ldstr "WorldBorderThickness"
IL_0029: callvirt instance int32 ProcGen.WorldGenSettings::GetIntSetting(string)   -> call int32 SmoothBorders.Border::Setting(WorldGenSettings, string)
IL_0035: ldstr "WorldBorderRange"
IL_003a: callvirt instance int32 ProcGen.WorldGenSettings::GetIntSetting(string)   -> call int32 SmoothBorders.Border::Setting(WorldGenSettings, string)
```

The patch applies only when both conditions hold for each key:
- the method loads that string exactly once (`ldstr`)
- the very next instruction `Calls(GetIntSetting(string))`

Only the opcode and operand of the call are changed, so labels and blocks stay. In every other case:
- the method's IL comes back unchanged
- `Patched` stays false
- one line is logged: `[SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them`

Every transpiler run first sets `Patched` to false, so a re-patch by another mod re-checks the IL.

## Verification

**Build** (the acceptance command):

```
> dotnet build mods/smooth-borders/src/SmoothBorders/SmoothBorders.csproj -c Release -p:ModDeployFolder=none
  SmoothBorders -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders\bin\Release\SmoothBorders.dll
  ...
  Merge succeeded in 0.8489081 s
  Merged PLib into SmoothBorders.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The full sln build was not run, because common/ TestRunner is being changed by unit 05.

**Offline transpiler check (no game).** A scratch net48 console, `scratchpad\ilcheck\` (not committed), loads the game's Assembly-CSharp from `F:\SteamLibrary\...\Managed` and the built mod DLL. It reads the real IL with `PatchProcessor.GetOriginalInstructions` and calls the mod's transpiler. The game's `Debug.Log*` calls are redirected to the console. Output:

```
target: Void DrawWorldBorder(ProcGenGame.WorldgenSimData ByRef, Chunk, SeededRandom, System.Collections.Generic.HashSet`1[System.Int32] ByRef, System.Collections.Generic.List`1[UnityEngine.RectInt] ByRef, OfflineCallbackFunction)
original instructions: 556
Patched after real IL: True
  changed [13] ldstr "WorldBorderThickness" ; callvirt System.Int32 ProcGen.WorldGenSettings::GetIntSetting(System.String target)  ->  call static System.Int32 SmoothBorders.Border::Setting(ProcGen.WorldGenSettings settings, System.String name)
  changed [18] ldstr "WorldBorderRange" ; callvirt System.Int32 ProcGen.WorldGenSettings::GetIntSetting(System.String target)  ->  call static System.Int32 SmoothBorders.Border::Setting(ProcGen.WorldGenSettings settings, System.String name)
count equal: True, changed: 2, all changes call Border.Setting: True
  [log] [SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them
duplicate range string: IL unchanged True, Patched False
  [log] [SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them
thickness call replaced: IL unchanged True, Patched False
Patched after re-run on real IL: True
applied: prefixes 1, postfixes 1, transpilers 1, Patched True
reverse stub threw: NullReferenceException
RESULT: PASS
```

What each line shows:
- **Real IL:** exactly the two calls change.
- **Fail-safe:** two mutated copies are handled safely: an extra `"WorldBorderRange"` load, and the thickness call replaced. Each leaves the IL unchanged with `Patched` false and logs the line.
- **Patch classes:** both classes apply through `CreateClassProcessor(...).Patch()`, as `PatchAll` does: 1 prefix, 1 postfix and 1 transpiler.
- **Reverse patch:** the stub's body was replaced. Called with a null instance, it fails inside the game's code with NullReferenceException, not the stub's NotImplementedException.

## What S6 must prove (needs the game)

1. **Flat draw.** With R = 0 every row and column is exactly F deep, apart from the vacuum skip. This is test T3/T4.
2. **The reverse patch sees the original IL, not the transpiled one** (plan risk a). In the check above the reverse patch was applied before the transpiler; `PatchAll`'s order depends on type order. If the probe ran the transpiled IL, the "vanilla" rectangles would be the flat ones: T5 (identity at defaults) would fail, and maximum-config runs would log no "rectangle grew" lines. T5 checks this.
3. **POIBounds merge gives vanilla placements at the defaults** (T5): no "rectangle grew" line at the defaults.
4. **No template holes above the defaults** (T4a).
5. **`Options.Load()` re-reads the file for each world** (plan risk b). T4b checks this.
6. **The probe's cost and side effects.** It clones about 98k `Sim.Cell` per primary world and calls `GetTerrainCellsForTag`, which only reads. Nothing in the probe should change the real cells, `borderCells` or `myRandom`. T5's identity check covers this indirectly.
7. **Log lines arrive on the worldgen thread** in the form the S6 log capture expects:
   - `[SmoothBorders] <filePath>: group <Id>, flat wall F tiles`
   - `... <edge> POI rectangle grew from V to F tiles`
   - `... no wall in the game, left as it is`

   Group Ids are `Primary`, `Small`, `Moonlets`, `TinyMoonlets`, or `own (T, R)` for an unknown pair.

## ICON tooling friction

None. `ks read --store C:/dev/ONI-Mods blk-f04d3902a855` and `blk-016318d101d7` both resolved directly. The handoff brief matched nothing for `mods/smooth-borders`; the plan's record ids were enough.
