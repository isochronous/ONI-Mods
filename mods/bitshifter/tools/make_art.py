"""Build the Ribbon Reader and Ribbon Writer kanims with the bit display.

    python tools/make_art.py [--game <OxygenNotIncluded folder>] [--preview [out.png]] [--check]

Starts from the vanilla logic_ribbon_reader and logic_ribbon_writer kanims, read from the
installed game (see common/tools/Kanim). In each, the
four bit lights (the "staircase" and its "selected" ring) are replaced by:

  panel           a dark rounded display panel with all fourteen segments drawn unlit
  seg_<a-g>_<1|2> one lit amber segment each, a-g clockwise from the top with g in the middle,
                  1 the tens digit and 2 the ones; the mod shows or hides them per digit
  signal_bloom    the selected bit's light (vanilla's bit-1 light), tinted on or off by the mod;
                  the light in the arrow uses it too
  signal_dark     the same light unlit, for the poses where its network is not wired

The display is drawn upright for the building standing vertically with the display box at the
bottom, the signal light below the digits, then turned into the kanim's horizontal frame. The
writer's display box is at the other end of the building from the reader's, so its display is
turned the other way round. Both use the size that fits the reader's face, centred on each
box's flat face (measured from the art, inset from the bevel lines).

The anims are net0, net1, net4 and net5, the vanilla "<ports>_1" poses without the bit lights
(the number counts the wired ports as the vanilla code does: reader input 4, output 1; writer
input 1, output 4), idle and off as copies of net0, and the vanilla place and ui. Output goes
to src/Bitshifter/anim/assets/bitshifter_ribbon_reader and ..._writer. --check builds them in
the temp folder instead and fails when they differ from those (after a game update, say).
"""
import argparse
import os
import sys
import tempfile

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "..", "..", "common", "tools", "Kanim"))
from kanim import Sprite, drift, extract, load, read_anims, read_build, read_kanim, render, show, vanilla, write_kanim  # noqa: E402

ASSETS = os.path.join(ROOT, "src", "Bitshifter", "anim", "assets")

# Display colours: an amber LED display behind a dark warm window, unlit segments faintly visible.
PANEL = (46, 40, 36, 255)
RIM = (120, 108, 100, 255)
INK = (25, 20, 40, 255)
UNLIT = (78, 64, 52, 255)
LIT = (255, 176, 58, 255)

# Geometry in anim units (a cell is 200), in the upright (vertical) view.
DIGIT_W, DIGIT_H = 24, 47
PAD, DIGIT_GAP, LIGHT_GAP, LIGHT = 6, 5, 7, 20
STROKE = int(DIGIT_H * 0.16)
PANEL_W, PANEL_H = 2 * DIGIT_W + DIGIT_GAP + 2 * PAD, DIGIT_H + 2 * PAD
GROUP_H = PANEL_H + LIGHT_GAP + LIGHT
OVERSAMPLE = 4

# Each building: its vanilla kanim, the symbol of the box that holds the display, that box's
# flat face colour, and which way the display turns into the horizontal frame (degrees, PIL's
# counter-clockwise).
BUILDINGS = {
    "bitshifter_ribbon_reader": ("logic_ribbon_reader", "reader_base", (134, 101, 211), -90),
    "bitshifter_ribbon_writer": ("logic_ribbon_writer", "writer_base", (74, 212, 208), 90),
}
BIT_SYMBOLS = {"bit1", "bit2", "bit3", "bit4", "bit1_bloom", "bit2_bloom", "bit3_bloom", "bit4_bloom", "selected"}
POSES = (0, 1, 4, 5)


def segments(x, y, w, h, t):
    """The seven segments of a digit in the box (x, y, w, h), stroke t, as hexagonal bars."""
    s = t // 2

    def hbar(cx, cy, length):
        return [(cx - length / 2 + s, cy - t / 2), (cx + length / 2 - s, cy - t / 2), (cx + length / 2, cy),
                (cx + length / 2 - s, cy + t / 2), (cx - length / 2 + s, cy + t / 2), (cx - length / 2, cy)]

    def vbar(cx, cy, length):
        return [(cx - t / 2, cy - length / 2 + s), (cx, cy - length / 2), (cx + t / 2, cy - length / 2 + s),
                (cx + t / 2, cy + length / 2 - s), (cx, cy + length / 2), (cx - t / 2, cy + length / 2 - s)]

    across, down = w - t, (h - t) / 2 - t * 0.1
    return {
        "a": hbar(x + w / 2, y + t / 2, across), "g": hbar(x + w / 2, y + h / 2, across),
        "d": hbar(x + w / 2, y + h - t / 2, across),
        "f": vbar(x + t / 2, y + h / 4, down), "e": vbar(x + t / 2, y + 3 * h / 4, down),
        "b": vbar(x + w - t / 2, y + h / 4, down), "c": vbar(x + w - t / 2, y + 3 * h / 4, down),
    }


