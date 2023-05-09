using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Drops
{
    public class Drop : Entity<Drop, DropData>
    {
        public string Description;
        public DropRarity Rarity;
        public EquipSlot EquipSlot;
        public bool Consumable;
        public float Duration;
        public Sprite Icon;
        public AudioType PickUpAudioType;
        public AudioType ConsumeAudioType;
        public List<Buff> Buffs = new();
        public List<DropLocation> Locations = new();

        public override void Load(DropData data, string modID)
        {
            base.Load(data, modID);
            Description = data.description;
            EquipSlot = data.equipSlot;
            Consumable = data.consumable;
            Duration = data.duration;
            Icon = Assets.GetIconSprite(data.iconPath, modID);
            Rarity = data.rarity;
            PickUpAudioType = EnumUtils.Parse(data.pickUpAudioType, true, AudioType.ToolItemWarpCorePickUp);
            ConsumeAudioType = EnumUtils.Parse(data.consumeAudioType, true, AudioType.ToolMarshmallowEat);
            Buffs = data.buffs.Select(d => Buff.LoadNew(d, modID)).ToList();
            Locations = data.locations.Select(l => DropLocation.LoadNew(l, modID)).ToList();
            foreach (var b in Buffs)
                b.Entity = this;
            foreach (var l in Locations)
                l.Drop = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            foreach (var b in Buffs) b.Resolve();
            foreach (var l in Locations) l.Resolve();
        }

        public override string ToDisplayString(bool richText = true)
            => richText ? UnityUtils.RichTextColor(base.ToDisplayString(false), Assets.GetRarityColor(Rarity)) : base.ToDisplayString(richText);
    }
}
