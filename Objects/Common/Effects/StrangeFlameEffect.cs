using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class StrangeFlameEffect : BuffEffect<StrangeFlameEffect, StrangeFlameEffectData>
    {

        public override void Load(StrangeFlameEffectData data, string modID)
        {
            base.Load(data, modID);
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionStrangeFlame();
    }
}
