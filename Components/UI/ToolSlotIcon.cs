using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.UI
{
    public class ToolSlotIcon : BuiltElement
    {
        EquipSlot equipSlot;
        int itemIndex;
        ToolMode tool;

        Image border;
        Image icon;
        Image buttonIcon;

        public void Init(EquipSlot equipSlot, int itemIndex)
        {
            this.equipSlot = equipSlot;
            this.itemIndex = itemIndex;
            tool = equipSlot switch
            {
                EquipSlot.Translator => ToolMode.Translator,
                EquipSlot.Signalscope => ToolMode.SignalScope,
                EquipSlot.Scout => ToolMode.Probe,
                EquipSlot.Launcher => ToolMode.Probe,
                EquipSlot.Item => ToolMode.Item,
                _ => ToolMode.None,
            };
        }

        public override void Setup()
        {

        }

        public override void Cleanup()
        {

        }

        public override void Rebuild()
        {
            border = MakeChild(border, "Border");
            border.color = Assets.HUDBackColor;

            icon = MakeChild(icon, "Icon");
            icon.rectTransform.sizeDelta = new Vector2(-2f, -2f);

            buttonIcon = MakeChild(buttonIcon, "Button");
            buttonIcon.rectTransform.anchorMin = new Vector2(0.6f, 0f);
            buttonIcon.rectTransform.anchorMax = new Vector2(1f, 0.4f);
            buttonIcon.rectTransform.pivot = new Vector2(1f, 0f);
            buttonIcon.preserveAspect = true;

            InputConsts.InputCommandType cmdType = equipSlot switch
            {
                EquipSlot.Flashlight => InputConsts.InputCommandType.FLASHLIGHT,
                EquipSlot.Signalscope => InputConsts.InputCommandType.SIGNALSCOPE,
                EquipSlot.Translator => InputConsts.InputCommandType.INTERACT,
                EquipSlot.Launcher => InputConsts.InputCommandType.PROBELAUNCH,
                _ => InputConsts.InputCommandType.UNDEFINED,
            };
            if (cmdType != InputConsts.InputCommandType.UNDEFINED)
            {
                var cmd = InputLibrary.GetInputCommand(cmdType);
                var imgs = cmd.GetUITextures(OWInput.UsingGamepad());
                ButtonPromptLibrary.SharedInstance.RefineUITextureListForDisplay(imgs);
                var img = imgs.FirstOrDefault();
                if (img != null)
                {
                    var btnSprite = Sprite.Create(img, new Rect(0f, 0f, img.width, img.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Vector4.zero, false);
                    buttonIcon.sprite = btnSprite;
                }
            }

            buttonIcon.enabled = buttonIcon.sprite != null;
        }

        public override bool Animate()
        {
            var active = tool == Locator.GetToolModeSwapper().GetToolMode();
            if (equipSlot == EquipSlot.Flashlight)
                active = Locator.GetFlashlight().IsFlashlightOn();
            else if (equipSlot == EquipSlot.Stick)
                active = PlayerStateUtils.IsRoasting;
            else if (equipSlot == EquipSlot.Item)
                active = ModPlayerController.GetHotbarIndex() == itemIndex;

            var drop = DropManager.GetEquippedDrop(equipSlot);
            if (equipSlot == EquipSlot.Item)
                drop = DropManager.GetHotbarDrop(itemIndex);

            var color = drop != null ? Assets.GetRarityColor(drop.Rarity) : Assets.HUDBackColor;

            icon.sprite = drop != null ? drop.Icon : null;
            icon.color = icon.sprite ? Color.white : Color.Lerp(Color.black, Assets.HUDBackColor, 0.25f);
            border.color = active ? color : Color.Lerp(Color.black, color, 0.5f);
            buttonIcon.enabled = buttonIcon.sprite != null && drop != null;
            
            return true;
        }
    }
}
