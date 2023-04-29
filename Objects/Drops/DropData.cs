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
    public class DropData : EntityData
    {
        [Description("A short description to display in the inventory for this drop.")]
        public string description;
        [Description("The 'quality' rating of this drop, which generally reflects its value.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public DropRarity rarity;
        [Description("Which equipment slot this drop can be equipped to, if any.")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public EquipSlot equipSlot;
        [Description("Effects that will apply while this drop is equipped.")]
        public List<BuffData> buffs = new();
        [Description("Locations in the solar system to place pickups for this drop.")]
        public List<DropLocationData> locations = new();
    }
}
