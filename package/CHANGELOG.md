# Changelog

## 1.1.1
- Rarity and weight sorts now group items of the same type together (food with food, wood with wood, ...) before falling back to A-Z, so materials no longer land in the middle of your food.

## 1.1.0
- New **type** sort: weapons, ammo, shields, armor, utility, tools, food, potions & meads, then materials split into wood, stone, ore, metal, hides and seeds, then other materials, trophies, fish and misc.
- New `ButtonStyle` option: set it to `Text` for the classic labelled buttons ("Sort: Name", "Sort: Type", ...) instead of icons.

## 1.0.1
- Renamed from ChestSort to **SortHeim**.
- Sort your **inventory** too (buttons beside the inventory panel). Your hotbar row and equipped gear stay put (configurable).
- New **rarity** sort: orders items by biome tier, Meadows → Ashlands. Crafted, smelted, cooked and brewed items inherit the tier of their ingredients. Fix or add any item with the `TierOverrides` setting.
- Removed value sort (almost every item has no trader value).
- Buttons: **A-Z** (name), the game's Encumbered icon (weight), and a gem (rarity), with hover tooltips.
- Hotkeys now sort the open chest, or your inventory if no chest is open.

## ChestSort 1.1.0
- Sort buttons became compact symbols with hover tooltips.

## ChestSort 1.0.0
- Initial release: sort open chests by name, value, or weight.
