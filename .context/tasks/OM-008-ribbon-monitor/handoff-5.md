# Handoff — OM-008-ribbon-monitor

**Scope:** `mods/bitshifter/tools/make_art.py`, `mods/bitshifter/anim-original/diagnostic_panel`, `mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel`, `mods/bitshifter/src/Bitshifter/Toolkit`
**Store:** `C:/dev/ONI-Mods-OM-008`
**Task scratch:** `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-008-ribbon-monitor/units/05-implement.md`

```
# Unit 05 — implement: 3GuB's final diagnostic panel art

Lens: implement. Worktree `C:\dev\ONI-Mods-OM-008`, branch `OM-008-ribbon-monitor`. Work only in this worktree.

## What

3GuB delivered the final Ribbon Diagnostic Panel art, including the build menu icon (the `ui` symbol/anim). The user copied the export over the files in `mods/bitshifter/anim-original/diagnostic_panel/` (`diagnostic_panel_0.png`, `diagnostic_panel_build.bytes`, `diagnostic_panel_anim.bytes`; git shows png and build.bytes modified). Bring it into the shipped kanim `mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel/` the way the previous drops went in: through `mods/bitshifter/tools/make_art.py` (read it first; the history is commits 57b43b8, 4291edb "lamps bit_1 to bit_31 without bloom", 3db9daf "panel lamps cut at the export's half-texel UVs").

## Steps

1. Inspect the new export: symbols, frames, anims, atlas size, whether lamp symbols are still `bit_1..bit_31` (or renamed), and what the `ui` symbol now holds. Compare with the current shipped kanim and with what `RibbonDiagnosticPanel.cs` / `RibbonDiagnosticPanelConfig.cs` reference (symbol names for SetSymbolVisiblity/SetSymbolTint, anim names off/on/place/ui).
2. Run make_art.py to regenerate the shipped kanim from the new export. The build menu icon is the artist's final: use the export's `ui` art as delivered; do not repaint it or substitute the scaled-render-plus-shadow generated icon. If make_art.py currently generates the icon itself, change it to take the artist's `ui` instead. Keep any processing the script does that the new export still needs (half-texel UV lamp cuts, bloom handling); drop steps the new export makes unnecessary. Never edit the files in `anim-original/` (they are the artist's originals).
3. If symbol names changed, update the C# references so lamps still show and tint per bit.
4. Build: `dotnet build mods/bitshifter/src/Bitshifter -c Release` must give 0 errors. Run `make_art.py --check` if it exists. Do NOT build or run Bitshifter.Tests and do NOT launch the game: the worktree's `common` is at 7687b3c (the TestRunner kit methods the panel tests use were lost and are not there), so the test project will not compile. Leave the `common` submodule pointer alone and do not commit it.
5. Render a preview PNG of the new shipped kanim (off, on with all lamps lit, ui icon) to `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\1426260b-1879-4167-b007-f5932c35d320\scratchpad\panel-art\` so the coordinator can show the user. Use the repo's kanim tooling (common/tools Kanim, make_art.py) if it can render; otherwise compose from the atlas + build data in Python.
6. Commit in the worktree, one terse line per commit, no explanatory body: the artist's export in `anim-original/` plus the regenerated `anim/assets/` (and any make_art.py / C# change) — e.g. "OM-008: 3GuB's final diagnostic panel art and build icon". End the commit message with:
   Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
   Do not push.

## Report

Write `units/05-implement-report.md`: what the export contains vs the previous one, what make_art.py now does, any C# change, build output (errors/warnings counts), the preview paths, commit hash(es), and anything about the art that looks wrong (e.g. the earlier defects: digit under lamp 31 reading 32, label 29 offset, spare cell mark) — report, don't fix the artist's drawing.
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

mods/bitshifter:domains/entities.md · reference
  The classes and Harmony patches of the Bitshifter mod (src/Bitshifter): entry point and compati…
  mods/bitshifter/.context/domains/entities.md · src/Bitshifter/** → mods/bitshifter/src/Bitshifter/Toolkit · src/Bitshifter/** → mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel · linked from domains/glossary.md
mods/bitshifter:domains/glossary.md · reference
  Glossary for the Bitshifter mod: abbreviations such as BAO and ONICPU, terms to avoid like Bit…
  mods/bitshifter/.context/domains/glossary.md · src/Bitshifter/** → mods/bitshifter/src/Bitshifter/Toolkit · src/Bitshifter/** → mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel · linked from domains/entities.md
mods/bitshifter:standards/code-style.md · reference
  C# code style for the Bitshifter mod: formatting, toolchain, imports, comments, Harmony patch a…
  mods/bitshifter/.context/standards/code-style.md · src/** → mods/bitshifter/src/Bitshifter/Toolkit · src/** → mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel · linked from standards/error-handling.md · linked from standards/naming-conventions.md
mods/bitshifter:standards/error-handling.md · reference
  Error handling conventions for the Bitshifter mod: guarding Harmony patches and transpilers, tr…
  mods/bitshifter/.context/standards/error-handling.md · src/** → mods/bitshifter/src/Bitshifter/Toolkit · src/** → mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel · linked from standards/code-style.md · linked from standards/naming-conventions.md
mods/bitshifter:standards/naming-conventions.md · reference
  Naming conventions for the Bitshifter mod: files, namespaces, variables, methods, strings and i…
  mods/bitshifter/.context/standards/naming-conventions.md · src/** → mods/bitshifter/src/Bitshifter/Toolkit · src/** → mods/bitshifter/src/Bitshifter/anim/assets/diagnostic_panel · linked from standards/code-style.md · linked from standards/error-handling.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods-OM-008

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/bitshifter:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

3 checkpoints, set by the coordinator for THIS work. There is no universal number: the bound is sized to the work.

Write each one directly — task scratch is exempt from the write path, so no change-set, no dispatch:

- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-5-1.md`
- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-5-2.md`
- `C:/dev/ONI-Mods-OM-008/.context/tasks/OM-008-ribbon-monitor/checkpoint-5-3.md`

This is where checkpoints go, whatever else a spec narrows for the lane's other writes.

A checkpoint says what is done, what is not, and what you would do next. Stop at the bound and report.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
