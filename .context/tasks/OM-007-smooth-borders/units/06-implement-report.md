# Unit 06 report — S3 groups, options, strings

Commit: 25b1416 "OM-007: Smooth Borders settings and strings" on OM-007-smooth-borders (trailers as specified; staged by path, common pointer not staged).

## Files (all under mods/smooth-borders/src/SmoothBorders/)
- Groups.cs (new): `Group` (Id, Default, Min, Max, Clamp, RangeText) and `Groups` table: Primary 4/1-16, Small 3/1-7, Moonlets 2/1-6, TinyMoonlets 1/1-3; `ForWorld(t, r)` maps (10,6) Primary; (5,2),(4,1) Small; (4,2),(3,1) Moonlets; (2,1),(1,0) Tiny; else null. `Thickness(t, r, Options)`: t<=0 gives 0, unknown pair gives max(1, t-r), else the clamped setting. No game types.
- Options.cs (new): OptIn JSON, `[ConfigFile(SharedConfigLocation: true)]`, four int properties, `ValueFor(Group)`, `Load()` clamping each value, `CreateOptions()` yields the intro then four `IntOptionsEntry` with label "Name (min-max)" (range from the table), tooltip the game default only, `LimitAttribute(min, max)`. No static `[Option]` properties.
- TopLeftTextOptionsEntry.cs (new): subclass of PLib 4.25.0 `TextBlockOptionsEntry` (checked by decompiling PLib.dll); overrides `CreateUIEntry` with a PLabel, UpperLeft text, FlexSize right, own word-wrap style, ColumnSpan 2.
- ModStrings.cs (new): `SMOOTHBORDERS.OPTIONS.INTRO` (design §1 wording verbatim), and PRIMARY/SMALL/MOONLETS/TINY_MOONLETS each NAME and TOOLTIP ("Game default: N."). No asteroid lists.
- Mod.cs: `Translations.Register` and `RegisterOptions` lines enabled (usings added).
- translations/SmoothBorders.pot (generated, 9 strings).
All .cs files CRLF like the rest of the repo.

## Output
`dotnet build mods/smooth-borders/src/SmoothBorders/SmoothBorders.csproj -c Release -p:ModDeployFolder=none`:
```
  Merged PLib into SmoothBorders.dll
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
`python common/tools/Translations/translations.py mods/smooth-borders`:
```
Wrote 9 strings to C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders\translations\SmoothBorders.pot
ALL CHECKS PASSED
== smooth-borders
```
`python common/tools/Translations/translations.py smooth-borders --build`: same, exit clean (pot written, ALL CHECKS PASSED).

## Not met as written
The full-solution build (`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`) ends "7 Error(s)", all in common/tools/TestRunner/Session.cs (`ModConfig` does not exist, `Action` null errors): unit 05's in-progress edits in common/. None are in SmoothBorders; SmoothBorders projects showed no warnings. Re-run the solution build once unit 05 lands.

## Friction with ICON tooling
- The handoff said "Nothing matched this scope", so I read no store pointers; no ks calls were needed.
- A bash heredoc batch (multiple files plus a Python heredoc) failed with "unexpected EOF" before writing anything, as the memory note on heredocs warns; I used the Write tool instead.
