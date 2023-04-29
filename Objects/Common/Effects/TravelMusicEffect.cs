using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class TravelMusicEffect : EntityLike<TravelMusicEffect, TravelMusicEffectData>
    {
        public AudioType AudioType;

        public override void Load(TravelMusicEffectData data, string modID)
        {
            AudioType = EnumUtils.Parse<AudioType>(data.audioType, true);
        }
    }
}
