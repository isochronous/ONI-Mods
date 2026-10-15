"""Build the asterisk the element sensors flash while they are set to Any.

    python tools/make_art.py

In the vanilla sensor kanims (liquid_element_sensor, gas_element_sensor,
conveyor_element_sensor, world_element_sensor, world_liquid_sensor) the icon that flashes on
the screen while the sensor is on (a drop, a gas gust, a lump of ore) is the symbol "elements".
Only the looping "on" anim draws it, frame by frame 0 to 25 over the second half of the loop;
the fade is baked into those 26 frames (frame 0 and 25 are empty, alpha peaks mid-way), not
done by the anim. The mod overrides "elements" with an asterisk symbol of this kanim (see
AnyGlyph.cs), so the override has to have the same 26 frames: a frame number past the end of
a symbol draws nothing.

The icons' colours are baked into the textures as well (the anims draw them with a white
multiplier), and so is where each icon sits relative to the anim's anchor for "elements",
which is not the middle of every screen. An override keeps the vanilla anchor, so each kanim
gets its own symbol, asterisk_<kanim>, in that kanim's icon colour and with the glyph offset
inside its frames so that it lands on the middle of the screen; see SENSORS for how that
middle is measured. AnyGlyph.cs picks the symbol by the sensor's kanim.

The asterisk is the plain asterisk of Liberation Sans Bold (the game's own UI face; SIL Open
Font License, see LICENSE-LiberationSans.txt), rendered large, thickened with a round stroke
so it reads as solid as the vanilla icons, and scaled down.

Writes src/DetectAnyElement/anim/assets/detect_any_asterisk/.
"""
import math
import os
import sys

from PIL import Image, ImageDraw, ImageFont

TOOLS = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(TOOLS)
sys.path.insert(0, os.path.join(ROOT, "..", "..", "common", "tools", "MakeKanim"))
from kanim_writer import Sprite, write_kanim  # noqa: E402

NAME = "detect_any_asterisk"
OUT = os.path.join(ROOT, "src", "DetectAnyElement", "anim", "assets", NAME)

FONT = os.path.join(TOOLS, "LiberationSans-Bold.ttf")
GLYPH = "*"
RENDER_PX = 900           # font size when rendered, before scaling down
STROKE = 0.025            # extra weight, as a fraction of the font size
SIZE_UNITS = 64.0         # display size (a cell is 200 units); the vanilla icons are 50 to 78
TEXTURE_SCALE = 1.0       # texture pixels per anim unit, sharper than vanilla's 0.5
MARGIN = 0.03             # transparent border round the glyph, so its edge is not cut
FRAMES = 26               # as many as the vanilla "elements" symbol

# Per sensor kanim, measured from the game's own files:
#   colour  mean colour of the vanilla icon's opaque pixels at its brightest frame
#   anchor  where the "on" anim puts the "elements" symbol's origin (x, y, scale), in anim
#           units: x right, y down, origin at the bottom centre of the cell
#   centre  the centroid of the screen symbol's opaque pixels in the "on" anim, or for the
#           conveyor the alpha centroid of the vanilla ore at its brightest frame
#   align   what of the asterisk goes on the centre: its bounding box ("box") or its alpha
#           centroid ("centroid"), which sits lower on a five-armed asterisk
SENSORS = {
    "liquid_element_sensor": {"colour": (146, 210, 217), "anchor": (-2.5, -100.15, 1.0), "centre": (-0.9, -100.4), "align": "box"},
    "gas_element_sensor": {"colour": (171, 247, 157), "anchor": (-2.5, -100.15, 1.0), "centre": (0.7, -101.4), "align": "box"},
    "conveyor_element_sensor": {"colour": (167, 246, 156), "anchor": (-3.05, -94.0, 0.745), "centre": (1.8, -94.9), "align": "centroid"},
    "world_element_sensor": {"colour": (172, 247, 157), "anchor": (-2.5, -104.15, 1.0), "centre": (4.8, -104.7), "align": "box"},
    "world_liquid_sensor": {"colour": (113, 232, 240), "anchor": (3.5, -108.65, 1.0), "centre": (4.8, -104.7), "align": "box"},
}


