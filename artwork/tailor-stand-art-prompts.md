# Codex art prompts: Outfit Stands Plus Forked

For Codex with its built-in image generator (`image_gen.imagegen`). Each section is a complete prompt to paste in. Follow the user's art workflow (`E:\Coding\Templates\Steam\rimworld\rimworld-art-guide.md`): first generate one master for approval, then derive the other views from the approved master, then export. Replace the placeholder PNGs at exactly the same paths and sizes. Record each prompt, the selected source and its hash next to the art, under `artwork/`.

Shared style (already part of each prompt below): RimWorld object sprite, top-down three-quarter camera like vanilla furniture, dark charcoal exterior contour (about 2 px at 128 px), a few broad flat shading planes with smooth antialiasing, no photographic lighting, no texture noise, genuine transparent background, complete object, nothing cropped.

---

## 1. Tailor's stand sewing kit overlay (fixed colours)

```text
Work in E:\Coding\Rimworld\Outfit Stands Plus Forked. Read E:\Coding\Templates\Steam\rimworld\rimworld-art-guide.md first and follow its workflow: approve one master, derive the other views, then export.

Make the sewing-kit overlay for the mod's "tailor's outfit stand". The game draws it on top of a vanilla outfit stand: a mannequin on a round plinth. It is the only thing that tells this stand apart on the map, so it must read clearly at normal zoom (about 64 px per map cell).

Object: a small round wicker sewing basket with a lid tipped open, two thread spools (one red, one blue) and a pair of iron scissors resting against it. Warm wicker brown, coloured thread, dark iron. These are fixed colours; nothing is tinted by the game.

RimWorld object sprite matching vanilla furniture: top-down three-quarter camera, dark charcoal exterior and inter-material contour about 2 px at 128 px, a few broad flat shading planes per material with smooth antialiasing, no photographic lighting, no noise, genuine transparency.

Framing (final export): 128x128 PNG canvas, drawn by the game at 1.4x1.4 map cells centred on the stand. The basket sits in the FRONT-RIGHT corner, beside the plinth and not over it: visible bounds about x 88-122, y 90-124. The plinth occupies about x 34-92, y 87-122, and the mannequin rises from the centre. Leave everything else transparent.

Views: generate the south (front) view as the master first and show it to me for approval. After approval, derive north (basket seen from behind, now in the front-left from the camera's side, i.e. mirror the placement: bounds about x 6-40, y 90-124) and east (seen from the side, placed front-right) as views of the same physical basket. Same parts, counts, proportions and palette; repaint for the view, don't just rotate.

Export: generate large (1024 px or more), then crop to visible pixels, scale uniformly with one reduction, and place into the 128x128 canvas at the bounds above. Overwrite:
  Textures/Things/Building/OutfitStand/TailorStand_SewingKit_south.png
  Textures/Things/Building/OutfitStand/TailorStand_SewingKit_north.png
  Textures/Things/Building/OutfitStand/TailorStand_SewingKit_east.png
Then make a review sheet that composites each view over the stand placeholder (NoOdyssey/Textures/Things/Building/OutfitStand/OutfitStand_Base_*.png) at 1x and 2x on a grey background. Record the prompt, the selected source, its hash and the export recipe under artwork/tailor-stand/.
```

---

## 2. Tailor's stand build-menu icons (adult and kid)

