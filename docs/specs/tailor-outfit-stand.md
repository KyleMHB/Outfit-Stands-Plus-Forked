# Spec: Tailor's outfit stand

Status: ready-for-agent. Agreed with the maintainer on 2026-11-11 through a design interview. No implementation yet.

## Problem Statement

Outfit stands make changing gear quick, but the only stand that repairs what it holds is the powered Mending stand. It needs spacer research (Fabrication), advanced components and electricity. A medieval or early colony has no way to keep its stored outfits in good condition: worn clothes slowly lose hit points and have to be thrown away and remade at a tailoring bench, even when only lightly damaged. Players want an early, low-tech stand that lets their tailors patch up the clothes kept on it as ordinary work.

## Solution

Add a **tailor's outfit stand**, plus a **tailor's kid outfit stand** when Biotech is active, unlocked by the Medieval research Complex Clothing and needing no power. It is a full outfit stand with every feature the mod's other stands have. In addition, colonists who do Tailoring automatically come to it and mend the textile apparel stored on it, spending a little cloth or leather as they go. Players control when mending starts and how far it goes with two mod settings. Each stand has an on/off toggle, and the player can order an immediate mend from the right-click menu. Mending yields at once to getting dressed: swapping or auto-use always wins, and no progress or paid material is lost.

## User Stories

1. As a medieval-era player, I want a stand that repairs stored clothes without electricity, so that I can keep outfits usable before I have power.
2. As a player, I want the tailor's stand unlocked by Complex Clothing, so that it arrives at the point in the tech tree where tailoring becomes a focus.
3. As a player, I want to build the stand from wood or metal plus a little cloth, so that it is cheap enough for an early colony.
4. As a player with Biotech, I want a kid version of the tailor's stand, so that children's outfits can be mended too.
5. As a player without Biotech, I want the kid version not to appear, so that I don't see a building I can't use.
6. As a player, I want the tailor's stand to work whether or not Odyssey is active, so that it follows the rest of the mod.
7. As a player, I want the tailor's stand to do everything a normal stand of this mod does (pawn assignment, naming, swap and return, auto-use on policy change and on draft/undraft, group gizmos), so that choosing it costs me no stand features.
8. As a player, I want my Tailoring colonists to mend clothes on the stand automatically, so that I don't have to micromanage repairs.
9. As a player, I want mending to count as Tailoring work, so that I control who does it from the Work tab I already use.
10. As a player, I want automatic mending to come after tailoring-bench bills, so that clothing production isn't starved by repairs.
11. As a player, I want only textile apparel (cloth- or leather-made) to be mended here, so that a primitive stand doesn't repair flak or power armour.
12. As a player, I want tainted apparel to be left alone, so that mending doesn't pretend to remove the corpse taint.
13. As a player, I want weapons held on the stand to be ignored by mending, so that the stand stays a clothing repair station.
14. As a player, I want mending to start only when an item falls below a "start mending below" percentage (default 90%), so that tailors don't walk over for tiny scratches.
15. As a player, I want mending to stop at a configurable "mend up to" percentage (default 100%), so that I can choose cheaper partial repairs, for example 80%.
16. As a player, I want the two percentages to never conflict (the start threshold always stays below the limit), so that I can't create settings that break mending.
17. As a player, I want the "mend up to" setting to apply only to the tailor's stand, so that my existing powered Mending stands behave exactly as before.
18. As a player, I want hit points restored step by step while the tailor works (one HP roughly every half second at normal work speed), so that an interrupted job keeps what was already repaired.
19. As a player, I want better and faster workers to mend faster (scaled by general labour speed, the same stat as tailoring benches), so that skill matters.
20. As a player, I want my tailors to gain Crafting experience from mending, so that the work trains them.
21. As a player, I want mending never to fail, lower quality or reduce maximum hit points, so that the cost of mending is predictable.
22. As a player, I want mending to cost one textile per 10 hit points restored, so that repairs have a fair price.
23. As a player, I want the tailor to use the cheapest textile available, so that my hyperweave and devilstrand aren't burned on patches.
24. As a player, I want forbidden textiles to be left alone, so that forbidding still protects my materials.
25. As a player, I want the cost charged at the start of each 10-HP block, with any unused part of a paid block kept for the next visit, so that interrupted jobs neither get free HP nor waste material.
26. As a player, I want one visit to mend every eligible item on the stand, most damaged first, so that the whole outfit is repaired in one trip.
27. As a player, I want the tailor to fetch enough material for the whole stand in one go, so that they don't make repeated trips.
28. As a player, I want leftover carried material to be dropped near the stand when the job ends, so that nothing disappears.
29. As a player, I want the stand to say it needs cloth when no usable textile is reachable, so that I understand why nobody is mending.
30. As a player, I want only one tailor at a time working on a stand, so that colonists don't crowd it or duplicate work.
31. As a player, I want the tailor to work from the stand's interaction cell, or from any adjacent cell when the "Remove interaction cells" setting is on, so that placement rules match the other stands.
32. As a player, I want a per-stand mending on/off toggle (on by default), so that I can keep some stands as display or storage only.
33. As a player, I want the mending toggle kept separate from storage settings and storage groups, so that copying filters doesn't change mending.
34. As a player, I want to right-click a tailor's stand with a colonist selected and order "Mend clothes", so that I can repair an outfit right now before a raid or caravan.
35. As a player, I want that order to ignore the start threshold and the toggle but still respect the "mend up to" limit, so that my explicit order does what I mean without exceeding my chosen limit.
36. As a player, I want getting dressed (manual swap, draft/undraft auto-use, policy-change auto-use) to interrupt mending immediately, so that preparing for combat never waits on repairs.
37. As a player, I want interrupted mending to resume later on its own, so that I don't have to reissue anything.
38. As a player, I want a "Remove tailor's stands" setting like the existing remove settings, so that I can hide the building if I don't want it.
39. As a player, I want the start and limit percentages in the mod settings window, so that I can tune mending for my playstyle.
40. As a player, I want to tell the tailor's stand apart on the map by a small sewing-kit overlay beside its base, so that I can find it among identical mannequins.
41. As a player, I want the tailor's stand to have its own build-menu icon, so that I can pick it quickly in the Furniture tab.
42. As a player, I want saves with tailor's stands to keep stored paid-HP progress and toggle state across save and load, so that nothing is lost.
43. As a player, I want existing saves to keep working when this update arrives, so that adding the feature is safe mid-colony.
44. As the maintainer, I want placeholder art with documented paths, sizes and image-generation prompts, so that I can produce the final art with Codex/imagegen and drop it in.
45. As the maintainer, I want automated in-game tests for the mending rules, so that regressions show up before a Workshop release.

