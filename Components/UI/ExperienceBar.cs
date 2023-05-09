using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.UI
{
    public class ExperienceBar : BuiltElement
    {
        const float ALPHA_MAX = 0.8f;

        CanvasGroup canvasGroup;
        Image back;
        Image fill;
        Image lead;
        Text currLevelText;
        Text nextLevelText;

        float prevExp;
        int prevLevel;
        float currExp;
        int currLevel;
        float nextExp;
        int nextLevel;

        float lastUpdateTime;

        public override void Setup()
        {
            prevExp = currExp = nextExp = CharacterManager.GetCurrentXP();
            prevLevel = currLevel = nextLevel = CharacterManager.GetCharacterLevel();

            CharacterManager.OnAwardXP.AddListener(OnRewardXP);
            CharacterManager.OnLevelUp.AddListener(OnLevelUp);
            OuterWildsRPG.Instance.ModHelper.Menus.PauseMenu.OnOpened += PauseMenu_OnOpened;
            OuterWildsRPG.Instance.ModHelper.Menus.PauseMenu.OnClosed += PauseMenu_OnClosed;
        }

        private void PauseMenu_OnClosed()
        {
            TriggerAnimation();
        }
        private void PauseMenu_OnOpened()
        {
            TriggerAnimation();
        }

        public override void Cleanup()
        {
            CharacterManager.OnAwardXP.RemoveListener(OnRewardXP);
            CharacterManager.OnLevelUp.RemoveListener(OnLevelUp);
            OuterWildsRPG.Instance.ModHelper.Menus.PauseMenu.OnOpened -= PauseMenu_OnOpened;
            OuterWildsRPG.Instance.ModHelper.Menus.PauseMenu.OnClosed -= PauseMenu_OnClosed;
        }

        public override void Rebuild()
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.sizeDelta = new Vector2(-20f, 5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 10f);

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;

            back = MakeChild(back, "Back");
            back.color = Assets.HUDBackColor;

            lead = MakeChild(lead, "Lead");
            lead.rectTransform.anchorMin = new Vector2(0f, 0f);
            lead.rectTransform.anchorMax = new Vector2(GetLeadPercentage(), 1f);
            lead.color = Assets.HUDActiveColor;

            fill = MakeChild(fill, "Fill");
            fill.rectTransform.anchorMin = new Vector2(0f, 0f);
            fill.rectTransform.anchorMax = new Vector2(GetFillPercentage(), 1f);
            fill.color = Assets.HUDForeColor;

            currLevelText = MakeChild(currLevelText, "CurrentLevel");
            currLevelText.rectTransform.anchorMin = new Vector2(0f, 1f);
            currLevelText.rectTransform.anchorMax = new Vector2(0f, 1f);
            currLevelText.rectTransform.sizeDelta = new Vector2(128f, 24f);
            currLevelText.rectTransform.pivot = new Vector2(0f, 0f);
            currLevelText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            currLevelText.text = $"Level {currLevel}";
            currLevelText.font = ModUI.Font;
            currLevelText.alignment = TextAnchor.LowerLeft;

            nextLevelText = MakeChild(nextLevelText, "NextLevel");
            nextLevelText.rectTransform.anchorMin = new Vector2(1f, 1f);
            nextLevelText.rectTransform.anchorMax = new Vector2(1f, 1f);
            nextLevelText.rectTransform.sizeDelta = new Vector2(128f, 24f);
            nextLevelText.rectTransform.pivot = new Vector2(1f, 0f);
            nextLevelText.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            nextLevelText.text = $"Level {currLevel + 1}";
            nextLevelText.font = ModUI.Font;
            nextLevelText.alignment = TextAnchor.LowerRight;
        }

        public override bool Animate()
        {
            var isLeadAnimating = currExp != nextExp || currLevel != nextLevel;
            var isFillAnimating = prevExp != currExp || prevLevel != currLevel;

            if (isLeadAnimating)
            {
                var currCap = (float)CharacterManager.GetNeededXP(currLevel + 1);
                var speed = currCap / 5f;

                if (currLevel != nextLevel)
                {
                    currExp = Mathf.MoveTowards(currExp, currCap, Time.unscaledDeltaTime * speed);
                    if (currExp == currCap)
                    {
                        currExp = 0f;
                        prevExp = 0f;
                        currLevel = nextLevel;
                    }
                }
                else
                {
                    currExp = Mathf.MoveTowards(currExp, nextExp, Time.unscaledDeltaTime * speed);
                }
                lastUpdateTime = Time.unscaledTime;
            }
            else if (isFillAnimating)
            {
                var currCap = (float)CharacterManager.GetNeededXP(currLevel + 1);
                var speed = currCap / 20f;

                if (prevLevel != currLevel)
                {
                    prevExp = 0f;
                    prevLevel = currLevel;
                }
                prevExp = Mathf.MoveTowards(prevExp, currExp, Time.unscaledDeltaTime * speed);
                lastUpdateTime = Time.unscaledTime;
            }

            lead.rectTransform.anchorMax = new Vector2(GetLeadPercentage(), 1f);
            fill.rectTransform.anchorMax = new Vector2(GetFillPercentage(), 1f);
            lead.enabled = isLeadAnimating || isFillAnimating;

            currLevelText.text = $"Level {currLevel}";
            nextLevelText.text = $"Level {currLevel + 1}";

            var visible = (Time.unscaledTime < lastUpdateTime + 5f && PlayerStateUtils.IsPlayable) || PlayerStateUtils.InPauseMenu;

            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, visible ? ALPHA_MAX : 0f, Time.unscaledDeltaTime);

            var isAnimating = canvasGroup.alpha > 0f;

            return isAnimating;
        }

        void OnRewardXP(int exp, string reason)
        {
            nextLevel = CharacterManager.GetCharacterLevel();
            nextExp = CharacterManager.GetCurrentXP();
            TriggerAnimation();
        }

        void OnLevelUp(int level)
        {
        }

        public float GetFillPercentage() => currLevel > prevLevel ? 0f : prevExp / CharacterManager.GetNeededXP(prevLevel + 1);
        public float GetLeadPercentage() => currExp / CharacterManager.GetNeededXP(currLevel + 1);
    }
}
