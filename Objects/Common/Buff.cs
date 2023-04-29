using OuterWildsRPG.Objects.Common.Effects;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common
{
    public class Buff : EntityLike<Buff, BuffData>
    {
        public HazardDamageEffect HazardDamage;
        public TranslationSpeedEffect TranslationSpeed;
        public TravelMusicEffect TravelMusic;
        public CustomEffect Custom;

        public override void Load(BuffData data, string modID)
        {
            HazardDamage = HazardDamageEffect.LoadNew(data.hazardDamage, modID);
            TranslationSpeed = TranslationSpeedEffect.LoadNew(data.translationSpeed, modID);
            TravelMusic = TravelMusicEffect.LoadNew(data.travelMusic, modID);
            Custom = CustomEffect.LoadNew(data.custom, modID);
        }
    }
}
