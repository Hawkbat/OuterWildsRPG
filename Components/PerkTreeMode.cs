using OuterWildsRPG.Objects.Perks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class PerkTreeMode : GraphMode, GraphMode.IGraphProvider
    {
        HashSet<string> hasRevealed = new();
        HashSet<string> hasRead = new();
        Dictionary<string, Vector2> positions = new();

        string selected = string.Empty;

        public Perk GetPerk(string id) => PerkManager.GetPerk(id, OuterWildsRPG.ModID);

        public string GetLinkID(string sourceID, string targetID) => $"{sourceID}@{targetID}";

        public void CalculatePositions()
        {
            var roots = PerkManager.GetAllPerks().Where(p => p.Prereq == null).Reverse();
            var row = 0;
            var col = 0;
            foreach (var root in roots)
            {
                col = SetPerkPosition(root, row, col);
            }
        }

        int SetPerkPosition(Perk perk, int row, int col)
        {
            var width = GetDescendentWidth(perk);

            var x = 150f * col + (150f * width) * 0.5f;
            var y = 150f * row;

            positions[perk.FullID] = new Vector2(x, y);

            foreach (var dep in perk.Dependents)
            {
                col = SetPerkPosition(dep, row + 1, col);
            }
            return col + width;
        }

        int GetDescendentWidth(Perk perk)
        {
            if (!perk.Dependents.Any()) return 1;
            return perk.Dependents.Sum(GetDescendentWidth);
        }

        public bool GetNavigationSnapping() => true;

        public List<string> GetInitialCards() =>
            PerkManager.GetAllPerks().Select(p => p.FullID).ToList();

        public List<KeyValuePair<string, string>> GetInitialLinks() =>
            PerkManager.GetAllPerks()
            .Where(p => p.Prereq != null)
            .Select(p => new KeyValuePair<string, string>(p.FullID, p.Prereq.FullID))
            .ToList();

        public List<string> GetInitialRevealQueue()
            => PerkManager.GetAllPerks()
            .Where(p => !hasRevealed.Contains(p.FullID))
            .Select(p => p.FullID)
            .ToList();

        public string GetInitialFocusedCard() => string.Empty;

        public bool CanMarkCard(string id)
            => PerkManager.CanUnlockPerk(GetPerk(id));

        public bool AttemptMarkCard(string id)
            => PerkManager.UnlockPerk(GetPerk(id));

        public bool AttemptUnmarkCard(string id) => false;

        public bool AttemptMarkCardAsRead(string id) => hasRead.Add(id);

        public bool AttemptMarkLinkAsRead(string sourceID, string targetID) => hasRead.Add(GetLinkID(sourceID, targetID));

        public bool AttemptSelectCard(string id)
        {
            selected = id;
            return true;
        }

        public bool AttemptDeselectCard(string id)
        {
            selected = string.Empty;
            return true;
        }

        public bool AttemptSelectLink(string sourceID, string targetID)
        {
            selected = GetLinkID(sourceID, targetID);
            return true;
        }

        public bool AttemptDeselectLink(string sourceID, string targetID)
        {
            selected = string.Empty;
            return true;
        }

        public string GetCardName(string id) => GetPerk(id).Name;

        public Vector2 GetCardPosition(string id) => positions[id];

        public bool GetCardMarked(string id) => PerkManager.HasUnlockedPerk(GetPerk(id));

        public bool GetCardUnread(string id) => !hasRead.Contains(id);

        public bool GetCardMoreToExplore(string id) => false;

        public Sprite GetCardPhoto(string id) => Assets.GetEntitySprite(id);

        public float GetCardSize(string id) => 1;

        public Color GetCardColor(string id) =>
            PerkManager.HasUnlockedPerk(GetPerk(id)) ? new Color(0.8f, 0.8f, 0.8f) :
            PerkManager.CanUnlockPerk(GetPerk(id)) ? new Color(0.6f, 0.6f, 0.6f) :
            new Color(0.4f, 0.4f, 0.4f);

        public Color GetCardHighlightColor(string id)
        {
            var color = GetCardColor(id);
            Color.RGBToHSV(GetCardColor(id), out var h, out var s, out var v);
            return Color.HSVToRGB(h, s, Mathf.Clamp01(v + 0.4f));
        }

        public List<string> GetCardDescription(string id) => new();

        public List<string> GetLinkDescription(string sourceID, string targetID) => new();

        public bool GetCardIsRumor(string id) => false;

        public bool GetCardWasRumor(string id) => false;

        public bool GetCardIsRevealed(string id) => true;

        public bool GetCardWasRevealed(string id) => hasRevealed.Contains(id);

        public bool GetLinkIsRevealed(string sourceID, string targetID) => true;

        public bool GetLinkWasRevealed(string sourceID, string targetID) => hasRevealed.Contains(GetLinkID(sourceID, targetID));

        public void OnCardRevealStateUpdated(string id) => hasRevealed.Add(id);

        public void OnLinkRevealStateUpdated(string sourceID, string targetID) => hasRevealed.Add(GetLinkID(sourceID, targetID));
    }
}
