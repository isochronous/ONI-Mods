# Unit 02 — implement: heavy-watt joint plates as jumper bridges

Step 1: Make the vanilla Heavi-Watt Joint Plate (`WireBridgeHighWattageConfig`) and Heavi-Watt Conductive Joint Plate (`WireRefinedBridgeHighWattageConfig`) jumper bridges, and add in-game tests for them.
Ticket: OM-009

Decisions already made by Jeremy (do not revisit):
- The plate's centre tile stays solid exactly as vanilla. Only its ends gain jumper placement (may sit on a power port, same as the wire bridges do).
- A plate end may NOT share a cell with a regular wire bridge end, in either direction. Keep the existing refusals (Patches.cs ~181-229). Plate end on another plate end: follow whatever the mod does for wire-bridge end on wire-bridge end.
- Plates are governed by the existing Power bridges option. No new option, no new player-visible strings.
- Plates join `BridgeKind.Power` (its node and link tables), not a new BridgeKind.

Files to create/modify (from 01-explore-findings.md Q1 "Adding the plates, file by file" and Q3 items 1-5):
  - mods/jumper-bridges/src/JumperBridges/BridgeKind.cs: `Of()` recognises `HighWattBridgeTile` as Power; `NodeAnim` gives each plate its heavy-watt wire art; fix doc comments.
  - mods/jumper-bridges/src/JumperBridges/Patches.cs: prefab postfixes on `WireBridgeHighWattageConfig` (complete + under construction); the `BuildingDef.MarkArea` prefix must keep vanilla's footprint marks (Building and FoundationTile layers) for plates — the centre tile must stay registered; `IsValidHighWattBridgeLocation` allows ends on ports while keeping vanilla's other refusals (AreCellsValid, end on FoundationTile, Grid.HasDoor, door under construction); close the tile-over-end hole if it still exists under the chosen rules.
  - mods/jumper-bridges test project (existing `[OniTest]` Power tests): add plate tests — one case per behaviour (comprehensive = representative): plate end on a port connects and carries power; centre cell is solid and registered on FoundationTile; plate end refused on a wire bridge end; deconstruct reaches the plate (centre). Do not copy the wire-bridge "middle does not mark" assertion. Pace steps to game time, as the existing tests do.
  - mods/jumper-bridges/README.md and CHANGELOG.md (and the Workshop description if the mod keeps one): mention the joint plates, in the existing style — short, player-facing, no mechanics, no hedging. Changelog: a short plain bullet under an unreleased heading, following the file's existing pattern.
Pattern to follow: the existing Power wire-bridge patches and tests in the same files.
Prior reports: units/01-explore-findings.md (read it; it has file:line citations and the vanilla decompile notes, decompile method at its top).

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — include the actual output tail.
  - Do NOT launch the game or run the in-game tests; the manager runs them.
  - Commit all created/modified files with one terse line like `OM-009: jumper heavy-watt joint plates`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/02-implement-report.md
  - Must contain: diff summary per file, the new test names (for a -Filter glob), any deviation from the decisions above with the reason, and the build output verbatim.
  - Model tier: complex
