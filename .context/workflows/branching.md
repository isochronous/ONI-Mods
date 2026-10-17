---
tags: git-branches
doc_id: doc-dac1c8fb1a00
description: "Branching in ONI-Mods: the single main branch, ICON task branches named OM-NNN, per-mod release tags, the common submodule, and the steps for task work, mod work, releases and merging"
---

# Git Branching Strategy

## Branch Structure
**Id:** blk-50d5e8ba9d8f  
**Tags:** common-submodule, release-process

One long-lived branch, `main`. Every mod lives in the same repository under `mods/<name>`, and the owner commits to `main` directly: the history is linear except for the merge commits that brought each mod in with its history ("Bring in <mod> with its history"). Work that runs under an ICON task gets its own short-lived branch.

```
main
 |
 +-- OM-001-populate-context        (task branch: <prefix>-<NNN>-<kebab description>)
 |
 +-- tags: <mod>-v<version>         (one per release, made by release.py)
```

`common/` is a git submodule of `https://github.com/isochronous/oni-mods-common.git` (see `.gitmodules`). It has its own history and its own `main`; the root repository records which commit of it is checked out.

## Branch Types
**Id:** blk-2ea6729a037b  
**Tags:** task-id, release-process

| Branch Type | Pattern | Base Branch | Merge To |
|-------------|---------|-------------|----------|
| Main | `main` | - | - |
| Task | `OM-NNN-short-description` | main | main |

There are no `develop`, `release/*` or `hotfix/*` branches. A release is a commit and a tag on `main`, never a branch.

## Naming Conventions
**Id:** blk-9121026ea410

### Task Branches
**Id:** blk-d41ad6d37e90  
**Tags:** task-id

The task ID (prefix `OM`, from `local_task_id_prefix` in `.context/iconrc.json`) is the first part of the name, followed by the task folder's kebab-case description. The branch name equals the folder name under `.context/tasks/`.

```
OM-001-populate-context
```

IDs are handed out by the manager when it creates the task folder; `commit-conventions.md` shows the generator.

### Release Tags
**Id:** blk-b55cf2df318c  
**Tags:** release-process

Each mod is tagged `<mod folder name>-v<version>`, annotated at the release commit by `common/tools/Release/release.py`:

```
bitshifter-v1.2.0
jumper-bridges-v0.2.0
upgrade-planner-v0.5.0
natural-backwalls-everywhere-v0.3.0
```

One legacy tag keeps a name from before a rename: `direct-automation-bridges-v0.1.1` (the mod is now Jumper Bridges, folder `jumper-bridges`). Release tooling finds a mod's last release as the newest `<folder>-v*` tag.

## Workflow
**Id:** blk-b8c88ce010ce

### Task Work
**Id:** blk-0e15a452ebc6  
**Tags:** commit-messages, task-id

```bash
# 1. Branch from an up-to-date main
git checkout main
git pull origin main
git checkout -b OM-001-populate-context

# 2. Commit as the work lands (one terse line, see commit-conventions.md)
git add mods/jumper-bridges
git commit -m "OM-001: <what changed>"

# 3. Push the branch
git push origin OM-001-populate-context
```

How a finished task branch returns to `main` (pull request, or a local fast-forward merge): TODO(owner). No pull request has been opened on `isochronous/ONI-Mods` so far, and merge commits appear only where a mod's history was brought in.

### Everyday Mod Work
**Id:** blk-18b48635f7e5  
**Tags:** commit-messages

The owner's own mod changes go straight to `main` with the mod's name as the subject prefix:

```bash
git add mods/bitshifter
git commit -m "Bitshifter: test for the NOT gate constant mask"
git push origin main
```

### Release Process
**Id:** blk-c57953089377  
**Tags:** release-process, changelog, steam-workshop

Releases are cut locally by `common/tools/Release/release_mods.py` (or the `/release-mods` command in `.claude/commands/release-mods.md`), never by hand and never by a branch. See `ci-cd.md` for what the script does. In short:

1. Write the player-facing notes under `## Unreleased` in `mods/<name>/CHANGELOG.md`, commit them as "Add changelog for <name> vX.Y.Z".
2. Run `python common/tools/Release/release_mods.py --plan`, then the same command without `--plan`.
3. The script bumps `mod_info.yaml` and the csproj `<Version>`, commits "Release <name> vX.Y.Z", pushes `main`, builds, zips, tags `<name>-vX.Y.Z`, creates the GitHub release and updates the Workshop item.

A mod with uncommitted changes, or whose repository has no remote, is skipped, never stashed.

### Changing common
**Id:** blk-a9ac0b6cd3cb  
**Tags:** common-submodule

`common/` is its own repository. Commit and push inside `common/` first, then commit the new submodule pointer in the root:

```bash
git -C common add -A
git -C common commit -m "TestRunner: <what changed>"
git -C common push origin main
git add common
git commit -m "common: pick up <what changed>"
```

After a game update, fix the shared references in `common/` once, push, and bump the pointer here (`git submodule update --remote common`), then rebuild and test the mods.

## Commit Messages
**Id:** blk-a9cad5493ade  
**Tags:** commit-messages

One terse subject line; new commits carry no explanatory body. The subject starts with the mod's name or `common`. See `commit-conventions.md`.

## Pull Request Guidelines
**Id:** blk-c3ed876035fc  
**Tags:** assistant-attribution

TODO(owner): no pull-request template exists in `.github/` (only issue forms under `.github/ISSUE_TEMPLATE/`). When a task branch goes through a pull request, the description carries the assistant's attribution lines as described in `commit-conventions.md` and lists what changed per mod.

### Before Merging a Task Branch
**Id:** blk-3c4f47a35d4f  
**Tags:** mod-build, in-game-testing, code-review, commit-messages

- [ ] `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds
- [ ] The in-game tests of every touched mod pass (`testing/unit-testing.md`)
- [ ] Self-review of the diff completed
- [ ] The commit subjects follow `commit-conventions.md`

## Protected Branches
**Id:** blk-5b6bfa900ebf

No branch protection or required signing is documented in the repository, and `commit.gpgsign` is not set in the local git config. TODO(owner): record any GitHub ruleset on `main`.

## Git Configuration
**Id:** blk-a059cb2ddfee  
**Tags:** common-submodule

```bash
git clone --recurse-submodules https://github.com/isochronous/ONI-Mods.git
git submodule update --init            # after a pull that moved the common pointer
```

`.gitattributes` forces LF under `.context/` and on tool files. `*.png`, `*.jpg`, `*.gif`, `*.zip` and the other binary outputs are declared `-text` in `.gitattributes`. Build output (`bin/`, `obj/`, `.vs/`, `__pycache__/`) is ignored.
