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
            __instance._totalTranslateTime = 2f * multiplier;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Campfire), nameof(Campfire.StartRoasting))]
        public static bool Campfire_StartRoasting(Campfire __instance)
        {
            var stick = DropManager.GetEquippedDrop(EquipSlot.Stick);
            if (stick == null)
            {
                OuterWildsRPG.MinorQueue.Enqueue("You need a roasting stick to roast marshmellows!");
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ToolModeSwapper), nameof(ToolModeSwapper.EquipToolMode))]
        public static bool ToolModeSwapper_EquipToolMode(ToolModeSwapper __instance, ToolMode mode)
        {
            return mode switch
            {
                ToolMode.Probe => DropManager.GetEquippedDrop(EquipSlot.Probe) != null,
                ToolMode.SignalScope => DropManager.GetEquippedDrop(EquipSlot.Signalscope) != null,
                ToolMode.Translator => DropManager.GetEquippedDrop(EquipSlot.Translator) != null,
                _ => true,
            };
        }
    }
}
