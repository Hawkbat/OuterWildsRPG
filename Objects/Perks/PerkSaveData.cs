using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Perks
{
    public class PerkSaveData
    {
        public static PerkSaveData Instance = new();

        public HashSet<string> UnlockedPerks = new();
        public HashSet<string> HasSeen = new();
        public HashSet<string> HasRead = new();
    }
}
