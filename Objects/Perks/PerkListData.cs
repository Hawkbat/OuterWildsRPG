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
    [Description("A file named 'perks.json' in the root of a mod folder that contains a list of perks to load.")]
    public class PerkListData : MultipleEntityData<PerkData>
    {
        [Required]
        [Description("A list of perks included in this mod.")]
        public List<PerkData> perks = new();

        public override IEnumerable<PerkData> GetEntities() => perks;
    }
}
