using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
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
            foreach (var drop in DropManager.GetEquippedDrops())
                foreach (var buff in drop.Buffs)
                    yield return buff;

        }

        public static float ModifyHazardDamage(HazardVolume.HazardType type, float damage)
        {
            var activeEffects = GetAllActiveBuffs()
                .Select(b => b.HazardDamage)
                .Where(e => e != null)
                .Where(e => e.Type == HazardVolume.HazardType.NONE || e.Type == type);

            var addition = 0f;
            foreach (var buff in activeEffects)
                addition += buff.Add;

            var multiplier = 1f;
            foreach (var buff in activeEffects)
                multiplier *= buff.Multiply;

            return (damage + addition) * multiplier;
        }

        public static float GetTranslationSpeedMultiplier()
        {
            var multiplier = 1f;
            var activeEffects = GetAllActiveBuffs().Select(b => b.TranslationSpeed).Where(e => e != null);
            foreach (var buff in activeEffects)
                multiplier *= buff.Multiply;
            return multiplier;
        }

        public static void UpdateTravelMusic()
        {
            var travelMusic = AudioType.Travel_Theme;

            var equippedRadio = DropManager.GetEquippedDrop(EquipSlot.Radio);
            if (equippedRadio != null)
            {
                var activeEffects = equippedRadio.Buffs.Select(b => b.TravelMusic).Where(b => b != null);
                if (activeEffects.Any())
                {
                    travelMusic = activeEffects.First().AudioType;
                }
            }

            Locator.GetGlobalMusicController()._travelSource.AssignAudioLibraryClip(travelMusic);
        }
    }
}
