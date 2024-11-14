using Newtonsoft.Json.Linq;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Perks;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG
{
    public static class Translations
    {
        public static string EffectDescriptionFogDensity(float amount) => Translate(nameof(EffectDescriptionFogDensity), PercentageModifier(1f / amount));
        public static string EffectDescriptionGiveDrop(Drop drop, int amount) => Translate(nameof(EffectDescriptionGiveDrop), drop, amount);
        public static string EffectDescriptionHazardDamage(float add, float multiply, HazardVolume.HazardType hazardType) => Translate(nameof(EffectDescriptionHazardDamage), PercentageModifier(add), PercentageModifier(multiply - 1f), Enum(hazardType));
        public static string EffectDescriptionHeal(float amount) => Translate(nameof(EffectDescriptionHeal), Percentage(amount));
        public static string EffectDescriptionHoldBreath(float amount) => Translate(nameof(EffectDescriptionHoldBreath), Modifier(amount));
        public static string EffectDescriptionInventorySpace(int amount) => Translate(nameof(EffectDescriptionInventorySpace), Modifier(amount));
        public static string EffectDescriptionJetpackBoostDuration(float add, float multiply) => Translate(nameof(EffectDescriptionJetpackBoostDuration), Modifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionJetpackBoostRecharge(float add, float multiply) => Translate(nameof(EffectDescriptionJetpackBoostRecharge), Modifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionJetpackBoostThrust(float add, float multiply) => Translate(nameof(EffectDescriptionJetpackBoostThrust), Modifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionJetpackThrust(float add, float multiply) => Translate(nameof(EffectDescriptionJetpackThrust), Modifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionJumpSpeed(float amount) => Translate(nameof(EffectDescriptionJumpSpeed), PercentageModifier(amount - 1f));
        public static string EffectDescriptionMaxHealth(float add, float multiply) => Translate(nameof(EffectDescriptionMaxHealth), PercentageModifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionMaxOxygen(float add, float multiply) => Translate(nameof(EffectDescriptionMaxOxygen), PercentageModifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionMaxFuel(float add, float multiply) => Translate(nameof(EffectDescriptionMaxFuel), PercentageModifier(add), PercentageModifier(multiply - 1f));
        public static string EffectDescriptionMoveSpeed(float amount) => Translate(nameof(EffectDescriptionMoveSpeed), PercentageModifier(amount - 1f));
        public static string EffectDescriptionStrangeFlame() => Translate(nameof(EffectDescriptionStrangeFlame));
        public static string EffectDescriptionTranslationSpeed(float amount) => Translate(nameof(EffectDescriptionTranslationSpeed), PercentageModifier(amount - 1f));
        public static string EffectDescriptionTravelMusic() => Translate(nameof(EffectDescriptionTravelMusic));
        public static string NotificationQuestStart(Quest quest) => Translate(nameof(NotificationQuestStart), quest);
        public static string NotificationQuestComplete(Quest quest) => Translate(nameof(NotificationQuestComplete), quest);
        public static string NotificationQuestStepProgress(QuestStep step, int progress, int total) => Translate(nameof(NotificationQuestStepProgress), step, progress, total);
        public static string NotificationQuestStepComplete(QuestStep step) => Translate(nameof(NotificationQuestStepComplete), step);
        public static string NotificationLevelUp(int level) => Translate(nameof(NotificationLevelUp), level);
        public static string NotificationXPGain(int xp, string reason) => Translate(nameof(NotificationXPGain), xp, reason);
        public static string NotificationUnspentPerkPoints(int points) => Translate(nameof(NotificationUnspentPerkPoints), points);
        public static string NotificationPickUpDrop(Drop drop) => Translate(nameof(NotificationPickUpDrop), drop);
        public static string NotificationPickUpDropFailed(Drop drop) => Translate(nameof(NotificationPickUpDropFailed), drop);
        public static string NotificationRemoveDrop(Drop drop) => Translate(nameof(NotificationRemoveDrop), drop);
        public static string NotificationEquipDrop(Drop drop) => Translate(nameof(NotificationEquipDrop), drop);
        public static string NotificationUnequipDrop(Drop drop) => Translate(nameof(NotificationUnequipDrop), drop);
        public static string QuestStepProgress(QuestStep step, int progress, int total) => Translate(nameof(QuestStepProgress), step, progress, total);
        public static string QuestStepOptional(QuestStep step) => Translate(nameof(QuestStepOptional), step);
        public static string PromptDropPickup(Drop drop) => Translate(nameof(PromptDropPickup), drop);
        public static string PromptEquipDrop(Drop drop) => Translate(nameof(PromptEquipDrop), drop);
        public static string PromptUnequipDrop(Drop drop) => Translate(nameof(PromptUnequipDrop), drop);
        public static string PromptConsumeDrop(Drop drop) => Translate(nameof(PromptConsumeDrop), drop);
        public static string PromptMoveFromInventoryToHotbar(Drop drop) => Translate(nameof(PromptMoveFromInventoryToHotbar), drop);
        public static string PromptMoveFromHotbarToInventory(Drop drop) => Translate(nameof(PromptMoveFromHotbarToInventory), drop);
        public static string PromptViewDrop(Drop drop) => Translate(nameof(PromptViewDrop), drop);
        public static string PromptUnlockPerk(Perk perk, int cost, int points) => Translate(nameof(PromptUnlockPerk), perk, cost, points);
        public static string PromptRefundPerk(Perk perk, int cost) => Translate(nameof(PromptRefundPerk), perk, cost);
        public static string PromptViewPerk(Perk perk) => Translate(nameof(PromptViewPerk), perk);
        public static string PromptExplore(string location) => Translate(nameof(PromptExplore), location);
        public static string XPGainMainQuest() => Translate(nameof(XPGainMainQuest));
        public static string XPGainSideQuest() => Translate(nameof(XPGainSideQuest));
        public static string XPGainMiscQuest() => Translate(nameof(XPGainMiscQuest));
        public static string XPGainQuest() => Translate(nameof(XPGainQuest));
        public static string ShopScreenListBuy(int currency) => Translate(nameof(ShopScreenListBuy), CurrencyLong(currency));
        public static string ShopScreenListSell(int currency) => Translate(nameof(ShopScreenListSell), CurrencyLong(currency));
        public static string ShopScreenListBuyBack(int currency) => Translate(nameof(ShopScreenListBuyBack), CurrencyLong(currency));
        public static string ShopScreenConfirmBuy(Drop drop, int cost) => Translate(nameof(ShopScreenConfirmBuy), drop, CurrencyLong(cost));
        public static string ShopScreenConfirmSell(Drop drop, int cost) => Translate(nameof(ShopScreenConfirmSell), drop, CurrencyLong(cost));
        public static string ShopScreenConfirmBuyBack(Drop drop, int cost) => Translate(nameof(ShopScreenConfirmBuyBack), drop, CurrencyLong(cost));
        public static string ShopScreenOutOfStock() => Translate(nameof(ShopScreenOutOfStock));
        public static string ShopScreenInsufficientFunds() => Translate(nameof(ShopScreenInsufficientFunds));
        public static string ShopScreenListOptionBack() => Translate(nameof(ShopScreenListOptionBack));
        public static string ShopScreenListOptionItem(Drop drop, int amount, int cost) => Translate(nameof(ShopScreenListOptionItem), drop, amount, CurrencyShort(cost));
        public static string ShopScreenConfirmOptionConfirm() => Translate(nameof(ShopScreenConfirmOptionConfirm));
        public static string ShopScreenConfirmOptionCancel() => Translate(nameof(ShopScreenConfirmOptionCancel));

        public static string Enum<T>(T value) where T : Enum
            => Translate($"{typeof(T).Name}.{EnumUtils.GetName(value)}");

        static string CurrencyLong(int amount)
            => Translate("CurrencyLong", amount);

        static string CurrencyShort(int amount)
            => Translate("CurrencyShort", amount);

        static string Modifier(float amount)
            => $"{(amount < 0 ? "" : "+")}{amount}";

        static string Modifier(int amount)
            => $"{(amount < 0 ? "" : "+")}{amount}";

        static string Percentage(float amount)
            => $"{Mathf.Round(amount * 100f)}%";

        static string PercentageModifier(float amount)
            => $"{(amount < 0f ? "" : "+")}{Mathf.Round(amount * 100f)}%";

        static string Translate(string key, params object[] args)
            => string.Format(TranslationUtils.GetGeneral(key) ?? key, args.Select(a => a switch
            {
                IDisplayable displayable => displayable.ToDisplayString(),
                Enum enumValue => TranslationUtils.GetGeneral($"{enumValue.GetType().Name}.{EnumUtils.GetName(enumValue.GetType(), enumValue)}"),
                _ => a,
            }).ToArray()).Trim();
    }
}
