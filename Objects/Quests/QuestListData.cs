using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestListData : MultipleEntityData<QuestData>
    {
        [Required]
        [Description("A list of quests included in this mod.")]
        public List<QuestData> quests = new();

        public override IEnumerable<QuestData> GetEntities() => quests;
    }
}
