using HarmonyLib;
using OuterWildsRPG.Components;
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
        public static void ItemTool_MoveItemToCarrySocket(ItemTool __instance, OWItem item)
        {
            var pickup = item.GetComponentInChildren<DropPickup>();
            if (pickup != null) pickup.OnPickedUp(item);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.SocketItem))]
        public static void ItemTool_SocketItem(ItemTool __instance)
        {
            var pickup = __instance._heldItem.GetComponentInChildren<DropPickup>();
            if (pickup != null) pickup.OnDropped(__instance._heldItem);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.DropItem))]
        public static void ItemTool_DropItem(ItemTool __instance)
        {
            var pickup = __instance._heldItem.GetComponentInChildren<DropPickup>();
            if (pickup != null) pickup.OnDropped(__instance._heldItem);
        }
    }
}
