using System;
using System.Collections.Generic;
using System.Linq;
using OuterWildsRPG.Utils;
using UnityEngine;
using OWML.Common;

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
                spacerPrompt.SetVisibility(promptsVisible && IsStarted);
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

        public void CalculateStatus()
        {
            bool shouldStart = (StartOn.Count == 0 && QuestSaveData.HasStartedQuest(Quest)) ||
                (StartOn.Count > 0 && StartOn.Any(c => c.Check())) ||
                (CompleteOn.Count > 0 && CompleteOn.Any(c => c.Check()));
            if (shouldStart && !IsStarted)
            {
                QuestSaveData.StartStep(this);
            }

            bool shouldComplete = CompleteOn.Count == 0 || CompleteOn.Any(c => c.Check());
            if (shouldComplete && !IsComplete)
            {
                QuestSaveData.CompleteStep(this);
                prompt._customSprite = OuterWildsRPG.CheckboxOnSprite;
                Locator.GetPromptManager().TriggerRebuild(prompt);
            }
        }
    }

    public class QuestCondition
    {
        public QuestStep Step;
        public QuestConditionType Type;
        public string Value;

        static bool wakeUp;
        static bool launchCodesEntered;
        static bool completeShipIgnition;
        static SatelliteNode satelliteNode1;
        static SatelliteNode satelliteNode2;
        static SatelliteNode satelliteNode3;

        public static void CleanUpSpecialConditions()
        {
            wakeUp = false;
            launchCodesEntered = false;
            completeShipIgnition = false;
            satelliteNode1 = null;
            satelliteNode2 = null;
            satelliteNode3 = null;
        }

        public static void SetUpSpecialConditionHooks()
        {
            GlobalMessenger.AddListener("WakeUp", () => wakeUp = true);
            GlobalMessenger.AddListener("LaunchCodesEntered", () => launchCodesEntered = true);
            GlobalMessenger.AddListener("CompleteShipIgnition", () => completeShipIgnition = true);
        }

        public static bool CheckSpecialCondition(QuestSpecialConditionType condition)
        {
            switch (condition)
            {
                case QuestSpecialConditionType.WakeUp:
                    return wakeUp;
                case QuestSpecialConditionType.LaunchCodesEntered:
                    return launchCodesEntered;
                case QuestSpecialConditionType.CompleteShipIgnition:
                    return completeShipIgnition;
                case QuestSpecialConditionType.SatelliteRepair0:
                    if (!satelliteNode1)
                        satelliteNode1 = UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:0").GetComponent<SatelliteNode>();
                    return !satelliteNode1._damaged;
                case QuestSpecialConditionType.SatelliteRepair1:
                    if (!satelliteNode2)
                        satelliteNode2 = UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:1").GetComponent<SatelliteNode>();
                    return !satelliteNode2._damaged;
                case QuestSpecialConditionType.SatelliteRepair2:
                    if (!satelliteNode3)
                        satelliteNode3 = UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:2").GetComponent<SatelliteNode>();
                    return !satelliteNode3._damaged;
                default:
                    return false;
            }
        }

        public bool Check()
        {
            return Type switch
            {
                QuestConditionType.None => false,
                QuestConditionType.Step => Step.Quest.Steps.Find(s => s.ID == Value).IsComplete,
                QuestConditionType.Fact => Locator.GetShipLogManager().IsFactRevealed(Value),
                QuestConditionType.Entry => Locator.GetShipLogManager().GetEntry(Value).CalculateState() == ShipLogEntry.State.Explored,
                QuestConditionType.EntryRumored => Locator.GetShipLogManager().GetEntry(Value).CalculateState() == ShipLogEntry.State.Rumored,
                QuestConditionType.Frequency => PlayerData.KnowsFrequency((SignalFrequency)Enum.Parse(typeof(SignalFrequency), Value, true)),
                QuestConditionType.Signal => PlayerData.KnowsSignal((SignalName)Enum.Parse(typeof(SignalName), Value, true)),
                QuestConditionType.DialogueCondition => DialogueConditionManager.SharedInstance.GetConditionState(Value),
                QuestConditionType.PersistentCondition => PlayerData.GetPersistentCondition(Value),
                QuestConditionType.Special => CheckSpecialCondition((QuestSpecialConditionType)Enum.Parse(typeof(QuestSpecialConditionType), Value, true)),
                _ => throw new ArgumentOutOfRangeException($"Unknown {nameof(QuestConditionType)}: {Type}"),
            };
        }
    }

    public enum QuestType
    {
        None,
        Side,
        Main,
        Misc,
    }

    public enum QuestState
    {
        NotStarted,
        InProgress,
        Completed,
    }

    public enum QuestConditionType
    {
        None,
        Step,
        Fact,
        Entry,
        EntryRumored,
        Frequency,
        Signal,
        DialogueCondition,
        PersistentCondition,
        Special,
    }

    public enum QuestSpecialConditionType
    {
        None,
        WakeUp,
        LaunchCodesEntered,
        CompleteShipIgnition,
        SatelliteRepair0,
        SatelliteRepair1,
        SatelliteRepair2,
    }
}
