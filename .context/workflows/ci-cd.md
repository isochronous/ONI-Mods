---
doc_id: doc-141d002b7975
description: "What build, test and release automation exists for ONI-Mods: local scripts, in-game tests, GitHub releases, Workshop upload, Discord notice, and the one GitHub Action."
---
# CI/CD Pipeline

## Overview
**Id:** blk-bb2818272047

There is no build or test pipeline on a CI server. The mods compile against the game's own assemblies (`Assembly-CSharp.dll`, `0Harmony.dll` and the rest of `OxygenNotIncluded_Data/Managed`), which cannot live on a GitHub runner, so builds, tests and releases all run on the owner's machine. The one GitHub Action in the repository labels issues.

```
 build (dotnet) -> in-game tests (TestRunner) -> ModCheck -> release_mods.py -> GitHub release + Steam Workshop + Discord notice
   local            local, real game            local          local                  gh CLI        WorkshopUpload     webhook
```

## Stage Details
**Id:** blk-5a9acfb57722

### 1. Build
**Id:** blk-a4c3e53a9565

**Trigger**: by hand, and as the first step of every test and release run  
**Purpose**: compile each mod and, unless told not to, deploy it into the game's local mods folder

```
dotnet build ONI-Mods.sln -c Release
dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none      # build without deploying
dotnet build mods/<name>/src/<Mod> -c Release                     # one mod
```

Shared MSBuild configuration is in `common/oni-mod.props` and `common/oni-mod.targets` (target framework net48, game references, deploy step), reached through `$(OniCommon)` set in `Directory.Build.props`. The game folder is found in the usual Steam locations; override with the `GameFolder` property or the `ONI_GAME_FOLDER` environment variable. A successful build deploys the DLL, `mod.yaml`, `mod_info.yaml`, `assets/`, `anim/` and `translations/` to `Documents/Klei/OxygenNotIncluded/mods/local/<ModName>`. Natural Backwalls also deploys its `worldgen/` folder through its own `DeployWorldgen` target in `NaturalBackwalls.csproj`.

### 2. Test
**Id:** blk-abee22d3745c  
**Trigger**: by hand, before committing a mod change and before a release  
**Purpose**: run the mod's `[OniTest]` coroutines in the real game, nobody at the keyboard

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges
```

The launcher builds `mods/<name>/src/<Mod>.Tests`, checks the test DLL, starts the game through Steam, and prints one line per test. Exit code 0 means every test passed. Details in `testing/unit-testing.md` and `testing/integration-testing.md`.

### 3. Check
**Id:** blk-67a69f539b07  
**Trigger**: run by the test launcher on every test DLL; also by hand  
**Purpose**: load a mod DLL against the real game assemblies without launching the game, catching type-load failures and missing members after a game update

```
dotnet run --project common/tools/ModCheck -- <game Managed dir> <mod dll>
```

`Invoke-OniTests.ps1 -BuildOnly` runs the build and this check with no game session.

### 4. Translations check
**Id:** blk-e7a1010d79a4  
**Trigger**: after changing player-visible strings, before a release  
**Purpose**: rewrite each translating mod's `.pot` and report missing, empty, stale, obsolete and refused `.po` entries

```
python common/tools/Translations/translations.py [mod-folder ...] [--build]
```

### 5. Release
**Id:** blk-e23962568cff  
**Trigger**: by hand, from the owner's machine, through `/release-mods` (`.claude/commands/release-mods.md`) or directly  
**Purpose**: version, package, tag and publish every mod that changed since its last tag

```
python common/tools/Release/release_mods.py --plan
python common/tools/Release/release_mods.py
python common/tools/Release/release_mods.py sweep-zones smart-weight-plate --bump-for sweep-zones=minor
```

A mod is due when commits since its last `<folder>-v*` tag touched `src/` or `publish/preview.png`/`.jpg`. For each due mod the script:

1. bumps `mod_info.yaml` and the csproj `<Version>` (patch unless told otherwise; not at all if already bumped or never released), turns `## Unreleased` in `CHANGELOG.md` into `## X.Y.Z - <date>`, commits "Release <folder> vX.Y.Z" and pushes. A mod without a changelog entry is skipped.
2. runs `release.py`: Release build, `publish/content` staged from the build, `publish/<Mod>.zip`, tag `<folder>-vX.Y.Z`, GitHub release with `<Mod>-<version>.zip` attached and the changelog section as its body.
3. updates the Steam Workshop item (when the mod has one) with `common/tools/WorkshopUpload`: the zip, the preview, `publish/workshop-description.txt` and the changelog section as the change note. Steam must be running as the item owner. The item id is in `publish/workshop-id.txt`. A description edited on the Workshop page is never overwritten; it is saved as `publish/workshop-description.live.txt` for merging by hand.
4. posts a release notice to Discord.

Mods with uncommitted changes are skipped, never stashed. `--dry-run` stops after the zip; `--no-workshop` makes a GitHub-only release.

### 6. Issue labelling (the only GitHub Action)
**Id:** blk-386e66e27501  
**Trigger**: an issue is opened or edited  
**Purpose**: set the issue's label to the mod chosen in the issue form's "Mod" dropdown

