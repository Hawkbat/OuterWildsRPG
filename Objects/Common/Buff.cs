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
        public JetpackBoostDurationEffect JetpackBoostDuration;
        public JetpackBoostRechargeEffect JetpackBoostRecharge;
        public JetpackBoostThrustEffect JetpackBoostThrust;
        public JetpackThrustEffect JetpackThrust;
        public JumpSpeedEffect JumpSpeed;
        public MaxHealthEffect MaxHealth;
        public MaxOxygenEffect MaxOxygen;
        public MaxFuelEffect MaxFuel;
        public MoveSpeedEffect MoveSpeed;
        public StrangeFlameEffect StrangeFlame;
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
            JetpackBoostDuration = JetpackBoostDurationEffect.LoadNew(data.jetpackBoostDuration, modID);
            JetpackBoostRecharge = JetpackBoostRechargeEffect.LoadNew(data.jetpackBoostRecharge, modID);
            JetpackBoostThrust = JetpackBoostThrustEffect.LoadNew(data.jetpackBoostThrust, modID);
            JetpackThrust = JetpackThrustEffect.LoadNew(data.jetpackThrust, modID);
            JumpSpeed = JumpSpeedEffect.LoadNew(data.jumpSpeed, modID);
            MaxHealth = MaxHealthEffect.LoadNew(data.maxHealth, modID);
            MaxOxygen = MaxOxygenEffect.LoadNew(data.maxOxygen, modID);
            MaxFuel = MaxFuelEffect.LoadNew(data.maxFuel, modID);
            MoveSpeed = MoveSpeedEffect.LoadNew(data.moveSpeed, modID);
            StrangeFlame = StrangeFlameEffect.LoadNew(data.strangeFlame, modID);
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
            if (JetpackBoostDuration != null) yield return JetpackBoostDuration;
            if (JetpackBoostRecharge != null) yield return JetpackBoostRecharge;
            if (JetpackBoostThrust != null) yield return JetpackBoostThrust;
            if (JetpackThrust != null) yield return JetpackThrust;
            if (JumpSpeed != null) yield return JumpSpeed;
            if (MaxHealth != null) yield return MaxHealth;
            if (MaxOxygen != null) yield return MaxOxygen;
            if (MaxFuel != null) yield return MaxFuel;
            if (StrangeFlame != null) yield return StrangeFlame;
            if (MoveSpeed != null) yield return MoveSpeed;
            if (TranslationSpeed != null) yield return TranslationSpeed;
            if (TravelMusic != null) yield return TravelMusic;
        }
    }
}
