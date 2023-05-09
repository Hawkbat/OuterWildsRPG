using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.UI
{
    public class QuestListQuest : BuiltElement
    {
        const float ROW_GAP = 2f;
        static readonly Vector2 MAIN_HEADING_SIZE = new(192f, 16f);
        static readonly Vector2 SIDE_HEADING_SIZE = new(160f, 14f);
        static readonly Vector2 MISC_HEADING_SIZE = new(128f, 10f);

        QuestList questList;
        Quest quest;

        List<QuestListStep> questListSteps = new();

        CanvasGroup canvasGroup;
        Text heading;
        Image underline;

        bool animateStart;
        bool animateCompletion;

        public Quest GetQuest() => quest;

        public IEnumerable<QuestListStep> GetActiveSteps() => questListSteps.Where(s => s.gameObject.activeSelf);

        public Vector2 GetInitialSize()
        {
            var headingSize = GetHeadingSize();
            if (GetActiveSteps().Any())
            {
                var x = Mathf.Max(headingSize.x, GetActiveSteps().Max(s => s.GetInitialSize().x));
                var y = headingSize.y + GetActiveSteps().Sum(s => s.GetInitialSize().y + ROW_GAP);
                return new Vector2(x, y);
            }
            return headingSize;
        }

        public Vector2 GetCurrentSize()
        {
            if (!IsBuilt()) return GetInitialSize();
            if (GetActiveSteps().Any())
            {
                var x = Mathf.Max(heading.rectTransform.sizeDelta.x, GetActiveSteps().Max(s => s.GetCurrentSize().x));
                var y = heading.rectTransform.sizeDelta.y + GetActiveSteps().Sum(s => s.GetCurrentSize().y + ROW_GAP);
                return new Vector2(x, y);
            }
            return heading.rectTransform.sizeDelta;
        }

        public Vector2 GetHeadingSize() => quest?.Type switch
        {
            QuestType.Main => MAIN_HEADING_SIZE,
            QuestType.Side => SIDE_HEADING_SIZE,
            QuestType.Misc => MISC_HEADING_SIZE,
            _ => SIDE_HEADING_SIZE,
        };

        public void Init(QuestList questList, Quest quest)
        {
            this.questList = questList;
            this.quest = quest;
        }

        public override void Setup()
        {
            QuestManager.OnTrackQuest.AddListener(OnTrackQuest);
            QuestManager.OnUntrackQuest.AddListener(OnUntrackQuest);
        }

        public override void Cleanup()
        {
            QuestManager.OnTrackQuest.RemoveListener(OnTrackQuest);
            QuestManager.OnUntrackQuest.RemoveListener(OnUntrackQuest);
        }

        public override void Rebuild()
        {
            if (quest == null) throw new Exception($"{nameof(QuestListQuest)} built before {nameof(Init)}");

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            heading = MakeChild(heading, "Heading");
            heading.rectTransform.anchorMin = new Vector2(0f, 1f);
            heading.rectTransform.anchorMax = new Vector2(0f, 1f);
            heading.rectTransform.sizeDelta = GetHeadingSize();
            heading.rectTransform.pivot = new Vector2(0f, 1f);
            heading.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            heading.font = ModUI.Font;
            heading.fontSize = quest.Type switch
            {
                QuestType.Main => 12,
                QuestType.Side => 10,
                QuestType.Misc => 7,
                _ => 10,
            };
            heading.text = quest.ToDisplayString().ToUpper();
            heading.color = Assets.HUDActiveColor;

            underline = MakeChild(underline, "Underline");
            underline.rectTransform.anchorMin = new Vector2(0f, 1f);
            underline.rectTransform.anchorMax = new Vector2(0f, 1f);
            underline.rectTransform.sizeDelta = new Vector2(GetHeadingSize().x, 1f);
            underline.rectTransform.pivot = new Vector2(0f, 0.5f);
            underline.rectTransform.anchoredPosition = new Vector2(0f, -GetHeadingSize().y);
            underline.color = Assets.HUDActiveColor;
            underline.enabled = quest.Type == QuestType.Main;

            var pos = new Vector2(0f, 0f);
            pos.y -= heading.rectTransform.sizeDelta.y;

            foreach (var step in quest.Steps)
            {
                if (step.IsInProgress) pos.y -= ROW_GAP;

                var existing = questListSteps.Find(s => s.GetStep() == step);
                if (existing) questListSteps.Remove(existing);

                var questListStep = MakeChild(existing, step.ID);
                questListStep.rectTransform.anchoredPosition = pos;

                questListStep.Init(this, step);
                questListSteps.Add(questListStep);

                if (step.IsInProgress) pos.y -= questListStep.GetInitialSize().y;
            }

            gameObject.SetActive(quest.IsInProgress && quest.IsTracked);

            questList.TriggerAnimation();
        }

        public override bool Animate()
        {
            var pos = new Vector2(0f, 0f);
            pos.y -= heading.rectTransform.sizeDelta.y;
            foreach (var step in GetActiveSteps())
            {
                pos.y -= ROW_GAP;
                var stepPos = step.rectTransform.anchoredPosition;
                if (stepPos != pos)
                {
                    step.rectTransform.anchoredPosition = Vector2.MoveTowards(step.rectTransform.anchoredPosition, pos, Time.unscaledDeltaTime * 128f);
                    questList.TriggerAnimation();
                }
                pos.y -= step.GetCurrentSize().y;
            }

            if (animateCompletion)
            {
                var t = Mathf.Clamp01(Mathf.InverseLerp(quest.CompletedTime, quest.CompletedTime + 1f, Time.unscaledTime));
                t = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * (1f - t));

                canvasGroup.alpha = 1f - t;

                questList.TriggerAnimation();

                if (t >= 1f)
                {
                    animateCompletion = false;
                    gameObject.SetActive(false);
                }
            }
            else if (animateStart)
            {
                var t = Mathf.Clamp01(Mathf.InverseLerp(quest.StartedTime, quest.StartedTime + 1f, Time.unscaledTime));
                t = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * t);

                canvasGroup.alpha = t;

                questList.TriggerAnimation();

                if (t >= 1f)
                {
                    animateStart = false;
                }
            }

            return true;
        }

        void OnTrackQuest(Quest quest)
        {
            if (quest != this.quest) return;
            animateStart = true;
            questList.MoveToEnd(this);
            rectTransform.SetAsLastSibling();
            gameObject.SetActive(true);
            TriggerAnimation();
            questList.TriggerAnimation();
        }

        void OnUntrackQuest(Quest quest)
        {
            if (quest != this.quest) return;
            animateCompletion = true;
            animateStart = false;
            TriggerAnimation();
            questList.TriggerAnimation();
        }
    }
}
