using HarmonyLib;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Common.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public static class BuffPatches
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
            var multiplier = BuffManager.GetStatMultiplier<TranslationSpeedEffect>();
            // Vanilla is a flat 0.2f
            __instance._totalTranslateTime = 1f * multiplier;
        }
    }
}
