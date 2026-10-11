using System.Collections.Generic;
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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Awake))]
        private static void Awake_Postfix(InventoryGui __instance)
        {
            Buttons.Clear();
            if (!ChestSortPlugin.ShowButtons.Value) return;

            var template = __instance.m_takeAllButton;
            var panel = __instance.m_container;
            if (template == null || panel == null)
            {
                ChestSortPlugin.Log.LogWarning("Could not find the chest panel; sort buttons disabled (hotkeys still work).");
                return;
            }

            var templateRect = (RectTransform)template.transform;
            float width = ChestSortPlugin.ButtonWidth.Value > 0f ? ChestSortPlugin.ButtonWidth.Value : templateRect.rect.width;
            float height = templateRect.rect.height;
            if (width <= 1f) width = 140f;
            if (height <= 1f) height = 36f;

            var offset = ChestSortPlugin.ButtonOffset.Value;
            var modes = new[] { SortMode.Name, SortMode.Value, SortMode.Weight };
            for (int i = 0; i < modes.Length; i++)
            {
                var go = CreateButton(template, panel, modes[i]);
                var rect = (RectTransform)go.transform;
                // Row hanging just below the chest panel's bottom-left corner.
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = offset + new Vector2(i * (width + Spacing), 0f);
                Buttons.Add(go);
            }
        }

        private static GameObject CreateButton(Button template, RectTransform parent, SortMode mode)
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
            if (label != null) label.text = "Sort: " + mode;

            go.SetActive(true);
            return go;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Update))]
        private static void Update_Postfix(InventoryGui __instance)
        {
            if (__instance.m_currentContainer == null || !InventoryGui.IsVisible()) return;
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
