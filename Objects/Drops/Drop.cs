using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Drops
{
    public class Drop : Entity<Drop, DropData>
    {
        public string Description;
        public DropRarity Rarity;
        public EquipSlot EquipSlot;
        public List<Buff> Buffs = new();
        public List<DropLocation> Locations = new();

        public override void Load(DropData data, string modID)
        {
            base.Load(data, modID);
            Description = data.description;
            EquipSlot = data.equipSlot;
            Rarity = data.rarity;
            Buffs = data.buffs.Select(d => Buff.LoadNew(d, modID)).ToList();
            Locations = data.locations.Select(l => DropLocation.LoadNew(l, modID)).ToList();
            foreach (var location in Locations)
                location.Drop = this;
        }

        public override void Resolve(DropData data, Dictionary<string, Drop> entities)
        {

        }
    }
}
