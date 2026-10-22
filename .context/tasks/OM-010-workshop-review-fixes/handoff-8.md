# Handoff — OM-010-workshop-review-fixes

**Scope:** `mods/salty-surf`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-010-workshop-review-fixes/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-010-workshop-review-fixes/units/08-implement.md`

```
# Unit 08 — implement: Salty Surf tag in the Properties line
Ticket OM-010. Read units/04-explore-salty-surf.md. Hide the SaltySurfWater tag from Water's and Salt Water's Properties line: add it to GameTags.HiddenElementTags and place it first in each element's tag list so no trailing separator is left (verify against the decompile what the Properties line builder does). Behaviour unchanged. Add ONE [OniTest] (in Intake.cs or alongside) asserting the Properties/description text for Water has no MISSING string and still contains the tag-bearing behaviour the mod relies on. CHANGELOG bullet.

Acceptance criteria:
  - Build only your own projects, e.g. `dotnet build mods/salty-surf/src/<Project>/<Project>.csproj -c Release -p:ModDeployFolder=none` and the Tests project if you changed it (other units build other mods at the same time; do not build the whole solution). Show the output tail.
  - Do NOT launch the game. Do NOT commit or stage anything: the manager commits. Touch only mods/salty-surf/ files.
  - Player-visible text: short, plain, no hedging (Workshop writing style). CHANGELOG: one short plain bullet under an Unreleased heading, following the file's pattern.

Report: per-file change summary, new/changed test names, build output verbatim.
- File: .context/tasks/OM-010-workshop-review-fixes/units/08-implement-report.md
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-0b8e3ef08773 · reference
  Example
  mods/vent-freeze-fix/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-0fe521f2b16f · reference
  Example
  mods/upgrade-planner/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-32ae0ce82848 · reference
  Example
  mods/bitshifter/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-353f3e62bd46 · reference
  Example
  mods/multichannel-critter-sensor/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-431be121fc8c · reference
  Example
  mods/corner-build-anything/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-5379b7234d12 · reference
  Example
  mods/motion-sensor-range/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-5b62e7507604 · reference
  Example
  mods/just-fall-already/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-6a8bea597750 · reference
  Example
  mods/detect-any-element/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-7261503b82df · reference
  Example
  mods/storage-pod-redux/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-8ce83687722f · reference
  Example
  mods/sweep-zones/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-94c9c06009e9 · reference
  Example
  mods/refined-building/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-b243414742bb · reference
  Example
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-cb4940771b30 · reference
  Example
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-cd84394283fa · reference
  Example
  mods/no-magic-fridges/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-e155218a57bf · reference
  Example
  mods/salty-surf/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-ea3b63724204 · reference
  Example
  mods/smart-weight-plate/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-f3f1f9eab24b · reference
  Example
  mods/jumper-bridges/.context/standards/code-style.md#imports-includes/example · names mods/salty-surf/src/SaltySurf/Mod.cs → mods/salty-surf
blk-00fe6a155298 · reference
  Examples by Type
  mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-01b48de8fa57 · reference
  Examples by Type
  mods/motion-sensor-range/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-0b089ce1d390 · reference
  Examples by Type
  mods/smart-weight-plate/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-0fc5ffaf284d · reference
  Examples by Type
  mods/refined-building/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-19a06aa47b06 · reference
  Examples by Type
  mods/upgrade-planner/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-2963fe01febc · reference
  Examples by Type
  mods/salty-surf/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-44860daaca69 · reference
  Examples by Type
  mods/sweep-zones/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-5157bbf172ab · reference
  Examples by Type
  mods/storage-pod-redux/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-701ebf962f66 · reference
  Examples by Type
  mods/detect-any-element/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-7ebeb4864e2b · reference
  Examples by Type
  mods/vent-freeze-fix/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-9b2fe98dd4dd · reference
  Examples by Type
  mods/just-fall-already/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-a8ba49e2c00b · reference
  Examples by Type
  mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-bce469611e0a · reference
  Examples by Type
  mods/jumper-bridges/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-c63d3276e032 · reference
  Examples by Type
  mods/no-magic-fridges/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-c963897629f3 · reference
  Examples by Type
  mods/corner-build-anything/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-cdec4d90d437 · reference
  Examples by Type
  mods/multichannel-critter-sensor/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
blk-dfc2718274e5 · reference
  Examples by Type
  mods/bitshifter/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/salty-surf/src/SaltySurf/ModStrings.cs → mods/salty-surf
mods/salty-surf:domains/entities.md · reference
  The SurfWater tag helper, Harmony patches, mod strings and translations of Salty Surf in src/Sa…
  mods/salty-surf/.context/domains/entities.md · src/SaltySurf/** → mods/salty-surf · linked from domains/glossary.md
mods/salty-surf:domains/glossary.md · reference
  Glossary for Salty Surf: abbreviations, terms to avoid, and definitions of the Mechanical Surfb…
  mods/salty-surf/.context/domains/glossary.md · src/SaltySurf/** → mods/salty-surf · linked from domains/entities.md
mods/salty-surf:standards/code-style.md · reference
  C# code style for Salty Surf: indentation, braces, imports, comments, control structures, Harmo…
  mods/salty-surf/.context/standards/code-style.md · src/** → mods/salty-surf · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 2 further candidate(s) matched and are not listed above (7955 of 8192 bytes; the whole selection is 8617). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/salty-surf --budget 8617

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/salty-surf:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
