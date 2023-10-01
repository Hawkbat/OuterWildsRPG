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
using System.ComponentModel;

namespace OuterWildsRPG.Objects.Common
{
    [Description("Various possible effects that will apply when the buff is active.")]
    public class BuffData : EntityLikeData
    {
        public CustomEffectData custom;
        public FogDensityEffectData fogDensity;
        public GiveDropEffectData giveDrop;
        public HazardDamageEffectData hazardDamage;
        public HealEffectData heal;
        public HoldBreathEffectData holdBreath;
        public InventorySpaceEffectData inventorySpace;
        public JumpSpeedEffectData jumpSpeed;
        public MaxHealthEffectData maxHealth;
        public MoveSpeedEffectData moveSpeed;
        public TranslationSpeedEffectData translationSpeed;
        public TravelMusicEffectData travelMusic;
    }
}
