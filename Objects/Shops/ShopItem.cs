using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Shops
{
    public class ShopItem : EntityLike<ShopItem, ShopItemData>
    {
        public Drop Drop;
        public int InitialStock = 1;
        public bool Restocks;
        public Shop Shop;

        public override void Load(ShopItemData data, string modID)
        {
            base.Load(data, modID);
            InitialStock = data.stock;
            Restocks = data.restocks;
        }

        public override void Resolve()
        {
            base.Resolve();
            Drop = DropManager.GetDrop(GetRawData().drop);
        }
    }
}
