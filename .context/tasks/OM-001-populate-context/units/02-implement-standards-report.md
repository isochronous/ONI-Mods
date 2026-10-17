# Unit 02 report: standards change-sets for the 17 mod leaves

Output: 17 files `.context/tasks/OM-001-populate-context/changesets/standards-<mod>.json`, each with 3 `create` ops (`standards/code-style.md`, `standards/error-handling.md`, `standards/naming-conventions.md`), `node: "mods/<mod>"`, `task: "OM-001"`, `applies_to: ["src/**"]`, no tags. Nothing applied, nothing committed. Existing `standards/README.md` files untouched.

Generator (scratch, not in the repo): `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/` with `shared.py` (the three shared bodies), `mods_data.py` (per-mod sections) and `build.py` (composes the JSON).

## Shared conventions found

- Toolchain: C# 9, net48, game assemblies as references, optional `ReferenceUnityUI` / `UsePLib` (ILRepack-merged) / `UseTranslations` switches: `common/oni-mod.props`, `common/oni-mod.targets`. No `.editorconfig` exists.
- Formatting: tabs in `.cs`, two spaces in project files; Allman braces; single short statement bodies braceless on the next line; trailing commas in multi-line initialisers; no line limit (`mods/bitshifter/src/Bitshifter/Mod.cs`, `mods/jumper-bridges/src/JumperBridges/Mod.cs`). Files are LF in the index and CRLF in the working tree (`core.autocrlf` true).
- Usings: above the namespace, alphabetical with `System.*` first, aliases for `ModStrings` branches last (`mods/salty-surf/src/SaltySurf/Mod.cs`, `mods/sweep-zones/src/SweepZones/Mod.cs`). `Debug` is either `UnityEngine.Debug` or the game's global class; `mods/just-fall-already/.../FallPoke.cs` qualifies it because of `System.Diagnostics`.
- Strings: `+` concatenation, `string.Format` on `LocString`; no interpolation in mod code. Text lives in `ModStrings`, mirroring `STRINGS`, UPPER_SNAKE (`common/src/Translations.cs`, `mods/smart-weight-plate/.../ModStrings.cs`).
- Harmony: static patch classes named `<Type>_<Method>_Patch`, `Prefix`/`Postfix`/`Transpiler`/`Finalizer`, `Prepare()` for option-gated patches, `TargetMethods()` for multi-target; `base.OnLoad` applies patches, Bitshifter patches from `OnAllModsLoaded` (`mods/bitshifter/.../Mod.cs`, `mods/jumper-bridges/.../Patches.cs`, `mods/detect-any-element/.../AnyGlyph.cs`).
- Components: `KMonoBehaviour`, `[MyCmpGet]`, `[Serialize]` with OptIn (`mods/no-magic-fridges/.../FridgeThermostat.cs`, `mods/sweep-zones/.../ZoneStore.cs`).
- Options: PLib `POptions`, `[JsonObject(OptIn)]`, `[ConfigFile(SharedConfigLocation: true)]`, key strings `STRINGS.<MOD>.OPTIONS.*` (`mods/vent-freeze-fix/.../Options.cs`, `mods/storage-pod-redux/.../Options.cs`).
- Error handling: warn-and-leave-vanilla on missing reflection member, transpiler match count, missing tech/prefab/art; try/catch only for other-mod integration, file/JSON IO, worldgen/per-cell patches and per-job queue work; Finalizer for the unguarded worldgen thread; yield-to-other-mod logic; `[<Name>] ` log prefix; levels Log/LogWarning/LogError; `PUtil.Log*` unused; telemetry gated by a debug option (`mods/no-magic-fridges/.../Patches.cs`, `mods/corner-build-anything/.../ReachTablePatches.cs`, `mods/natural-backwalls-everywhere/.../WorldgenThreadPatch.cs`, `mods/bitshifter/.../Mod.cs`, `mods/vent-freeze-fix/.../VentFreezeWatcher.cs`). One silent catch exists (`GuessLanguageCode` in `common/src/Translations.cs`) and is documented as the exception.
- Naming: folders kebab-case, projects/namespaces PascalCase (`NaturalBackwalls` differs from its folder), `Mod.cs` / `Patches.cs` / `ModStrings.cs` / `Options.cs` / `<Building>Config.cs`, `[OniTest("<Mod>: ...")]` tests in `<Name>.Tests`, `staticID` `Isochronous.<Name>`, camelCase private fields and mutable statics, PascalCase constants and constant-like `static readonly`.

