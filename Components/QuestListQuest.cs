using OuterWildsRPG.Objects.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components
{
    public class QuestListQuest : BuiltElement
    {
        const float ROW_GAP = 2f;
        static readonly Vector2 HEADING_SIZE = new(256f, 16f);

        QuestList questList;
        Quest quest;

        List<QuestListStep> questListSteps = new();

        CanvasGroup canvasGroup;
        Text heading;

        bool animateStart;
        bool animateCompletion;

        public Quest GetQuest() => quest;

        public IEnumerable<QuestListStep> GetActiveSteps() => questListSteps.Where(s => s.gameObject.activeSelf);

        public Vector2 GetInitialSize()
        {
            if (GetActiveSteps().Any()) {
                var x = Mathf.Max(HEADING_SIZE.x, GetActiveSteps().Max(s => s.GetInitialSize().x));
                var y = HEADING_SIZE.y + GetActiveSteps().Sum(s => s.GetInitialSize().y + ROW_GAP);
                return new Vector2(x, y);
            }
            return HEADING_SIZE;
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

        public void Init(QuestList questList, Quest quest)
        {
            this.questList = questList;
            this.quest = quest;
        }

        public override void Setup()
        {
            QuestManager.OnStartQuest.AddListener(OnStartQuest);
            QuestManager.OnCompleteQuest.AddListener(OnCompleteQuest);
            QuestManager.OnTrackQuest.AddListener(OnTrackQuest);
            QuestManager.OnUntrackQuest.AddListener(OnUntrackQuest);
        }

        public override void Cleanup()
        {
            QuestManager.OnStartQuest.RemoveListener(OnStartQuest);
            QuestManager.OnCompleteQuest.RemoveListener(OnCompleteQuest);
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
            heading.rectTransform.sizeDelta = HEADING_SIZE;
            heading.rectTransform.pivot = new Vector2(0f, 1f);
            heading.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            heading.font = QuestHUD.Font;
            heading.fontSize = 12;
            heading.text = quest.Name.ToUpper();
            heading.color = Assets.HUDActiveColor;

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
            var anyAnimated = false;

            var pos = new Vector2(0f, 0f);
            pos.y -= heading.rectTransform.sizeDelta.y;
            foreach (var step in GetActiveSteps())
            {
                pos.y -= ROW_GAP;
                var stepPos = step.rectTransform.anchoredPosition;
                if (stepPos != pos)
                {
                    step.rectTransform.anchoredPosition = Vector2.MoveTowards(step.rectTransform.anchoredPosition, pos, Time.deltaTime * 2048f);
                    anyAnimated = true;
                }
                pos.y -= step.GetCurrentSize().y;
            }

            if (animateCompletion)
            {
                var t = Mathf.Clamp01(Mathf.InverseLerp(quest.CompletedTime, quest.CompletedTime + 1f, Time.time));
                t = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * (1f - t));

                canvasGroup.alpha = 1f - t;

                if (t >= 1f)
                {
                    animateCompletion = false;
                    gameObject.SetActive(false);
                }
            }
            else if (animateStart)
            {
                var t = Mathf.Clamp01(Mathf.InverseLerp(quest.StartedTime, quest.StartedTime + 1f, Time.time));
                t = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * t);

                canvasGroup.alpha = t;

                if (t >= 1f)
                {
                    animateStart = false;
                }
            }

            if (anyAnimated || animateCompletion || animateStart)
                questList.TriggerAnimation();

            return anyAnimated || animateCompletion || animateStart;
        }

        void OnStartQuest(Quest quest)
        {
            if (quest != this.quest) return;

        }

        void OnCompleteQuest(Quest quest)
        {
            if (quest != this.quest) return;
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
