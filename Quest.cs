using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OuterWildsRPG
{
    public class QuestFile
    {
        public List<Quest> Quests;
    }

    public class Quest
    {
        public string ID;
        public string Name;
        public List<QuestStep> Steps;
        public int? XP;

        public bool IsStarted => Steps.Any(s => s.IsStarted);
        public bool IsInProgress => !IsComplete && IsStarted;
        public bool IsComplete => QuestSaveData.HasCompletedQuest(this) || Steps.All(s => s.IsComplete || s.Optional);
    }

    public class QuestStep
    {
        public Quest Quest;

        public string ID;
        public string Text;
        public List<QuestCondition> RevealOn;
        public List<QuestCondition> CompleteOn;
        public string LocationEntry;
        public string LocationPath;
        public bool Optional;

        public bool IsStarted =>
            ((RevealOn == null || RevealOn.Count == 0) && Quest.IsStarted) ||
            (RevealOn != null && RevealOn.Count > 0 && RevealOn.Any(id => CheckCondition(id))) ||
            (CompleteOn != null && CompleteOn.Count > 0 && CompleteOn.Any(id => CheckCondition(id)));
        public bool IsInProgress => IsStarted && !IsComplete;
        public bool IsComplete => QuestSaveData.HasCompletedStep(this) ||
            CompleteOn == null || CompleteOn.Count == 0 || CompleteOn.Any(id => CheckCondition(id));

        public bool CheckCondition(QuestCondition c)
        {
            switch (c.Type)
            {
                case QuestConditionType.None:
                    return false;
                case QuestConditionType.Step:
                    return Quest.Steps.Find(s => s.ID == c.Value).IsComplete;
                case QuestConditionType.Fact:
                    return Locator.GetShipLogManager().IsFactRevealed(c.Value);
                case QuestConditionType.Entry:
                    return Locator.GetShipLogManager().GetEntry(c.Value).CalculateState() == ShipLogEntry.State.Explored;
                case QuestConditionType.EntryRumored:
                    return Locator.GetShipLogManager().GetEntry(c.Value).CalculateState() == ShipLogEntry.State.Rumored;
                case QuestConditionType.Frequency:
                    return PlayerData.KnowsFrequency((SignalFrequency)Enum.Parse(typeof(SignalFrequency), c.Value, true));
                case QuestConditionType.Signal:
                    return PlayerData.KnowsSignal((SignalName)Enum.Parse(typeof(SignalName), c.Value, true));
                case QuestConditionType.DialogueCondition:
                    return DialogueConditionManager.SharedInstance.GetConditionState(c.Value);
                case QuestConditionType.PersistentCondition:
                    return PlayerData.GetPersistentCondition(c.Value);
                case QuestConditionType.Special:
                    return CheckSpecialCondition((QuestSpecialConditionType)Enum.Parse(typeof(QuestSpecialConditionType), c.Value, true));
                default:
                    throw new ArgumentOutOfRangeException($"Unknown {nameof(QuestConditionType)}: {c.Type}");
            }
        }



        static SatelliteNode satelliteNode1;
        static SatelliteNode satelliteNode2;
        static SatelliteNode satelliteNode3;

        public static bool CheckSpecialCondition(QuestSpecialConditionType condition)
        {
            switch (condition)
            {
                case QuestSpecialConditionType.SatelliteRepair0:
                    if (!satelliteNode1)
                        satelliteNode1 = Utils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:0").GetComponent<SatelliteNode>();
                    return !satelliteNode1._damaged;
                case QuestSpecialConditionType.SatelliteRepair1:
                    if (!satelliteNode2)
                        satelliteNode2 = Utils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:1").GetComponent<SatelliteNode>();
                    return !satelliteNode2._damaged;
                case QuestSpecialConditionType.SatelliteRepair2:
                    if (!satelliteNode3)
                        satelliteNode3 = Utils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:2").GetComponent<SatelliteNode>();
                    return !satelliteNode3._damaged;
                default:
                    return false;
            }
        }
    }

    public class QuestCondition
    {
        public QuestConditionType Type;
        public string Value;
    }

    public class QuestFileData
    {
        [Required]
        [Description("A list of quests included in this mod.")]
        public List<QuestData> quests = new();

        public QuestFile Parse()
        {
            return new QuestFile()
            {
                Quests = quests.Select(q => q.Parse()).ToList(),
            };
        }
    }

    public class QuestData
    {
        [Required]
        [Description("A unique identifier for this quest. This must not match the ID of any other quests in this mod.")]
        public string id;
        [Required]
        [Description("The name that will be displayed in the UI and quest log for this quest.")]
        public string name;
        [Required]
        [Description("The list of steps that must be completed for this quest to be marked as complete.")]
        public List<QuestStepData> steps = new();
        [Description("The amount of XP to award to the player for completing this quest. If no value is provided, a default XP reward will be given.")]
        public int? xp;

        public Quest Parse()
        {
            var quest = new Quest()
            {
                ID = id,
                Name = name,
                Steps = steps.Select(s => s.Parse()).ToList(),
                XP = xp,
            };
            foreach (var step in quest.Steps) step.Quest = quest;
            return quest;
        }
    }

    public class QuestStepData
    {
        [Required]
        [Description("A unique identifier for this step. This must not match the ID of any other steps in this quest.")]
        public string id;
        [Required]
        [Description("The instructions that will be displayed in the UI and quest log for this step.")]
        public string text;
        [Description("A list of conditions that will cause this step to be displayed, if any are met. If no conditions are specified, the step will be revealed as soon as the quest is started.")]
        public List<QuestConditionData> revealOn = new();
        [Required]
        [Description("A list of conditions that will flag the step as completed, if any are met. If no conditions are specified, the step will complete immediately.")]
        public List<QuestConditionData> completeOn = new();
        [Description("The ID of an entry that will be targeted by a quest marker when this step is active. This will match the location targeted when using the \"Mark on HUD\" feature in the ship log.")]
        public string locationEntry;
        [Description("The full path of a GameObject that will be targeted by a quest marker when this step is active. If an object has identically named siblings, you can specify the Nth child of that name with a colon, like: \"iningRig_Body/Nodes/BrokenNode:2\".")]
        public string locationPath;
        [Description("Whether the step must be completed for the quest to be completed")]
        public bool optional;

        public QuestStep Parse()
        {
            return new QuestStep()
            {
                ID = id,
                Text = text,
                RevealOn = revealOn.Select(c => c.Parse()).ToList(),
                CompleteOn = completeOn.Select(c => c.Parse()).ToList(),
                LocationEntry = locationEntry,
                LocationPath = locationPath,
                Optional = optional,
            };
        }
    }

    public class QuestConditionData
    {
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public QuestConditionType type;
        public string value;

        public QuestCondition Parse()
        {
            return new QuestCondition()
            {
                Type = type,
                Value = value,
            };
        }
    }

    [Description("The type of element to check for. E.g. 'step' will check for the completion state of the step with the given name.")]
    public enum QuestConditionType
    {
        None,
        Step,
        Fact,
        Entry,
        EntryRumored,
        Frequency,
        Signal,
        DialogueCondition,
        PersistentCondition,
        Special,
    }

    public enum QuestSpecialConditionType
    {
        None,
        SatelliteRepair0,
        SatelliteRepair1,
        SatelliteRepair2,
    }
}
