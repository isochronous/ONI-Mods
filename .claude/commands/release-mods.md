---
description: Release the ONI mods that changed since their last release (new zip, GitHub release, Workshop update). Optional arguments - a list of mod folder names (mods/<name> in C:\dev\ONI-Mods); default is every mod.
argument-hint: "[repo ...]"
---

Release the isochronous Oxygen Not Included mods with `C:\dev\ONI-Mods\common\tools\Release\release_mods.py`. Every mod is a folder `mods/<name>` of the one repo `C:\dev\ONI-Mods` (isochronous/ONI-Mods), tagged `<name>-v<version>`. Mods to consider: $ARGUMENTS (if that is empty, all of them).

1. Run the plan first and show it to the user as a short table (mod, last release, what happens, Workshop item):

   ```
   python C:/dev/ONI-Mods/common/tools/Release/release_mods.py --plan $ARGUMENTS
   ```

2. For each mod the plan wants to bump, read the listed commits and choose the level: `patch` for fixes, art, wording and internal changes; `minor` for new player-visible features or settings; `major` only if saves or settings from the previous version stop working. When unsure, use `patch`. A mod with no release yet, or whose version the user already bumped, is released at its current version.

3. Skipped mods: report why (uncommitted work, behind its remote, nothing shipped changed, no changelog entry). A mod skipped only for the changelog entry is still releasable once step 4 is done. Never commit, stash or discard the user's uncommitted work to get a mod released; just say it was skipped.

4. Changelog. Every releasable mod must have a filled-in `## Unreleased` section in its `CHANGELOG.md`; the script refuses to release one without it (the plan reports it as skipped with "no changelog entry"). Write it now for each such mod, from the listed commits and, where the subject is not enough, the diff: short player-facing bullets under `## Unreleased`, describing what changed in the game rather than in the code ("Placed but unbuilt plates now show the usual white outline instead of the finished building", not "Fix place anim"). Group as `### Added` / `### Changed` / `### Fixed` only when there is more than one kind. A previous release renames `## Unreleased` to the version, so insert a fresh `## Unreleased` section right under `# Changelog` when it is absent. Create the file from this skeleton when it is missing:

   ```
   # Changelog

   ## Unreleased

   - ...
   ```

   Commit each mod's changelog, only that file, as "Add changelog for <name> vX.Y.Z" (X.Y.Z being the version the plan will release) and push. The release script turns the section into `## X.Y.Z - <date>` and uses it as both the GitHub release notes and the Workshop change note, so this is the only place notes are written.

5. If at least one mod is releasable, run it for real, passing the levels that are not `patch`:

   ```
   python C:/dev/ONI-Mods/common/tools/Release/release_mods.py $ARGUMENTS --bump-for <name>=minor
   ```

   This bumps `mod_info.yaml` and the csproj, dates the changelog section, commits "Release <name> vX.Y.Z" (only the mod's folder), pushes, builds, zips, tags `<name>-vX.Y.Z`, creates the GitHub release with `<Mod>-<version>.zip` attached, and then, if the mod is on the Steam Workshop, updates its item with the same zip, preview, description and change note (Steam must be running). The item id comes from `publish/workshop-id.txt`, or, when that file is missing, from looking the mod's title up among the user's published items; an id found that way is written to the file, committed and pushed by the script. Invoking this command is the user's approval for those pushes, releases and Workshop updates; do not ask again. Pass `--no-workshop` only if the user asks for a GitHub-only release.

6. Report each new release's URL (`gh release view <name>-v<version> -R isochronous/ONI-Mods --json url`), whether the Workshop update returned `k_EResultOK`, whether the Discord release notice posted (the script prints "Discord release notice: HTTP 204"; the webhook URL lives in `~/.config/oni-mods/discord-webhook.txt`, never in a repo), and any failures with their output. If the GitHub release succeeded but the Workshop step failed, retry just that step with `common/tools/WorkshopUpload update <id> publish/<Mod>.zip publish/content/preview.png`; never steamcmd.

The Workshop description is uploaded only if the live text is still what the previous release uploaded. If the script says the live text was edited on Steam, it has saved it as `publish/workshop-description.live.txt` and left the Workshop page alone: show the user the difference between that file and `publish/workshop-description.txt`, merge as they direct, and only then upload with `WorkshopUpload update <id> --description-file`. Never overwrite a live description from the local file without reading the live one first (`WorkshopUpload description <id>`).

A mod the plan reports as "not on the Workshop" (no id file and no published item with its title) gets a GitHub release only: say so, and do not publish a new Workshop item unless asked.
