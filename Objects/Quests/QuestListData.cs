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
    [Description("A file named 'quests.json' in the root of a mod folder that contains a list of quests to load.")]
    public class QuestListData : MultipleEntityData<QuestData>
    {
        [Required]
        [Description("A list of quests included in this mod.")]
        public List<QuestData> quests = new();

        public override IEnumerable<QuestData> GetEntities() => quests;
    }
}
