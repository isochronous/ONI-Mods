## Task: OM-009
## Branch: OM-009-jumper-heavy-wire
## Objective: Add heavy-watt wire and heavy-watt conductive wire bridges to Jumper Bridges. Unlike the other bridge types, the vanilla heavy-watt joint plate's centre tile is a solid block, so the jumper variant has to handle a solid middle cell.
## Folder: .context/tasks/OM-009-jumper-heavy-wire/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: (none)
- **Current**: investigation   (status: in-progress)
- **Next**: implementation
- **Loaded skill**: task-plan-phase-investigation
- **Branch**: OM-009-jumper-heavy-wire
- **Attempts (current phase)**: 1

## Decisions
- (none yet)

## Key Files
- mods/jumper-bridges/: the mod being extended

## Phase Handoff Log

## Progress
- [ ] Investigate: how Jumper Bridges defines and places bridges, and how vanilla WireRefinedBridgeHighWattage / conductive heavy-watt joint plate handle the solid centre tile ← IN PROGRESS
- [ ] Update this plan with findings and next steps from investigation

## Open Questions / Blockers
- How a jumper (long-span) bridge should treat the solid centre: one solid tile in the middle, or something else? To settle after investigation.

## Constraints
- In-game tests at human speed, narrowest filter; ask before long runs.
