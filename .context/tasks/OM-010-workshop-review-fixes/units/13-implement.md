# Unit 13 — implement: Sweep Zones review fixes
Ticket OM-010. Read units/11-implement-report.md and units/12-review-findings.md. Touch only mods/sweep-zones/.
Jeremy's decisions:
- M2: set ZoneStore.PlayerCancelled only when the cancelled debris was marked for sweeping at that moment AND sits in a sweep zone. Cancelling digs, buildings, Move To and so on must not tag debris. A zone painted later sweeps everything in it.
- M1: any debris merging into a cancelled pile lifts the cancel (patch the merge step, e.g. Clearable.OnAbsorb or wherever the decompile shows it), so the whole pile is swept again.
Also: pace Marking.PlayerCancelSticks to game time between mark, cancel and check; add ONE test for the merge rule and ONE that cancelling a non-sweep order on zone debris leaves it swept (one case per behaviour). Make README and CHANGELOG state the rule exactly. Partial pickup leaving the rest cancelled is fine as is.

Acceptance criteria:
  - Build only the projects of the mods you touch: `dotnet build <csproj> -c Release -p:ModDeployFolder=none`. Output tail in report. Another unit edits other mods at the same time: do not build the whole solution, do not stage or commit, do not launch the game.
  - Player text short, plain, no hedging. If player-visible strings change, run `python common/tools/Translations/translations.py <mod-folder> --build` and update the translations (write .po with Write/Edit).

Report: File .context/tasks/OM-010-workshop-review-fixes/units/13-implement-report.md. Must contain: per-change summary, test names, build output verbatim. Model tier: default
