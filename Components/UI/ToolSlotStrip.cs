using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.UI
{
    public class ToolSlotStrip : BuiltElement
    {
        CanvasGroup canvasGroup;
        readonly Dictionary<EquipSlot, ToolSlotIcon> toolSlotIcons = new();
        readonly Dictionary<int, ToolSlotIcon> hotbarIcons = new();

        public override void Setup()
        {

        }

        public override void Cleanup()
        {

        }

        public override void Rebuild()
        {
            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;

            var equipSlots = new List<EquipSlot>() {
                EquipSlot.Flashlight,
                EquipSlot.Signalscope,
                EquipSlot.Translator,
                EquipSlot.Stick,
                EquipSlot.Launcher,
            };

            var totalCount = equipSlots.Count + DropManager.GetTotalHotbarCapacity() + 1;

            var size = 30f;
            var gap = 5f;
            var offset = -0.5f * (size * totalCount + gap * (totalCount - 1) - size);
            var x = offset;
            foreach (var equipSlot in equipSlots)
            {
                var existing = toolSlotIcons.ContainsKey(equipSlot) ? toolSlotIcons[equipSlot] : null;
                if (existing) toolSlotIcons.Remove(equipSlot);

                var toolSlotIcon = MakeChild(existing, equipSlot.ToString());
                toolSlotIcon.Init(equipSlot, 0);

                toolSlotIcon.rectTransform.anchorMin = new(0.5f, 0f);
                toolSlotIcon.rectTransform.anchorMax = new(0.5f, 0f);
                toolSlotIcon.rectTransform.pivot = new Vector2(0.5f, 0f);
                toolSlotIcon.rectTransform.sizeDelta = new(size, size);
                toolSlotIcon.rectTransform.anchoredPosition = new Vector2(x, 25f);

                toolSlotIcons.Add(equipSlot, toolSlotIcon);
                x += size + gap;
            }

            x += size + gap;

            for (var itemIndex = 0; itemIndex < DropManager.GetTotalHotbarCapacity(); itemIndex++)
            {
                var existing = hotbarIcons.ContainsKey(itemIndex) ? hotbarIcons[itemIndex] : null;
                if (existing) hotbarIcons.Remove(itemIndex);

                var hotbarIcon = MakeChild(existing, $"Hotbar{itemIndex}");
                hotbarIcon.Init(EquipSlot.Item, itemIndex);

                hotbarIcon.rectTransform.anchorMin = new(0.5f, 0f);
                hotbarIcon.rectTransform.anchorMax = new(0.5f, 0f);
                hotbarIcon.rectTransform.pivot = new Vector2(0.5f, 0f);
                hotbarIcon.rectTransform.sizeDelta = new(size, size);
                hotbarIcon.rectTransform.anchoredPosition = new Vector2(x, 25f);

                hotbarIcons.Add(itemIndex, hotbarIcon);
                x += size + gap;
            }
        }

        public override bool Animate()
        {
            var active = PlayerStateUtils.IsPlayable && !PlayerState.InDreamWorld();
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, active ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            return true;
        }
    }
}
