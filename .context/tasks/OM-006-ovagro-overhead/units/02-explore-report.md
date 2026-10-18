# Unit 02 report: scaffolding mods/ovagro-overhead

Read-only exploration. Nothing outside this report was changed. "Copy from" paths are relative to `C:\dev\ONI-Mods`.

## 1. Analogue chosen

**mods/just-fall-already** (v0.1.1; tag `just-fall-already-v0.1.x`) is the file-layout analogue.

- A single-feature Harmony mod with no options and no strings. It has the smallest complete file set: csproj, `mod.yaml`, `mod_info.yaml`, `Mod.cs`, one patch file, a `.Tests` project, `publish/` and README/CHANGELOG.
- Its `Doors.cs` tests are also the best template for the build, place and wait tests (section 5).
- It uses neither `UseTranslations` nor PLib. If Ovagro Overhead has no player-visible text, that is the right default.

Supporting analogues, for one piece each:

| Piece | Analogue | Why |
|---|---|---|
| Mod with strings and translations | mods/detect-any-element | `UseTranslations`, `ModStrings.cs`, `translations/`, `Translations.Register`. Smaller than vent-freeze-fix, which adds PLib options. |
| An unreleased new mod's state | mods/corner-build-anything | v0.1.0, CHANGELOG has `## Unreleased`, `publish/` holds only `workshop-description.txt` and `workshop-tags.txt` (no preview, no id), no tag yet. This is exactly what a new mod looks like before its first release. |
| README Installing/Building text | mods/jumper-bridges | It is the only README that points at the monorepo (see gap G2). |

## 2. File checklist

Folder `mods/ovagro-overhead/`. PascalCase name: **OvagroOverhead** (matches the "AssemblyName = RootNamespace = project folder" rule in the naming-conventions record `mods/just-fall-already:standards/naming-conventions.md`, `blk-fc52cb225e0f`).

### 2.1 Code and metadata (required for a build)

| File to create | Copy from | Change |
|---|---|---|
| `src/OvagroOverhead/OvagroOverhead.csproj` | `mods/just-fall-already/src/JustFallAlready/JustFallAlready.csproj` | `AssemblyName` and `RootNamespace` = `OvagroOverhead`; `<Version>0.1.0</Version>`. Keep the `$(OniCommon)oni-mod.props` import at the top and the `oni-mod.targets` import at the bottom. Add `<ReferenceUnityUI>true</ReferenceUnityUI>` only if UI types are needed (not for a pure plant patch). |
| `src/OvagroOverhead/mod.yaml` | `.../JustFallAlready/mod.yaml` | `title: "Ovagro Overhead"`; `description:` one sentence; `staticID: "Isochronous.OvagroOverhead"`. |
| `src/OvagroOverhead/mod_info.yaml` | `.../JustFallAlready/mod_info.yaml` | `version: 0.1.0`. Keep `supportedContent: ALL`, `minimumSupportedBuild: 744825`, `APIVersion: 2`. See section 4 for DLC fields. |
| `src/OvagroOverhead/Mod.cs` | `.../JustFallAlready/Mod.cs` | Class `OvagroOverheadMod : UserMod2`, `namespace OvagroOverhead`. `OnLoad` calls `base.OnLoad(harmony)` (which applies every `[HarmonyPatch]` class in the assembly) and logs `[OvagroOverhead] Loaded version ...`. |
| `src/OvagroOverhead/<Patches>.cs` | `.../JustFallAlready/FallPoke.cs` for the patch style | The feature code (unit 01 decides what). Patch convention is `blk-bcc50465e110`: static class, `[HarmonyPatch(typeof(T), nameof(T.M))]`, methods named `Prefix`/`Postfix`/`Transpiler`/`Finalizer`; a transpiler logs when it cannot find its instruction (see the error-handling record in the same node). |
| `src/OvagroOverhead.Tests/OvagroOverhead.Tests.csproj` | `mods/just-fall-already/src/JustFallAlready.Tests/JustFallAlready.Tests.csproj` | Rename `JustFallAlready` to `OvagroOverhead` in `AssemblyName`, `RootNamespace` and the `ProjectReference`. Keep `<ModDeployFolder>none</ModDeployFolder>` and the TestRunner `ProjectReference` (`$(OniCommon)tools\TestRunner\TestRunner.csproj`, `Private="false"`). |
| `src/OvagroOverhead.Tests/<Topic>.cs` | `mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs` | See section 5. |

