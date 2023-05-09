using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OuterWildsRPG.Components.Graph
{
    public class GraphCard : GraphElement
    {

        string id;

        [SerializeField] Text nameText;
        [SerializeField] Image photo;
        [SerializeField] Text questionMark;
        [SerializeField] Image unreadIcon;
        [SerializeField] Image hudMarkerIcon;
        [SerializeField] Image moreToExploreIcon;
        [SerializeField] Image background;
        [SerializeField] Image nameBackground;
        [SerializeField] Image border;

        Vector2 origIconSize;
        Vector2 origCardSize;

        public static GraphCard ConvertFromEntryCard(ShipLogEntryCard entryCard)
        {
            var graphCard = entryCard.gameObject.AddComponent<GraphCard>();
            graphCard.rectTransform = entryCard.GetComponent<RectTransform>();
            graphCard.nameText = entryCard._name;
            graphCard.photo = entryCard._photo;
            graphCard.questionMark = entryCard._questionMark;
            graphCard.unreadIcon = entryCard._unreadIcon;
            graphCard.hudMarkerIcon = entryCard._hudMarkerIcon;
            graphCard.moreToExploreIcon = entryCard._moreToExploreIcon;
            graphCard.background = entryCard._background;
            graphCard.nameBackground = entryCard._nameBackground;
            graphCard.border = entryCard._border;
            Destroy(entryCard);
            return graphCard;
        }

        public override string GetID() => id;

        public Vector2 GetAnchoredPosition() => rectTransform.anchoredPosition;

        public void Init(string id, GraphMode graphMode, IGraphProvider graphProvider, FontAndLanguageController fontAndLangCtrl)
        {
            Init(graphMode, graphProvider);

            this.id = id;

            nameText.font = Locator.GetUIStyleManager().GetShipLogCardFont();
            questionMark.color = Locator.GetUIStyleManager().GetShipLogRumorColor();
            origIconSize = moreToExploreIcon.rectTransform.sizeDelta;
            origCardSize = rectTransform.sizeDelta;
            fontAndLangCtrl.AddTextElement(nameText, false, true, false);
            nameText.SetAllDirty();
            questionMark.SetAllDirty();

            PostInit();
        }

        public override void Refresh()
        {
            var prevNameText = nameText.text;
            var prevPhotoSprite = photo.sprite;

            rectTransform.anchoredPosition = graphProvider.GetCardPosition(id);

            nameText.text = graphProvider.GetCardName(id);
            photo.sprite = graphProvider.GetCardPhoto(id);
            unreadIcon.gameObject.SetActive(graphProvider.GetCardUnread(id));
            hudMarkerIcon.gameObject.SetActive(graphProvider.GetCardMarked(id));
            moreToExploreIcon.gameObject.SetActive(graphProvider.GetCardMoreToExplore(id));

            rectTransform.sizeDelta = origCardSize;
            var excessNameHeight = nameText.preferredHeight - nameText.rectTransform.rect.height;
            if (excessNameHeight > 0f)
                rectTransform.sizeDelta += Vector2.up * excessNameHeight;

            border.color = hasFocus ? graphProvider.GetCardHighlightColor(id) : graphProvider.GetCardColor(id);
            nameBackground.color = border.color;
            var cardSize = graphProvider.GetCardSize(id);
            rectTransform.localScale = Vector3.one * cardSize;
            var iconSize = cardSize < 1f ? origIconSize / cardSize : origIconSize;
            hudMarkerIcon.rectTransform.sizeDelta = iconSize;
            unreadIcon.rectTransform.sizeDelta = iconSize;
            moreToExploreIcon.rectTransform.sizeDelta = iconSize;

            if (nameText.text != prevNameText)
                nameText.SetAllDirty();
            if (photo.sprite != prevPhotoSprite)
                photo.SetAllDirty();

            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);

            photo.gameObject.SetActive(!isRumored && photo.sprite);
            background.gameObject.SetActive(isRumored || !photo.sprite);
            questionMark.gameObject.SetActive(isRumored);

            gameObject.SetActive(!IsHidden() && isRevealed);
        }

        public Vector2 GetEdgeIntersection(Vector2 outsidePos)
        {
            var normalized = (outsidePos - rectTransform.anchoredPosition).normalized;
            var cardSize = graphProvider.GetCardSize(id);
            var sizeX = rectTransform.sizeDelta.x * 0.5f * cardSize;
            var sizeY = rectTransform.sizeDelta.y * 0.5f * cardSize;
            if (Mathf.Abs(normalized.y / normalized.x) < Mathf.Abs(sizeY / sizeX))
            {
                var x = Mathf.Sign(normalized.x) * sizeX;
                var y = normalized.y / normalized.x * x;
                return rectTransform.anchoredPosition + new Vector2(x, y);
            }
            else
            {
                var y = Mathf.Sign(normalized.y) * sizeY;
                var x = normalized.x / normalized.y * y;
                return rectTransform.anchoredPosition + new Vector2(x, y);
            }
        }

        public override bool CheckPointCollision(Vector3 point)
            => border.rectTransform.rect.Contains(border.rectTransform.InverseTransformPoint(point));

        public override bool CanSelect() => graphProvider.CanSelectCard(id);
        public override bool AttemptSelect() => graphProvider.AttemptSelectCard(id);
        public override bool AttemptDeselect() => graphProvider.AttemptDeselectCard(id);
        public override IEnumerable<string> GetDescription() => graphProvider.GetCardDescription(id);

        public override bool ShouldPlayRevealAnimation()
        {
            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var wasRevealed = graphProvider.GetCardWasRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);
            var wasRumored = graphProvider.GetCardWasRumor(id);
            return isRevealed != wasRevealed || isRumored != wasRumored;
        }

        protected override void StartRevealAnimation()
        {
            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var wasRevealed = graphProvider.GetCardWasRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);
            var wasRumored = graphProvider.GetCardWasRumor(id);

            gameObject.SetActive(true);
            background.gameObject.SetActive(true);
            if (isRevealed && !wasRevealed)
            {
                rectTransform.localScale = new Vector3(1f, 0f, 1f);
            }
            if (!isRevealed && wasRevealed)
            {
                rectTransform.localScale = Vector3.one * graphProvider.GetCardSize(id);
            }
            if (!isRumored && wasRumored)
            {
                photo.rectTransform.localScale = new Vector3(1f, 0f, 1f);
                questionMark.gameObject.SetActive(true);
                questionMark.SetAllDirty();
            }
            if (isRumored && !wasRumored)
            {
                photo.rectTransform.localScale = Vector3.one;
            }
            photo.gameObject.SetActive(photo.sprite && isRevealed && (!isRumored || isRumored != wasRumored));
        }

        protected override void UpdateRevealAnimation(float t)
        {
            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var wasRevealed = graphProvider.GetCardWasRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);
            var wasRumored = graphProvider.GetCardWasRumor(id);

            if (isRevealed != wasRevealed)
            {
                var v = isRevealed ? t : 1f - t;
                var cardSize = graphProvider.GetCardSize(id);
                rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, v) * cardSize;
            }
            if (isRumored != wasRumored)
            {
                var v = isRumored ? 1f - t : t;
                photo.rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, v);
            }
        }

        protected override void FinishRevealAnimation()
        {
            graphProvider.OnCardRevealStateUpdated(id);
        }
    }
}
