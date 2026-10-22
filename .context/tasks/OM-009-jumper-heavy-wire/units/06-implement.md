# Unit 06 — implement: let wire bridge ends and joint plate ends share a cell

Step: reverse the no-sharing rule. Ticket: OM-009 (reopened; branch OM-009-jumper-heavy-wire, PR #13).

Jeremy's new decision (do not revisit): a Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge end and a Heavi-Watt (Conductive) Joint Plate end MAY share a cell, in either order of building. The joined circuit takes the lowest rating and can overload the wire bridge; that is the player's choice. A tile may not be built over such a shared end (including over the wire bridge's end when the plate is the one sharing it). All other decisions in plan.md stand.

Files to modify: mods/jumper-bridges/** as needed —
  - Patches.cs: drop the refusals both ways (IsValidWireBridgeLocation prefix and IsValidHighWattBridgeLocation prefix, the `jointPlate` filtering in PowerPorts.HasEnd / BridgeFootprint.AnyPowerEndAt as it applies). Make sure the IsValidTileLocation refusal covers a shared end whichever bridge was built first and whether or not it holds the WireConnectors slot (see units/01-explore-findings.md Q3 item 3). Simplify any code that existed only for the no-sharing rule.
  - JointPlates.cs: replace NotOnWireBridgeEnd with one test that a plate end on a wire bridge end is valid and both are on one circuit, and that a tile on that shared end is refused. One case per behaviour; paced like the others.
  - Player text: option tooltip (ModStrings.cs + .pot + zh/ko/ru .po), README, CHANGELOG Unreleased bullet, publish/workshop-description.txt — state the rule plainly (ends may share with any power bridge end; no tile over a plate end). No mechanics, no hedging, no warnings about overload beyond a plain fact if the existing text style uses one. Rebuild, then `python common/tools/Translations/translations.py mods/jumper-bridges --build`.
Pattern to follow: existing code and tests in the same files.
Prior reports: units/01-explore-findings.md, 02-, 03-review-findings.md, 04-implement-report.md, 05-review-findings.md.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — output tail in report.
  - translations.py check passes — output in report.
  - Do NOT launch the game.
  - Commit all modified files with one terse line, e.g. `OM-009: plate and wire bridge ends may share a cell`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`. Do not push.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/06-implement-report.md
  - Must contain: per-file change summary, test names changed, build and translations output verbatim.
  - Model tier: default
