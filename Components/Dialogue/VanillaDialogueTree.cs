using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.Dialogue
{
    public class VanillaDialogueTree : CustomDialogueTree<DialogueNode, DialogueOption> {
        public void Init(CharacterDialogueTree dialogueTree)
        {
            dialogueTree._interactVolume.OnPressInteract -= dialogueTree.OnPressInteract;
            Init(new VanillaDialogueTreeProvider(dialogueTree), true);
        }
    }

    public class VanillaDialogueTreeProvider : ICustomDialogueTreeProvider<DialogueNode, DialogueOption>
    {
        readonly CharacterDialogueTree dialogueTree;

        public VanillaDialogueTreeProvider(CharacterDialogueTree dialogueTree)
        {
            this.dialogueTree = dialogueTree;
        }

        public Transform GetAttentionPoint() => dialogueTree._attentionPoint;

        public Vector3 GetAttentionPointOffset() => dialogueTree._attentionPointOffset;

        public string GetCharacterName()
            => TextTranslation.Translate(dialogueTree._characterName);

        public DialogueNode GetEntryNode(string context)
            => string.IsNullOrEmpty(context) ? dialogueTree._mapDialogueNodes.Values
            .Reverse()
            .FirstOrDefault(n => n.EntryConditionsSatisfied()) : dialogueTree._mapDialogueNodes[context];

        public bool? GetFlashlightState()
            => dialogueTree._turnOnFlashlight ? true : dialogueTree._turnOffFlashlight ? false : null;

        public bool GetIsRecording() => dialogueTree._isRecording;

        public string GetInteractPrompt()
        {
            if (dialogueTree._characterName == "SIGN")
                return UITextLibrary.GetString(UITextType.ReadPrompt);
            else if (dialogueTree._characterName == "RECORDING")
                return UITextLibrary.GetString(UITextType.RecordingPrompt);
            return $"{UITextLibrary.GetString(UITextType.TalkToPrompt)} {TextTranslation.Translate(dialogueTree._characterName)}";
        }

        public string GetNodeText(DialogueNode node, int page)
        {
            var options = new List<DialogueOption>();
            node.GetNextPage(out var text, ref options);
            return text;
        }

        public bool GetNodeHasNextPage(DialogueNode node, int page) => node.HasNext();

        public IEnumerable<DialogueOption> GetNodeOptions(DialogueNode node)
            => node._listDialogueOptions.Where(CheckOptionValidity);

        bool CheckOptionValidity(DialogueOption dialogueOption)
        {
            if (dialogueOption.ConditionRequirement != string.Empty && !DialogueConditionManager.SharedInstance.GetConditionState(dialogueOption.ConditionRequirement))
                return false;
            foreach (var c in dialogueOption.PersistentCondition)
                if (!PlayerData.GetPersistentCondition(c))
                    return false;
            foreach (var c in dialogueOption.LogConditionRequirement)
                if (Locator.GetShipLogManager().GetFact(c) != null && !Locator.GetShipLogManager().GetFact(c).IsRevealed())
                    return false;
            if (dialogueOption.CancelledRequirement != string.Empty && DialogueConditionManager.SharedInstance.GetConditionState(dialogueOption.CancelledRequirement))
                return false;
            foreach (var c in dialogueOption.CancelledPersistentRequirement)
                if (PlayerData.GetPersistentCondition(c))
                    return false;
            return true;
        }

        public DialogueNode GetNodeTarget(DialogueNode node) => node.Target;

        public DialogueNode GetOptionTarget(DialogueOption option, DialogueNode node)
            => dialogueTree._mapDialogueNodes[option.TargetName];

        public string GetOptionText(DialogueOption option, DialogueNode node) => option.Text;

        public void OnConversationEnded()
        {

        }

        public void OnConversationStarted()
        {

        }

        public void OnNodeEntered(DialogueNode node)
        {
            if (node == null) return;
            node.RefreshConditionsData();
        }

        public void OnNodeExited(DialogueNode node)
        {
            if (node == null) return;
            node.SetNodeCompleted();
        }

        public void OnOptionSelected(DialogueOption option, DialogueNode node)
        {

        }
    }
}
