using OuterWildsRPG.Objects.Common.Dialogue;
using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Shops
{
    public class Shop : Entity<Shop, ShopData>
    {
        public string CharacterPath;
        public Shopkeeper CharacterClone;
        public DialogueTreeData DialogueTree;
        public bool MergeDialogue;
        public List<ShopItem> Items = new();

        public override void Load(ShopData data, string modID)
        {
            base.Load(data, modID);
            CharacterPath = data.characterPath;
            CharacterClone = Shopkeeper.LoadNew(data.characterClone, modID);
            DialogueTree = null;
            if (!string.IsNullOrEmpty(data.dialogueXmlPath))
            {
                try
                {
                    var xml = Assets.GetTextFileContents($"shops/{data.dialogueXmlPath}", modID);
                    DialogueTree = DialogueUtils.ConvertTree(xml);
                } catch (Exception ex)
                {
                    OuterWildsRPG.LogException(ex, $"Failed to read XML file for shop {FullID}");
                }
            }
            MergeDialogue = data.mergeDialogue;
            Items = data.items.Select(i => ShopItem.LoadNew(i, modID)).ToList();
            foreach (var item in Items) item.Shop = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            CharacterClone?.Resolve();
            foreach (var item in Items) item.Resolve();
            if (DialogueTree != null)
                TranslationUtils.RegisterDialogueTree(DialogueTree);
        }
    }
}
