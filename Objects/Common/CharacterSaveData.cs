using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common
{
    internal class CharacterSaveData
    {
        public static CharacterSaveData Instance = new();

        public int TotalXP;
        public HashSet<string> DiscoveredLocations = new();
        public HashSet<string> ExploredLocations = new();
    }
}
