---
tags: code-style
doc_id: doc-84c05e729b11
description: "Naming conventions for Natural Backwalls Everywhere: files, variables, methods, types, ModStrings keys, ids and tags, plus the NaturalBackwalls patch class pattern, biome and zone string groups and test class names"
applies_to:
  - "src/**"
---

# Naming Conventions

## Files and Directories
**Id:** blk-529e052f8306

### General Rules
**Id:** blk-bc1605978ba1

- A mod lives in `mods/<kebab-case-name>/` (`natural-backwalls-everywhere`, `just-fall-already`).
- Its code is under `mods/<name>/src/<PascalName>/` with a matching `src/<PascalName>.Tests/` project. `AssemblyName`, `RootNamespace` and the project folder share one PascalCase name (`DontCountEggsOnRails`).
- C# files are PascalCase and named for the main type they hold. One concept per file; small related types may share a file (`Display` in `mods/bitshifter/src/Bitshifter/Patches.cs`).
- Release tags are `<mod-folder>-v<version>` (repository README).

### Examples by Type
**Id:** blk-a8ba49e2c00b  

| Type | Convention | Example |
|------|------------|---------|
| Mod entry class | `Mod.cs`, class `<Name>Mod : UserMod2` | `BitshifterMod` in `Mod.cs` |
| Harmony patches | `Patches.cs`, or a file named for what is patched | `PlanetoidPatch.cs`, `SettleSimPatch.cs` |
| Building config | `<Building>Config.cs`, class `<Building>Config : IBuildingConfig` with `public const string ID` | `SmartWeightPlateConfig.cs` |
| Side screen | `<Building>SideScreen.cs` | `SmartWeightPlateSideScreen.cs` |
| Components | `<Thing>.cs` named for the component | `FridgeThermostat.cs`, `ZoneStore.cs` |
| Strings | `ModStrings.cs` | `mods/salty-surf/src/SaltySurf/ModStrings.cs` |
| Options | `Options.cs`, class `Options` | `mods/vent-freeze-fix/src/VentFreezeFix/Options.cs` |
| Tests | `<Topic>.cs` in `<Name>.Tests`, one static class per topic | `Reach.cs`, `Gates.cs`, `Persistence.cs`; `Kit.cs` for shared helpers |
| Mod metadata | `mod.yaml`, `mod_info.yaml` in the project folder | `staticID: "Isochronous.<PascalName>"` |
| Translations | `translations/<locale>.po`, template `<Name>.pot` | `mods/detect-any-element/src/DetectAnyElement/translations` |
| Art | `anim/` (kanim files), `assets/` | `bitshifter_ribbon_reader_kanim` |
| Release material | `mods/<name>/publish/`, `CHANGELOG.md`, `README.md` | |

## Code Naming
**Id:** blk-24046c551181

### Variables
**Id:** blk-9d32bd324e3e

- camelCase for locals, parameters and private instance fields; no underscore prefix.
- PascalCase for constants (`ArmSeconds`, `Bits`, `TechId`) and for `static readonly` values that act as constants or lookups (`Standard`, `YieldToWorkshopIds`, `Contested`).
- camelCase for private static mutable state and caches (`armed`, `scratch`, `lastGrid`, `symbols`, `anyRows`).
- Descriptive names; single letters only for loop indexes and the `e` of a catch.

```csharp
private const float ArmSeconds = 2f;
private static readonly Dictionary<int, float> armed = new Dictionary<int, float>();
```
(`mods/just-fall-already/src/JustFallAlready/FallPoke.cs`)

### Functions/Methods
**Id:** blk-ea141dfef780  
**Tags:** harmony-patching

- PascalCase, starting with a verb: `Apply`, `Decide`, `SwapTable`, `CopyRangeFrom`, `ApplyRange`.
- Game callbacks keep the game's names (`OnSpawn`, `OnPrefabInit`, `OnCleanUp`, `Sim1000ms`).
- Harmony methods are named for their kind: `Prefix`, `Postfix`, `Transpiler`, `Finalizer`, plus `Prepare`, `TargetMethod`, `TargetMethods`.

