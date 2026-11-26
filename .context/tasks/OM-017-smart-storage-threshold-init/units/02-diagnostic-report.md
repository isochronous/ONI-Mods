# Unit 02: Smart Storage diagnostic (OM-017)

## Status
- Step 1 (diagnostic build): **done**. Commit `bc2e8f6` "OM-017: temporary Smart Storage diagnostic logging". Built Release and deployed to `Documents\Klei\OxygenNotIncluded\mods\local\FridgeThresholdsExtended` (DLL 14:42, contains `SmartStorageDiag`). The local FTE entry in mods.json is the one enabled for Spaced Out, so the next game start loads it.
- Step 2 (Save Runner run on Moono): **not run. Blocked**, for two reasons:
  1. **SaveRunner cannot launch Moono.** It starts the game only through `steam.exe -applaunch 457140`, and it follows an `OniMoono` process only when Steam's launch options start oni-moono. Steam's launch options for 457140 are empty (`userdata\9401199\config\localconfig.vdf`). Jeremy starts `F:\Modding\oni-moono\player\OniMoono.exe` directly; Player.log line 1 shows `Moono starting: commit=287fc57...`. OM-014 decided that only TestRunner and ModTest get `-Runtime Moono`, and SaveRunner keeps launching through Steam. So a SaveRunner run today is a stock-runtime run. The brief said to stop rather than improvise another launcher, and editing Steam settings is off limits.
  2. **Jeremy's game is open.** `OniMoono` pids 36064 and 36388 have been running since 10:13 today. SaveRunner refuses to start while any OniMoono is open.
- Nothing was launched. mods.json was not touched and no `mods.json.saverun-backup` exists. No Player.log copy was needed.

## What the diagnostic logs
File: `mods/fridge-thresholds-extended/src/FridgeThresholdsExtended/Diagnostics/SmartStorageDiag.cs`, plus one line in `Mod.cs` (`OnAllModsLoaded`, after `ThresholdsCompat.Apply`). Every line starts with `[FTE-DIAG] `.
- **At OnAllModsLoaded:** the Harmony patch owners, priorities and methods on `StorageLockerSmart.UpdateLogicAndActiveState` and `OnSpawn`, before and after the diagnostic's own patches.
- **`Locker.OnSpawn enter` / `exit`** for each locker. Each line has the full state:
  - game time, frame, instance id, cell, name, `lockerSpawned`
  - fill as stored/cap=fraction, and vanilla `PercentFull`
  - vanilla `activated`, `op=IsOperational/active`
  - the FULL port output (`port=`) and the network value at the port cell (`net=`)
  - the thresholds component: present or absent, `spawned`, registered (`ThresholdsBase.Get` non-null), and `activated`, `high`, `low`, `greenOnLow` and `last` (LastSetFlag)
- **`Thresholds.OnSpawn exit`** for each locker, with the same state, to show the spawn order.
- **`Update #n`** for each `UpdateLogicAndActiveState` call:
  - `originalRan=`: the `__runOriginal` seen by a `Priority.Last` prefix. It is True when the thresholds prefix let the vanilla body run.
  - `port a->b`, `last a->b`, the caller chain, and the state before and after the call.
  - Limited to the first 3 calls per locker, plus any call where the port or LastSetFlag changed.
- **`Sweep 2 s` / `Sweep 20 s`** (game time after `Game.OnSpawn`): each locker as `ok` or `MISMATCH`, then a count line. Expected = thresholds `activated && IsOperational`. A mismatch is any of: component not registered, LastSetFlag null, or port != expected.
- **Cap:** 4000 lines in total.

## What to read in the log (once a run exists)
- **Candidate 1 is confirmed if** the spawn-time call shows:
  - `Update #1 ... originalRan=True`, or `registered=False` on the `before:` line, or `last null->null`
  - and later sweeps show `MISMATCH` with `last=null` on lockers whose contents did not change
- **Candidate 2 holds if** the spawn-time call shows:
  - `originalRan=False` and `last null->X`, with a port that matches
  - but `activated` on the after-line differs from what the saved fill and thresholds imply
- **Neither candidate fits if** LastSetFlag is set and matches, yet `port`/`net` differ. Then something else writes the port.

## Note from code, while writing it
Storage Refrigerator Thresholds adds `StorageThresholds` in a **prefix** on `StorageLockerSmartConfig.DoPostConfigureComplete`. Vanilla adds `StorageLockerSmart` inside that method (decompiled config, line 37). So the thresholds component comes before the locker on the prefab. Under stock spawn order it spawns first and registers before `StorageLockerSmart.OnSpawn` calls `UpdateLogicAndActiveState`. This is the reverse of the OM-011 fridge case, which is why the log is needed: under stock order candidate 1 should not happen, and the log will show whether Moono changes the order or something else bypasses the prefix.

## Fix proposal (unchanged until the log exists)
- **If the log shows candidate 1:** make the resend independent of order, as in report 01 section 4.2.
  1. Add a postfix on `StorageLockerSmart.OnSpawn`. When `ThresholdsBase.Get(go)` is non-null, set `LastSetFlag = null` and call `UpdateLogicAndActiveState` through `AccessTools.MethodDelegate`, so the call goes through the patched method.
  2. Add a postfix on `ThresholdsBase.OnSpawn`, bound by reflection. When the locker `isSpawned`, make the same call.
  3. Whichever spawns second sends the thresholds value.
- **If it shows candidate 2:** follow report 01 section 4.3 (defer the first evaluation).

## Options for getting the run (manager or Jeremy to choose)
1. **Jeremy's next own session.** The diagnostic build is already deployed. His next Moono start with the newest save, `save_files\The Aftermath\The Aftermath v009.sav`, writes the `[FTE-DIAG]` lines to Player.log. No launcher is needed and his loadout is exact, including Mod Profile Manager. Ask him to:
   - restart the game, load the save
   - unpause for ~25 s, then nudge one slider on an affected locker
   - quit without saving, or save as usual (the build changes no behaviour)
   - Then copy `Player.log` and grep `[FTE-DIAG]`.
2. **Add `-Runtime Moono` to SaveRunner.** It would reuse `Get-MoonoArgumentLine` / `Wait-MoonoGame` from `tools/TestRunner/GameProcess.ps1`, as TestRunner does. This is a common-tools change in `common/` and a reversal of the OM-014 decision, so it needs Jeremy's OK.
3. **Run SaveRunner on stock.** It is unattended, but it is not the runtime he plays on. If stock shows no mismatch, that points to Moono's start order.

In every case the game must be closed first: his current session is open.
