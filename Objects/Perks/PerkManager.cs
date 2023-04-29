using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.Objects.Common;
using UnityEngine.Events;

namespace OuterWildsRPG.Objects.Perks
{
    public class PerkManager
    {
        public class PerkEvent : UnityEvent<Perk> { }
        public static PerkEvent OnUnlockPerk = new();

        static Dictionary<string, Perk> perks = new();

        public static Dictionary<string, Perk> Dictionary => perks;

        public static Perk GetPerk(string id, string modID)
        {
            if (!id.Contains("/")) id = $"{modID}/{id}";
            if (perks.TryGetValue(id, out var perk))
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
            => PerkSaveData.Instance.UnlockedPerks.Select(s => GetPerk(s, OuterWildsRPG.ModID));

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
                OnUnlockPerk.Invoke(perk);
                return true;
            }
            return false;
        }
    }
}
