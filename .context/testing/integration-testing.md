---
tags: in-game-testing
doc_id: doc-b7d4de4db710
description: "How ONI-Mods tests a mod inside the whole running game: the TestRunner launcher options, input-layer and tool tests, generated worlds, saves, other authors' mods, screenshots and machine requirements"
---

# Integration Testing Conventions

## Scope
**Id:** blk-e1cba092696e  
**Tags:** simulated-input, generated-worlds, save-files, mod-compatibility

Integration tests here verify that a mod works inside the whole running game: its building placed through the real build tool, its Harmony patches against the game's own code paths, a generated world, a loaded save, other authors' mods installed beside it. They are the same `[OniTest]` coroutines as in `unit-testing.md`, run by the same TestRunner; what differs is how much of the game each one drives.

| Layer | What the test drives |
|-------|----------------------|
| `[Layer(0)]` | The game's own API for setup and oracles (`BuildingDef.Build`, `Grid`, the logic network) |
| `[Layer(1)]` | The real `InterfaceTool` fed the same `OnMouseMove` / `OnLeftClickDown` / `OnLeftClickUp` calls the player controller makes |

## Test Environment
**Id:** blk-648237728a61

### Game and fixture
**Id:** blk-a66da6f9aaa2  
**Tags:** mod-loadout, save-files, test-cleanup

