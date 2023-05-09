using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Components.Dialogue
{
    public interface ICustomDialogueTree
    {
        void SwitchTo(ICustomDialogueTree target, string context);
        bool InConversation();
        void StartConversation(string context, bool silent);
        void EndConversation(bool silent);
    }
}