Conventions that apply:
- C# 9, net48, namespace blocks (not file-scoped), tabs, Allman braces.
- One namespace per mod, equal to `RootNamespace`.
- Log prefix `[<PascalName>]`.
- Existing csproj and source files use CRLF line endings; the `.Tests` csproj files use LF. Neither is enforced by `.gitattributes` (only `.context/**` is forced to LF), so the choice is free.

### 2.2 Repo-facing documents

| File to create | Copy from | Change |
|---|---|---|
| `README.md` | `mods/just-fall-already/README.md` for structure; `mods/jumper-bridges/README.md` for the Installing/Building/Testing text | Sections: intro line, "What it does", "Installing", "Building", optional "Testing" and "Translations". Use the monorepo links (G2). |
| `CHANGELOG.md` | `mods/corner-build-anything/CHANGELOG.md` | `# Changelog` then `## Unreleased` with one first-version bullet. `release_mods.py` rewrites `## Unreleased` to `## 0.1.0 - <date>` and refuses to release without that section. Style: few short plain bullets saying what changed. |
| `.gitignore` | `mods/just-fall-already/.gitignore` | Unchanged: `bin/ obj/ .vs/ *.user publish/content/ publish/*.zip publish/workshop-description.live.txt`. |
| `.gitattributes` | `mods/just-fall-already/.gitattributes` | Unchanged. It is identical to `mods/.gitattributes`, an ICON-generated file, so `ks integrate` (below) may write it. |
| LICENSE | none | No mod has its own LICENSE and there is no repo-root LICENSE. Nothing to create. Only `mods/detect-any-element/tools/LICENSE-LiberationSans.txt` and the PLib copy in upgrade-planner exist, for bundled third-party files. |

### 2.3 Publish material

`publish/` is tracked Workshop material. Never restore or overwrite its files from elsewhere.

| File | Copy from | Change | When |
|---|---|---|---|
| `publish/workshop-description.txt` | `mods/just-fall-already/publish/workshop-description.txt` | Steam BBCode: a headline line, `[list][*]...[/list]`, a one-line cost/compat note, the credit line, `Source and issues: https://github.com/isochronous/ONI-Mods`. Jumper Bridges adds an `[h2]Compatibility[/h2]` paragraph. No hedging language (memory). | Before first release |
| `publish/workshop-tags.txt` | `mods/just-fall-already/publish/workshop-tags.txt` | Line 1 is a comment, line 2 the category (`tweaks` in all 8 mods that have the file), then one DLC name per line. Drop DLC lines the mod does not support. | Before first release |
| `publish/preview.png` or `publish/preview.jpg` | any | 512x512. Source of the zip's `preview.png`; `release.py` rescales anything above 512 and converts jpg to png. | Before first release |
| `publish/workshop-id.txt` | none | Do not create. See section 6. | Written by the release script |

Preview art: `common/tools/MakePreview/title_preview.py` makes a 512x512 titled preview from a background image (style notes in its docstring, fonts in `common/tools/MakePreview/fonts/`). `make_preview.py` composes a sprite with a caption band. Its docstring and the common README say 256x256, but the committed previews are 512x512 (G6).

### 2.4 Knowledge-store node (ICON, not build)

Every mod folder is its own store node: `mods/<name>/.context/` has `iconrc.json`, `META.md`, `projects.md`, `overview.md`, `tags.md`, `domains/{README,entities,glossary}.md`, `standards/{README,code-style,error-handling,naming-conventions}.md`, `decisions/README.md`, `retrospectives/shared.md` and `.gitignore`.

