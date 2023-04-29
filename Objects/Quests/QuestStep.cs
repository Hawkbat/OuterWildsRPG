using Epic.OnlineServices.Platform;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;
using OuterWildsRPG.Utils;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Quests
{
    public class QuestStep : EntityLike<QuestStep, QuestStepData>
    {
        public Quest Quest;

        public string ID;
        public string Text;
        public List<QuestCondition> StartOn = new();
        public QuestConditionMode StartMode;
        public List<QuestCondition> CompleteOn = new();
        public QuestConditionMode CompleteMode;
        public string LocationEntry;
        public string LocationPath;
        public bool Optional;
        public bool IsHint;
        public bool PreventEarlyComplete;

        public bool IsStarted => QuestManager.HasStartedStep(this);
        public bool IsInProgress => IsStarted && !IsComplete;
        public bool IsComplete => QuestManager.HasCompletedStep(this);

        public float StartedTime => startedTime;
        public float CompletedTime => completedTime;

        private CanvasMarker canvasMarker;
        private CanvasMapMarker mapMarker;
        private Transform markerTarget;

        private float startedTime;
        private float completedTime;

        public override void Load(QuestStepData data, string modID)
        {
            Text = data.text;
            StartOn = data.startOn.Select(c => QuestCondition.LoadNew(c, modID)).ToList();
            StartMode = data.startMode;
            CompleteOn = data.completeOn.Select(c => QuestCondition.LoadNew(c, modID)).ToList();
            CompleteMode = data.completeMode;
            LocationEntry = data.locationEntry;
            LocationPath = data.locationPath;
            Optional = data.optional;
            IsHint = data.isHint;
            PreventEarlyComplete = data.preventEarlyComplete;
            foreach (var c in StartOn) c.Step = this;
            foreach (var c in CompleteOn) c.Step = this;
        }

        public void SetUp()
        {
            if (canvasMarker == null && mapMarker == null)
            {
                try
                {
                    markerTarget = null;

                    if (!string.IsNullOrEmpty(LocationEntry))
                    {
                        markerTarget = Locator.GetEntryLocation(LocationEntry).GetTransform();
                    }
                    else if (!string.IsNullOrEmpty(LocationPath))
                    {
                        markerTarget = UnityUtils.GetTransformAtPath(LocationPath);
                    } else if (StartOn.Any(c => c.Type == QuestConditionType.HaveDrop) || CompleteOn.Any(c => c.Type == QuestConditionType.HaveDrop))
                    {
                        try
                        {
                            var condition = StartOn.Find(c => c.Type == QuestConditionType.HaveDrop) ?? CompleteOn.Find(c => c.Type == QuestConditionType.HaveDrop);
                            var drop = DropManager.GetDrop(condition.Value, OuterWildsRPG.ModID);
                            var pickup = DropManager.FindDropPickup(drop);
                            markerTarget = pickup.transform;
                        } catch (Exception ex)
                        {
                            OuterWildsRPG.Instance.ModHelper.Console.WriteLine($"Failed to locate applicable item drop for {Quest.ID}:{ID}", MessageType.Warning);
                            OuterWildsRPG.Instance.ModHelper.Console.WriteLine(ex.Message, MessageType.Error);
                        }
                    }

                    if (markerTarget != null)
                    {
                        canvasMarker = Locator.GetMarkerManager().InstantiateNewMarker();
                        Locator.GetMarkerManager().RegisterMarker(canvasMarker, markerTarget, Text, 0f);
                        mapMarker = Locator.GetMapController().GetMarkerManager().InstantiateNewMarker(true);
                        mapMarker.SetLabel(Text);
                        Locator.GetMapController().GetMarkerManager().RegisterMarker(mapMarker, markerTarget);
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
            if (Locator.GetMarkerManager() && canvasMarker != null)
                Locator.GetMarkerManager().UnregisterMarker(canvasMarker);
            canvasMarker = null;

            if (Locator.GetMapController()?.GetMarkerManager() && mapMarker != null)
                Locator.GetMapController().GetMarkerManager().UnregisterMarker(mapMarker);
            mapMarker = null;

            foreach (var cond in StartOn) cond.CleanUp();
            foreach (var cond in CompleteOn) cond.CleanUp();
        }

        public void Update(bool markersVisible)
        {
            CalculateStatus(null);

            var ownMarkersVisible = markersVisible && IsInProgress && markerTarget && markerTarget.gameObject.activeSelf;

            if (canvasMarker)
            {
                canvasMarker.SetVisibility(ownMarkersVisible);
            }
            if (mapMarker)
            {
                mapMarker.SetVisibility(ownMarkersVisible);
            }
        }

        public void CalculateStatus(QuestCondition triggeringCondition)
        {
            bool shouldStart = AreStartConditionsMet(triggeringCondition) || AreCompletionConditionsMet(triggeringCondition);
            if (shouldStart && !IsStarted)
            {
                startedTime = Time.time;
                QuestManager.StartStep(this);
            }

            bool shouldComplete = (!PreventEarlyComplete || AreStartConditionsMet(triggeringCondition)) && AreCompletionConditionsMet(triggeringCondition);
            if (shouldComplete && !IsComplete)
            {
                completedTime = Time.time;
                QuestManager.CompleteStep(this);

            }
        }

        bool AreStartConditionsMet(QuestCondition triggeringCondition)
        {
            if (StartOn.Count == 0)
                return QuestManager.HasStartedQuest(Quest);
            if (StartMode == QuestConditionMode.Any)
                return StartOn.Any(c => c.Check() || c == triggeringCondition);
            else if (StartMode == QuestConditionMode.All)
                return StartOn.All(c => c.Check() || c == triggeringCondition);
            return false;
        }

        bool AreCompletionConditionsMet(QuestCondition triggeringCondition)
        {
            if (CompleteOn.Count == 0)
                return QuestManager.HasCompletedQuest(Quest);
            if (CompleteMode == QuestConditionMode.Any)
                return CompleteOn.Any(c => c.Check() || c == triggeringCondition);
            else if (CompleteMode == QuestConditionMode.All)
                return CompleteOn.All(c => c.Check() || c == triggeringCondition);
            return false;
        }
    }
}
