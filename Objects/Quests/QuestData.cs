using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestData : EntityData
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Description("How significant this quest is, which determines ship log priority and XP rewards.")]
        public QuestType type;
        [Required]
        [Description("The list of steps that must be completed for this quest to be marked as complete.")]
        public List<QuestStepData> steps = new();
    }
}
