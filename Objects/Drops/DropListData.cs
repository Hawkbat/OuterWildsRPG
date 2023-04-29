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
    public class DropListData : MultipleEntityData<DropData>
    {
        [Required]
        [Description("A list of drops included in this mod.")]
        public List<DropData> drops = new();

        public override IEnumerable<DropData> GetEntities() => drops;
    }
}
