using HarmonyLib;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Drops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.Update))]
        public static void HazardDetector_Update(HazardDetector __instance)
        {
            if (__instance._isPlayerDetector)
            {
                __instance._genericDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.GENERAL, __instance._genericDamagePerSecond);
                __instance._darkMatterDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.DARKMATTER, __instance._darkMatterDamagePerSecond);
                __instance._fireDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.FIRE, __instance._fireDamagePerSecond);
                __instance._heatDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.HEAT, __instance._heatDamagePerSecond);
                __instance._sandDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.SANDFALL, __instance._sandDamagePerSecond);
                __instance._electricityDamagePerSecond = BuffManager.ModifyHazardDamage(HazardVolume.HazardType.ELECTRICITY, __instance._electricityDamagePerSecond);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.SwitchTextNode))]
        public static void NomaiTranslatorProp_SwitchTextMode(NomaiTranslatorProp __instance)
        {
            var multiplier = BuffManager.GetTranslationSpeedMultiplier();
            // Vanilla is a flat 0.2f
            __instance._totalTranslateTime = 1f * multiplier;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Campfire), nameof(Campfire.StartRoasting))]
        public static bool Campfire_StartRoasting(Campfire __instance)
        {
            var stick = DropManager.GetEquippedDrop(EquipSlot.Stick);
            if (stick == null)
            {
                OuterWildsRPG.MinorQueue.Enqueue("You need a roasting stick to roast marshmellows...");
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ToolModeSwapper), nameof(ToolModeSwapper.EquipToolMode))]
        public static bool ToolModeSwapper_EquipToolMode(ToolModeSwapper __instance, ToolMode mode)
        {
            return mode switch
            {
                ToolMode.Probe => DropManager.GetEquippedDrop(EquipSlot.Scout) != null && DropManager.GetEquippedDrop(EquipSlot.Launcher) != null,
                ToolMode.SignalScope => DropManager.GetEquippedDrop(EquipSlot.Signalscope) != null,
                ToolMode.Translator => DropManager.GetEquippedDrop(EquipSlot.Translator) != null,
                ToolMode.Item => DropManager.GetEquippedDrop(EquipSlot.Item) != null,
                _ => true,
            };
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Flashlight), nameof(Flashlight.TurnOn))]
        public static bool Flashlight_TurnOn(Flashlight __instance)
        {
            var flashlight = DropManager.GetEquippedDrop(EquipSlot.Flashlight);
            if (flashlight == null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.SpawnMallow))]
        public static bool Marshmallow_SpawnMallow(Marshmallow __instance, bool playSFX)
        {
            if (!DropManager.RemoveDrop(DropManager.GetDrop("MARSHMALLOW")))
            {
                __instance.RemoveMallow();
                OuterWildsRPG.MinorQueue.Enqueue("You are out of marshmallows...");
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Marshmallow), nameof(Marshmallow.Remove))]
        public static bool Marshmallow_Remove(Marshmallow __instance)
        {
            DropManager.ReceiveDrop(DropManager.GetDrop("MARSHMALLOW"));
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerSpacesuit), nameof(PlayerSpacesuit.SuitUp))]
        public static bool PlayerSpacesuit_SuitUp(PlayerSpacesuit __instance, bool isTrainingSuit, bool instantSuitUp, ref bool putOnHelmet)
        {
            if (isTrainingSuit) return true;

            if (!DropManager.HasDrop(DropManager.GetDrop("BASIC_SUIT"))) {
                DropManager.ReceiveDrop(DropManager.GetDrop("BASIC_SUIT"));
                DropManager.ReceiveDrop(DropManager.GetDrop("BASIC_HELMET"));
                DropManager.ReceiveDrop(DropManager.GetDrop("BASIC_JETPACK"));
                DropManager.ReceiveDrop(DropManager.GetDrop("BASIC_SCOUT"));
                DropManager.ReceiveDrop(DropManager.GetDrop("BASIC_LAUNCHER"));
            }

            var suit = DropManager.GetEquippedDrop(EquipSlot.Suit);
            if (suit == null) return false;
            var helmet = DropManager.GetEquippedDrop(EquipSlot.Helmet);
            if (helmet == null) putOnHelmet = false;

            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerSpacesuit), nameof(PlayerSpacesuit.PutOnHelmet))]
        public static bool PlayerSpacesuit_PutOnHelmet(PlayerSpacesuit __instance)
        {
            if (__instance.IsWearingSuit() && __instance.IsTrainingSuit()) return true;
            var helmet = DropManager.GetEquippedDrop(EquipSlot.Helmet);
            if (helmet == null) return false;
            return true;
        }
    }
}
