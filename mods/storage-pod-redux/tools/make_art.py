"""Build the Storage Pod and Cool Pod kanims.

    python tools/make_art.py

Each pod is one sprite: publish/sprite-storage.png and publish/sprite-cool.png when they
exist, else a placeholder drawn here (a rounded pod with ink outlines) so the mod can be
built and tested before the real art arrives. Both kanims carry the animation set the
vanilla Storage Bin and Refrigerator use through StorageController and FilteredStorage:

  off / on / working   the pod (on = powered, for the Cool Pod: indicator lit)
  place / ui           construction ghost (white outline) and build-menu icon
  meter                16-frame fill gauge; FilteredStorage drives it by fill level and
                       tracks it to the hidden meter_target symbol
  logicmeter           Cool Pod only: 2-frame full/not-full indicator on logicmeter_target

Units are Klei anim pixels (a cell is 200 x 200, origin bottom centre, y down).
"""
import os
import sys

from PIL import Image, ImageDraw

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "common", "tools", "MakeKanim"))
from kanim_writer import Sprite, place_outline, write_kanim  # noqa: E402

OUT = os.path.join(ROOT, "src", "StoragePodRedux", "anim", "assets")
SIZE = 184.0            # display size of the pod's longer side, anim units
CENTER = (0.0, -100.0)  # cell centre
METER_FRAMES = 16
SPRITE_PX = 160         # placeholder resolution


def placeholder(fill, accent, lit=False, cool=False):
    """A rounded pod: dark ink outline, a hatch seam, four bolts, and for the Cool Pod a
    frost-blue window with an indicator light."""
    im = Image.new("RGBA", (SPRITE_PX, SPRITE_PX), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    ink = (18, 18, 22, 255)
    d.rounded_rectangle((8, 8, SPRITE_PX - 8, SPRITE_PX - 8), radius=34, fill=fill, outline=ink, width=7)
    # hatch seam and handle
    d.line((SPRITE_PX // 2, 14, SPRITE_PX // 2, SPRITE_PX - 14), fill=ink, width=5)
    d.rounded_rectangle((SPRITE_PX // 2 - 22, SPRITE_PX // 2 - 8, SPRITE_PX // 2 + 22, SPRITE_PX // 2 + 8), radius=6, fill=accent, outline=ink, width=4)
    for x, y in ((28, 28), (SPRITE_PX - 28, 28), (28, SPRITE_PX - 28), (SPRITE_PX - 28, SPRITE_PX - 28)):
        d.ellipse((x - 7, y - 7, x + 7, y + 7), fill=accent, outline=ink, width=3)
    if cool:
        d.rounded_rectangle((26, 48, SPRITE_PX // 2 - 14, SPRITE_PX - 48), radius=10, fill=(196, 232, 244, 255), outline=ink, width=4)
        light = (120, 255, 120, 255) if lit else (70, 90, 80, 255)
        d.ellipse((SPRITE_PX - 52, SPRITE_PX // 2 - 30, SPRITE_PX - 36, SPRITE_PX // 2 - 14), fill=light, outline=ink, width=3)
    return im


def load_or_draw(name, **kw):
    path = os.path.join(ROOT, "publish", "sprite-%s.png" % name)
    if os.path.exists(path):
        im = Image.open(path).convert("RGBA")
        return im.crop(im.getbbox()), True
    return placeholder(**kw), False


def gauge_frames(color):
    """meter_level frames: a vertical bar filling bottom-up, plus the frame around it."""
    w, h = 18, 90
    frame = Image.new("RGBA", (w + 8, h + 8), (0, 0, 0, 0))
    ImageDraw.Draw(frame).rounded_rectangle((0, 0, w + 7, h + 7), radius=5, fill=(40, 40, 46, 255), outline=(18, 18, 22, 255), width=3)
    levels = []
    for i in range(METER_FRAMES):
        im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        top = h - round(h * i / (METER_FRAMES - 1))
        if top < h:
            ImageDraw.Draw(im).rectangle((0, top, w - 1, h - 1), fill=color)
        levels.append(im)
    return frame, levels


def sized(im, longer):
    k = longer / max(im.size)
    return Sprite(im, im.width * k, im.height * k)


def build(name, kanim, sprite_off, sprite_on, cool):
    body = [sized(sprite_off, SIZE), sized(sprite_on, SIZE)]
    frame_im, level_ims = gauge_frames((120, 230, 120, 255) if not cool else (140, 200, 255, 255))
    gauge_h = 44.0
    meter_frame = sized(frame_im, gauge_h)
    meter_levels = [Sprite(im, meter_frame.width * 18 / 26, meter_frame.height * 90 / 98) for im in level_ims]
    dot = Sprite(Image.new("RGBA", (2, 2), (0, 0, 0, 0)), 2, 2)
    symbols = {
        "body": body,
        "meter_frame": [meter_frame],
        "meter_level": meter_levels,
        "meter_target": [dot],
        "place": [Sprite(place_outline(sprite_off), body[0].width, body[0].height)],
        "ui": [body[0]],
    }
    cx, cy = CENTER
    gauge_x, gauge_y = cx + SIZE / 2 - 26, cy
    pod_off = [("body", 0, cx, cy), ("meter_target", 0, gauge_x, gauge_y)]
    pod_on = [("body", 1, cx, cy), ("meter_target", 0, gauge_x, gauge_y)]
    if cool:
        light = Sprite(Image.new("RGBA", (2, 2), (0, 0, 0, 0)), 2, 2)
        symbols["logicmeter_target"] = [light]
        symbols["logic_light"] = [sized(logic_light(False), 22), sized(logic_light(True), 22)]
        for frame in (pod_off, pod_on):
            frame.append(("logicmeter_target", 0, cx + SIZE / 2 - 26, cy - SIZE / 2 + 22))
    anims = {
        "off": [pod_off],
        "on": [pod_on],
        "working": [pod_on],
        "place": [[("place", 0, cx, cy)]],
        "ui": [[("ui", 0, cx, cy)]],
        # Meter anims are drawn around the tracked symbol, so at the origin.
        "meter": [[("meter_level", i, 0.0, 0.0), ("meter_frame", 0, 0.0, 0.0)] for i in range(METER_FRAMES)],
    }
    if cool:
        anims["logicmeter"] = [[("logic_light", 0, 0.0, 0.0)], [("logic_light", 1, 0.0, 0.0)]]
    write_kanim(os.path.join(OUT, kanim), kanim, symbols, anims)
    print("wrote %s (%s_kanim)" % (kanim, kanim))


def logic_light(on):
    im = Image.new("RGBA", (24, 24), (0, 0, 0, 0))
    ImageDraw.Draw(im).ellipse((2, 2, 21, 21), fill=(255, 90, 90, 255) if on else (60, 60, 66, 255), outline=(18, 18, 22, 255), width=3)
    return im


def main():
    storage, real = load_or_draw("storage", fill=(214, 196, 150, 255), accent=(120, 100, 70, 255))
    print("storage pod sprite:", "publish/sprite-storage.png" if real else "placeholder")
    build("storage", "storage_pod", storage, storage, cool=False)

    cool_off, real = load_or_draw("cool", fill=(170, 205, 220, 255), accent=(80, 120, 140, 255), cool=True)
    cool_on = cool_off if real else placeholder(fill=(170, 205, 220, 255), accent=(80, 120, 140, 255), lit=True, cool=True)
    print("cool pod sprite:", "publish/sprite-cool.png" if real else "placeholder")
    build("cool", "cool_pod", cool_off, cool_on, cool=True)


if __name__ == "__main__":
    main()
