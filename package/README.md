# SortHeim

One-click sorting for **chests** and **your inventory** in Valheim — by **name**, **weight**, or **rarity**.

## Usage

Open your inventory or a chest. Small buttons appear beside your inventory and under the chest (hover for a tooltip):

| Button | Sorts by |
| --- | --- |
| **A-Z** | Item name, alphabetically (in your game language) |
| Encumbered icon | Weight, heaviest first |
| Gem icon | Rarity: biome tier, latest biome first |

Partial stacks of the same item are merged first. When sorting your inventory, your **hotbar row** and **equipped gear** never move.

### How rarity works

Valheim has no rarity stat, so SortHeim ranks items by the biome they come from:

0 Meadows → 1 Black Forest → 2 Swamp → 3 Mountain → 4 Plains → 5 Mistlands → 6 Ashlands → 7 Deep North

Raw materials and trophies come from a built-in table. Anything crafted, smelted, cooked or brewed takes the highest tier among its ingredients (via its cheapest recipe), so modded items with recipes are ranked automatically. Items with no known tier go last. To fix or add one, use `TierOverrides`, e.g. `TrophyMoose=7, MyModdedOre=5` (prefab names).

## Configuration

Edit in Gale's config editor, or `BepInEx/config/mbaya.SortHeim.cfg`:

| Section | Key | Default | Notes |
| --- | --- | --- | --- |
| UI | ShowChestButtons / ShowInventoryButtons | true | Hide either set of buttons |
| UI | ChestButtonOffset / InventoryButtonOffset | (0, -6) / (6, 0) | Nudge the buttons if they overlap another mod's UI |
| UI | ButtonWidth | 0 | 0 = automatic compact width |
| Inventory | KeepHotbar | true | Never move hotbar items (slots 1-8) |
| Inventory | KeepEquipped | true | Never move equipped items |
| Sorting | MergeStacks | true | Combine partial stacks first |
| Sorting | UseStackTotals | true | Compare whole-stack weight instead of per-item |
| Sorting | NameDirection / WeightDirection / RarityDirection | Asc / Desc / Desc | |
| Sorting | TierOverrides | (empty) | `PrefabName=Tier` pairs |
| Hotkeys | SortByName / SortByWeight / SortByRarity | (none) | Sorts the open chest, or your inventory if none is open |

## Multiplayer

Client-side only; other players don't need it. Chests are only sorted while you have them open (which makes you their owner), so it's safe on dedicated servers.