```text
Work in E:\Coding\Rimworld\Outfit Stands Plus Forked. Use the approved sewing-kit art from artwork/tailor-stand/ as the reference for the basket. Follow E:\Coding\Templates\Steam\rimworld\rimworld-art-guide.md.

Make the build-menu icons for "tailor's outfit stand" and "tailor's kid outfit stand". Each shows a featureless dress-form mannequin (egg-shaped head, torso, short pole, round low plinth) with the approved wicker sewing basket at its front-right foot. The kid version has the same head and plinth but a smaller, narrower torso and a shorter pole.

The mannequin and plinth are near-white light grey (the game multiplies the map version by the building material's colour; the icon should read as a neutral light mannequin). The basket keeps its fixed colours. RimWorld style: dark charcoal contour about 1 px at 64 px, broad flat shading, smooth antialiasing, genuine transparency.

Framing: 64x64 PNG. Adult: whole object visible bounds about x 14-50, y 3-61, basket included. Kid: about x 14-50, y 12-61. Centred horizontally, standing on the bottom edge area.

Views: south (front) master first, then show me. After approval, derive north (back) and east (side, facing right) of the same object.

Export: generate large, crop, scale uniformly once, place into 64x64. Overwrite:
  Textures/Things/Building/OutfitStand/TailorOutfitStand_MenuIcon_south.png / _north.png / _east.png
  Textures/Things/Building/OutfitStand/TailorKidOutfitStand_MenuIcon_south.png / _north.png / _east.png
Review them at 64 px and at the build-menu size (about 54 px) beside the existing stand icons in NoOdyssey/Textures/Things/Building/OutfitStand/. Record prompt, source, hash and recipe under artwork/tailor-stand/.
```

---

## 3. Outfit stand mannequin art for players without Odyssey (stuff-tinted)

These replace the scripted placeholders in `NoOdyssey/Textures`. They load only when Odyssey is off. The base game draws them as separate layers at fixed paths, so every layer must keep its exact canvas and visible bounds, or clothing and hats won't sit on the mannequin. Don't use or imitate Odyssey's art; this must be an original design.

```text
Work in E:\Coding\Rimworld\Outfit Stands Plus Forked. Read E:\Coding\Templates\Steam\rimworld\rimworld-art-guide.md and follow it. Look at the current placeholders in NoOdyssey/Textures/Things/Building/OutfitStand/ and the recipe scripts/draw-no-odyssey-art.py: they define the exact framing you must keep. Do not use or reference Ludeon's Odyssey outfit stand art.

Make an original outfit stand mannequin for RimWorld: a tailor's dress form. Egg-shaped featureless head, a torso with gentle shoulders, waist and hips, a short pole and a low round plinth. ALL parts are near-white light grey (principal values about 246 light, 214 mid, 176 shadow), because the game multiplies every layer by the building material's colour (wood, steel, stone). No fixed colours. Dark charcoal contour about 2 px at 128 px, broad flat shading planes, smooth antialiasing, top-down three-quarter camera like vanilla furniture, genuine transparency.

The game draws four separate layers, each a 128x128 PNG in its own canvas, centred on the stand:
  Base (plinth), drawn 1.4 cells: visible bounds about x 34-92, y 87-122.
  Body (adult torso + pole), drawn 1.2 cells: about x 39-89, y 43-108 (south/north); about x 43-86 (east, side profile, chest toward the right).
  BodyChild (smaller torso + pole), drawn 1.2 cells: about x 49-79, y 43-94; east about x 46-82.
  Head (egg), drawn 1.3 cells and raised 0.34 cells above the stand centre: about x 42-86, y 40-89; east about x 39-89 (leans toward the right).
Clothing is drawn over the body and hats over the head, so keep the torso within its bounds and the neck at the top centre.

Steps: design the complete mannequin (south view) as one master and show me. After approval, derive north (back) and east (side, facing right) as views of the same object. Then split each view into its four layers along real contours, so the layers composite back into the approved design. Use stand_preview() in scripts/draw-no-odyssey-art.py to composite and check.

Also make the two 64x64 build-menu icons (the composed mannequin, adult and kid) and the 128x128 command icon UI/Commands/SwapOutfits (a shirt with two opposing swap arrows; fixed colours allowed for this one).

Export each file at its exact path and size, overwriting the placeholders (19 files):
  NoOdyssey/Textures/Things/Building/OutfitStand/OutfitStand_{Base,Body,BodyChild,Head}_{north,east,south}.png
  NoOdyssey/Textures/Things/Building/OutfitStand/{OutfitStand,KidOutfitStand}_MenuIcon_{north,east,south}.png
  NoOdyssey/Textures/UI/Commands/SwapOutfits.png
Make a review sheet with each view composited (adult and kid), tinted with wood brown, steel grey and marble white, at 1x and 2x. Record prompts, sources, hashes and recipe under artwork/no-odyssey-stand/.
```
