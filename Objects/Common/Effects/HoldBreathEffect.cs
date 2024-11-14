using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HoldBreathEffect : BuffEffect<HoldBreathEffect, HoldBreathEffectData>, IStatBuffEffect
    {
        public float Seconds;

        float IStatBuffEffect.Add => Seconds;
        float IStatBuffEffect.Multiply => 1f;

        public override void Load(HoldBreathEffectData data, string modID)
        {
            base.Load(data, modID);
            Seconds = data.seconds;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionHoldBreath(Seconds);

    }
}
