# Unit 05 report: Upgrade Planner overlay cache

Cause: `colorsBoxVersion` was declared (init -1) and compared in `GetOverlayColorData`, but never assigned, so the cache check always failed and the highlight recompiled and rebuilt every cell on every call.

## Files
- mods/upgrade-planner/src/UpgradePlanner/Tool/UpgradeTool.cs: `colorsBoxVersion = boxVersion;` assigned where the matcher is rebuilt. Doc comment on `matcher` now says plan and cells are redone when the plan or box changes, else at most once per `RecompileSeconds`.
- Invalidation paths checked: plan change (matcherPlan/matcherVersion), box change (boxVersion bumped in the box update and ClearBox, which runs before the check), tool deactivate and apply-end (`matcher = null`), 1 s timer. Each still invalidates. Building changes between recompiles show up within 1 s, as designed.
- mods/upgrade-planner/src/UpgradePlanner.Tests/Closing.cs: new test `HighlightIsCached` ("Upgrade Planner: the highlight is not worked out again on a second call with nothing changed"). Observable: the returned set is the tool's own and a recompile clears it, so the test adds a marker CellColorData, calls again, asserts the marker survives. No test seam needed. Not run (no game launch).
- mods/upgrade-planner/README.md (line 61): highlight is remade when the plan or drag box changes, otherwise at most once a second.
- mods/upgrade-planner/CHANGELOG.md: new "Unreleased / Fixed" bullet: "The building highlight is worked out once a second instead of every frame."

## Build output (verbatim tails)
UpgradePlanner.csproj (-c Release -p:ModDeployFolder=none):
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.38
```
UpgradePlanner.Tests.csproj:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.74
```

## Notes
- units/01-explore-upgrade-planner.md does not exist (the explore report is units/01-explore.md, which is only the spec); I worked from the code.
- Brief: the try/catch standards pointers from other mods were irrelevant; nothing governing was missing.
