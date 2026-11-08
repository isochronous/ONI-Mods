# Unit 07 report — Smooth Borders on U60

## Finding: the patch was never off on U60

The "DrawWorldBorder has changed; borders stay as the game draws them" warning in unit 06's Player.log does not come from the load-time patch. Both lines come from the `FailSafes.ChangedMethodLeftAlone` test. That test runs the transpiler on two altered copies of `DrawWorldBorder` (the thickness read doubled, then removed) to prove that the guard turns the mod off. Each copy logs the warning once, so the test produces exactly two. In unit 06's snapshot (`scratchpad/logs/Player-all-snapshot.log`, lines 1702-1706), both warnings come 2 ms after the test's own "629 instructions; changed: 14 ... 19 ..." line, inside the test, and the test then passes. No other log from unit 06 carries the line. The same log shows the real patch active on U60: "the mod's DrawWorldBorder patches are back on (transpiler matched: True)".

## What changed in vanilla

Nothing that affects the patch. On U60 the transpiler finds both `GetIntSetting` reads (instructions 14 and 19 of 629) and swaps them for `Border.Setting`. Unit 01's PatchCheck showed the same thing on the U60 IL, and ILDiff did not list `DrawWorldBorder` among the real changes. I did not run a separate U59/U60 diff, because the in-game run below shows the patch working on 762230.

## BOTH-BUILDS

No mod code changed, and `minimumSupportedBuild` stays at 744825.

## Files changed

- `mods/smooth-borders/src/SmoothBorders.Tests/FailSafes.cs` (+2 lines, CRLF kept): before the two altered copies, the test logs "next: two altered copies of the method; each draws the mod's "DrawWorldBorder has changed" warning, expected here and not a sign of a game change". This is so the two warnings are not read as a game change again.

## Test gap: the suite already fails when the patch is inactive

No new test was needed. These existing tests fail if the patch does not apply:
- `FailSafes.ChangedMethodLeftAlone` asserts `Border.Patched` ("the transpiler matched when the game patched DrawWorldBorder at load"). It is false whenever the load-time transpiler falls back.
- `Identity.*Flat` asserts `FlatWalls().Count > 0` (the mod's "flat wall N tiles" log lines). `Flatness.*` measures every wall and asserts it is flat at the configured thickness. `Flatness.RectanglesGrowAtMaxima` asserts that the POI rectangles grew.
- `Identity.*Vanilla` unpatches the mod and asserts the opposite: no flat wall, and the game's wall is uneven. This shows the measurement tells the two states apart, which is the "fails with the patch disabled" evidence.

I did not run against a patch-disabled build. The assertions above read `Border.Patched` and the generated world directly, so with the patch off at least `ChangedMethodLeftAlone`, the two `*Flat` tests and the three `Flatness` tests would fail.

## Build

```
dotnet build SmoothBorders.Tests/SmoothBorders.Tests.csproj -c Release -nologo -p:ModDeployFolder=none
    3 Warning(s)   (pre-existing CS0618 FindObjectOfType in TestRunner)
    0 Error(s)
```

## In-game run (U60-762230-SCRPAN, `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods SmoothBorders`)

```
Loadout: TestRunner + SmoothBorders; mode Spaced Out; fixture ...\fixtures\testbed-so.sav
PASS     AsteroidKinds.EveryWorldInTheTable / DefaultsAreTheGamesThinnest / PairsOutsideTheTable / ConfigLimitedOnRead
PASS     Dialog.LabelsAndTooltips / IntroTopLeft / SlidersAndDefaults / SavesAndResets
PASS     FailSafes.ChangedMethodLeftAlone
PASS     Flatness.AtMaxima / RectanglesGrowAtMaxima / AtMixedValues
PASS     Identity.TerraniaVanilla / TerraniaFlat / TerraniaSame / ClassicTerraVanilla / ClassicTerraFlat / ClassicTerraSame
SKIPPED  Identity.BaseTerraVanilla / BaseTerraFlat / BaseTerraSame: base game cluster, game in Spaced Out mode
Session done: 18 passed, 0 failed, 0 errors, 0 timeouts, 3 skipped of 21; 176.1 s real time
```

Full output: `scratchpad/logs/u07-SB.txt`. The Player.log copy is `scratchpad/logs/Player-u07.log`. Relevant lines:

```
06:06:36.914 FailSafes.ChangedMethodLeftAlone: 629 instructions; changed: 14 callvirt ...GetIntSetting -> call SmoothBorders.Border::Setting, 19 ...
06:06:36.914 FailSafes.ChangedMethodLeftAlone: next: two altered copies of the method; each draws the mod's "DrawWorldBorder has changed" warning, expected here ...
06:06:36.916 [WARNING] [SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them
06:06:36.917 [WARNING] [SmoothBorders] DrawWorldBorder has changed; borders stay as the game draws them
06:06:39.207 [SmoothBorders] expansion1::worlds/TerraMoonlet: group Primary, flat wall 16 tiles
... (48 "flat wall" lines in all)
06:07:54.343 [SmoothBorders.Tests] the mod's DrawWorldBorder patches are back on (transpiler matched: True)
```

Acceptance note: Player.log still carries the warning, twice, but only as the fail-safe test's expected output, which is now labelled. It does not appear at load or during worldgen. Removing it completely would mean silencing the mod's real warning while that test runs, which would hide what the test is proving. I left it as it is.

## Why unit 01's check "missed" it

It did not miss anything: PatchCheck ran the transpiler on the real U60 IL and correctly reported a match. The gap was in unit 06's log triage, which grepped Player.log for mod warnings from a full test session and took this one as a load-time event. It did not check which test was running at that timestamp, and it did not see the "transpiler matched: True" line. To fix the method: when a session log is triaged, attribute each warning to the `running <test>` line before it, and treat warnings inside a fail-safe or negative-path test as expected unless that test fails. Tests that provoke a mod's warnings on purpose should say so in the log just before, as FailSafes now does.

## Save safety

TestRunner was not changed. The run used its own fixture under `runs/`.

## Friction (for the coordinator to record)

None with ICON. The handoff carried no pointers ("Nothing matched this scope"), so `ks read` was not needed.
