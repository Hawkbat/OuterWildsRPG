using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class TravelMusicEffect : BuffEffect<TravelMusicEffect, TravelMusicEffectData>
    {
        public AudioType AudioType;

        public override void Load(TravelMusicEffectData data, string modID)
        {
            base.Load(data, modID);
            AudioType = EnumUtils.Parse<AudioType>(data.audioType, true);
        }

        public override bool IsInstant() => false;

        public override string GetDescription() => Translations.EffectDescriptionTravelMusic();
    }
}
