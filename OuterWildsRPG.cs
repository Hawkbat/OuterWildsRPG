using HarmonyLib;
using OuterWildsRPG.Components.Graph;
using OuterWildsRPG.Components.ShipLogModes;
using OuterWildsRPG.Components.UI;
using OuterWildsRPG.Enums;
using OuterWildsRPG.External;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Objects.Shops;
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
        public static ICustomShiplogModesAPI CustomShiplogModesAPI;
        public static ModSaveUtility ModSaveUtility;

        public static bool DebugMode;
        public static bool Ready;

        public static ModUI ModUI;

        public static NotificationQueue MajorQueue = new() { Time = 5f };
        public static NotificationQueue MinorQueue = new() { ConcurrentLimit = 4 };
        public static NotificationQueue ExpQueue = new() { ConcurrentLimit = 3, Position = PromptPosition.LowerLeft };
        public static NotificationQueue DropQueue = new() { ConcurrentLimit = 3, Position = PromptPosition.BottomCenter };

        public static string ModID => Instance.ModHelper.Manifest.UniqueName;

        private void Awake()
        {
            Instance = this;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        void LoadEntities<TEntity, TSingle, TMultiple>(Func<TEntity, bool> register, string type) where TEntity : Entity<TEntity, TSingle>, new() where TSingle : EntityData, new() where TMultiple : MultipleEntityData<TSingle>, new()
        {
            var total = 0;
            foreach (var mod in ModHelper.Interaction.GetMods())
            {
                var modID = mod.ModHelper.Manifest.UniqueName;

                List<TEntity> modEntities = new();

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
                            register(entity);
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
                        if (!filePath.ToLower().EndsWith(".json")) continue;
                        var fileName = Path.GetFileName(filePath);
                        var singleData = JsonHelper.LoadJsonObject<TSingle>(filePath, false);
                        if (singleData != null)
                        {
                            var entity = new TEntity();
                            entity.Load(singleData, modID);
                            register(entity);
                            modEntities.Add(entity);
                        } else
                        {
                            ModHelper.Console.WriteLine($"Failed to load {type} {fileName} for {modID}.", MessageType.Error);
                        }
                    }
                }
                total += modEntities.Count;
                if (modEntities.Any())
                    ModHelper.Console.WriteLine($"Loaded {modEntities.Count} {type} from {modID}.", MessageType.Success);
            }

            ModHelper.Console.WriteLine($"Loaded {total} total {type}.", MessageType.Success);
        }

        void ResolveEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            foreach (var entity in entities) entity.Resolve();
        }

        public override void Configure(IModConfig config)
        {
            DebugMode = ModHelper.Config.GetSettingsValue<bool>("Debug Mode");
        }

        private void Start()
        {
            Configure(ModHelper.Config);

            Assets.Init();

            CustomShiplogModesAPI = ModHelper.Interaction.TryGetModApi<ICustomShiplogModesAPI>("dgarro.CustomShipLogModes");
            ModSaveUtility = new ModSaveUtility(this);

            SaveDataManager.Init();

            TranslationUtils.RegisterAllTranslations();

            LoadEntities<Quest, QuestData, QuestListData>(QuestManager.RegisterQuest, "quests");
            LoadEntities<Perk, PerkData, PerkListData>(PerkManager.RegisterPerk, "perks");
            LoadEntities<Drop, DropData, DropListData>(DropManager.RegisterDrop, "drops");
            LoadEntities<Shop, ShopData, ShopListData>(ShopManager.RegisterShop, "shops");
            ResolveEntities(QuestManager.GetAllQuests());
            ResolveEntities(PerkManager.GetAllPerks());
            ResolveEntities(DropManager.GetAllDrops());
            ResolveEntities(ShopManager.GetAllShops());

            QuestManager.OnStartQuest.AddListener(OnStartQuest);
            QuestManager.OnStepProgressed.AddListener(OnStepProgressed);
            QuestManager.OnCompleteStep.AddListener(OnCompleteStep);
            QuestManager.OnCompleteQuest.AddListener(OnCompleteQuest);
            CharacterManager.OnAwardXP.AddListener(OnAwardXP);
            CharacterManager.OnLevelUp.AddListener(OnLevelUp);
            DropManager.OnReceiveDrop.AddListener(OnPickUpDrop);
            DropManager.OnRemoveDrop.AddListener(OnRemoveDrop);
            DropManager.OnEquipDrop.AddListener(OnEquipDrop);
            DropManager.OnUnequipDrop.AddListener(OnUnequipDrop);

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

            LogSuccess($"{nameof(OuterWildsRPG)} is initialized.", true);
        }

        private void SetUp()
        {
            SaveDataManager.SetUp();
            BuffManager.SetUp();
            CharacterManager.SetUp();
            DropManager.SetUp();
            ShopManager.SetUp();
            QuestManager.SetUp();
            WorldIconManager.SetUp();
            PlayerStateUtils.SetUp();

            if (!ModUI)
            {
                var questHudRect = UnityUtils.MakeRectChild(null, "QuestHUD");
                ModUI = questHudRect.gameObject.AddComponent<ModUI>();
            }

            var questLogGO = new GameObject("QuestLogMode");
            questLogGO.transform.parent = UnityUtils.GetTransformAtPathUnsafe("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            questLogGO.transform.localPosition = Vector3.zero;
            questLogGO.transform.localRotation = Quaternion.identity;
            questLogGO.transform.localScale = Vector3.one;
            var questLogMode = questLogGO.AddComponent<QuestLogMode>();

            CustomShiplogModesAPI.AddMode(questLogMode, () => true, () => "Quest Log");
            questLogMode.ShiplogModesAPI = CustomShiplogModesAPI;

            var perkTreeMode = GraphMode.Create<PerkTreeMode>();
            CustomShiplogModesAPI.AddMode(perkTreeMode, () => true, () => "Perk Tree");

            var inventoryMode = GraphMode.Create<InventoryMode>();
            CustomShiplogModesAPI.AddMode(inventoryMode, () => true, () => "Inventory");

            // DEBUG; Export ship log details to reference in quest steps
            //ShipLogExporter.Export();

            Ready = true;
        }

        private void CleanUp()
        {
            Ready = false;

            BuffManager.CleanUp();
            CharacterManager.CleanUp();
            DropManager.CleanUp();
            ShopManager.CleanUp();
            QuestManager.CleanUp();
            WorldIconManager.CleanUp();
            PlayerStateUtils.CleanUp();
        }

        private void Update()
        {
            if (!Ready) return;

            if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
            {
                WarpToSpawnPoint("Spawn_TH");
            }
            if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
            {
                WarpToSpawnPoint("Spawn_TH_Observatory");
            }

            BuffManager.Update();
            CharacterManager.Update();
            DropManager.Update();
            QuestManager.Update();

            if (PlayerStateUtils.IsPlayable)
            {
                NotificationQueue.UpdateAll();
            }
        }

        public void OnStartQuest(Quest quest)
        {
            var themeJingle = quest.Theme switch
            {
                QuestTheme.Hearthian => Assets.HearthianJingleAudioClip,
                QuestTheme.Nomai => Assets.NomaiJingleAudioClip,
                QuestTheme.Stranger => Assets.StrangerJingleAudioClip,
                _ => Assets.HearthianJingleAudioClip,
            };
            if (quest.Type == QuestType.Misc)
                themeJingle = null;
            MajorQueue.Enqueue(Translations.NotificationQuestStart(quest), clip: themeJingle);
        }

        public void OnStepProgressed(QuestStep step)
        {
            if (step.IsComplete) return;
            string text;
            if (step.CompleteMode == QuestConditionMode.All)
                text = Translations.NotificationQuestStepProgress(step, step.GetCompletionConditionProgress(), step.CompleteOn.Count);
            else
                text = step.ToDisplayString();
            MinorQueue.Enqueue(text);
        }

        public void OnCompleteQuest(Quest quest)
        {
            MajorQueue.Enqueue(Translations.NotificationQuestComplete(quest), sound: AudioType.TH_ZeroGTrainingAllRepaired);
        }

        public void OnCompleteStep(QuestStep step)
        {
            MinorQueue.Enqueue(Translations.NotificationQuestStepComplete(step), sound: AudioType.ShipLogRevealEntry);
        }

        public void OnAwardXP(int xp, string reason)
        {
            ExpQueue.Enqueue(Translations.NotificationXPGain(xp, reason));
        }

        public void OnLevelUp(int level)
        {
            MajorQueue.Enqueue(Translations.NotificationLevelUp(level), clip: Assets.LevelUpJingleAudioClip);
            if (PerkManager.GetUnspentPerkPoints() > 0)
                ExpQueue.Enqueue(Translations.NotificationUnspentPerkPoints(PerkManager.GetUnspentPerkPoints()), time: 10f);
        }

        public void OnPickUpDrop(Drop drop)
        {
            DropQueue.Enqueue(Translations.NotificationPickUpDrop(drop));
        }

        public void OnRemoveDrop(Drop drop)
        {
            DropQueue.Enqueue(Translations.NotificationRemoveDrop(drop));
        }

        public void OnEquipDrop(Drop drop, EquipSlot slot)
        {
            DropQueue.Enqueue(Translations.NotificationEquipDrop(drop));
        }

        public void OnUnequipDrop(Drop drop, EquipSlot slot)
        {
            DropQueue.Enqueue(Translations.NotificationUnequipDrop(drop));
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

        public static void Log(string message, bool debug = false)
        {
            if (debug && !DebugMode) return;
            Instance.ModHelper.Console.WriteLine(message, MessageType.Info);
        }

        public static void LogWarning(string message, bool debug = false)
        {
            if (debug && !DebugMode) return;
            Instance.ModHelper.Console.WriteLine(message, MessageType.Warning);
        }

        public static void LogError(string message, bool debug = false)
        {
            if (debug && !DebugMode) return;
            Instance.ModHelper.Console.WriteLine(message, MessageType.Error);
        }

        public static void LogSuccess(string message, bool debug = false)
        {
            if (debug && !DebugMode) return;
            Instance.ModHelper.Console.WriteLine(message, MessageType.Success);
        }

        public static void LogException(Exception ex, string message, bool debug = false)
        {
            if (debug && !DebugMode) return;
            LogWarning(message);
            LogError(ex.ToString());
        }
    }
}