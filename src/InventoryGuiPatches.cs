using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChestSort
{
    [HarmonyPatch(typeof(InventoryGui))]
    internal static class InventoryGuiPatches
    {
        private const float Spacing = 6f;
        private static readonly List<GameObject> Buttons = new List<GameObject>();
        private static Image _weightIcon;
        private static bool _weightIconFromGame;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Awake))]
        private static void Awake_Postfix(InventoryGui __instance)
        {
            Buttons.Clear();
            _weightIcon = null;
            _weightIconFromGame = false;
            if (!ChestSortPlugin.ShowButtons.Value) return;

            var template = __instance.m_takeAllButton;
            var panel = __instance.m_container;
            if (template == null || panel == null)
            {
                ChestSortPlugin.Log.LogWarning("Could not find the chest panel; sort buttons disabled (hotkeys still work).");
                return;
            }

            var templateRect = (RectTransform)template.transform;
            float height = templateRect.rect.height;
            if (height <= 1f) height = 36f;
            // Symbols only need a compact, roughly square button.
            float width = ChestSortPlugin.ButtonWidth.Value > 0f ? ChestSortPlugin.ButtonWidth.Value : Mathf.Round(height * 1.5f);

            var tooltipSource = __instance.GetComponentsInChildren<UITooltip>(true).FirstOrDefault(t => t.m_tooltipPrefab != null);

            var offset = ChestSortPlugin.ButtonOffset.Value;
            var modes = new[] { SortMode.Name, SortMode.Value, SortMode.Weight };
            for (int i = 0; i < modes.Length; i++)
            {
                var go = CreateButton(template, panel, modes[i], tooltipSource);
                var rect = (RectTransform)go.transform;
                // Row hanging just below the chest panel's bottom-left corner.
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = offset + new Vector2(i * (width + Spacing), 0f);
                Buttons.Add(go);
            }
        }

        private static GameObject CreateButton(Button template, RectTransform parent, SortMode mode, UITooltip tooltipSource)
        {
            var go = Object.Instantiate(template.gameObject, parent, false);
            go.name = "ChestSort_" + mode;

            // The clone inherits Take All's persistent onClick; replace it outright.
            var button = go.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => InventorySorter.SortOpenContainer(mode));
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
            if (mode == SortMode.Weight)
            {
                if (label != null) label.text = "";
                _weightIcon = AddIcon(go, label != null ? label.color : Color.white);
            }
            else if (label != null)
            {
                label.text = mode == SortMode.Name ? "A-Z" : "$";
            }

            AddTooltip(go, "Sort by " + mode.ToString().ToLowerInvariant(), tooltipSource);

            go.SetActive(true);
            return go;
        }

        private static Image AddIcon(GameObject button, Color fallbackTint)
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
            image.color = fallbackTint;
            image.sprite = Icons.Scale;
            TryUseEncumberedIcon(image);
            return image;
        }

        /// <summary>Swaps in the game's own "Encumbered" status icon once ObjectDB has it.</summary>
        private static void TryUseEncumberedIcon(Image image)
        {
            var sprite = Icons.Encumbered;
            if (sprite == null) return;
            image.sprite = sprite;
            image.color = Color.white; // game artwork: show its own colours
            _weightIconFromGame = true;
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
        private static void Update_Postfix(InventoryGui __instance)
        {
            if (__instance.m_currentContainer == null || !InventoryGui.IsVisible()) return;

            // ObjectDB may not have been ready when the buttons were built.
            if (_weightIcon != null && !_weightIconFromGame) TryUseEncumberedIcon(_weightIcon);

            if (IsTyping()) return;

            if (ChestSortPlugin.NameKey.Value.IsDown()) InventorySorter.SortOpenContainer(SortMode.Name);
            else if (ChestSortPlugin.ValueKey.Value.IsDown()) InventorySorter.SortOpenContainer(SortMode.Value);
            else if (ChestSortPlugin.WeightKey.Value.IsDown()) InventorySorter.SortOpenContainer(SortMode.Weight);
        }

        private static bool IsTyping() =>
            (Chat.instance != null && Chat.instance.HasFocus())
            || Console.IsVisible()
            || TextInput.IsVisible();
    }
}
