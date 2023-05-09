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
    [Description("Defines a custom effect that can be interacted with via the mod API.")]
    public class CustomEffectData : EntityLikeData
    {
        [Required]
        [Description("A custom value that uniquely identifies this effect. Also used as the description's translation key.")]
        public string id;

        [Required]
        [Description("The text to display in the UI for this effect.")]
        public string description;
    }
}
