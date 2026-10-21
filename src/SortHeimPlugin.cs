using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SortHeim
{
    public enum SortMode
    {
        Name,
        Type,
        Weight,
        Rarity
    }

    public enum ButtonStyle
    {
        Icons,
        Text
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    [BepInPlugin(Guid, Name, Version)]
    public class SortHeimPlugin : BaseUnityPlugin
    {
        public const string Guid = "mbaya.SortHeim";
        public const string Name = "SortHeim";
        public const string Version = "1.1.2";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> ShowChestButtons;
        internal static ConfigEntry<Vector2> ChestButtonOffset;
        internal static ConfigEntry<bool> ShowInventoryButtons;
        internal static ConfigEntry<Vector2> InventoryButtonOffset;
        internal static ConfigEntry<float> ButtonWidth;
        internal static ConfigEntry<ButtonStyle> Style;

        internal static ConfigEntry<bool> MergeStacks;
        internal static ConfigEntry<bool> UseStackTotals;
        internal static ConfigEntry<bool> KeepHotbar;
        internal static ConfigEntry<bool> KeepEquipped;

        internal static ConfigEntry<SortDirection> NameDirection;
        internal static ConfigEntry<SortDirection> TypeDirection;
        internal static ConfigEntry<SortDirection> WeightDirection;
        internal static ConfigEntry<SortDirection> RarityDirection;
        internal static ConfigEntry<string> TierOverrides;

        internal static ConfigEntry<KeyboardShortcut> NameKey;
        internal static ConfigEntry<KeyboardShortcut> TypeKey;
        internal static ConfigEntry<KeyboardShortcut> WeightKey;
        internal static ConfigEntry<KeyboardShortcut> RarityKey;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            ShowChestButtons = Config.Bind("UI", "ShowChestButtons", true,
                "Show the sort buttons under an open chest.");
            ChestButtonOffset = Config.Bind("UI", "ChestButtonOffset", new Vector2(0f, -6f),
                "Offset (x, y) of the chest button row from the bottom-left corner of the chest panel. Takes effect on next game start.");
            ShowInventoryButtons = Config.Bind("UI", "ShowInventoryButtons", true,
                "Show the sort buttons next to your inventory.");
            InventoryButtonOffset = Config.Bind("UI", "InventoryButtonOffset", new Vector2(6f, 0f),
                "Offset (x, y) of the inventory button column from the top-right corner of the inventory panel. Takes effect on next game start.");
            Style = Config.Bind("UI", "ButtonStyle", ButtonStyle.Icons,
                "Icons = compact symbol buttons (A-Z, type, weight, rarity icons). Text = wider buttons labelled 'Sort: Name' etc. Takes effect on next game start.");
            ButtonWidth = Config.Bind("UI", "ButtonWidth", 0f,
                "Width of each sort button in pixels. 0 = automatic (compact for Icons, Take All width for Text). Takes effect on next game start.");

            MergeStacks = Config.Bind("Sorting", "MergeStacks", true,
                "Combine partial stacks of the same item before sorting.");
            UseStackTotals = Config.Bind("Sorting", "UseStackTotals", true,
                "Sort by the whole stack's weight (true) or a single item's weight (false).");
            KeepHotbar = Config.Bind("Inventory", "KeepHotbar", true,
                "Never move items in your hotbar row (slots 1-8) when sorting your inventory.");
            KeepEquipped = Config.Bind("Inventory", "KeepEquipped", true,
                "Never move equipped items when sorting your inventory.");

            NameDirection = Config.Bind("Sorting", "NameDirection", SortDirection.Ascending, "Order used when sorting by name.");
            TypeDirection = Config.Bind("Sorting", "TypeDirection", SortDirection.Ascending,
                "Order used when sorting by type. Ascending = weapons, ammo, shields, armor, utility, tools, food, potions, wood, stone, ore, metal, hides, seeds, other materials, trophies, fish, misc. Items within a type are sorted by name.");
            WeightDirection = Config.Bind("Sorting", "WeightDirection", SortDirection.Descending, "Order used when sorting by weight.");
            RarityDirection = Config.Bind("Sorting", "RarityDirection", SortDirection.Descending,
                "Order used when sorting by rarity. Descending = latest biome first. Items with no known tier always go last.");
            TierOverrides = Config.Bind("Sorting", "TierOverrides", "",
                "Set or fix an item's rarity tier, as comma-separated PrefabName=Tier pairs, e.g. 'TrophyMoose=7, Coins=0'. " +
                "Tiers: 0 Meadows, 1 Black Forest, 2 Swamp, 3 Mountain, 4 Plains, 5 Mistlands, 6 Ashlands, 7 Deep North.");

            NameKey = Config.Bind("Hotkeys", "SortByName", KeyboardShortcut.Empty, "Sort by name (the open chest, or your inventory if no chest is open).");
            TypeKey = Config.Bind("Hotkeys", "SortByType", KeyboardShortcut.Empty, "Sort by type (the open chest, or your inventory if no chest is open).");
            WeightKey = Config.Bind("Hotkeys", "SortByWeight", KeyboardShortcut.Empty, "Sort by weight (the open chest, or your inventory if no chest is open).");
            RarityKey = Config.Bind("Hotkeys", "SortByRarity", KeyboardShortcut.Empty, "Sort by rarity (the open chest, or your inventory if no chest is open).");

            TierOverrides.SettingChanged += (_, __) => RarityTiers.Invalidate();

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(SortHeimPlugin).Assembly);

            Log.LogInfo($"{Name} {Version} loaded");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        internal static SortDirection DirectionFor(SortMode mode)
        {
            switch (mode)
            {
                case SortMode.Type: return TypeDirection.Value;
                case SortMode.Weight: return WeightDirection.Value;
                case SortMode.Rarity: return RarityDirection.Value;
                default: return NameDirection.Value;
            }
        }
    }
}
