using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Perks
{
    public class Perk : Entity<Perk, PerkData>
    {
        public int Level;
        public Sprite Icon;
        public Color Color;
        public Perk Prereq;
        public List<Buff> Buffs = new();

        public List<Perk> Dependents = new();

        public override void Load(PerkData data, string modID)
        {
            base.Load(data, modID);
            Level = data.level;
            Icon = Assets.GetIconSprite(data.iconPath, modID);
            Color = data.color.ToColor();
            Prereq = null;
            Buffs = data.buffs.Select(b => Buff.LoadNew(b, modID)).ToList();
            foreach (var b in Buffs)
                b.Entity = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            foreach (var b in Buffs) b.Resolve();

            var prereq = GetRawData().prereq;
            if (!string.IsNullOrEmpty(prereq))
            {
                Prereq = PerkManager.GetPerk(prereq, ModID);
                if (Prereq == null) OuterWildsRPG.LogError($"Could not locate specified prerequisite {prereq} for perk {this}");
                else Prereq.Dependents.Add(this);
            }
        }

        public int GetPerkCost(bool includeDependents)
        {
            if (!includeDependents) return 1;
            return GetPerkCost(false) + Dependents.Sum(p => p.GetPerkCost(true));
        }
    }
}
