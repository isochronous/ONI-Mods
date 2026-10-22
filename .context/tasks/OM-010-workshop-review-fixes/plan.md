## Task: OM-010
## Branch: OM-010-workshop-review-fixes
## Objective: Fix four issues before the next release round. Three come from another dev's analysis of Workshop mods; Jeremy raised the fourth.
1. Upgrade Planner reads/refreshes every map cell every frame while its plan overlay is active.
2. Vent Freeze Fix adds vent watchers to any vent spawned at any time, not only on load.
3. Just Fall Already logs every forced fall.
4. Salty Surf's custom liquid element has no name string and shows in the sandbox element picker with a missing-string label. Hide it from sandbox tools if possible; otherwise give it a name that tells players not to paint it into the map.
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
- The claims are second-hand: each is checked against the code before anything changes.

## Key Files
- mods/upgrade-planner/, mods/vent-freeze-fix/, mods/just-fall-already/, mods/salty-surf/

## Phase Handoff Log

## Progress
- [ ] Investigate all four in parallel (units 01-04, one per mod) ← IN PROGRESS
- [ ] Update this plan with findings, ask Jeremy anything with more than one defensible answer

## Open Questions / Blockers
- (none yet)

## Constraints
- In-game tests at human speed, narrowest filter; ask before runs. Check free memory before a long run.
- Release comes after this task, as a separate round (Jeremy).
