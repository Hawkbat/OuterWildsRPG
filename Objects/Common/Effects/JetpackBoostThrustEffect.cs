using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class JetpackBoostThrustEffect : StatBuffEffect<JetpackBoostThrustEffect, JetpackBoostThrustEffectData>
    {
        public override void Load(JetpackBoostThrustEffectData data, string modID)
        {
            base.Load(data, modID);
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionJetpackBoostThrust(Add, Multiply);

    }
}
