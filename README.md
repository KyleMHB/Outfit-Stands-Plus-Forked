# Outfit Stands Plus Forked

Outfit Stands Plus Forked is a RimWorld 1.6 and 1.7 mod that makes outfit stands easier to use and safer to integrate with other gear-flow mods.

It keeps stand naming, assignment, and apparel handling tied to the assigned pawn or custom stand identity while avoiding duplicate or unsafe transfers.

## Features

- **Custom stand naming** based on pawn nicknames or explicit names.
- **Rename support** for adult, kid, mechanized, and mending stand variants.
- **Shared multi-pawn gizmos** for using assigned stands with selected groups.
- **Live apparel policy sync** across selected apparel policies.
- **Safer gear transfer validation** to avoid dropping or duplicating items.
- **Optional integration** with Gear Up And Go and Defensive Positions Forked.
- **Single-pawn stand assignment** to keep ownership clear and predictable.

## Installation

### Steam Workshop

Subscribe on Steam Workshop and enable **Harmony** and **Outfit Stands Plus Forked** in RimWorld's mod list. Odyssey is optional.

### Manual Installation

1. Download or clone the repository.
2. Place the mod folder in your RimWorld `Mods` directory.
3. Enable **Harmony** and **Outfit Stands Plus Forked**.

## Odyssey

Odyssey is optional. Outfit stands come from Odyssey, but their code is part of the base game, so without Odyssey the `NoOdyssey` folder (loaded by `LoadFolders.xml` only when Odyssey is off) supplies:

- the plain `Building_OutfitStand` and, with Biotech, `Building_KidOutfitStand` (same defNames as Odyssey, so saves keep their stands if Odyssey is added later)
- the `UseOutfitStand` and `PutApparelOnOutfitStand` jobs and the stand text
- stand art at the texture paths the base game draws (`Things/Building/OutfitStand/*`, `UI/Commands/SwapOutfits`)

A Harmony patch, applied only without Odyssey, lets apparel be force-worn onto a stand; the base game allows that only with Odyssey. With Odyssey active none of this loads, and the stands use Odyssey's defs and art as before.

The current no-Odyssey art is a placeholder drawn by `python scripts/draw-no-odyssey-art.py`. To replace it, overwrite the PNGs under `NoOdyssey/Textures` at the same paths and sizes (128x128 stand layers, 64x64 menu icons, 128x128 command icon). Stand layers are multiplied by the stuff colour, so keep them near-white.

## Usage

Use the stand gizmos in RimWorld to assign a pawn, rename a stand, or trigger the stand's gear flow.

When multiple pawns are selected, the shared gizmo can apply the stand action to the selected eligible pawns.

## Configuration

The mod uses RimWorld's mod settings and the stand gizmos themselves.

Important behavior:

- stand names follow the assigned pawn unless you rename them manually
- invalid or unsafe gear transfers are skipped instead of forcing a bad swap
- optional integrations are only active when the other mod is loaded

## Building from Source

Prerequisites:

- .NET SDK compatible with the project
- RimWorld reference files and Harmony available at the configured paths

Build the project from the repository root:

```powershell
dotnet build .\Source\OutfitStandsPlus\OutfitStandsPlus.csproj -c Release
```

The project outputs to `Assemblies\OutfitStandsPlusForked.dll`.

## Testing and Validation

The primary validation command is the Release build:

```powershell
dotnet build .\Source\OutfitStandsPlus\OutfitStandsPlus.csproj -c Release
```

The project builds against `Krafs.Rimworld.Ref` 1.6.4871 and the .NET Framework 4.7.2 reference assemblies from the local NuGet cache; override `RimWorldManagedPath` or `FrameworkPathOverride` if yours differ.

In-game checks use the shared `../RimWorldDevTests` runner. Batch `osp-stands` checks the stand defs and jobs, art and text, force-wear targeting, drawing, the use and return jobs, and save/reload. Run it once per mod set (Harmony + Core + this mod, with Biotech on and off, and with Odyssey):

```powershell
dotnet build DevTests/OutfitStandsPlusForked.DevTests.csproj -c Release
python tools/osp_test.py run osp-stands --seed 1
python tools/osp_test.py close-session
python tools/osp_test.py release-check
```

The runner uses the active `ModsConfig.xml`; back it up, set the test list and restore it afterwards. The installed mod folder must match the working tree (`deploy.ps1`).

## Contributing

> Contributions, issues, and feature requests are welcome.

## Links


Support me on Ko-fi. This does not imply endorsement by the original authors.

[![Support me on Ko-fi](https://img.shields.io/badge/Support_me_on_Ko--fi-72a4f2?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/I7L525WMJ6)
[![GitHub Repository](https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/KyleMHB/Outfit-Stands-Plus-Forked)
- [Upstream Outfit Stands Plus on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3545172389)

- [Better Pawn Control Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724294345)
- [Cargo Containers Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=3714050938)
- [Everything Has Quality Reforked](https://steamcommunity.com/sharedfiles/filedetails/?id=3710884766)
- [Gear Up and Go Reforked](https://steamcommunity.com/sharedfiles/filedetails/?id=3724312502)
- [Outfit Stands Plus Forked on Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3724311713)
- [Stats Forked](https://steamcommunity.com/sharedfiles/filedetails/?id=3712317437)

## License

> This project is a fork of **Outfit Stands Plus** and inherits the original project's license. See the original project for license terms.

## Credits

- Original Outfit Stands Plus authors.
- Current refork maintainers.
