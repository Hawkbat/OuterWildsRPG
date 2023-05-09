using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class GiveDropEffect : BuffEffect<GiveDropEffect, GiveDropEffectData>
    {
        public Drop Drop;
        public int Amount;
        
        public override void Load(GiveDropEffectData data, string modID)
        {
            base.Load(data, modID);
            Amount = data.amount;
        }

        public override void Resolve()
        {
            base.Resolve();
            var dropID = GetRawData().drop;
            Drop = DropManager.GetDrop(dropID, Buff.Entity.ModID);
            if (Drop == null) OuterWildsRPG.LogError($"Could not locate specified drop {dropID} for give drop effect on {Buff.Entity}");
        }

        public override bool IsInstant() => true;

        public override string GetDescription() => Translations.EffectDescriptionGiveDrop(Drop, Amount);
    }
}
