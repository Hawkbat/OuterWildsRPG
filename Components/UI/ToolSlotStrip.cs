using OuterWildsRPG.Enums;
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
                EquipSlot.Item,
            };
            var size = 30f;
            var gap = 5f;
            var offset = -0.5f * (size * equipSlots.Count + gap * (equipSlots.Count - 1) - size);
            var x = offset;
            foreach (var equipSlot in equipSlots)
            {
                var existing = toolSlotIcons.ContainsKey(equipSlot) ? toolSlotIcons[equipSlot] : null;
                if (existing) toolSlotIcons.Remove(equipSlot);

                var toolSlotIcon = MakeChild(existing, equipSlot.ToString());
                toolSlotIcon.Init(equipSlot);

                toolSlotIcon.rectTransform.anchorMin = new(0.5f, 0f);
                toolSlotIcon.rectTransform.anchorMax = new(0.5f, 0f);
                toolSlotIcon.rectTransform.pivot = new Vector2(0.5f, 0f);
                toolSlotIcon.rectTransform.sizeDelta = new(size, size);
                toolSlotIcon.rectTransform.anchoredPosition = new Vector2(x, 25f);

                toolSlotIcons.Add(equipSlot, toolSlotIcon);
                x += size + gap;
            }
        }

        public override bool Animate()
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, PlayerStateUtils.IsPlayable ? 1f : 0f, Time.unscaledDeltaTime);
            return true;
        }
    }
}
