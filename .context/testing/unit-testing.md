---
tags: in-game-testing
doc_id: doc-797b7d024719
description: "How ONI-Mods writes [OniTest] coroutines run in the real game by TestRunner: tool list, test project layout, TestKit assertions and waits, per-test options, run commands, coverage rules and anti-patterns"
---

# Unit Testing Conventions

## Framework and Tools
**Id:** blk-5e77d0d9ce90  
**Tags:** mod-build, game-update-compatibility, translations

There is no desktop unit-test project in this repository: no xUnit, NUnit or MSTest. The mods run inside Unity's Mono in the game and call game code that exists only there, so every test runs in the real game.

| Tool | Purpose |
|------|---------|
| `common/tools/TestRunner` | Dev-only mod and launcher (`Invoke-OniTests.ps1`) that load a fixture save, run every `[OniTest]` coroutine in `tests/*.dll`, and write results |
| `Isochronous.TestKit` (`common/tools/TestRunner/Kit`) | `TestContext` (`t`), `t.Assert`, `t.World`, `t.Dupes`, `t.Tools`, `t.Frames`, `t.WaitGameSeconds`, `t.WaitUntil` |
| `common/tools/ModCheck` | Loads a built DLL against the game assemblies on .NET 8 and JIT-compiles every method, catching type-load failures and missing members after a game update |
| `common/tools/Translations/translations.py` | Checks `.po` files against the strings the built DLL contains |

The full reference for the kit is `common/tools/TestRunner/README.md`.

## Test File Location
**Id:** blk-41ffba520973  
**Tags:** mod-build

Each mod has a test project beside it, referencing the mod and the TestRunner project:

| Source | Test |
|--------|------|
| `mods/detect-any-element/src/DetectAnyElement/` | `mods/detect-any-element/src/DetectAnyElement.Tests/` |
| `mods/jumper-bridges/src/JumperBridges/` | `mods/jumper-bridges/src/JumperBridges.Tests/` |
| `mods/natural-backwalls-everywhere/src/NaturalBackwalls/` | `mods/natural-backwalls-everywhere/src/NaturalBackwalls.Tests/` |

All 17 mods have a `<Mod>.Tests` project, grouped by what is tested (for Detect Any Element: `Glyph.cs`, `Persistence.cs`, `PipeSensors.cs`, `SideScreen.cs`, `WorldSensors.cs`). A test project's csproj imports `$(OniCommon)oni-mod.props`, sets `ModDeployFolder` to `none` (never deployed as a mod), and references `$(OniCommon)tools/TestRunner/TestRunner.csproj` with `Private="false"`; copy `mods/detect-any-element/src/DetectAnyElement.Tests/DetectAnyElement.Tests.csproj`.

## Test Structure
**Id:** blk-9a1088df279a

### File Organization
**Id:** blk-1a5825c096e0  
**Tags:** test-assertions

A test class is `public static`; a test is a `public static IEnumerator` taking a `TestContext`, marked `[OniTest("<name>")]` with a `[Layer(n)]`. Tests run in declaration order. From `mods/detect-any-element/src/DetectAnyElement.Tests/Persistence.cs`:

```csharp
public static class Persistence
{
    [OniTest("DAE: copying settings from a sensor on Any makes the target Any, and from an element makes it that element", Timeout = 40), Layer(0)]
    public static IEnumerator CopySettings(TestContext t)
    {
        CellRect r = t.Rect(6, 1);                                      // clear cells; r.Cell(x, y) from the bottom-left
        GameObject source = t.World.Build("LogicElementSensorGas", r.Cell(1, 0));
        GameObject target = t.World.Build("LogicElementSensorGas", r.Cell(3, 0));
        yield return t.WaitGameSeconds(1);
        // ... act ...
        t.Assert.True(AnyElement.Is(target.GetComponent<Filterable>().SelectedTag), "the target took Any");
    }
}
```

Arrange, act, assert, with a `yield return` of game time between steps.

