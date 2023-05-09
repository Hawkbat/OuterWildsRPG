using OuterWildsRPG.Components.Dialogue;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Shops;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class ShopkeeperController : MonoBehaviour
    {
        Shop shop;
        VanillaDialogueTree vanillaDialogueTree;
        ShopDialogueTree shopDialogueTree;

        public void Init(Shop shop)
        {
            this.shop = shop;

            var dialogueTree = gameObject.GetComponentInChildren<CharacterDialogueTree>();

            if (dialogueTree != null)
            {
                if (!shop.MergeDialogue)
                    DialogueUtils.Clear(dialogueTree);
                DialogueUtils.Merge(dialogueTree, shop.DialogueTree);
                vanillaDialogueTree = dialogueTree.gameObject.AddComponent<VanillaDialogueTree>();
                vanillaDialogueTree.Init(dialogueTree);
                shopDialogueTree = dialogueTree.gameObject.AddComponent<ShopDialogueTree>();
                shopDialogueTree.Init(shop);
                vanillaDialogueTree.OnOptionSelected.AddListener(OnOptionSelected);
            }
        }

        void OnDestroy()
        {
            if (vanillaDialogueTree != null)
            {
                vanillaDialogueTree.OnOptionSelected.RemoveListener(OnOptionSelected);
            }
        }

        void OnOptionSelected(DialogueOption option, DialogueNode node)
        {
            if (option.TargetName == "SHOP_BUY")
            {
                shopDialogueTree.EnterModal(ShopMode.Buy, vanillaDialogueTree, node.Name);
            }
            else if (option.TargetName == "SHOP_SELL")
            {
                shopDialogueTree.EnterModal(ShopMode.Sell, vanillaDialogueTree, node.Name);
            }
            else if (option.TargetName == "SHOP_BUYBACK")
            {
                shopDialogueTree.EnterModal(ShopMode.BuyBack, vanillaDialogueTree, node.Name);
            }
        }
    }
}
