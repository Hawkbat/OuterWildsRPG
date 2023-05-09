using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Drops
{
    [Description("A file named 'drops.json' in the root of a mod folder that contains a list of drops to load.")]
    public class DropListData : MultipleEntityData<DropData>
    {
        [Required]
        [Description("A list of drops included in this mod.")]
        public List<DropData> drops = new();

        public override IEnumerable<DropData> GetEntities() => drops;
    }
}
