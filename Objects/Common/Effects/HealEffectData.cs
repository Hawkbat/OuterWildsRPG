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
    [Description("Restores a percentage of the player's health.")]
    public class HealEffectData : EntityLikeData
    {
        [Required]
        [Description("The percentage of total health to restore. E.g. 0.2 will restore 20% of maximum health.")]
        public float amount;
    }
}