def digit_segments(position):
    o = OVERSAMPLE
    return segments((PAD + position * (DIGIT_W + DIGIT_GAP)) * o, PAD * o, DIGIT_W * o, DIGIT_H * o, STROKE * o)


def group_layer(draw):
    """A layer the size of the display group (panel and light) in the upright view."""
    im = Image.new("RGBA", (PANEL_W * OVERSAMPLE, GROUP_H * OVERSAMPLE), (0, 0, 0, 0))
    draw(ImageDraw.Draw(im))
    return im.resize((PANEL_W, GROUP_H), Image.LANCZOS)


def draw_panel(d):
    o = OVERSAMPLE
    d.rounded_rectangle([0, 0, PANEL_W * o - 1, PANEL_H * o - 1], radius=5 * o, fill=PANEL, outline=INK, width=2 * o)
    d.rounded_rectangle([2 * o, 2 * o, PANEL_W * o - 1 - 2 * o, PANEL_H * o - 1 - 2 * o], radius=4 * o, outline=RIM, width=o)
    for position in range(2):
        for poly in digit_segments(position).values():
            d.polygon(poly, fill=UNLIT)


def display_sprites(turn):
    """{symbol: (Sprite, (dx, dy))}: the panel and segments turned into the kanim's frame, each
    with its centre's offset from the display group's centre in anim units, plus the signal
    light's offset (no sprite: it reuses the vanilla light)."""
    layers = {"panel": group_layer(draw_panel)}
    for position in range(2):
        for seg in "abcdefg":
            def lit(d, poly=digit_segments(position)[seg]):
                d.polygon(poly, fill=LIT)
            layers[f"seg_{seg}_{position + 1}"] = group_layer(lit)
    out = {}
    for name, im in layers.items():
        turned = im.rotate(turn, expand=True)
        box = turned.getbbox()
        crop = turned.crop(box)
        offset = ((box[0] + box[2]) / 2 - turned.width / 2, (box[1] + box[3]) / 2 - turned.height / 2)
        out[name] = (Sprite(crop, crop.width, crop.height), offset)
    below = PANEL_H + LIGHT_GAP + LIGHT / 2 - GROUP_H / 2  # the light, from the group centre, upright view
    out["signal"] = (None, (-below if turn < 0 else below, 0.0))
    return out


def face_centre(game, name, box_symbol, face_colour):
    """Centre of the flat face of the box that holds the display, in anim units: the bounding
    box of the face-coloured region around the box's own position, holes filled."""
    tex, build, anim = vanilla(name, game)
    scale = 2.0
    canvas, (ox, oy) = render(tex, build, anim, "5_1", scale=scale, skip=BIT_SYMBOLS)
    seed = None
    _, names = read_build(build)
    for image, _, _, m, _ in read_anims(anim)["5_1"][0]:
        if names.get(image) == box_symbol:
            seed = (int(ox + m[4] * scale), int(oy + m[5] * scale))
    rgb = np.array(canvas.convert("RGB")).astype(int)
    alpha = np.array(canvas)[:, :, 3] > 0
    face = alpha & (np.abs(rgb - np.array(face_colour)).sum(axis=2) < 60)
    face = ndimage.binary_opening(face, iterations=2)
    labels, _ = ndimage.label(face)
    x, y = seed
    near = labels[y - 20:y + 20, x - 20:x + 20]
    region = ndimage.binary_fill_holes(labels == np.bincount(near[near > 0]).argmax())
    ys, xs = np.where(region)
    return (((xs.min() + xs.max()) / 2 - ox) / scale, ((ys.min() + ys.max()) / 2 - oy) / scale)


