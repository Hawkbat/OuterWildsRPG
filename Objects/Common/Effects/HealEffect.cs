using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HealEffect : BuffEffect<HealEffect, HealEffectData>, IStatBuffEffect
    {
        public float Amount;

        public float Add => Amount;
        public float Multiply => 1f;

        public override void Load(HealEffectData data, string modID)
        {
            base.Load(data, modID);
            Amount = data.amount;
        }

        public override bool IsInstant() => true;

        public override string GetDescription() => Translations.EffectDescriptionHeal(Amount);
    }
}
