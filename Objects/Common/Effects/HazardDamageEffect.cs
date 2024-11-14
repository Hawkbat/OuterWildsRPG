using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HazardType = HazardVolume.HazardType;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HazardDamageEffect : StatBuffEffect<HazardDamageEffect, HazardDamageEffectData>
    {
        public HazardType Type;

        public override void Load(HazardDamageEffectData data, string modID)
        {
            base.Load(data, modID);
            Type = (HazardType)data.type;
            Add = data.add;
            Multiply = data.multiply;
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionHazardDamage(Add, Multiply, Type);
    }
}
