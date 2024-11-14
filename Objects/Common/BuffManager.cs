using OuterWildsRPG.Components;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common.Effects;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common
{
    public static class BuffManager
    {
        public static IEnumerable<Buff> GetAllActiveBuffs()
        {
            foreach (var perk in PerkManager.GetUnlockedPerks())
                foreach (var buff in perk.Buffs)
                    yield return buff;
            foreach (var drop in DropManager.GetEquippedDrops())
                foreach (var buff in drop.Buffs)
                    yield return buff;
            foreach (var drop in DropManager.GetConsumedDrops())
                foreach (var buff in drop.Buffs)
                    yield return buff;
        }

        public static void ApplyInstantEffects(Buff buff)
        {
            foreach (var effect in buff.GetEffects().Where(e => e.IsInstant()))
            {
                if (effect is HealEffect heal)
                {
                    var playerResources = Locator.GetPlayerBody().GetComponentInChildren<PlayerResources>();
                    playerResources._currentHealth = Mathf.Min(100f, playerResources._currentHealth + 100f * heal.Amount);
                }
                if (effect is GiveDropEffect giveDrop)
                {
                    for (int i = 0; i < giveDrop.Amount; i++)
                        DropManager.ReceiveDrop(giveDrop.Drop);
                }
            }
        }

        public static float ModifyHazardDamage(HazardVolume.HazardType type, float damage)
            => damage * GetStatMultiplier<HazardDamageEffect>(e => e.Type == HazardVolume.HazardType.NONE || e.Type == type);

        public static float GetStatMultiplier<T>() where T : IStatBuffEffect
            => GetStatMultiplier(GetAllActiveBuffs()
                .SelectMany(b => b.GetEffects())
                .OfType<T>()
                .Where(e => e != null));

        public static float GetStatMultiplier<T>(Func<T, bool> filter) where T : IStatBuffEffect
            => GetStatMultiplier(GetAllActiveBuffs()
                .SelectMany(b => b.GetEffects())
                .OfType<T>()
                .Where(e => e != null)
                .Where(filter));

        public static float GetStatMultiplier<T>(IEnumerable<T> activeEffects) where T : IStatBuffEffect
        {
            var addition = 0f;
            foreach (var effect in activeEffects)
                addition += effect.Add;

            var multiplier = 1f;
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;

            return (1f + addition) * multiplier;
        }

        public static TValue GetEffectValue<TEffect, TValue>(Func<TEffect, TValue> map, TValue defaultValue = default) where TEffect : IBuffEffect
        {
            var activeEffects = GetAllActiveBuffs()
                .SelectMany(b => b.GetEffects())
                .OfType<TEffect>()
                .Where(e => e != null);

            var result = defaultValue;

            foreach (var effect in activeEffects)
            {
                var value = map(effect);
                if (!EqualityComparer<TValue>.Default.Equals(value, defaultValue))
                {
                    result = value;
                }
            }

            return result;
        }

        static void UpdateResources()
        {
            PlayerResources._maxHealth = 100f * GetStatMultiplier<MaxHealthEffect>();
            PlayerResources._maxOxygen = 450f * GetStatMultiplier<MaxOxygenEffect>();
            PlayerResources._maxFuel = 100f * GetStatMultiplier<MaxFuelEffect>();
            PlayerResources._suffocationDuration = GetStatMultiplier<HoldBreathEffect>();
        }

        static void UpdateMovementSpeed()
        {
            var multiplier = GetStatMultiplier<MoveSpeedEffect>();

            var player = Locator.GetPlayerController();
            player._runSpeed = 6f * multiplier;
            player._walkSpeed = 3f * multiplier;
            player._strafeSpeed = 4f * multiplier;
            player._acceleration = 0.5f * multiplier;
            player._airSpeed = 3f * multiplier;
            player._airAcceleration = 0.2f * multiplier;
        }

        static void UpdateJumpSpeed()
        {
            var multiplier = GetStatMultiplier<JumpSpeedEffect>();

            var player = Locator.GetPlayerController();
            player._minJumpSpeed = 3f * multiplier;
            player._maxJumpSpeed = 6f * multiplier;
        }

        static void UpdateJetpack()
        {
            var jetpack = Locator.GetPlayerController()._jetpackModel as JetpackThrusterModel;
            jetpack._maxTranslationalThrust = 10f * GetStatMultiplier<JetpackThrustEffect>();
            jetpack._boostThrust = 10f * GetStatMultiplier<JetpackBoostThrustEffect>();
            jetpack._boostSeconds = 1f * GetStatMultiplier<JetpackBoostDurationEffect>();
            jetpack._chargeSecondsGround = 1f * GetStatMultiplier<JetpackBoostRechargeEffect>();
            jetpack._chargeSecondsAir = 8f * GetStatMultiplier<JetpackBoostRechargeEffect>();

            var useGreenFlame = GetEffectValue<StrangeFlameEffect, bool>(e => true, false);
            Locator.GetPlayerController()._playerResources._jetpackFlameColorSwapper.SetFlameColor(useGreenFlame);
        }

        static void UpdateTravelMusic()
        {
            var currentTravelMusic = Locator.GetGlobalMusicController()._travelSource.audioLibraryClip;
            var travelMusic = GetEffectValue<TravelMusicEffect, AudioType>(e => e.AudioType, AudioType.Travel_Theme);

            if (travelMusic != currentTravelMusic)
            {
                Locator.GetGlobalMusicController()._travelSource.AssignAudioLibraryClip(travelMusic);
            }
        }

        public static void SetUp()
        {
            foreach (var planetaryFogController in GameObject.FindObjectsOfType<PlanetaryFogController>())
            {
                planetaryFogController.gameObject.AddComponent<FogBuffController>();
            }
        }

        public static void Update()
        {
            UpdateResources();
            UpdateMovementSpeed();
            UpdateJumpSpeed();
            UpdateJetpack();
            UpdateTravelMusic();
        }

        public static void CleanUp()
        {

        }
    }
}
