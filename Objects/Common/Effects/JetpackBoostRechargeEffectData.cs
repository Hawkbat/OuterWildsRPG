using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    [Description("Modifies the amount of time it takes for a player's jetpack to recharge after boosting.")]
    public class JetpackBoostRechargeEffectData : EntityLikeData
    {
        [Required]
        [DefaultValue(1f)]
        [Description("The percentage to multiply the total jetpack recharge duration by. E.g. a value of 0.25 will result in 25% jetpack recharge duration, or a 75% reduction.")]
        public float multiply = 1f;
    }
}
