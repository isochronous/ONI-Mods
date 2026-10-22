# Unit 11 — explore then implement: Sweep Zones keeps player-cancelled sweeps cancelled

Ticket OM-010 (branch OM-010-workshop-review-fixes). Mod: mods/sweep-zones (Sweep Zones Redux).

Problem (Jeremy): if a player cancels a sweep errand on debris inside a sweep zone, the mod marks it for sweeping again within a second. Player-cancelled errands should stay cancelled.

Constraint (Jeremy): do it only if it is fairly simple. Do not add a lot of complexity.

Step 1, explore: find how the mod re-marks debris (the periodic scan, what it skips) and how a player cancel reaches the debris: the cancel tool or the item's own user menu toggle, Clearable / "MarkForClear"/"CancelClearing", and any events or flags the game sets. Use the decompile (`ilspycmd -t <Type>` against F:/SteamLibrary/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed/Assembly-CSharp.dll) as needed. Decide what "stays cancelled" means for a pile: until it moves or merges, or until the zone is redrawn. Say which you chose and why.

Step 2: if a simple fix exists (one small patch or flag, saved per item if it must survive a save), implement it with ONE [OniTest] paced to game time like the mod's existing tests, plus a short plain CHANGELOG bullet under Unreleased and README text if the README describes re-marking. If no simple fix exists, make no code change and report the options with their cost.

Acceptance criteria:
  - Build only mods/sweep-zones projects: `dotnet build <csproj> -c Release -p:ModDeployFolder=none`. Show the output tail.
  - Touch only mods/sweep-zones/. Do not launch the game. If player-visible strings change, run `python common/tools/Translations/translations.py mods/sweep-zones --build` and show it passes.
  - Commit with one terse line, e.g. `OM-010: Sweep Zones keeps cancelled sweeps cancelled`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`. Another unit only reads the repo; nobody else commits.

Report: File .context/tasks/OM-010-workshop-review-fixes/units/11-implement-report.md (commit it with the work). Must contain: how re-marking and cancelling work (file:line), what "stays cancelled" means here, the change or the options, test name, build output verbatim. Model tier: default
