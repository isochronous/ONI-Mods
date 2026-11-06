# Unit 03 report: Just Fall Already logging

## Verdict: partly true (as phrased, mostly false in cost)

The mod logs once per forced fall, not per frame or per grid batch. "Does not need to" is true for players but false for the test suite, which reads that line.

## Log calls (all in mods/just-fall-already/src/JustFallAlready)
| Where | When | Hot? | Keep? |
|---|---|---|---|
| Mod.cs:12 "Loaded version" | once at load | no | keep |
| FallPoke.cs:158 Critters Fall Through Open Doors found / not found | once, first door event after load (lazy getter, OpenDoorsAreAir, :142-162) | no | keep (the once-per-load summary) |
| FallPoke.cs:127 "<brain> at cell N lost its <navtype> footing to a door; told it to fall" | once per actual poke: needs a door to finish a state change (Door_SetWorldState_Patch :62, updateSim only), an armed cell in a grid batch, a critter in that cell, and its nav type invalid there | no: gated behind armed.Count != 0 (:106), armed cell hit (:113), critter present, footing invalid (:123) | the only one with a real consumer, see below |
| FallPoke.cs:138 slow-handler line | only when a handler run exceeds 2.0 ms (:36, :137) | no: threshold-gated | keep (diagnostic, rare) |

The hot path is OnGridUpdated (:104), which fires on every nav grid batch. Its early return at :106 costs one Count check when no door is armed. No log call is reachable on that path without a door event. A door event arms cells for at most 2 s (ArmSeconds) and they are disarmed after the last grid's batch (:131-135), so logs per door event are bounded by the number of critters that lose footing at that moment (a pez dropper: a handful).

## Cost
One string concat plus Unity Debug.Log (writes Player.log, with stack trace capture by default) per poked critter, per door change. Negligible versus the work already done there: CrittersAt rebuilds a per-frame dictionary over all Components.Brains (:164-185), and brain.UpdateBrain() itself. A base with many pez droppers cycling doors could log a line per critter per cycle; that grows Player.log but costs no measurable frame time. The log is not the cost to chase; the per-frame Brains scan in CrittersAt (first armed hit each frame) is bigger.

## Dependency that the fix must respect
Tests/Doors.cs:26-29 WatchPokes subscribes to Application.logMessageReceived and counts messages that start with "[JustFallAlready]" and contain "told it to fall". Every [OniTest] in Doors.cs that asserts a poke or no poke (lines 76, 107, 128, 150, 171 explicit) depends on line :127 being emitted verbatim. The class summary (Doors.cs:9) and FallPoke.cs:26 doc comment and store blk-9d52ccfdb3df ("Telemetry: each poke is logged") all state this as the mod's observable act.

## Smallest fix, if one is wanted
Nothing is required. If the reviewer insists on less logging, the least invasive change that keeps behaviour and tests: leave :127 (poke line) and :158 as is, since they are already once-per-event and once-per-load. Alternative with real reach: replace the :127 Debug.Log with a static event/counter that the tests read, and log only a count in the slow-line; reach = FallPoke.cs plus WatchPokes in Doors.cs (all 5 tests), and the Key Code Paths record. Behaviour of the mod itself is unchanged by either, since the log has no side effects (the log happens after brain.UpdateBrain(); string concat reads brain.name and navigator.CurrentNavType only).

## Test coverage
Existing [OniTest]s in Doors.cs cover the poke path and its log line (door closing on a critter; stacked pez dropper; door opening under a critter, none poked; duplicant, not poked; explicit CFTOD test). No test asserts the absence of logging when no door is armed. One test that would cover a fix to hot-path logging: build a base with no door events, run N frames with critters, assert zero "[JustFallAlready]" messages. It would pass today.

## Store records
- blk-9d52ccfdb3df (Key Code Paths) is correct against the code (poke logged with brain, cell and nav type; slow handlers log timing).
- blk-7d0c65d332f3 (FallPoke entity) is correct; its member table omits `scratch`, `cacheFrame` (minor), and says "crittersByCell ... gathered once per frame" which matches :167.
- Noted: mods/just-fall-already:tags.md has no coverage or tags (reported by the brief).

## Brief friction
The brief listed ~35 pointers; all but 3 (blk-9d52ccfdb3df, blk-7d0c65d332f3, and the just-fall-already standards) were other mods' code-style/naming "Loops/Variables/Example" records matched only because they name FallPoke.cs. They did not apply. Nothing governing was missing. The truncated brief ended before the decisions list; I did not widen it.
