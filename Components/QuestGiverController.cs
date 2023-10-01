using OuterWildsRPG.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class QuestGiverController : MonoBehaviour
    {
        Quest quest;

        public Quest GetQuest() => quest;

        public void Init(Quest quest)
        {
            this.quest = quest;
        }
    }
}
