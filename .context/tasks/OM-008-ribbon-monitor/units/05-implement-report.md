# Unit 05 report: 3GuB's final diagnostic panel art

Commit: 4098d77 "OM-008: 3GuB's final diagnostic panel art and build icon" (export in anim-original, regenerated anim/assets, make_art.py). Not pushed; common pointer untouched.

## The export vs the previous one

- Symbols: bit_1..bit_31 (44x46 each, names unchanged), body 426x426, place 426x426, numbers 730x650, ui 238x242. The TileSize helper symbol is gone. ui now carries a pivot of (0, -121).
- Anims: off, on, place, ui, one frame each; anim.bytes is byte-identical to the previous export. Every frame's bounds are NaN, as before.
- Atlas 1024x512. No bloom flags on any symbol (lamps stay without bloom).
- ui is the artist's finished icon: the panel with all 31 lamps lit red and their numbers, over a soft shadow.
- The earlier defects are fixed in this drawing: the digit under lamp 31 reads 31, label 29 sits in line, no spare cell mark.

## make_art.py

build_panel no longer repacks the panel. It copies the export's png and build.bytes byte for byte (the artist's ui, UVs and packing as delivered) and rewrites only anim.bytes, setting each frame's bounds to the box around its sprites (off/on/place: centre -0,-200, 426x426; ui: centre 0,-117, 238x242). Dropped: the generated scaled-render-plus-shadow icon, the 1024 repack and the half-texel inset cut. The inset cut is not needed when the artist's UVs are kept, and it could not run anyway: `read_kanim(inset=True)` lived in common 549ff6f, which is gone from the object store; the worktree's common (7687b3c) has no inset parameter. The reader/writer part is unchanged.

`python tools/make_art.py` regenerated the kanims (reader/writer unchanged); `python tools/make_art.py --check`: "the committed kanims match a fresh build".

## C#

No change: RibbonDiagnosticPanel uses bit_1..bit_31 and plays "on", both still in the export.

## Build

`dotnet build mods/bitshifter/src/Bitshifter -c Release`: Build succeeded, 0 warnings, 0 errors. Test project not built, game not launched.

## Preview

Dropped by the coordinator (the user checks in game). I looked at the atlas only, to check the drawing.

## Art notes

Nothing looks wrong. The numbers layer is white digits on transparency, shown at alpha 0.9 over the body as the export sets it.