### Naming Conventions
**Id:** blk-7316ec9fd58f  
**Tags:** test-assertions, screenshots

- The display name starts with the mod's short prefix and a colon (`DAE:`, `NB:`, `VFF:`, `JFA:`, `MSR:`, `MCS:`, `NMF:`, `SWP:`, `DCEOR:`, `Bitshifter:`, `SaltySurf:`, `PLib:`), then a sentence saying the behaviour: "wire dragged from a bridge end to its neighbour connects both". Photograph-only tests use `looks:`, and a control case that runs beside the behaviour it checks uses `control:` (`mods/jumper-bridges/src/JumperBridges.Tests/WireDrags.cs`).
- Class names group by subject (`Persistence`, `SideScreen`, `Glyph`), method names say the case (`CopySettings`, `DragFromEnd`).
- The assertion message names what was expected ("the target took Any"), not the call.

```csharp
// Good
[OniTest("VFF: a tile out of a small packet in the landing cell under the vent is dug up and left as debris at full mass", Timeout = 40), Layer(0)]
// Avoid
[OniTest("test 1")]
```

## Mocking Patterns
**Id:** blk-0b401ff80cf2

There is no mocking library. Tests use the real game objects: `t.World.Build(id, cell)` builds an actual building (`BuildingDef.Build`, instant), `t.Dupes.Spawn(n, cell)` makes real Duplicants, `t.Dupes.Drop(material, kg, cell)` leaves a loose pile. Setup helpers shared by one mod's tests live in a `Kit.cs` beside them (`mods/detect-any-element/src/DetectAnyElement.Tests/Kit.cs`).

### Settings and environment
**Id:** blk-0272c27e23d2  
**Tags:** test-cleanup, player-log

- `t.InstantBuild(on)` turns instant build off or on for the test, restored at cleanup.
- `t.ResearchAll()` researches every tech so menus offer everything.
- `t.Speed(n)` sets game speed 1 to 3 for the test.
- `t.Cleanup(action)` runs after the test whatever happened; `t.Skip(reason)` skips.
- `t.AllowLogError(regex)` accepts an error the game is expected to log; any other `Error`, `Exception` or `Assert` logged during a test fails it.

## Assertion Patterns
**Id:** blk-0194e02d0fd6  
**Tags:** test-assertions

`t.Assert` offers `True`, `False`, `Eq`, `NotEq`, `Null`, `NotNull`, `Has(flag, bits)`, `Lacks` and `Fail`, each taking a message.

### Waiting
**Id:** blk-33fb80177cba  
**Tags:** test-pacing, player-log

Yield `t.Frames(n)`, `t.WaitGameSeconds(s)` (scaled sim time), `t.WaitRealSeconds(s)`, or `t.WaitUntil(cond, timeout, what)`; a timeout fails the test with `what`. Wait for a condition instead of counting frames.

**Pace tests to the game.** The game is built for a player's speed. Build, change, rebuild and clear frames apart, and its sim callbacks arrive after the objects they belong to are gone (once an endless exception every tick, the game at 2 FPS). Wait game time (a second or two) after building or clearing, give a drag a moment between its steps, and watch `Player.log` for a repeating exception during long runs.

## Test Data
**Id:** blk-b9b075d91c5b

### Fixtures
**Id:** blk-66e912fbb652  
**Tags:** save-files, test-cleanup

The fixture is a save: `common/tools/TestRunner/fixtures/testbed-so.sav` in Spaced Out mode, `testbed-base.sav` otherwise, chosen by the game's current mode; `-Fixture <path.sav>` overrides it. `testbed-so.sav` is tracked in `common`; `testbed-base.sav` is not there, so a base-game run needs a save copied in under that name. The harness picks a 40x10 area of the world, clears it before the first test and after every test, and `t.Rect(w, h)` hands a test its cells. See `common/tools/TestRunner/fixtures/README.md`.

