# Unit 04 report: S1 scaffold of mods/smooth-borders

Commit: `88214fb` "OM-007: Smooth Borders scaffold" on OM-007-smooth-borders (13 files, 121 insertions; common/ untouched).

## Files created
- `mods/smooth-borders/.gitignore`, `.gitattributes` (copies of ovagros-overhead's)
- `mods/smooth-borders/CHANGELOG.md` (`## Unreleased`, one first-version bullet)
- `mods/smooth-borders/src/SmoothBorders/SmoothBorders.csproj` (v0.1.0, UsePLib, UseTranslations, ReferenceUnityUI, Newtonsoft.Json Private=false)
- `src/SmoothBorders/mod.yaml` (title "Smooth Borders", staticID `Isochronous.SmoothBorders`, description from design section 5)
- `src/SmoothBorders/mod_info.yaml` (supportedContent ALL, minimumSupportedBuild 744825, version 0.1.0, APIVersion 2)
- `src/SmoothBorders/Mod.cs` (`SmoothBordersMod : UserMod2`, all-or-nothing try/catch, `PUtil.InitLibrary(false)`, Translations/POptions registration lines commented until S3, "Loaded version" log)
- `src/SmoothBorders.Tests/SmoothBorders.Tests.csproj` (empty project, copy of the Ovagros tests csproj, no PLib reference)

## Files modified
- `ONI-Mods.sln`: solution folder `smooth-borders`, projects SmoothBorders and SmoothBorders.Tests, config lines, NestedProjects (new GUIDs, inserted after the Ovagros entries; CRLF and BOM kept)
- `README.md`: row after Smart Weight Plate (alphabetical)
- `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml`: option "Smooth Borders"
- `.github/workflows/label-mod.yml`: `"Smooth Borders": "smooth-borders"`

All new and edited files use CRLF like their neighbours. The edit script was in the scratchpad, not the repo.

## Build evidence
Command: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`

Tail of the second run (filtered for SmoothBorders lines and the totals):
```
  SmoothBorders -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders\bin\Release\SmoothBorders.dll
  Added assembly '...\SmoothBorders\bin\Release\SmoothBorders.dll'
  Added assembly '...\SmoothBorders\bin\Release\PLib.dll'
  Merging 2 assemblies to '...\SmoothBorders\bin\Release\SmoothBorders.dll'
  Merged PLib into SmoothBorders.dll
  SmoothBorders.Tests -> C:\dev\ONI-Mods\mods\smooth-borders\src\SmoothBorders.Tests\bin\Release\SmoothBorders.Tests.dll
    5 Warning(s)
    0 Error(s)
```
First (full) run: `291 Warning(s)`, `0 Error(s)`, `Time Elapsed 00:00:10.95`. Warnings in both runs come from other projects (motion-sensor-range PreviewGen CA1416, TestRunner and other mods' Tests CS0618/CS0649); none carries a SmoothBorders path. `git status` before the commit showed only the S1 paths.

## Not done, by design
Store node for the mod (manager M1), PLib-using code, groups/options/patches/tests (S3+). The solution-level "0 warnings" in S1's acceptance cannot hold: the solution already has other projects' warnings. The check applied was no warnings from SmoothBorders.

## Report back on the packet
- Pointers that did not apply: none of the brief's records were needed (the work was a copy of the Ovagros pattern; I did not open the ks pointers).
- Missing from the brief: nothing found.
- The handoff's warning "mods/smooth-borders is not on disk" was expected (the directory is what this unit creates).

## ICON tooling friction
None. I did not call ks.
