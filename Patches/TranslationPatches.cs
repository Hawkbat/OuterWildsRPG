using HarmonyLib;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public static class TranslationPatches
    {

        [HarmonyPrefix, HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.Translate))]
        public static bool TextTranslation_Translate(string key, ref string __result)
        {
            var translation = TranslationUtils.GetDialogue(key);
            if (!string.IsNullOrEmpty(translation))
            {
                __result = translation;
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.Translate_ShipLog))]
        public static bool TextTranslation_Translate_ShipLog(string key, ref string __result)
        {
            var translation = TranslationUtils.GetShipLog(key);
            if (!string.IsNullOrEmpty(translation))
            {
                __result = translation;
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.Translate_UI))]
        public static bool TextTranslation_Translate_UI(int key, ref string __result)
        {
            var translation = TranslationUtils.GetUI(key);
            if (!string.IsNullOrEmpty(translation))
            {
                __result = translation;
                return false;
            }
            return true;
        }
    }
}
