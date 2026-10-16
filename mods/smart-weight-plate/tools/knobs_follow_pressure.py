"""Rework 3GuB's smart_weight_plate kanim so it plays correctly in the game.

Run from the repo root:  python tools/knobs_follow_pressure.py

What it changes, and why:
- The knob motion follows pressure, not the signal. The artist keyed it to the on_ transitions,
  but the plate's signal and its pressure are independent (the invert option), so the off_
  transitions get the same motion with the red light, and both pressed idles hold the swapped pose.
- The transitions are faster, 10 frames at 30 fps instead of 20, with a quintic ease-in (the knobs
  creep at first and slam into the far end like a released spring), generated from the artist's
  start and end poses. The button sinks
  and rises with the same curve (the export switched it between two heights in one frame) and by
  less, so its top stays visible over the base when pressed, like the vanilla plate.
- The button draws behind the plate: it is the last element of every frame. Kanimal's convention,
  which the game follows, is that the first element of a frame is the frontmost.
- The _fg suffix the artist used makes kanimal set the Foreground flag on those symbols, which
  the game renders through a separate layering path that behaves differently in overlays; the
  flag is cleared so every view draws the frame in plain element order.

The anim and build files are read and written with common/tools/Kanim (see its format notes).
"""
import os
import sys

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "common", "tools", "Kanim"))
from kanim import FOREGROUND, read_anim_file, read_build_file, write_anim_file, write_build_file  # noqa: E402

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "src", "SmartWeightPlate", "anim", "assets", "smart_weight_plate")
ANIM = os.path.join(ROOT, "smart_weight_plate_anim.bytes")
BUILD = os.path.join(ROOT, "smart_weight_plate_build.bytes")
TRANSITION_FRAMES = 10
# How far the button sinks when pressed, in animation units. The artist's export used 56, which
# puts the whole button behind the base; 32 leaves its top visible over the base, like the vanilla plate.
BUTTON_PRESS_TRAVEL = 32.0

# ---------------------------------------------------------------- anim
# Elements are [0 symbol, 1 frame, 2 layer, 3 flags, 4-7 abgr, 8-13 m1..m6, 14 order].
with open(ANIM, "rb") as f:
    anim = read_anim_file(f.read())
anims = anim.anims
names = dict(anim.hashes)
by_name = {a[0]: a for a in anims}
def sym_is(el, n): return names.get(el[0]) == n
LIGHT_OFF, LIGHT_ON = 0, 1

# A released spring: barely moving at first, then slamming into the stop. Quintic ease-in, so the
# knobs cover the last half of the travel in the final two frames.
def ease_in(t): return t ** 5

def transition(first, last, light_frame):
    """Frames from the first pose to the last pose: knobs eased in, everything else from the last pose."""
    out = []
    for i in range(TRANSITION_FRAMES):
        t = ease_in(i / (TRANSITION_FRAMES - 1))
        els = []
        for a, z in zip(first[4], last[4]):
            el = list(z)
            if sym_is(el, "slider_fg") or sym_is(el, "button"):
                for k in (12, 13):   # m5, m6: translation
                    el[k] = a[k] + (z[k] - a[k]) * t
            if sym_is(el, "light_fg"):
                el[1] = light_frame
            els.append(el)
        out.append([last[0], last[1], last[2], last[3], els])
    return out

press_first, press_last = by_name["on_down_pre"][3][0], by_name["on_down_pre"][3][-1]
release_first, release_last = by_name["on_up_pre"][3][0], by_name["on_up_pre"][3][-1]
# The release should end exactly on the idle pose and the press should start from it.
rest = {el[1]: (el[12], el[13]) for el in by_name["on_up"][3][0][4] if sym_is(el, "slider_fg")}
for el in release_last[4]:
    if sym_is(el, "slider_fg"): el[12], el[13] = rest[el[1]]
for el in press_first[4]:
    if sym_is(el, "slider_fg"): el[12], el[13] = rest[el[1]]
# The button: up in the raised idle, BUTTON_PRESS_TRAVEL lower when pressed, moving through the transitions.
up_y = next(el[13] for el in by_name["on_up"][3][0][4] if sym_is(el, "button"))
down_y = up_y + BUTTON_PRESS_TRAVEL
for pose, y in ((press_first, up_y), (press_last, down_y), (release_first, down_y), (release_last, up_y)):
    for el in pose[4]:
        if sym_is(el, "button"): el[13] = y
for idle in ("on_down", "off_down"):
    for el in by_name[idle][3][0][4]:
        if sym_is(el, "button"): el[13] = down_y
by_name["on_down_pre"][3] = transition(press_first, press_last, LIGHT_ON)
by_name["off_down_pre"][3] = transition(press_first, press_last, LIGHT_OFF)
by_name["on_up_pre"][3] = transition(release_first, release_last, LIGHT_ON)
by_name["off_up_pre"][3] = transition(release_first, release_last, LIGHT_OFF)

# Pressed idles hold the end-of-press knob positions.
end_of_press = {el[1]: (el[12], el[13]) for el in press_last[4] if sym_is(el, "slider_fg")}
for idle in ("on_down", "off_down"):
    for el in by_name[idle][3][0][4]:
        if sym_is(el, "slider_fg"): el[12], el[13] = end_of_press[el[1]]

# Button behind everything: last in every frame (first element is the frontmost).
for name, hsh, rate, frames in anims:
    for fr in frames:
        btn = [el for el in fr[4] if sym_is(el, "button")]
        fr[4] = [el for el in fr[4] if not sym_is(el, "button")] + btn

with open(ANIM, "wb") as f:
    f.write(write_anim_file(anim))
for a in anims:
    print("  %-13s %2d frames  front-to-back: %s" % (a[0], len(a[3]), [names.get(el[0]) for el in a[3][0][4]]))

# ---------------------------------------------------------------- build: clear the Foreground flag
with open(BUILD, "rb") as f:
    build = read_build_file(f.read())
bnames = dict(build.hashes)
for symbol in build.symbols:
    if symbol[3] & FOREGROUND:
        symbol[3] &= ~FOREGROUND
        print("  cleared Foreground on", bnames.get(symbol[0], hex(symbol[0])))
with open(BUILD, "wb") as f:
    f.write(write_build_file(build))
print("done")