- `iconrc.json` is `{"repo_type":"project","local_task_id_prefix":"OM","default_branch":"main"}`.
- `projects.md` lists `OvagroOverhead -> src/OvagroOverhead` and `OvagroOverhead.Tests -> src/OvagroOverhead.Tests`.
- `mods/.context/projects.md` has one row per mod, alphabetical (`| name | path |`), and gets `| ovagro-overhead | ovagro-overhead |`.
- Create these with `ks integrate` / the `ICON:icon-integrate` skill and the write path (`ks write`), not by hand. `ks --help` lists `init`, `integrate` and `write`. The `.context/**` files are LF.
- Deferred to the implement/completion unit.

## 3. Build and solution edits

### 3.1 Directory.Build.props

Nothing to change. It only defines `$(OniCommon)` = `<repo>\common\`, and every csproj imports through it.

### 3.2 ONI-Mods.sln

No `dotnet sln add` is needed in principle, but the file is hand-maintained and every mod has the same 3 blocks. File is UTF-8 with BOM and CRLF. Add, with three new unique GUIDs (`F` = solution folder, `P` = mod project, `T` = test project):

1. **Project entries.** Insert after the `vent-freeze-fix` pair (they follow no strict order) and before the `common` folder:
```
Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "ovagro-overhead", "ovagro-overhead", "{F}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OvagroOverhead", "mods\ovagro-overhead\src\OvagroOverhead\OvagroOverhead.csproj", "{P}"
EndProject
```
   The test project goes in the later block of `.Tests` entries (after `NaturalBackwalls.Tests`, before the `tools` folders):
```
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "OvagroOverhead.Tests", "mods\ovagro-overhead\src\OvagroOverhead.Tests\OvagroOverhead.Tests.csproj", "{T}"
EndProject
```
2. **ProjectConfigurationPlatforms** (inside `GlobalSection(ProjectConfigurationPlatforms)`), four lines for each of `{P}` and `{T}`:
```
		{X}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{X}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{X}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{X}.Release|Any CPU.Build.0 = Release|Any CPU
```
3. **NestedProjects** (inside `GlobalSection(NestedProjects)`), both under the mod's folder, as just-fall-already does at its lines 341 and 366:
```
		{P} = {F}
		{T} = {F}
```
4. Verify with `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` (README: that flag builds without deploying) or `dotnet sln ONI-Mods.sln list`. A new GUID can be made with `[guid]::NewGuid()`.

The release and test scripts do not read the sln. They find mods by folder (`mods/<name>/src/<Mod>/mod_info.yaml`, `src/<Mod>.Tests/<Mod>.Tests.csproj`).

### 3.3 Root README.md

Add a row to the mod table, in alphabetical position (after "No Magic Fridges"): `| Ovagro Overhead | [mods/ovagro-overhead](mods/ovagro-overhead) |`.

### 3.4 common/

- Referenced automatically through `$(OniCommon)oni-mod.props` and `.targets`. They give the net48 target, LangVersion 9, the game assembly references (`Assembly-CSharp`, `-firstpass`, `0Harmony`, `UnityEngine`, `CoreModule`) and `DeployToLocalMods`.
- A successful build deploys to `Documents\Klei\OxygenNotIncluded\mods\local\<MSBuildProjectName>`. For the Tests project, `ModDeployFolder` is `none`.
- Opt-ins: `ReferenceUnityUI`, `ReferenceImageConversion`, `UsePLib` (PLib 4.25.0 ILRepack-merged), `UseTranslations` (compiles `common/src/Translations.cs`; the namespace is `Isochronous`).
- `common` is a git submodule (`.gitmodules`, `https://github.com/isochronous/oni-mods-common.git`). Do not edit it for this task.

### 3.5 Naming, ids and versions

| Item | Value |
|---|---|
| Folder | `mods/ovagro-overhead` |
| AssemblyName, RootNamespace, project folder | `OvagroOverhead` |
| Mod class | `OvagroOverheadMod : UserMod2` |
| `staticID` | `Isochronous.OvagroOverhead` (all mods follow `Isochronous.<PascalName>`) |
| Release tag | `ovagro-overhead-v<version>` (prefix is the folder name) |
| Zip | `OvagroOverhead-<version>.zip` |
| Harmony id | Not set by hand: `UserMod2.OnLoad(Harmony)` gets one from the game |
| First version | **0.1.0**, in `mod_info.yaml` and the csproj `<Version>`. Both must match. `release_mods.py` gives a mod with no tag its first release at the version it already has, and bumps nothing. Precedents: corner-build-anything 0.1.0, dont-count-eggs-on-rails 0.1.0, just-fall-already 0.1.0. |

