using OuterWildsRPG.Objects.Common.Dialogue;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Shops
{
    public class ShopData : EntityData
    {
        [Required]
        [Description("The full path to the character to use as a shopkeeper for this shop.")]
        public string characterPath;

        [Description("If present, clones the character and repositions them instead of using the original as-is.")]
        public ShopkeeperData characterClone;

        [Required]
        [Description("A mod-relative file path to an .xml file containing the dialogue tree to use when speaking to the shopkeeper.")]
        public string dialogueXmlPath;

        [Description("Whether the shopkeeper's dialogue tree should be merged with the existing character's dialogue. Otherwise, it replaces it entirely.")]
        public bool mergeDialogue;

        [Required]
        [Description("The drops to sell in this shop.")]
        public List<ShopItemData> items = new();
    }
}
