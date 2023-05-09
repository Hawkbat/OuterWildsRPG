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
    [Description("A condition that must be met for the quest step to be started/completed.")]
    public class QuestConditionData : EntityLikeData
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Description("The type of element to check for. E.g. 'step' will check for the completion state of the step with the given ID.")]
        public QuestConditionType type;

        [Required]
        [Description("An appropriate ID for the type specified in 'type'. E.g. a ship log entry ID for the 'entry' type or a SignalName enum value for the 'signal' type.")]
        public string value;

        [Description("The ID of an entry that will be targeted by a quest marker when this condition's step is active. This will match the location targeted when using the \"Mark on HUD\" feature in the ship log.")]
        public string markerEntry;

        [Description("The full path of a GameObject that will be targeted by a quest marker when this condition's step is active. If an object has identically named siblings, you can specify the Nth child of that name with a colon, like: \"MiningRig_Body/Nodes/BrokenNode:2\".")]
        public string markerPath;

        [Description("Conditions automatically place quest markers based on the condition type. Set this property to true to disable these auto-placed markers.")]
        public bool disableAutoMarkers;
    }
}
