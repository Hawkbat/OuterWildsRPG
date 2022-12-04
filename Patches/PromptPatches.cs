using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public class PromptPatches
    {
        public static PromptPosition QuestPromptPosition => OuterWildsRPG.QuestPromptPosition;
        public static ScreenPromptList QuestPromptList => OuterWildsRPG.QuestPromptList;

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.UpdatePromptsEnabled))]
        public static void PromptManager_UpdatePromptsEnabled(PromptManager __instance, bool promptsOptionEnabled)
        {
            bool active = promptsOptionEnabled && __instance._promptsVisible;
            if (QuestPromptList != null) QuestPromptList.gameObject.SetActive(active);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.GetScreenPromptList))]
        public static bool PromptManager_GetScreenPromptList(PromptPosition listPosition, ref ScreenPromptList __result)
        {
            if (listPosition == QuestPromptPosition)
            {
                __result = QuestPromptList;
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.GetTextAnchor))]
        public static bool PromptManager_GetTextAnchor(PromptPosition listPosition, ref TextAnchor __result)
        {
            if (listPosition == QuestPromptPosition)
            {
                __result = TextAnchor.MiddleLeft;
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.RemoveScreenPrompt), new[] { typeof(ScreenPrompt) } )]
        public static void PromptManager_RemoveScreenPrompt(ScreenPrompt buttonPrompt)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(buttonPrompt))
            {
                QuestPromptList.RemoveScreenPrompt(buttonPrompt);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.GetHighestPriorityApplicableToPrompt))]
        public static bool PromptManager_GetHighestPriorityApplicableToPrompt(ScreenPrompt prompt, ref int __result)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(prompt))
            {
                __result = QuestPromptList.GetHighestVisiblePriority();
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.UpdateText))]
        public static void PromptManager_UpdateText(ScreenPrompt prompt, string updatedText)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(prompt))
            {
                QuestPromptList.OnUpdateScreenPromptText(prompt, updatedText);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.TriggerRebuild))]
        public static void PromptManager_TriggerRebuild(ScreenPrompt prompt)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(prompt))
            {
                QuestPromptList.RebuildScreenPromptUIElement(prompt);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.RefreshOnWillRender))]
        public static void PromptManager_RefreshOnWillRender(ScreenPrompt prompt)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(prompt))
            {
                QuestPromptList.RefreshScreenPromptUIElement(prompt);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.RefreshVisibilityOnWillRender))]
        public static void PromptManager_RefreshVisibilityOnWillRender(ScreenPrompt prompt)
        {
            if (QuestPromptList != null && QuestPromptList.Contains(prompt))
            {
                QuestPromptList.RefreshScreenPromptUIElementVisibility(prompt);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.RebuildUI))]
        public static void PromptManager_RebuildUI()
        {
            if (QuestPromptList != null) QuestPromptList.RebuildAllScreenPromptUIElements();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PromptManager), nameof(PromptManager.OnChangeGUIMode))]
        public static void PromptManager_OnChangeGUIMode()
        {
            bool active = true;
            if (GUIMode.IsHiddenMode()) active = false;
            if (QuestPromptList != null) QuestPromptList.gameObject.SetActive(active);
        }
    }
}
