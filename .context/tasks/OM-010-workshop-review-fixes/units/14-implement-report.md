# Unit 14 report

## Changes
- Upgrade Planner (UpgradeTool.cs): new field `colorsWorld`; `GetOverlayColorData` reads `activeWorldId` once, includes it in the cache check, stores it on refresh.
- Vent Freeze Fix: "spawns" replaced by "is built or a save is loaded" in README, publish/workshop-description.txt, mod.yaml, ModStrings.cs tooltip, and msgid in .pot/ko/ru/zh (msgstr unchanged, meaning is the same). README: "It looks at" now "The watcher looks at". CHANGELOG: Unreleased bullet added.
- Salty Surf: Mod.cs comment now says a hidden tag writes nothing, the visible tag before it writes the separator unless last. Test PropertiesLine checks every description line for a trailing "," (no prefix lookup).
- Just Fall Already: CHANGELOG bullet split into two.

## Output (verbatim tails)
translations.py mods/vent-freeze-fix --build:
Wrote 13 strings to ...\VentFreezeFix.pot
ko.po: 13/13 translated
ru.po: 13/13 translated
zh.po: 13/13 translated
ALL CHECKS PASSED

dotnet build VentFreezeFix.csproj: Build succeeded. 0 Warning(s) 0 Error(s)
dotnet build SaltySurf.Tests.csproj: 1 Warning(s) (CS0618 FindObjectsOfType in Persistence.cs, pre-existing) 0 Error(s)
dotnet build UpgradePlanner.csproj: Build succeeded. 0 Warning(s) 0 Error(s)
Just Fall Already: changelog only, no build.
