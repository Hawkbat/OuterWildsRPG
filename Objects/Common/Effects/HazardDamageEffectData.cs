using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HazardDamageEffectData : EntityLikeData
    {
        [Required]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        [Description("The type of hazard to modify damage for.")]
        public HazardType type;
        [Description("The percentage of the initial damage to add/reduce. E.g. a value of -0.1 means subtract 10% of the base damage from the total.")]
        public float add = 0f;
        [Description("The percentage to multiply the total damage by. E.g. a value of 0.25 will result in 25% damage, or a 75% reduction.")]
        public float multiply = 1f;

        public enum HazardType
        {
            All = 0,
            General = 1,
            GhostMatter = 2,
            Heat = 4,
            Fire = 8,
            Sandfall = 16,
            Electricity = 32
        }
    }
}
