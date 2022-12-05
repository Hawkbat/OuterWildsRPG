using OuterWildsRPG.Utils;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects
{
    public class QuestStep
    {
        public Quest Quest;

        public string ID;
        public string Text;
        public List<QuestCondition> StartOn = new();
        public List<QuestCondition> CompleteOn = new();
        public string LocationEntry;
        public string LocationPath;
        public bool Optional;

        public bool IsStarted => QuestSaveData.HasStartedStep(this);
        public bool IsInProgress => IsStarted && !IsComplete;
        public bool IsComplete => QuestSaveData.HasCompletedStep(this);

        private ScreenPrompt prompt;
        private CanvasMarker canvasMarker;
        private CanvasMapMarker mapMarker;

        public void SetUp()
        {
            if (prompt == null)
            {
                prompt = new ScreenPrompt($"{(Optional ? "(Optional) " : "")}{Text}", customSprite: IsComplete ? OuterWildsRPG.CheckboxOnSprite : OuterWildsRPG.CheckboxOffSprite);
                Locator.GetPromptManager().AddScreenPrompt(prompt, OuterWildsRPG.QuestPromptPosition);
            }
            if (canvasMarker == null && mapMarker == null)
            {
                try
                {
                    Transform location = null;

                    if (!string.IsNullOrEmpty(LocationEntry))
                    {
                        location = Locator.GetEntryLocation(LocationEntry).GetTransform();
                    }
                    else if (!string.IsNullOrEmpty(LocationPath))
                    {
                        location = UnityUtils.GetTransformAtPath(LocationPath);
                    }

                    if (location != null)
                    {
                        canvasMarker = Locator.GetMarkerManager().InstantiateNewMarker();
                        Locator.GetMarkerManager().RegisterMarker(canvasMarker, location, Text, 0f);
                        mapMarker = Locator.GetMapController().GetMarkerManager().InstantiateNewMarker(true);
                        mapMarker.SetLabel(Text);
                        Locator.GetMapController().GetMarkerManager().RegisterMarker(mapMarker, location);
                    }
                }
                catch (Exception ex)
                {
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to create quest marker for {Quest.ID}:{ID}", MessageType.Warning);
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                }
            }
            foreach (var cond in StartOn) cond.SetUp();
            foreach (var cond in CompleteOn) cond.SetUp();
        }

        public void CleanUp()
        {
            if (Locator.GetPromptManager() && prompt != null)
                Locator.GetPromptManager().RemoveScreenPrompt(prompt);
            prompt = null;

            if (Locator.GetMarkerManager() && canvasMarker != null)
                Locator.GetMarkerManager().UnregisterMarker(canvasMarker);
            canvasMarker = null;

            if (Locator.GetMapController()?.GetMarkerManager() && mapMarker != null)
                Locator.GetMapController().GetMarkerManager().UnregisterMarker(mapMarker);
            mapMarker = null;

            foreach (var cond in StartOn) cond.CleanUp();
            foreach (var cond in CompleteOn) cond.CleanUp();
        }

        public void Update(bool promptsVisible)
        {
            CalculateStatus();

            if (prompt != null)
            {
                prompt.SetVisibility(promptsVisible && Quest.IsInProgress && IsStarted);
                if (IsComplete) prompt.SetTextColor(OuterWildsRPG.OnColor);
                else prompt.SetTextColor(prompt.GetDefaultColor());
            }
            if (canvasMarker)
            {
                canvasMarker.SetVisibility(promptsVisible && Quest.IsInProgress && IsInProgress);
            }
            if (mapMarker)
            {
                mapMarker.SetVisibility(promptsVisible && Quest.IsInProgress && IsInProgress);
            }
        }

        public void CalculateStatus(bool forceStart = false, bool forceComplete = false)
        {
            bool shouldStart = forceStart || (StartOn.Count == 0 && QuestSaveData.HasStartedQuest(Quest)) ||
                (StartOn.Count > 0 && StartOn.Any(c => c.Check())) ||
                (CompleteOn.Count > 0 && CompleteOn.Any(c => c.Check()));
            if (shouldStart && !IsStarted)
            {
                QuestSaveData.StartStep(this);
            }

            bool shouldComplete = forceComplete || CompleteOn.Count == 0 || CompleteOn.Any(c => c.Check());
            if (shouldComplete && !IsComplete)
            {
                QuestSaveData.CompleteStep(this);
                prompt._customSprite = OuterWildsRPG.CheckboxOnSprite;
                Locator.GetPromptManager().TriggerRebuild(prompt);
            }
        }
    }
}
