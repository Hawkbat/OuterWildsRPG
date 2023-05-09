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
    [Description("A quest that can be completed by the player for various rewards.")]
    public class QuestData : EntityData
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Description("How significant this quest is, which determines ship log priority and XP rewards.")]
        public QuestType type;
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Description("The visual and audible theme used to present this quest to the player.")]
        public QuestTheme theme;
        [Required]
        [Description("The list of steps that must be completed for this quest to be marked as complete.")]
        public List<QuestStepData> steps = new();
    }
}