`.github/workflows/label-mod.yml` runs `actions/github-script@v7` on `ubuntu-latest`, reads the "### Mod" answer and applies the matching mod-folder label, removing a stale mod label. The issue forms are `.github/ISSUE_TEMPLATE/bug_report.yml` and `feature_request.yml`; blank issues are disabled in `config.yml`. A new mod needs its name added to the `labels` map in the workflow and to the forms' dropdown.

## Environment Configuration
**Id:** blk-4674175526e2

There is one environment: the owner's Windows machine with the game installed.

| Setting | Value |
|---------|-------|
| Target framework | net48 (the game's own) |
| Game assemblies | Steam install, or `GameFolder` / `GameLibsFolder` / `ONI_GAME_FOLDER` |
| Deploy folder | `Documents/Klei/OxygenNotIncluded/mods/local/<ModName>`, `ModDeployFolder=none` to skip |
| Test runs | Steam running, game not already running, one run at a time (machine-wide lock) |
| Python tools | `python -m pip install -r common/tools/requirements.txt` |

## Pipeline Configuration
**Id:** blk-84dfa3586d7e

### Secrets Management
**Id:** blk-60f67ae78c1f

| Secret | Used In | Description |
|--------|---------|-------------|
| Discord webhook URL | `release.py` | Read from a local config file outside the repository; never committed |
| Steam login | `WorkshopUpload` | The running Steam client, signed in as the Workshop item owner |
| GitHub credentials | `git push`, `gh release` | The owner's own `gh` and git login |
| `steam_api64.dll` (Steamworks SDK 1.60+) | `WorkshopUpload` | Placed next to the built exe; the game's own copy is too old |

## Branch Policies
**Id:** blk-bbd635551450

| Branch | Pipeline | Publishes To |
|--------|----------|--------------|
| main | `release_mods.py` run by hand | GitHub release, Steam Workshop, Discord |
| task branches | local build and in-game tests | - |

## Notifications
**Id:** blk-4e383b3abdff

### Success
**Id:** blk-c292c1e2d291

- Discord: `release.py` posts a release notice and prints "Discord release notice: HTTP 204" when it lands.

### Failure
**Id:** blk-3ba2ba80d9c0

- None automatic. Each script prints its failure; the `/release-mods` command reports it back to the owner.

## Manual Actions
**Id:** blk-5194c1c5cf71

### Retry a Workshop update after a successful GitHub release
**Id:** blk-f9cf6f321689

```bash
common/tools/WorkshopUpload/bin/Release/<tfm>/WorkshopUpload update <id> publish/<Mod>.zip publish/content/preview.png
```

Build the tool first with `dotnet build common/tools/WorkshopUpload -c Release`. The exact output folder is the project's own; TODO(owner): record the path the owner runs it from.

### First Workshop upload of a new mod
**Id:** blk-319d2b9dd5c7

A deliberate `WorkshopUpload publish <Mod>.zip <preview.png> --title "..."`, or Klei's own uploader. `release.py` never creates a new Workshop item; a mod with no item gets a GitHub release only.

### Rollback
**Id:** blk-bcf4dc421871

A release is not rolled back by the scripts. TODO(owner): record the procedure (withdraw the Workshop update, delete the GitHub release and tag).

## Troubleshooting
**Id:** blk-9b5c8ba817a9

### Common Failures
**Id:** blk-66b645c893e6

| Error | Cause | Solution |
|-------|-------|----------|
| "Steam failed to download the mod" for subscribers | Item uploaded with steamcmd `workshop_build_item` (manifest item, the game only reads legacy single-zip items) | Never use steamcmd for ONI; update with `WorkshopUpload update` |
| WorkshopUpload FileNotFound sharing the zip | `steam_appid.txt` not 636750, or Steam not running | Keep `steam_appid.txt` at 636750; start Steam as the item owner |
| Mod skipped by `release_mods.py` | Uncommitted changes, no remote, or no `## Unreleased` entry | Commit or finish the changelog, then rerun |
| Test run exit code 2 | The game never produced a status (launch, load or crash) | Read `Player.log` in the run folder under `common/tools/TestRunner/runs/` |
| Test run exit code 3 | Build or ModCheck failure | Read the build output; run with `-BuildOnly` |
| Player's mod list missing after an interrupted test run | The run was killed before restoring `mods.json` | The next run restores `mods.json.onitest-backup`, or copy it over `mods.json` by hand |
| Runs "waiting for it to finish" | Another run holds the machine-wide lock | Wait; runs are queued one at a time |

### Viewing Logs
**Id:** blk-d5054ba426af

- Test runs: `common/tools/TestRunner/runs/<label>-<stamp>/` (`results.jsonl`, `summary.txt`, `Player.log`)
- The game's own log: `%USERPROFILE%/AppData/LocalLow/Klei/Oxygen Not Included/Player.log`; copy it before any new launch after a crash
- GitHub Action: the Actions tab of `isochronous/ONI-Mods`
