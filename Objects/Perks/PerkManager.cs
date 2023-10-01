using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Perks
{
    public class PerkManager
    {
        public class PerkEvent : UnityEvent<Perk> { }
        public static PerkEvent OnUnlockPerk = new();
        public static PerkEvent OnRefundPerk = new();

        static readonly Dictionary<string, Perk> perks = new();

        public static Perk GetPerk(string id, string modID = null)
        {
            if (perks.TryGetValue(Entity.GetID(id, modID), out var perk))
            {
                return perk;
            }
            return null;
        }

        public static bool RegisterPerk(Perk perk)
        {
            if (!perks.ContainsKey(perk.FullID))
            {
                perks.Add(perk.FullID, perk);
                return true;
            }
            return false;
        }

        public static IEnumerable<Perk> GetAllPerks()
            => perks.Values;

        public static IEnumerable<Perk> GetUnlockedPerks()
            => PerkSaveData.Instance.UnlockedPerks.Select(s => GetPerk(s));

        public static int GetTotalPerkPoints()
            => CharacterManager.GetCharacterLevel();

        public static int GetSpentPerkPoints()
            => PerkSaveData.Instance.UnlockedPerks.Count;

        public static int GetUnspentPerkPoints()
            => GetTotalPerkPoints() - GetSpentPerkPoints();

        public static bool HasUnlockedPerk(Perk perk)
            => PerkSaveData.Instance.UnlockedPerks.Contains(perk.FullID);

        public static bool CanUnlockPerk(Perk perk)
        {
            if (HasUnlockedPerk(perk)) return false;
            if (GetUnspentPerkPoints() <= 0) return false;
            if (perk.Prereq != null && !HasUnlockedPerk(perk.Prereq)) return false;
            return true;
        }

        public static bool UnlockPerk(Perk perk)
        {
            if (PerkSaveData.Instance.UnlockedPerks.Add(perk.FullID))
            {
                SaveDataManager.Save();
                OnUnlockPerk.Invoke(perk);
                return true;
            }
            return false;
        }

        public static bool CanRefundPerk(Perk perk)
        {
            if (!HasUnlockedPerk(perk)) return false;
            return true;
        }

        public static bool RefundPerk(Perk perk)
        {
            if (PerkSaveData.Instance.UnlockedPerks.Remove(perk.FullID))
            {
                foreach (var dependent in perk.Dependents)
                    RefundPerk(dependent);
                SaveDataManager.Save();
                OnRefundPerk.Invoke(perk);
                return true;
            }
            return false;
        }

        public static bool HasSeenPerk(Perk perk)
            => PerkSaveData.Instance.HasSeen.Contains(perk.FullID);

        public static bool SeePerk(Perk perk)
        {
            if (PerkSaveData.Instance.HasSeen.Add(perk.FullID))
            {
                SaveDataManager.Save();
                return true;
            }
            return false;
        }

        public static bool UnseePerk(Perk perk)
        {
            if (PerkSaveData.Instance.HasSeen.Remove(perk.FullID))
            {
                SaveDataManager.Save();
                return true;
            }
            return false;
        }

        public static bool HasReadPerk(Perk perk)
            => PerkSaveData.Instance.HasRead.Contains(perk.FullID);

        public static bool ReadPerk(Perk perk)
        {
            if (PerkSaveData.Instance.HasRead.Add(perk.FullID))
            {
                SaveDataManager.Save();
                return true;
            }
            return false;
        }
    }
}
