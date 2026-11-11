# Unit 04 report: why the Moono launch "did not start within 120 s"

Read-only. Nothing launched, edited or checked out. Log contents treated as data.

## Verdict
Not settled by files alone. The evidence rules out "started, then the script looked for the wrong process": the game never reached its own logging. Between "player never started" and "player started and died before writing a log", the second is slightly better supported, but no file distinguishes them. The narrowest launch that would settle it is at the end.

## Evidence

1. The Moono player never wrote a Player.log. Moono writes `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log`, line 1 `[engine] Moono starting: commit=...` (GameProcess.ps1:75-83, OM-014 unit 01/02 reports). After the failed run:
   - `Player.log` mtime 2026-11-11 18:11:30.234; line 1 is `Mono path[0] = 'F:/SteamLibrary/.../Managed'` (the stock game's), last lines are `Input System module state changed to: Shutdown.` It is the stock session's log, closed at 18:11:30.
   - `Player-prev.log` mtime 18:09:40 (the earlier stock session). Had Moono started and opened its log, it would have rotated/overwritten these. No file under `LocalLow\Klei\Oxygen Not Included` changed after 18:11:30.
2. The warden never saw a game. `%LOCALAPPDATA%\IsochronousOniTests\warden.log` ends with `2026-11-11 18:11:31 [0] waiting up to 150 s for the game` and has no `watching` line after it (compare every earlier session: `waiting ...` then `[pid] watching`). The previous session's lines (`18:11:30 game exited`, `18:11:31 live session closed`) are the stock session's. The warden then died with the launcher (`Stop-Process $wardenPid` in the `finally`, Invoke-OniTests.ps1:~598).
3. No run artefacts from a game: the run folder `runs/run-20261111-181125-moono` holds only `fixture.sav`, `mods.json.real`, `mods.json.used`, `check/`, `tests/`. No `status`, no `Player.log` copy (the copy at :571 is after the throw).
4. Nothing running afterwards, and the script has no kill path for a hung Moono, so a player that was still alive at 120 s would still be alive. It either never started or exited.
5. No Windows Application event log entries (any provider) 18:00-18:20; no new `OniMoono*` WER/CrashDumps file (last is `OniMoono.exe.4560.dmp`, 2026-11-10 16:59); no `launch.json`/new file under `%LOCALAPPDATA%\oni-moono` or `LocalLow\...\oni-moono` (only `update-check.json`, 17:24). Prefetch is unreadable from this account. So no crash record; a clean early exit (an error dialog dismissed or a quiet "exiting now") is not excluded.
6. The script side is consistent with what OM-014 designed: line 559 `Get-MoonoArgumentLine $gameFolder $gameArgs`, line 561 `Start-Process -FilePath $moono -ArgumentList $argLine -WorkingDirectory (Split-Path $moono -Parent)`, line 567 `Get-GameProcess | Select-Object -First 1` every 2 s for 120 s. `Get-GameProcess` (GameProcess.ps1:19-39) takes an `OniMoono` process only if its CIM command line contains `--onitest-`; the launched line does contain it, so a running player would have been found. Wrong-name/PID-file is not the cause. (A Moono process whose command line could not be read would print "Cannot read the command line of OniMoono process ..." at :34; the run's output shows none.)

## What OM-014 left
- Branch `OM-014-onimoono-direct-launch` exists in both repos. `common` is checked out at `61b048a OM-014: implementation report` (on top of `a82d83c ModTest: -Runtime`, `6faceba TestRunner: -Runtime ... Moono started directly with --game`, then docs commits). The outer repo's recorded submodule pointer is still `5078e10` on HEAD, main and OM-014 alike, so the working tree's `M common` is just "common is ahead on the OM-014 commits". The checked-out runner is the OM-014 implementation, plus this session's uncommitted edits (Invoke-OniTests.ps1:232 `$legs` fix and C# edits in Kit/TestContext.cs, Session.cs, TestBehaviour.cs).
- OM-014 plan.md (`common/.context/tasks/OM-014-onimoono-direct-launch/plan.md`): phase "implementation", Completed: investigation only. The Prove step is unticked: "BLOCKED: Jeremy's own Moono game open". `units/02-implement-report.md` says outright "The in-game proof was not run", never exercised `Start-Process OniMoono.exe --game ... --onitest-...`, and lists "Exact direct-launch command line for OniMoono.exe" as an open blocker. Unit 01 flagged the same: "arguments passing through standalone mode is unproven ... One narrow TestRunner run on Moono settles it before more code".
- So the direct launch has never once worked in a real run; OM-015's attempt is its first execution. The `-Runtime Both` path was also broken at :232 until this session's fix, which is why nobody hit this earlier.
- Direct mode is documented nowhere by Moono (README covers only the Steam launch-options form `"OniMoono.exe" -- %command%`, setup.ps1:136). The only direct-launch knowledge is strings in OniMoono.dll: usage `OniMoono [--game DIR] [--log FILE] ... [-- STEAM-COMMAND ...]`, modes `direct (--game)` and `standalone`, and `launch:` messages such as `launch: standalone: Steam isn't running`, `launch: Steam did not come up within`, `launch: standalone: the install isn't ready: `.

## Candidate causes (none confirmed)
- A. Timing against the stock session just ended. The stock game's Player.log closed at 18:11:30 and the Moono start came within about a second. Steam may still have regarded app 457140 as running (or mid-shutdown), which Moono's launcher handles as "install isn't ready" or exits. This leg ran immediately after `Stop-LiveSession`; OM-014's proof design (Both) also runs it that way, but it was never tried.
- B. Moono's direct launch in this configuration exits early for a reason it only reports in a dialog or its own console (it is a launcher; its stdout is not captured by `Start-Process`).
- C. The first-run requirement that Steam be running and logged in (it was: stock had just run).
- Less likely: an argument Moono rejects (`--onitest-*` are documented as passed through; unproven), or `--game` path quoting (the path has no spaces; `ConvertTo-CommandLineArgument` is tested).

## Recommended fix
- This task (OM-015) should not patch the runner. Run the OM-015 tests on Stock (works) and treat the Moono leg as blocked on OM-014's Prove step.
- OM-014 owns the fix: finish its Prove step. Make the runner capture why Moono ended: pass `--log <run folder>\moono-launch.log` (Moono's usage lists `--log FILE`) and, if the player has exited by the first poll, say so with its `ExitCode` instead of waiting 120 s (`Start-Process -PassThru`, check `HasExited`, surface the exit code and the log). That turns this failure from a silent 120 s timeout into a diagnosis.
- Also consider a short settle wait (until no OxygenNotIncluded/OniMoono process and Steam shows the app stopped) between legs/after `Stop-LiveSession`, if the narrow run below shows candidate A.

## Narrowest launch that would tell (needs Jeremy's approval)
With no game open, no stock leg before it, mods.json untouched: run the player alone with a log and a visible exit, e.g.
`F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --log C:\Users\thegr\AppData\Local\Temp\moono-probe.log` (the main menu; no `--onitest-` args, no mods.json change, close it by hand), or the same via TestRunner as `-Runtime Moono -Mods <one small mod> -Filter "*Defaults*"` once nothing else is open. If it starts, line 1/2 of the log show the launch mode, then one TestRunner `-Runtime Moono` run on its own (no preceding stock session) separates candidate A from the argument pass-through question. If it does not start, its log or the dialog names the reason.

## Open questions
- Does Moono's direct mode exit when Steam still holds the app as running right after the stock session ended (candidate A)?
- Does standalone/direct mode pass `--onitest-*` through to the game (unit 01 risk 1)?
- Is the Moono launcher a GUI-subsystem exe whose errors are only dialogs? (Would explain the silence.)
- Did Jeremy see any Moono window or dialog around 18:11? Only he can say.

## ICON tooling friction
- None with `ks`; I did not need to widen. The handoff brief's pointers (TestRunner standards, tools overview) were neutral for this diagnosis. Governing context that did not appear: the OM-014 task folder (`common/.context/tasks/OM-014-onimoono-direct-launch/`) and `warden.log`; I found OM-014 only through `git log` in `common`.
- The handoff's "Git context" assumes `git diff main OM-014-... -- common` shows the difference; all three refs record `5078e10` for `common`, so that diff is empty. The real difference is inside the `common` repo (checked out at `61b048a`). A reader following the spec literally would conclude nothing differs.
