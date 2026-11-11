"""Draw the placeholder art for the tailor's outfit stand.

Reuses the helpers (supersampling, dark contour, mannequin menu icons) of draw-no-odyssey-art.py. Everything is original
vector art drawn with Pillow. The maintainer replaces these placeholders; the paths and sizes below must stay as they are.

    Textures/Things/Building/OutfitStand/TailorStand_SewingKit_{north,east,south}.png        128x128 overlay, fixed colours
    Textures/Things/Building/OutfitStand/TailorOutfitStand_MenuIcon_{north,east,south}.png   64x64 build-menu icon
    Textures/Things/Building/OutfitStand/TailorKidOutfitStand_MenuIcon_{north,east,south}.png 64x64 build-menu icon

The overlay is drawn over the stand by TailorOutfitStand.DrawAt at 1.4 x 1.4 cells, Cutout shader, never stuff-tinted: a
small wicker basket with two thread spools and scissors in the front-right corner, beside the plinth (the base spans
roughly x 34-92, y 87-122 of its 128 canvas). West is east mirrored by Graphic_Multi, so no _west file.

    python scripts/draw-tailor-stand-art.py [OUT_DIR] [--sheet SHEET.png]

OUT_DIR defaults to Textures. --sheet also writes a review contact sheet on a grey background.
"""
import argparse
import importlib.util
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]

# The existing script has a hyphen in its name, so load it by path.
_spec = importlib.util.spec_from_file_location('draw_no_odyssey_art', Path(__file__).with_name('draw-no-odyssey-art.py'))
base = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(base)
SS, OUTLINE = base.SS, base.OUTLINE

WICKER, WICKER_DARK, WICKER_LIGHT = (196, 152, 92), (150, 108, 60), (224, 186, 124)
RED, BLUE, WOOD = (202, 62, 54), (62, 104, 192), (176, 136, 84)
STEEL, STEEL_DARK, HANDLE = (206, 210, 214), (140, 146, 152), (214, 138, 52)


def poly(points):
    """Straight-edged polygon mask (points in 128px units)."""
    m = base.canvas()
    ImageDraw.Draw(m).polygon([(SS * x, SS * y) for x, y in points], fill=255)
    return m


def rect(x0, y0, x1, y1):
    m = base.canvas()
    ImageDraw.Draw(m).rectangle([SS * x0, SS * y0, SS * x1, SS * y1], fill=255)
    return m


def ring(cx, cy, rx, ry, thick):
    return ImageChops.subtract(base.ellipse_mask(cx, cy, rx, ry), base.ellipse_mask(cx, cy, rx - thick, ry - thick))


def clip(mask, other):
    return ImageChops.multiply(mask, other)


def stripes(mask, ys, x0, x1, color, width=1.0):
    """Horizontal weave lines clipped to mask, returned as (mask, colour) for compose."""
    lines = base.canvas()
    d = ImageDraw.Draw(lines)
    for y in ys:
        d.line([(SS * x0, SS * y), (SS * x1, SS * y)], fill=255, width=int(width * SS))
    return clip(lines, mask), color


def compose_rgb(parts, size=128):
    """parts: (mask, (r, g, b)) back to front, each with a dark contour; light top edge for a little form."""
    rgba = Image.new('RGBA', (size * SS, size * SS), (0, 0, 0, 0))
    for mask, color, *flags in parts:
        if not (flags and flags[0] == 'nooutline'):
            rgba.paste(Image.new('RGBA', rgba.size, OUTLINE), mask=base.outline_of(mask, 1.6))
        rgba.paste(Image.new('RGBA', rgba.size, color + (255,)), mask=mask)
    return rgba.resize((size, size), Image.LANCZOS)


