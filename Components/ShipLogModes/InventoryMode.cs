using OuterWildsRPG.Components.Graph;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.ShipLogModes
{
    public class InventoryMode : GraphMode, IGraphProvider
    {
        const int COLUMNS = 10;
        static readonly Vector2 GRID_SPACING = new(130f, -170f);
        static readonly Vector2 EQUIPMENT_SPACING = GRID_SPACING * 1.5f;
        static readonly Vector2 EQUIPMENT_OFFSET = new Vector2(-3.5f, 0f) * EQUIPMENT_SPACING;
        static readonly Vector2 HOTBAR_SPACING = GRID_SPACING * 1.25f;
        static readonly Vector2 HOTBAR_OFFSET = new Vector2(0, -2f) * HOTBAR_SPACING;

        readonly Dictionary<string, EquipSlot> equipSlotsById = new();
        readonly Dictionary<EquipSlot, string> idsByEquipSlot = new();
        readonly Dictionary<string, int> inventoryIndexesById = new();
        readonly Dictionary<int, string> idsByinventoryIndex = new();
        readonly Dictionary<string, int> hotbarIndexesById = new();
        readonly Dictionary<int, string> idsByHotbarIndex = new();

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            foreach (var value in EnumUtils.GetValues<EquipSlot>())
            {
                if (value == EquipSlot.None || value == EquipSlot.Item) continue;
                var key = $"EQUIP_{value.GetName()}";
                equipSlotsById[key] = value;
                idsByEquipSlot[value] = key;
            }

            var count = Mathf.Max(DropManager.GetTotalInventoryCapacity(), DropManager.GetInventoryDrops().Count());
            for (var index = 0; index < count; index++)
            {
                var key = $"INV_{index}";
                inventoryIndexesById[key] = index;
                idsByinventoryIndex[index] = key;
            }

            for (var index = 0; index < DropManager.GetTotalHotbarCapacity(); index++)
            {
                var key = $"HOTBAR_{index}";
                hotbarIndexesById[key] = index;
                idsByHotbarIndex[index] = key;
            }

            base.Initialize(centerPromptList, upperRightPromptList, oneShotSource);
        }

        EquipSlot GetEquipSlotByID(string id)
        {
            if (equipSlotsById.TryGetValue(id, out var slot)) return slot;
            if (hotbarIndexesById.ContainsKey(id)) return EquipSlot.Item;
            return EquipSlot.None;
        }

        int GetIndexByID(string id)
        {
            if (inventoryIndexesById.TryGetValue(id, out var invIndex)) return invIndex;
            if (hotbarIndexesById.TryGetValue(id, out var hotbarIndex)) return hotbarIndex;
            return -1;
        }

        bool IsEquipSlot(string id) => GetEquipSlotByID(id) != EquipSlot.None && GetEquipSlotByID(id) != EquipSlot.Item;

        bool IsHotbarSlot(string id) => hotbarIndexesById.ContainsKey(id);

        Drop GetDrop(string id)
        {
            var equipSlot = GetEquipSlotByID(id);
            if (equipSlot == EquipSlot.Item)
                return DropManager.GetHotbarDrop(GetIndexByID(id));
            if (equipSlot != EquipSlot.None)
                return DropManager.GetEquippedDrop(equipSlot);
            return DropManager.GetInventoryDrop(GetIndexByID(id));
        }

        public bool GetNavigationSnapping() => true;
        public string GetMarkCardPrompt(string id)
        {
            var drop = GetDrop(id);
            if (drop == null) return string.Empty;
            if (IsHotbarSlot(id)) return Translations.PromptMoveFromHotbarToInventory(drop);
            if (IsEquipSlot(id)) return Translations.PromptUnequipDrop(drop);
            if (drop.Consumable) return Translations.PromptConsumeDrop(drop);
            if (drop.EquipSlot == EquipSlot.Item) return Translations.PromptMoveFromInventoryToHotbar(drop);
            if (drop.EquipSlot != EquipSlot.None) return Translations.PromptEquipDrop(drop);
            return string.Empty;
        }
        public string GetUnmarkCardPrompt(string id) => string.Empty;
        public string GetSelectCardPrompt(string id) => GetDrop(id) != null ? Translations.PromptViewDrop(GetDrop(id)) : string.Empty;
        public string GetSelectLinkPrompt(string sourceID, string targetID) => string.Empty;
        public IEnumerable<string> GetCards() =>
            idsByEquipSlot.Values.Concat(idsByinventoryIndex.Values).Concat(idsByHotbarIndex.Values);
        public IEnumerable<KeyValuePair<string, string>> GetLinks() => new List<KeyValuePair<string, string>>();
        public string GetInitialFocusedCard() => string.Empty;
        public bool CanMarkCard(string id)
            => GetDrop(id) != null && (GetDrop(id).Consumable || GetDrop(id).EquipSlot != EquipSlot.None);
        public bool AttemptMarkCard(string id)
        {
            var drop = GetDrop(id);
            if (IsEquipSlot(id))
                return DropManager.UnequipDrop(drop, drop.EquipSlot);
            if (drop.Consumable)
            {
                if (DropManager.ConsumeDrop(drop))
                {
                    Locator.GetPlayerAudioController().PlayOneShotInternal(drop.ConsumeAudioType);
                    return true;
                }
            }
            if (drop.EquipSlot == EquipSlot.Item)
            {
                if (IsHotbarSlot(id))
                    return DropManager.MoveDropFromHotbarToInventory(drop);
                else
                    return DropManager.MoveDropFromInventoryToHotbar(drop);
            }
            if (drop.EquipSlot != EquipSlot.None)
                return DropManager.EquipDrop(drop, drop.EquipSlot);
            return false;
        }
        public bool AttemptUnmarkCard(string id) => false;
        public bool CanSelectCard(string id) => GetDrop(id) != null;
        public bool AttemptSelectCard(string id)
        {
            var drop = GetDrop(id);
            if (drop == null) return false;
            DropManager.ReadDrop(drop);
            return true;
        }
        public bool AttemptDeselectCard(string id) => true;
        public bool CanSelectLink(string sourceID, string targetID) => false;
        public bool AttemptSelectLink(string sourceID, string targetID) => false;
        public bool AttemptDeselectLink(string sourceID, string targetID) => false;
        public string GetCardName(string id) =>
            GetDrop(id)?.ToDisplayString(false) ?? (IsEquipSlot(id) ? Translations.Enum(GetEquipSlotByID(id)) : string.Empty);
        public Vector2 GetCardPosition(string id)
        {
            var slot = GetEquipSlotByID(id);
            if (slot == EquipSlot.None)
            {
                var index = GetIndexByID(id);
                var pos = new Vector2(index % COLUMNS, index / COLUMNS);
                return pos * GRID_SPACING;
            }
            else if (slot == EquipSlot.Item)
            {
                var index = GetIndexByID(id);
                var pos = new Vector2(index % COLUMNS, index / COLUMNS);
                return pos * HOTBAR_SPACING + HOTBAR_OFFSET;
            }
            else
            {
                var pos = slot switch
                {
                    EquipSlot.Helmet => new Vector2(0f, 0f),
                    EquipSlot.Suit => new Vector2(0f, 1f),
                    EquipSlot.Jetpack => new Vector2(-1f, 0.5f),
                    EquipSlot.Scout => new Vector2(-1f, 1.5f),
                    EquipSlot.Launcher => new Vector2(-1f, 2.5f),
                    EquipSlot.Signalscope => new Vector2(1f, 1.5f),
                    EquipSlot.Flashlight => new Vector2(1f, 0.5f),
                    EquipSlot.Translator => new Vector2(1f, 2.5f),
                    EquipSlot.Radio => new Vector2(0f, 2f),
                    EquipSlot.Stick => new Vector2(0f, 3f),
                    _ => Vector2.zero,
                };
                return pos * EQUIPMENT_SPACING + EQUIPMENT_OFFSET;
            }
        }
        public bool GetCardMarked(string id) => false;
        public bool GetCardUnread(string id) => GetDrop(id) != null && !DropManager.HasReadDrop(GetDrop(id));
        public bool GetCardMoreToExplore(string id) => false;
        public Sprite GetCardPhoto(string id) => GetDrop(id)?.Icon;
        public float GetCardSize(string id) => GetEquipSlotByID(id) switch
        {
            EquipSlot.None => 1f,
            EquipSlot.Item => 1.25f,
            _ => 1.5f,
        };
        public Color GetCardColor(string id)
        {
            var color = GetCardHighlightColor(id);
            Color.RGBToHSV(color, out var h, out var s, out var v);
            return Color.HSVToRGB(h, s, 0.6f * v);
        }
        public Color GetCardHighlightColor(string id)
        {
            var drop = GetDrop(id);
            if (drop == null) return Assets.HUDBackColor;
            return Assets.GetRarityColor(drop.Rarity);
        }
        public IEnumerable<string> GetCardDescription(string id)
        {
            var list = new List<string>();
            var drop = GetDrop(id);
            if (drop == null) return list;
            list.Add(drop.Description);
            foreach (var buff in drop.Buffs)
                foreach (var effect in buff.GetEffects())
                    list.Add(effect.GetDescription());
            return list;
        }
        public IEnumerable<string> GetLinkDescription(string sourceID, string targetID) => new List<string>();
        public bool GetCardIsRumor(string id) => GetDrop(id) != null && !GetDrop(id).Icon;
        public bool GetCardWasRumor(string id) => GetDrop(id) != null && !DropManager.HasSeenDrop(GetDrop(id)) || GetCardIsRumor(id);
        public bool GetCardIsRevealed(string id) => true;
        public bool GetCardWasRevealed(string id) => true;
        public bool GetLinkIsRevealed(string sourceID, string targetID) => false;
        public bool GetLinkWasRevealed(string sourceID, string targetID) => false;
        public void OnCardRevealStateUpdated(string id, bool revealed, bool rumored)
        {
            if (revealed)
            {
                var drop = GetDrop(id);
                if (drop != null) DropManager.SeeDrop(drop);
            }
        }
        public void OnLinkRevealStateUpdated(string sourceID, string targetID, bool revealed) { }
    }
}
