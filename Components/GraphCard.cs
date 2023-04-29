using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OuterWildsRPG.Components
{
    public class GraphCard : MonoBehaviour, IShipLogSelectable
    {
        GraphMode.IGraphProvider graphProvider;

        string id;

        [SerializeField] RectTransform rectTransform;
        [SerializeField] Text nameText;
        [SerializeField] Image photo;
        [SerializeField] Text questionMark;
        [SerializeField] Image unreadIcon;
        [SerializeField] Image hudMarkerIcon;
        [SerializeField] Image moreToExploreIcon;
        [SerializeField] Image background;
        [SerializeField] Image nameBackground;
        [SerializeField] Image border;

        bool isRevealAnimReady;
        Vector2 origIconSize;
        float startAnimTime;
        bool updateRevealAnim;

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

        void Awake()
        {
            enabled = false;
        }

        public Vector2 GetAnchoredPosition() => rectTransform.anchoredPosition;

        public bool IsVisible() => gameObject.activeSelf;

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
            } else
            {
                var y = Mathf.Sign(normalized.y) * sizeY;
                var x = normalized.x / normalized.y * y;
                return rectTransform.anchoredPosition + new Vector2(x, y);
            }
        }

        public void Init(string id, GraphMode.IGraphProvider graphProvider, FontAndLanguageController fontAndLangCtrl)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            this.id = id;
            this.graphProvider = graphProvider;
            rectTransform.anchoredPosition = graphProvider.GetCardPosition(id);
            isRevealAnimReady = false;
            nameText.text = graphProvider.GetCardName(id);
            nameText.font = Locator.GetUIStyleManager().GetShipLogCardFont();
            questionMark.color = Locator.GetUIStyleManager().GetShipLogRumorColor();
            unreadIcon.gameObject.SetActive(false);
            hudMarkerIcon.gameObject.SetActive(false);
            moreToExploreIcon.gameObject.SetActive(false);
            origIconSize = moreToExploreIcon.rectTransform.sizeDelta;
            fontAndLangCtrl.AddTextElement(nameText, false, true, false);
            nameText.SetAllDirty();
            questionMark.SetAllDirty();
            UpdateStateVisuals();
        }

        public void OnEnterComputer()
        {
            nameText.text = graphProvider.GetCardName(id);
            photo.sprite = graphProvider.GetCardPhoto(id);
            var excessNameHeight = nameText.preferredHeight - nameText.rectTransform.rect.height;
            if (excessNameHeight > 0f)
                rectTransform.sizeDelta += Vector2.up * excessNameHeight;
            border.color = graphProvider.GetCardColor(id);
            var cardSize = graphProvider.GetCardSize(id);
            rectTransform.localScale = Vector3.one * cardSize;
            if (cardSize < 1f)
            {
                var iconSize = origIconSize / cardSize;
                hudMarkerIcon.rectTransform.sizeDelta = iconSize;
                unreadIcon.rectTransform.sizeDelta = iconSize;
                moreToExploreIcon.rectTransform.sizeDelta = iconSize;
            }
            nameText.SetAllDirty();
            photo.SetAllDirty();
        }
        public void OnEnterDetectiveMode()
        {
            UpdateUnreadIconVisibility();
            border.color = graphProvider.GetCardColor(id);
            nameBackground.color = border.color;
            moreToExploreIcon.gameObject.SetActive(graphProvider.GetCardMoreToExplore(id));
            UpdateStateVisuals();
        }

        public void SetMarked(bool marked)
        {
            hudMarkerIcon.gameObject.SetActive(marked);
        }

        public string GetID() => id;

        public bool IsRevealAnimationReady() => isRevealAnimReady;

        public bool CheckPointCollision(Vector3 point)
            => border.rectTransform.rect.Contains(border.rectTransform.InverseTransformPoint(point));
        
        public void OnGainFocus()
        {
            border.color = graphProvider.GetCardHighlightColor(id);
            nameBackground.color = border.color;
        }

        public void OnLoseFocus()
        {
            border.color = graphProvider.GetCardColor(id);
            nameBackground.color = border.color;
        }

        public void MarkAsRead()
        {
            if (graphProvider.AttemptMarkCardAsRead(id))
            {
                unreadIcon.gameObject.SetActive(false);
            }
        }

        public void UpdateUnreadIconVisibility()
        {
            unreadIcon.gameObject.SetActive(graphProvider.GetCardUnread(id));
        }

        public void PrepareRevealAnimation()
        {
            UpdateUnreadIconVisibility();
            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var wasRevealed = graphProvider.GetCardWasRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);
            var wasRumored = graphProvider.GetCardWasRumor(id);
            if (isRevealed != wasRevealed || isRumored != wasRumored)
            {
                isRevealAnimReady = true;
                gameObject.SetActive(true);
                background.gameObject.SetActive(true);
                if (isRevealed && !wasRevealed)
                {
                    rectTransform.localScale = new Vector3(1f, 0f, 1f);
                    if (!photo.sprite)
                    {
                        questionMark.gameObject.SetActive(true);
                        questionMark.SetAllDirty();
                    }
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
        }

        public void PlayRevealAnimation()
        {
            if (isRevealAnimReady)
            {
                startAnimTime = Time.unscaledTime;
                isRevealAnimReady = false;
                updateRevealAnim = true;
                enabled = true;
                return;
            }
        }

        public void OnSelect()
        {

        }

        private void UpdateStateVisuals()
        {
            var isRevealed = graphProvider.GetCardIsRevealed(id);
            var isRumored = graphProvider.GetCardIsRumor(id);
            gameObject.SetActive(isRevealed);
            photo.gameObject.SetActive(isRevealed && !isRumored && photo.sprite);
            background.gameObject.SetActive(isRevealed && isRumored);
            questionMark.gameObject.SetActive(isRevealed && (isRumored || !photo.sprite));
        }

        private void Update()
        {
            if (updateRevealAnim)
            {
                var isRevealed = graphProvider.GetCardIsRevealed(id);
                var isRumored = graphProvider.GetCardIsRumor(id);
                var wasRevealed = graphProvider.GetCardWasRevealed(id);
                var wasRumored = graphProvider.GetCardWasRumor(id);

                var t = Mathf.InverseLerp(startAnimTime, startAnimTime + 0.2f, Time.unscaledTime);

                if (isRevealed != wasRevealed)
                {
                    var v = isRevealed ? t : 1f - t;
                    var cardSize = graphProvider.GetCardSize(id);
                    rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, v) * cardSize;
                }
                if (isRumored != wasRumored)
                {
                    var v = isRumored ? t : 1f - t;
                    photo.rectTransform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), Vector3.one, v);
                }
                if (t == 1f)
                {
                    updateRevealAnim = false;
                    UpdateStateVisuals();
                    graphProvider.OnCardRevealStateUpdated(GetID());
                }
            }
            if (!updateRevealAnim)
            {
                enabled = false;
            }
        }
    }
}