- Use: the real game, launched through Steam by `common/tools/TestRunner/Invoke-OniTests.ps1`, with only TestRunner, the mods under test and `-With` mods enabled.
- Setup: the launcher rewrites `mods.json`, loads the fixture save from the main menu, picks a 40x10 test area, and runs the tests. A detached warden (`Watch-OniGame.ps1`) puts the player's `mods.json` back after the game exits.
- Cleanup: the harness clears the test area (objects, loose items, solid cells; Duplicants are the test's to remove) before the first test and after every test, blocks every save to disk outside the run folder, and starts each mod's tests on a fresh load of the fixture.

### Other mods
**Id:** blk-4ed03be05da6  
**Tags:** mod-compatibility, mod-loadout

- Approach: real installs. `-With Mod1,Mod2` enables more local mods; `-Steam id1,id2` enables installed Workshop mods by item id, to test next to another author's mod (for example a mod whose patches touch the same game methods). Some suites test the interaction directly: `mods/bitshifter/src/Bitshifter.Tests/Compatibility.cs` for Better Automation Overlay and Blueprints Expanded, and the `PLib:` tests in `mods/upgrade-planner/src/UpgradePlanner.Tests/DialogDrag.cs`.
- Tools: none beyond the harness.

## Test Structure
**Id:** blk-4a52ff2f7294

### Setup and Teardown
**Id:** blk-3753681f851b  
**Tags:** test-cleanup

There is no `beforeAll`/`afterAll`. Per-test setup is the test body; teardown is `t.Cleanup(action)` (runs whatever happened) and the harness's area clearing. Tests that need the whole area walled and air-filled mark `[OniTest(Boxed = true)]` and call `t.Dupes.Box()`.

```csharp
[OniTest("DAB: wire dragged from a bridge end to its neighbour connects both", Timeout = 30), Layer(1)]
public static IEnumerator DragFromEnd(TestContext t)
{
    CellRect r = t.Rect(6, 1);
    t.World.Build("LogicWireBridge", r.Cell(3, 0));
    yield return t.WaitUntil(() => BridgeEndNode.TryGet(r.Cell(2, 0), out _), 3, "the bridge's end node");
    UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");   // the real utility build tool
    yield return wire.Path(r.Cell(2, 0), r.Cell(1, 0));           // down, move, up, one frame per step
    t.Assert.Eq(Color.white, wire.PreviewTint(r.Cell(1, 0)), "the neighbour previews white");
}
```

(From `common/tools/TestRunner/README.md`; Jumper Bridges was formerly Direct Automation Bridges, hence `DAB`.)

## Input and Tool Testing
**Id:** blk-7ce1bb4fbb33  
**Tags:** simulated-input

`t.Tools.UtilityTool(id, element)` returns a drag session on the real utility build tool: `Path(cells...)` visits each cell with a frame between (a careful drag), `Sweep(from, to)` is one move (a fast swipe), `PreviewTint(cell)` is the preview's colour just before the button went up (white means buildable). Disposed, or at cleanup, the select tool is back and any drag is cancelled. `UtilityDragSession.PrimeBuildMenu(t)` wakes the build menu's product screen once before a test drives the BuildTool directly. `yield return t.Press(Action.Escape)` presses a key as the game hands one to its screens; the physical keyboard is ignored for the session. `t.PointAt(cell)` puts the locked mouse over a cell for hover cards.

## Database Integration
**Id:** blk-28a5ab6b019c  
**Tags:** save-files

Not applicable: the mods keep no database. State under test lives in building components, `KPrefabID` and settings saved with the building, mod options (PLib), and the save file itself (`SaveFile` in the kit; `Persistence.cs` in Detect Any Element copies settings and saves and reloads).

## Generated Worlds
**Id:** blk-87272da04dee  
**Tags:** generated-worlds, test-pacing

`[OniTest(Worldgen = "SNDST-C-100-0-0-0")]` runs the test in a world freshly generated from that coordinate with every mod of the run active, instead of the fixture. The harness goes to the main menu, applies the coordinate, runs the game's own worldgen screen (about 20 s for a Spaced Out cluster), embarks, and presses Begin on the opening message. Consecutive tests with one coordinate share the world; nothing in a generated world is cleared, and `t.Area` is not for such a test. `t.Generated` reports the applied coordinate, the cluster, worldgen errors and seconds. A coordinate of the other mode skips the test; a worldgen thread silent for 300 s ends the session. Raise `-TimeoutSec` for runs with several worlds. Example: `mods/natural-backwalls-everywhere/src/NaturalBackwalls.Tests/Generated.cs`.

For worldgen checks with nobody at the keyboard and no test DLL, `common/tools/WorldgenDriver/Invoke-Worldgen.ps1` generates worlds in the real game (`common/tools/WorldgenDriver/README.md`).

## Reproducing a Report in a Save
**Id:** blk-eaae04fb77a1  
**Tags:** save-files, mod-loadout, github-issues

`common/tools/SaveRunner` loads a player's save with nobody at the keyboard, blocks every save to disk, records a probe once per game second and quits:

```
pwsh -File common/tools/SaveRunner/Invoke-SaveRun.ps1 -Save <file.sav> -Seconds 90 -Probe fridges [-With ...] [-Without ...] -Label <name>
```

Run it with the mod under test and again with `-Without <Mod>` and compare the two `probe.csv` files. Mod Profile Manager restarts the game on a changed loadout and has to be left out (`-Without 2854869130,3281716506`). A test can also load a player's save as its fixture: `Invoke-OniTests.ps1 -Fixture <file.sav>` (copied into the run folder, never written back), with `[OniTest(FreshSave = true)]` for tests that change the colony outside the test area.

## Isolated Manual Testing
**Id:** blk-90a1fe9c8d49  
**Tags:** mod-loadout

`common/tools/ModTest/Test-OniMods.ps1` launches the game with only the mods under development enabled and restores the normal loadout afterwards (`-Only`, `-Omit`, `-NoLocal`, `-Steam`, `-DryRun`). It is for a person to play in, not an automated run.

## Test Data Management
**Id:** blk-05c6c405aa0d

### Seeding
**Id:** blk-d4ad4f83c5a5

`t.Dupes.Spawn(n, cell)` (Duplicants with a base trait, attributes at 20, work scheduled every hour, needs topped up), `t.Dupes.Grant(skillIds...)` / `GrantAll()`, `t.Dupes.Drop(material, kg, cell)`, `t.ResearchAll()`.

### Cleanup Strategies
**Id:** blk-976d76b5a752  
**Tags:** test-cleanup, save-files

| Strategy | When to Use |
|----------|-------------|
| Harness clears the area after each test | Default |
| `Boxed = true` (only the inside is cleared between boxed tests) | Tests that work in a walled, air-filled box |
| `FreshSave = true` | Tests that change the colony outside the test area |
| `t.Cleanup(action)` | Restore anything a test changed (a setting, a game speed) |

## Running Integration Tests
**Id:** blk-288e6ebcd312  
**Tags:** screenshots

```bash
# Every test of one mod, any layer
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls

# Explicit tests (long, or photographs) only run when a filter selects them
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods NaturalBackwalls -Filter "*Overview*" -TimeoutSec 900

# Keep the colony open afterwards; the launcher waits until the game is closed
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -KeepGame

# Log the grid marks the game makes during tests
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Trace
```

## Configuration
**Id:** blk-dbe0d7b6cc98

### Launcher Options
**Id:** blk-1b45a760667b  
**Tags:** save-files, mod-build

| Option | Default | Meaning |
|--------|---------|---------|
| `-TimeoutSec` | 600 | Real seconds the whole session may take |
| `-TestTimeoutSec` | 60 | Real seconds per test unless the attribute says otherwise |
| `-Fixture` | `fixtures/testbed-so.sav` in Spaced Out mode, `fixtures/testbed-base.sav` otherwise | Save loaded for fixture tests |
| `-Label` | `run` | Prefix of the run folder `runs/<label>-<stamp>` |
| `-NoBuild`, `-NoCheck`, `-BuildArgs` | - | Skip the build or ModCheck pass, or pass arguments to `dotnet build` |

### Machine requirements
**Id:** blk-b55434e0dd4a  
**Tags:** machine-setup, save-files

Steam running; the game not running; the game folder found in a Steam location or named by `ONI_GAME_FOLDER`; `testbed-so.sav` is tracked in `common/tools/TestRunner/fixtures/`; a base-game run needs a `testbed-base.sav` copied in beside it.

### Screenshots
**Id:** blk-bac04f55a861  
**Tags:** screenshots, generated-worlds, steam-workshop

`yield return t.Screenshot(name)` writes `artifacts/<Class>.<Method>-<name>.png` into the run folder and lists it in the result. `t.LookAt(cell, size)` points the camera first (orthographic size 14 shows the fixture area, 8 a close-up). In a generated world, lift the fog first with `Grid.Reveal`. A screenshot is something to look at, never an oracle; previews for Workshop pages are made from such runs (`tools/make_preview.py` in Jumper Bridges).

## Best Practices
**Id:** blk-46b28a0040e8  
**Tags:** test-pacing, player-log, game-update-compatibility, test-cleanup

1. **Isolate tests** - each test builds what it needs in its own area; nothing carries over.
2. **Drive the real thing where the bugs live** - a drag bug needs `[Layer(1)]` and the real tool, not a direct call.
3. **Pace to game time** - wait on conditions and game seconds between steps (see `unit-testing.md`).
4. **Test error paths** - a game update that renames a private member, a mod loaded beside another author's, a save made without the mod.
5. **Keep tests fast** - Explicit and long Worldgen tests stay out of a normal run.
6. **Check the log** - a repeating exception in `Player.log` during a long run is a failure even when every test passed.

## Related
**Id:** blk-ed0ecc983d7f

- See also: [unit testing](unit-testing.md)
- Governed by: [CI/CD workflow](../workflows/ci-cd.md)

## Checking what removing a mod leaves in a save
**Id:** blk-9a58d8528c61

A mod that adds its own prefabs leaves a player's save in a state only the game without the mod can show. Check it in game before writing a removal claim in a README or Workshop description:

1. An `[OniTest(Explicit = true, FreshSave = true)]` test in the mod's suite builds what a player would leave behind (each state of each new object), writes the cells to a text file and saves with `SaveLoader.Instance.Save` under `Session.Dir` (`mods/ovagro-overhead/src/OvagroOverhead.Tests/Removal.cs`).
2. A probe test in another mod's suite, marked `[OniTest(KeepSave = true, Explicit = true)]` so the harness does not clear part of the save before it, loads that save with `-Fixture` and reports what each listed cell holds, loose items nearby and the plots' orders, at load and after some game seconds. The other mod's loadout keeps the mod under test off. Delete the probe afterwards.
3. Read `Player.log` for `Could not find prefab` warnings and for exceptions: a vanilla object whose mod-made partner was dropped can throw every tick.

Ovagro Overhead's check (OM-006) found its hanging plant dropped on load while the vanilla vines it had grown stayed behind with no plant and threw a NullReferenceException every tick; the vines became the mod's own prefab so both drop together.

## Plant scenes
**Id:** blk-c1a9de995bf0

Facts that each cost a failed run while building plant scenes for Ovagro Overhead:

- A Hydroponic Farm with no pipe reports `NeedLiquidIn` and its plant wilts, even with water put straight into its storage; build a `LiquidConduit` in the farm's cell.
- The test map is dug out and all vacuum. A scene that needs air is walled with tiles and filled with `SimMessages.ReplaceElement`; a plant that does not check pressure (Ovagro and its vines) grows in a thin fill, which keeps gas out of screenshots.
- Some plants have no maturity amount: the Ovagro Node grows by playing its grow animation once and then flags itself grown, so a test waits on its state machine, not on maturity. Its vines do have maturity, and forcing it on each new vine grows a chain in seconds.
- Pips walk floors, walls and ceilings, but cannot cross a gap from a wall or ceiling to a floating ledge; spawn a Pip on the surface it has to reach, and assert `Navigator.CanReach` before waiting on it.
- When a wait stalls, log the object's state every few seconds and take a screenshot with the HUD on (status icons) before failing; the cause is usually visible there.
