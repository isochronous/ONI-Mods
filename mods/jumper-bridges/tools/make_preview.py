"""Makes the Workshop preview, publish/preview.png, from the preview tests' screenshots.

Take the shots first (the game must be closed):

    pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods JumperBridges -Filter "*Preview*" -Label jb-preview

then, from the repo root:

    python mods/jumper-bridges/tools/make_preview.py common/tools/TestRunner/runs/jb-preview-<stamp>

(a second run folder after the first takes the power shots from that run instead).

The image is the power setup in the power overlay over the automation setup in the automation
overlay, a black bar between, both at one cell size and each half lined up on its highlighted wire
row. Each half keeps the overlay's pixels only where the overlay highlights something the setup put
there (wires, bridge ends, port icons, readouts) and takes the scene's own colours from the
normal-view shot of the same frame everywhere else; the shot of the same overlay frame with the setup
gone tells the setup's highlights from what the overlay draws on its own (its grid, dark patches).
The title goes on with common/tools/MakePreview/title_preview.py, centred on the bar.
"""
import argparse
import os
import re
import subprocess
import sys
import warnings

import numpy as np
from PIL import Image, ImageFilter
from scipy import ndimage

HERE = os.path.dirname(os.path.abspath(__file__))
MOD = os.path.dirname(HERE)
REPO = os.path.dirname(os.path.dirname(MOD))
TITLE_TOOL = os.path.join(REPO, "common", "tools", "MakePreview", "title_preview.py")

SIZE = 1024       # the composed background; the title tool scales it to 512
BAR = 14
CELL = 128        # pixels per game cell in the background
HALF = (SIZE - BAR) // 2
AT = 265          # where a half's highlighted wire row sits, in pixels down the half

# Test name, cells across, the highlighted row (from the setup's bottom), where it sits in its half.
POWER = ("Power", "power", 6, 1, AT - 37)
AUTOMATION = ("Automation", "automation", 7, 1, AT)

TITLE = ["--title", "Jumper Bridges", "--lines", "Jumper|Bridges", "--style", "serif", "--font", "graystroke",
         "--position", "0.5215", "--width", "0.7", "--fill", "255,206,74", "--fill2", "255,150,190"]


def crop_of(run, test):
    """The setup's rectangle in the screenshots, from the "crop" line the test logs."""
    pattern = re.compile(r"Preview\." + test + r": crop (\d+),(\d+),(\d+),(\d+)")
    with open(os.path.join(run, "Player.log"), encoding="utf-8", errors="replace") as log:
        for line in log:
            match = pattern.search(line)
            if match:
                return tuple(int(v) for v in match.groups())
    sys.exit("no crop line for Preview." + test + " in " + run)


def coloured(overlay, normal, empty, crop, cell):
    """The overlay frame with the normal view's colours put back wherever the overlay only greys the scene."""
    o = np.asarray(Image.open(overlay).convert("RGB"), dtype=np.float32)
    n = np.asarray(Image.open(normal).convert("RGB"), dtype=np.float32)
    e = np.asarray(Image.open(empty).convert("RGB"), dtype=np.float32)
    luma = n @ np.array([0.299, 0.587, 0.114], dtype=np.float32)
    # The overlay's grey of the scene: a linear map of the normal view's luma, fitted on the whole frame
    # (the background dominates), then refitted without the pixels that clearly do not follow it.
    target = o.mean(axis=2)
    keep = np.ones_like(target, dtype=bool)
    for _ in range(3):
        # A million points on a straight line: polyfit's conditioning warning is noise here.
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            a, b = np.polyfit(luma[keep].ravel(), target[keep].ravel(), 1)
        resid = np.abs(o - (a * luma + b)[..., None]).max(axis=2)
        keep = resid < 30
    sat = o.max(axis=2) - o.min(axis=2)
    highlight = (resid > 45) | (sat > 40)
    # Only what the setup changed in the overlay frame.
    changed = np.abs(o - e).max(axis=2) > 24
    highlight &= ndimage.binary_dilation(changed, iterations=2)
    # Fill what the highlights enclose (a port icon's black background over a dark building).
    highlight = ndimage.binary_fill_holes(highlight)
    # Only on and around the setup: a quarter cell at the sides and below, a cell above for the readouts.
    x0, y0, x1, y1 = crop
    region = np.zeros_like(highlight)
    region[max(0, int(y0 - cell)):int(y1 + cell / 4), max(0, int(x0 - cell / 4)):int(x1 + cell / 4)] = True
    highlight &= region
    m = Image.fromarray((highlight * 255).astype(np.uint8))
    m = m.filter(ImageFilter.MinFilter(3)).filter(ImageFilter.MaxFilter(3))
    m = m.filter(ImageFilter.MaxFilter(5)).filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(1.5))
    w = np.asarray(m, dtype=np.float32)[..., None] / 255
    return Image.fromarray((o * w + n * (1 - w)).clip(0, 255).astype(np.uint8))


def half(run, spec):
    """One half of the background: the setup centred across, its highlighted row at its height."""
    test, shot, cells_w, row, at = spec
    crop = crop_of(run, test)
    base = os.path.join(run, "artifacts", "JumperBridges.Tests.Preview." + test + "-" + shot)
    x0, y0, x1, y1 = crop
    cell = (x1 - x0) / cells_w
    im = coloured(base + "-ports.png", base + "-normal.png", base + "-empty.png", crop, cell)
    k = CELL / cell
    w, h = SIZE / k, HALF / k
    cx = (x0 + x1) / 2
    top = y1 - (row + 0.5) * cell - at / k
    box = (round(cx - w / 2), round(top), round(cx + w / 2), round(top + h))
    if box[0] < 0 or box[1] < 0 or box[2] > im.width or box[3] > im.height:
        sys.exit("the " + shot + " half reaches outside its screenshot: " + str(box))
    return im.crop(box).resize((SIZE, HALF), Image.LANCZOS)


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("run", help="the preview tests' run folder")
    parser.add_argument("power_run", nargs="?", help="a run folder to take the power shots from instead")
    parser.add_argument("--out", default=os.path.join(MOD, "publish", "preview.png"))
    parser.add_argument("--background", help="also keep the untitled 1024px background here")
    args = parser.parse_args()

    image = Image.new("RGB", (SIZE, SIZE), (0, 0, 0))
    image.paste(half(args.power_run or args.run, POWER), (0, 0))
    image.paste(half(args.run, AUTOMATION), (0, SIZE - HALF))
    background = args.background or os.path.splitext(args.out)[0] + "-background.png"
    image.save(background)
    subprocess.run([sys.executable, TITLE_TOOL] + TITLE + [background, args.out], check=True)
    if not args.background:
        os.remove(background)


if __name__ == "__main__":
    main()
