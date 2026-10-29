"""Build the Ribbon Reader and Ribbon Writer kanims with the bit display, and the Ribbon
Diagnostic Panel's kanim with its build menu icon.

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
input 1, output 4), idle and off as copies of net0, and the vanilla place and ui with their
art changed to match: the construction ghost loses the bit lights' white outlines and gets
outlines of the panel and the signal light, and the build menu icon is the building itself, unlit,
scaled down over the plain shadow the game's icons have. Output goes
to src/Bitshifter/anim/assets/bitshifter_ribbon_reader and ..._writer.

The Ribbon Diagnostic Panel's kanim (diagnostic_panel, by 3GuB, build menu icon included) is
read from anim-original/diagnostic_panel, the artist's export kept as delivered, and written to
src/Bitshifter/anim/assets/diagnostic_panel with one change: the export leaves every frame's
bounds unset (NaN), so they are computed from the frame's sprites. The texture and the build
file are copied as they are. A new export from the artist goes into anim-original and through
this script again.

--check builds all three in the temp folder instead and fails when they differ from the
committed ones (after a game update or a new export, say).
"""
import argparse
import os
import shutil
import sys
import tempfile

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "..", "..", "common", "tools", "Kanim"))
from kanim import (Sprite, drift, extract, load, read_anim_file, read_anims, read_build, read_kanim,  # noqa: E402
                   render, show, vanilla, write_anim_file, write_kanim)

ASSETS = os.path.join(ROOT, "src", "Bitshifter", "anim", "assets")

# The Ribbon Diagnostic Panel's kanim, as the artist exported it.
DIAG = "diagnostic_panel"
DIAG_ORIGINAL = os.path.join(ROOT, "anim-original", DIAG)

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


def face_region(image, face_colour, seed, window=20):
    """The flat face of a box in an image: the face-coloured region with the most pixels near
    the seed pixel, holes (the bit lights drawn on it) filled."""
    rgb = np.array(image.convert("RGB")).astype(int)
    alpha = np.array(image.convert("RGBA"))[:, :, 3] > 0
    face = alpha & (np.abs(rgb - np.array(face_colour)).sum(axis=2) < 60)
    face = ndimage.binary_opening(face, iterations=2)
    labels, _ = ndimage.label(face)
    x, y = seed
    near = labels[max(y - window, 0):y + window, max(x - window, 0):x + window]
    return ndimage.binary_fill_holes(labels == np.bincount(near[near > 0]).argmax())


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
    region = face_region(canvas, face_colour, seed)
    ys, xs = np.where(region)
    return (((xs.min() + xs.max()) / 2 - ox) / scale, ((ys.min() + ys.max()) / 2 - oy) / scale)


def outline(alpha, thickness):
    """White line art along the edge of an alpha mask (a PIL "L" image), `thickness` pixels
    wide, drawn oversampled so the line is smooth."""
    o = OVERSAMPLE
    big = np.array(alpha.resize((alpha.width * o, alpha.height * o), Image.LANCZOS)) > 127
    edge = big & ~ndimage.binary_erosion(big, iterations=max(1, round(thickness * o)))
    line = Image.fromarray((edge * 255).astype(np.uint8)).resize(alpha.size, Image.BOX)
    out = Image.new("RGBA", alpha.size, (255, 255, 255, 0))
    out.putalpha(line)
    return out


def sprite_frame(sprite, element):
    """(scale in texture pixels per anim unit, the anim position of the sprite's top left)."""
    _, _, x, y, opts = element
    a, b, c, d = opts["matrix"]
    assert abs(a - 1) < 1e-3 and abs(d - 1) < 1e-3 and abs(b) < 1e-3 and abs(c) < 1e-3, "a transformed sprite"
    return sprite.image.width / sprite.width, (x - sprite.width / 2, y - sprite.height / 2)


def paste_at(canvas, image, scale, origin, centre):
    """Pastes image (already at the canvas's scale) with its centre on an anim position."""
    x = round((centre[0] - origin[0]) * scale - image.width / 2)
    y = round((centre[1] - origin[1]) * scale - image.height / 2)
    canvas.alpha_composite(image, (x, y))


def place_art(symbols, anims, staircase, display):
    """The construction ghost: vanilla's white outlines without the bit lights' outlines, plus
    outlines of the display panel and the signal light. staircase lists the bit light elements
    of a pose and display the display's elements."""
    sprite = symbols["place"][0]
    scale, origin = sprite_frame(sprite, anims["place"][0][0])
    image = sprite.image.convert("RGBA")
    # Clear what lies under the bit lights, a little beyond their edges.
    mask = Image.new("L", image.size, 0)
    for name, index, x, y, opts in staircase:
        light = symbols[name][index]
        alpha = light.image.getchannel("A").resize((round(light.width * scale), round(light.height * scale)), Image.LANCZOS)
        layer = Image.new("RGBA", image.size, (0, 0, 0, 0))
        paste_at(layer, Image.merge("RGBA", (alpha,) * 4), scale, origin, (x, y))
        mask = Image.fromarray(np.maximum(np.array(mask), np.array(layer.getchannel("A"))))
    cleared = ndimage.binary_dilation(np.array(mask) > 0, iterations=2)
    pixels = np.array(image)
    pixels[cleared, 3] = 0
    image = Image.fromarray(pixels)
    for name, index, x, y in display:
        if name.startswith("seg_"):
            continue
        part = symbols[name][index]
        alpha = part.image.getchannel("A").resize((round(part.width * scale), round(part.height * scale)), Image.LANCZOS)
        paste_at(image, outline(alpha, 1.5), scale, origin, (x, y))
    return Sprite(image, sprite.width, sprite.height)