## 4. DLC gating

- **No mod in this repository declares DLC gating.** All 17 `mod_info.yaml` files use `supportedContent: ALL`, `minimumSupportedBuild: 744825`, `APIVersion: 2`. A search of the mods and tools for `requiredDlcIds`/`forbiddenDlcIds` finds only `Invoke-OniTests.ps1`, which matches `dlcIds` in a save header. So there is no in-repo example to copy.
- The game itself knows both fields. I checked only the installed game's `Assembly-CSharp.dll` (`F:\SteamLibrary\steamapps\common\OxygenNotIncluded`), which contains the strings `requiredDlcIds` and `forbiddenDlcIds`. The DLC id constants present in it are `VANILLA_ID`, `EXPANSION1_ID` (Spaced Out), `DLC2_ID`, `DLC3_ID`, `DLC4_ID` and `DLC5_ID`.
- The YAML syntax for the fields (list form, per-mod key placement in `mod_info.yaml`) is not verified here. Unit 01 or the implement unit should check how the game parses `ContentInfo`/`mod_info.yaml` before writing it.
- Gating is probably unnecessary. A patch that targets an Ovagro type loads in a game without that DLC only if the type exists in the base `Assembly-CSharp.dll`. If the class is always compiled in, the patch loads everywhere and does nothing without the DLC. `[HarmonyPatch]` on a missing type fails at patch time, so confirm the type's presence (unit 01).
- Workshop tags, separately from `mod_info`, list the DLCs supported (`workshop-tags.txt`), one name per line: `Base Game`, `Spaced Out!`, `The Frosty Planet Pack`, `The Bionic Booster Pack`, `The Prehistoric Planet Pack`, `The Aquatic Planet Pack`. A mod that works only with one DLC should list only that.

## 5. In-game tests

