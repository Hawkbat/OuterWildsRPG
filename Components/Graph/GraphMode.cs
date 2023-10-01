using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static OuterWildsRPG.Components.Graph.SimpleGraph;

namespace OuterWildsRPG.Components.Graph
{
    public class GraphMode : ShipLogMode
    {
        IGraphProvider graphProvider;

        OWAudioSource oneShotSource;
        string prevEntryID;
        ScreenPromptList centerPromptList;
        ScreenPromptList upperRightPromptList;

        List<GraphCard> cardList = new();
        Dictionary<string, GraphCard> cardDict = new();
        List<GraphLink> linkList = new();
        Dictionary<string, GraphLink> linkDict = new();
        List<GraphLink> highlightedLinkList = new();
        List<GraphElement> cardOrLinkRevealQueue = new();

        OWAudioSource navigateAudioSource;
        ScreenPrompt zoomPrompt;
        ScreenPrompt skipPrompt;
        ScreenPrompt selectPrompt;
        ScreenPrompt markPrompt;

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

        GraphElement focusedElement;

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
            }
            else
            {
                throw new Exception($"No suitable {nameof(graphProvider)} provided while creating a graph mode of type {typeof(T).Name}.");
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

        IEnumerable<GraphElement> GetGraphElements() => cardList.Cast<GraphElement>().Concat(linkList);

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            this.oneShotSource = oneShotSource;
            this.centerPromptList = centerPromptList;
            this.upperRightPromptList = upperRightPromptList;

            highlightedLinkList = new List<GraphLink>();
            cardOrLinkRevealQueue = new List<GraphElement>();

            navigateAudioSource.SetLocalVolume(0f);

            zoomPrompt = new ScreenPrompt(InputLibrary.mapZoomIn, InputLibrary.mapZoomOut, UITextLibrary.GetString(UITextType.LogZoomPrompt), ScreenPrompt.MultiCommandType.POS_NEG, 0, ScreenPrompt.DisplayState.Normal, false);
            skipPrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.LogSkipPrompt), 0, ScreenPrompt.DisplayState.Normal, false);
            selectPrompt = new ScreenPrompt(InputLibrary.interact, "", 0, ScreenPrompt.DisplayState.Normal, false);
            markPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "", 0, ScreenPrompt.DisplayState.Normal, false);

            RefreshCards();
            RefreshLinks();
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
                card.Refresh();
            foreach (var link in linkList)
            {
                link.Refresh();
                var reverseLinkID = link.GetReversedID();
                if (linkDict.ContainsKey(reverseLinkID))
                {
                    var reverseLink = linkDict[reverseLinkID];
                    reverseLink.Refresh();
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

            foreach (var el in GetGraphElements())
                el.Refresh();

            focusedElement = null;
            canvasAnimator.AnimateTo(1f, Vector3.one, 0.5f, null, false);

            var customFocusID = graphProvider.GetInitialFocusedCard();

            var customRevealQueue = GetGraphElements().Where(el => el.ShouldPlayRevealAnimation());

            if (customRevealQueue.Count() > 0)
            {
                StartRevealAnimation(customRevealQueue.ToList());
            }
            else if (!string.IsNullOrEmpty(customFocusID))
            {
                CenterOnCard(customFocusID);
            }
            else
            {
                FrameRevealedCards();
            }

            slideProjector.OnEnterDetectiveMode();
            navigateAudioSource.SetLocalVolume(0f);
            navigateAudioSource.Play();

            Locator.GetPromptManager().AddScreenPrompt(zoomPrompt, upperRightPromptList, TextAnchor.MiddleRight, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(skipPrompt, upperRightPromptList, TextAnchor.MiddleRight, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(selectPrompt, centerPromptList, TextAnchor.MiddleCenter, -1, false);
            Locator.GetPromptManager().AddScreenPrompt(markPrompt, centerPromptList, TextAnchor.MiddleCenter, -1, false);
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
            Locator.GetPromptManager().RemoveScreenPrompt(selectPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(skipPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(markPrompt);
        }

        public override void UpdateMode()
        {
            UpdatePrompts();
            var x = scaleRoot.localScale.x;
            if (updateFrameAll)
            {
                UpdateFrameAll();
            }
            else if (updateSnapToCard)
            {
                UpdateSnapToCard();
            }
            else if (!updateRevealAnim)
            {
                UpdateNavigationInput();
                UpdateFocusedSelectable();
                if (focusedElement != null)
                {
                    if (focusedElement is GraphCard card)
                    {
                        bool marked = graphProvider.GetCardMarked(card.GetID());
                        bool canMark = !marked && graphProvider.CanMarkCard(card.GetID());
                        if (!descriptionField.IsVisible() && (marked || canMark))
                        {
                            string text = marked ? graphProvider.GetUnmarkCardPrompt(card.GetID()) : graphProvider.GetMarkCardPrompt(card.GetID());
                            markPrompt.SetText(text);
                            markPrompt.SetVisibility(true);
                            markPrompt.SetDisplayState(ScreenPrompt.DisplayState.Normal);
                        }
                        if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
                        {
                            if (!marked && canMark && graphProvider.AttemptMarkCard(card.GetID()))
                            {
                                RefreshAll();
                                oneShotSource.PlayOneShot(AudioType.ShipLogMarkLocation);
                            }
                            else if (marked && graphProvider.AttemptUnmarkCard(card.GetID()))
                            {
                                RefreshAll();
                                oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation);
                            }
                        }
                    }
                    if (!descriptionField.IsVisible() && OWInput.IsNewlyPressed(InputLibrary.interact))
                    {
                        if (focusedElement.CanSelect())
                        {
                            if (focusedElement.AttemptSelect())
                            {
                                SetDescriptionFieldTexts(focusedElement.GetDescription());
                                descriptionField.SetVisible(true);
                                RefreshAll();
                                oneShotSource.PlayOneShot(AudioType.ShipLogSelectEntry);
                            } else
                            {
                                oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                            }
                        }
                    }
                    else if (descriptionField.IsVisible() && (OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.cancel)))
                    {
                        if (focusedElement.AttemptDeselect())
                        {
                            descriptionField.SetVisible(false);
                            RefreshAll();
                            oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
                        } else
                        {
                            oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                        }
                    }
                }
            }
            else if (OWInput.IsNewlyPressed(InputLibrary.interact))
            {
                FinishRevealAnimation();
            }
            else
            {
                UpdateRevealAnimation();
            }
            var targetVolume = Mathf.Abs(x - scaleRoot.localScale.x) > 0.001f ? 1f : 0f;
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
            minScale = Mathf.Min(1f, boundsSize.y / boundsSize.x > ratio ? canvasSize.y / boundsSize.y : canvasSize.x / boundsSize.x);
            maxScale = 5f;
        }

        private void UpdatePrompts()
        {
            var canSelect = focusedElement != null && focusedElement.CanSelect();
            zoomPrompt.SetVisibility(!updateRevealAnim);
            selectPrompt.SetVisibility(!updateRevealAnim && canSelect && !descriptionField.IsVisible());
            skipPrompt.SetVisibility(updateRevealAnim);
            markPrompt.SetVisibility(false);
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
                            if (c.Equals(focusedElement)) return false;
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
            }
            else
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
            if (focusedElement != null && !focusedElement.CheckPointCollision(reticle.position))
            {
                if (descriptionField.IsVisible())
                {
                    if (focusedElement.AttemptDeselect())
                    {
                        descriptionField.SetVisible(false);
                        RefreshAll();
                        oneShotSource.PlayOneShot(AudioType.ShipLogDeselectEntry);
                    } else
                    {
                        oneShotSource.PlayOneShot(AudioType.NonDiaUINegativeSFX);
                    }
                }
                ChangeFocus(null);
            }
            var newFocusedElement = GetGraphElements().LastOrDefault(e => e.IsVisible() && e.CheckPointCollision(reticle.position));
            if (newFocusedElement != null && newFocusedElement != focusedElement)
            {
                ChangeFocus(newFocusedElement);
                if (Time.unscaledTime > enterModeTime + 0.5f)
                    oneShotSource.PlayOneShot(AudioType.ShipLogHighlightEntry);
                descriptionField.SetVisible(false);
            }
        }

        private void ChangeFocus(GraphElement selectable)
        {
            if (focusedElement != null)
            {
                focusedElement.Unfocus();
                foreach (var link in highlightedLinkList)
                    link.Unfocus();
                highlightedLinkList.Clear();
            }
            focusedElement = selectable;
            if (focusedElement == null)
                return;
            focusedElement.Focus();
            if (focusedElement is GraphCard card)
            {
                selectPrompt.SetText(graphProvider.GetSelectCardPrompt(card.GetID()));
                foreach (var link in linkList)
                {
                    if (link.GetTargetCard() == card)
                    {
                        highlightedLinkList.Add(link);
                        link.Focus();
                    }
                }
            }
            else if (focusedElement is GraphLink link)
            {
                selectPrompt.SetText(graphProvider.GetSelectLinkPrompt(link.GetSourceID(), link.GetTargetID()));
            }
        }

        public void CenterOnCard(string id)
        {
            if (!cardDict.ContainsKey(id)) return;
            var card = cardDict[id];
            panRoot.anchoredPosition = -card.GetAnchoredPosition();
            scaleRoot.localScale = Vector3.one;
            UpdateFocusedSelectable();
            if (focusedElement != null)
                descriptionField.SetVisible(true);
        }

        private void FrameRevealedCards()
        {
            var boundsSize = maxBounds - minBounds;
            var boundsCenter = minBounds + boundsSize * 0.5f;
            panRoot.anchoredPosition = -boundsCenter;
            scaleRoot.localScale = Vector3.one * minScale;
        }

        private void StartFrameAll()
        {
            updateFrameAll = true;
            startPanTime = Time.unscaledTime;
            startPanPos = panRoot.anchoredPosition;
            startScale = scaleRoot.localScale;
        }

        private void UpdateFrameAll()
        {
            var t = Mathf.InverseLerp(startPanTime, startPanTime + panDuration, Time.unscaledTime);
            t = Mathf.SmoothStep(0f, 1f, t);
            var boundsSize = maxBounds - minBounds;
            var boundsCenter = -(minBounds + boundsSize * 0.5f);
            panRoot.anchoredPosition = Vector2.Lerp(startPanPos, boundsCenter, t);
            scaleRoot.localScale = Vector2.Lerp(startScale, Vector2.one * minScale, t);
            if (t >= 1f)
            {
                FinishFrameAll();
            }
        }

        private void FinishFrameAll()
        {
            updateFrameAll = false;
        }

        private void StartSnapToCard(GraphCard card)
        {
            updateSnapToCard = true;
            startPanTime = Time.unscaledTime;
            startPanPos = panRoot.anchoredPosition;
            panDuration = 0.15f;
            targetCard = card;
        }

        private void FinishSnapToCard()
        {
            updateSnapToCard = false;
            targetCard = null;
        }

        private void UpdateSnapToCard()
        {
            var t = Mathf.InverseLerp(startPanTime, startPanTime + panDuration, Time.unscaledTime);
            t = Mathf.SmoothStep(0f, 1f, t);
            var pos = -targetCard.GetAnchoredPosition();
            panRoot.anchoredPosition = Vector2.Lerp(startPanPos, pos, t);
            if (t >= 1f)
            {
                FinishSnapToCard();
            }
        }

        private void StartRevealAnimation(List<GraphElement> revealQueue)
        {
            cardOrLinkRevealQueue = revealQueue;
            updateRevealAnim = true;
            updateFrameAll = false;
            targetCard = null;
            animWaitSeconds = 0.5f;
            panDuration = 0.7f;
            queueIndex = 0;
            startScale = scaleRoot.localScale;

            foreach (var el in cardOrLinkRevealQueue)
                if (el.IsVisible())
                    el.PrepareRevealAnimation();
        }

        private void FinishRevealAnimation()
        {
            foreach (var el in cardOrLinkRevealQueue)
            {
                if (el.IsRevealAnimationReady())
                    el.PlayRevealAnimation(panDuration);
            }
            cardOrLinkRevealQueue.Clear();
            updateRevealAnim = false;
            targetCard = null;
            StartFrameAll();
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
                    foreach (var el in cardOrLinkRevealQueue.Skip(queueIndex))
                    {
                        if (el is GraphLink link)
                        {
                            if (link.GetSourceID() == targetCard.GetID() && link.IsRevealAnimationReady() && !cardDict[link.GetTargetID()].IsRevealAnimationReady())
                            {
                                link.PlayRevealAnimation(panDuration);
                            }
                        }
                    }
                    targetCard.PlayRevealAnimation(0.2f);
                    oneShotSource.PlayOneShot(AudioType.ShipLogRevealEntry);
                    animWaitSeconds = 0.8f;
                    targetCard = null;
                    return;
                }
            }
            else
            {
                if (queueIndex < cardOrLinkRevealQueue.Count)
                {
                    var el = cardOrLinkRevealQueue[queueIndex];
                    if (el is GraphCard card)
                    {
                        if (card.IsRevealAnimationReady())
                        {
                            targetCard = card;
                            startPanTime = Time.unscaledTime;
                            startPanPos = panRoot.anchoredPosition;
                            startScale = scaleRoot.localScale;
                        }
                    } else if (el is GraphLink link)
                    {
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

        private void RefreshCards()
        {
            var missingIDs = new HashSet<string>(cardList.Select(c => c.GetID()));
            foreach (var cardID in graphProvider.GetCards())
            {
                if (cardDict.TryGetValue(cardID, out var card))
                {
                    card.Refresh();
                    missingIDs.Remove(cardID);
                } 
                else
                {
                    var go = Instantiate(entryCardTemplate, entryCardTemplate.transform.parent);
                    go.name = cardID;
                    card = go.GetComponent<GraphCard>();
                    card.gameObject.SetActive(true);
                    card.Init(cardID, this, graphProvider, fontAndLanguageController);
                    cardList.Add(card);
                    cardDict.Add(card.GetID(), card);
                }
            }
            foreach (var cardID in missingIDs)
            {
                if (cardDict.TryGetValue(cardID, out var card))
                {
                    cardDict.Remove(cardID);
                    cardList.Remove(card);
                    Destroy(card.gameObject);
                }
            }
        }

        private void RefreshLinks()
        {
            var missingIDs = new HashSet<string>(linkList.Select(l => l.GetID()));
            foreach (var linkPair in graphProvider.GetLinks())
            {
                var linkID = GraphLink.GetID(linkPair.Key, linkPair.Value);
                if (linkDict.TryGetValue(linkID, out var link))
                {
                    link.Refresh();
                    missingIDs.Remove(linkID);
                }
                else
                {
                    var sourceID = linkPair.Key;
                    var targetID = linkPair.Value;
                    var sourceCard = cardDict[sourceID];
                    var targetCard = cardDict[targetID];
                    var go = Instantiate(entryLinkTemplate, entryLinkTemplate.transform.parent);
                    go.name = $"LINK_{sourceID}-->{targetID}";
                    link = go.GetComponent<GraphLink>();
                    link.gameObject.SetActive(true);
                    link.Init(this, sourceCard, targetCard, graphProvider);
                    linkList.Add(link);
                    linkDict.Add(link.GetID(), link);
                }
            }
            foreach (var linkID in missingIDs)
            {
                if (linkDict.TryGetValue(linkID, out var link))
                {
                    linkDict.Remove(linkID);
                    linkList.Remove(link);
                    Destroy(link.gameObject);
                }
            }
        }

        private void RefreshAll()
        {
            RefreshCards();
            RefreshLinks();

            var customRevealQueue = GetGraphElements().Where(el => el.ShouldPlayRevealAnimation());

            if (customRevealQueue.Count() > 0)
            {
                StartRevealAnimation(customRevealQueue.ToList());
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
