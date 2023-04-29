using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class HazardDamageEffect : EntityLike<HazardDamageEffect, HazardDamageEffectData>
    {
        public HazardVolume.HazardType Type;
        public float Add;
        public float Multiply;

        public override void Load(HazardDamageEffectData data, string modID)
        {
            Type = (HazardVolume.HazardType)data.type;
            Add = data.add;
            Multiply = data.multiply;
        }
    }
}
