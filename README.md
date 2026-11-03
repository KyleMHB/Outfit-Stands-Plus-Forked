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

Subscribe on Steam Workshop and enable **Harmony**, **Odyssey**, and **Outfit Stands Plus Forked** in RimWorld's mod list.

### Manual Installation

1. Download or clone the repository.
2. Place the mod folder in your RimWorld `Mods` directory.
3. Enable **Harmony**, **Odyssey**, and **Outfit Stands Plus Forked**.

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
