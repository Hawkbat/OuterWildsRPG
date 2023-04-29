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
    public class DropLocationData : EntityLikeData
    {
        [Required]
        [Description("The full path to a GameObject in the solar system that the drop should be attached to.")]
        public string parentPath;
        [Description("The position to place the drop at, relative to the parent.")]
        public Vector3Data position;
        [Description("The rotation of the drop, relative to the parent.")]
        public Vector3Data rotation;
        [Description("Full paths to GameObjects to disable when picking up this drop.")]
        public List<string> visuals = new();
    }
}
