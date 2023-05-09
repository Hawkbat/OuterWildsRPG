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
    [Description("A location in the solar system where this drop can be picked up by the player.")]
    public class DropLocationData : PropLikeData
    {
        [Description($"Whether this drop respawns at the start of every loop.")]
        public bool respawns;

        [Description("Full paths to GameObjects to disable when picking up this drop.")]
        public List<string> visuals = new();
    }
}