def centroid(mask):
    """Alpha-weighted centre of a mask, in pixels."""
    columns = mask.resize((mask.width, 1), Image.BOX)
    rows = mask.resize((1, mask.height), Image.BOX)
    xs = list(columns.tobytes())
    ys = list(rows.tobytes())
    return (sum((i + 0.5) * v for i, v in enumerate(xs)) / sum(xs),
            sum((i + 0.5) * v for i, v in enumerate(ys)) / sum(ys))


def glyph_mask():
    """The asterisk as an alpha mask on a square canvas with a small margin, centred on its
    bounding box."""
    font = ImageFont.truetype(FONT, RENDER_PX)
    canvas = Image.new("L", (RENDER_PX * 2, RENDER_PX * 2), 0)
    ImageDraw.Draw(canvas).text((RENDER_PX, RENDER_PX), GLYPH, font=font, fill=255, anchor="mm",
                                stroke_width=round(RENDER_PX * STROKE), stroke_fill=255)
    box = canvas.getbbox()
    side = round(max(box[2] - box[0], box[3] - box[1]) * (1 + 2 * MARGIN))
    left = (box[0] + box[2]) // 2 - side // 2
    top = (box[1] + box[3]) // 2 - side // 2
    return canvas.crop((left, top, left + side, top + side))


def symbol_frames(mask, colour, offset):
    """The 26 fading frames, the glyph moved by `offset` (symbol units) from the frame's centre.

    The kanim writer puts a frame's centre on the symbol's origin, so the frame is grown by
    twice the offset and the glyph drawn off-centre in it."""
    dx, dy = offset
    width, height = SIZE_UNITS + 2 * abs(dx), SIZE_UNITS + 2 * abs(dy)
    size = (round(width * TEXTURE_SCALE), round(height * TEXTURE_SCALE))
    glyph_px = mask.size[0]
    left = round((width / 2 + dx) * TEXTURE_SCALE - glyph_px / 2)
    top = round((height / 2 + dy) * TEXTURE_SCALE - glyph_px / 2)
    frames = []
    for i in range(FRAMES):
        # Vanilla fades in and out over the frames; a half sine matches its curve closely.
        alpha = math.sin(math.pi * i / (FRAMES - 1))
        image = Image.new("RGBA", size, colour + (0,))
        frame_mask = Image.new("L", size, 0)
        frame_mask.paste(mask.point(lambda v, a=alpha: round(v * a)), (left, top))
        image.putalpha(frame_mask)
        frames.append(Sprite(image, width, height))
    return frames


def main():
    mask = glyph_mask().resize((round(SIZE_UNITS * TEXTURE_SCALE),) * 2, Image.LANCZOS)
    # How far the asterisk's alpha centroid sits from its box centre, in symbol units.
    mx, my = centroid(mask)
    lean = ((mx - mask.width / 2) / TEXTURE_SCALE, (my - mask.height / 2) / TEXTURE_SCALE)
    symbols = {}
    for kanim, sensor in SENSORS.items():
        ax, ay, scale = sensor["anchor"]
        cx, cy = sensor["centre"]
        # The anim scales the symbol about its origin, so the offset is in unscaled units.
        offset = ((cx - ax) / scale, (cy - ay) / scale)
        if sensor["align"] == "centroid":
            offset = (offset[0] - lean[0], offset[1] - lean[1])
        symbols["asterisk_" + kanim] = symbol_frames(mask, sensor["colour"], offset)
        print("asterisk_%s: offset %.1f, %.1f" % ((kanim,) + offset))
    # The anim is not played by the mod; a kanim just needs one, and this one shows the glyphs.
    peak = FRAMES // 2
    row = [(name, peak, (i - 2) * SIZE_UNITS * 1.5, -100.0) for i, name in enumerate(symbols)]
    write_kanim(OUT, NAME, symbols, {"ui": [row]}, texture_size=1024)
    print("wrote %s: %d symbols of %d frames, %.0f units" % (OUT, len(symbols), FRAMES, SIZE_UNITS))


if __name__ == "__main__":
    main()
