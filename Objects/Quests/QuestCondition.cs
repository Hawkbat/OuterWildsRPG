using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestCondition : EntityLike<QuestCondition, QuestConditionData>
    {
        public QuestStep Step;
        public QuestConditionType Type;
        public string Value;
        public string MarkerEntry;
        public string MarkerPath;
        public bool DisableAutoMarkers;

        private Quest valueQuest;
        private QuestStep valueStep;
        private Drop valueDrop;
        private ShipLogFact valueFact;
        private ShipLogEntry valueEntry;
        private SignalFrequency valueFrequency;
        private SignalName valueSignal;

        private bool disabled;
        private bool wereMarkersNeeded;
        private List<CanvasMarker> canvasMarkers = new();
        private List<CanvasMapMarker> mapMarkers = new();

        public QuestSpecialConditionType SpecialConditionType
            => EnumUtils.Parse<QuestSpecialConditionType>(Value, true);

        public bool IsStartCondition => Step.StartOn.Contains(this);
        public bool IsCompletionCondition => Step.CompleteOn.Contains(this);

        public override void Load(QuestConditionData data, string modID)
        {
            base.Load(data, modID);
            Type = data.type;
            Value = data.value;
            MarkerEntry = data.markerEntry;
            MarkerPath = data.markerPath;
            DisableAutoMarkers = data.disableAutoMarkers;
        }

        public void SetUp()
        {
            disabled = false;
            try
            {
                switch (Type)
                {
                    case QuestConditionType.Step:
                        valueStep = Step.Quest.Steps.Find(s => s.ID == Value);
                        if (valueStep == null) throw new NullReferenceException();
                        break;
                    case QuestConditionType.Quest:
                        valueQuest = QuestManager.GetQuest(Value, Step.Quest.ModID);
                        if (valueQuest == null) throw new NullReferenceException();
                        break;
                    case QuestConditionType.EquipDrop:
                    case QuestConditionType.HaveDrop:
                        valueDrop = DropManager.GetDrop(Value, Step.Quest.ModID);
                        if (valueDrop == null) throw new NullReferenceException();
                        break;
                    case QuestConditionType.Fact:
                        valueFact = Locator.GetShipLogManager().GetFact(Value);
                        if (valueFact == null) throw new NullReferenceException();
                        break;
                    case QuestConditionType.Entry:
                    case QuestConditionType.EntryRumored:
                        valueEntry = Locator.GetShipLogManager().GetEntry(Value);
                        if (valueEntry == null) throw new NullReferenceException();
                        break;
                    case QuestConditionType.Frequency:
                        valueFrequency = EnumUtils.Parse(Value, true, (SignalFrequency)int.MaxValue);
                        if (valueFrequency == (SignalFrequency)int.MaxValue) throw new ArgumentOutOfRangeException($"Failed to parse {nameof(SignalFrequency)} value");
                        break;
                    case QuestConditionType.Signal:
                        valueSignal = EnumUtils.Parse(Value, true, (SignalName)int.MaxValue);
                        if (valueSignal == (SignalName)int.MaxValue) throw new ArgumentOutOfRangeException($"Failed to parse {nameof(SignalName)} value");
                        break;
                }
            } catch (Exception ex)
            {
                OuterWildsRPG.LogException(ex, $"Failed to find matching value for {this}. The condition will be disabled.");
                disabled = true;
            }
            try
            {
                if (Type == QuestConditionType.Conversation)
                {
                    var target = UnityUtils.GetTransformAtPathUnsafe(Value).GetComponentInChildren<CharacterDialogueTree>();
                    target.OnEndConversation += Trigger;
                }
                else if (Type == QuestConditionType.EnterVolume || Type == QuestConditionType.ExitVolume)
                {
                    var target = UnityUtils.GetTransformAtPathUnsafe(Value).GetComponentInChildren<OWTriggerVolume>();
                    if (Type == QuestConditionType.EnterVolume)
                        target.OnEntry += TriggerIfPlayer;
                    else if (Type == QuestConditionType.ExitVolume)
                        target.OnExit += TriggerIfPlayer;
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
                            UnityUtils.GetTransformAtPathUnsafe("MiningRig_Body/Nodes/BrokenNode:0").GetComponent<SatelliteNode>().OnRepaired += Trigger1;
                            break;
                        case QuestSpecialConditionType.SatelliteRepair1:
                            UnityUtils.GetTransformAtPathUnsafe("MiningRig_Body/Nodes/BrokenNode:1").GetComponent<SatelliteNode>().OnRepaired += Trigger1;
                            break;
                        case QuestSpecialConditionType.SatelliteRepair2:
                            UnityUtils.GetTransformAtPathUnsafe("MiningRig_Body/Nodes/BrokenNode:2").GetComponent<SatelliteNode>().OnRepaired += Trigger1;
                            break;
                        case QuestSpecialConditionType.SuitUp:
                            // Not using the global SuitUp message because that also fires for the training suit
                            UnityUtils.GetTransformAtPathUnsafe("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear/InteractVolume").GetComponent<MultiInteractReceiver>().OnPressInteract += Trigger1;
                            break;
                    }
                
                }
            }
            catch (Exception ex)
            {
                OuterWildsRPG.LogException(ex, $"Failed to attach to condition target for {this}. The condition will be disabled.");
                disabled = true;
            }
        }

        public void Update(bool markersVisible)
        {
            var areMarkersNeeded = Step.IsInProgress && !Check();
            if (areMarkersNeeded != wereMarkersNeeded && (Locator.GetMarkerManager() || Locator.GetMapController()))
            {
                if (areMarkersNeeded)
                {
                    foreach (var markerTarget in GetMarkerTargets().Where(t => t != null))
                    {
                        var canvasMarker = Locator.GetMarkerManager().InstantiateNewMarker();
                        Locator.GetMarkerManager().RegisterMarker(canvasMarker, markerTarget, Step.ToDisplayString(), 0f);
                        canvasMarkers.Add(canvasMarker);

                        var mapMarker = Locator.GetMapController().GetMarkerManager().InstantiateNewMarker(true);
                        mapMarker.SetLabel(Step.ToDisplayString());
                        Locator.GetMapController().GetMarkerManager().RegisterMarker(mapMarker, markerTarget);
                        mapMarkers.Add(mapMarker);

                        var outerFogWarp = markerTarget.root.GetComponentInChildren<OuterFogWarpVolume>();
                        if (outerFogWarp != null)
                        {
                            canvasMarker.SetOuterFogWarpVolume(outerFogWarp);
                            mapMarker.SetOuterFogWarpVolume(outerFogWarp);
                            Locator.GetMarkerManager().RequestFogMarkerUpdate();
                        }
                    }
                } else
                {
                    foreach (var canvasMarker in canvasMarkers)
                        Locator.GetMarkerManager().UnregisterMarker(canvasMarker);
                    canvasMarkers.Clear();

                    foreach (var mapMarker in mapMarkers)
                        Locator.GetMapController().GetMarkerManager().UnregisterMarker(mapMarker);
                    mapMarkers.Clear();
                }
                wereMarkersNeeded = areMarkersNeeded;
            }
            foreach (var canvasMarker in canvasMarkers)
                canvasMarker.SetVisibility(markersVisible);
            foreach (var mapMarker in mapMarkers)
                mapMarker.SetVisibility(markersVisible);
        }

        public void CleanUp()
        {
            foreach (var canvasMarker in canvasMarkers)
                Locator.GetMarkerManager().UnregisterMarker(canvasMarker);
            canvasMarkers.Clear();

            foreach (var mapMarker in mapMarkers)
                Locator.GetMapController().GetMarkerManager().UnregisterMarker(mapMarker);
            mapMarkers.Clear();

            wereMarkersNeeded = false;

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
            Step.CalculateStatus(this);
        }

        // Needed to avoid managing extra callback instances when using GlobalMessenger with type args
        private void Trigger1<T>(T _) => Trigger();
        private void Trigger2<T, U>(T _, U __) => Trigger();

        private void TriggerIfPlayer(GameObject go)
        {
            if (go.transform.root == Locator.GetPlayerBody().transform.root)
            {
                Trigger();
            }
        }

        public bool Check()
        {
            if (disabled) return false;
            try
            {
                return Type switch
                {
                    QuestConditionType.None => false,
                    QuestConditionType.Step => valueStep.IsComplete,
                    QuestConditionType.Quest => valueQuest.IsComplete,
                    QuestConditionType.Fact => valueFact.IsRevealed(),
                    QuestConditionType.Entry => valueEntry.CalculateState() == ShipLogEntry.State.Explored,
                    QuestConditionType.EntryRumored => valueEntry.CalculateState() == ShipLogEntry.State.Rumored,
                    QuestConditionType.Frequency => PlayerData.KnowsFrequency(valueFrequency),
                    QuestConditionType.Signal => PlayerData.KnowsSignal(valueSignal),
                    QuestConditionType.DialogueCondition => DialogueConditionManager.SharedInstance.GetConditionState(Value),
                    QuestConditionType.PersistentCondition => PlayerData.GetPersistentCondition(Value),
                    QuestConditionType.Conversation => false,
                    QuestConditionType.EnterVolume => false,
                    QuestConditionType.ExitVolume => false,
                    QuestConditionType.HaveDrop => DropManager.HasDrop(valueDrop),
                    QuestConditionType.EquipDrop => DropManager.GetEquippedDrop(valueDrop.EquipSlot) == valueDrop,
                    QuestConditionType.Special => false,
                    _ => throw new ArgumentOutOfRangeException($"Unknown {nameof(QuestConditionType)}: {Type}"),
                };
            } catch (Exception ex)
            {
                OuterWildsRPG.LogException(ex, $"An error occured while processing the status of {this}. The condition will be disabled.");
                disabled = true;
                return false;
            }
        }

        public IEnumerable<Transform> GetMarkerTargets()
        {
            if (!string.IsNullOrEmpty(MarkerEntry))
                yield return Locator.GetEntryLocation(MarkerEntry)?.GetTransform();
            if (!string.IsNullOrEmpty(MarkerPath))
                yield return UnityUtils.GetTransformAtPath(MarkerPath, null);
            if (DisableAutoMarkers) yield break;
            switch (Type)
            {
                case QuestConditionType.Fact:
                    foreach (var target in QuestManager.GetFactTargets(valueFact.GetID()))
                        yield return target;
                    break;
                case QuestConditionType.Entry:
                case QuestConditionType.EntryRumored:
                    yield return Locator.GetEntryLocation(valueEntry.GetID())?.GetTransform();
                    break;
                case QuestConditionType.Frequency:
                    foreach (var signal in Locator.GetAudioSignals().Where(s => s.GetFrequency() == valueFrequency))
                        yield return signal.transform;
                    break;
                case QuestConditionType.Signal:
                    foreach (var signal in Locator.GetAudioSignals().Where(s => s.GetName() == valueSignal))
                        yield return signal.transform;
                    break;
                case QuestConditionType.Conversation:
                    var talker = UnityUtils.GetTransformAtPath(Value, null);
                    if (talker != null)
                    {
                        var convo = talker.GetComponentInChildren<CharacterDialogueTree>(true);
                        if (convo == null)
                            convo = talker.GetComponentInParent<CharacterDialogueTree>();
                        if (convo != null)
                            yield return convo._attentionPoint;
                        else
                            yield return talker;
                    }
                    break;
                case QuestConditionType.HaveDrop:
                    foreach (var pickup in DropManager.FindDropPickups(valueDrop))
                        yield return pickup.transform;
                    break;
                case QuestConditionType.Special:
                    switch (SpecialConditionType)
                    {
                        case QuestSpecialConditionType.LaunchCodesEntered:
                            yield return UnityUtils.GetTransformAtPath("TimberHearth_Body/Sector_TH/Sector_Village/Interactables_Village/LaunchTower/Launch_Tower/ElevatorController/Elevator/Props_HEA_ControlPanel_Anim", null);
                            break;
                        case QuestSpecialConditionType.CompleteShipIgnition:
                            yield return UnityUtils.GetTransformAtPath("Ship_Body/Module_Cockpit/Systems_Cockpit/CockpitAttachPoint", null);
                            break;
                        case QuestSpecialConditionType.SatelliteRepair0:
                            yield return UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:0", null);
                            break;
                        case QuestSpecialConditionType.SatelliteRepair1:
                            yield return UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:1", null);
                            break;
                        case QuestSpecialConditionType.SatelliteRepair2:
                            yield return UnityUtils.GetTransformAtPath("MiningRig_Body/Nodes/BrokenNode:2", null);
                            break;
                        case QuestSpecialConditionType.EnterRemoteFlightConsole:
                            yield return UnityUtils.GetTransformAtPath("TimberHearth_Body/Sector_TH/Sector_Village/Interactables_Village/ModelRocket_Station/ModelRocketSpawn", null);
                            break;
                        case QuestSpecialConditionType.SuitUp:
                            yield return UnityUtils.GetTransformAtPath("Ship_Body/Module_Supplies/Systems_Supplies/ExpeditionGear/InteractVolume", null);
                            break;
                    }
                    break;
            }
        }

        public override string ToString() => $"{Step.FullID} (Condition: {Type}, Value: {Value})";
    }
}
