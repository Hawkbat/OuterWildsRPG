using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Perks
{
    public class PerkData : EntityData
    {
        [Description("The minimum level requirement to unlock this perk.")]
        public int level;
        [Description("The ID of a perk that must be unlocked before this perk becomes available.")]
        public string prereq;
        public List<BuffData> buffs = new();
    }
}
