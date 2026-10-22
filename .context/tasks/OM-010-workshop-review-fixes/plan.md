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
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-010-workshop-review-fixes
- **Attempts (current phase)**: 1

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

## Progress
- [x] Investigate all four (units 01-04):
  - Upgrade Planner: partly true — GetOverlayColorData cache never hits (UpgradeTool.cs:71 colorsBoxVersion never assigned, check at :302), so Compile + all BuildingCompletes scan every frame instead of once a second. One-line fix. Store record says once a second (wrong until fixed).
  - Vent Freeze Fix: true but harmless — watcher on every vent (Mod.cs:54-60), 5 ticks then removes itself; a built vent's first tick has no history and does nothing. Gating to load-spawned vents is possible; Repair.cs tests rely on built vents ticking. README/Workshop/entities.md:97 say "after a load" — wrong.
  - Just Fall Already: partly true — one line per actual poke (FallPoke.cs:127), bounded; 5 tests count that line (Doors.cs:26-29). Bigger cost: per-frame Components.Brains scan in CrittersAt.
  - Salty Surf: unit says no element is added, only tag SaltySurfWater (Mod.cs:30, 47-58) on Water/Salt Water; the missing string shows in their Properties line. Contradicts Jeremy's sandbox observation — ask.
- [x] Ask Jeremy — answers in Decisions
- [ ] Implement units 05-08 in parallel (no commits by units; manager commits) ← IN PROGRESS
- [ ] Sweep Zones: explore + implement if simple (unit 11) ← IN PROGRESS
- [ ] Review, in-game run (narrow filters), store notes, retro, PR

## Open Questions / Blockers
- (none yet)

## Constraints
- In-game tests at human speed, narrowest filter; ask before runs. Check free memory before a long run.
- Sweep Zones: only if simple; no large complexity for it (Jeremy). If it is not simple, report options instead of building.
- Release comes after this task, as a separate round (Jeremy).
