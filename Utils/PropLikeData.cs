using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public abstract class PropLikeData : EntityLikeData
    {
        [Required]
        [Description("The full path to a GameObject in the solar system that the prop should be attached to.")]
        public string parentPath;

        [Description("The position to place the prop at.")]
        public Vector3Data position;

        [Description("The rotation of the prop.")]
        public Vector3Data rotation;

        [Description("Whether the position and rotation are relative to the parent, instead of the whole planet.")]
        public bool isRelativeToParent;

        [Description($"Whether to orient the prop to point 'up' away from the center of the planet, or use the provided rotation value. Defaults to true if `{nameof(rotation)}` is omitted and false otherwise.")]
        public bool? alignRadial;
    }
}
