using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ChestSort
{
    public enum SortMode
    {
        Name,
        Value,
        Weight
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    [BepInPlugin(Guid, Name, Version)]
    public class ChestSortPlugin : BaseUnityPlugin
    {
        public const string Guid = "mbaya.ChestSort";
        public const string Name = "ChestSort";
        public const string Version = "1.1.0";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> ShowButtons;
        internal static ConfigEntry<Vector2> ButtonOffset;
        internal static ConfigEntry<float> ButtonWidth;
        internal static ConfigEntry<bool> MergeStacks;
        internal static ConfigEntry<bool> UseStackTotals;

        internal static ConfigEntry<SortDirection> NameDirection;
        internal static ConfigEntry<SortDirection> ValueDirection;
        internal static ConfigEntry<SortDirection> WeightDirection;

        internal static ConfigEntry<KeyboardShortcut> NameKey;
        internal static ConfigEntry<KeyboardShortcut> ValueKey;
        internal static ConfigEntry<KeyboardShortcut> WeightKey;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            ShowButtons = Config.Bind("UI", "ShowButtons", true,
                "Show the sort buttons (A-Z, $, weight icon) under an open chest.");
            ButtonOffset = Config.Bind("UI", "ButtonOffset", new Vector2(0f, -6f),
                "Offset (x, y) of the button row, relative to the bottom-left corner of the chest panel. Takes effect on next game start.");
            ButtonWidth = Config.Bind("UI", "ButtonWidth", 0f,
                "Width of each sort button in pixels. 0 = automatic (compact). Takes effect on next game start.");

            MergeStacks = Config.Bind("Sorting", "MergeStacks", true,
                "Combine partial stacks of the same item before sorting.");
            UseStackTotals = Config.Bind("Sorting", "UseStackTotals", true,
                "Sort by the whole stack's value/weight (true) or by a single item's value/weight (false).");

            NameDirection = Config.Bind("Sorting", "NameDirection", SortDirection.Ascending, "Order used when sorting by name.");
            ValueDirection = Config.Bind("Sorting", "ValueDirection", SortDirection.Descending, "Order used when sorting by trader value.");
            WeightDirection = Config.Bind("Sorting", "WeightDirection", SortDirection.Descending, "Order used when sorting by weight.");

            NameKey = Config.Bind("Hotkeys", "SortByName", KeyboardShortcut.Empty, "Hotkey to sort the open chest by name.");
            ValueKey = Config.Bind("Hotkeys", "SortByValue", KeyboardShortcut.Empty, "Hotkey to sort the open chest by value.");
            WeightKey = Config.Bind("Hotkeys", "SortByWeight", KeyboardShortcut.Empty, "Hotkey to sort the open chest by weight.");

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(ChestSortPlugin).Assembly);

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
                case SortMode.Value: return ValueDirection.Value;
                case SortMode.Weight: return WeightDirection.Value;
                default: return NameDirection.Value;
            }
        }
    }
}
