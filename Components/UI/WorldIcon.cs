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
    public class WorldIcon : BuiltElement
    {
        const float MIN_SIZE = 0.1f;
        const float MAX_SIZE = 0.5f;
        
        WorldIconTarget target;

        CanvasGroup canvasGroup;
        Image image;
        Text text;

        RaycastHit[] raycastHitBuffer = new RaycastHit[32];
        bool hasLineOfSight;
        float nextCheckTime;

        public WorldIconTarget GetTarget() => target;

        public void Init(WorldIconTarget target)
        {
            this.target = target;
        }

        public override void Setup()
        {

        }

        public override void Cleanup()
        {

        }

        public override void Rebuild()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;

            canvasGroup = MakeComponent(canvasGroup);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0f;

            image = MakeChild(image, "Image");
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.zero;
            image.rectTransform.pivot = new Vector2(0.5f, 0f);
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.sprite = target != null ? target.GetIcon() : null;

            text = MakeChild(text, "Text");
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.zero;
            text.rectTransform.pivot = new Vector2(0.5f, 1f);
            text.rectTransform.anchoredPosition = Vector2.zero;
            text.font = ModUI.Font;
            text.fontSize = 24;
            text.text = target != null ? target.GetName() : null;
        }

        public override bool Animate()
        {
            if (target == null || target.GetTarget() == null || !target.isActiveAndEnabled)
            {
                OuterWildsRPG.LogError($"Lost target for icon {name}!");
                gameObject.SetActive(false);
                return false;
            }

            var targetPos = target.GetTarget().TransformPoint(target.GetOffset());

            var distance = Vector3.Distance(Locator.GetPlayerTransform().position, targetPos);
            var t = Mathf.Clamp01(Mathf.InverseLerp(target.GetMinDistance(), target.GetMaxDistance(), distance));

            image.sprite = target.GetIcon();
            image.rectTransform.sizeDelta = new Vector2(image.preferredWidth, image.preferredHeight);
            text.text = target.GetName();
            text.color = Assets.HUDForeColor;
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);

            rectTransform.localScale = Vector3.one * Mathf.Lerp(MAX_SIZE, MIN_SIZE, t);

            rectTransform.anchoredPosition = ModUI.WorldToCanvasPosition(targetPos, Vector2.zero);

            var cam = Locator.GetPlayerCamera().transform;
            var diff = targetPos - cam.position;
            var dir = diff.normalized;
            var dist = diff.magnitude;
            var isOnScreen = Vector3.Dot(cam.transform.forward, dir) > 0f;
            var isInRange = distance < target.GetMaxDistance();

            image.enabled = image.sprite != null && isOnScreen && isInRange && PlayerStateUtils.IsPlayable;
            text.enabled = !string.IsNullOrEmpty(text.text) && isOnScreen && isInRange && PlayerStateUtils.IsPlayable;

            if ((image.enabled || text.enabled) && Time.time > nextCheckTime)
            {
                hasLineOfSight = true;
                var hitCount = Physics.RaycastNonAlloc(targetPos, -dir, raycastHitBuffer, dist, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
                for (var i = 0; i < hitCount; i++)
                {
                    if (!raycastHitBuffer[i].collider.CompareTag("Player"))
                    {
                        hasLineOfSight = false;
                        break;
                    }
                }
                nextCheckTime = Time.time + UnityEngine.Random.Range(0.2f, 0.4f);
            }

            canvasGroup.alpha = hasLineOfSight ? 1f : 0.4f;

            return true;
        }
    }
}
