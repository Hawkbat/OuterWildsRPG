using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.UI
{
    public class QuestList : BuiltElement
    {
        const float ROW_GAP = 8f;

        CanvasGroup canvasGroup;
        List<QuestListQuest> questListQuests = new();

        public IEnumerable<QuestListQuest> GetActiveQuests() =>
            questListQuests.Where(q => q.gameObject.activeSelf);

        public override void Setup()
        {

        }

        public override void Cleanup()
        {

        }

        public override void Rebuild()
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(10f, -150f);

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;

            var pos = new Vector2(0f, 0f);
            foreach (var quest in QuestManager.GetAllQuests())
            {
                var existing = questListQuests.Find(q => q.GetQuest() == quest);
                if (existing) questListQuests.Remove(existing);

                var questListQuest = MakeChild(existing, quest.FullID);
                questListQuest.rectTransform.anchoredPosition = pos;

                questListQuest.Init(this, quest);
                questListQuests.Add(questListQuest);

                if (quest.IsInProgress && quest.IsTracked) pos.y -= questListQuest.GetInitialSize().y + ROW_GAP;
            }
        }

        public override bool Animate()
        {
            var pos = new Vector2(0f, 0f);
            foreach (var quest in GetActiveQuests())
            {
                var questPos = quest.rectTransform.anchoredPosition;
                if (questPos != pos)
                {
                    quest.rectTransform.anchoredPosition = Vector2.MoveTowards(quest.rectTransform.anchoredPosition, pos, Time.unscaledTime * 128f);
                }
                pos.y -= quest.GetCurrentSize().y + ROW_GAP;
            }

            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, PlayerStateUtils.IsPlayable ? 1f : 0f, Time.unscaledDeltaTime);

            return true;
        }

        public void MoveToEnd(QuestListQuest quest)
        {
            questListQuests.Remove(quest);
            questListQuests.Add(quest);
        }
    }
}
