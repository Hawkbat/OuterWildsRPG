using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class FogDensityEffect : BuffEffect<FogDensityEffect, FogDensityEffectData>
    {
        public float Multiply = 1f;

        public override void Load(FogDensityEffectData data, string modID)
        {
            base.Load(data, modID);
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionFogDensity(Multiply);

    }
}
