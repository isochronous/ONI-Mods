## Task: OM-010
## Branch: OM-010-workshop-review-fixes
## Objective: Fix four issues before the next release round. Three come from another dev's analysis of Workshop mods; Jeremy raised the fourth.
1. Upgrade Planner reads/refreshes every map cell every frame while its plan overlay is active.
2. Vent Freeze Fix adds vent watchers to any vent spawned at any time, not only on load.
3. Just Fall Already logs every forced fall.
4. Salty Surf's custom liquid element has no name string and shows in the sandbox element picker with a missing-string label. Hide it from sandbox tools if possible; otherwise give it a name that tells players not to paint it into the map.
5. Sweep Zones Redux (added by Jeremy 2026-10-22): a sweep errand the player cancels on debris inside a sweep zone is re-marked within a second; player-cancelled errands should stay cancelled, if that is fairly simple.
## Folder: .context/tasks/OM-010-workshop-review-fixes/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation, implementation, testing, completion
- **Current**: completion   (status: done)
- **Next**: none — task complete
- **Loaded skill**: task-plan-phase-completion
- **Branch**: OM-010-workshop-review-fixes
- **Attempts (current phase)**: 0

## Decisions
- One task and one branch for all four: each fix is small and they ship together in the next release round (Jeremy, 2026-10-22). One unit per mod node.
- Upgrade Planner: assign colorsBoxVersion so the 1 s cache works (no choice involved).
- Vent Freeze Fix: leave the code; fix README, Workshop description and store note to say every vent gets the watcher and it does nothing on a built one (Jeremy).
- Just Fall Already: log the per-fall line only when the game was launched by the TestRunner (an `--onitest-` command-line argument); fix the per-frame all-critters scan in CrittersAt (Jeremy).
- Salty Surf: the report Jeremy had says the unnamed tag shows in Water/Salt Water's Properties line, matching unit 04; hide the tag from that line (GameTags.HiddenElementTags, tag first in the list) (Jeremy).
- The claims are second-hand: each is checked against the code before anything changes.

## Key Files
- mods/upgrade-planner/, mods/vent-freeze-fix/, mods/just-fall-already/, mods/salty-surf/

## Phase Handoff Log

### Handoff: investigation → implementation → testing → completion
**Sub-agent outputs**: units 01-04 explore (03, 04 reports on disk; 01, 02 summarised in Progress), 05-09/11/13/14 implement, 10/12/15 review, 16 debug.
**Reviewer findings**: all critical/moderate resolved or decided by Jeremy; minors fixed.
**Verification evidence**: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` → 0 Error(s). Run om010-20261022-123342: "73 passed, 1 failed, 0 errors, 0 timeouts, 16 skipped" (fail = missing RequireBlueprints guard, fixed). Run om010be-20261022-125051: "17 passed, 0 failed". translations.py vent-freeze-fix: ALL CHECKS PASSED.
**What the next phase needs**: nothing — release round next (Jeremy).
**Retro Stage-1 draft**: see .context/retrospectives/icon.md OM-010 entry.

## Progress
- [x] Investigate all four (units 01-04):
  - Upgrade Planner: partly true — GetOverlayColorData cache never hits (UpgradeTool.cs:71 colorsBoxVersion never assigned, check at :302), so Compile + all BuildingCompletes scan every frame instead of once a second. One-line fix. Store record says once a second (wrong until fixed).
  - Vent Freeze Fix: true but harmless — watcher on every vent (Mod.cs:54-60), 5 ticks then removes itself; a built vent's first tick has no history and does nothing. Gating to load-spawned vents is possible; Repair.cs tests rely on built vents ticking. README/Workshop/entities.md:97 say "after a load" — wrong.
  - Just Fall Already: partly true — one line per actual poke (FallPoke.cs:127), bounded; 5 tests count that line (Doors.cs:26-29). Bigger cost: per-frame Components.Brains scan in CrittersAt.
  - Salty Surf: unit says no element is added, only tag SaltySurfWater (Mod.cs:30, 47-58) on Water/Salt Water; the missing string shows in their Properties line. Contradicts Jeremy's sandbox observation — ask.
- [x] Ask Jeremy — answers in Decisions
- [ ] Implement units 05-08 in parallel (no commits by units; manager commits) ← IN PROGRESS
- [x] Translations for VFF tooltip (unit 09) — dbf7ec0
- [x] Review 10 (four mods) — no critical; moderate VFF wording resolved by Jeremy; 8 minor
- [x] Sweep Zones (unit 11) — 4e444dd, PlayerCancelled tag, Marking.PlayerCancelSticks
- [x] Review 12 (Sweep Zones) — M1 merges, M2 any-cancel tags; Jeremy chose both recommended fixes
- [x] Fix units 13 and 14 — 768d2ac; review 15 clean (3 minor, wording fixed by manager); store notes c407186
- [x] Whole suites run om010-20261022-123342: 73 passed, 1 failed (UpgradePlanner ByHand.CancelledRebuild: 'the plan is complete: Pick at least one building that can be built in Granite.'), 16 skipped (need Blueprints Expanded)
- [x] Debug CancelledRebuild (unit 16) — not OM-010: test lacked RequireBlueprints; added (3a70d48)
- [x] Blueprints Expanded tests om010be-20261022-125051 (-Steam 3468585385): 17 passed, 0 failed
- [x] Retro and tooling friction landed; PR

## Review Checkpoint
- Reviews 10, 12, 15 covered aa2fae7, 4e444dd, 768d2ac; later commits are review wording fixes and one test guard, verified by runs om010 (73 pass; 1 fail traced to the missing guard) and om010be (17/17).

## Open Questions / Blockers
- Note for Jeremy: the Cancel tool's default "All" filter dragged over a sweep zone also remembers cancelled sweeps there (within his rule).
- Unit 01/02 explore reports were never written; their findings live in ## Progress.

## Constraints
- In-game tests at human speed, narrowest filter; ask before runs. Check free memory before a long run.
- Vent Freeze Fix: a built vent's watcher may dig an impossible tile on ticks 2-5; keep it, text as is (Jeremy, after review 10).
- Sweep Zones: the cancel sticks only when the debris was marked for sweeping at that moment and sits in a sweep zone; any debris merging into a cancelled pile lifts the cancel (Jeremy, after review 12).
- Sweep Zones: only if simple; no large complexity for it (Jeremy). If it is not simple, report options instead of building.
- Release comes after this task, as a separate round (Jeremy).
