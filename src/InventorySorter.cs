using System;
using System.Collections.Generic;
using System.Linq;

namespace ChestSort
{
    internal static class InventorySorter
    {
        /// <summary>Sorts the currently open chest. Returns false if nothing could be sorted.</summary>
        public static bool SortOpenContainer(SortMode mode)
        {
            var gui = InventoryGui.instance;
            if (gui == null || !InventoryGui.IsVisible()) return false;

            var container = gui.m_currentContainer;
            if (container == null) return false;

            // Don't shuffle the grid out from under an item the player is dragging.
            if (gui.m_dragItem != null) return false;

            // Opening a chest hands its ZDO to the opener; only the owner may write it.
            var nview = container.m_nview;
            if (nview == null || !nview.IsValid() || !nview.IsOwner())
            {
                ChestSortPlugin.Log.LogWarning("Not the owner of this container; skipping sort.");
                return false;
            }

            var inventory = container.GetInventory();
            if (inventory == null) return false;

            Sort(inventory, mode);
            return true;
        }

        public static void Sort(Inventory inventory, SortMode mode)
        {
            List<ItemDrop.ItemData> items = inventory.m_inventory;
            if (items.Count == 0) return;

            // Start from the on-screen order so merging fills the earliest stacks first.
            items.Sort((a, b) =>
            {
                int c = a.m_gridPos.y.CompareTo(b.m_gridPos.y);
                return c != 0 ? c : a.m_gridPos.x.CompareTo(b.m_gridPos.x);
            });

            if (ChestSortPlugin.MergeStacks.Value)
                Merge(items);

            var sorted = Order(items, mode).ToList();

            int width = Math.Max(1, inventory.GetWidth());
            for (int i = 0; i < sorted.Count; i++)
                sorted[i].m_gridPos = new Vector2i(i % width, i / width);

            items.Clear();
            items.AddRange(sorted);

            // Recomputes weight and fires m_onChanged, which makes Container persist to its ZDO.
            inventory.Changed();
        }

        private static IEnumerable<ItemDrop.ItemData> Order(List<ItemDrop.ItemData> items, SortMode mode)
        {
            bool totals = ChestSortPlugin.UseStackTotals.Value;
            bool desc = ChestSortPlugin.DirectionFor(mode) == SortDirection.Descending;

            IOrderedEnumerable<ItemDrop.ItemData> ordered;
            switch (mode)
            {
                case SortMode.Value:
                    ordered = desc ? items.OrderByDescending(i => ValueOf(i, totals)) : items.OrderBy(i => ValueOf(i, totals));
                    ordered = ordered.ThenBy(DisplayName, StringComparer.CurrentCultureIgnoreCase);
                    break;
                case SortMode.Weight:
                    ordered = desc ? items.OrderByDescending(i => WeightOf(i, totals)) : items.OrderBy(i => WeightOf(i, totals));
                    ordered = ordered.ThenBy(DisplayName, StringComparer.CurrentCultureIgnoreCase);
                    break;
                default:
                    ordered = desc
                        ? items.OrderByDescending(DisplayName, StringComparer.CurrentCultureIgnoreCase)
                        : items.OrderBy(DisplayName, StringComparer.CurrentCultureIgnoreCase);
                    break;
            }

            // Stable, deterministic tie-breakers: higher quality first, then bigger stacks first.
            return ordered
                .ThenByDescending(i => i.m_quality)
                .ThenByDescending(i => i.m_stack);
        }

        private static string DisplayName(ItemDrop.ItemData item)
        {
            string raw = item.m_shared.m_name;
            return Localization.instance != null ? Localization.instance.Localize(raw) : raw;
        }

        private static float ValueOf(ItemDrop.ItemData item, bool totals) =>
            totals ? item.GetValue() : item.m_shared.m_value;

        private static float WeightOf(ItemDrop.ItemData item, bool totals) =>
            totals ? item.GetWeight() : item.m_shared.m_weight;

        /// <summary>Tops up partial stacks of identical items and drops any that end up empty.</summary>
        private static void Merge(List<ItemDrop.ItemData> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var target = items[i];
                int max = target.m_shared.m_maxStackSize;
                if (max <= 1) continue;

                for (int j = i + 1; j < items.Count && target.m_stack < max; j++)
                {
                    var source = items[j];
                    if (source.m_stack <= 0 || !CanStack(target, source)) continue;

                    int moved = Math.Min(max - target.m_stack, source.m_stack);
                    target.m_stack += moved;
                    source.m_stack -= moved;
                }
            }

            items.RemoveAll(i => i.m_stack <= 0);
        }

        private static bool CanStack(ItemDrop.ItemData a, ItemDrop.ItemData b) =>
            a.m_shared.m_name == b.m_shared.m_name
            && a.m_quality == b.m_quality
            && a.m_variant == b.m_variant
            && a.m_worldLevel == b.m_worldLevel;
    }
}