## Implementation Decisions

- **New buildings.** An adult and a kid tailor's stand, defNames `OutfitStandsPlus_TailorOutfitStand` and `OutfitStandsPlus_TailorKidOutfitStand` (the kid one only with Biotech). Labels "tailor's outfit stand" and "tailor's kid outfit stand". They derive from the shared outfit stand base (Odyssey's, or the mod's NoOdyssey copy) in the same way as the Mechanized stands. They carry the same mod comps as the Mechanized stands (main comp, stand comp, adult or kid assignable comp), but no power comp and no equip-speed bonus. Research prerequisite: Complex Clothing. Stuff categories Woody and Metallic, about 50 stuff plus about 10 cloth, construction skill about 4. Interaction cell at (0,0,1).
- **Mending state lives on the stand.** A new comp, separate from the powered Mending stand's tick-based handler, holds: the per-stand mending toggle (default on), and the stored paid HP (HP paid for but not yet applied). Both are saved. The powered Mending stand and its settings are not changed.
- **Eligibility rule (one shared query).** An item on a tailor's stand is eligible when it is apparel, not tainted, made from a stuff in the Fabric or Leathery categories, and below the "mend up to" limit. For automatic work, at least one item must also be below the "start mending below" threshold. A forced order skips the threshold and the toggle. The work giver, the job driver, the inspect string and the tests all use the same query.
- **Work giver.** A new scanner work giver in the Tailoring work type, ordered after the tailoring-bench bill giver. It offers a job for a tailor's stand that is spawned, has mending toggled on, has an eligible item below threshold, isn't reserved, is reachable at its interaction cell (or adjacent touch cell when interaction cells are removed), and has enough reachable, non-forbidden textile for at least one unpaid block, unless stored paid HP already covers some. It picks the cheapest textile by unit market value.
- **Job driver.** Reserves the stand. Fetches textile enough for the whole stand's remaining need (sum over eligible items of HP to the limit, minus stored paid HP, rounded up to 10-HP blocks, capped by what one trip can carry). Goes to the interaction cell. Then loops over eligible items, most damaged first:
  - When stored paid HP is zero, it consumes one textile and adds 10 paid HP.
  - Every 30 ticks divided by the pawn's General labour speed, it moves 1 paid HP onto the item.
  - It gives Crafting XP comparable to tailoring.
  - It stops an item at the limit and moves on.
  - The job ends when nothing is eligible or no material remains. Carried leftovers are dropped.
