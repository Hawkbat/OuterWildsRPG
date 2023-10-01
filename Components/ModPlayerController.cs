using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class ModPlayerController : MonoBehaviour
    {
        static ModPlayerController instance;

        int hotbarIndex = -1;
        bool isUnsuspendPickup;
        List<OWItemSocket> sockets;
        Dictionary<string, DropPickupController> dropPickups = new();

        public static int GetHotbarIndex() => instance.hotbarIndex;
        public static void SetHotbarIndex(int index) => instance.hotbarIndex = index;

        public static bool IsUnsuspendPickup() => instance.isUnsuspendPickup;

        void Awake()
        {
            instance = this;
            sockets = FindObjectsOfType<OWItemSocket>().ToList();
        }

        void Update()
        {
            if (!PlayerStateUtils.IsPlayable) return;
            if (PlayerState.InDreamWorld()) return;
            if (Locator.GetToolModeSwapper().GetToolGroup() != ToolGroup.Suit) return;
            if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None) &&
                !Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item)) return;

            var hasChanged = false;

            if (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft) ||
                OWInput.IsNewlyPressed(InputLibrary.toolOptionUp))
            {
                if (hotbarIndex > 0)
                    hotbarIndex--;
                else
                    hotbarIndex = 0;
                hasChanged = true;
            }
            if (OWInput.IsNewlyPressed(InputLibrary.toolOptionRight) ||
                OWInput.IsNewlyPressed(InputLibrary.toolOptionDown))
            {
                if (hotbarIndex < DropManager.GetTotalHotbarCapacity() - 1)
                    hotbarIndex++;
                else
                    hotbarIndex = DropManager.GetTotalHotbarCapacity() - 1;
                hasChanged = true;
            }

            if (OWInput.IsNewlyPressed(InputLibrary.cancel))
            {
                hotbarIndex = -1;
                hasChanged = true;
            }

            if (Locator.GetToolModeSwapper().GetItemCarryTool()._waitForUnsocketAnimation)
            {
                return;
            }

            var activeItem = GetActiveItem();

            var slotHasItem = false;

            var hotbarDrop = DropManager.GetHotbarDrop(hotbarIndex);
            if (hotbarDrop != null)
            {
                var itemPickup = dropPickups.ContainsKey(hotbarDrop.FullID) ? dropPickups[hotbarDrop.FullID] : null;
                if (!dropPickups.ContainsKey(hotbarDrop.FullID))
                {
                    itemPickup = DropManager
                        .FindDropPickups(hotbarDrop)
                        .FirstOrDefault(d => d.IsVanillaItem());
                    dropPickups.Add(hotbarDrop.FullID, itemPickup);
                }
                if (itemPickup != null)
                {
                    itemPickup.ToggleVisuals(false);
                    var owItem = itemPickup.GetVanillaItem();
                    slotHasItem = true;
                    if (activeItem != owItem || IsSuspended(owItem))
                    {
                        hasChanged = true;
                        SwapItems(owItem);
                    }
                }
            }
            if (hasChanged && !slotHasItem && activeItem)
            {
                SuspendHeldItem(activeItem);
            }
        }

        public static void RegisterPickup(DropPickupController dropPickup)
        {
            var drop = dropPickup.GetLocation().Drop;
            if (!instance.dropPickups.ContainsKey(drop.FullID))
            {
                instance.dropPickups.Add(drop.FullID, dropPickup);
            }
            if (dropPickup.IsVanillaItem() && DropManager.HasDrop(drop))
            {
                instance.SuspendHeldItem(dropPickup.GetVanillaItem());
            }
        }

        OWItem GetActiveItem()
        {
            return Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
        }

        bool IsHeld(OWItem item)
        {
            if (item == null) return false;
            return UnityUtils.IsTransformDescendent(item.transform, Locator.GetPlayerBody().transform);
        }

        bool IsSuspended(OWItem item)
        {
            if (item == null) return false;
            return !item.gameObject.activeSelf && IsHeld(item);
        }

        void SwapItems(OWItem newItem)
        {
            if (GetActiveItem() != null)
            {
                SuspendHeldItem(GetActiveItem());
            }
            if (newItem != null)
            {
                UnsuspendHeldItem(newItem);
            }
        }

        void SuspendHeldItem(OWItem item)
        {
            if (item == null || IsSuspended(item)) return;
            if (item == GetActiveItem())
            {
                Locator.GetToolModeSwapper().GetItemCarryTool()._heldItem = null;
                Locator.GetToolModeSwapper().GetItemCarryTool().UnequipTool();
            }
            item.SetColliderActivation(false);
            item.gameObject.SetActive(false);
        }

        void UnsuspendHeldItem(OWItem item)
        {
            if (item == null) return;
            item.gameObject.SetActive(true);
            if (item is ScrollItem scroll) scroll._nomaiWallText.HideImmediate();
            item.SetColliderActivation(false);
            var socket = sockets.FirstOrDefault(s => s != null && s.GetSocketedItem() == item);
            if (socket != null)
                socket.RemoveFromSocket();
            isUnsuspendPickup = true;
            Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(item);
            isUnsuspendPickup = false;
            if (socket != null)
                item.OnCompleteUnsocket();
        }
    }
}
