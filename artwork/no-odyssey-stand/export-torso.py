# Export the regenerated torsos (torso-regenerated/) into the six OutfitStand_Body* game textures. Run from the project root.
import sys, importlib.util
sys.dont_write_bytecode = True
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter
root = Path.cwd()
src = root / 'artwork/no-odyssey-stand/torso-regenerated'
dst = root / 'NoOdyssey/Textures/Things/Building/OutfitStand'
lut = [v if v <= 45 else round(45+(v-45)*171/89) if v <= 134 else round(216+(v-134)*14/46) if v <= 180 else min(245, round(230+(v-180)*15/50)) for v in range(256)]
def fit(im, area):
    a = im.getchannel('A'); im = im.crop(a.point(lambda v: 255 if v > 16 else 0).getbbox())
    x, y, w, h = area; s = min(w/im.width, h/im.height)
    im = im.resize((round(im.width*s), round(im.height*s)), Image.Resampling.LANCZOS)
    out = Image.new('RGBA', (128, 128)); out.alpha_composite(im, (round(x+(w-im.width)/2), y)); return out
def finish(im):
    g = im.convert('L').point(lut); im = Image.merge('RGBA', (g, g, g, im.getchannel('A')))
    alpha = im.getchannel('A'); ring = ImageChops.subtract(alpha, alpha.filter(ImageFilter.MinFilter(5)))
    rgb = im.convert('RGB'); rgb.paste((35, 35, 35), mask=ring); rgb.putalpha(alpha); return rgb
for view in ('south', 'east', 'north'):
    adult = fit(Image.open(src/f'adult-{view}.png').convert('RGBA'), (39, 43, 50, 65))
    kid = fit(Image.open(src/f'child-{view}.png').convert('RGBA'), ((46 if view == 'east' else 49), 43, (36 if view == 'east' else 30), 52))
    finish(adult).save(dst/f'OutfitStand_Body_{view}.png'); finish(kid).save(dst/f'OutfitStand_BodyChild_{view}.png')
    print(view, 'adult', adult.getbbox(), 'kid', kid.getbbox())

# Rebuild the six build-menu icons from the new torsos, as export.py composes them.
spec = importlib.util.spec_from_file_location('stand', root/'scripts/draw-no-odyssey-art.py'); stand = importlib.util.module_from_spec(spec); spec.loader.exec_module(stand)
for view in ('south', 'east', 'north'):
    layer = lambda kind: Image.open(dst/f'OutfitStand_{kind}_{view}.png').convert('RGBA')
    for kid in (False, True):
        preview = stand.stand_preview(layer('Base'), layer('BodyChild' if kid else 'Body'), layer('Head'), view)
        crop = preview.crop(preview.getbbox()); h = 49 if kid else 58
        crop = crop.resize((round(crop.width*h/crop.height), h), Image.Resampling.LANCZOS)
        icon = Image.new('RGBA', (64, 64)); icon.alpha_composite(crop, ((64-crop.width)//2, 61-h))
        icon.save(dst/f'{"Kid" if kid else ""}OutfitStand_MenuIcon_{view}.png')
