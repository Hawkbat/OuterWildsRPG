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
        public List<string> Hotbar = new();
        public List<string> Inventory = new();
        public Dictionary<string, float> Consumables = new();
        public HashSet<string> HasPickedUp = new();
        public HashSet<string> HasSeen = new();
        public HashSet<string> HasRead = new();
    }
}
