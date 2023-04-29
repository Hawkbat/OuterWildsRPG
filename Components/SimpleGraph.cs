﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class SimpleGraph : GraphMode.IGraphProvider
    {
        Dictionary<string, Card> cards = new();
        Dictionary<string, Link> links = new();
        string initialFocusedCard = string.Empty;
        string selected = string.Empty;
        bool useSnapping;

        public SimpleGraph() { }

        public SimpleGraph(List<Card> cards, List<Link> links)
        {
            foreach (var card in cards) AddCard(card);
            foreach (var link in links) AddLink(link);
        }

        public void AddCard(Card card)
        {
            cards[card.ID] = card;
        }

        public void AddLink(Link link)
        {
            links[link.GetID()] = link;
        }

        public Card GetCard(string id) => cards[id];
        public Link GetLink(string sourceID, string targetID) => links[$"{sourceID}@{targetID}"];

        public void SetInitialCard(Card card)
        {
            initialFocusedCard = card != null ? card.ID : string.Empty;
        }

        public Card GetSelectedCard() => GetCard(selected);

        public Link GetSelectedLink()
        {
            if (selected.Contains("@"))
            {
                var bits = selected.Split('@');
                var sourceID = bits[0];
                var targetID = bits[1];
                return GetLink(sourceID, targetID);
            }
            return null;
        }

        public void SetNavigationSnapping(bool snapping)
        {
            useSnapping = snapping;
        }

        public bool GetNavigationSnapping() => useSnapping;

        public bool AttemptDeselectCard(string id)
        {
            if (selected == GetCard(id).ID)
            {
                selected = string.Empty;
                return true;
            }
            return false;
        }

        public bool AttemptDeselectLink(string sourceID, string targetID)
        {
            if (selected == GetLink(sourceID, targetID).GetID())
            {
                selected = string.Empty;
                return true;
            }
            return false;
        }

        public bool AttemptMarkCard(string id)
        {
            var card = GetCard(id);
            if (!card.Marked && card.CanMark)
            {
                card.Marked = true;
                return true;
            }
            return false;
        }

        public bool AttemptMarkCardAsRead(string id)
        {
            var card = GetCard(id);
            if (card.Unread)
            {
                card.Unread = false;
                return true;
            }
            return false;
        }

        public bool AttemptMarkLinkAsRead(string sourceID, string targetID) => true;

        public bool AttemptSelectCard(string id)
        {
            var card = GetCard(id);
            if (selected != card.ID)
            {
                selected = card.ID;
                return true;
            }
            return false;
        }

        public bool AttemptSelectLink(string sourceID, string targetID)
        {
            var link = GetLink(sourceID, targetID);
            if (selected != link.GetID())
            {
                selected = link.GetID();
                return true;
            }
            return false;
        }

        public bool AttemptUnmarkCard(string id)
        {
            var card = GetCard(id);
            if (card.Marked)
            {
                card.Marked = false;
                return true;
            }
            return false;
        }

        public bool CanMarkCard(string id) => GetCard(id).CanMark && !GetCard(id).Marked;

        public Color GetCardColor(string id) => GetCard(id).Color;

        public List<string> GetCardDescription(string id) => GetCard(id).DescriptionLines;

        public Color GetCardHighlightColor(string id) => GetCard(id).HighlightColor;

        public bool GetCardIsRevealed(string id) => GetCard(id).CurrentState != Card.State.Hidden;

        public bool GetCardIsRumor(string id) => GetCard(id).CurrentState == Card.State.Rumored;

        public bool GetCardMarked(string id) => GetCard(id).Marked;

        public bool GetCardMoreToExplore(string id) => GetCard(id).MoreToExplore;

        public string GetCardName(string id) => GetCard(id).Name;

        public Sprite GetCardPhoto(string id) => GetCard(id).Photo;

        public Vector2 GetCardPosition(string id) => GetCard(id).Position;

        public float GetCardSize(string id) => GetCard(id).Size;

        public bool GetCardUnread(string id) => GetCard(id).Unread;

        public bool GetCardWasRevealed(string id) => GetCard(id).PreviousState != Card.State.Hidden;

        public bool GetCardWasRumor(string id) => GetCard(id).PreviousState == Card.State.Rumored;

        public string GetInitialFocusedCard() => initialFocusedCard;

        public List<string> GetInitialCards() => cards.Values.Select(c => c.ID).ToList();

        public List<KeyValuePair<string, string>> GetInitialLinks()
            => links.Values.Select(l => new KeyValuePair<string, string>(l.SourceID, l.TargetID)).ToList();

        public List<string> GetInitialRevealQueue()
            => cards.Values.Where(c => c.CurrentState != c.PreviousState).Select(c => c.ID)
            .Concat(links.Values.Where(l => l.CurrentState != l.PreviousState).Select(l => l.GetID()))
            .ToList();

        public List<string> GetLinkDescription(string sourceID, string targetID)
            => GetLink(sourceID, targetID).DescriptionLines;

        public bool GetLinkIsRevealed(string sourceID, string targetID)
            => GetLink(sourceID, targetID).CurrentState == Link.State.Visible;

        public bool GetLinkWasRevealed(string sourceID, string targetID)
            => GetLink(sourceID, targetID).PreviousState == Link.State.Visible;

        public void OnCardRevealStateUpdated(string id)
        {
            var card = GetCard(id);
            card.PreviousState = card.CurrentState;
        }

        public void OnLinkRevealStateUpdated(string sourceID, string targetID)
        {
            var link = GetLink(sourceID, targetID);
            link.PreviousState = link.CurrentState;
        }

        public class Card
        {
            public string ID;
            public string Name;
            public Vector2 Position;
            public bool Marked;
            public bool CanMark;
            public bool Unread;
            public bool MoreToExplore;
            public Sprite Photo;
            public float Size;
            public Color Color;
            // Generally the same hue and saturation, just with value upped to 100%
            public Color HighlightColor;
            public List<string> DescriptionLines;

            public State CurrentState;
            public State PreviousState;

            public Card(string id, string name, Vector2 position, List<string> descriptionLines = null, bool marked = false, bool canMark = false, bool unread = false, bool moreToExplore = false, Sprite photo = null, float size = 1f, Color? color = null, Color? highlightColor = null, State currentState = State.Revealed, State previousState = State.Hidden)
            {
                ID = id;
                Name = name;
                Position = position;
                DescriptionLines = descriptionLines ?? new List<string>();
                Marked = marked;
                CanMark = canMark;
                Unread = unread;
                MoreToExplore = moreToExplore;
                Photo = photo;
                Size = size;
                Color = color ?? new Color(0.6f, 0.6f, 0.6f);

                if (highlightColor.HasValue)
                {
                    HighlightColor = highlightColor.Value;
                }
                else
                {
                    Color.RGBToHSV(Color, out float h, out float s, out float v);
                    HighlightColor = Color.HSVToRGB(h, s, 1f);
                }

                CurrentState = currentState;
                PreviousState = previousState;
            }

            public enum State
            {
                Hidden = 0,
                Rumored = 1,
                Revealed = 2,
            }
        }

        public class Link
        {
            public string SourceID;
            public string TargetID;
            public List<string> DescriptionLines;

            public State CurrentState;
            public State PreviousState;

            public Link(string sourceID, string targetID, List<string> descriptionLines = null, State currentState = State.Visible, State previousState = State.Hidden)
            {
                SourceID = sourceID;
                TargetID = targetID;
                DescriptionLines = descriptionLines ?? new List<string>();
                CurrentState = currentState;
                PreviousState = previousState;
            }

            public string GetID() => $"{SourceID}@{TargetID}";

            public enum State
            {
                Hidden = 0,
                Visible = 1,
            }
        }
    }
}
