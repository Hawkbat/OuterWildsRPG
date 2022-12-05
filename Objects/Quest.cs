using System;
using System.Collections.Generic;
using System.Linq;
using OuterWildsRPG.Utils;
using UnityEngine;
using OWML.Common;
using OWML.Utils;

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

    public class QuestCondition
    {
        public QuestStep Step;
        public QuestConditionType Type;
        public string Value;

        public QuestSpecialConditionType SpecialConditionType => EnumUtils.Parse<QuestSpecialConditionType>(Value, true);

        public bool IsStartCondition => Step.StartOn.Contains(this);
        public bool IsCompletionCondition => Step.CompleteOn.Contains(this);

        public void SetUp()
        {
            if (Type == QuestConditionType.Conversation)
            {
                try
                {
                    var target = UnityUtils.GetTransformAtPath(Value).GetComponentInChildren<CharacterDialogueTree>();
                    target.OnEndConversation += Trigger;
                } catch (Exception ex)
                {
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to attach to conversation event for {Step.Quest.ID}:{Step.ID}", MessageType.Warning);
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                }
            } else if (Type == QuestConditionType.Special)
            {
                switch (SpecialConditionType)
                {
                    case QuestSpecialConditionType.WakeUp:
                        GlobalMessenger.AddListener("WakeUp", Trigger);
                        break;
                    case QuestSpecialConditionType.LaunchCodesEntered:
                        GlobalMessenger.AddListener("LaunchCodesEntered", Trigger);
                        break;
                    case QuestSpecialConditionType.CompleteShipIgnition:
                        GlobalMessenger.AddListener("CompleteShipIgnition", Trigger);
                        break;
                    case QuestSpecialConditionType.EnterRemoteFlightConsole:
                        GlobalMessenger<OWRigidbody>.AddListener("EnterRemoteFlightConsole", Trigger1);
                        break;
                    case QuestSpecialConditionType.SatelliteRepair0:
                        UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:0").GetComponent<SatelliteNode>().OnRepaired += (node) => Trigger();
                        break;
                    case QuestSpecialConditionType.SatelliteRepair1:
                        UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:1").GetComponent<SatelliteNode>().OnRepaired += (node) => Trigger();
                        break;
                    case QuestSpecialConditionType.SatelliteRepair2:
                        UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:2").GetComponent<SatelliteNode>().OnRepaired += (node) => Trigger();
                        break;
                    case QuestSpecialConditionType.SuitUp:
                        // Not using the global SuitUp message because that also fires for the training suit
                        UnityUtils.GetTransformAtPath("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear/InteractVolume").GetComponent<MultiInteractReceiver>().OnPressInteract += (cmd) => Trigger();
                        break;
                }
            }
        }

        public void CleanUp()
        {
            if (Type == QuestConditionType.Special)
            {
                switch (SpecialConditionType)
                {
                    case QuestSpecialConditionType.WakeUp:
                        GlobalMessenger.RemoveListener("WakeUp", Trigger);
                        break;
                    case QuestSpecialConditionType.LaunchCodesEntered:
                        GlobalMessenger.RemoveListener("LaunchCodesEntered", Trigger);
                        break;
                    case QuestSpecialConditionType.CompleteShipIgnition:
                        GlobalMessenger.RemoveListener("CompleteShipIgnition", Trigger);
                        break;
                    case QuestSpecialConditionType.EnterRemoteFlightConsole:
                        GlobalMessenger<OWRigidbody>.RemoveListener("EnterRemoteFlightConsole", Trigger1);
                        break;
                }
            }
        }

        public void Trigger()
        {
            Step.CalculateStatus(IsStartCondition, IsCompletionCondition);
        }

        private void Trigger1<T>(T _) => Trigger();

        public bool Check()
        {
            return Type switch
            {
                QuestConditionType.None => false,
                QuestConditionType.Step => Step.Quest.Steps.Find(s => s.ID == Value).IsComplete,
                QuestConditionType.Quest => OuterWildsRPG.Quests.Any(q => q.ID == Value && q.IsComplete),
                QuestConditionType.Fact => Locator.GetShipLogManager().IsFactRevealed(Value),
                QuestConditionType.Entry => Locator.GetShipLogManager().GetEntry(Value).CalculateState() == ShipLogEntry.State.Explored,
                QuestConditionType.EntryRumored => Locator.GetShipLogManager().GetEntry(Value).CalculateState() == ShipLogEntry.State.Rumored,
                QuestConditionType.Frequency => PlayerData.KnowsFrequency(EnumUtils.Parse<SignalFrequency>(Value, true)),
                QuestConditionType.Signal => PlayerData.KnowsSignal(EnumUtils.Parse<SignalName>(Value, true)),
                QuestConditionType.DialogueCondition => DialogueConditionManager.SharedInstance.GetConditionState(Value),
                QuestConditionType.PersistentCondition => PlayerData.GetPersistentCondition(Value),
                QuestConditionType.Conversation => false,
                QuestConditionType.Special => false,
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
        Quest,
        Fact,
        Entry,
        EntryRumored,
        Frequency,
        Signal,
        DialogueCondition,
        PersistentCondition,
        Conversation,
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
        EnterRemoteFlightConsole,
        SuitUp,
    }
}
