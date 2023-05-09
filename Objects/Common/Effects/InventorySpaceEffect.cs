using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class InventorySpaceEffect : BuffEffect<InventorySpaceEffect, InventorySpaceEffectData>
    {
        public int Amount;

        public override void Load(InventorySpaceEffectData data, string modID)
        {
            base.Load(data, modID);
            Amount = data.amount;
        }

        public override bool IsInstant() => true;

        public override string GetDescription() => Translations.EffectDescriptionInventorySpace(Amount);
    }
}
