using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Drops
{
    public class DropLocation : PropLike<DropLocation, DropLocationData>
    {
        public Drop Drop;
        public bool Respawns;
        public List<string> Visuals = new();

        public string GetUniqueKey() => $"{Drop.FullID}${Drop.Locations.IndexOf(this)}";

        public override void Load(DropLocationData data, string modID)
        {
            base.Load(data, modID);
            Respawns = data.respawns;
            Visuals = data.visuals;
        }
    }
}
