---
doc_id: doc-e8c1c2d0eab9
description: "Rules for implementing the game's own interfaces in this repo's mods, which build against net48: the entity-config interfaces' default methods have to be written out"
---
# Game interfaces under net48

Every mod builds with `TargetFramework` net48 and C# 9, set once in `common/oni-mod.props` (decided on 2026-10-17, OM-006: stay on net48).

## Entity configs
**Id:** blk-d210f7f86e10

The game gives `IEntityConfig.GetDlcIds()` and `IHasDlcRestrictions.GetAnyRequiredDlcIds()` default bodies. A net48 build cannot use default interface methods (error CS8702), so a class implementing them writes both out with the defaults' values:

```csharp
public string[] GetAnyRequiredDlcIds()
{
	return null;
}

public string[] GetDlcIds()
{
	return null;
}
```

`GetDlcIds` stays null so the game's entity loader takes the `IHasDlcRestrictions` branch and reads `GetRequiredDlcIds` and `GetForbiddenDlcIds`. A config that adds a building (`IBuildingConfig`) is unaffected: that is a class the mod inherits from. Ovagros Overhead's `VineMotherCeilingConfig` and `VineBranchCeilingConfig` were the first entity configs in the repo (`mods/ovagros-overhead/src/OvagrosOverhead/`).

## Why the game can and the mods cannot
**Id:** blk-f2de0f6edb04

The game runs on Unity's Mono, whose `mscorlib.dll` (in the game's `OxygenNotIncluded_Data/Managed`) carries `RuntimeFeature.DefaultImplementationsOfInterfaces`; the net48 reference assemblies the mods compile against do not, and the compiler refuses default interface bodies without it. Compiling against the game's own `mscorlib.dll`, or targeting `netstandard2.1`, would lift this; either is a change to `common/oni-mod.props` for every mod and was left for later.
