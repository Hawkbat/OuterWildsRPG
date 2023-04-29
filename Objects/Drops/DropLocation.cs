using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Drops
{
    public class DropLocation : EntityLike<DropLocation, DropLocationData>
    {
        public Drop Drop;
        public string ParentPath;
        public Vector3 Position;
        public Vector3 Rotation;
        public List<string> Visuals = new();

        public string GetUniqueKey() => $"{Drop.FullID}${Drop.Locations.IndexOf(this)}";

        public override void Load(DropLocationData data, string modID)
        {
            ParentPath = data.parentPath;
            Position = data.position.ToVector3();
            Rotation = data.rotation.ToVector3();
            Visuals = data.visuals;
        }
    }
}