**Where tests live and how they are found**
- Project `mods/<name>/src/<Mod>.Tests/<Mod>.Tests.csproj`, referencing the mod project and `common/tools/TestRunner/TestRunner.csproj`.
- Test classes are `public static` with methods `public static IEnumerator M(TestContext t)` marked `[OniTest("PREFIX: sentence", Timeout = n), Layer(0)]` (using `Isochronous.TestKit`).
- The launcher takes the project name, not the folder: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods OvagroOverhead [-Filter "*Name*"] [-BuildOnly]`. It looks for `src/<Mod>.Tests/<Mod>.Tests.csproj` under every `mods/*` folder of the checkout containing `common/`. No registration anywhere. The assembly's `[OniTest]` methods are discovered by the in-game harness from `tests/*.dll` in the run folder.
- `-BuildOnly` builds and runs ModCheck without a game session. A test session needs Steam and the game, which this unit does not run.

**Fixture.** `common/tools/TestRunner/fixtures/testbed-so.sav` is the Spaced Out fixture: cycle 0, no Duplicants, every DLC. `.sav` files are untracked. `testbed-base.sav` is documented but not present (G4), so base-game-only runs have no fixture. The harness clears a 40x10 area before each test.

**Best template for "build something, place an entity, wait for it to grow":** `mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs`. There is no growth or plant test anywhere in the repo.

- `t.Speed(3)` to speed up game time, `CellRect inside = t.Dupes.Box()` to wall the area with tiles and fill it with air.
- `t.World.Build("Tile", inside.Cell(x, y), element: SimHashes.SandStone)` to place a finished building. The signature is `Build(prefabId, cell, orientation, element)`. It throws with the game's reason when the placement is invalid.
- Spawn an entity: `Util.KInstantiate(Assets.GetPrefab("Hatch"), Grid.CellToPosCBC(cell, Grid.SceneLayer.Creatures))`, `go.SetActive(true)`, then `t.Cleanup(() => Util.KDestroyGameObject(go))`. Use the plant's scene layer for a plant or seed. `mods/dont-count-eggs-on-rails/.../Eggs.cs` shows `GameUtil.KInstantiate` for an item, and `mods/multichannel-critter-sensor/.../Kit.cs` has a reusable `Spawn` helper.
- `yield return t.WaitGameSeconds(n)` and `t.WaitUntil(cond, timeoutSeconds, "what")` for waiting. `t.Assert.True/False/Eq/...` for checks. `t.Log(...)` for notes.
- Any logged `Error`, `Exception` or `Assert` fails the test unless `t.AllowLogError(regex)` is used.
- For a plant that has to grow to maturity, a test will need a long wait or a way to set maturity directly. That is a game-code question for unit 01 and the implement unit. `Timeout` defaults to 60 real seconds (`-TestTimeoutSec`) and `[OniTest(Timeout = n)]` overrides it, as do `-TimeoutSec` for the session and `t.Speed(n)` for game time.

Per memory: pace tests to game time between steps (no building and destroying on adjacent frames), and run with the player's mods in mind if a bug only shows in Jeremy's game.

Other suites worth a glance: `mods/no-magic-fridges/.../Fridges.cs` (272 lines, builds and observes over time), `mods/detect-any-element/.../Kit.cs` (a per-mod `Kit.cs` of shared helpers: chamber building, settle waits).

## 6. Release prerequisites (nothing was run)

Required before the first release. The release is done by `python common/tools/Release/release_mods.py ovagro-overhead [--plan]`, which calls `release.py`.

1. Both versions match (`mod_info.yaml` and csproj `<Version>`), at 0.1.0.
2. `CHANGELOG.md` has a `## Unreleased` section with content. `--plan` reports a mod without one as skipped; "nothing ships on commit subjects".
3. All tracked files in `mods/ovagro-overhead` are committed (the script skips or refuses a mod with uncommitted tracked changes, and a repo with no remote or a HEAD not pushed). Untracked files do not block it.
4. `publish/preview.png` or `preview.jpg` (512x512). Without one, the zip has no `preview.png`; `release_mods.py` also counts a preview change as a reason to release.
5. `publish/workshop-description.txt` and `publish/workshop-tags.txt`.
6. README.md with Installing/Building (zip name `OvagroOverhead-<version>.zip`).
7. Exactly one `src/*/*.csproj` that is not `*.Tests.csproj` (the scripts require it).
8. `mod.yaml` title and `staticID`; `mod_info.yaml`. These are staged into `publish/content` with the DLL and any `anim/`, `assets/`, `translations/`, `strings/`, `worldgen/`, `templates/`, `elements/` and `codex/` folders. The zip root holds `mod.yaml`, `mod_info.yaml`, the DLL and `preview.png`.
9. Tools on the machine: `gh` authenticated, Steam running for the Workshop step, `common/tools/WorkshopUpload` built with `steam_api64.dll`, Pillow for the preview rescale. The Discord notice needs `~/.config/oni-mods/discord-webhook.txt`; with no webhook it is skipped.

Filled only at or after first upload:
- `publish/workshop-id.txt`: absent for a new mod. `release.py` looks the mod up by `mod.yaml` title among published items. When there is no match ("unlisted") the Workshop step is skipped and a GitHub release is made. The first Workshop upload is a deliberate `WorkshopUpload publish <zip> <preview> --title ... --description-file ... --tags ...` (or Klei's uploader). When a later release finds the item by title, it writes the id file and commits "Record the Workshop item id".
- The git tag `ovagro-overhead-v0.1.0` and the GitHub release (`gh release create`) are made by the script.
- `publish/workshop-description.live.txt` appears only if the live description was edited on the Workshop page; it is gitignored.

## 7. Translations

- Strings are declared in a `ModStrings` class whose nested classes mirror the game's `STRINGS` tree (`ModStrings.MISC.TAGS.X` registers as `STRINGS.MISC.TAGS.X`). Every field is `public static LocString`. Code reads the field when needed, never caching it into a const. Mod-private text goes under its own branch, `ModStrings.<MODNAME>.SIDESCREEN.TITLE`.
- It needs `<UseTranslations>true</UseTranslations>` in the csproj, `using Isochronous;` and `Translations.Register(harmony, typeof(ModStrings))` in `OnLoad` (optional third argument: an `Action` run after each language pass). Compare detect-any-element's `Mod.cs`, `ModStrings.cs`, csproj and `translations/{DetectAnyElement.pot,zh.po,ko.po,ru.po}`. The `translations/` folder deploys with the build and ships in the zip.
- `python common/tools/Translations/translations.py <mod-folder> [--build]` rewrites the `.pot` from the built DLL and checks each `.po`. Run it after changing strings and before a release. po files are written with the Write tool, never heredocs (memory).
- **A mod with no player-visible text needs nothing**: no `ModStrings.cs`, no `UseTranslations`, no `translations/` folder (just-fall-already, dont-count-eggs-on-rails and others). That is the likely case for a mod that only changes where plants can be placed.
- If a visible string is added (a tooltip, a status line), add the full set: `ModStrings.cs`, `UseTranslations`, `.pot`, and the zh/ko/ru `.po` files that the nine translated mods ship. The Workshop description of a translated mod ends with the "Translations done via artificial intelligence..." lines in four languages (see detect-any-element's file).

## 8. Scaffolding scripts or templates

- **None for a new mod.** No template folder, cookiecutter, `new-mod` script or `dotnet new` template exists in the repo or `common/tools`. The convention is to copy an existing mod, as above.
- Related tools that exist: `release_mods.py`/`release.py` (release), `Test-OniMods.ps1` (`common/tools/ModTest`, launches the game with only the dev mods enabled), `Invoke-OniTests.ps1`, `ModCheck` (checks a test DLL loads against the game assemblies), `translations.py`, `kanim.py`, `make_preview.py`/`title_preview.py`, `WorkshopUpload`, `SaveRunner`, `WorldgenDriver`. A new mod local-deploys automatically on its first build. `Test-OniMods.ps1` registers freshly built local mods the game has not seen yet.
- ICON side: `ks integrate` / `ICON:icon-integrate` wires a new project folder into the store.

## 9. Gaps and doc mismatches

- **G1.** `common/README.md` "Usage in a mod repo" still shows the old csproj form (`$(MSBuildThisFileDirectory)..\..\common\oni-mod.props`). Every csproj now uses `$(OniCommon)oni-mod.props`, which `Directory.Build.props` defines. Copy a real csproj, not the README snippet.
- **G2.** Most mod READMEs (just-fall-already, detect-any-element, corner-build-anything, vent-freeze-fix and others) still say "Download ... from [latest release](https://github.com/isochronous/<mod>/releases/latest)" and `git clone ... isochronous/<mod>.git`, the pre-merge per-mod repos. Only jumper-bridges' README uses `isochronous/ONI-Mods/releases?q=jumper-bridges` and `git clone https://github.com/isochronous/ONI-Mods.git` + `dotnet build ONI-Mods/mods/<name>/src/<Mod>`. Use the jumper-bridges form. Workshop descriptions already say "Source and issues: https://github.com/isochronous/ONI-Mods".
- **G3.** `common/README.md` describes `release_mods.py` and `release.py` as working per "mod repo next to this one", and says a mod is due when `publish/preview.png` changes. The code (`SHIPPED = ["src","publish/preview.png","publish/preview.jpg"]`) handles `mods/<name>` folders and jpg. Behaviour is right; the README text lags.
- **G4.** `testbed-base.sav` is documented in `common/tools/TestRunner/README.md` and `fixtures/README.md` but is not in `fixtures/`. Only `testbed-so.sav` exists. The launcher throws if the game's mode is base and no `-Fixture` is given.
- **G5.** Invoke-OniTests.ps1 walks up to a sibling called `oni-mods-common` to find the workspace; in this checkout there is none, so the fallback `..` of `common` (C:\dev\ONI-Mods) is used and mods are found through its `mods/*` folders. It works, but its header text ("workspace holds every mod repo") describes the old layout.
- **G6.** `make_preview.py` says it makes 256x256 previews; `title_preview.py` and all committed previews are 512x512.
- **G7.** Not determined: the exact `mod_info.yaml` syntax for `requiredDlcIds`/`forbiddenDlcIds` (section 4) and whether Ovagro's classes are in the base assembly. Nothing in this repo is an example of either.
- **G8.** `mods/detect-any-element` has both `preview/preview.jpg` (1024x1024 source) and `publish/preview.jpg` (512). Other mods keep only `publish/`. Not needed for a new mod.
- **G9.** The `ks brief` for this scope listed `blk-...` code-style records from 15 different mods (the same two sections repeated per mod, named against `Patches.cs` in detect-any-element). They are copies of one standard. The relevant content is in `blk-568c3162f3c6` (Language and Toolchain) and `blk-568d224900b6` (Harmony Patches), which I read.

