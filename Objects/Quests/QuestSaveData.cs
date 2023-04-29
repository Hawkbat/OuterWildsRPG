using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestSaveData
    {

        public static QuestSaveData Instance = new();

        public Dictionary<string, HashSet<string>> StartedSteps = new();
        public Dictionary<string, HashSet<string>> CompletedSteps = new();
        public HashSet<string> CompletedQuests = new();
        public HashSet<string> StartedQuests = new();
        public HashSet<string> TrackedQuests = new();

    }
}
