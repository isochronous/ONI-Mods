# Unit 10 report: options dialog shows, saves and resets the four settings

## Fix taken
- The four ints are ordinary `[JsonProperty]` properties with `[Limit]`, so PLib's own `ReadFrom` / `WriteTo` (OptionsDialog.UpdateOptions, WriteOptions, OnResetConfig) handle them, including "Reset to default". `CreateOptions` now returns null.
- PLib source C:\dev\incubator\plib\ONIMods\PLibOptions (no .git there, version not checkable by commit; it has `DynamicOptionAttribute` and the same `OptionsDialog` structure as the decompiled 4.25 from unit 08).
- Order: `OptionsEntry.BuildOptions` walks `forType.GetProperties` and adds each entry to its category list in that order (OptionsEntry.cs ~l.66-77). `FillModOptions` renders each list in order. So a property declared first renders first.
- Intro: `[DynamicOption(typeof(TopLeftTextOptionsEntry))] public LocText Intro => null;` declared before the ints. `TryCreateEntry` builds the handler through `CreateDynamicOption` / `ExecuteConstructor` (OptionsEntry.cs l.108-180), which passes the property name to a `string` constructor parameter. `TopLeftTextOptionsEntry(string field)` builds its own `OptionAttribute(OPTIONS.INTRO)`. No `[Option]` on this property, because with two attributes `TryCreateEntry` takes the first that yields an entry and attribute order is not guaranteed. The property has no setter and no JsonProperty, so nothing is saved.
- Rows: `[DynamicOption(typeof(RangedIntOptionsEntry))] [Limit(a,b)] [JsonProperty]` per int. New `RangedIntOptionsEntry : IntOptionsEntry` (ctor `(string field, LimitAttribute limit)`) picks name and tooltip from ModStrings by field and appends " (min-max)" from the limit, so labels and tooltips are unchanged ("Primary asteroids (1-16)", "Game default: 4."). It is a plain IntOptionsEntry for value handling, so Reset to default refreshes the rows (tested).
- Intro area `Lines` 3 -> 4.
- Added request: a 1 px rule (`PSpacer` with an Image in `PUITuning.Colors.BackgroundLight`, full width) between the intro text and the fields. The entry's cell is now a vertical `PPanel("IntroBlock")` (Spacing 6) holding the text area and the rule; the cell keeps its padding (4,4,4,6), no border, top-left text.

## Diff summary (commit by path)
- `Options.cs`: attributes on the properties, `Intro` property, `CreateOptions` returns null, doc comment, unused usings dropped.
- `TopLeftTextOptionsEntry.cs`: `(string field)` ctor, Lines 4, IntroBlock panel with rule.
- `RangedIntOptionsEntry.cs`: new.
- `SmoothBorders.Tests/Dialog.cs`: new `SavesAndResets` (sets sliders to 7/5/4/2, clicks OK, `Options.Load()` reads 7/5/4/2, reopens without deleting config and shows 7,5,4,2, clicks Reset to default and shows 4,3,2,1 with sliders at the defaults); `IntroTopLeft` now asserts the rule exists, is 1-2 px high, sits below the intro text area and above the first field, and spans the block width.
- .pot unchanged (no string keys moved; translations.py rewrote it identically).

## Output
Build `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none -nologo`: `0 Error(s)`, no smooth-borders warnings (grep count 0; the 288 warnings belong to other projects).
translations.py: `ALL CHECKS PASSED`.

Run sb-fix (`-Filter '*Dialog*;*AsteroidKinds*;*AtMixedValues*'`, before the rule):
```
PASS     SmoothBorders.Tests/Flatness.AtMixedValues (0.0 s)
Session done: 9 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 9 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 9; 52.5 s real time; fixture loaded again 1 time(s)
```
Flatness.AtMixedValues (T4b: 7/1/5/2, generated with the config shape the dialog now writes) passed; worldgen reads the same JSON property names as before.

Run sb-rule (`-Filter '*Dialog*;*AsteroidKinds*'`, final code):
```
PASS     SmoothBorders.Tests/Dialog.LabelsAndTooltips (1.6 s)
PASS     SmoothBorders.Tests/Dialog.IntroTopLeft (1.5 s)
PASS     SmoothBorders.Tests/Dialog.SlidersAndDefaults (1.5 s)
PASS     SmoothBorders.Tests/Dialog.SavesAndResets (5.0 s)
Session done: 8 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 8 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 8; 22 s real time
```
The T4b run was before the rule change, which touches only the dialog.

Screenshot (rule visible below the 4-line intro, fields show 4/3/2/1):
C:\dev\ONI-Mods\common\tools\TestRunner\runs\sb-rule-20261019-123546\artifacts\SmoothBorders.Tests.Dialog.IntroTopLeft-dialog.png

Commit: one commit on OM-007-smooth-borders, subject "OM-007: options read and saved by PLib, rule under the intro" (hash printed by git log in the session; run `git log -1`). Trailers as specified.

## ICON tooling friction
None. I did not use ks; the handoff carried the spec and the PLib source was read directly.
