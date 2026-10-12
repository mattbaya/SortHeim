using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SortHeim
{
    [HarmonyPatch(typeof(InventoryGui))]
    internal static class InventoryGuiPatches
    {
        private const float Spacing = 6f;
        private static readonly SortMode[] Modes = { SortMode.Name, SortMode.Type, SortMode.Weight, SortMode.Rarity };

        // Weight buttons still showing the fallback scale, waiting for the game's icon.
        private static readonly List<Image> PendingWeightIcons = new List<Image>();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Awake))]
        private static void Awake_Postfix(InventoryGui __instance)
        {
            PendingWeightIcons.Clear();

            var template = __instance.m_takeAllButton;
            if (template == null)
            {
                SortHeimPlugin.Log.LogWarning("Could not find a button to copy; sort buttons disabled (hotkeys still work).");
                return;
            }

            var templateRect = (RectTransform)template.transform;
            float height = templateRect.rect.height;
            if (height <= 1f) height = 36f;
            bool text = SortHeimPlugin.Style.Value == ButtonStyle.Text;
            // Symbols only need a compact, roughly square button; text needs Take All's width.
            float autoWidth = text ? templateRect.rect.width : Mathf.Round(height * 1.5f);
            if (autoWidth <= 1f) autoWidth = 140f;
            float width = SortHeimPlugin.ButtonWidth.Value > 0f ? SortHeimPlugin.ButtonWidth.Value : autoWidth;
            var size = new Vector2(width, height);

            var tooltipSource = __instance.GetComponentsInChildren<UITooltip>(true).FirstOrDefault(t => t.m_tooltipPrefab != null);

            // Chest: a row hanging just below the chest panel's bottom-left corner.
            if (SortHeimPlugin.ShowChestButtons.Value && __instance.m_container != null)
            {
                for (int i = 0; i < Modes.Length; i++)
                {
                    var mode = Modes[i];
                    var rect = CreateButton(template, __instance.m_container, mode, "chest",
                        () => InventorySorter.SortOpenContainer(mode), tooltipSource);
                    Place(rect, size, anchor: new Vector2(0f, 0f), pivot: new Vector2(0f, 1f),
                        SortHeimPlugin.ChestButtonOffset.Value + new Vector2(i * (width + Spacing), 0f));
                }
            }

            // Inventory: a column beside the inventory panel's top-right corner.
            if (SortHeimPlugin.ShowInventoryButtons.Value && __instance.m_player != null)
            {
                for (int i = 0; i < Modes.Length; i++)
                {
                    var mode = Modes[i];
                    var rect = CreateButton(template, __instance.m_player, mode, "inventory",
                        () => InventorySorter.SortPlayerInventory(mode), tooltipSource);
                    Place(rect, size, anchor: new Vector2(1f, 1f), pivot: new Vector2(0f, 1f),
                        SortHeimPlugin.InventoryButtonOffset.Value + new Vector2(0f, -i * (height + Spacing)));
                }
            }
        }

        private static void Place(RectTransform rect, Vector2 size, Vector2 anchor, Vector2 pivot, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static RectTransform CreateButton(Button template, RectTransform parent, SortMode mode, string target,
            Action onClick, UITooltip tooltipSource)
        {
            var go = Object.Instantiate(template.gameObject, parent, false);
            go.name = $"SortHeim_{target}_{mode}";

            // The clone inherits Take All's persistent onClick; replace it outright.
            var button = go.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => onClick());
            button.interactable = true;

            // Strip the gamepad hint (it would otherwise trigger/show Take All's binding)
            // and the localizer (it would overwrite our label with "Take All").
            foreach (var pad in go.GetComponentsInChildren<UIGamePad>(true))
            {
                if (pad.m_hint != null) Object.Destroy(pad.m_hint);
                Object.Destroy(pad);
            }
            foreach (var c in go.GetComponentsInChildren<Component>(true))
            {
                if (c != null && c.GetType().Name == "Localize") Object.Destroy(c);
            }

            var label = go.GetComponentInChildren<TMP_Text>(true);
            var tint = label != null ? label.color : Color.white;
            if (SortHeimPlugin.Style.Value == ButtonStyle.Text)
            {
                if (label != null) label.text = "Sort: " + mode;
            }
            else switch (mode)
            {
                case SortMode.Name:
                    if (label != null) label.text = "A-Z";
                    break;
                case SortMode.Type:
                    if (label != null) label.text = "";
                    AddIcon(go, Icons.Type, tint);
                    break;
                case SortMode.Weight:
                    if (label != null) label.text = "";
                    var weightIcon = AddIcon(go, Icons.Scale, tint);
                    if (!TryUseEncumberedIcon(weightIcon)) PendingWeightIcons.Add(weightIcon);
                    break;
                case SortMode.Rarity:
                    if (label != null) label.text = "";
                    AddIcon(go, Icons.Gem, tint);
                    break;
            }

            string what = mode == SortMode.Rarity ? "rarity (biome)" : mode.ToString().ToLowerInvariant();
            AddTooltip(go, $"Sort {target} by {what}", tooltipSource);

            go.SetActive(true);
            return (RectTransform)go.transform;
        }

        private static Image AddIcon(GameObject button, Sprite sprite, Color tint)
        {
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)icon.transform;
            rect.SetParent(button.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 4f);
            rect.offsetMax = new Vector2(-4f, -4f);

            var image = icon.GetComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.sprite = sprite;
            image.color = tint;
            return image;
        }

        /// <summary>Swaps in the game's own "Encumbered" status icon if ObjectDB has it.</summary>
        private static bool TryUseEncumberedIcon(Image image)
        {
            var sprite = Icons.Encumbered;
            if (sprite == null || image == null) return false;
            image.sprite = sprite;
            image.color = Color.white; // game artwork: show its own colours
            return true;
        }

        private static void AddTooltip(GameObject button, string text, UITooltip source)
        {
            var tooltip = button.GetComponent<UITooltip>();
            if (tooltip == null)
            {
                if (source == null) return;
                tooltip = button.AddComponent<UITooltip>();
                tooltip.m_tooltipPrefab = source.m_tooltipPrefab;
            }
            tooltip.m_topic = "";
            tooltip.m_text = text;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Update))]
        private static void Update_Postfix()
        {
            if (!InventoryGui.IsVisible()) return;

            // ObjectDB may not have been ready when the buttons were built.
            if (PendingWeightIcons.Count > 0)
                PendingWeightIcons.RemoveAll(i => i == null || TryUseEncumberedIcon(i));

            if (IsTyping()) return;

            if (SortHeimPlugin.NameKey.Value.IsDown()) InventorySorter.SortActive(SortMode.Name);
            else if (SortHeimPlugin.TypeKey.Value.IsDown()) InventorySorter.SortActive(SortMode.Type);
            else if (SortHeimPlugin.WeightKey.Value.IsDown()) InventorySorter.SortActive(SortMode.Weight);
            else if (SortHeimPlugin.RarityKey.Value.IsDown()) InventorySorter.SortActive(SortMode.Rarity);
        }

        private static bool IsTyping() =>
            (Chat.instance != null && Chat.instance.HasFocus())
            || global::Console.IsVisible()
            || TextInput.IsVisible();
    }
}
