using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class MaxOxygenEffect : StatBuffEffect<MaxOxygenEffect, MaxOxygenEffectData>
    {
        public override void Load(MaxOxygenEffectData data, string modID)
        {
            base.Load(data, modID);
            Add = data.add;
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionMaxOxygen(Add, Multiply);
    }
}
