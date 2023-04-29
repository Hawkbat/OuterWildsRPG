using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components
{
    public class QuestHUD : BuiltElement
    {
        public static Font Font;

        static QuestHUD instance;

        Canvas canvas;
        CanvasScaler canvasScaler;

        ExperienceBar expBar;
        QuestList questList;
        Dictionary<string, QuestGiverIcon> questGiverIcons = new();

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

            foreach (var quest in QuestManager.GetAllQuests())
            {
                if (quest.IsStarted || !quest.Steps.Any(s => !string.IsNullOrEmpty(s.LocationPath))) continue;

                var existing = questGiverIcons.ContainsKey(quest.FullID) ? questGiverIcons[quest.FullID] : null;
                if (existing) questGiverIcons.Remove(quest.FullID);

                var questGiverIcon = MakeChild(existing, quest.FullID);
                questGiverIcon.Init(quest);

                questGiverIcons.Add(quest.FullID, questGiverIcon);
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
