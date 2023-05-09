using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    [Description("Changes the travel music played when flying the ship between planets.")]
    public class TravelMusicEffectData : EntityLikeData
    {
        [Required]
        [Description("An Outer Wilds AudioType value that will be used as the travel music will this buff is active. See https://nh.outerwildsmods.com/reference/audio_enum.html")]
        public string audioType;
    }
}
