using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestManager
    {
        const int DEFAULT_QUEST_XP = 1000;
        const int DEFAULT_MAIN_QUEST_XP = 2000;
        const int DEFAULT_SIDE_QUEST_XP = 1000;
        const int DEFAULT_MISC_QUEST_XP = 500;

        public class QuestStepEvent : UnityEvent<QuestStep> { }
        public static QuestStepEvent OnStartStep = new();
        public static QuestStepEvent OnCompleteStep = new();
        public class QuestEvent : UnityEvent<Quest> { }
        public static QuestEvent OnStartQuest = new();
        public static QuestEvent OnCompleteQuest = new();
        public static QuestEvent OnTrackQuest = new();
        public static QuestEvent OnUntrackQuest = new();

        static Dictionary<string, Quest> quests = new();

        public static Dictionary<string, Quest> Dictionary => quests;

        public static Quest GetQuest(string id, string modID)
        {
            if (!id.Contains("/")) id = $"{modID}/{id}";
            if (quests.TryGetValue(id, out var quest))
            {
                return quest;
            }
            return null;
        }

        public static bool RegisterQuest(Quest quest)
        {
            if (!quests.ContainsKey(quest.FullID))
            {
                quests.Add(quest.FullID, quest);
                return true;
            }
            return false;
        }

        public static IEnumerable<Quest> GetAllQuests()
            => quests.Values;

        public static IEnumerable<Quest> GetStartedQuests()
            => quests.Values.Where(q => q.IsStarted);

        public static IEnumerable<Quest> GetInProgressQuests()
            => quests.Values.Where(q => q.IsInProgress);

        public static IEnumerable<Quest> GetCompletedQuests()
            => quests.Values.Where(q => q.IsComplete);

        public static bool HasStartedStep(QuestStep step)
        {
            return QuestSaveData.Instance.StartedSteps.ContainsKey(step.Quest.FullID) && QuestSaveData.Instance.StartedSteps[step.Quest.FullID].Contains(step.ID);
        }

        public static bool StartStep(QuestStep step)
        {
            if (!QuestSaveData.Instance.StartedSteps.ContainsKey(step.Quest.FullID))
                QuestSaveData.Instance.StartedSteps.Add(step.Quest.FullID, new HashSet<string>());
            if (QuestSaveData.Instance.StartedSteps[step.Quest.FullID].Add(step.ID))
            {
                OnStartStep.Invoke(step);
                return true;
            }
            return false;
        }

        public static bool HasCompletedStep(QuestStep step)
        {
            return QuestSaveData.Instance.CompletedSteps.ContainsKey(step.Quest.FullID) && QuestSaveData.Instance.CompletedSteps[step.Quest.FullID].Contains(step.ID);
        }

        public static bool CompleteStep(QuestStep step)
        {
            if (!QuestSaveData.Instance.CompletedSteps.ContainsKey(step.Quest.FullID))
                QuestSaveData.Instance.CompletedSteps.Add(step.Quest.FullID, new HashSet<string>());
            if (QuestSaveData.Instance.CompletedSteps[step.Quest.FullID].Add(step.ID))
            {
                OnCompleteStep.Invoke(step);
                return true;
            }
            return false;
        }

        public static bool HasStartedQuest(Quest quest)
        {
            return QuestSaveData.Instance.StartedQuests.Contains(quest.FullID);
        }

        public static bool StartQuest(Quest quest)
        {
            if (QuestSaveData.Instance.StartedQuests.Add(quest.FullID))
            {
                OnStartQuest.Invoke(quest);
                SetTrackingQuest(quest, true);
                return true;
            }
            return false;
        }

        public static bool HasCompletedQuest(Quest quest)
        {
            return QuestSaveData.Instance.CompletedQuests.Contains(quest.FullID);
        }

        public static bool CompleteQuest(Quest quest)
        {
            if (QuestSaveData.Instance.CompletedQuests.Add(quest.FullID))
            {
                OnCompleteQuest.Invoke(quest);
                if (quest.Type == QuestType.Main)
                    CharacterManager.AwardXP(DEFAULT_MAIN_QUEST_XP, $"Completed main quest {quest.Name}!");
                else if (quest.Type == QuestType.Side)
                    CharacterManager.AwardXP(DEFAULT_SIDE_QUEST_XP, $"Completed side quest {quest.Name}!");
                else if (quest.Type == QuestType.Misc)
                    CharacterManager.AwardXP(DEFAULT_MISC_QUEST_XP, $"Completed miscellaneous quest {quest.Name}!");
                else
                    CharacterManager.AwardXP(DEFAULT_QUEST_XP, $"Completed quest {quest.Name}!");
                SetTrackingQuest(quest, false);
                return true;
            }
            return false;
        }

        public static bool IsTrackingQuest(Quest quest)
        {
            return QuestSaveData.Instance.TrackedQuests.Contains(quest.FullID);
        }

        public static void SetTrackingQuest(Quest quest, bool tracking)
        {
            if (tracking)
            {
                if (QuestSaveData.Instance.TrackedQuests.Add(quest.FullID))
                {
                    OnTrackQuest.Invoke(quest);
                }
            }
            else
            {
                if (QuestSaveData.Instance.TrackedQuests.Remove(quest.FullID))
                {
                    OnUntrackQuest.Invoke(quest);
                }
                
            }
        }
    }
}
