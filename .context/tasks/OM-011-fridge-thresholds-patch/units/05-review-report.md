# OM-011 review report 2: Fridge Thresholds Extended (1e1d658..HEAD)

Read-only review, game not launched. Passes 1-5 of code-quality-rules run over ThresholdsCompat.cs, README.md, Tests/Fridges.cs, Tests csproj and the sln diff; testing-discipline applied to the five tests. Evidence read: 03-review-report.md, 04-test-report.md (run 4: 5 of 5 passed).

**Verdict: no blocking findings.** The M1 fix is sound. One moderate item concerns what the round-trip test proves, and three minor items follow.

## Findings

### Moderate

**M1. The round-trip test does not fail without the M1 fix, except through a component-order assertion.** (Pass 5, testing-discipline: tests prove behaviour)
- Where: Fridges.cs:287-316 (`RoundTripLoad`), asserts at about :296 and :309-315. The test title and the green-after-load asserts name the first signal after a load. The M1 scenario needs a Freezer that is powered when it spawns. The log in 04-test-report shows the loaded Freezer was `operational False` at the first update, so Freezer's own IsFull and the thresholds flag are both red then. The green asserts run after the power-on, which raises OperationalChanged and recomputes the port. They therefore pass with or without the fix (04-test-report says as much for the first look).
- The only assertions that tell the fix apart are `thresholdsAt < freezerAt` (Fridges.cs:268 and :296). That checks component order, which is the mechanism, not the behaviour.
- Fix expected, either one:
  - Rename the test and its docs so they say what is asserted ("the thresholds component is ahead of Freezer's own, and the 35% green survives a load"), and drop the claim that it covers the first signal. Or
  - Add a behaviour check that does fail without the fix. For example, assert `LastSetFlag` is `null` or the port is correct at the first update after the load while the Freezer is already powered, which needs a save written while the generator is a saved, running part of the circuit and a read before the first power event. Show it red with the config prefix disabled once, per testing-discipline RED. If that is not feasible in the harness, record the order assertion as the accepted oracle in the test summary.

### Minor

**m1. Two buildings are placed in the same frame.** (Pass 1, memory "tests at human speed")
- Where: Fridges.cs:163-164 (`FreezerFollowsThresholds` builds Freezer and Refrigerator back to back) and :217-218 (`FreezerCopySettings` builds two Freezers back to back). The wait comes only after both are placed. The 04 runs passed, so this is not a failure, but the user told the agent several times to pace build and destroy steps in game time.
- Fix expected: `yield return t.WaitGameSeconds(1)` between the two `t.World.Build` calls.

**m2. Diagnostic logging left in the shipped tests.** (Pass 4)
- Where: Fridges.cs:~258 (`t.Log` of components, "before discovery" line with IsDiscovered/category/listed), :~274, and the "first look", "after the load" and "once powered" lines in `RoundTripLoad`. These were added to debug runs 2-3. Most are useful once, but they now sit in a suite that passes.
- Fix expected: keep the logs that explain an assertion's value (the component-order logs, the fill log in `Fill`) and delete the "before discovery" and `Look()` lines, or leave a one-line comment saying why they stay. The DiscoveredResources workaround itself is commented well (Fridges.cs, comment above `Tag food`, naming `TreeFilterable.OnSpawn`): that part is fine.

**m3. Adjacent branches are not covered for the port.** (Pass 5, coverage matrix)
- Where: Fridges.cs:150-193. `SendGreenOnLow = true` is set only in the copy test (:228), which reads the properties back and never checks the port. Every port test uses green on high. The Freezer prefix evaluates `updateLogicState` with the property the green-on-low path flips, so a break in that path would not be caught.
- Fix expected: one short case in `FreezerFollowsThresholds` (or a sibling test) that sets green on low and asserts the port is the inverse at 60% and 10%. Alternatively list it as an intentionally untested branch.

