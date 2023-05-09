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
    [Description("A file named 'shops.json' in the root of a mod folder that contains a list of shops to load.")]
    public class ShopListData : MultipleEntityData<ShopData>
    {
        [Required]
        [Description("A list of shops included in this mod.")]
        public List<ShopData> shops = new();

        public override IEnumerable<ShopData> GetEntities() => shops;
    }
}
