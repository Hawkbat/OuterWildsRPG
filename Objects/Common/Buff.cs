using OuterWildsRPG.Objects.Common.Effects;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common
{
    public class Buff : EntityLike<Buff, BuffData>
    {
        public IEntity Entity;
        public MoveSpeedEffect MoveSpeed;
        public JumpSpeedEffect JumpSpeed;
        public HazardDamageEffect HazardDamage;
        public TranslationSpeedEffect TranslationSpeed;
        public TravelMusicEffect TravelMusic;
        public InventorySpaceEffect InventorySpace;
        public HealEffect Heal;
        public GiveDropEffect GiveDrop;
        public CustomEffect Custom;

        public override void Load(BuffData data, string modID)
        {
            MoveSpeed = MoveSpeedEffect.LoadNew(data.moveSpeed, modID);
            JumpSpeed = JumpSpeedEffect.LoadNew(data.jumpSpeed, modID);
            HazardDamage = HazardDamageEffect.LoadNew(data.hazardDamage, modID);
            TranslationSpeed = TranslationSpeedEffect.LoadNew(data.translationSpeed, modID);
            TravelMusic = TravelMusicEffect.LoadNew(data.travelMusic, modID);
            InventorySpace = InventorySpaceEffect.LoadNew(data.inventorySpace, modID);
            Heal = HealEffect.LoadNew(data.heal, modID);
            GiveDrop = GiveDropEffect.LoadNew(data.giveDrop, modID);
            Custom = CustomEffect.LoadNew(data.custom, modID);
            foreach (var effect in GetEffects())
                effect.Buff = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            foreach (var effect in GetEffects())
                effect.Resolve();
        }

        public IEnumerable<IBuffEffect> GetEffects()
        {
            if (MoveSpeed != null) yield return MoveSpeed;
            if (JumpSpeed != null) yield return JumpSpeed;
            if (HazardDamage != null) yield return HazardDamage;
            if (TranslationSpeed != null) yield return TranslationSpeed;
            if (TravelMusic != null) yield return TravelMusic;
            if (InventorySpace != null) yield return InventorySpace;
            if (Heal != null) yield return Heal;
            if (GiveDrop != null) yield return GiveDrop;
            if (Custom != null) yield return Custom;
        }
    }
}
