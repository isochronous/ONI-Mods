# Upgrade Planner tests

In-game tests, run with the shared test runner (`common/tools/TestRunner`, whose README has the
options). The tests about settings need Blueprints Expanded, and the planner's tests are run with
the Workshop mods players use it with:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624
```

| Workshop id | Mod | Why |
|---|---|---|
| 3468585385 | Blueprints Expanded | The planner's dependency: it carries buildings' settings. Without it the settings tests skip. |
| 3724635599 | Adaptive Building Menu | Changes the material pickers the window borrows. |
| 1832319118 | Suppress Notifications | Its suppressions are kept on upgrade (`OtherModSettings.cs`). |
| 2047308624 | No Manual Delivery | Its "hold" is kept on upgrade (`OtherModSettings.cs`). |

## Settings: smoke test and full run

`BuildingSettings.ProblemTypesKept` is the smoke test (about a minute and a half): the types that
have had a problem, a sensor and a gate, and the buildings with Suppress Notifications' and No
Manual Delivery's settings, built, their settings changed where a player can change them, then
changed to another material with the real tool and instant build. It fails when a loaded mod's
setting was never checked, so a registration that stops working shows.

`BuildingSettings.SettingsKept` does the same for every type All buildings offers that stands on a
floor, a wall or a backwall (or anywhere): about 320 types in 15 bays, six minutes or so. It is
`Explicit`, so it runs only when asked:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624 -Filter "*SettingsKept*" -TimeoutSec 2400 -TestTimeoutSec 2100
```

`BuildingSettings.Survey` (also `Explicit`) lists the settings Blueprints Expanded stores for each
type and what each building offers the player (menu buttons, side screens); use it when a game or
mod update adds settings. A setting kind the test has no way to change fails `SettingsKept` with
"the test has no way to change"; add it to `SettingKinds.cs`, with its control in `Control` if the
game shows it only on some buildings.

Add a type to `ProblemTypes` whenever the full run finds something new.

## Work done by Duplicants

`ByHand` researches everything, turns instant build off and has Duplicants carry out the planner's
orders: the fixture area boxed in with air and life support (`t.Dupes`, one box for the whole
class), a crew of three spawned inside with every attribute at 20, the materials dropped on the
floor, the game at speed 3.

| Test | What the crew does |
|---|---|
| `Tiles` | Sandstone tiles to granite; the orders wait until there is granite to fetch. |
| `Pipes` | Gas pipes to insulated pipes, connections kept. |
| `Wires` | Copper wires to gold, connections kept. |
| `Rebuilt` | A sensor, a gate, a floor machine and the wide farm tile taken down and built again through the game's reconstruct, settings kept. |
| `ReplacedWithSettings` | A storage tile and a carpet built over in another material: the filter kept, the carpet's reed fiber fetched. |
| `Doors` | The four doors, across the corridor the crew walks along, rebuilt with their settings (access left open). |
| `TwoIngredients` | A germ sensor's plastic changed to plastium with its copper kept, and another's copper changed to steel with its plastium kept. |
| `CancelledOrders` | Replacement orders cancelled with the Cancel tool are never built, even with granite to fetch. |
| `CancelledRebuild` | A rebuild cancelled with the Cancel tool before the crew comes leaves the bin as it was. |

`Rebuilt`, `ReplacedWithSettings`, `Doors` and `TwoIngredients` need Blueprints Expanded. About three
minutes for the nine:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624 -Filter "*ByHand*"
```

`ByHandAll` holds the same by hand over everything the planner offers, for after a game or mod
update. Its tests are `Explicit`, with a crew of three and a pile of materials at each end of the
box, every crew with every skill (conductive and Heavi-Watt wire and conveyor rails ask one); they
collect every problem and report them together, logging each as it is found:

- `RepresentativeTypes` (about a minute) and `RepresentativeReplacements` (about a minute): the
  same as the two below over the fewest types (30) and pairings (17) that between them show every
  trait of all of them. A type's traits are what the planner and the game branch on: rebuilt or
  built over, layer, placement rule, size, ingredients and item ingredients, rotations, a skill
  to build it, pipe, power and automation ports, door, storage, and each kind of setting it holds;
  a pairing's are both types' traits, whether it is built over itself or another type on its
  layer, the layers it goes between, and whether it is bigger or smaller. The run logs what was
  taken and why. A type the game adds is taken when it brings a trait no other shows. This is the
  run for after an update; the full runs are for when it finds something.
- `EveryType` (about 12 minutes): every type All buildings offers, 307 of them in 31 batches along
  the floor, its settings changed, built again in its second material by the crews, about 1040
  settings checked. Dev-only types are left out: they are built only at once, never by hand.
- `EveryReplacement` (about 20 minutes): every pairing the Replace type tab offers (each type and
  every type that can be built over it, its own included, about 440), three pieces each (joint
  plates in a wall, where the game allows them), built over by the crews. Left out: a type built
  over itself when it comes in one material, and snow, which melts in the box's air.
- `SettingsUnderLoad`: four rounds of six types rebuilt at speed 3, all keeping their settings; it
  caught the race in Blueprints Expanded's reconstruct (see the changelog).
- `SuspectTypesRebuilt` and `SuspectReplacements`: the types and pairings a full run last had a
  problem with, on their own, for a closer look; edit their lists.

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624 -Filter "*ByHandAll.Representative*" -TimeoutSec 900 -TestTimeoutSec 600
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624 -Filter "*ByHandAll.Every*" -TimeoutSec 3600 -TestTimeoutSec 3000
```

`ToolParity` (in the default run) has the game's own build tool and the planner each order joint
plates over the same tiles, in a row and in a wall, and compares where they put them.

## Tests on a player's save

`DoorCrusher` runs on the save the door crusher report came from, the only one with the door
crusher. Its tests are `Explicit`, so they run only when the filter names them:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599 -Fixture "<Documents>\Klei\OxygenNotIncluded\save_files\Mod Test\ActuallyTestMapNow.sav" -Filter "*DoorCrusher*"
```
