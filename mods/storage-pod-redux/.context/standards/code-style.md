---
doc_id: doc-3f8274c8e738
description: "C# formatting, imports, comments, Harmony patch and component conventions for Storage Pod Redux (brings back the Storage Pod and Cool Pod), with this mod's own layout."
applies_to:
  - "src/**"
---
# Code Style Guide

## Formatting
**Id:** blk-645fab47f254

### Indentation
**Id:** blk-6f333a1f8d0d

- Tabs, one per level, in every `.cs` file (`mods/bitshifter/src/Bitshifter/Mod.cs`).
- Project files (`.csproj`, `common/oni-mod.props`, `common/oni-mod.targets`) use two spaces.
- Files are committed with LF line endings; Windows checkouts convert them to CRLF (`core.autocrlf` is true), so do not hand-normalise line endings.

### Line Length
**Id:** blk-c130e4cc3f34

- No limit is configured and the repository has no `.editorconfig`.
- Wrap long calls and string concatenations after an argument or before a `+`, and indent the continuation one tab. The start-up log call in `mods/jumper-bridges/src/JumperBridges/Mod.cs` is the pattern.
- TODO(owner): decide whether to add an `.editorconfig` recording tabs and brace placement so editors enforce what the code already does.

### Braces
**Id:** blk-8a5c70148048

- Opening brace on its own line (Allman) for namespaces, types, members and multi-line control blocks.
- A body of one short statement goes on the next line without braces:

```csharp
foreach (Mod mod in mods)
{
	if (!mod.IsEnabledForActiveDlc())
		continue;
	...
}
```

- Multi-line initialisers and enums end with a trailing comma (`YieldToWorkshopIds` in `mods/bitshifter/src/Bitshifter/Mod.cs`, `CoolPodMode` in `mods/storage-pod-redux/src/StoragePodRedux/Options.cs`).

## Language and Toolchain
**Id:** blk-bad4e306ff10

- C# 9 on .NET Framework 4.8 (`LangVersion` 9.0 and `TargetFramework` net48 in `common/oni-mod.props`). Namespace blocks, not file-scoped namespaces; no nullable reference types.
- Mods compile against the game's own assemblies (`common/oni-mod.targets`): `Assembly-CSharp`, `Assembly-CSharp-firstpass`, `0Harmony` and Unity modules. The game's .NET Framework lacks some newer APIs (`ConditionalWeakTable.AddOrUpdate` is missing, see `mods/detect-any-element/src/DetectAnyElement/Patches.cs`).
- Optional references are switched on in the mod csproj before the targets import: `ReferenceUnityUI`, `ReferenceImageConversion`, `UsePLib` (PLib is ILRepack-merged into the mod DLL) and `UseTranslations` (compiles `common/src/Translations.cs` in).
- One namespace per mod, equal to its `RootNamespace`. Subfolders organise files and do not add namespaces (`mods/upgrade-planner/src/UpgradePlanner/Plan`, `Rules`, `Tool`, `UI` all declare `namespace UpgradePlanner`).

## Imports/Includes
**Id:** blk-31a1dbd322c4

### Organization
**Id:** blk-93a7df3af5e2

`using` directives sit above the namespace, in one block with no blank lines, sorted alphabetically with `System.*` first. Aliases for `ModStrings` branches come last.

1. `System.*`
2. Everything else alphabetically: `HarmonyLib`, `Isochronous`, `KMod`, `Newtonsoft.Json`, `PeterHan.PLib.*`, `ProcGenGame`, `UnityEngine`
3. Aliases (`using TOOL = SweepZones.ModStrings.UI.TOOLS.SWEEPZONES;`)

### Example
**Id:** blk-7261503b82df

```csharp
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Isochronous;
using Klei;
using KMod;
using STRINGS;
using UnityEngine;
```
(`mods/salty-surf/src/SaltySurf/Mod.cs`)

`Debug.Log` resolves either to `UnityEngine.Debug` or to the game's own global `Debug` class, so several `Mod.cs` files omit `using UnityEngine`. When a file imports `System.Diagnostics`, qualify the call as `UnityEngine.Debug.Log` (`mods/just-fall-already/src/JustFallAlready/FallPoke.cs`).

## Comments
**Id:** blk-23bc077eed9e

### When to Comment
**Id:** blk-3d37bcce99a4

- On every non-trivial class, an XML `<summary>` that says what the game does and what the patch changes, so a reader can check the patch against the game code. The long summary on `OvercrowdingMonitor_UpdateRegion_Patch` in `mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails/Mod.cs` is the model.
- Where a game quirk forces an odd line: `// The game's .NET Framework has no AddOrUpdate on this table.`
- Where another mod is involved, name it and its Workshop id (`mods/bitshifter/src/Bitshifter/Mod.cs`, `mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs`).
- Escape angle brackets in XML comments as `&lt;` and `&gt;`.

### When NOT to Comment
**Id:** blk-655abaa90905

- Code that reads clearly from its names.
- Commented-out code; delete it.
- Restating what a line does.

### Format
**Id:** blk-85841718b47c