## 10. Briefing notes (store pointers)

- Opened and applied: `blk-ab0660690485`, `blk-bcc50465e110`, `blk-568c3162f3c6`, `blk-568d224900b6` (code-style) and `mods/just-fall-already:standards/naming-conventions.md` (found via the folder, not via the brief).
- Not applicable to this task: the detect-any-element `entities.md` records (`blk-dd68a64821ad`, `blk-e07d0d8f7399`, `blk-e4bdbace6e5b`, `blk-f7cd1a1120d9`, `blk-014063748fa2`, `blk-1687ce1b77e2`, `blk-2c3758b48106`, `blk-8499447d8b7f`, `blk-752306474b78`) describe that mod's sensors, not scaffolding. I did not open the common-store tool records (`StringsTemplate`, `ModCheck`, `WorkshopUpload`, `SaveRunner`, `WorldgenDriver`); I read the tools' sources and READMEs directly because the questions were about behaviour the code and README already state.
- Missed by the brief: nothing in `common/` governing "how a new mod is added" appeared (the common README is the nearest thing). The `ks` records for `common/tools/Release` and `TestRunner` (the actual scaffolding and release behaviour) did not surface in the first 8 KB for this scope; the brief only matched `tools/WorkshopUpload`, `ModCheck`, `Translations`, `SaveRunner`, `WorldgenDriver` and `TestRunner:domains/entities.md`.
- The one-line reasons on the `blk-` code-style pointers ("Language and Toolchain", "Harmony Patches") are headings only, and each said it "names mods/detect-any-element/.../Patches.cs", which is how a record for a different mod got in; the content did match the task.

