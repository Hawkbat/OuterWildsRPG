using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    [Description("Defines a custom effect that can be interacted with via the mod API.")]
    public class CustomEffectData : EntityLikeData
    {
        [Description("A custom value that uniquely identifies this effect, for use with the mod API.")]
        public string id;
        [Description("The text to display in the UI for this effect.")]
        public string description;
    }
}
