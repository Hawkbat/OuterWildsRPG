using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.UI
{
    public class ModUI : BuiltElement
    {
        public static Font Font;

        static ModUI instance;

        Canvas canvas;
        CanvasScaler canvasScaler;

        ExperienceBar expBar;
        QuestList questList;
        ToolSlotStrip toolStrip;
        readonly Dictionary<string, QuestGiverIcon> questGiverIcons = new();
        readonly Dictionary<string, DialogueIcon> dialogueIcons = new();
        readonly Dictionary<string, TranslateIcon> translateIcons = new();

        public override void Setup()
        {
            instance = this;

            OnChangeGUIMode();
            OnLanguageChanged();

            GlobalMessenger.AddListener("ChangeGUIMode", OnChangeGUIMode);
            TextTranslation.Get().OnLanguageChanged += OnLanguageChanged;
        }

        public override void Cleanup()
        {
            GlobalMessenger.RemoveListener("ChangeGUIMode", OnChangeGUIMode);
            TextTranslation.Get().OnLanguageChanged -= OnLanguageChanged;
        }

        public override void Rebuild()
        {
            canvas = MakeComponent(canvas);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.referencePixelsPerUnit = 16f;
            canvas.enabled = !GUIMode.IsHiddenMode();

            canvasScaler = MakeComponent(canvasScaler);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 1f;
            canvasScaler.referencePixelsPerUnit = 16f;
            canvasScaler.referenceResolution = new Vector2(800f, 450f);

            expBar = MakeChild(expBar, "ExperienceBar");

            questList = MakeChild(questList, "QuestList");

            toolStrip = MakeChild(toolStrip, "ToolStrip");

            foreach (var quest in QuestManager.GetAllQuests())
            {
                if (quest.IsStarted || !quest.GetQuestGiver()) continue;

                var existing = questGiverIcons.ContainsKey(quest.FullID) ? questGiverIcons[quest.FullID] : null;
                if (existing) questGiverIcons.Remove(quest.FullID);

                var questGiverIcon = MakeChild(existing, quest.FullID);
                questGiverIcon.Init(quest);

                questGiverIcons.Add(quest.FullID, questGiverIcon);
            }


            foreach (var dialogue in FindObjectsOfType<CharacterDialogueTree>())
            {
                var path = UnityUtils.GetTransformPath(dialogue.transform);
                var existing = dialogueIcons.ContainsKey(path) ? dialogueIcons[path] : null;
                if (existing) dialogueIcons.Remove(path);

                var dialogueIcon = MakeChild(existing, dialogue.transform.parent.name);
                dialogueIcon.Init(dialogue, questGiverIcons.Values);

                dialogueIcons.Add(path, dialogueIcon);
            }

            foreach (var nomaiText in FindObjectsOfType<NomaiText>())
            {
                var path = UnityUtils.GetTransformPath(nomaiText.transform);
                var existing = translateIcons.ContainsKey(path) ? translateIcons[path] : null;
                if (existing) translateIcons.Remove(path);

                var translateIcon = MakeChild(existing, nomaiText.transform.parent.name);
                translateIcon.Init(nomaiText);

                translateIcons.Add(path, translateIcon);
            }
        }

        public override bool Animate()
        {
            return false;
        }

        public static Vector3 WorldToCanvasPosition(Vector3 worldPosition, Vector2 anchor)
        {
            var camera = Locator.GetActiveCamera();
            var viewportPosition = (Vector2)camera.WorldToViewportPoint(worldPosition);
            var scale = instance.rectTransform.sizeDelta;
            return (viewportPosition - anchor) * scale;
        }

        private void OnChangeGUIMode()
        {
            if (canvas != null)
                canvas.enabled = !GUIMode.IsHiddenMode();
        }

        private void OnLanguageChanged()
        {
            if (TextTranslation.Get().IsLanguageLatin())
            {
                Font = Locator.GetPromptManager()._font;
            }
            else
            {
                Font = TextTranslation.GetFont();
            }
            TriggerRebuild();
        }
    }
}
