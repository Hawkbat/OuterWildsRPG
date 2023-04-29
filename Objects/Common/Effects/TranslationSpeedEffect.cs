using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class TranslationSpeedEffect : EntityLike<TranslationSpeedEffect, TranslationSpeedEffectData>
    {
        public float Multiply;

        public override void Load(TranslationSpeedEffectData data, string modID)
        {
            Multiply = data.multiply;
        }
    }
}
