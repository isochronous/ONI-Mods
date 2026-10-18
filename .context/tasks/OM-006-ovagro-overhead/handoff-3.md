# Handoff — OM-006-ovagro-overhead

**Scope:** `mods/just-fall-already/src`, `.github/workflows/label-mod.yml`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-006-ovagro-overhead/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-006-ovagro-overhead/units/03-design.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-9d52ccfdb3df · reference
  Key Code Paths
  mods/just-fall-already/.context/domains/entities.md#key-code-paths · names mods/just-fall-already/src/JustFallAlready.Tests/Doors.cs, mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-0b8e3ef08773 · reference
  Example
  mods/vent-freeze-fix/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-0fe521f2b16f · reference
  Example
  mods/upgrade-planner/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-16cd4984fdb3 · reference
  Loops
  mods/motion-sensor-range/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-1b40623f402c · reference
  Loops
  mods/vent-freeze-fix/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-29fb02c618a8 · reference
  Loops
  mods/smart-weight-plate/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-2aba30d1f782 · reference
  Variables
  mods/corner-build-anything/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-308a838456d3 · reference
  Loops
  mods/storage-pod-redux/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-32ae0ce82848 · reference
  Example
  mods/bitshifter/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-353f3e62bd46 · reference
  Example
  mods/multichannel-critter-sensor/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-396cf026d230 · reference
  Variables
  mods/motion-sensor-range/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-3a88acdbb150 · reference
  Variables
  mods/upgrade-planner/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-431be121fc8c · reference
  Example
  mods/corner-build-anything/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-47fe26d5347a · reference
  Variables
  mods/salty-surf/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-483b9df9284d · reference
  Loops
  mods/multichannel-critter-sensor/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-486b038b51f1 · reference
  Variables
  mods/storage-pod-redux/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-5379b7234d12 · reference
  Example
  mods/motion-sensor-range/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-5b62e7507604 · reference
  Example
  mods/just-fall-already/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6958eb513dec · reference
  Loops
  mods/sweep-zones/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-699b426eb8f6 · reference
  Variables
  mods/sweep-zones/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6a8bea597750 · reference
  Example
  mods/detect-any-element/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-6b9561f02621 · reference
  Loops
  mods/refined-building/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7261503b82df · reference
  Example
  mods/storage-pod-redux/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-74082ad9f366 · reference
  Variables
  mods/vent-freeze-fix/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-76e79f89b3c4 · reference
  Variables
  mods/just-fall-already/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7bf2c80b2f8e · reference
  Loops
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7d0c65d332f3 · reference
  Static class with the whole mod. A postfix on `Pathfinding.AddNavGrid` subscribes to each navig…
  mods/just-fall-already/.context/domains/entities.md#key-entities/fallpoke · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-7e5838a4ab04 · reference
  Loops
  mods/no-magic-fridges/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-8ce83687722f · reference
  Example
  mods/sweep-zones/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-8f46b5b1e7a3 · reference
  Loops
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-94c9c06009e9 · reference
  Example
  mods/refined-building/.context/standards/code-style.md#imports-includes/example · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-96641cb455da · reference
  Variables
  mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9998cf9f18c6 · reference
  Variables
  mods/multichannel-critter-sensor/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9bc5907f3229 · reference
  Variables
  mods/bitshifter/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-9d32bd324e3e · reference
  Variables
  mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md#code-naming/variables · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
blk-a138faf04479 · reference
  Loops
  mods/detect-any-element/.context/standards/code-style.md#control-structures/loops · names mods/just-fall-already/src/JustFallAlready/FallPoke.cs → mods/just-fall-already/src
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 28 further candidate(s) matched and are not listed above (8077 of 8192 bytes; the whole selection is 14914). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for .github/workflows/label-mod.yml mods/just-fall-already/src --decisions --budget 14914

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/just-fall-already:tags.md, tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
