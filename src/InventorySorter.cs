using System;
using System.Collections.Generic;
using System.Linq;

namespace SortHeim
{
    internal static class InventorySorter
    {
        /// <summary>Sorts the open chest, or the player's inventory if no chest is open.</summary>
        public static bool SortActive(SortMode mode)
        {
            var gui = InventoryGui.instance;
            if (gui != null && gui.m_currentContainer != null) return SortOpenContainer(mode);
            return SortPlayerInventory(mode);
        }

        public static bool SortOpenContainer(SortMode mode)
        {
            if (!CanSortNow(out var gui)) return false;

            var container = gui.m_currentContainer;
            if (container == null) return false;

            // Opening a chest hands its ZDO to the opener; only the owner may write it.
            var nview = container.m_nview;
            if (nview == null || !nview.IsValid() || !nview.IsOwner())
            {
                SortHeimPlugin.Log.LogWarning("Not the owner of this container; skipping sort.");
                return false;
            }

            var inventory = container.GetInventory();
            if (inventory == null) return false;

            Sort(inventory, mode, item => false, firstRow: 0);
            return true;
        }

        public static bool SortPlayerInventory(SortMode mode)
        {
            if (!CanSortNow(out _)) return false;

            var player = Player.m_localPlayer;
            var inventory = player != null ? player.GetInventory() : null;
            if (inventory == null) return false;

            bool keepHotbar = SortHeimPlugin.KeepHotbar.Value;
            bool keepEquipped = SortHeimPlugin.KeepEquipped.Value;

            Sort(inventory, mode,
                item => (keepHotbar && item.m_gridPos.y == 0) || (keepEquipped && item.m_equipped),
                firstRow: keepHotbar ? 1 : 0);
            return true;
        }

        private static bool CanSortNow(out InventoryGui gui)
        {
            gui = InventoryGui.instance;
            // Don't shuffle the grid out from under an item the player is dragging.
            return gui != null && InventoryGui.IsVisible() && gui.m_dragItem == null;
        }

        /// <param name="isPinned">Items that must keep their current slot.</param>
        /// <param name="firstRow">Rows above this are never filled with sorted items.</param>
        public static void Sort(Inventory inventory, SortMode mode, Func<ItemDrop.ItemData, bool> isPinned, int firstRow)
        {
            List<ItemDrop.ItemData> items = inventory.m_inventory;
            if (items.Count == 0) return;

            var pinned = items.Where(isPinned).ToList();
            // Start from the on-screen order so merging fills the earliest stacks first.
            var movable = items.Where(i => !isPinned(i))
                .OrderBy(i => i.m_gridPos.y).ThenBy(i => i.m_gridPos.x)
                .ToList();
            if (movable.Count == 0) return;

            if (SortHeimPlugin.MergeStacks.Value)
                Merge(movable);

            var sorted = Order(movable, mode).ToList();

            var taken = new HashSet<Vector2i>(pinned.Select(i => i.m_gridPos));
            int width = inventory.GetWidth();
            int height = inventory.GetHeight();
            int next = 0;
            for (int y = firstRow; y < height && next < sorted.Count; y++)
            for (int x = 0; x < width && next < sorted.Count; x++)
            {
                var pos = new Vector2i(x, y);
                if (!taken.Contains(pos)) sorted[next++].m_gridPos = pos;
            }

            if (next < sorted.Count)
            {
                // Can't happen (sorted items came from these slots), but never lose items.
                SortHeimPlugin.Log.LogError("Ran out of slots while sorting; aborting.");
                return;
            }

            items.Clear();
            items.AddRange(pinned);
            items.AddRange(sorted);

            // Recomputes weight and fires m_onChanged, which persists the inventory.
            inventory.Changed();
        }

        private static IEnumerable<ItemDrop.ItemData> Order(List<ItemDrop.ItemData> items, SortMode mode)
        {
            bool desc = SortHeimPlugin.DirectionFor(mode) == SortDirection.Descending;
            var byName = StringComparer.CurrentCultureIgnoreCase;

            IOrderedEnumerable<ItemDrop.ItemData> ordered;
            switch (mode)
            {
                case SortMode.Weight:
                    bool totals = SortHeimPlugin.UseStackTotals.Value;
                    ordered = desc ? items.OrderByDescending(i => WeightOf(i, totals)) : items.OrderBy(i => WeightOf(i, totals));
                    ordered = ordered.ThenBy(DisplayName, byName);
                    break;
                case SortMode.Rarity:
                    var tiers = items.ToDictionary(i => i, RarityTiers.Get);
                    // Unknown tiers always sink to the end, whichever direction is chosen.
                    ordered = items.OrderBy(i => tiers[i] == RarityTiers.Unknown ? 1 : 0);
                    ordered = desc ? ordered.ThenByDescending(i => tiers[i]) : ordered.ThenBy(i => tiers[i]);
                    ordered = ordered.ThenBy(DisplayName, byName);
                    break;
                default:
                    ordered = desc ? items.OrderByDescending(DisplayName, byName) : items.OrderBy(DisplayName, byName);
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
