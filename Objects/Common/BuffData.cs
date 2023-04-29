using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OWML.Utils;
using OuterWildsRPG.Utils;
using OuterWildsRPG.Objects.Common.Effects;

namespace OuterWildsRPG.Objects.Common
{
    public class BuffData : EntityData
    {
        public HazardDamageEffectData hazardDamage;
        public TranslationSpeedEffectData translationSpeed;
        public TravelMusicEffectData travelMusic;
        public CustomEffectData custom;
    }
}
