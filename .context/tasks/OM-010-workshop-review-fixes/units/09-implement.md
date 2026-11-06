# Unit 09 — implement: Vent Freeze Fix tooltip translations

Ticket OM-010. The English Repair TOOLTIP in mods/vent-freeze-fix/src/VentFreezeFix/ModStrings.cs changed in aa2fae7 (the watcher now is described as running for the first second after a liquid vent spawns, on load or when built). Bring the .pot and the ko, ru and zh .po files up to date for that string, following how the repo's translation workflow does it (see the mod translations system: `python common/tools/Translations/translations.py <mod-folder> --build`; write .po files with the Write/Edit tools, never shell heredocs). Use the game's own terms in each language, matching the existing lines in those files.

Acceptance criteria:
  - `python common/tools/Translations/translations.py mods/vent-freeze-fix --build` reports ALL CHECKS PASSED — output in report.
  - Touch only mods/vent-freeze-fix/. Do not launch the game.
  - Commit with one terse line `OM-010: Vent Freeze Fix tooltip translations`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

Report: File .context/tasks/OM-010-workshop-review-fixes/units/09-implement-report.md (commit it with the work); the three new translated lines and the check output. Model tier: default
