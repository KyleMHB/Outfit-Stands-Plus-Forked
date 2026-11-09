"""Draw the outfit stand art used when Odyssey is not active.

Core's Building_OutfitStand draws fixed texture paths (Things/Building/OutfitStand/OutfitStand_Base, _Body, _BodyChild,
_Head) plus the build-menu icons and UI/Commands/SwapOutfits. Odyssey ships those files; without it this mod must.
Everything here is original vector art drawn with Pillow. Only the canvas size and the visible bounds of each layer
match the game's layout, so clothing and hats line up with the mannequin the way core expects.

Layers are drawn near-white because core multiplies them by the stand's stuff colour (Cutout shader).

    python scripts/draw-no-odyssey-art.py [OUT_DIR] [--sheet SHEET.png]

OUT_DIR defaults to NoOdyssey/Textures. --sheet also writes a review contact sheet on a grey background.
"""
import argparse
import math
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
SS = 8  # supersampling factor
OUTLINE = (38, 36, 34, 255)
LIGHT, MID, DARK = 246, 214, 176  # principal fill values before stuff tint


def canvas(size=128):
    return Image.new('L', (size * SS, size * SS), 0)


def smooth(points, steps=12):
    """Catmull-Rom through closed polygon points (in 128px units), returned in supersampled pixels."""
    out, n = [], len(points)
    for i in range(n):
        p0, p1, p2, p3 = (points[(i + k) % n] for k in (-1, 0, 1, 2))
        for s in range(steps):
            t = s / steps
            t2, t3 = t * t, t * t * t
            out.append(tuple(SS * 0.5 * (2 * p1[j] + (-p0[j] + p2[j]) * t + (2 * p0[j] - 5 * p1[j] + 4 * p2[j] - p3[j]) * t2
                                        + (-p0[j] + 3 * p1[j] - 3 * p2[j] + p3[j]) * t3) for j in (0, 1)))
    return out


def poly_mask(points, size=128):
    m = canvas(size)
    ImageDraw.Draw(m).polygon(smooth(points), fill=255)
    return m


def ellipse_mask(cx, cy, rx, ry, size=128):
    m = canvas(size)
    ImageDraw.Draw(m).ellipse([SS * (cx - rx), SS * (cy - ry), SS * (cx + rx), SS * (cy + ry)], fill=255)
    return m


def shade(mask, light_dir=(-0.6, -0.8), lo=MID, hi=LIGHT, rim=DARK, rim_width=5):
    """Broad two-plane shading: lighter toward light_dir, a darker rim band on the far side."""
    w, h = mask.size
    box = mask.getbbox()
    if not box:
        return Image.new('L', mask.size, 0)
    x0, y0, x1, y1 = box
    cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    half = max(x1 - x0, y1 - y0) / 2
    lx, ly = light_dir
    grad = Image.new('L', (w, h))
    px = grad.load()
    step = SS
    for y in range(y0, y1, step):
        for x in range(x0, x1, step):
            d = ((x - cx) * lx + (y - cy) * ly) / half  # +1 toward light
            v = hi if d > -0.15 else lo
            for yy in range(y, min(y + step, h)):
                for xx in range(x, min(x + step, w)):
                    px[xx, yy] = v
    # far-side rim: mask minus a copy shifted toward the light
    shift = int(rim_width * SS)
    moved = ImageChops.offset(mask, int(lx * shift), int(ly * shift))
    rim_band = ImageChops.subtract(mask, moved)
    grad.paste(rim, mask=rim_band)
    # soften the plane boundaries so they read as form, not seams
    return grad.filter(ImageFilter.GaussianBlur(2.5 * SS))


def outline_of(mask, width=2.2):
    grown = mask.filter(ImageFilter.MaxFilter(int(width * SS) * 2 + 1))
    return grown


def compose(parts, size=128):
    """parts: list of (mask, value_layer) back to front. Returns RGBA with a dark contour around every part."""
    rgba = Image.new('RGBA', (size * SS, size * SS), (0, 0, 0, 0))
    for mask, value in parts:
        ring = outline_of(mask)
        rgba.paste(Image.new('RGBA', rgba.size, OUTLINE), mask=ring)
        fill = Image.merge('RGBA', (value, value, value, Image.new('L', rgba.size, 255)))
        rgba.paste(fill, mask=mask)
    return rgba.resize((size, size), Image.LANCZOS)


