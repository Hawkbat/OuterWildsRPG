using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects
{

    public class QuestListData
    {
        [Required]
        [Description("A list of quests included in this mod.")]
        public List<QuestData> quests = new();
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
        [Description("How significant this quest is, which determines ship log priority and XP rewards.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public QuestType type;
        [Required]
        [Description("The list of steps that must be completed for this quest to be marked as complete.")]
        public List<QuestStepData> steps = new();

        public Quest Parse()
        {
            var quest = new Quest()
            {
                ID = id,
                Name = name,
                Type = type,
                Steps = steps.Select(s => s.Parse()).ToList(),
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
        public List<QuestConditionData> startOn = new();
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
            var step = new QuestStep()
            {
                ID = id,
                Text = text,
                StartOn = startOn.Select(c => c.Parse()).ToList(),
                CompleteOn = completeOn.Select(c => c.Parse()).ToList(),
                LocationEntry = locationEntry,
                LocationPath = locationPath,
                Optional = optional,
            };
            foreach (var cond in step.StartOn) cond.Step = step;
            foreach (var cond in step.CompleteOn) cond.Step = step;
            return step;
        }
    }

    public class QuestConditionData
    {
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Required]
        [Description("The type of element to check for. E.g. 'step' will check for the completion state of the step with the given name.")]
        public QuestConditionType type;
        [Required]
        [Description("An appropriate ID for the type specified in 'type'. E.g. a ship log entry ID for the 'entry' type or a SignalName enum value for the 'signal' type.")]
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
}