def basket_parts(cx=104, cy=110):
    """Basket, two spools and scissors, centred about (cx, cy) in 128px units. Back to front."""
    parts = []
    # scissors behind the basket rim, leaning right
    blade_a = poly([(cx + 5, cy - 6), (cx + 7.5, cy - 7), (cx + 14, cy - 27), (cx + 12, cy - 28)])
    blade_b = poly([(cx + 14, cy - 6), (cx + 11.5, cy - 7), (cx + 5, cy - 27), (cx + 7, cy - 28)])
    parts += [(blade_a, STEEL), (blade_b, STEEL_DARK)]
    parts += [(ring(cx + 6, cy - 3, 3, 3, 1.4), HANDLE), (ring(cx + 13, cy - 3, 3, 3, 1.4), HANDLE)]
    # two spools standing in the basket: wood caps, thread between
    for sx, thread in ((cx - 6, RED), (cx + 1, BLUE)):
        parts.append((rect(sx - 3.2, cy - 17, sx + 3.2, cy - 5), WOOD))
        parts.append((rect(sx - 2.4, cy - 15, sx + 2.4, cy - 7), thread))
        parts.append((rect(sx - 3.2, cy - 17, sx + 3.2, cy - 15.6), WOOD, 'nooutline'))
        parts.append((rect(sx - 3.2, cy - 6.4, sx + 3.2, cy - 5), WOOD, 'nooutline'))
    # basket body (tapered) with a rim and weave lines
    body = poly([(cx - 12, cy - 6), (cx + 12, cy - 6), (cx + 9, cy + 9), (cx - 9, cy + 9)])
    body_back = base.ellipse_mask(cx, cy + 9, 9, 2.6)
    parts.append((ImageChops.lighter(body, body_back), WICKER))
    parts.append(stripes(body, [cy - 1, cy + 3, cy + 7], cx - 14, cx + 14, WICKER_DARK))
    rim = base.ellipse_mask(cx, cy - 6, 12.5, 3.4)
    parts.append((rim, WICKER_LIGHT))
    parts.append((base.ellipse_mask(cx, cy - 6, 9.5, 2.0), WICKER_DARK, 'nooutline'))
    return parts


def draw_overlay(view):
    """128px sewing-kit overlay: only the basket group, in the front-right corner beside the plinth."""
    cx = 97 if view != 'east' else 95  # tucked against the plinth's right edge (moved in 8 px after in-game review)
    cy = 108 if view != 'north' else 104  # seen from behind the kit sits a little further back
    return compose_rgb(basket_parts(cx, cy))


def menu_icon(child, view):
    """The existing stand icon with a smaller basket in the lower-right corner."""
    icon = base.menu_icon(child, view)
    kit = draw_overlay('south')
    crop = kit.crop(kit.getbbox())
    scale = 24 / crop.height
    crop = crop.resize((max(1, round(crop.width * scale)), 24), Image.LANCZOS)
    out = icon.copy()
    out.alpha_composite(crop, (64 - crop.width - 1, 64 - 24 - 2))
    return out


def build():
    files = {}
    stand = 'Things/Building/OutfitStand/'
    for view in ('north', 'east', 'south'):
        files[f'{stand}TailorStand_SewingKit_{view}.png'] = draw_overlay(view)
        files[f'{stand}TailorOutfitStand_MenuIcon_{view}.png'] = menu_icon(False, view)
        files[f'{stand}TailorKidOutfitStand_MenuIcon_{view}.png'] = menu_icon(True, view)
    return files


def contact_sheet(files, path):
    """Each overlay is shown composed over the stand base/body/head, as in game, beside the menu icons."""
    names = sorted(files)
    cell = 136
    sheet = Image.new('RGBA', (cell * 3, cell * 3), (92, 92, 92, 255))
    for i, n in enumerate(names):
        im = files[n]
        if 'SewingKit' in n:
            view = n.rsplit('_', 1)[1][:-4]
            tint = lambda im_: Image.merge('RGBA', [im_.getchannel(c).point(lambda v: int(v * 0.72)) for c in 'RGB'] + [im_.getchannel('A')])
            stack = base.stand_preview(tint(base.draw_base(view)), tint(base.draw_body(view)), tint(base.draw_head(view)), view)
            kit = im.resize((round(1.4 * 256 / 2.2),) * 2, Image.LANCZOS)
            stack.alpha_composite(kit, ((256 - kit.width) // 2, (256 - kit.height) // 2))
            im = stack.resize((128, 128), Image.LANCZOS)
        elif im.width == 64:
            im = im.resize((128, 128), Image.NEAREST)
        sheet.alpha_composite(im, ((i % 3) * cell + 4, (i // 3) * cell + 4))
    sheet.save(path)


if __name__ == '__main__':
    ap = argparse.ArgumentParser()
    ap.add_argument('out', nargs='?', type=Path, default=ROOT / 'Textures')
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
