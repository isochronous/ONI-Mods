# Unit 06 report: the lost TestRunner kit methods, rewritten

Every call in the Bitshifter tests compiles as written. No test file changed. Nothing pushed, the game not launched, no in-game test run.

## Commits

| Repo | Branch | Hash | Message |
|---|---|---|---|
| common | OM-008-ribbon-panel (from 357073f) | a4d2ef8 | TestRunner: Anims symbol tint and visibility, Screens select/side screen/find/click, World.StatusItems |
| ONI-Mods worktree | OM-008-ribbon-monitor | a74fb95 | OM-008: common with the rewritten panel kit (pointer only) |

Task scratch files (handoff-6.md, units/06-implement.md, checkpoint-6-1.md, this report) are left uncommitted for the coordinator.

## Files (all under `common/tools/TestRunner`)

- `Kit/Anims.cs` (new)
- `Kit/Screens.cs` (new)
- `Kit/World.cs` (StatusItems)
- `Kit/TestContext.cs` (`Screens` property, class doc)
- `Reflect.cs` (two fields, two SelfCheck lines)
- `README.md` (intro names layer 2; entries for StatusItems, Anims, t.Screens)

## Methods

`Anims` (`public static class`, `Isochronous.TestKit`). Tests call it statically, as `Anims.SymbolVisible(...)`, not as `t.Anims`.
- `public static Color SymbolTint(KBatchedAnimController kbac, KAnimHashedString symbol)`: looks the symbol up the way `KBatchedAnimController.SetSymbolTint` does (`KAnimBatchManager.Instance().GetBatchGroupData(kbac.GetBatchGroupID()).GetSymbol(symbol)`). It then reads `symbolInstances[symbol.symbolIndexInSourceBuild].color` from `kbac.symbolInstanceGpuData`'s private `symbolInstancesConverter` (`Reflect.SymbolInstanceGpuDataConverter`). The game has a setter for the tint but no getter.
- `public static bool SymbolVisible(KBatchedAnimController kbac, KAnimHashedString symbol)`: `kbac.GetSymbolVisiblity(symbol)`.
- If the controller is null, or its build has no such symbol, both methods throw `OniAssertException` naming the symbol. Without that check, `GetSymbolVisiblity` returns true for a symbol that does not exist.

`Screens` (`public sealed class`, `TestContext.Screens`, layer 2):
- `public IEnumerator Select(GameObject go)`: calls `SelectTool.Instance.Select(go's KSelectable)`, the same call a player's click on the building makes. It then waits up to 10 s real time until `DetailsScreen.Instance.target == go` and the details screen is active, then waits 3 frames. If the object or its KSelectable is null, the test fails. The first Select in a test registers a cleanup that clears the selection.
- `public void Deselect()`: `SelectTool.Instance.Select(null)`.
- `public T SideScreen<T>() where T : SideScreenContent`: the `screenInstance` of type T in `DetailsScreen.sideScreens` (`Reflect.DetailsScreenSideScreens`) that is active in the hierarchy, or null. This matches the helper in `Bitshifter.Tests/SideScreen.cs`.
- `public GameObject Find(GameObject root, string name)`: breadth-first search, so the nearest match wins. It includes inactive objects and the root itself, and returns null when nothing matches. A null root fails the test, so a nested `Find(Find(screen, "Hint"), "Text")` reports the miss instead of throwing a NullReferenceException.
- `public void Click(GameObject go)`: sends pointer down, pointer up and pointer click through `ExecuteEvents` with one `PointerEventData` (left button, `pointerPress` set). KButton, MultiToggle and KToggle each react as they do to a real click. A button that is not interactable ignores it, which is what GridDisabled relies on. A null or inactive widget fails the test.

`World.StatusItems`:
- `public List<string> StatusItems(GameObject go)`: `entry.GetName()` for each entry of `KSelectable.GetStatusItemGroup()`, in the group's order, with each item resolved against its data the way the hover card does. It returns an empty list when there is no group. A missing KSelectable fails the test.

`Reflect`:
- `SymbolInstanceGpuDataConverter`, `DetailsScreenSideScreens`, both added to `SelfCheck`.

## Differences from the lost version (per 04-implement-report)

- Additions:
  - the missing-symbol check also covers `SymbolVisible`;
  - Select clears the selection at cleanup;
  - Click fails on an inactive widget;
  - Find fails on a null root.
- Signatures and return types match the call sites exactly.

## Build

Every build used `-p:ModDeployFolder=none`, so nothing was copied into the game's local mods folder (the main checkout's deployed TestRunner and Bitshifter are untouched).

- `dotnet build common/tools/TestRunner/TestRunner.csproj -c Release`: Build succeeded, 3 warnings (the existing CS0618 FindObjectOfType ones in Worldgen.cs and TestBehaviour.cs), 0 errors.
  - The first try failed with `CS0117 'EventSystem' does not contain a definition for 'current'`: the game's global `EventSystem` type shadows Unity's. Fixed by writing `UnityEngine.EventSystems.EventSystem.current` in full.
- `dotnet build mods/bitshifter/src/Bitshifter.Tests -c Release`, which builds `src/Bitshifter` too: Build succeeded, 0 warnings, 0 errors.
- All 18 mod test projects in ONI-Mods.sln built with 0 errors: jumper-bridges, upgrade-planner, corner-build-anything, dont-count-eggs-on-rails, no-magic-fridges, just-fall-already, bitshifter, motion-sensor-range, multichannel-critter-sensor, smart-weight-plate, refined-building, storage-pod-redux, sweep-zones, detect-any-element, salty-surf, vent-freeze-fix, natural-backwalls-everywhere, ovagros-overhead.

## The common move 7687b3c to 357073f

Nothing broke. The TestRunner commits in that range cover:
- Prepare hook
- -Mode
- Workshop clusters
- NoColony-first
- warden/prefs fixes
- fixture clear of no-layer buildings and build previews

Every test project compiles against the new main. Main already had `t.Press`, `t.Screenshot`, `t.PointAt` and `t.LookAt`, so I did not duplicate them. Main had no `Screens`, `Find`, `Anims` or `StatusItems`.

## Not honoured as written

Nothing. Every call in DiagnosticPanel.cs, DiagnosticPanelLook.cs, Rig.cs and Compatibility.cs compiles unchanged.

## Not verified

None of this has run in game. Reflect's two new fields are looked up by name at runtime (`AccessTools.Field`). Both exist in the decompiled game assemblies (ilspycmd on the installed Assembly-CSharp/-firstpass), and `SelfCheck` confirms them at the start of the first run.
