using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG
{
    public class QuestLogMode : ShipLogMode
    {
        OWAudioSource oneShotSource;
        string prevEntryID;
        ScreenPrompt trackQuestPrompt;
        ScreenPrompt untrackQuestPrompt;

        ScreenPromptList centerPromptList;
        ScreenPromptList upperRightPromptList;

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            this.oneShotSource = oneShotSource;
            trackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Track Quest");
            untrackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Untrack Quest");
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            prevEntryID = entryID;
            oneShotSource.PlayOneShot(AudioType.ShipLogEnterDetectiveMode);
        }

        public override void ExitMode()
        {

        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public void SelectQuest(Quest quest)
        {

        }

        public void DeselectQuest(Quest quest)
        {

        }

        public override void UpdateMode()
        {
            if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
            {

            }
            if (OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2))
            {

            }
            if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
            {

            }
        }

        public override bool AllowModeSwap()
        {
            return true;
        }

        public override bool AllowCancelInput()
        {
            return true;
        }

        public override string GetFocusedEntryID()
        {
            return prevEntryID;
        }
    }
}
