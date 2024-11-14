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
    [Description("Modifies the maximum fuel of the player.")]
    public class MaxFuelEffectData : EntityLikeData
    {
        [DefaultValue(0f)]
        [Description("The percentage of the base fuel to add/reduce. E.g. a value of -0.1 means subtract 10% of the base fuel from the total.")]
        public float add = 0f;

        [DefaultValue(1f)]
        [Description("The percentage to multiply the total max fuel by. E.g. a value of 0.25 will result in 25% of base max fuel, or a 75% reduction.")]
        public float multiply = 1f;
    }
}
