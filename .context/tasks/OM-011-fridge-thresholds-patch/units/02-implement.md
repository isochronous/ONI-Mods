Task: OM-011 — Fridge Thresholds Extended (new mod)
Lens: implement. Work on branch OM-011-fridge-thresholds-patch in C:\dev\ONI-Mods. Do not commit; do not launch the game; do not touch other mods or common/.

Design: read C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\plan.md (Decisions + the investigation handoff block) and units\01-research-report.md. Decompiles referenced there are in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad.

Build:
1. New mod at mods/fridge-thresholds-extended, copying the skeleton of mods/vent-freeze-fix (csproj importing $(OniCommon) props/targets, mod.yaml with staticID "Isochronous.FridgeThresholdsExtended", mod_info.yaml, Mod.cs, ModStrings.cs if the pattern requires it, README.md, CHANGELOG.md with a 0.1.0 entry). Version 0.1.0. Add the project to ONI-Mods.sln the way the others are. Skip PLib options (no settings). Only add translations/ if the skeleton requires it; there are no player-visible strings beyond mod.yaml.
2. Compat class applied from UserMod2.OnAllModsLoaded, shaped like mods/smart-weight-plate/src/SmartWeightPlate/Mod.cs:22-58: resolve thresholds types with AccessTools.TypeByName; if missing, log once and return. Group A (IR fridges): for the five DupesRefrigeration config classes, prefix DoPostConfigureComplete(GameObject go) adding RefrigeratorThresholds when absent. Group B (Freezer): prefix Psyko.Freezer.Freezer.UpdateLogicCircuit as described in the report (skip if a prefix owned by a harmony id containing "llunak" already exists), plus OnCopySettings postfix copying SendGreenOnLow/ActivateValue/DeactivateValue from the source object's thresholds component (check the thresholds source C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs:328-344 for the exact members). Try/catch per group, warn on any missing member and leave that target alone. No compile-time reference to any of the three mods; no foreign type in any patch signature or field type. Bind reflection once.
3. README: follow the store's Workshop writing style (short, player-facing: what it does, compatibility; requires Storage Refrigerator Thresholds; works with Freezer and Ronivan's Legacy - Industrial Revolution). No hedging.
4. Match the surrounding code's naming, comment density and idioms.

Verify: `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` must succeed with no new warnings from the new project. Copy the relevant output tail into the report.
Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\02-implement-report.md — files created/changed, build output, anything that deviates from the design and why, and what still needs an in-game check.
Model tier: complex
