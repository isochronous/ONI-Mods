# Unit 11 — implement: Steps S7 + S8, README, CHANGELOG and Workshop text

Steps S7 and S8: the mod's player-facing text, so it is ready for its first release (no release runs in this task).
Ticket: OM-006

## Files to create/modify
As the S7 and S8 sections of units/04-plan-report.md list: mods/ovagro-overhead/README.md, mods/ovagro-overhead/CHANGELOG.md, and mods/ovagro-overhead/publish/workshop-description.txt and publish/workshop-tags.txt. Do not create publish/workshop-id.txt (the release script writes it) or a preview image (a separate step with the user).

## Pattern to follow
README: the structure of mods/just-fall-already/README.md, with the Installing and Building text of mods/jumper-bridges/README.md (it uses the monorepo links). CHANGELOG: mods/corner-build-anything/CHANGELOG.md. Workshop description and tags: the newest mods' publish/ files (the tags file carries the category and the supported DLC lines; this mod needs the Prehistoric Planet Pack).

## What the text says
What the mod does, in the player's words: Ovagro can grow from ceilings — Pips plant Ovagro seeds on natural ceilings, and Duplicants plant them in Farm Tiles and Hydroponic Farms rotated to face down; the vines grow along the ceiling. Floor planting is unchanged. It requires the Prehistoric Planet Pack. Before removing the mod from a save, dig up or clear ceiling Ovagro (wild ones and those in downward farm tiles), because the game drops them on load without the mod (design risks, units/03-design-report.md).

## Writing rules (user)
- No hedging or liability language: say what it does, plainly. No "should", "may", "experimental", no caveats about verification.
- CHANGELOG: `# Changelog`, `## Unreleased`, at most two or three short plain bullets in the player's words saying what the mod adds; no explanation of how it works.
- No invented facts: compatibility claims only as far as the tests show (they ran with this mod alone).

## Acceptance criteria
- Files match their analogues' structure; links point at the monorepo, not old per-mod repos.
- `python common/tools/Translations/translations.py mods/ovagro-overhead` is not needed (no ModStrings); say so in the report.
- Commit all created files in one commit on OM-006-ovagro-overhead, message `OM-006: Ovagro Overhead README, changelog and Workshop text`, with trailers
  Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_01DGJwTThCais45vGenfy1A9
  Leave .context/ untouched except the report file, which you do not commit.

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/11-implement-report.md
- Must contain: each file with its full text (they are short), the analogues used, the commit hash, and a "Store reads" section.
- Model tier: default
