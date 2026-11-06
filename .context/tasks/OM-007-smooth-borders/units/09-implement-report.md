# Unit 09 report: intro text as PLib's wrapping, left-aligned text

Commit: **249a66b** "OM-007: intro text left-aligned with PLib wrapping" on OM-007-smooth-borders, both trailers. Staged by path: TopLeftTextOptionsEntry.cs and Dialog.cs only. The game was not launched; nothing was run in it (the new layout is unverified in game).

## Why the old entry was wrong
PLib's `PLabel` always gives its text child `TextAlignmentOptions.Center`: `PTextComponent.TextChildHelper`, `locText.alignment = TMPro.TextAlignmentOptions.Center;` at PLibUI/PTextComponent.cs:130. `PLabel.TextAlignment` (a TextAnchor) only places that child in the label (`ArrangeComponent`, PTextComponent.cs:40-76; called from PLabel.cs:66). So wrapped lines stayed centred whatever the entry set.

## PLib API used (PLib 4.25.0 per common/oni-mod.targets:75; source read in C:/dev/incubator/plib/ONIMods, same files, citations by file and line)
- `PeterHan.PLib.UI.PTextArea` (PLibUI/PTextArea.cs:29), the only PLib control with wrapping, left-aligned text:
  - `TextAlignment` property, type `TextAlignmentOptions` (line 61), default `TopLeft` (line 100). Passed to the text via `PTextField.ConfigureField(..., TextAlignment)` (line 130-131; ConfigureField sets `component.alignment` at PLibUI/PTextField.cs:38).
  - `textDisplay.enableWordWrapping = true;` (line 132), so lines wrap at the area's width.
  - `LineCount` (line 44; height is `LineCount` lines, line 153-154); `FlexSize` (line 38; applied to flexibleWidth/Height, lines 155-156); `BackColor` (line 33, applied line 125); `TextStyle` (line 71); `ToolTip` (line 76); `Text` (line 66).
  - `AddOnRealize(...)` (line 110), used to reach the realized object.
- `TMP_InputField.readOnly` (Unity TMP), set in the realize handler: PTextArea builds an editable input field (line 136, `lineType = MultiLineNewline`, line 173), so it is made read-only to behave as static text.
- `PUITuning.Fonts.TextLightStyle` and `PUITuning.Colors.Transparent` (same style the old entry derived from; transparent background so the dialog shows through).

## Diff summary
- `mods/smooth-borders/src/SmoothBorders/TopLeftTextOptionsEntry.cs`: `CreateUIEntry` now adds `new PTextArea(Field)` (Text, ToolTip, TextLightStyle, Transparent BackColor, `TextAlignment = TopLeft`, `LineCount = 6`, `FlexSize = Vector2.right`, read-only on realize) in the same grid spec as before (row, col 0, ColumnSpan 2, UpperLeft, CONTROL_MARGIN). The `PLabel`, the derived wrap style and `MakeStyle` are gone. Class name kept (still top-left text); Options.cs untouched. The object is still named "Intro" (root of the PTextArea).
- `mods/smooth-borders/src/SmoothBorders.Tests/Dialog.cs`: `Shown.Intro` is now a `TMP_Text`, found as the child named "Text" under a "Text Area" under "Intro" (PTextArea's hierarchy, PTextArea.cs:116-128); `Fields` skips read-only input fields so the intro is not counted as a number field (SlidersAndDefaults expects 4); `LineLeft` takes `TMP_Text`. Assertions are unchanged.

## Things to check in game (not done)
- `LineCount = 6` is a guess at the intro's wrapped height (about 410 characters); a different dialog width may leave spare space or clip. `Dialog.IntroTopLeft` logs the line count and takes a screenshot.
- The PTextArea draws a 1-px sliced box border in the text colour; that is part of PTextArea.

## Build (`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`, tail)
```
  SmoothBorders -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders\bin\Release\SmoothBorders.dll
  Merged PLib into SmoothBorders.dll
  SmoothBorders.Tests -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders.Tests\bin\Release\SmoothBorders.Tests.dll
    5 Warning(s)
    0 Error(s)
Time Elapsed 00:00:04.20
```
0 errors; none of the warnings mention SmoothBorders (grep count 0; the 5 are from other mods). The first full build (before the CRLF check) showed 288 warnings in other mods, 0 errors.

## Other
- `.context/tasks/OM-007-smooth-borders/plan.md` shows as modified in the working tree; it is not mine and not staged.
- Git warns about LF to CRLF on the two files (core.autocrlf=true); the blobs were LF before and are LF now.

## ICON tooling friction
None. I did not use `ks read` (the handoff's brief matched nothing).
