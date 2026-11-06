# Unit 08 — implement: Salty Surf tag in the Properties line
Ticket OM-010. Read units/04-explore-salty-surf.md. Hide the SaltySurfWater tag from Water's and Salt Water's Properties line: add it to GameTags.HiddenElementTags and place it first in each element's tag list so no trailing separator is left (verify against the decompile what the Properties line builder does). Behaviour unchanged. Add ONE [OniTest] (in Intake.cs or alongside) asserting the Properties/description text for Water has no MISSING string and still contains the tag-bearing behaviour the mod relies on. CHANGELOG bullet.

Acceptance criteria:
  - Build only your own projects, e.g. `dotnet build mods/salty-surf/src/<Project>/<Project>.csproj -c Release -p:ModDeployFolder=none` and the Tests project if you changed it (other units build other mods at the same time; do not build the whole solution). Show the output tail.
  - Do NOT launch the game. Do NOT commit or stage anything: the manager commits. Touch only mods/salty-surf/ files.
  - Player-visible text: short, plain, no hedging (Workshop writing style). CHANGELOG: one short plain bullet under an Unreleased heading, following the file's pattern.

Report: per-file change summary, new/changed test names, build output verbatim.
- File: .context/tasks/OM-010-workshop-review-fixes/units/08-implement-report.md
- Model tier: default