```csharp
/// <summary>
/// The "Any" choice. It is a tag like the elements a sensor's Filterable holds, so it rides
/// in the same saved field and copies with the building's settings.
/// </summary>
public static class AnyElement

// Single-line note for a quirk or a reason.
```

## Control Structures
**Id:** blk-3bd34806c643

### Conditionals
**Id:** blk-e4a5925c561f

Guard clauses and early `return` or `continue` keep nesting shallow (`SettleSimBackwalls.UpdateFromSim` in `mods/natural-backwalls-everywhere/src/NaturalBackwalls/SettleSimPatch.cs`):

```csharp
if (!Grid.IsValidCell(cell))
	continue;
ushort idx = data->backwallElement[cell];
if (idx == ushort.MaxValue || idx >= elements.Count)
	continue;
```

### Loops
**Id:** blk-308a838456d3

- Plain `for` and `foreach`. Code that runs every sim tick or frame avoids LINQ and allocations; reuse scratch lists (`scratch` in `mods/just-fall-already/src/JustFallAlready/FallPoke.cs`).
- Local types are written out (`Tech tech = ...`). `var` appears mainly in `mods/upgrade-planner`, `mods/natural-backwalls-everywhere` and the test projects; follow the surrounding file.

### Strings
**Id:** blk-2302785791ec

- Log and UI text is built with `+` concatenation, or `string.Format` on a `LocString`. String interpolation is not used in mod code.
- Player-visible text is a `LocString` field in `ModStrings`, never a literal at the call site (see [naming conventions](naming-conventions.md)).

## Harmony Patches
**Id:** blk-45b3c9a86343

- A patch is a static class with `[HarmonyPatch(typeof(Type), nameof(Type.Method))]`, or `[HarmonyPatch(typeof(Type), "PrivateMethod")]` for a non-public method, and methods named `Prefix`, `Postfix`, `Transpiler` or `Finalizer` (`mods/bitshifter/src/Bitshifter/Patches.cs`).
- Game state arrives through Harmony parameters: `__instance`, `__result`, `___field`. Fields read repeatedly use `AccessTools.FieldRefAccess` kept in a `static readonly` field (`mods/detect-any-element/src/DetectAnyElement/Patches.cs`).
- A prefix returns `true` to let the game's method run and `false` only when it replaces the method (`mods/dont-count-eggs-on-rails`, `mods/bitshifter` `UpdateVisuals`).
- A patch that depends on an option implements `Prepare()` (`mods/jumper-bridges/src/JumperBridges/Patches.cs`); a patch with several targets implements `TargetMethods()`.
- `UserMod2.OnLoad` calls `base.OnLoad(harmony)`, which applies every patch in the assembly, then registers libraries and logs the version. A mod that must patch after all others applies its patches from `OnAllModsLoaded` instead (`mods/bitshifter/src/Bitshifter/Mod.cs`).
- Transpilers search for the instruction they change and report when it is not found; see [error handling](error-handling.md).

## Game Components
**Id:** blk-f8667089c191

- A component extends `KMonoBehaviour`, overrides `OnPrefabInit`, `OnSpawn` and `OnCleanUp` and calls `base` from each. Sibling components come in through `[MyCmpGet]` or `[MyCmpReq]` (`mods/no-magic-fridges/src/NoMagicFridges/FridgeThermostat.cs`).
- Saved state uses `[Serialize]` fields with `[SerializationConfig(MemberSerialization.OptIn)]` on the class (`mods/sweep-zones/src/SweepZones/ZoneStore.cs`).
- Components are added to prefabs from a `DoPostConfigureComplete` postfix with `go.AddOrGet<T>()`.

## Project-Specific Rules
**Id:** blk-d6a5f3394410

- Mod options are a PLib `POptions` class marked `[JsonObject(MemberSerialization.OptIn)]` and `[ConfigFile(SharedConfigLocation: true)]`, read through a static `Load()` (`mods/vent-freeze-fix/src/VentFreezeFix/Options.cs`) or through `SingletonOptions<Options>` (`mods/storage-pod-redux`). Option titles are string-table keys, so the translation follows the language.
- Every mod logs one line at load: `[<Name>] Loaded version <assembly version>`.
- Tests live in a separate `<Name>.Tests` project and are written as `[OniTest]` coroutines against the real game.

### Storage Pod Redux specifics
**Id:** blk-7e6babf9ac79

- `UsePLib` and `UseTranslations`. `Options` derives from `SingletonOptions<Options>` and carries `[RestartRequired]` on the class; `CoolPodMode` is an enum whose values each carry an `[Option]` (`src/StoragePodRedux/Options.cs`).
- `StoragePodReduxMod.OriginalActive` is a static property with a private setter, set in `OnAllModsLoaded` from `ModDetection.IsEnabled`; `Buildings.Decide(mods)` then decides which buildings exist.
- `Patches.cs` is a `public static class Patches` with nested patches. `BuildingConfigManager_RegisterBuilding_Patch.Prefix` returns `Buildings.IsEnabled(config)`, so a disabled building is never registered.
- `StoragePodConfig` and `CoolPodConfig` use the ids of the original mod's buildings.

## Related
**Id:** blk-38c0e0a39ec7

- See also: [naming conventions](naming-conventions.md)
- See also: [error handling](error-handling.md)
