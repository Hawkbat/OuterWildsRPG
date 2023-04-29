using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OuterWildsRPG.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.Utils;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestStepData : EntityLikeData
    {
        [Required]
        [Description("A unique identifier for this step. This must not match the ID of any other steps in this quest.")]
        public string id;
        [Required]
        [Description("The instructions that will be displayed in the UI and quest log for this step.")]
        public string text;
        [Description("A list of conditions that will cause this step to be displayed if they are met. If no conditions are specified, the step will be revealed as soon as the quest is started.")]
        public List<QuestConditionData> startOn = new();
        [Description("Whether the step will be displayed if any conditions are met, or if all are met.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [DefaultValue(QuestConditionMode.Any)]
        public QuestConditionMode startMode;
        [Required]
        [Description("A list of conditions that will flag the step as completed if they are met. If no conditions are specified, the step will complete immediately.")]
        public List<QuestConditionData> completeOn = new();
        [Description("Whether the step will be completed if any conditions are met, or if all are met.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [DefaultValue(QuestConditionMode.Any)]
        public QuestConditionMode completeMode;
        [Description("The ID of an entry that will be targeted by a quest marker when this step is active. This will match the location targeted when using the \"Mark on HUD\" feature in the ship log.")]
        public string locationEntry;
        [Description("The full path of a GameObject that will be targeted by a quest marker when this step is active. If an object has identically named siblings, you can specify the Nth child of that name with a colon, like: \"MiningRig_Body/Nodes/BrokenNode:2\".")]
        public string locationPath;
        [Description("Whether the step must be completed for the quest to be completed.")]
        [DefaultValue(false)]
        public bool optional;
        [Description("Whether this step is a hint, which will be displayed differently from checklist steps.")]
        public bool isHint;
        [Description("Whether to prevent the step from being completed early if its completion conditions are met but not its start conditions.")]
        public bool preventEarlyComplete;
    }
}
