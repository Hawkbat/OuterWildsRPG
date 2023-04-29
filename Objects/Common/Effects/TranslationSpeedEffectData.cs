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
    public class TranslationSpeedEffectData : EntityLikeData
    {
        [Description("The percentage to multiply the total translation time by. E.g. a value of 0.25 will result in translating taking 25% of the usual time, or a 75% reduction.")]
        public float multiply = 1f;
    }
}
