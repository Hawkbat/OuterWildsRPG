using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.Objects;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class QuestLogMode : ShipLogMode
    {
        OWAudioSource oneShotSource;
        string prevEntryID;
        ScreenPrompt trackQuestPrompt;
        ScreenPrompt untrackQuestPrompt;

        ScreenPromptList centerPromptList;
        ScreenPromptList upperRightPromptList;

        List<Quest> quests = new();
        Quest selected;

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            this.oneShotSource = oneShotSource;
            this.centerPromptList = centerPromptList;
            this.upperRightPromptList = upperRightPromptList;

            trackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Track Quest");
            untrackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Untrack Quest");
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            prevEntryID = entryID;
            oneShotSource.PlayOneShot(AudioType.ShipLogEnterDetectiveMode);

            quests = OuterWildsRPG.Quests.Where(q => q.IsStarted).ToList();

            Locator.GetPromptManager().AddScreenPrompt(trackQuestPrompt, centerPromptList, TextAnchor.MiddleRight);
            Locator.GetPromptManager().AddScreenPrompt(untrackQuestPrompt, centerPromptList, TextAnchor.MiddleRight);
        }

        public override void ExitMode()
        {
            Locator.GetPromptManager().RemoveScreenPrompt(trackQuestPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(untrackQuestPrompt);
        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public override void UpdateMode()
        {
            var index = quests.IndexOf(selected);
            if (index < 0 && quests.Count > 0)
            {
                selected = quests[0];
                index = 0;
            }

            if (OWInput.IsNewlyPressed(InputLibrary.up) || OWInput.IsNewlyPressed(InputLibrary.up2))
            {
                selected = quests[index > 0 ? index - 1 : quests.Count - 1];
            }

            if (OWInput.IsNewlyPressed(InputLibrary.down) || OWInput.IsNewlyPressed(InputLibrary.down2))
            {
                selected = quests[index == quests.Count - 1 ? 0 : index + 1];
            }

            bool isTracked = QuestSaveData.IsTrackingQuest(selected);
            if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
            {
                QuestSaveData.SetTrackingQuest(selected, !isTracked);
            }
            trackQuestPrompt.SetVisibility(!isTracked);
            untrackQuestPrompt.SetVisibility(isTracked);
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
