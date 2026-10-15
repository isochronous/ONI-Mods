# ONI Mods

[Oxygen Not Included](https://www.klei.com/games/oxygen-not-included) mods by isochronous, one folder each under `mods/`. Each mod's own README says what it does and how to install it; releases are tagged `<mod>-v<version>` and listed on this repo's Releases page.

| Mod | Folder |
|---|---|
| Bitshifter | [mods/bitshifter](mods/bitshifter) |
| Corner Build Anything | [mods/corner-build-anything](mods/corner-build-anything) |
| Detect Any Element | [mods/detect-any-element](mods/detect-any-element) |
| Direct Automation Bridges | [mods/direct-automation-bridges](mods/direct-automation-bridges) |
| Don't Count Eggs on Rails | [mods/dont-count-eggs-on-rails](mods/dont-count-eggs-on-rails) |
| Just Fall Already! | [mods/just-fall-already](mods/just-fall-already) |
| Motion Sensor Range | [mods/motion-sensor-range](mods/motion-sensor-range) |
| Multichannel Critter Sensor | [mods/multichannel-critter-sensor](mods/multichannel-critter-sensor) |
| Natural Backwalls Everywhere | [mods/natural-backwalls-everywhere](mods/natural-backwalls-everywhere) |
| No Magic Fridges | [mods/no-magic-fridges](mods/no-magic-fridges) |
| Refined Building | [mods/refined-building](mods/refined-building) |
| Salty Surf | [mods/salty-surf](mods/salty-surf) |
| Smart Weight Plate | [mods/smart-weight-plate](mods/smart-weight-plate) |
| Storage Pod Redux | [mods/storage-pod-redux](mods/storage-pod-redux) |
| Sweep Zones Redux | [mods/sweep-zones](mods/sweep-zones) |
| Upgrade Planner | [mods/upgrade-planner](mods/upgrade-planner) |
| Vent Freeze Fix | [mods/vent-freeze-fix](mods/vent-freeze-fix) |

## Building

```
git clone --recurse-submodules https://github.com/isochronous/oni-mods.git
dotnet build oni-mods/ONI-Mods.sln -c Release
```

`common/` is [oni-mods-common](https://github.com/isochronous/oni-mods-common), the shared build, test harness and release tools, as a submodule; `Directory.Build.props` points every project at it as `$(OniCommon)`. Building a mod deploys it into the game's local mods folder; add `-p:ModDeployFolder=none` to build without deploying. See `common/README.md` for the build, test and release tooling.

The mods were separate repositories until October 2026; each one's history was brought here with it.
