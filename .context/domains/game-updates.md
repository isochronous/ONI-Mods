---
tags: game-update-compatibility
doc_id: doc-27e357c98796
description: "How ONI-Mods gets ready for an Oxygen Not Included update: where the new and previous game builds are, how breakage is found, how fixes are split between shipping now and waiting for the release, and what U60 changed"
applies_to:
  - "mods/**"
  - "common/tools/TestRunner/**"
witness: none
---

# Game updates

## Where the builds are
**Id:** blk-c7f7236acdfb

- The build reads the game from the Steam install at `F:\SteamLibrary\steamapps\common\OxygenNotIncluded` through `common/oni-mod.props`; no copy of the game DLLs is kept in the repo. Switching Steam to Klei's public testing branch switches what every mod builds against.
- A frozen copy of the previous build is at `F:\ONI-Frozen\APP`. It is the baseline for diffing an update and for checking that a fix still works on the live build.

## Finding what an update breaks
**Id:** blk-47c67ad69006

A clean `dotnet build` is not enough: patches that name their target by string or `nameof`, and reflection on private members, compile fine when the target is gone. The checks that found U60's breaks:

- Build the solution against the new build and list errors per mod.
- Run each mod's Harmony patch classes for real (Prepare, target lookup, parameter binding, transpilers) against the new DLLs in a scratch .NET 8 Harmony host; a net48 host cannot load the game's types.
- Resolve every string-named member with Harmony's `AccessTools`.
- Diff the decompiled source of every patched game method between the frozen previous build and the new one.
- For a fix meant to work on both builds, scan the built DLL's member references against the previous build's assemblies (Mono.Cecil): a DLL built against the new game can reference an overload or return type the old one lacks.
- Then the in-game smoke test: every suite, with one shared loadout. A test-build `Assembly-CSharp.dll` is a debug build, so transpilers that match it are checked again against the release build.

When reading `Player.log` after a smoke test, tie each warning to the test running at that moment: fail-safe tests feed a mod altered copies of a method on purpose and draw the mod's own "has changed" warning.

## Two lanes for update fixes
**Id:** blk-46019a2fa7f8

Decided by Jeremy for U60 (2026-11-07, OM-012):

- A fix that works on both the live build and the new one ships now: resolve a patch target by name, patch whichever method exists (one patch class per build, each with a `Prepare` that checks its target), avoid new-only types. `minimumSupportedBuild` stays.
- A change that needs API only the new build has waits on a held branch stacked on the ship-now branch, merged when the update is officially released, and raises `minimumSupportedBuild` to the testing build's number (the release build is higher).
- While the game is on a testing branch, no run may write over an existing save: a save from a later version does not open in an earlier one.

## What U60 changed (testing build 762230, from U59 744825)
**Id:** blk-d1c23a566ac6

- Descriptors: `MechanicalSurfboard` (and other descriptor providers) moved from an explicit `IGameObjectEffectDescriptor.GetDescriptors` to `CollectDescriptors(GameObject, List<Descriptor>)`; `GameUtil.GetAllDescriptors` is obsolete in favour of `CollectAllDescriptors`.
- Crops: `Crop.CropVal` replaced `cropId`/`numProduced` with a `products` list; `PlantAttributes.YieldAmount` became `YieldFactor`; `ExtendEntityToBasicPlant` takes `harvest:` and `EntityTemplates.ConfigureCrop` sets the crop; `PlantableCellQuery.Reset` changed signature; backwall planting is new.
- `MaterialSelector.OnSelectMaterial` became private; `MaterialSelectionPanel.ConfigureScreen` callbacks return `BuildableState` where they returned `bool`.
- `Constructable.GetOffsets` returns `IReadOnlyList<CellOffset>`.
- Events: `Subscribe`/`Unsubscribe` with `IntraObjectHandler` are obsolete; `Subscribe(int, Action<object,object>, object)` and `Unsubscribe(ref int)` exist on U59 too. The context-object `GameScenePartitioner.Add` overload is U60-only.
- Timelapse and preview images are written through a new private `Timelapser.CapturePng`, bypassing `WriteToPng`.
- Worldgen progress is reported from a worker thread, where `Time.realtimeSinceStartup` throws.
- Fridges: `temperatureAdjuster.CoolingOnly` is set and items at or below the fridge's temperature get no warming reservoir, which fixes what No Magic Fridges was made for; it is retired.
- PLib 4.25.0: `EquipmentTemplates.CreateEquipmentDef` gained an overload, so PLib's building manager logs an ambiguity; no mod here uses it.