**m4. The Freezer config prefix is installed before the logic prefix, in the same try.** (Pass 3)
- Where: ThresholdsCompat.cs:162-163. If `harmony.Patch(original ...)` throws after the config prefix is in place, the thresholds component is added to the Freezer while its port is not driven by it. This only produces the old behaviour (the sliders show, the port reports full) plus a warning, so it is harmless. Reordering so `UpdateLogicCircuit` is patched first, then the config prefix, gives the cleaner failure.
- Fix expected: optional; swap the two `Patch` lines.

## Passes and checks run

1. **M1 fix, no double component (Pass 1, 3): holds.** `AddThresholdsPrefix` (ThresholdsCompat.cs:115-119) adds the component only when `GetComponent(type) == null`. The thresholds mod's postfix uses `AddOrGet` (03-review-report, Storage.cs:411), so it finds the one already there. 04-test-report shows thresholds at index 34 and Freezer at 37 on both a built and a loaded Freezer, and no duplicate (the order assertion would have reported the first match only, so a duplicate would be hidden; `AddOrGet` semantics are the guard). The IR fridges use the same prefix.
2. **Absence-safety and JIT-safety (Pass 3): hold.** Every patched signature uses `GameObject`, `Component`, `FilteredStorage`, `Operational`, `LogicPorts` and `object`. `FreezerConfigType` is a string const resolved through `AccessTools.TypeByName`; a missing class gives `configure == null`, reported as "FreezerConfig.DoPostConfigureComplete not found", and the Freezer logic part is skipped, which is what the file's design promises. The llunak early return (ThresholdsCompat.cs:~145) still comes before the config prefix, so a thresholds mod that handles Freezer itself does not get a second add. IR "class not found" warning (ThresholdsCompat.cs:90-94) is now distinct from "method not found" and sits after the all-null return, so it fires only when some IR classes resolve and others do not. That closes m1 of the first review.
3. **README last line (Pass 4): fine.** "The build does not need any of the three mods installed." says what is true and drops the "Harmony only, no PLib" mechanics (first review m4).
4. **Tests assert what they name (Pass 5):** `FreezerFollowsThresholds` asserts 1,1,0,0,1 on the Freezer and on a Refrigerator control, plus red when unpowered. `IndustrialRevolutionFridge` asserts registration, the component, and 1,1,0. `FreezerCopySettings` asserts the pre-state (false), `ApplyCopy` returns true, and all three values on the target. None is a "no exception" test. The round-trip pair is covered by M1 above.
5. **Reflection-only access to other mods' types: holds.** `AccessTools.TypeByName` and `AccessTools.Property` only; no using of foreign namespaces, and the csproj references only the mod and TestRunner.
6. **Pacing in game time: holds** (`WaitGameSeconds`, `WaitUntil` with a game-time limit, `t.Speed(2)`), except m1.
7. **Cleanup:** `RoundTripLoad` restores `Session.SavePath` and deletes both run-folder files in `t.Cleanup`. If `WriteRoundTrip` fails before saving, `Session.SavePath` is not yet changed, so nothing leaks.
8. **Tests project shape: matches.** The csproj is identical to VentFreezeFix.Tests apart from the names and project reference (same props/targets imports, `ModDeployFolder none`, `Private="false"` on both references). The sln has the project entry, Debug/Release configs and the NestedProjects line under the `fridge-thresholds-extended` folder `{9A160FD4-...}`, which is where the mod's main project sits. CRLF/BOM were kept per 04-test-report.
9. **Security (Pass 2):** no input, I/O limited to run-folder test files. Nothing applicable.

No commented-out code, no swallowed exceptions. Commit messages are single terse lines.

## Feedback on the brief
- The brief's pointers were all retrospective lessons from unrelated mods and none applied; it named no record on the harness or on testing in the repo, so the testing-discipline and 03/04 reports were the real guides. Pointer one-line reasons matched what they said. Nothing governing was missing that I could identify, other than the user memory "tests at human speed", which sits outside the store.
- No ICON friction in this unit.
