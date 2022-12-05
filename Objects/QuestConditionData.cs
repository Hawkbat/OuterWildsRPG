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
