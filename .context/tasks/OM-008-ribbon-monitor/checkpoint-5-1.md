# Checkpoint 5-1

Done: inspected the new export. Same symbols as the second export minus TileSize (bit_1..bit_31 44x46, body/place 426, numbers 730x650, ui 238x242 now with pivot y -121), same anims off/on/place/ui, anim.bytes unchanged, atlas 1024x512, no bloom flags, every anim frame's bounds NaN. C# references (bit_N, "on") still match: no C# change.

Found: the common checkout (7687b3c) has no read_kanim(inset=...) — 549ff6f, which had it, is gone from the object store. So the repack path in make_art.py does not run here.

Next: make build_panel keep the artist's png and build as delivered (ui included, no recut so no half-texel issue) and rewrite only the anim file with computed bounds; regenerate, build, preview, commit.
