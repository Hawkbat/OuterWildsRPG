using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.Objects.Common;

namespace OuterWildsRPG.Objects.Drops
{
    [Description("An inventory item (not to be confused with vanilla items) that can be picked up and interacted with via the ship's inventory menu.")]
    public class DropData : EntityData
    {
        [Description("A short description to display in the inventory for this drop.")]
        public string description;
        [Description("The 'quality' rating of this drop, which generally reflects its value.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public DropRarity rarity;
        [Description($"Which equipment slot this drop can be equipped to, if any. Mutually exclusive with {nameof(consumable)}.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public EquipSlot equipSlot;
        [Description($"Whether this item can be consumed, removing it from the player's inventory. Mutually exclusive with {nameof(equipSlot)}.")]
        public bool consumable;
        [Description($"How long the effects of this drop's buffs will last after being consumed, in seconds.")]
        public float duration;
        [Description("A file path (relative to your mod's folder) to an image file to use for this drop's icon. Can be shared with other drops.")]
        public string iconPath;
        [Description("An Outer Wilds AudioType value that will be played when the drop is picked up. See https://nh.outerwildsmods.com/reference/audio_enum.html")]
        public string pickUpAudioType;
        [Description("An Outer Wilds AudioType value that will be played when the drop is consumed. See https://nh.outerwildsmods.com/reference/audio_enum.html")]
        public string consumeAudioType;
        [Description("Effects that will apply while this drop is equipped.")]
        public List<BuffData> buffs = new();
        [Description("Locations in the solar system to place pickups for this drop.")]
        public List<DropLocationData> locations = new();
    }
}
