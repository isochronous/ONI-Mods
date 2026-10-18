# Unit 15 — review report: fixes for unit 13 findings 1, 3, 4, 6 (OM-006)

Reviewed: `git show ca8efd3 -- mods/ovagro-overhead/src` (Plots.cs, Removal.cs) and `git -C common show 2f23ffb` (Kit/Attributes.cs, TestBehaviour.cs). Read the surrounding code: `TestBehaviour.RunAll` (common/tools/TestRunner/TestBehaviour.cs:136-178), `Session.SaveIsFresh` set and reset points (Patches.cs:32, Session.cs:330), `Kit.GrowMother` and `Kit.GrowChains` (mods/ovagro-overhead/src/OvagroOverhead.Tests/Kit.cs:155-184), the sibling `Vines.cs:38-52`, and `Removal.cs` `Build`. Changed no file.

**Verdict: no block findings. 1 fix (partial), 1 nit.**

## Findings

| # | Sev | Where | Finding | Expected fix | Finding it applies |
|---|-----|-------|---------|--------------|--------------------|
| A | fix (partial) | `mods/ovagro-overhead/src/OvagroOverhead.Tests/Plots.cs:368` | Finding 3 asked for `Timeout = 220`, or lower inner caps. The change sets `Timeout = 200`. Inner caps are 90 + 90 real seconds (Kit.cs:158, and the `GrowChains` call at Plots.cs:~376), so 200 leaves about 20 s for setup and the game-time waits. That matches the sibling `Vines.cs:38` (also 200 with the same two calls), so it is not wrong, but the margin is smaller than the one asked for, and a slow machine can still end in a timeout instead of the named "did not grow" assert. | Set 220 as asked, or accept 200 and record that it matches the sibling. Not blocking. | 3 |
| B | nit | `mods/ovagro-overhead/src/OvagroOverhead.Tests/Removal.cs:54` | The replaced wait used `prefix + ": the farm's Ovagro to put out a branch on each side"`. `Kit.GrowMother` has no prefix parameter, so its timeout reads "the mother to grow and put out a branch on each side" with no case name. `Build` runs under a prefix (Removal.cs:26, 47-61), so a timeout here no longer says which case failed. The other asserts in `Build` still carry the prefix. | Optional: keep a prefixed message, e.g. wait on the branches here with `prefix`, or add an optional message argument to `Kit.GrowMother`. | 4 |

## Checked and fine

- **Finding 1 (doc, Attributes.cs:22-27).** The new text says the fixture area is not cleared before the test "when it runs first after a load" nor after the test before it. Code: the opening clear is skipped only when `keepFirst` (TestBehaviour.cs:138), and the between-tests clear is skipped when the next test is `KeepSave && !FreshSave` (TestBehaviour.cs:173-174). Both clauses match. The parenthetical "(when it runs first after a load)" refers to the opening clear only, and the "nor after the test before it" clause covers the non-first case. The earlier wrong "when that one keeps the save too" is gone. No finding.
- **Finding 4 (Removal.cs:54).** `Kit.GrowMother` (Kit.cs:157-158) waits on `LeftBranch != null && RightBranch != null` with the same 90 s cap, so the behaviour is unchanged. The `smi` is then fetched again at Removal.cs:55; this is a harmless extra lookup. The `Explicit` status of Removal's tests is unchanged. No other finding.
- **Finding 6 (TestBehaviour.cs:165-166).** The warning is placed after `Session.NextIndex = i + 1` and before `Session.SaveIsFresh = false`, so it reads the flag as left by the previous test. `SaveIsFresh` is set true only in `Game_OnSpawn_Patch` (Patches.cs:32) and reset to false at session start (Session.cs:330). So the first test of a run does not warn, and a KeepSave test after a reload does not warn. It warns only when a test has run since the last load, which is the case where the clear was skipped. The `LogWarning` level is stricter than the `Debug.Log` the finding suggested; it is fine. In a generated world nothing is cleared, so the warning is accurate there too. No finding.
- **Pass 2 (security).** No I/O, no input, no secrets in the changed lines. Nothing to report.
- **Pass 3 (integration).** `Removal` now calls the same helper as `Vines` and `Plots`; `Kit.GrowMother` is unchanged. The TestBehaviour change only adds a log call and does not alter control flow.
- **Pass 5 (tests).** The Plots test still asserts the same behaviour; only the timeout changed. The Removal change removes a wait, not an assertion.

## Summary

Findings 1, 4 (and 6) are applied correctly; 3 is applied with a smaller margin (200, not 220). No block findings; one fix (partial, not blocking) and one nit about the lost case prefix in the Removal timeout message.
