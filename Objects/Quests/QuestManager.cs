using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
        public static QuestStepEvent OnStepProgressed = new();
        public static QuestStepEvent OnCompleteStep = new();
        public class QuestEvent : UnityEvent<Quest> { }
        public static QuestEvent OnStartQuest = new();
        public static QuestEvent OnCompleteQuest = new();
        public static QuestEvent OnTrackQuest = new();
        public static QuestEvent OnUntrackQuest = new();

        static Dictionary<string, Quest> quests = new();

        static HashSet<string> autoTrackedQuests = new();
        static Dictionary<string, List<Transform>> factTargets = new();

        public static Quest GetQuest(string id, string modID = null)
        {
            if (quests.TryGetValue(Entity.GetID(id, modID), out var quest))
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
                SaveDataManager.Save();
                OnStartStep.Invoke(step);
                SetTrackingQuest(step.Quest, true, true);
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
                SaveDataManager.Save();
                OnCompleteStep.Invoke(step);
                SetTrackingQuest(step.Quest, true, true);
                return true;
            }
            return false;
        }

        public static bool ProgressStep(QuestStep step)
        {
            OnStepProgressed.Invoke(step);
            return true;
        }

        public static bool HasStartedQuest(Quest quest)
        {
            return QuestSaveData.Instance.StartedQuests.Contains(quest.FullID);
        }

        public static bool StartQuest(Quest quest)
        {
            if (QuestSaveData.Instance.StartedQuests.Add(quest.FullID))
            {
                SaveDataManager.Save();
                OnStartQuest.Invoke(quest);
                SetTrackingQuest(quest, true, quest.Type != QuestType.Main);
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
                SaveDataManager.Save();
                OnCompleteQuest.Invoke(quest);
                if (quest.Type == QuestType.Main)
                    CharacterManager.AwardXP(DEFAULT_MAIN_QUEST_XP, Translations.XPGainMainQuest());
                else if (quest.Type == QuestType.Side)
                    CharacterManager.AwardXP(DEFAULT_SIDE_QUEST_XP, Translations.XPGainSideQuest());
                else if (quest.Type == QuestType.Misc)
                    CharacterManager.AwardXP(DEFAULT_MISC_QUEST_XP, Translations.XPGainMiscQuest());
                else
                    CharacterManager.AwardXP(DEFAULT_QUEST_XP, Translations.XPGainQuest());
                SetTrackingQuest(quest, false, true);
                SetTrackingQuest(quest, false, false);
                return true;
            }
            return false;
        }

        public static bool IsTrackingQuest(Quest quest, bool includeAuto = true)
        {
            if (includeAuto && autoTrackedQuests.Contains(quest.FullID)) return true;
            return QuestSaveData.Instance.TrackedQuests.Contains(quest.FullID);
        }

        public static bool SetTrackingQuest(Quest quest, bool tracking, bool auto)
        {
            bool success;
            if (tracking)
            {
                if (!quest.IsInProgress) return false;
                if (auto)
                    success = autoTrackedQuests.Add(quest.FullID);
                else
                    success = QuestSaveData.Instance.TrackedQuests.Add(quest.FullID);
            }
            else
            {
                if (auto)
                    success = autoTrackedQuests.Remove(quest.FullID);
                else
                    success = QuestSaveData.Instance.TrackedQuests.Remove(quest.FullID);
            }
            if (success)
            {
                if (tracking) OnTrackQuest.Invoke(quest);
                else OnUntrackQuest.Invoke(quest);
                if (!auto)
                    SaveDataManager.Save();
            }
            return success;
        }

        public static void SetUp()
        {
            RegisterAllFactTargets();
            GenerateAutoQuests();

            foreach (var quest in GetAllQuests()) quest.SetUp();
        }

        public static void Update()
        {
            foreach (var quest in GetAllQuests())
                quest.Update(PlayerStateUtils.IsPlayable);
        }

        public static void CleanUp()
        {
            factTargets.Clear();

            foreach (var quest in GetAllQuests()) quest.CleanUp();
        }

        static void GenerateAutoQuests()
        {
            var hearthianEntryIDs = new HashSet<string>()
            {
                "CT_CHERT",
                "TH_VILLAGE",
                "TH_ZERO_G_CAVE",
                "TH_IMPACT_CRATER",
                "TH_QUANTUM_SHARD",
                "TH_RADIO_TOWER",
                "TM_ESKER",
                "TM_NORTH_POLE",
                "BH_RIEBECK",
                "GD_GABBRO_ISLAND",
                "DB_FELDSPAR",
                "DB_FROZEN_JELLYFISH",
            };
            var missableFactIDs = new HashSet<string>()
            {
                "TH_VILLAGE_X3",
                "GD_GABBRO_ISLAND_X1",
                "GD_GABBRO_ISLAND_R1",
                "TM_ESKER_R1",
            };
            foreach (var entry in Locator.GetShipLogManager()._entryList)
            {
                var id = $"AUTO_ENTRY_{entry.GetID()}";
                if (quests.ContainsKey(id)) continue;

                var isHearthian = hearthianEntryIDs.Contains(entry.GetID());
                var isStranger = entry.GetCuriosityName() == CuriosityName.InvisiblePlanet;

                var entryFacts = entry.GetExploreFacts()
                    .Concat(Locator.GetShipLogManager()._factList
                    .Where(f => f.GetSourceID() == entry.GetID()))
                    .Where(f => !missableFactIDs.Contains(f.GetID()));

                var quest = Quest.LoadNew(new QuestData()
                {
                    id = id,
                    name = entry.GetName(false),
                    type = QuestType.Misc,
                    theme =
                        isHearthian ? QuestTheme.Hearthian :
                        isStranger ? QuestTheme.Stranger :
                        QuestTheme.Nomai,
                    steps = new List<QuestStepData>()
                    {
                        new QuestStepData()
                        {
                            id = "EXPLORE",
                            text = Translations.PromptExplore(entry.GetName(false)),
                            startMode = QuestConditionMode.Any,
                            startOn = new List<QuestConditionData>()
                            {
                                new QuestConditionData()
                                {
                                    type = QuestConditionType.Entry,
                                    value = entry.GetID(),
                                }
                            },
                            completeMode = QuestConditionMode.All,
                            completeOn = entryFacts.Select(f => new QuestConditionData() {
                                type = QuestConditionType.Fact,
                                value = f.GetID(),
                            }).ToList(),
                        }
                    }
                }, OuterWildsRPG.ModID);
                quest.Resolve();
                RegisterQuest(quest);
            }
            foreach (var astroEntries in Locator.GetShipLogManager()._entryList.GroupBy(e => e.GetAstroObjectID()))
            {
                if (string.IsNullOrEmpty(astroEntries.Key)) continue;
                var id = $"AUTO_BODY_{astroEntries.Key}";
                if (quests.ContainsKey(id)) continue;
                var astroObjectName = AstroObject.StringIDToAstroObjectName(astroEntries.Key);
                var displayName = AstroObject.AstroObjectNameToString(astroObjectName);

                var theme = astroObjectName switch
                {
                    AstroObject.Name.TimberHearth => QuestTheme.Hearthian,
                    AstroObject.Name.TimberMoon => QuestTheme.Hearthian,
                    AstroObject.Name.MapSatellite => QuestTheme.Hearthian,
                    AstroObject.Name.CustomString => QuestTheme.Nomai,
                    AstroObject.Name.Sun => QuestTheme.Nomai,
                    AstroObject.Name.WhiteHole => QuestTheme.Nomai,
                    AstroObject.Name.WhiteHoleTarget => QuestTheme.Nomai,
                    AstroObject.Name.QuantumMoon => QuestTheme.Nomai,
                    AstroObject.Name.ProbeCannon => QuestTheme.Nomai,
                    AstroObject.Name.VolcanicMoon => QuestTheme.Nomai,
                    AstroObject.Name.SunStation => QuestTheme.Nomai,
                    AstroObject.Name.DreamWorld => QuestTheme.Stranger,
                    AstroObject.Name.RingWorld => QuestTheme.Stranger,
                    _ => QuestTheme.Default,
                };

                var quest = Quest.LoadNew(new QuestData()
                {
                    id = id,
                    name = displayName,
                    type = QuestType.Side,
                    theme = theme,
                    steps = new List<QuestStepData>()
                    {
                        new QuestStepData()
                        {
                            id = "EXPLORE",
                            text = Translations.PromptExplore(displayName),
                            startMode = QuestConditionMode.Any,
                            startOn = astroEntries.Select(e => new QuestConditionData()
                            {
                                type = QuestConditionType.Entry,
                                value = e.GetID(),
                            }).ToList(),
                            completeMode = QuestConditionMode.All,
                            completeOn = astroEntries.Select(e => new QuestConditionData()
                            {
                                type = QuestConditionType.Entry,
                                value = e.GetID(),
                            }).ToList(),
                        }
                    }
                }, OuterWildsRPG.ModID);
                quest.Resolve();
                RegisterQuest(quest);
            }
        }

        public static List<Transform> GetFactTargets(string factID)
            => factTargets.TryGetValue(factID, out var value) ? value : new();

        static void RegisterAllFactTargets()
        {
            foreach (var text in GameObject.FindObjectsOfType<NomaiText>())
            {
                if (text._listDBConditions == null) continue;
                foreach (var cond in text._listDBConditions)
                    RegisterFactTarget(cond.DatabaseID, text.transform);
            }

            foreach (var dialogue in GameObject.FindObjectsOfType<CharacterDialogueTree>())
            {
                if (dialogue._mapDialogueNodes == null) continue;
                foreach (var node in dialogue._mapDialogueNodes.Values)
                {
                    if (node._dbEntriesToSet == null) continue;
                    foreach (var factID in node._dbEntriesToSet)
                        RegisterFactTarget(factID, dialogue._attentionPoint != null ? dialogue._attentionPoint : dialogue.transform);
                }
            }

            foreach (var signal in Locator.GetAudioSignals())
                RegisterFactTarget(signal._revealFactID, signal.transform);

            foreach (var platform in NomaiRemoteCameraPlatform.s_platforms)
            {
                if (platform._slavePlatform == null) continue;
                RegisterFactTarget(platform._slavePlatform._dataPointID, platform.transform);
            }

            foreach (var trigger in GameObject.FindObjectsOfType<ShipLogFactListTriggerVolume>())
                foreach (var factID in trigger._factIDs)
                    RegisterFactTarget(factID, trigger.transform);

            foreach (var trigger in GameObject.FindObjectsOfType<ShipLogFactObserveTrigger>())
                foreach (var factID in trigger._factIDs)
                    RegisterFactTarget(factID, trigger.transform);

            foreach (var trigger in GameObject.FindObjectsOfType<ShipLogFactOrbSlotTrigger>())
                foreach (var factID in trigger._factIDs)
                    RegisterFactTarget(factID, trigger.transform);

            foreach (var trigger in GameObject.FindObjectsOfType<ShipLogFactSnapshotTrigger>())
                foreach (var factID in trigger._factIDs)
                    RegisterFactTarget(factID, trigger.transform);

            foreach (var trigger in GameObject.FindObjectsOfType<ShipLogFactTriggerVolume>())
                RegisterFactTarget(trigger._factID, trigger.transform);

            foreach (var slideReel in GameObject.FindObjectsOfType<SlideCollectionContainer>())
                foreach (var factID in slideReel._shipLogOnComplete.Split(','))
                    RegisterFactTarget(factID, slideReel.transform);

            foreach (var pictureFrame in GameObject.FindObjectsOfType<PictureFrameDoorInterface>())
                foreach (var factID in pictureFrame._revealFactIDs)
                    RegisterFactTarget(factID, pictureFrame.transform);

            foreach (var peephole in GameObject.FindObjectsOfType<Peephole>())
                foreach (var factID in peephole._factIDs)
                    RegisterFactTarget(factID, peephole.transform);

            foreach (var gravityCannon in GameObject.FindObjectsOfType<GravityCannonController>())
            {
                RegisterFactTarget(gravityCannon._launchShipLogFact, gravityCannon.transform);
                RegisterFactTarget(gravityCannon._retrieveShipLogFact, gravityCannon.transform);
            }
        }

        static void RegisterFactTarget(string factID, Transform target)
        {
            if (string.IsNullOrEmpty(factID)) return;
            if (!factTargets.ContainsKey(factID))
                factTargets[factID] = new List<Transform>();
            factTargets[factID].Add(target);
        }
    }
}
