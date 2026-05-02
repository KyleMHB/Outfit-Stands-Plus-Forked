# Outfit Stands Plus Forked Changes

Always update this file when changing the mod.

## Unreleased

### Added
- Added `deploy.ps1` to build the source repository and deploy runtime files to the Steam RimWorld mod folder.
- Added custom naming support for armor stands, including automatic names based on the assigned pawn nickname.
- Added rename support for all stand variants: adult, kid, mechanized adult, mechanized kid, mending adult, and mending kid stands.
- Added live apparel policy sync for stands using the union of multiple selected apparel policies.
- Added optional runtime compatibility handling for Gear Up And Go (`jungooji.GearUpAndGo`).
- Added optional runtime compatibility handling for Defensive Positions Forked (`GonDragon.DefensivePositions`).
- Added one shared multi-pawn `Use assigned armor stands` gizmo for selected pawn groups.
- Added right-click group gizmo options to set selected eligible pawns to Swap or Return mode.
- Added invalid minified stand fallback text for dev-spawned `OutfitStandPlusMinified` instances.

### Changed
- Moved the source repository workflow to `E:\Coding\Rimworld\Outfit Stands Plus Forked`, with the Steam RimWorld mod folder treated as a runtime deploy target.
- Stopped tracking generated runtime assemblies in Git; deploy now produces `Assemblies/OutfitStandsPlus.dll`.
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
- Fixed dev-spawned `OutfitStandPlusMinified` null `InnerThing` crashes in label, graphic, print, draw, and inspect paths.
- Fixed ineffective max assignment override by setting the assignable comp property count directly.

### Verified
- Built `Source/OutfitStandsPlus/OutfitStandsPlus.csproj` in Release with `0` warnings and `0` errors.
- Verified XML parses after keyed translation updates.
- Confirmed the compiled DLL was deployed to `Assemblies/OutfitStandsPlus.dll`.
