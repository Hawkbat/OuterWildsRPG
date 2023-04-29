using HarmonyLib;
using OuterWildsRPG.Components;
using OuterWildsRPG.Enums;
using OuterWildsRPG.External;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OuterWildsRPG
{
    public class OuterWildsRPG : ModBehaviour
    {
        public static OuterWildsRPG Instance;

        public static bool Ready;

        public static QuestHUD QuestHUD;
        public static QuestLogMode QuestLogMode;

        public static NotificationQueue MajorQueue = new();
        public static NotificationQueue MinorQueue = new() { ConcurrentLimit = 4 };
        public static NotificationQueue ExpQueue = new() { ConcurrentLimit = 3 };

        public static string ModID => Instance.ModHelper.Manifest.UniqueName;

        static bool inDialogue;
        static bool inComputer;

        private void Awake()
        {
            Instance = this;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        void LoadEntities<TEntity, TSingle, TMultiple>(Dictionary<string, TEntity> entities, string type) where TEntity : Entity<TEntity, TSingle>, new() where TSingle : EntityData, new() where TMultiple : MultipleEntityData<TSingle>, new()
        {
            Dictionary<string, TSingle> rawData = new();

            foreach (var mod in ModHelper.Interaction.GetMods())
            {
                var modID = mod.ModHelper.Manifest.UniqueName;

                List<TEntity> modEntities = new List<TEntity>();

                var dataPath = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, $"./{type}.json"));
                if (File.Exists(dataPath))
                {
                    var multipleData = mod.ModHelper.Storage.Load<TMultiple>($"{type}.json", false);
                    if (multipleData != null)
                    {
                        foreach (var singleData in multipleData.GetEntities())
                        {
                            var entity = new TEntity();
                            entity.Load(singleData, modID);
                            rawData.Add(entity.FullID, singleData);
                            entities.Add(entity.FullID, entity);
                            modEntities.Add(entity);
                        }
                    } else
                    {
                        ModHelper.Console.WriteLine($"Failed to load {type}.json for {modID}.", MessageType.Error);
                    }
                }
                var dataDirectory = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, $"./{type}/"));
                if (Directory.Exists(dataDirectory))
                {
                    foreach (var filePath in Directory.GetFiles(dataDirectory))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var singleData = JsonHelper.LoadJsonObject<TSingle>(filePath, false);
                        if (singleData != null)
                        {
                            var entity = new TEntity();
                            entity.Load(singleData, modID);
                            rawData.Add(entity.FullID, singleData);
                            entities.Add(entity.FullID, entity);
                            modEntities.Add(entity);
                        } else
                        {
                            ModHelper.Console.WriteLine($"Failed to load {type}/{fileName} for {modID}.", MessageType.Error);
                        }
                    }
                }
                if (modEntities.Any())
                    ModHelper.Console.WriteLine($"Loaded {modEntities.Count} {type} from {modID}.", MessageType.Success);
            }

            foreach (var (fullID, entity) in entities) {
                var raw = rawData[fullID];
                entity.Resolve(raw, entities);
            }

            ModHelper.Console.WriteLine($"Loaded {entities.Count} total {type}.", MessageType.Success);
        }

        private void Start()
        {
            Assets.LoadAll();

            LoadEntities<Quest, QuestData, QuestListData>(QuestManager.Dictionary, "quests");
            LoadEntities<Perk, PerkData, PerkListData>(PerkManager.Dictionary, "perks");
            LoadEntities<Drop, DropData, DropListData>(DropManager.Dictionary, "drops");

            QuestManager.OnStartQuest.AddListener(OnStartQuest);
            QuestManager.OnCompleteStep.AddListener(OnCompleteStep);
            QuestManager.OnCompleteQuest.AddListener(OnCompleteQuest);
            CharacterManager.OnAwardXP.AddListener(OnAwardXP);
            CharacterManager.OnLevelUp.AddListener(OnLevelUp);
            CharacterManager.OnDiscoverLocation.AddListener(OnDiscoverLocation);
            CharacterManager.OnExploreLocation.AddListener(OnExploreLocation);
            PerkManager.OnUnlockPerk.AddListener(OnUnlockPerk);
            DropManager.OnPickUpDrop.AddListener(OnPickUp);
            DropManager.OnEquipDrop.AddListener(OnEquipDrop);
            DropManager.OnUnequipDrop.AddListener(OnUnequipDrop);

            GlobalMessenger.AddListener("EnterConversation", () => inDialogue = true);
            GlobalMessenger.AddListener("ExitConversation", () => inDialogue = false);
            GlobalMessenger.AddListener("EnterShipComputer", () => inComputer = true);
            GlobalMessenger.AddListener("ExitShipComputer", () => inComputer = false);

            LoadManager.OnStartSceneLoad += (scene, loadScene) =>
            {
                CleanUp();
            };

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                UnityUtils.RunWhen(() =>
                    Locator.GetPromptManager() &&
                    Locator.GetShipLogManager() &&
                    Locator.GetMarkerManager(),
                    () => SetUp());
            };

            ModHelper.Console.WriteLine($"{nameof(OuterWildsRPG)} is initialized.", MessageType.Success);
        }

        private void SetUp()
        {
            if (!QuestHUD)
            {
                var questHudRect = UnityUtils.MakeRectChild(null, "QuestHUD");
                QuestHUD = questHudRect.gameObject.AddComponent<QuestHUD>();
            }

            foreach (var drop in DropManager.GetAllDrops())
                foreach (var location in drop.Locations)
                    DropManager.SpawnDropPickup(location);

            foreach (var quest in QuestManager.GetAllQuests()) quest.SetUp();

            BuffManager.UpdateTravelMusic();

            var questLogGO = new GameObject("QuestLogMode");
            questLogGO.transform.parent = UnityUtils.GetTransformAtPath("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            questLogGO.transform.localPosition = Vector3.zero;
            questLogGO.transform.localRotation = Quaternion.identity;
            questLogGO.transform.localScale = Vector3.one;
            QuestLogMode = questLogGO.AddComponent<QuestLogMode>();

            var customModesAPI = ModHelper.Interaction.TryGetModApi<ICustomShiplogModesAPI>("dgarro.CustomShipLogModes");
            customModesAPI.AddMode(QuestLogMode, () => true, () => "Quest Log");
            QuestLogMode.ShiplogModesAPI = customModesAPI;

            var perkTreeMode = GraphMode.Create<PerkTreeMode>();
            customModesAPI.AddMode(perkTreeMode, () => true, () => "Perk Tree");

            // DEBUG; Export ship log details to reference in quest steps
            //ShipLogExporter.Export();

            Ready = true;
        }

        private void CleanUp()
        {
            Ready = false;

            foreach (var quest in QuestManager.GetAllQuests()) quest.CleanUp();
        }

        private void Update()
        {
            if (!Ready) return;

            bool markersVisible = !GUIMode.IsHiddenMode() && !ModHelper.Menus.PauseMenu.IsOpen && !inDialogue && !inComputer;

            foreach (var quest in QuestManager.GetAllQuests())
                quest.Update(markersVisible);

            foreach (var entry in Locator.GetShipLogManager().GetEntryList())
            {
                if (!CharacterManager.HasDiscoveredLocation(entry) && entry.CalculateState() == ShipLogEntry.State.Explored)
                {
                    CharacterManager.DiscoverLocation(entry);
                }
                if (!CharacterManager.HasExploredLocation(entry) && entry.GetExploreFacts().All(f => f.IsRevealed()))
                {
                    CharacterManager.ExploreLocation(entry);
                }
            }

            if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
            {
                WarpToSpawnPoint("Spawn_TH");
            }
            if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
            {
                WarpToSpawnPoint("Spawn_TH_Observatory");
            }

            if (markersVisible)
            {
                NotificationQueue.UpdateAll();
            }
        }

        public void OnStartQuest(Quest quest)
        {
            MajorQueue.Enqueue($"Started quest {quest.Name}", sound: AudioType.ShipLogRevealEntry);
        }

        public void OnCompleteQuest(Quest quest)
        {
            MajorQueue.Enqueue($"Completed quest {quest.Name}", sound: AudioType.TH_ZeroGTrainingAllRepaired);
        }

        public void OnCompleteStep(QuestStep step)
        {
            MinorQueue.Enqueue($"{step.Text} - Complete", time: 3f, sound: AudioType.ShipLogRevealEntry);
        }

        public void OnDiscoverLocation(ShipLogEntry entry)
        {
            MajorQueue.Enqueue($"Discovered location {entry.GetName(false)}", sound: AudioType.ShipLogRevealEntry);
        }

        public void OnExploreLocation(ShipLogEntry entry)
        {
            MajorQueue.Enqueue($"Fully explored location {entry.GetName(false)}", sound: AudioType.TH_ZeroGTrainingAllRepaired);
        }

        public void OnAwardXP(int xp, string reason)
        {
            ExpQueue.Enqueue($"+{xp}XP", pos: PromptPosition.LowerLeft, time: 3f);
        }

        public void OnLevelUp(int level)
        {
            MajorQueue.Enqueue($"Reached Level {level}!", clip: Assets.QuestJingleAudioClip);
            if (PerkManager.GetUnspentPerkPoints() > 0)
                ExpQueue.Enqueue($"You have {PerkManager.GetUnspentPerkPoints()} unspent perk points.", PromptPosition.LowerLeft, time: 10f);
        }

        public void OnUnlockPerk(Perk perk)
        {

        }

        public void OnPickUp(Drop drop)
        {
            MinorQueue.Enqueue($"Picked up {drop.Name}");
        }

        public void OnEquipDrop(Drop drop, EquipSlot slot)
        {
            MinorQueue.Enqueue($"Equipped {drop.Name}");
        }

        public void OnUnequipDrop(Drop drop, EquipSlot slot)
        {
            MinorQueue.Enqueue($"Unequipped {drop.Name}");
        }

        private void WarpToSpawnPoint(string spawnPointName)
        {
            var spawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
            var spawnPoint = spawner._spawnList.FirstOrDefault(s => s.name == spawnPointName);
            if (spawnPoint is EyeSpawnPoint eyeSpawn)
            {
                Locator.GetEyeStateManager().SetState(eyeSpawn.GetEyeState());
            }
            spawner.DebugWarp(spawnPoint);
        }
    }
}