using OuterWildsRPG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Drops
{
    public class DropSaveData
    {
        public static DropSaveData Instance = new();

        public Dictionary<EquipSlot, string> Equipment = new();
        public List<string> Inventory = new();
    }
}
