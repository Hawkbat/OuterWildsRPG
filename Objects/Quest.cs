using System;
using System.Collections.Generic;
using System.Linq;
using OuterWildsRPG.Utils;
using UnityEngine;
using OWML.Common;
using OWML.Utils;
using OuterWildsRPG.Enums;

namespace OuterWildsRPG.Objects
{
    public class Quest
    {
        public string ID;
        public string Name;
        public QuestType Type;
        public List<QuestStep> Steps = new();

        public bool IsTracked => QuestSaveData.IsTrackingQuest(this);

        public bool IsStarted => QuestSaveData.HasStartedQuest(this);
        public bool IsInProgress => !IsComplete && IsStarted;
        public bool IsComplete => QuestSaveData.HasCompletedQuest(this);

        private ScreenPrompt namePrompt;
        private ScreenPrompt spacerPrompt;

        public void SetUp()
        {
            if (namePrompt == null)
            {
                namePrompt = new ScreenPrompt($"[{Name.ToUpper()}]");
                Locator.GetPromptManager().AddScreenPrompt(namePrompt, OuterWildsRPG.QuestPromptPosition);
            }

            foreach (var step in Steps) step.SetUp();

            if (spacerPrompt == null)
            {
                spacerPrompt = new ScreenPrompt(" ");
                Locator.GetPromptManager().AddScreenPrompt(spacerPrompt, OuterWildsRPG.QuestPromptPosition);
            }
        }

        public void CleanUp()
        {
            if (Locator.GetPromptManager() && namePrompt != null)
                Locator.GetPromptManager().RemoveScreenPrompt(namePrompt);
            namePrompt = null;

            if (Locator.GetPromptManager() && spacerPrompt != null)
                Locator.GetPromptManager().RemoveScreenPrompt(spacerPrompt);
            spacerPrompt = null;

            foreach (var step in Steps) step.CleanUp();
        }

        public void Update(bool promptsVisible)
        {
            CalculateStatus();

            if (namePrompt != null)
            {
                namePrompt.SetVisibility(IsInProgress);
                namePrompt.SetVisibility(promptsVisible && IsInProgress);
                namePrompt.SetTextColor(OuterWildsRPG.OnColor);
            }

            foreach (var step in Steps) step.Update(promptsVisible);

            if (spacerPrompt != null)
                spacerPrompt.SetVisibility(promptsVisible && IsInProgress);
        }

        public void CalculateStatus()
        {
            bool shouldStart = Steps.Any(s => s.IsStarted);
            if (shouldStart && !QuestSaveData.HasStartedQuest(this))
            {
                QuestSaveData.StartQuest(this);
            }

            bool shouldComplete = Steps.All(s => s.IsComplete || s.Optional);
            if (shouldComplete && !QuestSaveData.HasCompletedQuest(this))
            {
                QuestSaveData.CompleteQuest(this);
            }
        }
    }
}
