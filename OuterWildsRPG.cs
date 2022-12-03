using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OuterWildsRPG
{
    public class OuterWildsRPG : ModBehaviour
    {
        public static OuterWildsRPG Instance;

        public List<Quest> Quests = new();

        public PromptPosition QuestPromptPosition;
        public ScreenPromptList QuestPromptList;
        public QuestLogMode QuestLogMode;

        Dictionary<string, ScreenPrompt> prompts = new();
        Dictionary<string, CanvasMarker> canvasMarkers = new();

        Texture2D checkboxOnTex;
        Texture2D checkboxOffTex;
        Sprite checkboxOnSprite;
        Sprite checkboxOffSprite;
        Color onColor = new Color(0.9686f, 0.498f, 0.2078f);

        private void Awake()
        {
            Instance = this;

            QuestPromptPosition = EnumUtils.Create<PromptPosition>("QuestList");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            checkboxOnTex = ModHelper.Assets.GetTexture("assets/CheckboxOn.png");
            checkboxOffTex = ModHelper.Assets.GetTexture("assets/CheckboxOff.png");
            checkboxOnSprite = Sprite.Create(checkboxOnTex, new Rect(0f, 0f, checkboxOnTex.width, checkboxOnTex.height), Vector2.one * 0.5f, 256f);
            checkboxOffSprite = Sprite.Create(checkboxOffTex, new Rect(0f, 0f, checkboxOffTex.width, checkboxOffTex.height), Vector2.one * 0.5f, 256f);

            // Load Quests
            foreach (var mod in ModHelper.Interaction.GetMods())
            {
                var questsFound = false;
                var modQuestData = mod.ModHelper.Storage.Load<QuestFileData>("quests.json");
                if (modQuestData != null)
                {
                    var modQuests = modQuestData.Parse();
                    foreach (var quest in modQuests.Quests)
                        Quests.Add(quest);
                    if (modQuests.Quests.Count > 0) questsFound = true;
                }
                var modQuestsPath = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "./quests/"));
                if (Directory.Exists(modQuestsPath))
                {
                    foreach (var filePath in Directory.GetFiles(modQuestsPath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var fileQuestData = mod.ModHelper.Storage.Load<QuestData>("quests/" + fileName);
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
            QuestSaveData.OnAwardXP.AddListener(OnAwardXP);

            // Get the New Horizons API and load configs
            var newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

                prompts.Clear();
                canvasMarkers.Clear();

                ModHelper.Events.Unity.RunWhen(() => Locator.GetShipLogManager(), () =>
                {

                    // Register quest log ship log mode with the Custom Ship Log Modes API
                    var questLogGO = new GameObject("QuestLogMode");
                    questLogGO.transform.parent = Utils.GetTransformAtPath("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
                    questLogGO.transform.localPosition = Vector3.zero;
                    questLogGO.transform.localRotation = Quaternion.identity;
                    questLogGO.transform.localScale = Vector3.one;
                    QuestLogMode = questLogGO.AddComponent<QuestLogMode>();

                    var customModesAPI = ModHelper.Interaction.TryGetModApi<ICustomShiplogModesAPI>("dgarro.CustomShipLogModes");
                    customModesAPI.AddMode(QuestLogMode, () => Quests.Any(q => q.IsStarted), () => "Quest Log");

                    // DEBUG; Export ship log details to reference in quest steps
                    ModHelper.Events.Unity.FireInNUpdates(() => ShipLogExporter.Export(), 10);
                });
            };

            ModHelper.Console.WriteLine($"{nameof(OuterWildsRPG)} is initialized.", MessageType.Success);
        }

        private void Update()
        {
            if (!Locator.GetShipLogManager()) return;
            
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

            bool promptsVisible = !ModHelper.Menus.PauseMenu.IsOpen;

            foreach (var quest in Quests)
            {
                if (!prompts.ContainsKey(quest.ID))
                {
                    var prompt = new ScreenPrompt($"[{quest.Name.ToUpper()}]");
                    prompts.Add(quest.ID, prompt);
                    Locator.GetPromptManager().AddScreenPrompt(prompt, QuestPromptPosition);
                }
                prompts[quest.ID].SetVisibility(promptsVisible && quest.IsInProgress);
                prompts[quest.ID].SetTextColor(onColor);

                if (quest.IsComplete && !QuestSaveData.HasCompletedQuest(quest)) {
                    QuestSaveData.CompleteQuest(quest);
                }

                foreach (var step in quest.Steps)
                {
                    var key = $"{quest.ID}_{step.ID}";
                    if (!prompts.ContainsKey(key))
                    {
                        var prompt = new ScreenPrompt($"{(step.Optional ? "(Optional) " : "")}{step.Text}", customSprite: step.IsComplete ? checkboxOnSprite : checkboxOffSprite);
                        prompts.Add(key, prompt);
                        Locator.GetPromptManager().AddScreenPrompt(prompt, QuestPromptPosition);

                        try
                        {
                            Transform location = null;

                            if (!string.IsNullOrEmpty(step.LocationEntry))
                            {
                                location = Locator.GetEntryLocation(step.LocationEntry).GetTransform();
                            } else if (!string.IsNullOrEmpty(step.LocationPath))
                            {
                                location = Utils.GetTransformAtPath(step.LocationPath);
                            }

                            if (location != null)
                            {
                                var marker = Locator.GetMarkerManager().InstantiateNewMarker();
                                Locator.GetMarkerManager().RegisterMarker(marker, location, step.Text, 0f);
                                canvasMarkers.Add(key, marker);
                            }
                        } catch (Exception ex)
                        {
                            ModHelper.Console.WriteLine($"Failed to create quest marker for {key}");
                            ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                        }
                    }
                    prompts[key].SetVisibility(promptsVisible && quest.IsInProgress && step.IsStarted);
                    if (step.IsComplete) prompts[key].SetTextColor(onColor);
                    else prompts[key].SetTextColor(prompts[key].GetDefaultColor());
                    if (canvasMarkers.ContainsKey(key))
                        canvasMarkers[key].SetVisibility(step.IsInProgress);

                    if (step.IsComplete && !QuestSaveData.HasCompletedStep(step))
                    {
                        QuestSaveData.CompleteStep(step);
                        prompts[key]._customSprite = checkboxOnSprite;
                        Locator.GetPromptManager().TriggerRebuild(prompts[key]);

                        var prompt = new ScreenPrompt($"{step.Text} - Complete");
                        Locator.GetPromptManager().AddScreenPrompt(prompt, PromptPosition.Center, true);
                        Utils.RunAfterSeconds(3f, () => Locator.GetPromptManager().RemoveScreenPrompt(prompt, PromptPosition.Center));
                    }
                }

                var spacerKey = $"{quest.ID}_#";
                if (!prompts.ContainsKey(spacerKey))
                {
                    var prompt = new ScreenPrompt(" ");
                    prompts.Add(spacerKey, prompt);
                    Locator.GetPromptManager().AddScreenPrompt(prompt, QuestPromptPosition);
                }
                prompts[spacerKey].SetVisibility(promptsVisible && quest.IsStarted);
            }

            foreach (var entry in Locator.GetShipLogManager().GetEntryList())
            {
                if (!QuestSaveData.HasDiscoveredLocation(entry) && entry.CalculateState() == ShipLogEntry.State.Explored)
                {
                    QuestSaveData.DiscoverLocation(entry);
                }
            }
        }

        public void OnAwardXP(int xp, string reason)
        {
            var prompt = new ScreenPrompt($"{reason} +{xp}XP");
            Locator.GetPromptManager().AddScreenPrompt(prompt, PromptPosition.Center, true);
            Utils.RunAfterSeconds(5f, () => Locator.GetPromptManager().RemoveScreenPrompt(prompt, PromptPosition.Center));
        }
    }
}