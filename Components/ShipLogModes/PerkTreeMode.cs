using OuterWildsRPG.Components.Graph;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.ShipLogModes
{
    public class PerkTreeMode : GraphMode, IGraphProvider
    {
        readonly Dictionary<string, Vector2> positions = new();

        public Perk GetPerk(string id) => PerkManager.GetPerk(id, OuterWildsRPG.ModID);

        public string GetLinkID(string sourceID, string targetID) => GraphLink.GetID(sourceID, targetID);

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

            var x = 150f * col + 150f * (width - 1) * 0.5f;
            var y = -210f * row;

            positions[perk.FullID] = new Vector2(x, y);

            var childColumn = col;
            foreach (var dep in perk.Dependents)
            {
                childColumn = SetPerkPosition(dep, row + 1, childColumn);
            }
            return col + width;
        }

        int GetDescendentWidth(Perk perk)
        {
            if (!perk.Dependents.Any()) return 1;
            return perk.Dependents.Sum(GetDescendentWidth);
        }

        public bool GetNavigationSnapping() => true;
        public string GetMarkCardPrompt(string id)
            => GetPerk(id) != null ? Translations.PromptUnlockPerk(GetPerk(id), GetPerk(id).GetPerkCost(false), PerkManager.GetUnspentPerkPoints()) : string.Empty;
        public string GetUnmarkCardPrompt(string id)
            => GetPerk(id) != null ? Translations.PromptRefundPerk(GetPerk(id), GetPerk(id).GetPerkCost(true)) : string.Empty;
        public string GetSelectCardPrompt(string id)
            => GetPerk(id) != null ? Translations.PromptViewPerk(GetPerk(id)) : string.Empty;
        public string GetSelectLinkPrompt(string sourceID, string targetID) => string.Empty;

        public IEnumerable<string> GetCards() =>
            PerkManager.GetAllPerks().Select(p => p.FullID);

        public IEnumerable<KeyValuePair<string, string>> GetLinks() =>
            PerkManager.GetAllPerks()
            .Where(p => p.Prereq != null)
            .Select(p => new KeyValuePair<string, string>(p.FullID, p.Prereq.FullID));

        public string GetInitialFocusedCard() => string.Empty;

        public bool CanMarkCard(string id)
            => PerkManager.CanUnlockPerk(GetPerk(id));

        public bool AttemptMarkCard(string id)
            => PerkManager.UnlockPerk(GetPerk(id));

        public bool AttemptUnmarkCard(string id)
            => PerkManager.RefundPerk(GetPerk(id));

        public bool CanSelectCard(string id) => true;

        public bool AttemptSelectCard(string id) => PerkManager.ReadPerk(GetPerk(id)) || true;

        public bool AttemptDeselectCard(string id) => true;

        public bool CanSelectLink(string sourceID, string targetID) => false;

        public bool AttemptSelectLink(string sourceID, string targetID) => false;

        public bool AttemptDeselectLink(string sourceID, string targetID) => false;

        public string GetCardName(string id) => GetPerk(id).ToDisplayString(false);

        public Vector2 GetCardPosition(string id)
        {
            if (!positions.ContainsKey(id)) CalculatePositions();
            return positions[id];
        }

        public bool GetCardMarked(string id) => PerkManager.HasUnlockedPerk(GetPerk(id));

        public bool GetCardUnread(string id) => !PerkManager.HasReadPerk(GetPerk(id));

        public bool GetCardMoreToExplore(string id) => false;

        public Sprite GetCardPhoto(string id) => GetPerk(id).Icon;

        public float GetCardSize(string id) => 1;

        public Color GetCardColor(string id)
        {
            Color.RGBToHSV(GetPerk(id).Color, out var h, out var s, out var v);
            if (PerkManager.HasUnlockedPerk(GetPerk(id)))
                return Color.HSVToRGB(h, s, Mathf.Clamp01(v - 0.25f));
            else if (PerkManager.CanUnlockPerk(GetPerk(id)))
                return Color.HSVToRGB(h, s, Mathf.Clamp01(v - 0.5f));
            else
                return Color.HSVToRGB(h, s * 0.25f, Mathf.Clamp01(v - 0.5f));
        }

        public Color GetCardHighlightColor(string id)
        {

            Color.RGBToHSV(GetPerk(id).Color, out var h, out var s, out var v);
           if (!PerkManager.CanUnlockPerk(GetPerk(id)))
                return Color.HSVToRGB(h, s * 0.25f, Mathf.Clamp01(v - 0.25f));
            else
                return Color.HSVToRGB(h, s, v);

        }

        public IEnumerable<string> GetCardDescription(string id)
        {
            var perk = GetPerk(id);
            foreach (var buff in perk.Buffs)
                foreach (var effect in buff.GetEffects())
                    yield return effect.GetDescription();
        }

        public IEnumerable<string> GetLinkDescription(string sourceID, string targetID) => new List<string>();

        public bool GetCardIsRumor(string id) => false;

        public bool GetCardWasRumor(string id) => false;

        public bool GetCardIsRevealed(string id) => GetPerk(id).Prereq == null || PerkManager.HasUnlockedPerk(GetPerk(id).Prereq);

        public bool GetCardWasRevealed(string id) => PerkManager.HasSeenPerk(GetPerk(id));

        public bool GetLinkIsRevealed(string sourceID, string targetID) => true;

        public bool GetLinkWasRevealed(string sourceID, string targetID) => PerkManager.HasSeenPerk(GetPerk(sourceID)) && PerkManager.HasSeenPerk(GetPerk(targetID));

        public void OnCardRevealStateUpdated(string id, bool revealed, bool rumored) {
            if (revealed)
                PerkManager.SeePerk(GetPerk(id));
            else
                PerkManager.UnseePerk(GetPerk(id));
        }

        public void OnLinkRevealStateUpdated(string sourceID, string targetID, bool revealed) { }
    }
}
