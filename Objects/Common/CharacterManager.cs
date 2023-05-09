using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Common
{
    public class CharacterManager
    {
        const int LEVEL_CAP = 255;

        public class AwardXPEvent : UnityEvent<int, string> { }
        public static AwardXPEvent OnAwardXP = new AwardXPEvent();

        public class LevelUpEvent : UnityEvent<int> { }
        public static LevelUpEvent OnLevelUp = new LevelUpEvent();

        public static int GetTotalNeededXP(int level)
        {
            if (level <= 0) return 0;
            return 2000 * level + 100 * (level - 1) * (level - 1);
        }

        public static int GetNeededXP(int level)
        {
            return GetTotalNeededXP(level) - GetTotalNeededXP(level - 1);
        }

        public static int GetCurrentXP()
        {
            return CharacterSaveData.Instance.TotalXP - GetTotalNeededXP(GetCharacterLevel());
        }

        public static int GetCharacterLevel()
        {
            for (int level = 1; level <= LEVEL_CAP; level++)
            {
                if (GetTotalNeededXP(level) > CharacterSaveData.Instance.TotalXP)
                {
                    return level - 1;
                }
            }
            return LEVEL_CAP;
        }

        public static void AwardXP(int amount, string reason)
        {
            var beforeLevel = GetCharacterLevel();
            CharacterSaveData.Instance.TotalXP += amount;
            SaveDataManager.Save();
            OnAwardXP.Invoke(amount, reason);
            var afterLevel = GetCharacterLevel();
            for (var level = beforeLevel + 1; level <= afterLevel; level++)
            {
                OnLevelUp.Invoke(level);
            }
        }

        public static void Update()
        {

        }
    }
}
