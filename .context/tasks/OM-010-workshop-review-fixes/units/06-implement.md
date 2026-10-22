# Unit 06 — implement: Vent Freeze Fix text
Ticket OM-010. Read units/02-explore-vent-freeze-fix.md. No code change. README.md and publish/workshop-description.txt (and any other player text) say the watcher runs after a load; make them true: every liquid vent gets the watcher when it spawns, and on a newly built vent it has nothing to compare and does nothing. Keep it short and player-facing; no mechanics beyond what the text already explains. No CHANGELOG entry unless the file lists doc changes.

Acceptance criteria:
  - Build only your own projects, e.g. `dotnet build mods/vent-freeze-fix/src/<Project>/<Project>.csproj -c Release -p:ModDeployFolder=none` and the Tests project if you changed it (other units build other mods at the same time; do not build the whole solution). Show the output tail.
  - Do NOT launch the game. Do NOT commit or stage anything: the manager commits. Touch only mods/vent-freeze-fix/ files.
  - Player-visible text: short, plain, no hedging (Workshop writing style). CHANGELOG: one short plain bullet under an Unreleased heading, following the file's pattern.

Report: per-file change summary, new/changed test names, build output verbatim.
- File: .context/tasks/OM-010-workshop-review-fixes/units/06-implement-report.md
- Model tier: basic
