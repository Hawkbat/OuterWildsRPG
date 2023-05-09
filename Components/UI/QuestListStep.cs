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
    public class QuestListStep : BuiltElement
    {
        QuestListQuest questListQuest;
        QuestStep step;

        CanvasGroup canvasGroup;
        Image checkmark;
        Text label;
        Text optionalLabel;
        Image strikethrough;

        static readonly Vector2 ICON_SIZE = new Vector2(16f, 16f);
        static readonly Vector2 LABEL_SIZE = new Vector2(256f, 16f);
        static readonly Vector2 OPTIONAL_LABEL_SIZE = new Vector2(128f, 16f);
        static readonly float SPACING = 2f;
        static readonly float OPTIONAL_SPACING = 2f;

        bool animateStart;
        bool animateCompletion;

        public QuestStep GetStep() => step;

        public Vector2 GetInitialSize()
            => new Vector2(ICON_SIZE.x + SPACING + LABEL_SIZE.x, Mathf.Max(ICON_SIZE.y, LABEL_SIZE.y));

        public Vector2 GetCurrentSize()
            => IsBuilt() ? rectTransform.sizeDelta : GetInitialSize();

        public void Init(QuestListQuest questListQuest, QuestStep step)
        {
            this.questListQuest = questListQuest;
            this.step = step;
        }

        public override void Setup()
        {
            QuestManager.OnStartStep.AddListener(OnStartStep);
            QuestManager.OnStepProgressed.AddListener(OnStepProgressed);
            QuestManager.OnCompleteStep.AddListener(OnCompleteStep);
        }

        public override void Cleanup()
        {
            QuestManager.OnStartStep.RemoveListener(OnStartStep);
            QuestManager.OnStepProgressed.RemoveListener(OnStepProgressed);
            QuestManager.OnCompleteStep.RemoveListener(OnCompleteStep);
        }

        public override void Rebuild()
        {
            if (step == null) throw new Exception($"{nameof(QuestListStep)} built before {nameof(Init)}");

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.sizeDelta = step.IsInProgress ? GetInitialSize() : new Vector2(GetInitialSize().x, 0f);
            rectTransform.pivot = new Vector2(0f, 1f);

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.alpha = step.IsInProgress ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            checkmark = MakeChild(checkmark, "Checkmark");
            checkmark.rectTransform.anchorMin = new Vector2(0f, 1f);
            checkmark.rectTransform.anchorMax = new Vector2(0f, 1f);
            checkmark.rectTransform.sizeDelta = ICON_SIZE;
            checkmark.rectTransform.pivot = new Vector2(0f, 1f);
            checkmark.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            checkmark.enabled = !step.IsHint;

            label = MakeChild(label, "Label");
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(0f, 1f);
            label.rectTransform.sizeDelta = LABEL_SIZE;
            label.rectTransform.pivot = new Vector2(0f, 1f);
            label.font = ModUI.Font;
            label.fontSize = 10;
            label.text = step.ToDisplayString();
            label.fontStyle = step.IsHint ? FontStyle.Italic : FontStyle.Normal;

            optionalLabel = MakeChild(optionalLabel, "OptionalLabel");
            optionalLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            optionalLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            optionalLabel.rectTransform.sizeDelta = OPTIONAL_LABEL_SIZE;
            optionalLabel.rectTransform.pivot = new Vector2(0f, 1f);
            optionalLabel.font = ModUI.Font;
            optionalLabel.fontSize = 8;
            optionalLabel.text = "(Optional)";
            optionalLabel.color = Assets.HUDBackColor;
            optionalLabel.enabled = step.Optional && !step.IsHint;

            strikethrough = MakeChild(strikethrough, "Strikethrough");
            strikethrough.rectTransform.anchorMin = new Vector2(0f, 1f);
            strikethrough.rectTransform.anchorMax = new Vector2(0f, 1f);
            strikethrough.rectTransform.sizeDelta = new Vector2(0f, 1f);
            strikethrough.rectTransform.pivot = new Vector2(0f, 0.5f);
            strikethrough.color = Assets.HUDActiveColor;
            strikethrough.enabled = false;

            UpdateDisplayState();

            gameObject.SetActive(step.IsInProgress);
        }

        public override bool Animate()
        {
            if (animateCompletion)
            {
                var textRatio = label.preferredWidth / LABEL_SIZE.x;
                var t0 = Mathf.Clamp01(Mathf.InverseLerp(step.CompletedTime, step.CompletedTime + 3f * textRatio, Time.unscaledTime));
                strikethrough.enabled = true;
                strikethrough.rectTransform.sizeDelta = new Vector2(t0 * label.preferredWidth, strikethrough.rectTransform.sizeDelta.y);

                var t1 = Mathf.Clamp01(Mathf.InverseLerp(step.CompletedTime + 4f, step.CompletedTime + 5f, Time.unscaledTime));
                t1 = Mathf.SmoothStep(0f, 1f, t1);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * (1f - t1));

                canvasGroup.alpha = 1f - t1;

                questListQuest.TriggerAnimation();

                if (t1 >= 1f)
                {
                    animateCompletion = false;
                }
            }
            else if (animateStart)
            {
                var t = Mathf.Clamp01(Mathf.InverseLerp(step.StartedTime, step.StartedTime + 1f, Time.unscaledTime));
                t = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.sizeDelta = new Vector2(GetInitialSize().x, GetInitialSize().y * t);

                canvasGroup.alpha = t;

                questListQuest.TriggerAnimation();

                if (t >= 1f)
                {
                    animateStart = false;
                }
            }

            return true;
        }

        void UpdateDisplayState()
        {
            if (checkmark != null)
            {
                checkmark.sprite = step.IsComplete ? Assets.CheckboxOnSprite : Assets.CheckboxOffSprite;
            }
            if (label != null)
            {
                label.color = step.IsComplete ? Assets.HUDActiveColor : Assets.HUDForeColor;
                label.rectTransform.anchoredPosition = new Vector2(step.IsHint ? 0f : ICON_SIZE.x + SPACING, -2f);
                if (step.CompleteMode == Enums.QuestConditionMode.All)
                {
                    label.text = Translations.QuestStepProgress(step, step.GetCompletionConditionProgress(), step.CompleteOn.Count);
                } else
                {
                    label.text = step.ToDisplayString();
                }
            }
            if (optionalLabel != null)
            {
                optionalLabel.rectTransform.anchoredPosition = new Vector2(ICON_SIZE.x + SPACING + label.preferredWidth + OPTIONAL_SPACING, -4f);
            }
            if (strikethrough != null)
            {
                strikethrough.rectTransform.anchoredPosition = new Vector2(step.IsHint ? 0f : ICON_SIZE.x + SPACING, -8f);
            }
        }

        void OnStartStep(QuestStep step)
        {
            if (step != this.step) return;
            animateStart = true;
            gameObject.SetActive(true);
            UpdateDisplayState();
            TriggerAnimation();
        }

        void OnStepProgressed(QuestStep step)
        {
            if (step != this.step) return;
            UpdateDisplayState();
        }

        void OnCompleteStep(QuestStep step)
        {
            if (step != this.step) return;
            animateCompletion = true;
            animateStart = false;
            UpdateDisplayState();
            TriggerAnimation();
        }
    }
}
