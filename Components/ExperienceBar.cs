using OuterWildsRPG.Objects.Common;
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
    public class ExperienceBar : BuiltElement
    {
        const float ALPHA_MAX = 0.8f;

        CanvasGroup canvasGroup;
        Image back;
        Image fill;
        Image lead;

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
        }

        public override void Cleanup()
        {
            CharacterManager.OnAwardXP.RemoveListener(OnRewardXP);
            CharacterManager.OnLevelUp.RemoveListener(OnLevelUp);
        }

        public override void Rebuild()
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.sizeDelta = new Vector2(-20f, 5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 15f);

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = ALPHA_MAX;
            canvasGroup.interactable = false;

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
                    currExp = Mathf.MoveTowards(currExp, currCap, Time.deltaTime * speed);
                    if (currExp == currCap)
                    {
                        currExp = 0f;
                        prevExp = 0f;
                        currLevel = nextLevel;
                    }
                }
                else
                {
                    currExp = Mathf.MoveTowards(currExp, nextExp, Time.deltaTime * speed);
                }
                lastUpdateTime = Time.time;
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
                prevExp = Mathf.MoveTowards(prevExp, currExp, Time.deltaTime * speed);
                lastUpdateTime = Time.time;
            }

            lead.rectTransform.anchorMax = new Vector2(GetLeadPercentage(), 1f);
            fill.rectTransform.anchorMax = new Vector2(GetFillPercentage(), 1f);
            lead.enabled = isLeadAnimating || isFillAnimating;

            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, Time.time < lastUpdateTime + 5f ? ALPHA_MAX : 0f, Time.deltaTime);

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
