# Unit 05 report: S1, scaffold mods/ovagro-overhead

Commit `bea076e` on OM-006-ovagro-overhead, message `OM-006: scaffold Ovagro Overhead` with both trailers. `git status --short` is empty after it (this report file was written after the commit; the caller commits it with the phase).

## Diff summary

Created, `mods/ovagro-overhead/`:
- `.gitignore`, `.gitattributes`: byte copies of just-fall-already's.
- `src/OvagroOverhead/OvagroOverhead.csproj`: AssemblyName/RootNamespace OvagroOverhead, Version 0.1.0, props/targets imports (CRLF, as the template).
- `src/OvagroOverhead/mod.yaml`: title "Ovagro Overhead", one-sentence description, staticID `Isochronous.OvagroOverhead`.
- `src/OvagroOverhead/mod_info.yaml`: `requiredDlcIds:` list with `DLC4_ID`, minimumSupportedBuild 744825, version 0.1.0, APIVersion 2 (no `supportedContent`).
- `src/OvagroOverhead/Mod.cs`: `OvagroOverheadMod : UserMod2`, `OnLoad` calls `base.OnLoad(harmony)` and logs `[OvagroOverhead] Loaded version ...`. No patches.
- `src/OvagroOverhead.Tests/OvagroOverhead.Tests.csproj`: the JustFallAlready.Tests csproj renamed (`ModDeployFolder` none, TestRunner reference `Private="false"`). No test files.

Modified:
- `ONI-Mods.sln`: new GUIDs; solution folder `ovagro-overhead` {6860FBA7-...}, project OvagroOverhead {C6B01296-...} (after VentFreezeFix), OvagroOverhead.Tests {3522D94C-...} (after NaturalBackwalls.Tests); 8 configuration lines; 2 NestedProjects lines. BOM and CRLF preserved (starts `EF BB BF`; diff is 16 added lines only).
- `README.md`: row after No Magic Fridges.
- `.github/workflows/label-mod.yml`: `"Ovagro Overhead": "ovagro-overhead",` after No Magic Fridges.
- `.github/ISSUE_TEMPLATE/bug_report.yml`, `feature_request.yml`: "Ovagro Overhead" option after No Magic Fridges (plan A1).

## requiredDlcIds confirmed against the game loader

`decomp/KMod/Mod.cs` (scratchpad decomp), class `KMod.Mod`:
- `PackagedModInfo` (line 33) implements `IHasDlcRestrictions`; `public string[] requiredDlcIds { get; set; }` at line 38.
- `GetModInfoForFolder` (line 407) reads `mod_info.yaml` and parses it with `YamlIO.Parse<PackagedModInfo>` at line 436, so a YAML sequence under `requiredDlcIds:` fills the string array.
- Lines 442-480: the legacy `supportedContent` branch runs only when `requiredDlcIds == null && forbiddenDlcIds == null`; with `requiredDlcIds` set it is skipped.
- Lines 482-491: each entry is upper-cased and checked with `DlcManager.IsDlcId`; an unknown id only logs a warning.
- Lines 344 and 354: the `Mod` copies the array and gates loading with `DlcManager.IsCorrectDlcSubscribed(packagedModInfo)`.
- `fp/DlcManager.cs` line 70: `DLC4_ID = "DLC4_ID"`; line 112 registers it (Prehistoric Planet Pack).

The list form parses; whether the game loads the mod with DLC4 present is proven only by a real load (plan A3, first test run in S4a).

## Build output (verbatim tail)

`dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`:

```
    291 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.15
```

Both new projects built:
```
  OvagroOverhead -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead\bin\Release\OvagroOverhead.dll
  OvagroOverhead.Tests -> C:\dev\ONI-Mods\mods\ovagro-overhead\src\OvagroOverhead.Tests\bin\Release\OvagroOverhead.Tests.dll
```
`dotnet sln ONI-Mods.sln list | grep -i ovagro` shows both project paths. The 291 warnings are the existing ones in other mods (CA1416, CS0618, CS0649); none from the new projects. `-BuildOnly` via Invoke-OniTests was not run (the spec's acceptance names only the solution build).

## Deviations

- None from the S1 section. A script (Python, in the scratchpad) did the file creation and edits so CRLF/BOM could be controlled; the solution edit asserted its anchors exactly once each.
- mod.yaml description is my own one sentence (the S1 text left the wording open).

## Store reads

Runs (cwd `C:/dev/ONI-Mods`):
1. `node c:/dev/ai/icon/bin/ks.mjs read blk-c8ecd5229a93 blk-b6ca8e2d3bd3 blk-319d2b9dd5c7` resolved all three (3 rows, 2527 B).
   - `blk-c8ecd5229a93`: a new mod triggers sln entry, label map, README table, release plan; applied.
   - `blk-b6ca8e2d3bd3`: commit line format `OM-NNN: ...`, the build command; applied.
   - `blk-319d2b9dd5c7`: first Workshop upload is deliberate; not relevant to S1 (no release).
2. No `ks brief` run; the packet's brief was enough. Packet code-style/naming copies from other mods did not apply (Mod.cs is a literal copy of the template).

Read as files: handoff-5.md, units/04-plan-report.md (S1), the template mod's files, sln/README/label/issue-template anchors, decomp KMod/Mod.cs and fp/DlcManager.cs.

Packet report-back: nothing that governs this work was missing. The ~45 per-mod code-style pointers did not apply.

## ICON friction

- None hit. `ks read` with three ids worked first call. (Known and repeated: the packet's path brief spent most of its budget on other mods' copies of one code-style standard.)
- Not ICON: git prints "LF will be replaced by CRLF" warnings for files on commit (autocrlf); harmless.
