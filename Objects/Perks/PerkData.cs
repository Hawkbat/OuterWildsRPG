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
    [Description("A perk that can be unlocked by the player using perk points to grant certain passive effects.")]
    public class PerkData : EntityData
    {
        [Description("The minimum level requirement to unlock this perk.")]
        public int level;

        [Description("A file path (relative to your mod's folder) to an image file to use for this perk's icon. Can be shared with other perks.")]
        public string iconPath;

        [Description("The color to use for this perk's background in-game.")]
        public ColorData color;

        [Description("The ID of a perk that must be unlocked before this perk becomes available.")]
        public string prereq;
        public List<BuffData> buffs = new();

    }
}