## 11. Friction with ICON tooling

- The handoff's pointers were 36 KB of selection of which the first 8 KB held mostly duplicate per-mod code-style sections (G9), so the pointers that answered this task (mod layout, release, tests) were in the truncated remainder or absent. I answered from the files directly.
- `ks read --store C:/dev/ONI-Mods blk-...` worked on the first try. Passing four ids at once returned all four.
- No hook or skill refusals.

## 12. Store reads

- `ks brief` runs: none. I used the two briefs in the handoff packet and ran no further ones.
- `ks read` runs: one. `node c:/dev/ai/icon/bin/ks.mjs read --store C:/dev/ONI-Mods blk-ab0660690485 blk-bcc50465e110 blk-568c3162f3c6 blk-568d224900b6`.
- Packet pointers read: those four, all in the `C:/dev/ONI-Mods` store. No pointer from the `common` store was read.
- Informed findings: `blk-568c3162f3c6` and `blk-ab0660690485` (Language and Toolchain, identical text) informed the C# 9 / net48 / namespace-block / one-namespace-per-mod conventions in 2.1 and 3.4. `blk-568d224900b6` and `blk-bcc50465e110` (Harmony Patches, identical text) informed the patch-class convention in 2.1.
- Read by file, not by `ks read`: `mods/just-fall-already/.context/standards/naming-conventions.md` (section 2 and 3.5), the `.context` files of just-fall-already and `mods/.context/projects.md` (2.4), and all the build, tool, README and test files cited in the report. This differs from the instruction to use `ks read`.
- Everything else in the packet was not opened (see section 10).
