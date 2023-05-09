using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Objects.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common
{
    public static class SaveDataManager
    {

        public static void Init()
        {
            OuterWildsRPG.ModSaveUtility.OnLoad.AddListener(OnLoad);
            OuterWildsRPG.ModSaveUtility.OnSave.AddListener(OnSave);
            OuterWildsRPG.ModSaveUtility.OnReset.AddListener(OnReset);
        }

        public static void SetUp()
        {
            Load();
        }

        static void OnLoad(string profileName)
        {
            OuterWildsRPG.LogSuccess($"Loaded save data for profile {profileName}");
            Load();
        }

        static void OnSave(string profileName)
        {
            OuterWildsRPG.LogSuccess($"Saved save data for profile {profileName}");
            Save();
        }

        static void OnReset(string profileName)
        {
            OuterWildsRPG.LogSuccess($"Reset save data for profile {profileName}");
            OuterWildsRPG.ModSaveUtility.ResetAllValues();
        }

        public static void Save()
        {
            OuterWildsRPG.ModSaveUtility.WriteValue(nameof(CharacterSaveData), CharacterSaveData.Instance);
            OuterWildsRPG.ModSaveUtility.WriteValue(nameof(DropSaveData), DropSaveData.Instance);
            OuterWildsRPG.ModSaveUtility.WriteValue(nameof(PerkSaveData), PerkSaveData.Instance);
            OuterWildsRPG.ModSaveUtility.WriteValue(nameof(QuestSaveData), QuestSaveData.Instance);
            OuterWildsRPG.ModSaveUtility.WriteValue(nameof(ShopSaveData), ShopSaveData.Instance);
        }

        public static void Load()
        {

            CharacterSaveData.Instance = OuterWildsRPG.ModSaveUtility.ReadValue(nameof(CharacterSaveData), new CharacterSaveData());
            DropSaveData.Instance = OuterWildsRPG.ModSaveUtility.ReadValue(nameof(DropSaveData), new DropSaveData());
            PerkSaveData.Instance = OuterWildsRPG.ModSaveUtility.ReadValue(nameof(PerkSaveData), new PerkSaveData());
            QuestSaveData.Instance = OuterWildsRPG.ModSaveUtility.ReadValue(nameof(QuestSaveData), new QuestSaveData());
            ShopSaveData.Instance = OuterWildsRPG.ModSaveUtility.ReadValue(nameof(ShopSaveData), new ShopSaveData());
        }
    }
}
