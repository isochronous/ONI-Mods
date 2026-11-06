# Unit 11 report — implement: FixtureArea.Clear misses buildings on no layer

Model tier: default

## Diff summary

common/ commit `fb3b432fa8e2363052c4ae950f61b70c78ce7cba` on branch OM-009-jumper-heavy-wire (not pushed):
`TestRunner: fixture clear removes buildings on no layer`

- `tools/TestRunner/FixtureArea.cs` (+55/-1)
  - `Clear(rect, keep)` keeps collecting what `Grid.Objects` names in the rectangle. It now also adds every
    `Building` in the scene (`Object.FindObjectsByType<Building>(FindObjectsSortMode.None)`). That covers
    BuildingComplete and BuildingUnderConstruction. A building is added when one of its `PlacementCells` is in
    the rect (falls back to its origin cell if it has no PlacementCells). `keep` (the box's life support) is
    still skipped. The search is generic: it uses no mod's types. Inactive prefab templates are not returned.
  - New `internal static Track(GameObject)` and a private `built` list. After a clear's final frames,
    `WarnStanding` drops destroyed entries and logs
    `[tag] still standing in <rect> after the clear: <name> at <cell>; ...` (Debug.LogWarning) for every tracked
    object, except `keep`, whose placement cells (or cell) are still in the rect. Exceptions are logged with the
    same "threw" warning pattern Clear already uses.
  - Class summary updated.
- `tools/TestRunner/Kit/World.cs` (+1): `Build` calls `FixtureArea.Track(go)` after `def.Build`.
- `tools/TestRunner/README.md` (+3/-1): the clear paragraph now says that buildings touching the area go too,
  built or under construction, even when they sit on no object layer, and that the log warns about anything
  `t.World.Build` made that is still standing.

The "cleared ... N object(s) destroyed" count now includes the off-layer buildings. Expected after fix (from
unit 10): WireBridgeOnPlateEnd 12, BatteryBridgeLamp 10. This has not been checked in game. The game was not
launched.

## Build

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` from C:\dev\ONI-Mods, exit 0. Output tail:

```
  TestRunner -> C:\dev\ONI-Mods\common\tools\TestRunner\bin\Release\TestRunner.dll
    13 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.34
```

The first build used `FindObjectsOfType`, which added a CS0618 obsolete warning. I switched to
`FindObjectsByType(..., None)`, and the final build shows no warning from FixtureArea.cs or World.cs. The 13
warnings are existing ones in other projects.

## Store notes (for the coordinator to land)

- `tools/TestRunner:domains/entities.md` FixtureArea entry (blk-633dd6387bca) says "Cleared ... (objects,
  loose items, solid cells ...)". It could add buildings on no layer and the World.Build standing-check warning.
- The handoff pointers that applied: code-style, error-handling, entities. The overview blocks
  (blk-3ca89d57dfde, blk-647210c6a78a, blk-6c2bbf052519) and the glossary were not needed. Nothing that governs
  this work was missing.
