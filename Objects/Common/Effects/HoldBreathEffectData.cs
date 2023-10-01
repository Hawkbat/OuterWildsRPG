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
    [Description("Modifies the duration that a player can survive with no oxygen before suffocating or drowning.")]
    public class HoldBreathEffectData : EntityLikeData
    {
        [Required]
        [Description("The number of seconds to add or remove from the total suffocation duration. E.g. a value of 0.5 will result in suffocating taking half a second longer.")]
        public float seconds;
    }
}
