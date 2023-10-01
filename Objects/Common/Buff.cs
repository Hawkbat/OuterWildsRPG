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
        public CustomEffect Custom;
        public FogDensityEffect FogDensity;
        public GiveDropEffect GiveDrop;
        public HazardDamageEffect HazardDamage;
        public HealEffect Heal;
        public HoldBreathEffect HoldBreath;
        public InventorySpaceEffect InventorySpace;
        public JumpSpeedEffect JumpSpeed;
        public MaxHealthEffect MaxHealth;
        public MoveSpeedEffect MoveSpeed;
        public TranslationSpeedEffect TranslationSpeed;
        public TravelMusicEffect TravelMusic;

        public override void Load(BuffData data, string modID)
        {
            Custom = CustomEffect.LoadNew(data.custom, modID);
            FogDensity = FogDensityEffect.LoadNew(data.fogDensity, modID);
            GiveDrop = GiveDropEffect.LoadNew(data.giveDrop, modID);
            HazardDamage = HazardDamageEffect.LoadNew(data.hazardDamage, modID);
            Heal = HealEffect.LoadNew(data.heal, modID);
            HoldBreath = HoldBreathEffect.LoadNew(data.holdBreath, modID);
            InventorySpace = InventorySpaceEffect.LoadNew(data.inventorySpace, modID);
            JumpSpeed = JumpSpeedEffect.LoadNew(data.jumpSpeed, modID);
            MaxHealth = MaxHealthEffect.LoadNew(data.maxHealth, modID);
            MoveSpeed = MoveSpeedEffect.LoadNew(data.moveSpeed, modID);
            TranslationSpeed = TranslationSpeedEffect.LoadNew(data.translationSpeed, modID);
            TravelMusic = TravelMusicEffect.LoadNew(data.travelMusic, modID);
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
            if (Custom != null) yield return Custom;
            if (FogDensity != null) yield return FogDensity;
            if (GiveDrop != null) yield return GiveDrop;
            if (HazardDamage != null) yield return HazardDamage;
            if (Heal != null) yield return Heal;
            if (HoldBreath != null) yield return HoldBreath;
            if (InventorySpace != null) yield return InventorySpace;
            if (JumpSpeed != null) yield return JumpSpeed;
            if (MaxHealth != null) yield return MaxHealth;
            if (MoveSpeed != null) yield return MoveSpeed;
            if (TranslationSpeed != null) yield return TranslationSpeed;
            if (TravelMusic != null) yield return TravelMusic;
        }
    }
}
