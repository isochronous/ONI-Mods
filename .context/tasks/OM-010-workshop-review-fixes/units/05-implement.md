# Unit 05 — implement: Upgrade Planner overlay cache
Ticket OM-010. Read units/01-explore-upgrade-planner.md. Fix the cache so GetOverlayColorData recompiles at most once per RecompileSeconds unless the plan or box changes (assign colorsBoxVersion where the matcher is rebuilt; check every path that should invalidate still does). Add ONE [OniTest] that calls GetOverlayColorData twice in a row with no plan/box change and asserts the second call did not recompile (use an observable result, not internal state access; if nothing observable exists, say so and propose the smallest test seam). Fix the code comment and any README text that described the cache.

Acceptance criteria:
  - Build only your own projects, e.g. `dotnet build mods/upgrade-planner/src/<Project>/<Project>.csproj -c Release -p:ModDeployFolder=none` and the Tests project if you changed it (other units build other mods at the same time; do not build the whole solution). Show the output tail.
  - Do NOT launch the game. Do NOT commit or stage anything: the manager commits. Touch only mods/upgrade-planner/ files.
  - Player-visible text: short, plain, no hedging (Workshop writing style). CHANGELOG: one short plain bullet under an Unreleased heading, following the file's pattern.

Report: per-file change summary, new/changed test names, build output verbatim.
- File: .context/tasks/OM-010-workshop-review-fixes/units/05-implement-report.md
- Model tier: default
