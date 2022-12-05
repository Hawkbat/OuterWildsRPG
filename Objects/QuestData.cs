using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OuterWildsRPG.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects
{
    

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
}
