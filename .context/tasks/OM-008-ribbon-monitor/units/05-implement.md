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
