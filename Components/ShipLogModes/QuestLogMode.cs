using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OuterWildsRPG.External;
using OuterWildsRPG.Objects.Quests;
using OuterWildsRPG.Utils;
using UnityEngine;

namespace OuterWildsRPG.Components.ShipLogModes
{
    public class QuestLogMode : ShipLogMode
    {
        OWAudioSource oneShotSource;
        string prevEntryID;
        ScreenPrompt trackQuestPrompt;
        ScreenPrompt untrackQuestPrompt;

        ScreenPromptList centerPromptList;
        ScreenPromptList upperRightPromptList;

        MonoBehaviour itemList;

        List<Quest> quests = new();
        Quest selected;

        public ICustomShiplogModesAPI ShiplogModesAPI;

        public override void Initialize(ScreenPromptList centerPromptList, ScreenPromptList upperRightPromptList, OWAudioSource oneShotSource)
        {
            this.oneShotSource = oneShotSource;
            this.centerPromptList = centerPromptList;
            this.upperRightPromptList = upperRightPromptList;

            trackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Track Quest");
            untrackQuestPrompt = new ScreenPrompt(InputLibrary.markEntryOnHUD, "Untrack Quest");

            ShiplogModesAPI.ItemListMake(true, (itemList) =>
            {
                this.itemList = itemList;
                ShiplogModesAPI.ItemListSetName(itemList, "Quest Log");
            });
        }

        public override void EnterMode(string entryID = "", List<ShipLogFact> revealQueue = null)
        {
            prevEntryID = entryID;
            oneShotSource.PlayOneShot(AudioType.ShipLogEnterDetectiveMode);

            quests = QuestManager.GetStartedQuests()
                .OrderBy(q => q.IsComplete)
                .ThenBy(q => q.Type)
                .ThenBy(q => q.ToDisplayString())
                .ToList();

            Locator.GetPromptManager().AddScreenPrompt(trackQuestPrompt, upperRightPromptList, TextAnchor.MiddleRight);
            Locator.GetPromptManager().AddScreenPrompt(untrackQuestPrompt, upperRightPromptList, TextAnchor.MiddleRight);

            ShiplogModesAPI.ItemListOpen(itemList);

            UpdateItems();
            UpdateDescription();
        }

        public override void ExitMode()
        {
            Locator.GetPromptManager().RemoveScreenPrompt(trackQuestPrompt);
            Locator.GetPromptManager().RemoveScreenPrompt(untrackQuestPrompt);

            ShiplogModesAPI.ItemListClose(itemList);
        }

        public override void OnEnterComputer()
        {

        }

        public override void OnExitComputer()
        {

        }

        public override void UpdateMode()
        {
            int index = ShiplogModesAPI.ItemListGetSelectedIndex(itemList);
            int delta = ShiplogModesAPI.ItemListUpdateList(itemList);
            if (quests.Count == 0)
            {
                selected = null;
                trackQuestPrompt.SetVisibility(false);
                untrackQuestPrompt.SetVisibility(false);
                return;
            }
            index = (index + delta + quests.Count) % quests.Count;

            selected = quests[index];

            if (delta != 0) UpdateDescription();

            if (!selected.IsComplete && OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
            {
                if (selected.IsTrackedManual)
                    oneShotSource.PlayOneShot(AudioType.ShipLogUnmarkLocation);
                else
                    oneShotSource.PlayOneShot(AudioType.ShipLogMarkLocation);
                QuestManager.SetTrackingQuest(selected, !selected.IsTrackedManual, false);
                UpdateItems();
            }
            trackQuestPrompt.SetVisibility(!selected.IsTrackedManual);
            untrackQuestPrompt.SetVisibility(selected.IsTrackedManual);
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

        private void UpdateItems()
        {
            var items = quests.Select(q =>
            {
                var name = q.ToDisplayString();
                if (q.IsComplete) name = UnityUtils.RichTextColor(name, Assets.HUDActiveColor);
                var isMarked = q.IsTrackedManual;
                var isUnread = false;
                var moreToExplore = !q.IsComplete;
                return new Tuple<string, bool, bool, bool>(name, isMarked, isUnread, moreToExplore);
            }).ToList();
            ShiplogModesAPI.ItemListSetItems(itemList, items);
        }

        private void UpdateDescription()
        {
            ShiplogModesAPI.ItemListDescriptionFieldClear(itemList);
            if (selected != null)
            {
                foreach (var step in selected.Steps.Where(s => s.IsStarted).Reverse())
                {
                    var item = ShiplogModesAPI.ItemListDescriptionFieldGetNextItem(itemList);
                    var text = step.ToDisplayString();
                    if (step.Optional) text = Translations.QuestStepOptional(step);
                    if (step.IsComplete) text = UnityUtils.RichTextColor(text, Assets.HUDActiveColor);
                    item.DisplayText(text);
                }
            }
        }
    }
}
