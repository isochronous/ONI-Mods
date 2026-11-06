# Unit 14 — implement: minor review fixes in four mods
Ticket OM-010. Read units/10-review-findings.md (minor findings). Touch only mods/upgrade-planner, mods/vent-freeze-fix, mods/salty-surf, mods/just-fall-already.
- Upgrade Planner: add the active world id to the highlight cache key so switching asteroids refreshes at once.
- Vent Freeze Fix: player text says "spawns": use player words instead (e.g. "is built or a save is loaded") in README, Workshop description, mod.yaml, tooltip + translations; fix the README sentence where "It" now reads as the vent; add a short CHANGELOG bullet under Unreleased saying the description now matches what the watcher does. Text as is otherwise (Jeremy kept the behaviour).
- Salty Surf: make the separator comment exact; make the test's Properties-line check language-independent (no English prefix).
- Just Fall Already: split the CHANGELOG bullet into two, one per change. No test for the log gate is needed.

Acceptance criteria:
  - Build only the projects of the mods you touch: `dotnet build <csproj> -c Release -p:ModDeployFolder=none`. Output tail in report. Another unit edits other mods at the same time: do not build the whole solution, do not stage or commit, do not launch the game.
  - Player text short, plain, no hedging. If player-visible strings change, run `python common/tools/Translations/translations.py <mod-folder> --build` and update the translations (write .po with Write/Edit).

Report: File .context/tasks/OM-010-workshop-review-fixes/units/14-implement-report.md. Must contain: per-mod change summary, build and translations output verbatim. Model tier: default
