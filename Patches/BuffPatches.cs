using HarmonyLib;
using OuterWildsRPG.Objects.Common;
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

        [HarmonyPostfix, HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.GetNetDamagePerSecond))]
        public static void HazardDetector_GetNetDamagePerSecond(HazardDetector __instance, ref float __result)
        {
            if (__instance._isPlayerDetector)
            {
                var baseMaxHealth = 100f;
                var modifiedMaxHealth = 100f * BuffManager.GetMaxHealthMultiplier();
                var ratio = baseMaxHealth / modifiedMaxHealth;
                __result *= ratio;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.ApplyInstantDamage))]
        public static void PlayerResource_ApplyInstantDamage(PlayerResources __instance, ref float damage, InstantDamageType type, ref bool __result)
        {
            var baseMaxHealth = 100f;
            var modifiedMaxHealth = 100f * BuffManager.GetMaxHealthMultiplier();
            var ratio = baseMaxHealth / modifiedMaxHealth;
            damage *= ratio;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.UpdateOxygen))]
        public static bool PlayerResources_UpdateOxygen(PlayerResources __instance)
        {
            if (PlayerState.InDreamWorld()) return true;
            if (__instance._isSuffocating && !__instance.IsOxygenPresent() && __instance._currentOxygen <= 0f)
            {
                var modifier = BuffManager.GetSuffocationTimeModifier();
                var modifiedSuffocationTime = __instance._startSuffocationTime + 4f + modifier;
                if (Time.time < modifiedSuffocationTime)
                {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.IsSuffocating))]
        public static bool PlayerResources_IsSuffocating(PlayerResources __instance, ref bool __result)
        {
            if (PlayerState.InDreamWorld()) return true;
            if (__instance._isSuffocating && !__instance.IsOxygenPresent() && __instance._currentOxygen <= 0f)
            {
                var modifier = BuffManager.GetSuffocationTimeModifier();
                var modifiedStartSuffocationTime = __instance._startSuffocationTime + modifier;
                if (Time.time < modifiedStartSuffocationTime)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.SwitchTextNode))]
        public static void NomaiTranslatorProp_SwitchTextMode(NomaiTranslatorProp __instance)
        {
            var multiplier = BuffManager.GetTranslationSpeedMultiplier();
            // Vanilla is a flat 0.2f
            __instance._totalTranslateTime = 1f * multiplier;
        }
    }
}
