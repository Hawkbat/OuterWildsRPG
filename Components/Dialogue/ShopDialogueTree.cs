using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Objects.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.Dialogue
{
    public class ShopDialogueTree : CustomDialogueTree<ShopScreen, Drop>
    {
        Shop shop;
        ShopMode mode;
        ICustomDialogueTree previousTree;
        string previousNode;

        public Shop Shop => shop;
        public ShopMode Mode => mode;

        public void Init(Shop shop)
        {
            this.shop = shop;
            Init(new ShopDialogueTreeProvider(this), false);
        }

        public void EnterModal(ShopMode mode, ICustomDialogueTree previousTree, string previousNode)
        {
            this.mode = mode;
            this.previousTree = previousTree;
            this.previousNode = previousNode;
            previousTree.SwitchTo(this, previousNode);
        }

        public void ExitModal()
        {
            SwitchTo(previousTree, previousNode);
        }
    }

    public class ShopDialogueTreeProvider : ICustomDialogueTreeProvider<ShopScreen, Drop>
    {
        public Shop Shop => shopDialogueTree.Shop;
        public ShopMode Mode => shopDialogueTree.Mode;

        readonly ShopDialogueTree shopDialogueTree;

        Drop currentDrop;

        public ShopDialogueTreeProvider(ShopDialogueTree shopDialogueTree)
        {
            this.shopDialogueTree = shopDialogueTree;
        }

        public Transform GetAttentionPoint() => null;

        public Vector3 GetAttentionPointOffset() => Vector3.zero;

        public bool? GetFlashlightState() => null;

        public bool GetIsRecording() => false;

        public string GetInteractPrompt() => null;

        public string GetCharacterName() => Shop.ToDisplayString();

        public ShopScreen GetEntryNode(string context) => ShopScreen.List;

        public string GetNodeText(ShopScreen node, int page)
        {
            return node switch
            {
                ShopScreen.List => Mode switch
                {
                    ShopMode.Buy => Translations.ShopScreenListBuy(ShopManager.GetCurrencyAmount()),
                    ShopMode.Sell => Translations.ShopScreenListSell(ShopManager.GetCurrencyAmount()),
                    ShopMode.BuyBack => Translations.ShopScreenListBuyBack(ShopManager.GetCurrencyAmount()),
                    _ => "Unknown mode",
                },
                ShopScreen.Confirm => Mode switch
                {
                    ShopMode.Buy => Translations.ShopScreenConfirmBuy(currentDrop, ShopManager.GetBuyCost(Shop, currentDrop)),
                    ShopMode.Sell => Translations.ShopScreenConfirmSell(currentDrop, ShopManager.GetSellCost(Shop, currentDrop)),
                    ShopMode.BuyBack => Translations.ShopScreenConfirmBuyBack(currentDrop, ShopManager.GetBuyBackCost(Shop, currentDrop)),
                    _ => "Unknown mode",
                },
                ShopScreen.OutOfStock => Translations.ShopScreenOutOfStock(),
                ShopScreen.InsufficientFunds => Translations.ShopScreenInsufficientFunds(),
                _ => "Unknown screen",
            };
        }

        public bool GetNodeHasNextPage(ShopScreen node, int page) => false;

        public ShopScreen GetNodeTarget(ShopScreen node)
        {
            return node switch
            {
                ShopScreen.OutOfStock => ShopScreen.List,
                ShopScreen.InsufficientFunds => ShopScreen.List,
                _ => ShopScreen.None,
            };
        }

        public IEnumerable<Drop> GetNodeOptions(ShopScreen node)
        {
            return node switch
            {
                ShopScreen.List => Mode switch
                {
                    ShopMode.Buy => ShopManager.GetBuyableShopItems(Shop).Append(null),
                    ShopMode.Sell => ShopManager.GetSellableShopItems(Shop).Append(null),
                    ShopMode.BuyBack => ShopManager.GetBuyBackShopItems(Shop).Append(null),
                    _ => Enumerable.Empty<Drop>(),
                },
                ShopScreen.Confirm => new List<Drop>() { currentDrop, null },
                _ => Enumerable.Empty<Drop>(),
            };
        }

        public string GetOptionText(Drop option, ShopScreen node)
        {
            if (node == ShopScreen.List && option == null) return Translations.ShopScreenListOptionBack();
            return node switch
            {
                ShopScreen.List => Mode switch
                {
                    ShopMode.Buy => Translations.ShopScreenListOptionItem(option, ShopManager.GetBuyAmount(Shop, option), ShopManager.GetBuyCost(Shop, option)),
                    ShopMode.Sell => Translations.ShopScreenListOptionItem(option, ShopManager.GetSellAmount(Shop, option), ShopManager.GetSellCost(Shop, option)),
                    ShopMode.BuyBack => Translations.ShopScreenListOptionItem(option, ShopManager.GetBuyBackAmount(Shop, option), ShopManager.GetBuyBackCost(Shop, option)),
                    _ => "Unknown mode",
                },
                ShopScreen.Confirm => option != null ? Translations.ShopScreenConfirmOptionConfirm() : Translations.ShopScreenConfirmOptionCancel(),
                _ => "Unknown screen",
            };
        }

        public ShopScreen GetOptionTarget(Drop option, ShopScreen node)
        {
            if (node == ShopScreen.List && option == null) return ShopScreen.None;
            return node switch
            {
                ShopScreen.List => Mode switch {
                    ShopMode.Buy => ShopManager.GetBuyAmount(Shop, option) <= 0 ? ShopScreen.OutOfStock : ShopManager.GetBuyCost(Shop, option) > ShopManager.GetCurrencyAmount() ? ShopScreen.InsufficientFunds : ShopScreen.Confirm,
                    ShopMode.BuyBack => ShopManager.GetBuyBackAmount(Shop, option) <= 0 ? ShopScreen.OutOfStock : ShopManager.GetBuyBackCost(Shop, option) > ShopManager.GetCurrencyAmount() ? ShopScreen.InsufficientFunds : ShopScreen.Confirm,
                    _ => ShopScreen.Confirm,
                },
                ShopScreen.Confirm => ShopScreen.List,
                _ => ShopScreen.None,
            };
        }

        public void OnConversationStarted()
        {

        }

        public void OnConversationEnded()
        {

        }

        public void OnNodeEntered(ShopScreen node)
        {
            switch (node)
            {
                case ShopScreen.List:
                    currentDrop = null;
                    break;
                case ShopScreen.OutOfStock:
                case ShopScreen.InsufficientFunds:
                    Locator.GetPlayerAudioController().PlayNegativeUISound();
                    break;
            }
        }

        public void OnNodeExited(ShopScreen node)
        {

        }

        public void OnOptionSelected(Drop option, ShopScreen node)
        {
            switch (node)
            {
                case ShopScreen.List:
                    if (option == null)
                    {
                        shopDialogueTree.ExitModal();
                    } else
                    {
                        currentDrop = option;
                    }
                    break;
                case ShopScreen.Confirm:
                    if (option != null)
                    {
                        var success = Mode switch
                        {
                            ShopMode.Buy => ShopManager.BuyItem(Shop, option),
                            ShopMode.Sell => ShopManager.SellItem(Shop, option),
                            ShopMode.BuyBack => ShopManager.BuyBackItem(Shop, option),
                            _ => false,
                        };
                        if (success)
                        {
                            Locator.GetPlayerAudioController().PlayOneShotInternal(option.PickUpAudioType);
                        } else
                        {
                            Locator.GetPlayerAudioController().PlayNegativeUISound();
                            OuterWildsRPG.LogWarning($"Failed to {Mode} {option} from shop {Shop}");
                        }
                    }
                    break;
            }
        }
    }
}
