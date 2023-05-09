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
    [Description("Adds a drop directly to the player's inventory.")]
    public class GiveDropEffectData : EntityLikeData
    {

        [Required]
        [Description("The unique ID of the drop to give.")]
        public string drop;

        [DefaultValue(1)]
        [Description("The number of copies to give.")]
        public int amount = 1;
    }
}
