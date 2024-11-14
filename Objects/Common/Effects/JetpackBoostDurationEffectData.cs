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
    [Description("Modifies the amount of time a player can boost with the jetpack before recharging.")]
    public class JetpackBoostDurationEffectData : EntityLikeData
    {
        [Required]
        [DefaultValue(1f)]
        [Description("The percentage to multiply the total jetpack boost duration by. E.g. a value of 0.25 will result in 25% jetpack boost duration, or a 75% reduction.")]
        public float multiply = 1f;
    }
}
