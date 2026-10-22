# Unit 08 report: Salty Surf tag in the Properties line

Model tier: default. Nothing staged or committed; game not launched; tests not run (build only).

## Verified against the decompile
`Element.FullDescription` (Assembly-CSharp, ilspycmd): the loop skips tags in `GameTags.HiddenElementTags`, but writes `", "` after every non-hidden tag whose index is not the last. A hidden tag last would leave `"..., "`. A hidden tag first adds nothing. Matches unit 04.

## Files
- `mods/salty-surf/src/SaltySurf/Mod.cs`: `ElementLoader_Load_Patch.Postfix` calls `GameTags.HiddenElementTags.Add(SurfWater.Tag)` and builds the new `oreTags` with the tag first (copy of the old array after it), instead of `AddToArray`. Still guarded by `!HasTag`.
- `mods/salty-surf/src/SaltySurf.Tests/Intake.cs`: new test `PropertiesLine` ("SaltySurf: the Properties line of Water and Salt Water has no missing string and no trailing separator", Layer 0). For Water and Salt Water it asserts: tag still present; `FullDescription(false)` has no `MISSING` and no `SALTYSURFWATER`; a Properties line exists; it does not end in a comma.
- `mods/salty-surf/CHANGELOG.md`: `## Unreleased` with one bullet: "Water and Salt Water no longer show a missing-text entry in the Properties line of their codex page and info panel."

## Build output
SaltySurf (Release, ModDeployFolder=none):
```
  SaltySurf -> C:\dev\ONI-Mods\mods\salty-surf\src\SaltySurf\bin\Release\SaltySurf.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
SaltySurf.Tests, final run:
```
...Persistence.cs(52,36): warning CS0618: 'Object.FindObjectsOfType<T>()' is obsolete ... (pre-existing)
    1 Warning(s)
    0 Error(s)
```
(An earlier attempt failed with CS1061, `LocString` has no `Split`; fixed with a `(string)` cast.)

## Notes
- The test was not run in game. The Properties-line prefix is found by splitting the localized `ELEMENTPROPERTIES` string at `{`; if the game's string has leading markup the line is still matched by Contains.
- Store gap from unit 04 stands: record that a nameless tag shows as `MISSING.STRINGS.MISC.TAGS.<NAME>` and that `HiddenElementTags` hides it (glossary oreTags or Business Rules).
- ICON tooling friction: none.
