using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class EntityData : EntityLikeData
    {
        [Required]
        [Description("A unique internal identifier for this entity. This must not be the same as any other entity of the same type in your mod.")]
        public string id;
        [Required]
        [Description("The name to display in-game for this entity.")]
        public string name;
    }
}
