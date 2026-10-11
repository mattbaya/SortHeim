# ChestSort

Sort the contents of any open chest, cart, or ship storage by **name**, **value**, or **weight**.

## Usage

Open a chest. Three small buttons appear just below the chest panel (hover for a tooltip):

- **A-Z** – alphabetical by item name (in your game language)
- **$** – by trader value (coins, amber, pearls, ...); items with no value fall back to name order
- **Encumbered icon** (the same one the game shows when you carry too much) – by weight, heaviest first

Partial stacks of the same item are merged before sorting (configurable).

## Configuration

Edit in Gale's config editor, or `BepInEx/config/mbaya.ChestSort.cfg`:

| Section | Key | Default | Notes |
| --- | --- | --- | --- |
| UI | ShowButtons | true | Hide the buttons if you only want hotkeys |
| UI | ButtonOffset | (0, -6) | Nudge the button row if it overlaps another mod's UI |
| UI | ButtonWidth | 0 | 0 = automatic compact width |
| Sorting | MergeStacks | true | Combine partial stacks first |
| Sorting | UseStackTotals | true | Compare whole-stack value/weight instead of per-item |
| Sorting | NameDirection / ValueDirection / WeightDirection | Asc / Desc / Desc | |
| Hotkeys | SortByName / SortByValue / SortByWeight | (none) | e.g. `LeftAlt + N` |

## Multiplayer

Client-side only; other players don't need it. Sorting only happens while you have the chest open (which makes you its owner), so it's safe on dedicated servers.
