using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class CustomEffect : EntityLike<CustomEffect, CustomEffectData>
    {
        public string Description;

        public override void Load(CustomEffectData data, string modID)
        {
            Description = data.description;
        }
    }
}
