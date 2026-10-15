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

The last two need Blueprints Expanded. About two minutes for the five:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599,1832319118,2047308624 -Filter "*ByHand*"
```

## Tests on a player's save

`DoorCrusher` runs on the save the door crusher report came from, the only one with the door
crusher. Its tests are `Explicit`, so they run only when the filter names them:

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods UpgradePlanner -Steam 3468585385,3724635599 -Fixture "<Documents>\Klei\OxygenNotIncluded\save_files\Mod Test\ActuallyTestMapNow.sav" -Filter "*DoorCrusher*"
```
