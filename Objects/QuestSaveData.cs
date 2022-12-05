using OuterWildsRPG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects
{
    public class QuestSaveData
    {
        const int DEFAULT_QUEST_XP = 1000;
        const int DEFAULT_MAIN_QUEST_XP = 5000;
        const int DEFAULT_SIDE_QUEST_XP = 1000;
        const int DEFAULT_MISC_QUEST_XP = 200;
        const int DEFAULT_LOCATION_XP = 100;
        const int DEFAULT_LOCATION_SUBENTRY_XP = 50;
        const int DEFAULT_LOCATION_CURIOSITY_XP = 500;

        public static QuestSaveData Instance = new QuestSaveData();

        public class QuestStepEvent : UnityEvent<QuestStep> { }
        public static QuestStepEvent OnStartStep = new QuestStepEvent();
        public static QuestStepEvent OnCompleteStep = new QuestStepEvent();
        public class QuestEvent : UnityEvent<Quest> { }
        public static QuestEvent OnStartQuest = new QuestEvent();
        public static QuestEvent OnCompleteQuest = new QuestEvent();
        public class DiscoverLocationEvent : UnityEvent<ShipLogEntry> { }
        public static DiscoverLocationEvent OnDiscoverLocation = new DiscoverLocationEvent();
        public class AwardXPEvent : UnityEvent<int, string> { }
        public static AwardXPEvent OnAwardXP = new AwardXPEvent();

        public Dictionary<string, HashSet<string>> StartedSteps = new();
        public Dictionary<string, HashSet<string>> CompletedSteps = new();
        public HashSet<string> CompletedQuests = new();
        public HashSet<string> StartedQuests = new();
        public HashSet<string> TrackedQuests = new();
        public HashSet<string> DiscoveredLocations = new();
        public int XP;

        public static bool HasStartedStep(QuestStep step)
        {
            return Instance.StartedSteps.ContainsKey(step.Quest.ID) && Instance.StartedSteps[step.Quest.ID].Contains(step.ID);
        }

        public static bool StartStep(QuestStep step)
        {
            if (!Instance.StartedSteps.ContainsKey(step.Quest.ID))
                Instance.StartedSteps.Add(step.Quest.ID, new HashSet<string>());
            if (Instance.StartedSteps[step.Quest.ID].Add(step.ID))
            {
                OnStartStep.Invoke(step);
                return true;
            }
            return false;
        }

        public static bool HasCompletedStep(QuestStep step)
        {
            return Instance.CompletedSteps.ContainsKey(step.Quest.ID) && Instance.CompletedSteps[step.Quest.ID].Contains(step.ID);
        }

        public static bool CompleteStep(QuestStep step)
        {
            if (!Instance.CompletedSteps.ContainsKey(step.Quest.ID))
                Instance.CompletedSteps.Add(step.Quest.ID, new HashSet<string>());
            if (Instance.CompletedSteps[step.Quest.ID].Add(step.ID))
            {
                OnCompleteStep.Invoke(step);
                return true;
            }
            return false;
        }

        public static bool HasStartedQuest(Quest quest)
        {
            return Instance.StartedQuests.Contains(quest.ID);
        }

        public static bool StartQuest(Quest quest)
        {
            if (Instance.StartedQuests.Add(quest.ID))
            {
                OnStartQuest.Invoke(quest);
                return true;
            }
            return false;
        }

        public static bool HasCompletedQuest(Quest quest)
        {
            return Instance.CompletedQuests.Contains(quest.ID);
        }

        public static bool CompleteQuest(Quest quest)
        {
            if (Instance.CompletedQuests.Add(quest.ID))
            {
                OnCompleteQuest.Invoke(quest);
                if (quest.Type == QuestType.Main) AwardXP(DEFAULT_MAIN_QUEST_XP, $"Completed main quest {quest.Name}!");
                else if (quest.Type == QuestType.Side) AwardXP(DEFAULT_SIDE_QUEST_XP, $"Completed side quest {quest.Name}!");
                else if (quest.Type == QuestType.Misc) AwardXP(DEFAULT_MISC_QUEST_XP, $"Completed miscellaneous quest {quest.Name}!");
                else AwardXP(DEFAULT_QUEST_XP, $"Completed quest {quest.Name}!");
                return true;
            }
            return false;
        }

        public static bool IsTrackingQuest(Quest quest)
        {
            return Instance.TrackedQuests.Contains(quest.ID);
        }

        public static void SetTrackingQuest(Quest quest, bool tracking)
        {
            if (tracking)
                Instance.TrackedQuests.Add(quest.ID);
            else
                Instance.TrackedQuests.Remove(quest.ID);
        }

        public static bool HasDiscoveredLocation(ShipLogEntry entry)
        {
            return Instance.DiscoveredLocations.Contains(entry.GetID());
        }

        public static bool DiscoverLocation(ShipLogEntry entry)
        {
            if (Instance.DiscoveredLocations.Add(entry.GetID()))
            {
                OnDiscoverLocation.Invoke(entry);
                if (entry.IsCuriosity())
                    AwardXP(DEFAULT_LOCATION_CURIOSITY_XP, $"Discovered major location {entry.GetName(false)}!");
                else if (entry.HasParent())
                    AwardXP(DEFAULT_LOCATION_SUBENTRY_XP, $"Discovered minor location {entry.GetName(false)}!");
                else
                    AwardXP(DEFAULT_LOCATION_XP, $"Discovered location {entry.GetName(false)}!");
                return true;
            }
            return false;
        }

        public static void AwardXP(int amount, string reason)
        {
            Instance.XP += amount;
            OnAwardXP.Invoke(amount, reason);
        }
    }
}
