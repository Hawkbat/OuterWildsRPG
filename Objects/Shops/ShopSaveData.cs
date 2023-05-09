using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Shops
{
    public class ShopSaveData
    {
        public static ShopSaveData Instance = new();

        public int Currency = 10;

        public Dictionary<string, Dictionary<string, int>> Purchases = new();
        public Dictionary<string, Dictionary<string, int>> Sales = new();
    }
}