def mirrored(icon, building):
    """Whether the building's outline matches the vanilla icon's better turned left to right."""
    def mask(im):
        im = im.crop(im.getbbox()).getchannel("A").resize((64, 32), Image.BILINEAR)
        return np.asarray(im, dtype=float) / 255
    target, own = mask(icon), mask(building)
    return np.abs(target - own[:, ::-1]).sum() < np.abs(target - own).sum()


def ui_art(vanilla_icon, kanim):
    """The build menu icon: the building itself in its unlit pose, scaled down to the size of
    vanilla's icon, over the plain shadow the game's icons have (a black ellipse at 30%, as
    wide as the building and an eighth as tall, centred a little above its bottom edge).
    Facing the way vanilla's icon faces: the reader's is drawn mirrored from the building."""
    tex, build_bytes, anim_bytes = kanim
    segs = {f"seg_{seg}_{position}" for position in (1, 2) for seg in "abcdefg"}
    building, _ = render(tex, build_bytes, anim_bytes, "net0", scale=1.0, skip=segs)
    building = building.crop(building.getbbox())
    if mirrored(vanilla_icon.image, building):
        building = building.transpose(Image.FLIP_LEFT_RIGHT)
    return over_shadow(building, vanilla_icon)


def over_shadow(building, reference):
    """A rendered building scaled down to fit the texture of the reference icon (a Sprite),
    keeping its pixels per unit, over the icons' plain shadow."""
    shadow_h = building.width / 8
    # Fit building and shadow in the reference icon's texture, keeping its 0.5 pixels per unit.
    room = reference.image.width - 8, reference.image.height - 8
    fit = min(room[0] / building.width, room[1] / (building.height + shadow_h * 0.75))
    building = building.resize((round(building.width * fit), round(building.height * fit)), Image.LANCZOS)
    shadow_h *= fit
    o = OVERSAMPLE
    w, h = building.width + 8, round(building.height + shadow_h * 0.75) + 8
    shadow = Image.new("RGBA", (w * o, h * o), (0, 0, 0, 0))
    cy = (4 + building.height - shadow_h / 4) * o
    ImageDraw.Draw(shadow).ellipse([4 * o, cy - shadow_h * o / 2, (4 + building.width) * o, cy + shadow_h * o / 2], fill=(0, 0, 0, 76))
    icon = shadow.resize((w, h), Image.LANCZOS)
    icon.alpha_composite(building, (4, 4))
    scale = reference.image.width / reference.width
    return Sprite(icon, icon.width / scale, icon.height / scale)


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
    staircase = [element for element, (_, _, _, _, layer) in zip(anims["5_1"][0], raw["5_1"][0])
                 if element[0] in BIT_SYMBOLS and not (element[0] in ("bit1", "bit1_bloom") and layer not in layer_names)]
    symbols["place"] = [place_art(symbols, anims, staircase, display(False))]
    for name in BIT_SYMBOLS:
        symbols.pop(name, None)
    folder = os.path.join(out_root, new_name)
    write_kanim(folder, new_name, symbols, out, bloom=bloom)
    # The icon is a render of the building, so it is drawn from the kanim just written.
    symbols["ui"] = [ui_art(symbols["ui"][0], load(folder)[1:])]
    write_kanim(folder, new_name, symbols, out, bloom=bloom)
    print(f"{new_name}: display centred at ({gx:.1f}, {gy:.1f})")


def build_panel(out_root):
    """The Ribbon Diagnostic Panel: the artist's kanim from anim-original as it is (texture and
    build file copied byte for byte, so the sprites keep the export's own UVs), with each anim
    frame's bounds, which the export leaves NaN, set to the box around its sprites."""
    folder = os.path.join(out_root, DIAG)
    os.makedirs(folder, exist_ok=True)
    for suffix in ("_0.png", "_build.bytes"):
        shutil.copyfile(os.path.join(DIAG_ORIGINAL, DIAG + suffix), os.path.join(folder, DIAG + suffix))
    with open(os.path.join(DIAG_ORIGINAL, DIAG + "_build.bytes"), "rb") as f:
        built, _ = read_build(f.read())
    with open(os.path.join(DIAG_ORIGINAL, DIAG + "_anim.bytes"), "rb") as f:
        anim = read_anim_file(f.read())
    for _, _, _, frames in anim.anims:
        for frame in frames:
            x1 = y1 = float("inf")
            x2 = y2 = float("-inf")
            for symbol, index, *_rest in frame[4]:
                numbers = sorted(built[symbol])
                lower = [n for n in numbers if n <= index]
                px, py, pw, ph = built[symbol][max(lower) if lower else numbers[0]][:4]
                a, b, c, d, tx, ty = _rest[6:12]
                # The sprite's centre and the half extents of its transformed box.
                x, y = a * px + c * py + tx, b * px + d * py + ty
                ex, ey = abs(a) * pw / 2 + abs(c) * ph / 2, abs(b) * pw / 2 + abs(d) * ph / 2
                x1, x2, y1, y2 = min(x1, x - ex), max(x2, x + ex), min(y1, y - ey), max(y2, y + ey)
            frame[:4] = [(x1 + x2) / 2, (y1 + y2) / 2, x2 - x1, y2 - y1]
    with open(os.path.join(folder, DIAG + "_anim.bytes"), "wb") as f:
        f.write(write_anim_file(anim))
    print(f"{DIAG}: copied from {os.path.relpath(DIAG_ORIGINAL, ROOT)} with its anim bounds set")


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
    build_panel(out_root)
    if a.check:
        changed = [f for n in list(BUILDINGS) + [DIAG] for f in drift(os.path.join(out_root, n), os.path.join(ASSETS, n))]
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