### Classes/Types/Interfaces
**Id:** blk-dfa66a165b36  
**Tags:** harmony-patching

- PascalCase nouns. Interfaces take an `I` prefix (`IPlanTab` and `ITabView` in `mods/upgrade-planner`, and the game's own).
- Harmony patch classes: `<PatchedType>_<PatchedMethod>_Patch` (`LogicRibbonReader_GetBitDepth_Patch`, `Db_Initialize_Patch`, `GeneratedBuildings_LoadGeneratedBuildings_Patch`). A patch for a type with one concern may use `<Type>_Patch` (`LiquidVentConfig_Patch`). Patches nested in a holder class may drop the suffix (`Constructable_OnSpawn` in `ReachTablePatches`).
- Classes are `sealed` unless built for inheritance. The mod class is `public sealed`. Types the game does not reach are `internal`.
- Option enums and nested helper types are PascalCase (`CoolPodMode`, `BridgeKind`).

### Booleans
**Id:** blk-6937744d70bd

- Prefix with `Is`, `Has`, `Can`, `Should` or name the state: `IsEnabled`, `OriginalActive`, `isSpawned`, `warnedMissingInternals`.

### Strings (`ModStrings`)
**Id:** blk-7aecb9e1983d  
**Tags:** mod-strings

- `ModStrings` mirrors the game's `STRINGS` tree: nested static classes and `LocString` fields in UPPER_SNAKE_CASE. `ModStrings.BUILDINGS.PREFABS.<BUILDINGID>.NAME` is registered as `STRINGS.BUILDINGS.PREFABS.<BUILDINGID>.NAME`, the key the game looks a building name up by (`mods/smart-weight-plate/src/SmartWeightPlate/ModStrings.cs`).
- Mod-private text goes under a branch named for the mod in capitals (`ModStrings.SALTYSURF.ELEMENT_LIST`, `ModStrings.JUMPERBRIDGES.OPTIONS`).
- Option keys are written out as `STRINGS.<MOD>.OPTIONS.<OPTION>.NAME` and `.TOOLTIP`; categories as `STRINGS.<MOD>.OPTIONS.CATEGORY.<NAME>`.
- Long branches are shortened with an alias at the top of the file (`using OPTIONS = NaturalBackwalls.ModStrings.NATURALBACKWALLS.OPTIONS;`).

### Ids and Tags
**Id:** blk-068a52f8eb11  

- Building ids are PascalCase and match the config class (`SmartWeightPlate`); a mod that replaces another mod's buildings reuses that mod's ids (`StoragePodConfig`, `CoolPodConfig` in `mods/storage-pod-redux`).
- Tags and PAction ids carry a mod prefix: `DetectAnyElement_Any`, `Isochronous.SweepZones.Tool`.
- The `staticID` in `mod.yaml` is `Isochronous.<PascalName>`.
- Test names start with the mod's name or an abbreviation of it: `[OniTest("Bitshifter: a NOT gate on a ribbon inverts every bit")]`, `VFF:` for Vent Freeze Fix.

## Project-Specific Conventions
**Id:** blk-4365c851396d

### Natural Backwalls Everywhere specifics
**Id:** blk-f63e1ff3f8ed  
**Tags:** mod-strings, harmony-patching, in-game-testing, biome-group

- Biome and zone groups are UPPERCASE branches in `ModStrings.NATURALBACKWALLS` (`BIOMES`, `PATTERNS`, `ZONES`); biome group ids are the keys of the option dictionaries.
- Patch classes follow `<Type>_<Method>_Patch` (`TerrainCell_ApplyBackground_Patch`, `WorldGen_GenerateOffline_Patch`).
- Test classes: `Generated`, `Guards`, `Looks`, `Planning`, `Support` in `src/NaturalBackwalls.Tests`.

## Related
**Id:** blk-79a09c775370

- See also: [code style](code-style.md)
- See also: [error handling](error-handling.md)
