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
    [Description("Increases the capacity of the player's inventory.")]
    public class InventorySpaceEffectData : EntityLikeData
    {
        [Required]
        [Description("The number of inventory spaces to add.")]
        public int amount;
    }
}
