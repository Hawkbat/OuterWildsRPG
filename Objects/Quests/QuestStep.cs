using Epic.OnlineServices.Platform;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Common;
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
    public class QuestStep : EntityLike<QuestStep, QuestStepData>, IDisplayable
    {
        public Quest Quest;

        public string ID;
        public string Text;
        public List<QuestCondition> StartOn = new();
        public QuestConditionMode StartMode;
        public List<QuestCondition> CompleteOn = new();
        public QuestConditionMode CompleteMode;
        public bool Optional;
        public bool IsHint;
        public bool PreventEarlyComplete;

        public string FullID => $"{Quest.FullID}:{ID}";

        public bool IsStarted => QuestManager.HasStartedStep(this);
        public bool IsInProgress => IsStarted && !IsComplete;
        public bool IsComplete => QuestManager.HasCompletedStep(this);

        public float StartedTime => startedTime;
        public float CompletedTime => completedTime;

        public int GetStartConditionProgress() => StartOn.Where(c => c.Check()).Count();
        public int GetCompletionConditionProgress() => CompleteOn.Where(c => c.Check()).Count();

        public IEnumerable<QuestCondition> GetConditions()
        {
            foreach (var c in StartOn) yield return c;
            foreach (var c in CompleteOn) yield return c;
        }

        private int startProgress;
        private float startedTime;
        private int completionProgress;
        private float completedTime;

        public override void Load(QuestStepData data, string modID)
        {
            base.Load(data, modID);
            ID = data.id;
            Text = data.text;
            StartOn = data.startOn.Select(c => QuestCondition.LoadNew(c, modID)).ToList();
            StartMode = data.startMode;
            CompleteOn = data.completeOn.Select(c => QuestCondition.LoadNew(c, modID)).ToList();
            CompleteMode = data.completeMode;
            Optional = data.optional;
            IsHint = data.isHint;
            PreventEarlyComplete = data.preventEarlyComplete;
            foreach (var c in StartOn) c.Step = this;
            foreach (var c in CompleteOn) c.Step = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            foreach (var c in GetConditions()) c.Resolve();
            TranslationUtils.RegisterGeneral(FullID, Text);
        }

        public void SetUp()
        {
            foreach (var c in GetConditions()) c.SetUp();
        }

        public void CleanUp()
        {
            foreach (var c in GetConditions()) c.CleanUp();

            startProgress = 0;
            completionProgress = 0;
        }

        public void Update(bool markersVisible)
        {
            var ownMarkersVisible = markersVisible && IsInProgress;

            foreach (var c in GetConditions()) c.Update(ownMarkersVisible);

            CalculateStatus(null);
        }

        public void CalculateStatus(QuestCondition triggeringCondition)
        {
            bool shouldComplete = (!PreventEarlyComplete || AreStartConditionsMet(triggeringCondition)) && AreCompletionConditionsMet(triggeringCondition);
            bool shouldStart = AreStartConditionsMet(triggeringCondition) || shouldComplete;

            var newStartProgress = GetStartConditionProgress();
            var newCompletionProgress = GetCompletionConditionProgress();

            if (newStartProgress != startProgress)
            {
                startProgress = newStartProgress;
                QuestManager.ProgressStep(this);
            }
            if (newCompletionProgress != completionProgress)
            {
                completionProgress = newCompletionProgress;
                QuestManager.ProgressStep(this);
            }
            
            if (shouldStart && !IsStarted)
            {
                startedTime = Time.unscaledTime;
                QuestManager.StartStep(this);
            }
            if (shouldComplete && !IsComplete)
            {
                completedTime = Time.unscaledTime;
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

        public string ToDisplayString(bool richText = true) => TranslationUtils.GetGeneral(FullID);

        public override string ToString() => FullID;
    }
}
