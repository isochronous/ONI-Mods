Topic: Workshop mods Storage Refrigerator Thresholds (llunak, 2959340381), Freezer (Psyko, 2618339179), Ronivan's Industrial Revolution (find its Workshop id); Harmony patching across optional mods
Game build: whatever is installed (F:\SteamLibrary\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll)
Ticket: OM-011
Lens: research (read-only). Do not edit repo files except your report. Do not launch the game.

Background (verified earlier this session):
- Storage Refrigerator Thresholds source: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs. It adds a `RefrigeratorThresholds` component (IActivationRangeTarget) and a Harmony prefix on vanilla `Refrigerator.UpdateLogicCircuit` that calls `UpdateLogicState(stored/capacity)`. `OtherMods_Patch` (385-412) adds the component to Psyko's FreezerConfig, but nothing patches `Psyko.Freezer.Freezer.UpdateLogicCircuit` (it copies Refrigerator and sends IsFull & IsOperational). Its UpdateLogicCircuit falls back to calling the modded building's own method by reflection (304-313).
- Decompiled Freezer and thresholds DLL already exist in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\freezer-src and ...\scratchpad\srt-src.

Questions:
1. Which Workshop mod is "Ronivan's Industrial Revolution" (Ronivan publishes several; look in C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\*\mod.yaml and mod_info.yaml, and the mods.json in C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods). Is it installed? Which of its buildings are fridges/cold storage (refrigerator-like: FilteredStorage + logic port)? Give config class names, building IDs, and the component class each uses (vanilla Refrigerator, a copy, or something else) with its UpdateLogicCircuit equivalent. Decompile with ilspycmd into the scratchpad.
2. Does the thresholds mod already list any Industrial Revolution buildings in OtherMods_Patch or elsewhere?
3. For each target building, what exact patch makes the sliders drive the port: what to prefix, how to reach `RefrigeratorThresholds` and `UpdateLogicState` (public? internal? need reflection/AccessTools?), how stored fraction is computed so it matches the vanilla-Refrigerator path in the thresholds mod. If a building doesn't get the component at all, what must the patch add (and does the side screen then work)?
4. How should a patch mod handle any of the three mods being absent: load order (ONI mod load order / mod_info requirements), resolving types by name at runtime (AccessTools.TypeByName) and skipping patches when missing, and patching after the other mods have loaded (e.g. in OnAllModsLoaded / Db.Initialize postfix).
5. oni-moono caveat: Jeremy runs on oni-moono (clone at scratchpad\oni-moono). Note anything in its docs (ARCHITECTURE.md D13/D19/D21 on inlining) that the patch must respect, e.g. callers of the patched method that could inline it.
6. How existing mods in this repo are structured and built (look at one small one, e.g. mods/vent-freeze-fix: csproj, mod.yaml, mod_info.yaml, Directory.Build.props, common/), so the plan can copy the pattern. Note whether any existing mod here already patches another Workshop mod's types optionally.

Decision this research will inform: the patch design and the new mod's skeleton.
Constraints: patch only at runtime; never copy other authors' code into the repo; must not error when a target mod is missing.
Report: write to C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\01-research-report.md — findings with file:line citations, versions checked, a concrete recommended patch per building, and what you verified vs inferred. Keep it under ~150 lines.
Model tier: complex
