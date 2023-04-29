using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components
{
    public class GraphLink : MonoBehaviour, IShipLogSelectable
    {
        GraphMode.IGraphProvider graphProvider;

        GraphCard sourceCard;
        GraphCard targetCard;

        [SerializeField] RectTransform rectTransform;
        [SerializeField] Image line;
        [SerializeField] Image arrow;
        [SerializeField] Image arrowBackground;
        [SerializeField] RectTransform boundsRect;

        bool isRevealAnimReady;
        bool updateRevealAnim;
        float startRevealTime;
        float revealDuration;
        float arrowPosY;
        bool hasFocus;
        bool hidden;
        Color baseColor;

        public static GraphLink ConvertFromEntryLink(ShipLogEntryLink entryLink)
        {
            var graphLink = entryLink.gameObject.AddComponent<GraphLink>();
            graphLink.rectTransform = entryLink._rectTransform;
            graphLink.line = entryLink._line;
            graphLink.arrow = entryLink._arrow;
            graphLink.arrowBackground = entryLink._arrowBackground;
            graphLink.boundsRect = entryLink._boundsRect;
            Destroy(entryLink);
            return graphLink;
        }

        public void Init(GraphCard sourceCard, GraphCard targetCard, GraphMode.IGraphProvider graphProvider)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            rectTransform.SetAsFirstSibling();

            this.sourceCard = sourceCard;
            this.targetCard = targetCard;
            this.graphProvider = graphProvider;
            isRevealAnimReady = false;
            updateRevealAnim = false;
            hasFocus = false;
            hidden = false;
            baseColor = Locator.GetUIStyleManager().GetShipLogNeutralColor(false);
            line.color = baseColor;
            arrow.color = baseColor;
            arrowBackground.color = baseColor;
            enabled = false;
        }

        public string GetID() => $"{sourceCard.GetID()}@{targetCard.GetID()}";
        public string GetReversedID() => $"{targetCard.GetID()}@{sourceCard.GetID()}";
        public string GetSourceID() => sourceCard.GetID();
        public string GetTargetID() => targetCard.GetID();
        public GraphCard GetSourceCard() => sourceCard;
        public GraphCard GetTargetCard() => targetCard;

        public bool IsVisible() => !hidden && gameObject.activeSelf;

        public bool IsRevealAnimationReady() => isRevealAnimReady;

        public void Hide()
        {
            hidden = true;
            gameObject.SetActive(false);
        }

        public bool CheckPointCollision(Vector3 point)
            => boundsRect.rect.Contains(boundsRect.InverseTransformPoint(point));

        public void OnGainFocus()
        {
            arrow.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(true);
            line.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(true);
            arrowBackground.color = Locator.GetUIStyleManager().GetShipLogNeutralColor(true);
            hasFocus = true;
            enabled = true;
        }

        public void OnSelect()
        {

        }

        public void OnLoseFocus()
        {
            arrow.color = baseColor;
            line.color = baseColor;
            arrowBackground.color = baseColor;
            hasFocus = false;
        }

        public void MarkAsRead()
        {
            if (graphProvider.AttemptMarkLinkAsRead(GetSourceID(), GetTargetID()))
            {
                targetCard.UpdateUnreadIconVisibility();
            }
        }

        public void PrepareRevealAnimation()
        {
            var isRevealed = graphProvider.GetLinkIsRevealed(GetSourceID(), GetTargetID());
            var wasRevealed = graphProvider.GetLinkWasRevealed(GetSourceID(), GetTargetID());
            if (isRevealed != wasRevealed)
            {
                isRevealAnimReady = true;
                arrow.gameObject.SetActive(false);
                arrow.rectTransform.localScale = Vector3.zero;
                arrowBackground.gameObject.SetActive(false);
                arrowBackground.rectTransform.localPosition = Vector3.zero;
                line.rectTransform.localScale = new Vector3(1f, 0f, 1f);
            }
        }

        public void PlayRevealAnimation(float duration)
        {
            if (isRevealAnimReady)
            {
                enabled = true;
                revealDuration = duration;
                updateRevealAnim = true;
                startRevealTime = Time.unscaledTime;
                isRevealAnimReady = false;
                return;
            }
        }

        public void UpdatePosition()
        {
            var sourcePos = sourceCard.GetAnchoredPosition();
            var targetPos = targetCard.GetAnchoredPosition();
            var diff = targetPos - sourcePos;
            var dist = diff.magnitude;
            rectTransform.anchoredPosition = sourcePos;
            var rot = Vector2.Angle(Vector2.up, diff) * -Mathf.Sign(diff.x);
            rectTransform.localEulerAngles = new Vector3(0f, 0f, rot);
            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, dist);
            boundsRect.sizeDelta = new Vector2(boundsRect.sizeDelta.x, dist);
            var start = sourceCard.GetEdgeIntersection(targetPos);
            var end = targetCard.GetEdgeIntersection(sourcePos);
            arrowPosY = (start - sourcePos).magnitude + (end - start).magnitude * 0.5f;
            arrow.rectTransform.anchoredPosition = Vector2.up * arrowPosY;
            arrowBackground.rectTransform.anchoredPosition = Vector2.up * arrowPosY;
        }

        public void UpdateVisibility()
        {
            var active = !hidden && graphProvider.GetLinkIsRevealed(GetSourceID(), GetTargetID()) && graphProvider.GetCardIsRevealed(GetSourceID());
            gameObject.SetActive(active);
            line.color = baseColor;
            arrow.color = baseColor;
            arrowBackground.color = baseColor;
        }

        private void Update()
        {
            var updateScalingArrow = hasFocus;
            var targetSize = hasFocus ? 2f : 1f;
            if (!Mathf.Approximately(arrow.rectTransform.localScale.x, targetSize))
            {
                updateScalingArrow = true;
                var d = Mathf.MoveTowards(arrow.rectTransform.localScale.x, targetSize, Time.unscaledDeltaTime * 6f);
                arrow.rectTransform.localScale = Vector3.one * d;
                arrowBackground.rectTransform.localScale = Vector3.one * d;
            }
            if (updateRevealAnim)
            {
                var t = Mathf.InverseLerp(startRevealTime, startRevealTime + revealDuration, Time.unscaledTime);
                t = Mathf.SmoothStep(0f, 1f, t);
                line.rectTransform.localScale = new Vector3(1f, t, 1f);
                arrow.gameObject.SetActive(true);
                arrow.rectTransform.anchoredPosition = Vector2.up * arrowPosY * t;
                arrow.rectTransform.localScale = Vector3.one * Mathf.Clamp01(t * 2f);
                arrowBackground.gameObject.SetActive(true);
                arrowBackground.rectTransform.anchoredPosition = arrow.rectTransform.anchoredPosition;
                arrowBackground.rectTransform.localScale = arrow.rectTransform.localScale;
                if (t >= 1f)
                {
                    updateRevealAnim = false;
                    graphProvider.OnLinkRevealStateUpdated(GetSourceID(), GetTargetID());
                }
            }
            if (!updateRevealAnim && !updateScalingArrow)
            {
                enabled = false;
            }
        }
    }
}
