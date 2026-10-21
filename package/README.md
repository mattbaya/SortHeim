# SortHeim

**One-click sorting for chests and your inventory in Valheim: by name, type, weight or rarity.**

Open a chest, click a button, and everything falls into order. Open your inventory and do the same, without ever moving your hotbar or the gear you're wearing. No more hunting through a jumble of Wood, Flint and Cooked meat for the one thing you need.

<!-- IMAGE: sort-buttons.png (a chest and the inventory with the sort buttons visible) -->

## Contents

- [Usage](#usage)
- [Examples](#examples)
- [How rarity works](#how-rarity-works)
- [Hotkeys](#hotkeys)
- [Configuration](#configuration)
- [Multiplayer](#multiplayer)
- [Troubleshooting](#troubleshooting)
- [Works with](#works-with)

## Usage

Open your inventory or a chest. Small buttons appear beside your inventory and under the chest (hover for a tooltip). Prefer words? Set `ButtonStyle` to `Text` for buttons labelled "Sort: Name", "Sort: Type", and so on.

| Button | Sorts by |
| --- | --- |
| **A-Z** | Item name, alphabetically (in your game language) |
| Grid icon | Type: weapons, ammo, shields, armor, utility, tools, food, potions, wood, stone, ore, metal, hides, seeds, other materials, trophies, fish, misc |
| Encumbered icon | Weight, heaviest first |
| Gem icon | Rarity: biome tier, latest biome first |

Partial stacks of the same item are merged first, so five half-full stacks of Wood become fewer, fuller ones. When sorting your inventory, your **hotbar row** and **equipped gear** never move.

## Examples

**Tidy a storage chest by type.** Click the **grid** button. Weapons and armour come first, then food and potions, then the building materials grouped together: wood with wood, stone with stone, ore with ore, metal with metal, and finally trophies and odds and ends.

**Find what's heavy.** Over your carry limit? Click the **weight** button on your inventory and the heaviest stacks jump to the top, so you can see what to dump at home.

**See your best loot first.** The **rarity** button puts the stuff from the latest biome you've explored at the top. A chest full of Mistlands and Plains materials leads with the Mistlands ones, and the Meadows leftovers sink to the bottom.

**Alphabetical.** **A-Z** for the classic list when you know exactly what you're looking for.

**Quickly from the keyboard.** Bind a hotkey to a sort and press it with a chest (or your inventory) open. See [Hotkeys](#hotkeys).

**A cleaner inventory without touching your setup.** Sort your inventory right before a big crafting session. Your hotbar and equipped weapons, armour and tools stay exactly where they are.

**Prefer words.** `ButtonStyle = Text` gives you labelled buttons ("Sort: Name", "Sort: Type", ...) instead of icons.

## How rarity works

Valheim has no rarity stat, so SortHeim ranks items by the biome they come from:

0 Meadows → 1 Black Forest → 2 Swamp → 3 Mountain → 4 Plains → 5 Mistlands → 6 Ashlands → 7 Deep North

Raw materials and trophies come from a built-in table. Anything crafted, smelted, cooked or brewed takes the highest tier among its ingredients (via its cheapest recipe), so modded items with recipes are ranked automatically. Within a biome, items are grouped by type (food together, wood together, ...) and then A-Z. Items with no known tier go last. To fix or add one, use `TierOverrides`, e.g. `TrophyMoose=7, MyModdedOre=5` (prefab names).

## Hotkeys

By default no hotkeys are set. In the config you can assign one to each sort (`SortByName`, `SortByType`, `SortByWeight`, `SortByRarity`). A hotkey sorts the chest you have open, or your inventory if no chest is open.

## Configuration

Edit in Gale's config editor, or `BepInEx/config/mbaya.SortHeim.cfg`:

| Section | Key | Default | Notes |
| --- | --- | --- | --- |
| UI | ShowChestButtons / ShowInventoryButtons | true | Hide either set of buttons |
| UI | ChestButtonOffset / InventoryButtonOffset | (0, -6) / (6, 0) | Nudge the buttons if they overlap another mod's UI |
| UI | ButtonStyle | Icons | `Text` = labelled buttons ("Sort: Name", ...) |
| UI | ButtonWidth | 0 | 0 = automatic (compact icons, or Take All width for text) |
| Inventory | KeepHotbar | true | Never move hotbar items (slots 1-8) |
| Inventory | KeepEquipped | true | Never move equipped items |
| Sorting | MergeStacks | true | Combine partial stacks first |
| Sorting | UseStackTotals | true | Compare whole-stack weight instead of per-item |
| Sorting | NameDirection / TypeDirection / WeightDirection / RarityDirection | Asc / Asc / Desc / Desc | |
| Sorting | TierOverrides | (empty) | `PrefabName=Tier` pairs |
| Hotkeys | SortByName / SortByType / SortByWeight / SortByRarity | (none) | Sorts the open chest, or your inventory if none is open |

## Multiplayer

Client-side only; other players don't need it. Chests are only sorted while you have them open (which makes you their owner), so it's safe on dedicated servers.

## Troubleshooting

- **The buttons overlap another mod's UI.** Nudge them with `ChestButtonOffset` and `InventoryButtonOffset`, or hide either set with `ShowChestButtons` / `ShowInventoryButtons`.
- **The buttons are too small to read.** Set `ButtonStyle` to `Text`.
- **My hotbar item moved.** Check `KeepHotbar` is true (it covers slots 1-8). Items outside the hotbar row can be sorted.
- **An item sorts in the wrong biome.** Add it to `TierOverrides`, for example `TrophyMoose=7`. Use the item's prefab name.
- **A modded item sorts last.** Items with no known tier go last in the rarity sort. Add a `TierOverrides` entry for it.
- **Nothing happens in a chest.** The chest has to be open: the mod only sorts a chest while you have it open. A chest another player has open can't be opened by you until they close it.
- **Something else.** Open an issue at [github.com/mattbaya/SortHeim](https://github.com/mattbaya/SortHeim) and attach `BepInEx/LogOutput.log`.

## Works with

- **[GearHeim](https://thunderstore.io/c/valheim/p/Svaha/GearHeim/)**: keep your loadouts in a Loadout Chest and your other storage tidy with SortHeim.
- **[Bag O' Heim](https://thunderstore.io/c/valheim/p/Svaha/BagOHeim/)**: packs that refill themselves from your chests; sort those chests so everything's easy to find.
- **[PortalHeim SG1](https://thunderstore.io/c/valheim/p/Svaha/PortalHeim_SG1/)**: ring-gate portals you can dial from a menu or the map.
