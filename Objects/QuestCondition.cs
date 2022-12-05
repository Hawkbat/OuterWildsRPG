using OuterWildsRPG.Enums;
using OuterWildsRPG.Utils;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects
{
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
                }
                catch (Exception ex)
                {
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to attach to conversation event for {Step.Quest.ID}:{Step.ID}", MessageType.Warning);
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                }
            }
            else if (Type == QuestConditionType.Special)
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

        // Needed to avoid managing extra callback instances when using GlobalMessenger with type args
        private void Trigger1<T>(T _) => Trigger();
        private void Trigger2<T, U>(T _, U __) => Trigger();

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
}
