using System;
using System.Collections.Generic;

namespace SortHeim
{
    /// <summary>
    /// "Type" sort groups. Uses the game's item type, then splits the catch-all
    /// Material type into wood / stone / ore / metal / hide / seed by prefab name.
    /// </summary>
    internal static class ItemCategories
    {
        // Declaration order is the sort order.
        public enum Category
        {
            Weapon,
            Ammo,
            Shield,
            Armor,
            Utility,
            Tool,
            Food,
            Potion,
            Wood,
            Stone,
            Ore,
            Metal,
            Hide,
            Seed,
            Material,
            Trophy,
            Fish,
            Misc
        }

        // Prefab names, verified against the game's asset bundles.
        private static readonly Dictionary<string, Category> Materials = Build(
            (Category.Wood, "Wood FineWood RoundLog ElderBark YggdrasilWood Blackwood"),
            (Category.Stone, "Stone Flint Obsidian Crystal BlackMarble Grausten"),
            (Category.Metal, "Copper Tin Bronze Iron Silver BlackMetal Flametal FlametalNew"),
            (Category.Seed, "Acorn FirCone PineCone"));

        public static Category Get(ItemDrop.ItemData item)
        {
            var shared = item.m_shared;
            switch (shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                    return Category.Weapon;
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                    return Category.Ammo;
                case ItemDrop.ItemData.ItemType.Shield:
                    return Category.Shield;
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return Category.Armor;
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket:
                    return Category.Utility;
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch:
                    return Category.Tool;
                case ItemDrop.ItemData.ItemType.Consumable:
                    return shared.m_food > 0f || shared.m_foodStamina > 0f || shared.m_foodEitr > 0f
                        ? Category.Food
                        : Category.Potion;
                case ItemDrop.ItemData.ItemType.Material:
                    return MaterialCategory(item.m_dropPrefab != null ? item.m_dropPrefab.name : "");
                case ItemDrop.ItemData.ItemType.Trophy:
                    return Category.Trophy;
                case ItemDrop.ItemData.ItemType.Fish:
                    return Category.Fish;
                default:
                    return Category.Misc;
            }
        }

        private static Category MaterialCategory(string prefab)
        {
            if (Materials.TryGetValue(prefab, out var category)) return category;

            // Suffix rules also catch new and modded items (MooseHide, KaleSeeds, ...).
            if (EndsWith(prefab, "Hide") || EndsWith(prefab, "Pelt") || prefab == "LeatherScraps") return Category.Hide;
            if (EndsWith(prefab, "Seeds")) return Category.Seed;
            if (EndsWith(prefab, "Ore") || EndsWith(prefab, "OreNew") || EndsWith(prefab, "Scrap")) return Category.Ore;
            return Category.Material;
        }

        private static bool EndsWith(string s, string suffix) => s.EndsWith(suffix, StringComparison.Ordinal);

        private static Dictionary<string, Category> Build(params (Category category, string names)[] groups)
        {
            var map = new Dictionary<string, Category>();
            foreach (var (category, names) in groups)
                foreach (var name in names.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    map[name] = category;
            return map;
        }
    }
}