### Per-test options on `[OniTest]`
**Id:** blk-c8a18ea4088d  
**Tags:** generated-worlds, save-files

| Option | Meaning |
|--------|---------|
| `Timeout = n` | Real seconds allowed (default `-TestTimeoutSec`, 60) |
| `Explicit = true` | Skipped unless a `-Filter` selects it |
| `NoColony = true` | Needs no colony (strings, options dialog, registrations); runs at the main menu |
| `FreshSave = true` | Reload the fixture before the test |
| `Boxed = true` | Works in a walled, air-filled box (`t.Dupes.Box()`) |
| `Worldgen = "<coordinate>"` | Runs in a freshly generated world; see `integration-testing.md` |

A `NoColony` test is the closest thing here to a plain unit test: it exercises strings, registrations or option dialogs with no save loaded (`mods/natural-backwalls-everywhere/src/NaturalBackwalls.Tests/Looks.cs`).

## Running Tests
**Id:** blk-960faf85db39  
**Tags:** mod-build, mod-compatibility

```bash
# One mod (the project folder name under src/, not the mod folder)
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges

# Several mods in one session
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DetectAnyElement,JumperBridges

# Tests whose id or name matches a glob (also the only way to run an Explicit test)
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Filter "*DragFromEnd*"

# Build and ModCheck only, no game session
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -BuildOnly

# Alongside another author's Workshop mod, by item id
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Steam <id>
```

Steam must be running and the game must not be (other than the harness's own live session). Exit code 0: all passed; 1: a test failed, errored or timed out; 2: no status (launch, load or crash: read `Player.log` in the run folder); 3: build or ModCheck failure. The game stays up 15 seconds after a run so the next run reuses it (about 10 s instead of 40); one run at a time is allowed, others queue. Output is under `common/tools/TestRunner/runs/<label>-<stamp>/`.

Pass `-Steam <id>` when a bug appears only with a player's other mods installed: the harness enables those Workshop items next to the mod under test.

## Coverage Requirements
**Id:** blk-d3c50bc15df0  

No coverage tool is run and no percentage is required. TODO(owner): state a target if one is wanted. In practice a test is added with the behaviour or fix it covers (commits such as "Bitshifter: test for the NOT gate constant mask", "Salty Surf: in-game tests").

## Anti-Patterns to Avoid
**Id:** blk-6e0e91ddc944

### Frames-apart build, change and destroy
**Id:** blk-db81d810e051  
**Tags:** test-pacing

```csharp
// Bad: objects destroyed a frame after they were built; sim callbacks arrive late
t.World.Build("LogicWire", cell);
yield return null;
t.World.Destroy(go);

// Good: let the game settle, and wait on a condition
// (mods/dont-count-eggs-on-rails/src/DontCountEggsOnRails.Tests/Eggs.cs)
yield return t.WaitUntil(() => sensor.currentCount == 1, 15, "the sensor to count the egg on the floor");
```

### Asserting on a private field instead of the behaviour
**Id:** blk-4224fccba710  
**Tags:** test-assertions, game-update-compatibility

Assert what a player or the game's own API can see (the preview tint, the logic network, the symbol override), not an internal field. Private game members the harness reads are listed in `common/tools/TestRunner/Reflect.cs` with a self-check that names the member when a game update renames it; add to that list rather than reflecting ad hoc in a test.

### Meaningless assertions
**Id:** blk-f054250ed484  
**Tags:** test-assertions

```csharp
// Bad
t.Assert.True(go != null, "it worked");
// Good
t.Assert.Eq(Color.white, wire.PreviewTint(r.Cell(1, 0)), "the neighbour previews white");
```

### Letting an expected game error fail unrelated tests
**Id:** blk-fb116707eda5  
**Tags:** test-assertions, player-log

Declare it with `t.AllowLogError(regex)` in the one test that provokes it.

## Related
**Id:** blk-161afa8b2781

- See also: [integration testing](integration-testing.md)
- Harness reference: `common/tools/TestRunner/README.md`