## Per-mod differences captured (each mod's "<Title> specifics" section, in all three docs)

bitshifter: OnAllModsLoaded patching, WhyYield, `Display`, no PLib. corner-build-anything: nested transpiler classes without `_Patch`, `SwapTable` match count. detect-any-element: `ModStrings.MISC.TAGS`, afterApply tag name, `TargetMethods`, ConditionalWeakTable. dont-count-eggs-on-rails: single-file mod, prefix returns true/false. jumper-bridges: `LogPrefix`, options before `base.OnLoad`, `Prepare`, `BridgeKind`. just-fall-already: `FallPoke` static state, `UnityEngine.Debug`, telemetry. motion-sensor-range: `RangeSlider`, warn-once. multichannel-critter-sensor: PLib UI screen, `ThresholdAdapter`, `NoUnits`. natural-backwalls-everywhere: folder/namespace mismatch, worldgen try/catch, Finalizer, settle-sim transpiler, `unsafe`, `DeployWorldgen`, BiomePreview tool. no-magic-fridges: reflection guards, thermostat component. refined-building: single idempotent postfix, `var`. salty-surf: patches in `Mod.cs`, `SurfWater`, explicit interface `TargetMethod`. smart-weight-plate: compat class, `TechId`, range screen fix. storage-pod-redux: `SingletonOptions`, `Buildings.Decide`, `ModDetection`, reused ids. sweep-zones: `ZoneStore`, donor-field tolerance, icon fallback. upgrade-planner: subfolders under one namespace, `InternalsVisibleTo`, vendored PLib, Compat try/catch, `LogError` in queue. vent-freeze-fix: `[Serialize]` snapshot, Verbose-gated telemetry, Developer options.

## TODO(owner) left

- One, in every `code-style.md`, under Line Length: decide whether to add an `.editorconfig` recording tabs and brace placement.

## write plan summary (every file: exit 0, accepted)

Command per file, from `C:/dev/ONI-Mods`: `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at .`

```
standards-bitshifter.json                   exit0 created=3
standards-corner-build-anything.json        exit0 created=3
standards-detect-any-element.json           exit0 created=3
standards-dont-count-eggs-on-rails.json     exit0 created=3
standards-jumper-bridges.json               exit0 created=3
standards-just-fall-already.json            exit0 created=3
standards-motion-sensor-range.json          exit0 created=3
standards-multichannel-critter-sensor.json  exit0 created=3
standards-natural-backwalls-everywhere.json exit0 created=3
standards-no-magic-fridges.json             exit0 created=3
standards-refined-building.json             exit0 created=3
standards-salty-surf.json                   exit0 created=3
standards-smart-weight-plate.json           exit0 created=3
standards-storage-pod-redux.json            exit0 created=3
standards-sweep-zones.json                  exit0 created=3
standards-upgrade-planner.json              exit0 created=3
standards-vent-freeze-fix.json              exit0 created=3
```

Output shape for each (bitshifter shown):

```
task   OM-001
node    C:/dev/ONI-Mods/mods/bitshifter [aggregator]
what it writes
  create  .context/standards/code-style.md
  create  .context/standards/error-handling.md
  create  .context/standards/naming-conventions.md
what the writers report
  narrative  standards/code-style.md created
  narrative  standards/error-handling.md created
  narrative  standards/naming-conventions.md created
plan only — nothing was written.
```

## Friction

- None from ICON's engine or templates. Notes: the templates are web/TypeScript-shaped (imports example, REST/service layers, anti-pattern emojis), so most sections were rewritten rather than adapted; the plan output marks mod nodes as `[aggregator]` while still accepting writes.
- Environment: the session's auto-mode instruction to prefer Bash for file writes conflicts with the memory rule to write scripts with the Write tool; the Write tool was used for the generator scripts.
