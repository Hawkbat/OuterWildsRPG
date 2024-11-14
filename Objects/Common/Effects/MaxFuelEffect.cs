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
    public class MaxFuelEffect : StatBuffEffect<MaxFuelEffect, MaxFuelEffectData>
    {
        public override void Load(MaxFuelEffectData data, string modID)
        {
            base.Load(data, modID);
            Add = data.add;
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionMaxFuel(Add, Multiply);
    }
}
