using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class JetpackBoostDurationEffect : StatBuffEffect<JetpackBoostDurationEffect, JetpackBoostDurationEffectData>
    {
        public override void Load(JetpackBoostDurationEffectData data, string modID)
        {
            base.Load(data, modID);
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionJetpackBoostDuration(Add, Multiply);

    }
}
