## Task: OM-009
## Branch: OM-009-jumper-heavy-wire
## Objective: Add heavy-watt wire and heavy-watt conductive wire bridges to Jumper Bridges. Unlike the other bridge types, the vanilla heavy-watt joint plate's centre tile is a solid block, so the jumper variant has to handle a solid middle cell.
## Folder: .context/tasks/OM-009-jumper-heavy-wire/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-009-jumper-heavy-wire
- **Attempts (current phase)**: 0

## Decisions
- Centre tile stays solid as vanilla; only the plate ends gain jumper placement (Jeremy, option A of 01-explore-findings Q3). Smallest change, no seal/save/deconstruct impact.
- Plate ends may NOT share a cell with a regular wire bridge end; keep the existing refusals at Patches.cs:181-229 (Jeremy). Avoids the tile-over-end hole and mixed-rating rings.
- Plates come under the existing Power bridges option; no new option or strings (Jeremy).
- Plates join BridgeKind.Power's node/link tables via Of() recognising HighWattBridgeTile, not a new BridgeKind (one network manager per kind).

## Key Files
- mods/jumper-bridges/src/JumperBridges/BridgeKind.cs: Of() gains HighWattBridgeTile; NodeAnim gains the two plate IDs
- mods/jumper-bridges/src/JumperBridges/Patches.cs: plate prefab postfixes; MarkArea prefix (121-156) must keep vanilla footprint marks for plates; IsValidHighWattBridgeLocation jumper check
- mods/jumper-bridges/ tests (Power.cs), README.md, CHANGELOG.md

## Phase Handoff Log

### Handoff: investigation → implementation
**Sub-agent outputs**:
- explore unit report (units/01-explore-findings.md): "Jumper Bridges never changes a bridge's span, width, footprint or link offsets... So a jumper heavy-watt plate keeps its single solid centre cell." Main breakage: "The MarkArea prefix (121-156) must change. It returns false and skips the whole vanilla method... That is false for HighWattBridgeTile." Plates must join Power's tables. IsValidHighWattBridgeLocation must allow ends on ports while keeping AreCellsValid, FoundationTile, Grid.HasDoor and door-under-construction refusals. Deconstruct reaches plates at centre and ends; tests must not copy the wire-bridge "middle does not mark" assertion (Power.cs:141).
**Reviewer findings**: N/A this phase
**Verification evidence**: N/A (read-only investigation)
**Decisions delta**: the four decisions in ## Decisions
**Key files delta**: BridgeKind.cs, Patches.cs, Power.cs tests, README, CHANGELOG
**What the next phase needs**: units/01-explore-findings.md Q1 "Adding the plates, file by file" and Q3 items 1-5, with the decisions above.

## Progress
- [x] Investigate — units/01-explore-findings.md: the mod never changes span; plate keeps one solid centre cell; MarkArea prefix (Patches.cs:121-156) would strip plate footprint marks; plates must join BridgeKind.Power's tables; four centre options A–D
- [x] Ask Jeremy — keep centre solid, no plate/wire-bridge end sharing, under Power bridges option
- [ ] Implement plates in Jumper Bridges + in-game tests (unit 02) ← IN PROGRESS
- [ ] Review unit
- [ ] Run narrowest in-game test filter (ask Jeremy before long runs)

## Open Questions / Blockers
- (none)

## Constraints
- In-game tests at human speed, narrowest filter; ask before long runs.
