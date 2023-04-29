using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsRPG.Components
{
    public class GraphMode : ShipLogMode
    {
        IGraphProvider graphProvider;

        OWAudioSource oneShotSource;
        string prevEntryID;
        ScreenPromptList centerPromptList;
        ScreenPromptList upperRightPromptList;

        List<GraphCard> cardList;
        Dictionary<string, GraphCard> cardDict;
        List<GraphLink> linkList;
        Dictionary<string, GraphLink> linkDict;
        List<GraphLink> highlightedLinkList;
        List<string> cardOrLinkRevealQueue;

        OWAudioSource navigateAudioSource;
        ScreenPrompt zoomPrompt;
        ScreenPrompt viewEntryPrompt;
        ScreenPrompt skipPrompt;
        ScreenPrompt markOnHUDPrompt;

        float enterModeTime;
        bool updateRevealAnim;
        bool updateFrameAll;
        bool updateSnapToCard;
        GraphCard targetCard;
        float animWaitSeconds;
        float panDuration;
        int queueIndex;
        Vector2 startScale;
        float startPanTime;
        Vector2 startPanPos;
        Vector2 minBounds;
        Vector2 maxBounds;
        float minScale;
        float maxScale;
        float navSnapTime;
        
        IShipLogSelectable focusedSelectable;

        RectTransform panRoot;
        RectTransform scaleRoot;
        RectTransform rootCanvasTransform;
        RectTransform reticle;
        ShipLogEntryDescriptionField descriptionField;
        ShipLogSlideProjector slideProjector;
        CanvasGroupAnimator canvasAnimator;
        GameObject entryCardTemplate;
        GameObject entryLinkTemplate;

        FontAndLanguageController fontAndLanguageController;

        public interface IGraphProvider
        {
            public bool GetNavigationSnapping();
            public List<string> GetInitialCards();
            public List<KeyValuePair<string, string>> GetInitialLinks();
            public List<string> GetInitialRevealQueue();
            public string GetInitialFocusedCard();
            public bool CanMarkCard(string id);
            public bool AttemptMarkCard(string id);
            public bool AttemptUnmarkCard(string id);
            public bool AttemptMarkCardAsRead(string id);
            public bool AttemptMarkLinkAsRead(string sourceID, string targetID);
            public bool AttemptSelectCard(string id);
            public bool AttemptDeselectCard(string id);
            public bool AttemptSelectLink(string sourceID, string targetID);
            public bool AttemptDeselectLink(string sourceID, string targetID);
            public string GetCardName(string id);
            public Vector2 GetCardPosition(string id);
            public bool GetCardMarked(string id);
            public bool GetCardUnread(string id);
            public bool GetCardMoreToExplore(string id);
            public Sprite GetCardPhoto(string id);
            public float GetCardSize(string id);
            public Color GetCardColor(string id);
            public Color GetCardHighlightColor(string id);
            public List<string> GetCardDescription(string id);
            public List<string> GetLinkDescription(string sourceID, string targetID);
            public bool GetCardIsRumor(string id);
            public bool GetCardWasRumor(string id);
            public bool GetCardIsRevealed(string id);
            public bool GetCardWasRevealed(string id);
            public bool GetLinkIsRevealed(string sourceID, string targetID);
            public bool GetLinkWasRevealed(string sourceID, string targetID);
            public void OnCardRevealStateUpdated(string id);
            public void OnLinkRevealStateUpdated(string sourceID, string targetID);
        }

        public static T Create<T>() where T : GraphMode, IGraphProvider
            => Create<T>(null);

        public static T Create<T>(IGraphProvider graphProvider) where T : GraphMode
        {
            var detectiveModeTemplate = FindObjectOfType<ShipLogDetectiveMode>().gameObject;
            var go = Instantiate(detectiveModeTemplate, detectiveModeTemplate.transform.parent);
            go.SetActive(false);
            go.name = typeof(T).Name;
            var graphMode = ConvertFromDetectiveMode<T>(go.GetComponent<ShipLogDetectiveMode>());
            if (graphProvider != null)
            {
                graphMode.graphProvider = graphProvider;
            }
            else if (graphMode is IGraphProvider provider)
            {
                graphMode.graphProvider = provider;
            } else
            {
                throw new Exception($"No suitable {graphProvider} provided while creating a graph mode of type {typeof(T).Name}.");
            }
            go.transform.SetSiblingIndex(detectiveModeTemplate.transform.GetSiblingIndex() + 1);
            go.SetActive(true);
            return graphMode;
        }

        public static T ConvertFromDetectiveMode<T>(ShipLogDetectiveMode detectiveMode) where T : GraphMode
        {
            var graphMode = detectiveMode.gameObject.AddComponent<T>();
            graphMode.canvasAnimator = detectiveMode._canvasAnimator;
            graphMode.descriptionField = detectiveMode._descriptionField;

            graphMode.fontAndLanguageController = detectiveMode._fontAndLanguageController;
            graphMode.navigateAudioSource = detectiveMode._navigateAudioSource;
            graphMode.panRoot = detectiveMode._panRoot;
            graphMode.reticle = detectiveMode._reticle;
            graphMode.rootCanvasTransform = detectiveMode._rootCanvasTransform;
            graphMode.scaleRoot = detectiveMode._scaleRoot;
            graphMode.slideProjector = detectiveMode._slideProjector;

            graphMode.panRoot.GetComponent<Image>().enabled = false;

            var entryCardTemplate = Instantiate(detectiveMode.gameObject.GetComponentInChildren<ShipLogEntryCard>().gameObject, graphMode.panRoot);
            entryCardTemplate.SetActive(false);
            entryCardTemplate.name = "GraphCardTemplate";
            GraphCard.ConvertFromEntryCard(entryCardTemplate.GetComponent<ShipLogEntryCard>());
            graphMode.entryCardTemplate = entryCardTemplate;

            var entryLinkTemplate = Instantiate(detectiveMode.gameObject.GetComponentInChildren<ShipLogEntryLink>().gameObject, graphMode.panRoot);
            entryLinkTemplate.SetActive(false);
            entryLinkTemplate.name = "GraphLinkTemplate";
            GraphLink.ConvertFromEntryLink(entryLinkTemplate.GetComponent<ShipLogEntryLink>());
            graphMode.entryLinkTemplate = entryLinkTemplate;

            foreach (var entryCard in graphMode.panRoot.GetComponentsInChildren<ShipLogEntryCard>())
                if (entryCard.gameObject != entryCardTemplate)
                    Destroy(entryCard.gameObject);
            foreach (var entryLink in graphMode.panRoot.GetComponentsInChildren<ShipLogEntryLink>())
                if (entryLink.gameObject != entryLinkTemplate)
                    Destroy(entryLink.gameObject);

            Destroy(detectiveMode);
            return graphMode;
        }

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            this.oneShotSource = oneShotSource;
            this.centerPromptList = centerPromptList;
            this.upperRightPromptList = upperRightPromptList;

            highlightedLinkList = new List<GraphLink>();
            cardOrLinkRevealQueue = new List<string>();

            navigateAudioSource.SetLocalVolume(0f);

            zoomPrompt = new ScreenPrompt(InputLibrary.mapZoomIn, InputLibrary.mapZoomOut, UITextLibrary.GetString(UITextType.LogZoomPrompt), ScreenPrompt.MultiCommandType.POS_NEG, 0, ScreenPrompt.DisplayState.Normal, false);
            viewEntryPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogViewPrompt), 0, ScreenPrompt.DisplayState.Normal, false);
            skipPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogSkipPrompt), 0, ScreenPrompt.DisplayState.Normal, false);
            markOnHUDPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "", 0, ScreenPrompt.DisplayState.Normal, false);

            GenerateCards();
            GenerateLinks();
            canvasAnimator.SetImmediate(0f, Vector3.one * 0.5f);
        }

        public override bool AllowModeSwap() => !updateRevealAnim;

        public override bool AllowCancelInput() => !updateRevealAnim && !descriptionField.IsVisible();

        public override string GetFocusedEntryID() => prevEntryID;

        public override void OnEnterComputer()
        {
            UpdateBounds(false);
            FrameRevealedCards();
            foreach (var card in cardList)
                card.OnEnterComputer();
            foreach (var link in linkList)
            {
                link.UpdatePosition();
                link.UpdateVisibility();
                var reverseLinkID = link.GetReversedID();
                if (linkDict.ContainsKey(reverseLinkID))
                {
                    var reverseLink = linkDict[reverseLinkID];
                    reverseLink.UpdateVisibility();
                    if (reverseLink.IsVisible() && link.IsVisible())
                    {
                        if (reverseLink.transform.GetSiblingIndex() < link.transform.GetSiblingIndex())
                        {
                            link.Hide();
                        }
                        else
                        {
                            reverseLink.Hide();
                        }
                    }
                }
            }
        }

        public override void OnExitComputer()
        {

        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            prevEntryID = entryID;

            enterModeTime = Time.unscaledTime;
            foreach (var card in cardList)
            {
                if (graphProvider.GetCardIsRevealed(card.GetID()))
                {
                    card.OnEnterDetectiveMode();
                    card.SetMarked(graphProvider.GetCardMarked(card.GetID()));
                }
            }
            focusedSelectable = null;
            canvasAnimator.AnimateTo(1f, Vector3.one, 0.5f, null, false);

            var customFocusID = graphProvider.GetInitialFocusedCard();
            var customRevealQueue = graphProvider.GetInitialRevealQueue();
            if (customRevealQueue != null && customRevealQueue.Count > 0)
            {
                PlayRevealAnimation(customRevealQueue);
            } else if (customFocusID.Length > 0)
            {
                CenterOnCard(customFocusID);
            } else
            {
                FrameRevealedCards();
            }

            slideProjector.OnEnterDetectiveMode();
            navigateAudioSource.SetLocalVolume(0f);
            navigateAudioSource.Play();

            Locator.GetPromptManager().AddScreenPrompt(zoomPrompt, upperRightPromptList, TextAnchor.MiddleRight, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(skipPrompt, upperRightPromptList, TextAnchor.MiddleRight, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(viewEntryPrompt, centerPromptList, TextAnchor.MiddleCenter, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(markOnHUDPrompt, centerPromptList, TextAnchor.MiddleCenter, -1, false);
        }

        private void PlayRevealAnimation(List<string> revealQueue)
        {
            cardOrLinkRevealQueue = revealQueue;
            updateRevealAnim = true;
            updateFrameAll = false;
            targetCard = null;
            animWaitSeconds = 0.5f;
            panDuration = 0.7f;
            queueIndex = 0;
            startScale = scaleRoot.localScale;
            PrepareRevealAnimations();
        }

        public void CenterOnCard(string id)
        {
            if (!cardDict.ContainsKey(id)) return;
            var card = cardDict[id];
            panRoot.anchoredPosition = -card.GetAnchoredPosition();
            scaleRoot.localScale = Vector3.one;
            UpdateFocusedSelectable();
            if (focusedSelectable != null)
                descriptionField.SetVisible(true);
        }

        public override void ExitMode()
        {
            navigateAudioSource.SetLocalVolume(0f);
            navigateAudioSource.Stop();
            updateRevealAnim = false;
            updateFrameAll = false;
            descriptionField.SetVisible(false);
            canvasAnimator.AnimateTo(0f, Vector3.one * 0.05f, 0.5f, null, false);
            ChangeFocus(null);

            Locator.GetPromptManager().RemoveScreenPrompt(zoomPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(viewEntryPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(skipPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(markOnHUDPrompt);
        }

        public override void UpdateMode()
        {
            UpdatePrompts();
            var x = scaleRoot.localScale.x;
            if (updateFrameAll)
            {
                var t = Mathf.InverseLerp(startPanTime, startPanTime + panDuration, Time.unscaledTime);
                t = Mathf.SmoothStep(0f, 1f, t);
                var boundsSize = maxBounds - minBounds;
                var boundsCenter = -(minBounds + boundsSize * 0.5f);
                panRoot.anchoredPosition = Vector2.Lerp(startPanPos, boundsCenter, t);
                scaleRoot.localScale = Vector2.Lerp(startScale, Vector2.one * minScale, t);
                if (t >= 1f)
                {
                    updateFrameAll = false;
                }
            } else if (updateSnapToCard)
            {
                UpdateSnapToCard();
            } else if (!updateRevealAnim)
            {
                UpdateNavigationInput();
                UpdateFocusedSelectable();
                if (focusedSelectable != null)
                {
                    if (focusedSelectable.GetType() == typeof(GraphCard))
                    {
                        var card = (GraphCard)focusedSelectable;
                        bool marked = graphProvider.GetCardMarked(card.GetID());
                        bool canMark = !marked && graphProvider.CanMarkCard(card.GetID());
                        if (!descriptionField.IsVisible() && (marked || canMark))
                        {
                            string text = marked ? UITextLibrary.GetString(UITextType.LogRemoveMarkerPrompt) : UITextLibrary.GetString(UITextType.LogMarkLocationPrompt);
                            markOnHUDPrompt.SetText(text);
                            markOnHUDPrompt.SetVisibility(true);
                            markOnHUDPrompt.SetDisplayState(ScreenPrompt.DisplayState.Normal);
                        }
                        if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
                        {
                            if (canMark && graphProvider.AttemptMarkCard(card.GetID()))
                            {
                                card.SetMarked(true);
                                oneShotSource.PlayOneShot(AudioType.ShipLogMarkLocation);
                            } else if (marked && graphProvider.AttemptUnmarkCard(card.GetID()))
                            {
                                card.SetMarked(false);
                                oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation);
                            }
                        }
                    }
                    if (!descriptionField.IsVisible() && OWInput.IsNewlyPressed(InputLibrary.interact))
                    {
                        if (focusedSelectable is GraphCard card && !graphProvider.AttemptSelectCard(card.GetID()))
                        {
                            oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                        } else if (focusedSelectable is GraphLink link && !graphProvider.AttemptSelectLink(link.GetSourceID(), link.GetTargetID()))
                        {
                            oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                        } else
                        {
                            descriptionField.SetVisible(true);
                            focusedSelectable.MarkAsRead();
                            oneShotSource.PlayOneShot(AudioType.ShipLogSelectEntry);
                        }
                    } else if (descriptionField.IsVisible() && (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel)))
                    {
                        if (focusedSelectable is GraphCard card && !graphProvider.AttemptDeselectCard(card.GetID()))
                        {
                            oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                        }
                        else if (focusedSelectable is GraphLink link && !graphProvider.AttemptDeselectLink(link.GetSourceID(), link.GetTargetID()))
                        {
                            oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                        }
                        else
                        {
                            descriptionField.SetVisible(false);
                            oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
                        }
                    }
                }
            } else if (OWInput.IsNewlyPressed(InputLibrary.interact))
            {
                FinishRevealAnimation();
            } else
            {
                UpdateRevealAnimation();
            }
            var targetVolume = (Mathf.Abs(x - scaleRoot.localScale.x) > 0.001f) ? 1f : 0f;
            navigateAudioSource.SetLocalVolume(Mathf.MoveTowards(navigateAudioSource.GetLocalVolume(), targetVolume, Time.unscaledDeltaTime * 10f));
        }

        private void UpdateBounds(bool newlyRevealedOnly = false)
        {
            minBounds = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            maxBounds = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            var offset = 275f;
            foreach (var card in cardList)
            {
                if (graphProvider.GetCardIsRevealed(card.GetID()) && (!newlyRevealedOnly || card.IsRevealAnimationReady()))
                {
                    minBounds.x = Mathf.Min(minBounds.x, card.GetAnchoredPosition().x - offset);
                    minBounds.y = Mathf.Min(minBounds.y, card.GetAnchoredPosition().y - offset);
                    maxBounds.x = Mathf.Max(maxBounds.x, card.GetAnchoredPosition().x + offset);
                    maxBounds.y = Mathf.Max(maxBounds.y, card.GetAnchoredPosition().y + offset);
                }
            }
            var canvasSize = rootCanvasTransform.sizeDelta;
            var ratio = canvasSize.y / canvasSize.x;
            var boundsSize = maxBounds - minBounds;
            minScale = Mathf.Min(1f, (boundsSize.y / boundsSize.x > ratio) ? (canvasSize.y / boundsSize.y) : (canvasSize.x / boundsSize.x));
            maxScale = 5f;
        }

        private void FrameRevealedCards()
        {
            var boundsSize = maxBounds - minBounds;
            var boundsCenter = minBounds + boundsSize * 0.5f;
            panRoot.anchoredPosition = -boundsCenter;
            scaleRoot.localScale = Vector3.one * minScale;
        }

        private void UpdatePrompts()
        {
            zoomPrompt.SetVisibility(!updateRevealAnim);
            viewEntryPrompt.SetVisibility(!updateRevealAnim && focusedSelectable != null && !descriptionField.IsVisible());
            skipPrompt.SetVisibility(updateRevealAnim);
            markOnHUDPrompt.SetVisibility(false);
        }

        private void UpdateNavigationInput()
        {
            var zoom = scaleRoot.localScale.x;

            var panDelta = OWInput.GetAxisValue(InputLibrary.moveXZ);
            var pan = -panRoot.anchoredPosition;

            if (graphProvider.GetNavigationSnapping())
            {
                if (panDelta.magnitude < 0.1f)
                {
                    navSnapTime = float.MinValue;
                }
                else if (Time.unscaledTime > navSnapTime + 0.25f)
                {
                    var point = -panRoot.anchoredPosition;
                    var dir = panDelta.normalized;
                    for (var allowedAngle = 0.2f; allowedAngle < 1f; allowedAngle += 0.2f)
                    {
                        var card = cardList.Where(c =>
                        {
                            if (c.Equals(focusedSelectable)) return false;
                            var offset = c.GetAnchoredPosition() - point;
                            var angle = 1f - Vector2.Dot(dir, offset.normalized);
                            return angle <= allowedAngle;
                        }).OrderBy(c => (c.GetAnchoredPosition() - point).magnitude).FirstOrDefault();
                        if (card != null)
                        {
                            navSnapTime = Time.unscaledTime;
                            StartSnapToCard(card);
                            break;
                        }
                    }
                }
            } else
            {
                pan += panDelta * (50f / scaleRoot.localScale.x) * Time.unscaledDeltaTime * 10f;
                pan.x = Mathf.Clamp(pan.x, minBounds.x, maxBounds.x);
                pan.y = Mathf.Clamp(pan.y, minBounds.y, maxBounds.y);
                panRoot.anchoredPosition = -pan;
            }
            
            var zoomDelta = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
            zoom += 3f * zoomDelta * zoom * Time.unscaledDeltaTime;
            zoom = Mathf.Clamp(zoom, minScale, maxScale);
            scaleRoot.localScale = Vector3.one * zoom;
        }

        private void UpdateFocusedSelectable()
        {
            if (focusedSelectable != null && !focusedSelectable.CheckPointCollision(reticle.position))
            {
                if (descriptionField.IsVisible())
                {
                    if (focusedSelectable is GraphCard card && !graphProvider.AttemptDeselectCard(card.GetID()))
                    {
                        oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                    } else if (focusedSelectable is GraphLink link && !graphProvider.AttemptDeselectLink(link.GetSourceID(), link.GetTargetID()))
                    {
                        oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                    } else
                    {
                        descriptionField.SetVisible(false);
                        oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
                    }
                }
                ChangeFocus(null);
            }
            foreach (var card in cardList.Where(c => c.IsVisible() && c.CheckPointCollision(reticle.position)).Reverse())
            {
                if (!card.Equals(focusedSelectable))
                {
                    ChangeFocus(card);
                    if (Time.unscaledTime > enterModeTime + 0.5f)
                        oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);
                    SetDescriptionFieldTexts(graphProvider.GetCardDescription(card.GetID()));
                    descriptionField.SetVisible(false);
                }
                return;
            }
            foreach (var link in linkList.Where(l => l.IsVisible() & l.CheckPointCollision(reticle.position)))
            {
                if (!link.Equals(focusedSelectable))
                {
                    ChangeFocus(link);
                    if (Time.unscaledTime > enterModeTime + 0.5f)
                        oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);
                    SetDescriptionFieldTexts(graphProvider.GetLinkDescription(link.GetSourceID(), link.GetTargetID()));
                    descriptionField.SetVisible(false);
                }
                return;
            }
        }

        private void ChangeFocus(IShipLogSelectable selectable)
        {
            if (focusedSelectable != null)
            {
                focusedSelectable.OnLoseFocus();
                foreach (var link in highlightedLinkList)
                    link.OnLoseFocus();
                highlightedLinkList.Clear();
            }
            focusedSelectable = selectable;
            if (focusedSelectable == null)
                return;
            focusedSelectable.OnGainFocus();
            if (focusedSelectable.GetType() == typeof(GraphCard))
            {
                var card = (GraphCard)focusedSelectable;
                var isRumor = graphProvider.GetCardIsRumor(card.GetID());
                viewEntryPrompt.SetText(isRumor ? UITextLibrary.GetString(UITextType.LogViewRumoredEntryPrompt) : UITextLibrary.GetString(UITextType.LogViewPrompt));
                if (isRumor)
                {
                    foreach (var link in linkList)
                    {
                        if (link.GetTargetCard() == card)
                        {
                            highlightedLinkList.Add(link);
                            link.OnGainFocus();
                        }
                    }
                }
            } else
            {
                viewEntryPrompt.SetText(UITextLibrary.GetString(UITextType.LogViewRumorPrompt));
            }
        }

        private void PrepareRevealAnimations()
        {
            foreach (var id in cardOrLinkRevealQueue)
            {
                if (cardDict.ContainsKey(id))
                {
                    var card = cardDict[id];
                    if (!card.IsRevealAnimationReady())
                        card.PrepareRevealAnimation();
                }
                if (linkDict.ContainsKey(id))
                {
                    var link = linkDict[id];
                    if (link.IsVisible() && !link.IsRevealAnimationReady())
                        link.PrepareRevealAnimation();
                }
            }
        }

        private void StartSnapToCard(GraphCard card)
        {
            updateSnapToCard = true;
            startPanTime = Time.unscaledTime;
            startPanPos = panRoot.anchoredPosition;
            panDuration = 0.15f;
            targetCard = card;
        }

        private void UpdateSnapToCard()
        {
            var t = Mathf.InverseLerp(startPanTime, startPanTime + panDuration, Time.unscaledTime);
            t = Mathf.SmoothStep(0f, 1f, t);
            var pos = -targetCard.GetAnchoredPosition();
            panRoot.anchoredPosition = Vector2.Lerp(startPanPos, pos, t);
            if (t >= 1f)
            {
                updateSnapToCard = false;
                targetCard = null;
            }
        }

        private void UpdateRevealAnimation()
        {
            if (animWaitSeconds > 0f)
            {
                animWaitSeconds = Mathf.MoveTowards(animWaitSeconds, 0f, Time.unscaledDeltaTime);
                return;
            }
            if (targetCard != null)
            {
                var t = Mathf.InverseLerp(startPanTime, startPanTime + panDuration, Time.unscaledTime);
                t = Mathf.SmoothStep(0f, 1f, t);
                var pos = -targetCard.GetAnchoredPosition();
                panRoot.anchoredPosition = Vector2.Lerp(startPanPos, pos, t);
                scaleRoot.localScale = Vector2.Lerp(startScale, Vector2.one, t);
                if (t >= 1f)
                {
                    foreach (var id in cardOrLinkRevealQueue.Skip(queueIndex))
                    {
                        if (linkDict.ContainsKey(id))
                        {
                            var link = linkDict[id];
                            if (link.GetSourceID() == targetCard.GetID() && link.IsRevealAnimationReady() && !cardDict[link.GetTargetID()].IsRevealAnimationReady())
                            {
                                link.PlayRevealAnimation(panDuration);
                            }
                        }
                    }
                    targetCard.PlayRevealAnimation();
                    oneShotSource.PlayOneShot(AudioType.ShipLogRevealEntry);
                    animWaitSeconds = 0.8f;
                    targetCard = null;
                    return;
                }
            } else
            {
                if (queueIndex < cardOrLinkRevealQueue.Count)
                {
                    var id = cardOrLinkRevealQueue[queueIndex];
                    if (cardDict.ContainsKey(id))
                    {
                        var card = cardDict[id];
                        if (card.IsRevealAnimationReady())
                        {
                            targetCard = card;
                            startPanTime = Time.unscaledTime;
                            startPanPos = panRoot.anchoredPosition;
                            startScale = scaleRoot.localScale;
                        }
                    }
                    if (linkDict.ContainsKey(id))
                    {
                        var link = linkDict[id];
                        if (link.IsRevealAnimationReady())
                        {
                            targetCard = link.GetSourceCard();
                            startPanTime = Time.unscaledTime;
                            startPanPos = panRoot.anchoredPosition;
                            startScale = scaleRoot.localScale;
                            link.PlayRevealAnimation(panDuration);
                        }
                    }
                    queueIndex++;
                    return;
                }
                FinishRevealAnimation();
            }
        }

        private void FinishRevealAnimation()
        {
            foreach (var id in cardOrLinkRevealQueue)
            {
                if (cardDict.ContainsKey(id))
                {
                    var card = cardDict[id];
                    if (card.IsRevealAnimationReady())
                    {
                        card.PlayRevealAnimation();
                    }
                }
                if (linkDict.ContainsKey(id))
                {
                    var link = linkDict[id];
                    if (link.IsRevealAnimationReady())
                    {
                        link.PlayRevealAnimation(panDuration);
                    }
                }
            }
            cardOrLinkRevealQueue.Clear();
            updateRevealAnim = false;
            targetCard = null;
            updateFrameAll = true;
            startPanTime = Time.unscaledTime;
            startPanPos = panRoot.anchoredPosition;
            startScale = scaleRoot.localScale;
        }

        private void GenerateCards()
        {
            cardList = new List<GraphCard>();
            cardDict = new Dictionary<string, GraphCard>();
            foreach (var cardID in graphProvider.GetInitialCards())
            {
                var go = Instantiate(entryCardTemplate, entryCardTemplate.transform.parent);
                go.name = cardID;
                var card = go.GetComponent<GraphCard>();
                card.gameObject.SetActive(true);
                card.Init(cardID, graphProvider, fontAndLanguageController);
                cardList.Add(card);
                cardDict.Add(card.GetID(), card);
            }
        }

        private void GenerateLinks()
        {
            linkList = new List<GraphLink>();
            linkDict = new Dictionary<string, GraphLink>();
            foreach (var linkPair in graphProvider.GetInitialLinks())
            {
                var sourceID = linkPair.Key;
                var targetID = linkPair.Value;
                var sourceCard = cardDict[sourceID];
                var targetCard = cardDict[targetID];
                var go = Instantiate(entryLinkTemplate, entryLinkTemplate.transform.parent);
                go.name = $"LINK_{sourceID}-->{targetID}";
                var link = go.GetComponent<GraphLink>();
                link.gameObject.SetActive(true);
                link.Init(sourceCard, targetCard, graphProvider);
                linkList.Add(link);
                linkDict.Add(link.GetID(), link);
            }
            foreach (var link in linkList)
            {
                link.UpdatePosition();
                link.UpdateVisibility();
            }
        }

        private void SetDescriptionFieldTexts(IEnumerable<string> texts)
        {
            descriptionField.ResetListPos();
            descriptionField._link = null;
            descriptionField._entry = null;
            descriptionField._displayCount = 0;
            foreach (var item in descriptionField._factListItems)
                item.Clear();
            foreach (var text in texts)
                descriptionField._factListItems[descriptionField._displayCount++].DisplayText(text);
        }
    }
}
