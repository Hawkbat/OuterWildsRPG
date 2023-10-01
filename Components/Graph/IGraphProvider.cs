using System.Collections.Generic;
using UnityEngine;

namespace OuterWildsRPG.Components.Graph
{
    public interface IGraphProvider
    {
        public bool GetNavigationSnapping();
        public string GetMarkCardPrompt(string id);
        public string GetUnmarkCardPrompt(string id);
        public string GetSelectCardPrompt(string id);
        public string GetSelectLinkPrompt(string sourceID, string targetID);
        public IEnumerable<string> GetCards();
        public IEnumerable<KeyValuePair<string, string>> GetLinks();
        public string GetInitialFocusedCard();
        public bool CanMarkCard(string id);
        public bool AttemptMarkCard(string id);
        public bool AttemptUnmarkCard(string id);
        public bool CanSelectCard(string id);
        public bool AttemptSelectCard(string id);
        public bool AttemptDeselectCard(string id);
        public bool CanSelectLink(string sourceID, string targetID);
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
        public IEnumerable<string> GetCardDescription(string id);
        public IEnumerable<string> GetLinkDescription(string sourceID, string targetID);
        public bool GetCardIsRumor(string id);
        public bool GetCardWasRumor(string id);
        public bool GetCardIsRevealed(string id);
        public bool GetCardWasRevealed(string id);
        public bool GetLinkIsRevealed(string sourceID, string targetID);
        public bool GetLinkWasRevealed(string sourceID, string targetID);
        public void OnCardRevealStateUpdated(string id, bool revealed, bool rumored);
        public void OnLinkRevealStateUpdated(string sourceID, string targetID, bool revealed);
    }
}
