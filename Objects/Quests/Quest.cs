using System;
using System.Collections.Generic;
using System.Linq;
using OuterWildsRPG.Utils;
using UnityEngine;
using OWML.Common;
using OWML.Utils;
using OuterWildsRPG.Enums;

namespace OuterWildsRPG.Objects.Quests
{
    public class Quest : Entity<Quest, QuestData>
    {
        public QuestType Type;
        public List<QuestStep> Steps = new();

        public bool IsTracked => QuestManager.IsTrackingQuest(this);

        public bool IsStarted => QuestManager.HasStartedQuest(this);
        public bool IsInProgress => !IsComplete && IsStarted;
        public bool IsComplete => QuestManager.HasCompletedQuest(this);

        public float StartedTime => startedTime;
        public float CompletedTime => completedTime;

        private float startedTime;
        private float completedTime;

        public override void Load(QuestData data, string modID)
        {
            base.Load(data, modID);
            Type = data.type;
            Steps = data.steps.Select(s => QuestStep.LoadNew(s, modID)).ToList();
            foreach (var step in Steps)
                step.Quest = this;
        }

        public override void Resolve(QuestData data, Dictionary<string, Quest> entities)
        {

        }

        public void SetUp()
        {
            foreach (var step in Steps) step.SetUp();
        }

        public void CleanUp()
        {
            foreach (var step in Steps) step.CleanUp();
        }

        public void Update(bool markersVisible)
        {
            CalculateStatus();

            var ownMarkersVisible = markersVisible && IsStarted && IsTracked;

            foreach (var step in Steps) step.Update(ownMarkersVisible);
        }

        public void CalculateStatus()
        {
            bool shouldStart = Steps.Any(s => s.IsStarted);
            if (shouldStart && !QuestManager.HasStartedQuest(this))
            {
                QuestManager.StartQuest(this);
                startedTime = Time.time;
            }

            bool shouldComplete = Steps.All(s => s.IsComplete || s.Optional);
            if (shouldComplete && !QuestManager.HasCompletedQuest(this))
            {
                QuestManager.CompleteQuest(this);
                completedTime = Time.time;
            }
        }
    }
}
