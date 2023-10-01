using HarmonyLib;
using OuterWildsRPG.Components;
using OuterWildsRPG.Objects.Drops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public static class ItemPatches {
        [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.MoveItemToCarrySocket))]
        public static bool ItemTool_MoveItemToCarrySocket(ItemTool __instance, OWItem item)
        {
            if (PlayerState.InDreamWorld()) return true;
            var pickup = item.GetComponentInChildren<DropPickupController>(true);
            if (pickup != null)
            {
                if (ModPlayerController.IsUnsuspendPickup())
                    pickup.ToggleVisuals(false);
                else
                    return pickup.AttemptItemPickup();
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.SocketItem))]
        public static bool ItemTool_SocketItem(ItemTool __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            var pickup = __instance._heldItem.GetComponentInChildren<DropPickupController>(true);
            if (pickup != null)
            {
                return pickup.AttemptItemDrop();
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.DropItem))]
        public static bool ItemTool_DropItem(ItemTool __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            var pickup = __instance._heldItem.GetComponentInChildren<DropPickupController>(true);
            if (pickup != null)
            {
                return pickup.AttemptItemDrop();
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.UpdateInteract))]
        public static void ItemTool_UpdateInteract(ItemTool __instance, FirstPersonManipulator firstPersonManipulator, bool inputBlocked, ref bool __result)
        {
            if (PlayerState.InDreamWorld()) return;
            if (__instance._promptState != ItemTool.PromptState.CANNOT_HOLD_MORE) return;
            if (DropManager.GetRemainingHotbarCapacity() <= 0) return;

            var focusedItemSocket = firstPersonManipulator.GetFocusedItemSocket();
            var focusedOWItem = firstPersonManipulator.GetFocusedOWItem();
            var interactPressed = OWInput.IsNewlyPressed(InputLibrary.interact);

            var newState = __instance._promptState;
            var itemName = string.Empty;
            var stateChanged = false;

            if (!OWInput.IsInputMode(InputMode.Character | InputMode.NomaiRemoteCam) || PlayerState.IsViewingProjector())
            {
                inputBlocked = true;
            }
            if (inputBlocked || __instance._waitForUnsocketAnimation) return;
            if (focusedItemSocket != null)
            {
                if (focusedItemSocket.IsSocketOccupied())
                {
                    newState = (focusedItemSocket.UsesGiveTakePrompts() ? ItemTool.PromptState.TAKE : ItemTool.PromptState.UNSOCKET);
                    itemName = focusedItemSocket.GetSocketedItem().GetDisplayName();
                    if (interactPressed)
                    {
                        __instance.StartUnsocketItem(focusedItemSocket);
                        stateChanged = true;
                    }
                }
            } else if (focusedOWItem != null)
            {
                newState = ItemTool.PromptState.PICK_UP;
                itemName = focusedOWItem.GetDisplayName();
                if (interactPressed)
                {
                    __instance.MoveItemToCarrySocket(focusedOWItem);
                    __instance._heldItem = focusedOWItem;
                    Locator.GetPlayerAudioController().PlayPickUpItem(__instance._heldItem.GetItemType());
                    stateChanged = true;
                }
            }
            __instance.UpdateState(newState, itemName);
            __result = stateChanged;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TransformAnimator), nameof(TransformAnimator.ResetToOriginalPositionRotation))]
        public static void TransformAnimator_ResetToOriginalPositionRotation(TransformAnimator __instance)
        {
            __instance._translating = false;
            __instance._rotating = false;
            __instance.enabled = false;
        }
    }
}
