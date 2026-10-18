# Unit 06 — implement: Step S2, the hidden ceiling plant and the CeilingForm helper

Step S2: add `VineMotherCeiling` (+ `VineMotherCeiling_preview`) and the `CeilingForm` routing helper, with no Harmony patches yet.
Ticket: OM-006

## Files to create/modify
As listed under the S2 section of units/04-plan-report.md, inside mods/ovagro-overhead/src/OvagroOverhead/. The design is units/03-design-report.md, "Data: one new entity, no new seed", "Routing", "Mother art" and "DLC gating" sections; follow it exactly, including what it says NOT to call (`CreateAndRegisterSeedForPlant`, `CreateAndRegisterPreviewForPlant`), the unique base trait id, the codex redirect, and the art transform (Rotation 180, FlipX, Pivot).

## Pattern to follow
Vanilla `VineMotherConfig` and `SaltPlantConfig` in the decompiled source (C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\); the repo's code style per the packet's pointers.

## Prior reports
units/03-design-report.md, units/04-plan-report.md (S2), units/05-implement-report.md (scaffold as built).

## Acceptance criteria
- `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds; the report shows the output tail and states that the new code adds no warnings. Do not claim success without showing it.
- The entity config is discoverable by the game's entity loader as the design describes, and DLC4-gated the same way the vanilla config is.
- No player-visible strings are added (plan.md D2).
- Do not launch the game; in-game verification is S4a (T1).
- Commit all created/modified files in one commit on OM-006-ovagro-overhead: `OM-006: hidden ceiling Ovagro plant`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/06-implement-report.md
- Must contain: the diff summary, the build output tail verbatim, the commit hash, every place the code departs from the design and why, anything in the design the game code contradicted, and a "Store reads" section.
- Model tier: complex
