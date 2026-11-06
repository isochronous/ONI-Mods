# Unit 07 — implement: Just Fall Already log gate and critter scan
Ticket OM-010. Read units/03-explore-just-fall-already.md. (1) Log the per-fall line (FallPoke.cs:127) only when the game was started by the TestRunner: any command-line argument starting with `--onitest-` (see common/tools/TestRunner/Invoke-OniTests.ps1 launch args). Read the args once, not per fall. The once-per-load and slow-handler lines stay. The Doors.cs tests keep counting that line unchanged. (2) Stop CrittersAt rebuilding a dictionary over all Components.Brains every frame: find a cheaper lookup (e.g. only the armed cells, or a per-frame cache built lazily once), same behaviour. Update doc comments. CHANGELOG bullet.

Acceptance criteria:
  - Build only your own projects, e.g. `dotnet build mods/just-fall-already/src/<Project>/<Project>.csproj -c Release -p:ModDeployFolder=none` and the Tests project if you changed it (other units build other mods at the same time; do not build the whole solution). Show the output tail.
  - Do NOT launch the game. Do NOT commit or stage anything: the manager commits. Touch only mods/just-fall-already/ files.
  - Player-visible text: short, plain, no hedging (Workshop writing style). CHANGELOG: one short plain bullet under an Unreleased heading, following the file's pattern.

Report: per-file change summary, new/changed test names, build output verbatim.
- File: .context/tasks/OM-010-workshop-review-fixes/units/07-implement-report.md
- Model tier: default
