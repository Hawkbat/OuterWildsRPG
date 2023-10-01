using HarmonyLib;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Common.Effects;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Patches
{
    [HarmonyPatch]
    public static class DialoguePatches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(DialogueNode), nameof(DialogueNode.EntryConditionsSatisfied))]
        public static bool DialogueNode_EntryConditionsSatisfied(DialogueNode __instance, ref bool __result)
        {
            if (__instance._listEntryCondition.Count == 0)
            {
                __result = false;
                return false;
            }
            foreach (var condition in __instance._listEntryCondition)
            {
                if (DialogueConditionManager.SharedInstance.ConditionExists(condition))
                {
                    if (!DialogueConditionManager.SharedInstance.GetConditionState(condition))
                    {
                        __result = false;
                        return false;
                    }
                }
                else if (PlayerData.PersistentConditionExists(condition))
                {
                    if (!PlayerData.GetPersistentCondition(condition))
                    {
                        __result = false;
                        return false;
                    }
                }
                else
                {
                    OuterWildsRPG.LogWarning($"Unknown entry condition {condition} on dialogue node {__instance.Name}");
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(DialogueConditionManager), nameof(DialogueConditionManager.ReadPlayerData))]
        public static void DialogueConditionManager_ReadPlayerData(DialogueConditionManager __instance)
        {
            foreach (var quest in QuestManager.GetAllQuests())
            {
                AddSetCondition(quest.FullID, quest.IsStarted);
                AddSetCondition($"{quest.FullID}:COMPLETED", quest.IsComplete);
                foreach (var step in quest.Steps)
                {
                    AddSetCondition(step.FullID, step.IsStarted);
                    AddSetCondition($"{step.FullID}:COMPLETED", step.IsComplete);
                }
            }
            foreach (var drop in DropManager.GetAllDrops())
            {
                AddSetCondition(drop.FullID, DropManager.HasDrop(drop));
                AddSetCondition($"{drop.FullID}:EQUIPPED", DropManager.HasEquippedDrop(drop));
                AddSetCondition($"{drop.FullID}:INVENTORY", DropManager.HasInventoryDrop(drop));
                AddSetCondition($"{drop.FullID}:HOTBAR", DropManager.HasHotbarDrop(drop));
            }
            foreach (var perk in PerkManager.GetAllPerks())
            {
                AddSetCondition(perk.FullID, PerkManager.HasUnlockedPerk(perk));
            }
            foreach (var buff in BuffManager.GetAllActiveBuffs())
            {
                foreach (var effect in buff.GetEffects())
                {
                    if (effect is CustomEffect customEffect)
                    {
                        AddSetCondition(customEffect.ID, true);
                    }
                }
            }
        }

        static void AddSetCondition(string condition, bool value)
        {
            if (!DialogueConditionManager.SharedInstance.AddCondition(condition, value))
                DialogueConditionManager.SharedInstance.SetConditionState(condition, value);
        }
    }
}
