using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HealEffect : BuffEffect<HealEffect, HealEffectData>
    {
        public float Amount;

        public override void Load(HealEffectData data, string modID)
        {
            base.Load(data, modID);
            Amount = data.amount;
        }

        public override bool IsInstant() => true;

        public override string GetDescription() => Translations.EffectDescriptionHeal(Amount);
    }
}
