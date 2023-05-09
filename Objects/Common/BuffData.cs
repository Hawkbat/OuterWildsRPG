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
        public MoveSpeedEffectData moveSpeed;
        public JumpSpeedEffectData jumpSpeed;
        public HazardDamageEffectData hazardDamage;
        public TranslationSpeedEffectData translationSpeed;
        public TravelMusicEffectData travelMusic;
        public InventorySpaceEffectData inventorySpace;
        public HealEffectData heal;
        public GiveDropEffectData giveDrop;
        public CustomEffectData custom;
    }
}
