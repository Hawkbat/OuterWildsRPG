using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Shops
{
    public class ShopItemData : EntityLikeData
    {
        [Required]
        [Description("The unique ID of the drop to sell.")]
        public string drop;
        [DefaultValue(1)]
        [Description("The number of times the drop can be bought.")]
        public int stock = 1;
        [DefaultValue(false)]
        [Description("Whether this drop is restocked at the start of each loop or keeps the current stock between loops.")]
        public bool restocks;
    }
}
