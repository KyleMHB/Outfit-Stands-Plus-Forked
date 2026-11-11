# Changelog

## 2026-11-11

### Added

- Tailor's outfit stand and, with Biotech, tailor's kid outfit stand. Unlocked by Complex Clothing, built from wood or metal plus a little cloth, no power. Full outfit stand features (assignment, naming, swap and return, auto-use, group commands) plus mending: colonists who do Tailoring come to the stand and mend the cloth and leather apparel kept on it, spending one cloth or leather per 10 hit points (cheapest textile first, forbidden stacks left alone). Armour, tainted apparel and weapons are ignored. Mending is Tailoring work, ranked after tailoring-bench bills, gives Crafting experience and is scaled by general labour speed.
- Per-stand "Allow mending" toggle and a right-click "Mend clothes" order, which ignores the start threshold and the toggle but respects the "mend up to" limit.
- Mod settings "Start mending below" (default 90%), "Mend up to" (default 100%) and "Remove tailor's stands". The start threshold is always kept below the limit. They do not affect mending stands.
- Swapping, returning and auto-use always win over mending: the tailor yields at once and keeps the hit points already applied and any paid-for but unspent hit points.
- A sewing-kit overlay on the tailor's stands and their own build-menu icons. Both are scripted placeholders (`scripts/draw-tailor-stand-art.py`) until final art replaces them.
- Developer test batch `osp-tailor`, and a `biotech` test mod set.

## 2026-11-09

### Changed

- Odyssey is no longer required. Without Odyssey the mod supplies the plain and kid outfit stands (same defNames as Odyssey's, so saves carry over if Odyssey is added later; the kid stand needs Biotech), the stand jobs, the stand text and its own stand art. Odyssey players load exactly what they did before.
- Without Odyssey, apparel can still be force-worn onto an outfit stand; core only allows that with Odyssey, so a small patch enables it when Odyssey is off.
- The art used without Odyssey is a scripted placeholder (`scripts/draw-no-odyssey-art.py`) until final art replaces it.
- The project now builds against the 1.6 reference assemblies in the NuGet cache, since RimWorld 1.7 no longer ships the Unity managed folder.

### Added

- Developer test batch `osp-stands` (`DevTests/`, run with `python tools/osp_test.py run osp-stands`).

## 2026-10-18

### Fixed

- Fixed adult and child mending stands failing to repair stored gear in RimWorld 1.7. Repair speed, power requirements and saved repair progress are preserved.

## 2026-10-14

### Changed

- Added RimWorld 1.7 compatibility while retaining 1.6 support.

## 2026-05-03

### Added

- Added automatic assigned outfit stand use when a player-controlled pawn's apparel policy changes and the assigned stand has useful policy gear.
- Added automatic assigned stand decisions on undraft, preferring useful stand swaps and returning gear only when the stand has no useful current-policy gear.

### Fixed

- Fixed assigned outfit stand use disabling stand item intake, which could make pawns haul returned gear to other storage after Gear Up and Go.

### Changed

- Skipped automatic stand jobs when Gear Up and Go, stand-use, wear, or apparel-removal jobs are already active or queued.
- Bumped assembly version to `1.0.3.0`.

## 2026-05-02

### Changed

- Renamed the mod branding, namespaces, assembly, and package metadata to Outfit Stands Plus Forked while keeping save-linked defNames and Scribe keys stable.

### Added

- Added a README and Steam Workshop description for the documentation refresh.
- Added `deploy.ps1` to build the source repository and deploy runtime files to the Steam RimWorld mod folder.
- Added custom naming support for armor stands, including automatic names based on the assigned pawn nickname.
- Added rename support for all stand variants: adult, kid, mechanized adult, mechanized kid, mending adult, and mending kid stands.
- Added live apparel policy sync for stands using the union of multiple selected apparel policies.
- Added optional runtime compatibility handling for Gear Up And Go (`jungooji.GearUpAndGo`).
- Added optional runtime compatibility handling for Defensive Positions Forked (`GonDragon.DefensivePositions`).
- Added one shared multi-pawn `Use assigned armor stands` gizmo for selected pawn groups.
- Added right-click group gizmo options to set selected eligible pawns to Swap or Return mode.
- Added invalid minified stand fallback text for dev-spawned `OutfitStandPlusForkedMinified` instances.

### Changed

- Moved the source repository workflow to `E:\Coding\Rimworld\Outfit Stands Plus Forked`, with the Steam RimWorld mod folder treated as a runtime deploy target.
- Stopped tracking generated runtime assemblies in Git; deploy now produces `Assemblies/OutfitStandsPlusForked.dll`.
- Removed the broken missing `postbuild.ps1` project hook.
- Reworked stand gear transfers to use transactional validation before moving apparel or weapons.
- Recomputed transfer plans at execution time so stale apparel changes from other mods are skipped safely.
- Replaced weapon `MakeRoomFor` transfer behavior with explicit remove, add, and rollback handling.
- Stand transfers now skip invalid, missing, forbidden, locked, forced, unworn, unwearable, unstorable, or body-incompatible items instead of dropping or duplicating them.
- Stand acceptance and swap planning now refresh policy-linked apparel filters before use.
- Stand naming, inspect text, and pawn command labels now use the custom or assigned-pawn stand name.
- Armor stand assignment is now limited to one pawn per stand.
- Assigning a pawn to a stand now replaces the stand's existing pawn and removes that pawn from other stands.
- Legacy multi-pawn stand assignments are trimmed on load or spawn.
- Multi-pawn group use preserves each pawn's current Swap or Return mode.
- All shared assignment and command behavior applies through the common stand comps used by the mod-added variants.

### Fixed

- Fixed apparel and weapon swap paths that could leave items on the floor during normal stand use.
- Fixed stale transfer plans when other mods change pawn apparel during a stand job.
- Fixed stand jobs starting while Gear Up And Go or Defensive Positions Forked owns the pawn's current gear or movement flow.
- Fixed compatibility logging so missing optional mods or failed reflection disable the integration without hard errors.
- Fixed dev-spawned `OutfitStandPlusForkedMinified` null `InnerThing` crashes in label, graphic, print, draw, and inspect paths.
- Fixed ineffective max assignment override by setting the assignable comp property count directly.

### Verified

- Built `Source/OutfitStandsPlus/OutfitStandsPlus.csproj` in Release with `0` warnings and `0` errors.
- Verified XML parses after keyed translation updates.
- Confirmed the compiled DLL was deployed to `Assemblies/OutfitStandsPlusForked.dll`.
