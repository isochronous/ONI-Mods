---
description: "Overview of the ONI-Mods monorepo: its 17 mods under mods/, the shared common/ build kit submodule, the solution and props files, GitHub folder, and where each part of the repository lives"
---

# ONI-Mods

## What this is
**Id:** blk-8d8441ddc4a4

ONI-Mods is the monorepo for isochronous's Oxygen Not Included mods: 17 mods, one folder each under `mods/`, built from one solution (`ONI-Mods.sln`) against a shared build kit. Players get the mods from the Steam Workshop and from this repo's GitHub Releases; the repo is also where issues are filed. Each mod is released on its own, tagged `<mod>-v<version>`. The mods were separate repositories until October 2026; each one's history was brought here with it.

The shared build, in-game test harness and release tools live in `common/`, a git submodule of [oni-mods-common](https://github.com/isochronous/oni-mods-common). `Directory.Build.props` points every project at it as `$(OniCommon)`. A mod build deploys the mod into the game's local mods folder.

## Where things are
**Id:** blk-7aa672e09782

- `mods/` — the 17 mods, one folder each (`src/`, `publish/`, README, CHANGELOG); the `mods` node covers them. Each mod is its own node under it.
- `common/` — submodule oni-mods-common: `oni-mod.props` and `oni-mod.targets` (shared build and deploy), `ilrepack-plib.targets`, `src/Translations.cs`, and `tools/` (Release, WorkshopUpload, ModTest, TestRunner, SaveRunner, WorldgenDriver, Kanim, MakePreview, ModCheck, Translations). It is a node of its own.
- `ONI-Mods.sln` — the solution holding every mod project and its tests.
- `Directory.Build.props` — defines `$(OniCommon)`, the path to `common/`.
- `README.md` — mod list, build instructions and how releases are tagged.
- `.github/` — `workflows/label-mod.yml` (labels an issue with the mod named in the form's Mod answer) and `ISSUE_TEMPLATE/` (bug report and feature request forms).
- `.claude/commands/release-mods.md` — the `/release-mods` command that runs `common/tools/Release/release_mods.py`.
- `.gitmodules`, `.gitattributes`, `.gitignore` — repo plumbing.
- `.context/` — this node's knowledge: [workflows](workflows/) (branching, commit conventions, CI/CD, task-plan phases), [testing](testing/) (unit and integration conventions), [decisions](decisions/), [retrospectives](retrospectives/), `domains/tooling-friction.md`, and `tasks/`. `projects.md` lists the child nodes (`common`, `mods`).

There is no top-level `tools/` or `scripts/` folder; shared tooling is under `common/tools/`.
