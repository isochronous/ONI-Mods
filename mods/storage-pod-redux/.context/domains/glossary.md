---
tags: storage-building
witness_digest: "sha256:4ca967e5ba46efa9209fa2082f56d2c5d8c4bd31f26343f9b36b9df3d30ed70e"
witness: loose
applies_to:
  - "src/StoragePodRedux/**"
doc_id: doc-7eb761dcb233
description: "Glossary for Storage Pod Redux: abbreviations (kanim, PLib), terms to avoid such as Fridge Pod, and definitions of pod, prefab ID and vanilla fridge terms with the files that use them"
---

# Storage Pod Redux glossary

Terms for the Storage Pod Redux mod. For the mod's entities, rules and code paths see [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-34c753e3b961  
**Tags:** kanim-art, mod-options

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | The game the mod is for |
| kanim | Klei animation | Pod art: `storage_pod_kanim`, `cool_pod_kanim` |
| PLib | Peter Han's mod library | Options screen and mod init |
| W / kW | Watt / kilowatt | Cool Pod power and heat figures |

## Terms to Avoid
**Id:** blk-c8df511c1d86  
**Tags:** mod-compatibility

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| Fridge Pod | Cool Pod | Fridge Pod is Ronivan's Legacy's building; this mod's is the Cool Pod |
| Storage Pod mod | Storage Pod Redux, or "the original mod" | The original Skyrunner mod shares the prefab IDs; the two must be told apart |

## Cross-Domain Terms
**Id:** blk-99feade92c06

### Storage Pod
**Id:** blk-46849ef15e2a  
**Tags:** building-config  
**Definition**: This mod's 1x1 solid storage building (prefab `StoragePodConfig`), 5,000 kg by default.  
**Used in**: `StoragePodConfig.cs`, `Options.cs`, `Patches.cs`.

### Cool Pod
**Id:** blk-dc88b27b42b1  
**Tags:** refrigerator  
**Definition**: This mod's 1x1 powered fridge (prefab `CoolPodConfig`), 50 kg of food by default.  
**Used in**: `CoolPodConfig.cs`, `Buildings.cs`, `Options.cs`.

### Mini Fridge
**Id:** blk-5f575abce47b  
**Tags:** refrigerator, building-config  
**Definition**: The vanilla rocket-capable fridge (`MiniFridgeConfig`) whose component set the Cool Pod copies. The Storage Pod likewise copies the Storage Bin (`StorageLockerConfig`).  
**Used in**: `CoolPodConfig.cs`, `StoragePodConfig.cs`.

### BuildLocationRule.Anywhere
**Id:** blk-52d73c8fcf50  
**Tags:** building-config  
**Definition**: The game rule that lets a building be placed without a floor or wall behind it, which is why the pods fit inside rockets.  
**Used in**: both config classes.

### Kitchen refrigerator tag
**Id:** blk-e675dd0eae9b  
**Tags:** refrigerator, building-config  
**Definition**: `RoomConstraints.ConstraintTags.KitchenRefrigerator`; a building with it counts as the fridge a Kitchen room requires. The Cool Pod has it.  
**Used in**: `CoolPodConfig.ConfigureBuildingTemplate`.

### Full output
**Id:** blk-c0f37de3b12d  
**Tags:** refrigerator, building-config  
**Definition**: The logic port (`FilteredStorage.FULL_PORT_ID`) that sends a green signal when the storage is full; the Cool Pod has the same one as the Refrigerator.  
**Used in**: `CoolPodConfig.CreateBuildingDef`.

### Prefab ID
**Id:** blk-a0285ad72c56  
**Tags:** saved-state, building-config  
**Definition**: The string a building is saved under. Reusing the original mod's IDs is what keeps old saves' pods.  
**Used in**: `StoragePodConfig.ID`, `CoolPodConfig.ID`.

### Static ID
**Id:** blk-ffa41e0b104a  
**Tags:** mod-compatibility  
**Definition**: A mod's identity in `mod.yaml` (`staticID`). Used to detect other mods: `Storage Pod` and `RonivansLegacy_ChemicalProcessing`.  
**Used in**: `Mod.cs`, `ModDetection.cs`.

## Related
**Id:** blk-cb877d63299e

- See also: [entities](entities.md)
