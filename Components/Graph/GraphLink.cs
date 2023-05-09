using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.Graph
{
    public class GraphLink : GraphElement
    {
        GraphCard sourceCard;
        GraphCard targetCard;

        [SerializeField] Image line;
        [SerializeField] Image arrow;
        [SerializeField] Image arrowBackground;
        [SerializeField] RectTransform boundsRect;

        float arrowPosY;
        Color baseColor;
        Color focusColor;

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

        public override string GetID() => $"{sourceCard.GetID()}@{targetCard.GetID()}";
        public string GetReversedID() => $"{targetCard.GetID()}@{sourceCard.GetID()}";
        public string GetSourceID() => sourceCard.GetID();
        public string GetTargetID() => targetCard.GetID();
        public GraphCard GetSourceCard() => sourceCard;
        public GraphCard GetTargetCard() => targetCard;


        public void Init(GraphMode graphMode, GraphCard sourceCard, GraphCard targetCard, IGraphProvider graphProvider)
        {
            Init(graphMode, graphProvider);

            this.sourceCard = sourceCard;
            this.targetCard = targetCard;

            rectTransform.SetAsFirstSibling();
            baseColor = Locator.GetUIStyleManager().GetShipLogNeutralColor(false);
            focusColor = Locator.GetUIStyleManager().GetShipLogNeutralColor(true);

            PostInit();
        }

        public override bool CheckPointCollision(Vector3 point)
            => boundsRect.rect.Contains(boundsRect.InverseTransformPoint(point));

        public override bool CanSelect() => graphProvider.CanSelectLink(GetSourceID(), GetTargetID());
        public override bool AttemptSelect() => graphProvider.AttemptSelectLink(GetSourceID(), GetTargetID());
        public override bool AttemptDeselect() => graphProvider.AttemptDeselectLink(GetSourceID(), GetTargetID());
        public override IEnumerable<string> GetDescription() => graphProvider.GetLinkDescription(GetSourceID(), GetTargetID());

        public override void Refresh()
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

            arrow.color = hasFocus ? focusColor : baseColor;
            line.color = hasFocus ? focusColor : baseColor;
            arrowBackground.color = hasFocus ? focusColor : baseColor;

            var active = IsHidden() && graphProvider.GetLinkIsRevealed(GetSourceID(), GetTargetID()) && graphProvider.GetCardIsRevealed(GetSourceID());
            gameObject.SetActive(active);
        }

        public override bool ShouldPlayRevealAnimation()
        {
            var isRevealed = graphProvider.GetLinkIsRevealed(GetSourceID(), GetTargetID());
            var wasRevealed = graphProvider.GetLinkWasRevealed(GetSourceID(), GetTargetID());
            return isRevealed != wasRevealed;
        }

        protected override void StartRevealAnimation()
        {
            arrow.gameObject.SetActive(false);
            arrow.rectTransform.localScale = Vector3.zero;
            arrowBackground.gameObject.SetActive(false);
            arrowBackground.rectTransform.localPosition = Vector3.zero;
            line.rectTransform.localScale = new Vector3(1f, 0f, 1f);
        }

        protected override void UpdateRevealAnimation(float t)
        {
            t = Mathf.SmoothStep(0f, 1f, t);
            line.rectTransform.localScale = new Vector3(1f, t, 1f);
            arrow.gameObject.SetActive(true);
            arrow.rectTransform.anchoredPosition = Vector2.up * arrowPosY * t;
            arrow.rectTransform.localScale = Vector3.one * Mathf.Clamp01(t * 2f);
            arrowBackground.gameObject.SetActive(true);
            arrowBackground.rectTransform.anchoredPosition = arrow.rectTransform.anchoredPosition;
            arrowBackground.rectTransform.localScale = arrow.rectTransform.localScale;
        }

        protected override void FinishRevealAnimation()
        {
            graphProvider.OnLinkRevealStateUpdated(GetSourceID(), GetTargetID());
        }

        protected override bool Animate()
        {
            var targetSize = hasFocus ? 2f : 1f;
            var updateScalingArrow = hasFocus || !Mathf.Approximately(arrow.rectTransform.localScale.x, targetSize);
            if (updateScalingArrow)
            {
                var d = Mathf.MoveTowards(arrow.rectTransform.localScale.x, targetSize, Time.unscaledDeltaTime * 6f);
                arrow.rectTransform.localScale = Vector3.one * d;
                arrowBackground.rectTransform.localScale = Vector3.one * d;
            }
            return base.Animate() || updateScalingArrow;
        }
    }
}
