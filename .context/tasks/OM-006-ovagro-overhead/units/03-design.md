# Unit 03 — design: how Ovagro Overhead makes Ovagro grow from ceilings

## Change proposed
A new mod, mods/ovagro-overhead (OvagroOverhead), whose only feature is: an Ovagro Node (`VineMother`, seed `VineMotherSeed`) can be planted on a ceiling — by Pips in natural tiles, and by Duplicants in downward-facing (FlipV) Hydroponic Farms and Farm Tiles — and its vines (`VineBranch`) then grow along the ceiling's underside the way they grow along a floor. Floor planting stays exactly as in vanilla.

## Context the packet does not carry
- Read plan.md (`## Phase Handoff Log`, investigation block) and both explore reports: units/01-explore-report.md (game code, build 744825, with file/line references) and units/02-explore-report.md (repo scaffolding pattern).
- Decompiled game source: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\cc565cd0-8a2e-40a2-b730-be890f2c3a64\scratchpad\decomp\ (Assembly-CSharp, one .cs per type) and ...\scratchpad\fp\ (firstpass). Read the game code to confirm or overturn unit 01 wherever your design rests on it.
- The kanim art can be inspected if it matters (the game's anim bundles); state whether you did.
- Do not launch the game.

## Specific questions
1. One seed, two orientations: a second hanging prefab (plant + preview, maybe its own seed or the vanilla seed redirected) chosen at planting time, versus per-instance orientation on the vanilla prefab. For each: what the player sees (seed item, codex, planter side screen, tooltip "needs ceiling"), save/load behaviour (and what happens if the mod is removed from a save holding a ceiling Ovagro), branch seed drops, wild world-gen nodes, and how much game code each patches.
2. Pips: the Pip walks `NavType.Floor` and plants through `PlantableCellQuery` / `SeedPlantingStates` / `PlantableSeed.TestSuitableGround`, all keyed on `seed.direction`. Can a Pip plant a 1x2 hanging plant on a ceiling at all, and how does vanilla Dasha Saltvine (SaltPlant, 1x2 hanging) get wild-planted by Pips, if it does? What must change so Pips choose a ceiling cell for Ovagro, and does the Pip's choice between floor and ceiling need a rule?
3. Downward farm plots: how the plot spawns the ceiling form (the plot currently spawns the upright prefab one cell below), the planter side screen preview validity (`EntityPreview`), and the hydroponic irrigation path.
4. Vines: the first branch's `RootShape` (VineBranch.cs 466) and anything else on the branch chain that needs the mother's orientation; confirm the shape/anim logic is symmetric as unit 01 says.
5. Mother art: FlipY on `vine_mother_kanim` (with offset/pivot) versus anything else; how the grow/wilt/harvest anims and the plant's status icons look flipped.
6. Compatibility: which game methods the design patches, whether other mods are likely to patch the same ones, and DLC4 gating (`requiredDlcIds` in mod_info.yaml — confirm the field name and value from the game's mod loader code).
7. Testability: what in-game [OniTest] tests (TestRunner kit, unit 02's template mods/just-fall-already Doors.cs) can prove: wild ceiling planting by a Pip (or its validity query, if a Pip run is too slow), hydroponic and farm tile ceiling planting, branches growing along the ceiling, and a save/load round trip.

## Constraints
- Only feature is ceiling planting for Ovagro; nothing else (user).
- Vines must grow as expected when planted on the ceiling (user).
- Farm Tiles and Hydroponic Farms both rotate FlipV in this build, so both are in scope.
- Follow the repo's existing mod patterns (unit 02); do not introduce patterns the repo has not established without saying so.
- Where a question has more than one defensible answer, present the options with your pick and why; the user decides.

Ticket: OM-006

## Report
- File: .context/tasks/OM-006-ovagro-overhead/units/03-design-report.md
- Must contain: a recommendation (Approve / Approve with modifications / Reject of the change as proposed) and the recommended design, rationale, the alternatives rejected and why, the list of game methods patched, risks, the open decisions for the user (each with options and your pick), and a "Store reads" section listing every ks brief and ks read run and which pointers informed the design — written so it can land as a decision record.
- Model tier: complex
