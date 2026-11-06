# Unit 14 report: harness fixes (code and build only)

(Saved by the manager from the unit's hand-back message; condensed.)

## Commits
- common `8a320ef` (branch OM-007-smooth-borders): "TestRunner: NoColony tests first at the menu, last-save pointer restored, warden outside the launcher's tree".
- ONI-Mods `be89172`: "OM-007: EmptyEmpteraCamera skips in the base game, whose clusters have one asteroid".

## Changes
1. **Last-save pointer** (GameMode.ps1): `Get-SaveKeyRaw`, `Set-SaveKeyRaw`, `Save-SaveKey`, `Restore-SaveKey` keep the exact text after `SaveFilenameKey/:` in `kplayerprefs.yaml.onitest-savekey` (`absent` or `present:<text>`); restore verified, throws on mismatch. Invoke-OniTests.ps1 saves it next to Switch-GameMode on a fresh launch and restores it in leftover recovery, at fresh-launch start and in `finally` when no warden; Watch-OniGame.ps1 restores it after the mode.
2. **NoColony first** (Session.cs, Patches.cs, TestBehaviour.cs): Discover stable-sorts NoColony tests first. Start: all NoColony → as before; mixed at the menu → driver runs the NoColony group first; mixed in a colony → `PendingNoColony` + `Worldgen.LeaveColony()`, and the watchdog starts the driver once the menu settles. `RunWithoutColony` runs the leading NoColony tests, then `Session.Continue()`.
3. **Warden outlives the launcher**: `Start-Warden` creates it through `Invoke-CimMethod Win32_Process Create` (parent WmiPrvSE, out of the launcher's tree), falls back to Start-Process with a warning. Chosen over next-launch recovery because it restores everything when the game exits, not at some later launch.
4. **Compatibility.cs** CameraDiagnosis skips in the base game when there is no second asteroid.
- README updated.

## Evidence
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none`: "291 Warning(s) / 0 Error(s)". TestRunner: "0 Warning(s) / 0 Error(s)".
- Parse: GameMode.ps1, Invoke-OniTests.ps1, Watch-OniGame.ps1: 0 errors.
- Dry checks (scratchpad\dry.ps1, on a copy of kplayerprefs.yaml): empty value, a path with `$1`, spaces and parentheses, and a missing line all round-trip; whole-file hash identical. A fake warden started through Start-Warden survived `taskkill /F /T` of the launching pwsh.
- Not checked: the C# paths in game.

## Proposed in-game proofs
Mixed run (NoColony + another test) → "running the NoColony tests at the main menu before the rest"; same while in a colony → leaves the colony first; all-NoColony run; SaveFilenameKey back and no .onitest-savekey left; kill the launcher's tree mid-run → warden.log still "restored mods.json"; base-game EmptyEmpteraCamera → SKIPPED.

## Store facts
TestRunner domain/standards: NoColony tests sort first and run at the menu; the .onitest-savekey file and the GameMode.ps1 functions; warden started through WMI. Closes unit 13's SaveFilenameKey open item.

## Friction
None with ks. Brief matched TestRunner only through standards and entities; nothing on the warden or launcher recovery; tools/TestRunner tags.md has no coverage.
