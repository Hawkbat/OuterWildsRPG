using HarmonyLib;
using OuterWildsRPG.Components;
using OuterWildsRPG.External;
using OuterWildsRPG.Objects;
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

namespace OuterWildsRPG
{
    public class OuterWildsRPG : ModBehaviour
    {
        public static OuterWildsRPG Instance;

        public static List<Quest> Quests = new();

        public static bool Ready;

        public static PromptPosition QuestPromptPosition;
        public static ScreenPromptList QuestPromptList;
        public static QuestLogMode QuestLogMode;

        public static Texture2D CheckboxOnTex;
        public static Texture2D CheckboxOffTex;
        public static Sprite CheckboxOnSprite;
        public static Sprite CheckboxOffSprite;
        public static Color OnColor = new Color(0.9686f, 0.498f, 0.2078f);

        static bool inDialogue;

        private void Awake()
        {
            Instance = this;

            QuestPromptPosition = EnumUtils.Create<PromptPosition>("QuestList");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            CheckboxOnTex = ModHelper.Assets.GetTexture("assets/CheckboxOn.png");
            CheckboxOffTex = ModHelper.Assets.GetTexture("assets/CheckboxOff.png");
            CheckboxOnSprite = Sprite.Create(CheckboxOnTex, new Rect(0f, 0f, CheckboxOnTex.width, CheckboxOnTex.height), Vector2.one * 0.5f, 256f);
            CheckboxOffSprite = Sprite.Create(CheckboxOffTex, new Rect(0f, 0f, CheckboxOffTex.width, CheckboxOffTex.height), Vector2.one * 0.5f, 256f);

            // Load Quests
            foreach (var mod in ModHelper.Interaction.GetMods())
            {
                var questsFound = false;
                var modQuestData = mod.ModHelper.Storage.Load<QuestListData>("quests.json", false);
                if (modQuestData != null)
                {
                    var modQuests = modQuestData.quests.Select(q => q.Parse()).ToList();
                    foreach (var quest in modQuests)
                        Quests.Add(quest);
                    if (modQuests.Count > 0) questsFound = true;
                }
                var modQuestsPath = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "./quests/"));
                if (Directory.Exists(modQuestsPath))
                {
                    foreach (var filePath in Directory.GetFiles(modQuestsPath))
                    {
                        var fileQuestData = JsonHelper.LoadJsonObject<QuestData>(filePath, false);
                        if (fileQuestData != null)
                        {
                            var fileQuest = fileQuestData.Parse();
                            Quests.Add(fileQuest);
                            questsFound = true;
                        }
                    }
                }
                if (questsFound)
                    ModHelper.Console.WriteLine($"Loaded quests from {mod.ModHelper.Manifest.UniqueName}.", MessageType.Success);
            }

            // Wire up quest events
            QuestSaveData.OnStartQuest.AddListener(OnStartQuest);
            QuestSaveData.OnCompleteStep.AddListener(OnCompleteStep);
            QuestSaveData.OnCompleteQuest.AddListener(OnCompleteQuest);
            QuestSaveData.OnDiscoverLocation.AddListener(OnDiscoverLocation);
            QuestSaveData.OnAwardXP.AddListener(OnAwardXP);

            GlobalMessenger.AddListener("EnterConversation", () => inDialogue = true);
            GlobalMessenger.AddListener("ExitConversation", () => inDialogue = false);

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
            // Set up prompt list
            if (!QuestPromptList)
            {
                var sourceList = Locator.GetPromptManager().GetScreenPromptList(PromptPosition.UpperLeft);
                var clone = Instantiate(sourceList.gameObject);
                clone.transform.parent = sourceList.transform.parent;
                clone.transform.localPosition = new Vector3(-400f, 120f, 0f);
                clone.transform.localScale = Vector3.one;
                clone.name = "ScreenPromptListQuestList";
                QuestPromptList = clone.GetComponent<ScreenPromptList>();
                QuestPromptList.SetMinElementHeightAndWidth(Locator.GetPromptManager()._promptElementMinHeight, Locator.GetPromptManager()._promptElementMinWidth);
                foreach (var prompt in QuestPromptList._listPrompts) QuestPromptList.RemoveScreenPrompt(prompt);
            }

            // Set up prompts/markers
            foreach (var quest in Quests) quest.SetUp();

            // Register quest log ship log mode with the Custom Ship Log Modes API
            var questLogGO = new GameObject("QuestLogMode");
            questLogGO.transform.parent = UnityUtils.GetTransformAtPath("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            questLogGO.transform.localPosition = Vector3.zero;
            questLogGO.transform.localRotation = Quaternion.identity;
            questLogGO.transform.localScale = Vector3.one;
            QuestLogMode = questLogGO.AddComponent<QuestLogMode>();

            var customModesAPI = ModHelper.Interaction.TryGetModApi<ICustomShiplogModesAPI>("dgarro.CustomShipLogModes");
            customModesAPI.AddMode(QuestLogMode, () => Quests.Any(q => q.IsStarted), () => "Quest Log");

            // DEBUG; Export ship log details to reference in quest steps
            //ShipLogExporter.Export();

            Ready = true;
        }

        private void CleanUp()
        {
            Ready = false;

            // Clear out any set up prompts/markers
            foreach (var quest in Quests) quest.CleanUp();

            QuestPromptList = null;
        }

        private void Update()
        {
            if (!Ready) return;

            bool promptsVisible = !ModHelper.Menus.PauseMenu.IsOpen && !inDialogue;

            foreach (var quest in Quests)
                quest.Update(promptsVisible);

            foreach (var entry in Locator.GetShipLogManager().GetEntryList())
            {
                if (!QuestSaveData.HasDiscoveredLocation(entry) && entry.CalculateState() == ShipLogEntry.State.Explored)
                {
                    QuestSaveData.DiscoverLocation(entry);
                }
            }
        }

        public void OnStartQuest(Quest quest)
        {
            PromptNotify($"Started quest {quest.Name}");
        }

        public void OnCompleteQuest(Quest quest)
        {
            PromptNotify($"Completed quest {quest.Name}", sound: AudioType.TH_ZeroGTrainingAllRepaired);
        }

        public void OnCompleteStep(QuestStep step)
        {
            PromptNotify($"{step.Text} - Complete", time: 3f);
        }

        public void OnDiscoverLocation(ShipLogEntry entry)
        {
            PromptNotify($"Discovered location {entry.GetName(false)}");
        }

        public void OnAwardXP(int xp, string reason)
        {
            PromptNotify($"+{xp}XP", pos: PromptPosition.LowerLeft, time: 3f);
        }

        private void PromptNotify(string msg, PromptPosition pos = PromptPosition.Center, float time = 5f, AudioType sound = AudioType.ShipLogRevealEntry)
        {
            var prompt = new ScreenPrompt(msg);
            Locator.GetPromptManager().AddScreenPrompt(prompt, pos, true);
            UnityUtils.RunAfterSeconds(time, () => Locator.GetPromptManager().RemoveScreenPrompt(prompt, pos));
            Locator.GetPlayerAudioController().PlayOneShotInternal(sound);
        }
    }
}