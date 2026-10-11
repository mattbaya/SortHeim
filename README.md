# SortHeim

A client-side [BepInEx](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) mod for Valheim that sorts chests and your inventory by **name**, **weight**, or **rarity** (biome tier). Formerly *ChestSort*. Distributed via Thunderstore, so it installs from Gale / r2modman.

**Install:** search "SortHeim" in Gale or r2modman ([Svaha-SortHeim on Thunderstore](https://thunderstore.io/c/valheim/p/Svaha/SortHeim/)).

Player-facing docs: [package/README.md](package/README.md).

## Layout

| Path | Purpose |
| --- | --- |
| `src/` | Mod source (`SortHeim.csproj`, targets `netstandard2.1`) |
| `package/` | Thunderstore files: `manifest.json`, `README.md`, `CHANGELOG.md`, `icon.png` |
| `build.sh` | Builds and zips a Thunderstore package into `dist/` |
| `libs/Managed/` | Game assemblies to compile against (**git-ignored, never commit**) |

## Building

1. Install the .NET 8 SDK (or newer).
2. Provide Valheim's managed assemblies — either:
   - copy `assembly_valheim.dll`, `assembly_guiutils.dll`, `assembly_utils.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.InputLegacyModule.dll`, `UnityEngine.UI.dll`, `UnityEngine.UIModule.dll`, `Unity.TextMeshPro.dll` from `<Valheim>/valheim_Data/Managed` into `libs/Managed/`, or
   - set `VALHEIM_MANAGED=/path/to/valheim_Data/Managed`.

   No game install? The free dedicated server has the same assemblies:
   `steamcmd +@sSteamCmdForcePlatformType windows +force_install_dir ./valheim_server +login anonymous +app_update 896660 +quit`
   (they're in `valheim_server/valheim_server_Data/Managed`).
3. Run `./build.sh` → `dist/SortHeim-<version>.zip`.

## Rarity table

Raw-material tiers live in `src/RarityTiers.cs` (`BaseTable`). Names there must be exact prefab names; crafted/smelted/cooked items are derived automatically.

## Contributing

Bug reports and pull requests are welcome. Please open an issue first for anything larger than a small fix. Releases to Thunderstore are published by the maintainer.
