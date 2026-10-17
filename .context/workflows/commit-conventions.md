---
doc_id: doc-44a9d22a52a0
description: "The commit subject style used in ONI-Mods: mod-name prefix, one terse line, release and task commits, trailers."
---
# Commit Conventions

Derived from the `git log` of this repository (one author, with each mod's history brought in) and from `.claude/commands/release-mods.md`.

## Format
**Id:** blk-95a0fad57229

A commit subject is one terse line. New commits carry no explanatory body, and there is no conventional-commit type.

```
<Mod name>: <what changed, in plain words>
```

Use the mod's display name as the prefix (the same name its README and Workshop page use), then say what changed in terms of the game or the code, past the colon, as a sentence fragment. Real subjects from the log:

```
Jumper Bridges: new preview on glass tiles in the game's colours, made by tools/make_preview.py from the preview tests
Bitshifter: test for the NOT gate constant mask
Bitshifter: NOT gate inverts all 31 bits, bit number reads upright stood on end either way, Reader icon faces the original's way
Upgrade Planner: Suppress Notifications' damage bar setting no longer errors on a building that has not spawned
Natural Backwalls: tests in a generated cluster
```

### Shared tooling and housekeeping
**Id:** blk-02d371ff9e5a

Changes that belong to no mod start with the area that changed:

```
common: pick up its ICON .context node
Point at common with the Kanim tool; art scripts use it
Kanim scripts in the solution
Ignore Python caches
```

### Release commits (written by the release scripts)
**Id:** blk-f2f9b20f215b

```
Release bitshifter v1.2.0
Release jumper-bridges v0.2.0
Add changelog for natural-backwalls-everywhere v0.3.0
Record the Workshop item id
```

`release_mods.py` writes "Release <folder> vX.Y.Z"; the folder name and the version are the only variable parts. The changelog commit is made by hand (or by the `/release-mods` command) just before a release. Do not reword these: the release tooling and the `<folder>-v<version>` tags rely on them.

### Imported history
**Id:** blk-0de738af649c

```
Bring in vent-freeze-fix with its history
```

One commit per mod brought into the monorepo; not used for new work.

### Active task work
**Id:** blk-ffa775f8c3d0

Commits made while executing an ICON task carry the task ID as the prefix:

```
OM-001: task plan and unit specs for the populate-context pass
```

Phase-boundary commits made by the task-plan workflow add the `Phase-Handoff: <phase>` trailer (see `workflows/task-plan/`). The prefix is `OM`, from `local_task_id_prefix` in `.context/iconrc.json`.

## Task ID Generation
**Id:** blk-f7d3d5006f4f

The manager hands out task IDs for `.context/tasks/` folders and task branches. The first task is `OM-001` (three digits as shipped by the scaffold); later IDs follow the same width.

```bash
MAX=$(ls .context/tasks/ | sed -n 's/^OM-\([0-9][0-9]*\).*/\1/p' | sort -n | tail -n 1)
NEXT=$(printf '%03d' $((10#${MAX:-0} + 1)))
```

The task folder is the ID plus a kebab-case description, and the branch has the same name:

```
.context/tasks/OM-001-populate-context/
OM-001-populate-context
```

Task IDs are three digits (`OM-001`, `OM-002`), the width of the real branch `OM-001-populate-context`.

## Release Commits
**Id:** blk-9583fab238e3

A release adds no version suffix to the subject: the version is in the message ("Release bitshifter v1.2.0") and in the tag `bitshifter-v1.2.0`. The release script finds the last release from the tag, not from the log.

## Co-authorship
**Id:** blk-ffdd16c8e101

Commits made with an AI assistant end with the assistant's trailer after a blank line, then the session link, as in the repository's recent history:

```
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_<id>
```

The name in the first line is whichever model made the commit; use the exact trailer the session supplies. Commit signing is not configured (`commit.gpgsign` is unset); see `branching.md`.

## Well-formed Examples
**Id:** blk-790a45555cab

```
Bitshifter: test for the NOT gate constant mask
Jumper Bridges: preview with lit lamps and a switched-on Ribbon Writer
Salty Surf: in-game tests
common: pick up its ICON .context node
Add changelog for natural-backwalls-everywhere v0.3.0
Release jumper-bridges v0.2.0
OM-001: task plan and unit specs for the populate-context pass
```

## Changelog entries
**Id:** blk-bd19a93b7c1f

`mods/<name>/CHANGELOG.md` is where release notes live, written for players: short plain bullets under `## Unreleased`, saying what changed in the game ("Placed but unbuilt plates now show the usual white outline instead of the finished building"). Group under `### Added` / `### Changed` / `### Fixed` only when more than one kind is present. The release script turns `## Unreleased` into `## X.Y.Z - <date>` and uses it as the GitHub release body and the Workshop change note.