def seam(img_l, mask, points, value=DARK, width=1.1):
    """Draw a soft seam line clipped to mask (points in 128px units)."""
    line = canvas(img_l.size[0] // SS)
    ImageDraw.Draw(line).line([(SS * x, SS * y) for x, y in points], fill=255, width=int(width * SS), joint='curve')
    line = ImageChops.multiply(line, mask.filter(ImageFilter.MinFilter(3 * SS + 1)))
    img_l.paste(value, mask=line)


# ---- Base: a low round plinth. Visible bounds follow the game's base layer (x ~34-92, y 87-122).
def draw_base(view):
    cx = 63 if view == 'east' else 64
    top = ellipse_mask(cx, 98, 27, 10)
    body = ImageChops.lighter(ellipse_mask(cx, 110, 27, 10), top)
    side = canvas()
    ImageDraw.Draw(side).rectangle([SS * (cx - 27), SS * 98, SS * (cx + 27), SS * 110], fill=255)
    body = ImageChops.lighter(body, side)
    body_v = Image.new('L', body.size, DARK)
    top_v = shade(top, (-0.3, -1.0), lo=MID, hi=LIGHT, rim=MID, rim_width=3)
    # socket where the pole meets the plinth
    sock = ellipse_mask(cx, 97, 6, 2.5)
    return compose([(body, body_v), (top, top_v), (sock, Image.new('L', sock.size, DARK))])


# ---- Body: a dress-form torso on a short pole. Adult bounds ~x 39-89, y 43-108; child ~x 49-79, y 43-94.
def torso_front(child):
    if child:
        s = [(64, 43), (70, 44), (73, 49), (78, 53), (79, 60), (76, 70), (74, 77), (77, 84), (74, 89), (64, 90),
             (54, 89), (51, 84), (54, 77), (52, 70), (49, 60), (50, 53), (55, 49), (58, 44)]
        pole = (62, 86, 66, 94)
    else:
        s = [(64, 43), (71, 44), (74, 49), (84, 52), (89, 58), (87, 68), (81, 79), (80, 86), (85, 93), (80, 99),
             (64, 101), (48, 99), (43, 93), (48, 86), (47, 79), (41, 68), (39, 58), (44, 52), (54, 49), (57, 44)]
        pole = (61, 97, 67, 108)
    return s, pole


def torso_side(child):
    if child:
        s = [(63, 43), (68, 44), (70, 49), (73, 56), (73, 66), (70, 76), (72, 84), (69, 90), (60, 90), (56, 84),
             (57, 76), (55, 66), (55, 56), (57, 49), (59, 44)]
        pole = (61, 86, 65, 94)
    else:
        s = [(63, 43), (68, 44), (71, 49), (79, 55), (82, 64), (79, 75), (75, 84), (79, 93), (74, 100), (60, 101),
             (50, 97), (49, 89), (53, 80), (50, 70), (48, 60), (51, 52), (57, 48), (59, 44)]
        pole = (60, 97, 66, 107)
    return s, pole


def draw_body(view, child=False):
    shape, (px0, py0, px1, py1) = torso_side(child) if view == 'east' else torso_front(child)
    torso = poly_mask(shape)
    pole = canvas()
    ImageDraw.Draw(pole).rectangle([SS * px0, SS * py0, SS * px1, SS * py1], fill=255)
    light = (-0.5, -0.85) if view != 'north' else (0.5, -0.85)
    tv = shade(torso, light)
    if view == 'south':
        seam(tv, torso, [(64, 50), (64, 96)])
    elif view == 'north':
        seam(tv, torso, [(64, 52), (64, 92)], value=MID)
    else:
        seam(tv, torso, [(53, 60), (60, 61), (68, 60)] if not child else [(57, 62), (64, 63), (71, 62)])
    pv = Image.new('L', pole.size, MID)
    return compose([(pole, pv), (torso, tv)])


# ---- Head: a smooth egg on a neck peg. Bounds ~x 42-86, y 40-89 (east view leans forward).
def draw_head(view):
    if view == 'east':
        egg = poly_mask([(60, 40), (72, 41), (82, 47), (87, 58), (86, 70), (79, 80), (68, 86), (57, 86),
                         (48, 80), (43, 69), (43, 56), (48, 46)])
    else:
        egg = poly_mask([(64, 40), (75, 43), (83, 52), (86, 64), (83, 76), (75, 85), (64, 89), (53, 85), (45, 76),
                         (42, 64), (45, 52), (53, 43)])
    light = (-0.55, -0.8) if view != 'north' else (0.55, -0.8)
    hv = shade(egg, light, rim_width=4)
    # a small specular plane on the lit side, front and side views only
    if view != 'north':
        hl = ellipse_mask(55 if view == 'south' else 66, 53, 6, 4)
        hv.paste(255, mask=ImageChops.multiply(hl, egg))
    return compose([(egg, hv)])


def stand_preview(base, body, head, view, px_per_cell=256 / 2.2, size=256):
    """Compose layers the way Building_OutfitStand.DrawAt does: base 1.4, body 1.2 and head 1.3 cells, all centred
    on the stand, head moved by BodyTypeDef.headOffset (0.04, 0.34) (x mirrored for west, ignored north/south)."""
    big = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    for img, draw_size, (dx, dz) in ((base, 1.4, (0, 0)), (body, 1.2, (0, 0)),
                                     (head, 1.3, (0.04 if view == 'east' else 0, 0.34))):
        side = round(draw_size * px_per_cell)
        layer = img.resize((side, side), Image.LANCZOS)
        big.alpha_composite(layer, ((size - side) // 2 + round(dx * px_per_cell), (size - side) // 2 - round(dz * px_per_cell)))
    return big


def menu_icon(child, view):
    """64px build-menu icon: the in-game composition, cropped and fitted like the game's own icons."""
    big = stand_preview(draw_base(view), draw_body(view, child), draw_head(view), view)
    crop = big.crop(big.getbbox())
    target_h = 58 if not child else 49
    scale = target_h / crop.height
    crop = crop.resize((max(1, round(crop.width * scale)), target_h), Image.LANCZOS)
    out = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    out.alpha_composite(crop, ((64 - crop.width) // 2, 61 - target_h))
    return out


# ---- Swap-outfit command icon: a shirt between two opposing arrows.
def swap_icon():
    shirt = poly_mask([(44, 34), (55, 30), (64, 36), (73, 30), (84, 34), (98, 46), (90, 58), (82, 53), (82, 96),
                       (46, 96), (46, 53), (38, 58), (30, 46)])
    sv = shade(shirt, (-0.5, -0.85), lo=150, hi=190, rim=118)
    collar = poly_mask([(55, 30), (64, 42), (73, 30), (64, 36)])
    arrows = canvas()
    d = ImageDraw.Draw(arrows)
    # upper arrow pointing right, lower arrow pointing left
    d.rectangle([SS * 14, SS * 104, SS * 92, SS * 112], fill=255)
    d.polygon([(SS * 92, SS * 96), (SS * 112, SS * 108), (SS * 92, SS * 120)], fill=255)
    d.rectangle([SS * 36, SS * 14, SS * 114, SS * 22], fill=255)
    d.polygon([(SS * 36, SS * 6), (SS * 16, SS * 18), (SS * 36, SS * 30)], fill=255)
    rgba = compose([(shirt, sv), (collar, Image.new('L', collar.size, 110))])
    ar = compose([(arrows, Image.new('L', arrows.size, 255))])
    # colour the arrows a warm gold and the shirt a muted green; this icon is a UI texture, not stuff-tinted
    ar_rgb = Image.merge('RGBA', [ar.getchannel(c).point(lambda v, k=k: int(v * k)) for c, k in zip('RGB', (0.93, 0.78, 0.40))] + [ar.getchannel('A')])
    sh_rgb = Image.merge('RGBA', [rgba.getchannel(c).point(lambda v, k=k: min(255, int(v * k))) for c, k in zip('RGB', (0.78, 0.92, 0.70))] + [rgba.getchannel('A')])
    out = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    out.alpha_composite(sh_rgb)
    out.alpha_composite(ar_rgb)
    return out


def build():
    files = {}
    stand = 'Things/Building/OutfitStand/'
    for view in ('north', 'east', 'south'):
        files[f'{stand}OutfitStand_Base_{view}.png'] = draw_base(view)
        files[f'{stand}OutfitStand_Body_{view}.png'] = draw_body(view)
        files[f'{stand}OutfitStand_BodyChild_{view}.png'] = draw_body(view, child=True)
        files[f'{stand}OutfitStand_Head_{view}.png'] = draw_head(view)
        files[f'{stand}OutfitStand_MenuIcon_{view}.png'] = menu_icon(False, view)
        files[f'{stand}KidOutfitStand_MenuIcon_{view}.png'] = menu_icon(True, view)
    files['UI/Commands/SwapOutfits.png'] = swap_icon()
    return files


def contact_sheet(files, path):
    names = sorted(files)
    cell = 136
    sheet = Image.new('RGBA', (cell * 7, cell * ((len(names) + 6) // 7)), (92, 92, 92, 255))
    for i, n in enumerate(names):
        im = files[n]
        if im.width == 64:
            im = im.resize((128, 128), Image.NEAREST)
        sheet.alpha_composite(im, ((i % 7) * cell + 4, (i // 7) * cell + 4))
    sheet.save(path)


if __name__ == '__main__':
    ap = argparse.ArgumentParser()
    ap.add_argument('out', nargs='?', type=Path, default=ROOT / 'NoOdyssey' / 'Textures')
    ap.add_argument('--sheet', type=Path)
    args = ap.parse_args()
    files = build()
    for rel, im in files.items():
        p = args.out / rel
        p.parent.mkdir(parents=True, exist_ok=True)
        im.save(p)
    if args.sheet:
        contact_sheet(files, args.sheet)
    print(f'wrote {len(files)} textures to {args.out}')
