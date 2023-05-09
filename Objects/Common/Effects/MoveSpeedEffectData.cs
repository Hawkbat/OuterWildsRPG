﻿using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    [Description("Modifies the player's movement speed.")]
    public class MoveSpeedEffectData : EntityLikeData
    {
        [Required]
        [DefaultValue(1f)]
        [Description("The percentage to multiply the total movement speed by. E.g. a value of 0.25 will result in 25% movement speed, or a 75% reduction.")]
        public float multiply = 1f;
    }
}
