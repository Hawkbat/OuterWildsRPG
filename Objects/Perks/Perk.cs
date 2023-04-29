using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Perks
{
    public class Perk : Entity<Perk, PerkData>
    {
        public int Level;
        public Perk Prereq;
        public List<Buff> Buffs = new();

        public List<Perk> Dependents = new();

        public override void Load(PerkData data, string modID)
        {
            base.Load(data, modID);
            Level = data.level;
            Prereq = null;
            Buffs = data.buffs.Select(b => Buff.LoadNew(b, modID)).ToList();
        }

        public override void Resolve(PerkData data, Dictionary<string, Perk> entities)
        {
            if (!string.IsNullOrEmpty(data.prereq))
            {
                Prereq =
                    entities.TryGetValue(data.prereq, out Perk perk) ? perk :
                    entities.TryGetValue($"{ModID}/{data.prereq}", out Perk perk2) ? perk2 :
                    null;
                Prereq?.Dependents.Add(this);
            }
        }
    }
}