- **No failure or side effects.** Quality and max HP are never changed.
- **Interruption.** The mod's swap, return and auto-use paths and the vanilla stand use job must be able to take the stand from a mending job: mending is interruptible and must not block them. HP already applied and stored paid HP persist.
- **Player controls.**
  - A per-stand gizmo toggles mending.
  - A float-menu option "Mend clothes" on a tailor's stand starts a forced mending job for the selected colonist. It is shown disabled with a reason when nothing is below the limit, no textile is available, or the colonist can't do Tailoring.
  - The inspect string shows the mending state and "needs cloth" when blocked by material.
- **Settings.**
  - "Start mending below" (default 90%) and "Mend up to" (default 100%); start is kept below the limit.
  - "Remove tailor's stands", hiding the buildings like the existing remove settings.
  - These are new keys in the existing settings class; defaults apply to existing installs.
- **Art.** The mod overrides drawing for the tailor's stands to add a sewing-kit overlay beside the base for every rotation, with or without Odyssey, after the base/body layers. The two menu icons are dedicated textures. Placeholder art is generated by script, following the existing no-Odyssey placeholder workflow. A prompts document gives Codex/imagegen prompts in the maintainer's art-guide scaffold (canvas, framing, transparency, which parts are near-white and stuff-tinted versus fixed colour). The README records the texture paths and sizes.
- **Localisation.** English keyed strings for the gizmo, float menu, inspect lines, settings and job report string.
- **Compatibility.** The new defs are additive, so existing saves are unaffected. The stand participates in the mod's existing stand discovery (assignment, group commands, auto-use) through the shared `Building_OutfitStand` class hierarchy.

## Testing Decisions

- **One seam:** the in-game `osp-stands` DevTests batch, run by the shared RimWorldDevTests launcher on a disposable colony. No unit tests: the project rule is not to unit-test against the stripped RimWorld reference assemblies.
- **Good tests observe behaviour only:** item hit points, textile stack counts on the map, which items were touched, whether a job happened or was interrupted, and saved state after reload. They don't reach into private fields of the job driver. Stored paid HP may be read through the stand comp's public surface, because it is player-relevant state that survives save and load.
- **Cases** (all Mechanical level: god-mode placement, developer-spawned apparel, cloth and tailor; the game's own work giver chooses the job):
  - only textile apparel below the limit is mended; a flak vest and a tainted shirt are untouched;
  - automatic mending starts only below the start threshold, and stops at a non-default limit (80%);
  - material is charged per 10-HP block from the cheapest textile, and a forbidden stack is not used;
  - interrupting the job keeps applied HP and stored paid HP, and the next job uses the stored HP before charging again;
  - toggle off prevents automatic jobs, but a forced order still mends;
  - a swap or auto-use job takes the stand from a mending tailor;
  - tailor's stand state survives save and reload.
- **Run** in each mod set the batch already targets: Harmony + Core + this mod with Biotech on and off, and with Odyssey. Plus a manual in-game look at the overlay and menu icons.
- **Prior art:** the existing `osp-stands` cases (use and return jobs on plain and mechanized stands, reload segment) and the Adaptive Storage Framework Wealth Adjuster `sfwa-wealth` batch.

## Out of Scope

- Changing the powered Mending stand (no material cost, no limit setting, no textile filter).
- Mending armour, non-textile apparel, weapons or worn (not stored) apparel.
- Removing taint, raising quality or changing max HP.
- A per-stand material filter or per-stand threshold overrides.
- Matching the garment's own stuff as the repair material.
- A new work type or Work tab column.
- Final production art (the maintainer will generate it; placeholders ship until then).
- Translations beyond English.
- Steam publication.

## Further Notes

- Outfit stand layers are drawn by core from fixed texture paths, so the tailor's stand mannequin looks like any other stand. The overlay and menu icon are the only visual distinction.
- The kid tailor's stand needs Biotech. Unlike the existing Mechanized and Mending kid stands, it must not load without Biotech.
- Odyssey became optional on 2026-11-09 (NoOdyssey load folder). The tailor's stand must not depend on Odyssey content.
- The in-game run of the existing `osp-stands` batch is still pending at the time of writing; run it before or together with this feature's cases.