def build(out_root, game, new_name, game_name, box_symbol, face_colour, turn):
    tex, build_bytes, anim_bytes = vanilla(game_name, game)
    symbols, anims, bloom = read_kanim(tex, build_bytes, anim_bytes)
    layer_names = read_build(build_bytes)[1]
    gx, gy = face_centre(game, game_name, box_symbol, face_colour)
    art = display_sprites(turn)

    # The arrow's light is the bit-1 sprite on an unnamed layer (the staircase's own bit-1 light
    # is on the "bit1" layer); it becomes a signal light.
    raw = read_anims(anim_bytes)
    symbols["signal_bloom"] = [symbols["bit1_bloom"][0]]
    symbols["signal_dark"] = [symbols["bit1"][1]]
    bloom = (bloom - BIT_SYMBOLS) | {"signal_bloom"} | {n for n in art if n.startswith("seg_")}
    for n in art:
        if art[n][0] is not None:
            symbols[n] = [art[n][0]]

    def display(dark):
        elements = []
        for position in (1, 2):
            for seg in "abcdefg":
                dx, dy = art[f"seg_{seg}_{position}"][1]
                elements.append((f"seg_{seg}_{position}", 0, gx + dx, gy + dy))
        dx, dy = art["panel"][1]
        elements.append(("panel", 0, gx + dx, gy + dy))
        dx, dy = art["signal"][1]
        elements.append(("signal_dark" if dark else "signal_bloom", 0, gx + dx, gy + dy))
        return elements

    out = {}
    for ports in POSES:
        dark = ports < 4  # the network the bits are read from is not wired
        frame = display(dark)
        for element, (_, _, _, _, layer) in zip(anims[f"{ports}_1"][0], raw[f"{ports}_1"][0]):
            name = element[0]
            if name not in BIT_SYMBOLS:
                frame.append(element)
            elif name in ("bit1", "bit1_bloom") and layer not in layer_names:
                frame.append(("signal_dark" if dark else "signal_bloom", 0) + tuple(element[2:]))
        out[f"net{ports}"] = [frame]
    out["idle"] = out["off"] = out["net0"]
    out["place"], out["ui"] = anims["place"], anims["ui"]
    for name in BIT_SYMBOLS:
        symbols.pop(name, None)
    write_kanim(os.path.join(out_root, new_name), new_name, symbols, out, bloom=bloom)
    print(f"{new_name}: display centred at ({gx:.1f}, {gy:.1f})")


def preview(path):
    """Both buildings with both ports wired, showing 23, horizontal and turned upright."""
    hidden = {"seg_c_1", "seg_f_1", "seg_e_2", "seg_f_2"}
    tiles = []
    for name in BUILDINGS:
        _, tex, build_bytes, anim_bytes = load(os.path.join(ASSETS, name))
        im, _ = render(tex, build_bytes, anim_bytes, "net5", scale=2.0, skip=hidden)
        tiles += [im, im.rotate(90, expand=True)]
    gap = 20
    sheet = Image.new("RGBA", (sum(t.width for t in tiles) + gap * (len(tiles) + 1), max(t.height for t in tiles) + 2 * gap), (40, 40, 40, 255))
    x = gap
    for t in tiles:
        sheet.alpha_composite(t, (x, (sheet.height - t.height) // 2))
        x += t.width + gap
    sheet.save(path)
    print("wrote", path)


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--game", help="the game folder (default: ONI_GAME_FOLDER, then the usual Steam folders)")
    p.add_argument("--preview", nargs="?", const="",
                   help="also write a PNG of both buildings showing 23 (default: write it to the temp folder and open it)")
    p.add_argument("--check", action="store_true", help="build in the temp folder and compare with the committed kanims")
    a = p.parse_args()
    out_root = tempfile.mkdtemp(prefix="bitshifter-art-") if a.check else ASSETS
    extract([spec[0] for spec in BUILDINGS.values()], a.game)
    for new_name, spec in BUILDINGS.items():
        build(out_root, a.game, new_name, *spec)
    if a.check:
        changed = [f for n in BUILDINGS for f in drift(os.path.join(out_root, n), os.path.join(ASSETS, n))]
        if changed:
            sys.exit("the committed kanims differ from a fresh build: " + ", ".join(changed))
        print("the committed kanims match a fresh build")
    if a.preview is not None:
        path = a.preview or os.path.join(tempfile.gettempdir(), "bitshifter-preview.png")
        preview(path)
        if not a.preview:
            show(path)


if __name__ == "__main__":
    main()
