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
        {
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.HazardDamage)
                .Where(e => e != null)
                .Where(e => e.Type == HazardVolume.HazardType.NONE || e.Type == type);

            var addition = 0f;
            foreach (var effect in activeEffects)
                addition += effect.Add;

            var multiplier = 1f;
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;

            return (damage + damage * addition) * multiplier;
        }

        public static float GetTranslationSpeedMultiplier()
        {
            var multiplier = 1f;
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.TranslationSpeed)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;
            return multiplier;
        }

        public static float GetSuffocationTimeModifier()
        {
            var modifier = 0f;
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.HoldBreath)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                modifier += effect.Seconds;
            return modifier;
        }

        public static float GetMaxHealthMultiplier()
        {
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.MaxHealth)
                .Where(e => e != null);

            var addition = 0f;
            foreach (var effect in activeEffects)
                addition += effect.Add;

            var multiplier = 1f;
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;

            return (1f + addition) * multiplier;
        }

        public static int GetInventoryCapacity()
        {
            var capacity = 10;
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.InventorySpace)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                capacity += effect.Amount;
            return capacity;
        }

        public static float GetFogDensityMultiplier()
        {
            var multiplier = 1f;
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.FogDensity)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;
            return multiplier;
        }

        static void UpdateMovementSpeed()
        {
            var multiplier = 1f;

            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.MoveSpeed)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;

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
            var multiplier = 1f;

            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.JumpSpeed)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                multiplier *= effect.Multiply;

            var player = Locator.GetPlayerController();
            player._minJumpSpeed = 3f * multiplier;
            player._maxJumpSpeed = 6f * multiplier;
        }

        static void UpdateTravelMusic()
        {
            var currentTravelMusic = Locator.GetGlobalMusicController()._travelSource.audioLibraryClip;
            var travelMusic = AudioType.Travel_Theme;

            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.TravelMusic)
                .Where(e => e != null);
            foreach (var effect in activeEffects)
                travelMusic = effect.AudioType;

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
            UpdateMovementSpeed();
            UpdateJumpSpeed();
            UpdateTravelMusic();
        }

        public static void CleanUp()
        {

        }
    }
}
