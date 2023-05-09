using OuterWildsRPG.Components.UI;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.UI
{
    public class DialogueIcon : BuiltElement
    {
        const float MIN_DISTANCE = 10f;
        const float MAX_DISTANCE = 50f;
        const float MIN_SIZE = 0.1f;
        const float MAX_SIZE = 0.5f;

        CharacterDialogueTree dialogue;
        Transform target;

        Text text;

        public CharacterDialogueTree GetDialogue() => dialogue;
        public Transform GetTarget() => target;

        public void Init(CharacterDialogueTree dialogue, IEnumerable<QuestGiverIcon> questGiverIcons)
        {
            this.dialogue = dialogue;

            target = dialogue.transform;
            
            if (target == null || questGiverIcons.Any(g => g.GetTarget() == target))
            {
                gameObject.SetActive(false);
            }
        }
        public override void Setup()
        {
            QuestManager.OnStartQuest.AddListener(OnStartQuest);
        }

        public override void Cleanup()
        {
            QuestManager.OnStartQuest.RemoveListener(OnStartQuest);
        }

        public override void Rebuild()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;

            text = MakeChild(text, "Text");
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.zero;
            text.rectTransform.pivot = new Vector2(0.5f, 0f);
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.font = TextTranslation.GetFont();
            text.text = "...";
            text.fontSize = 64;
            text.color = Assets.HUDForeColor;
        }

        public override bool Animate()
        {
            if (target == null)
            {
                OuterWildsRPG.LogError($"Lost target for dialogue icon {name}!");
                gameObject.SetActive(false);
                return false;
            }
            var distance = Vector3.Distance(Locator.GetPlayerTransform().position, target.position);
            var t = Mathf.Clamp01(Mathf.InverseLerp(MIN_DISTANCE, MAX_DISTANCE, distance));

            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(MAX_SIZE, MIN_SIZE, t);
            rectTransform.anchoredPosition = ModUI.WorldToCanvasPosition(target.transform.position + target.transform.up, Vector2.zero);

            var cam = Locator.GetActiveCamera().transform;
            var isOnScreen = Vector3.Dot(cam.transform.forward, (target.position - cam.position).normalized) > 0f;
            var isInRange = distance < MAX_DISTANCE;
            text.enabled = isOnScreen && isInRange && PlayerStateUtils.IsPlayable;

            return true;
        }

        void OnStartQuest(Quest quest)
        {
            if (target == quest.GetQuestGiver())
            {
                gameObject.SetActive(true);
            }
        }
    }
}
