using System.Collections.Generic;
using UnityEngine;

namespace OuterWildsRPG.Components.Dialogue
{
    public interface ICustomDialogueTreeProvider<INode, IOption>
    {
        string GetCharacterName();
        string GetInteractPrompt();
        bool? GetFlashlightState();
        bool GetIsRecording();
        INode GetEntryNode(string context);
        Transform GetAttentionPoint();
        Vector3 GetAttentionPointOffset();
        string GetNodeText(INode node, int page);
        bool GetNodeHasNextPage(INode node, int page);
        INode GetNodeTarget(INode node);
        IEnumerable<IOption> GetNodeOptions(INode node);
        string GetOptionText(IOption option, INode node);
        INode GetOptionTarget(IOption option, INode node);

        void OnConversationStarted();
        void OnConversationEnded();
        void OnNodeEntered(INode node);
        void OnNodeExited(INode node);
        void OnOptionSelected(IOption option, INode node);
    }
}
