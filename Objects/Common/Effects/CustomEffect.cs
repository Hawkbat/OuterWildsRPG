using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Effects
{
    public class CustomEffect : BuffEffect<CustomEffect, CustomEffectData>
    {
        public string ID;
        public string Description;

        public override void Load(CustomEffectData data, string modID)
        {
            base.Load(data, modID);
            ID = data.id;
            Description = data.description;
            TranslationUtils.RegisterGeneral(data.id, data.description);
        }

        public override bool IsInstant() => false;
        public override string GetDescription() => TranslationUtils.GetGeneral(ID);
    }
}
