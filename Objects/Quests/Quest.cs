using System;
using System.Collections.Generic;
using System.Linq;
using OuterWildsRPG.Utils;
using UnityEngine;
using OWML.Common;
using OWML.Utils;
using OuterWildsRPG.Enums;
using OuterWildsRPG.Objects.Drops;

namespace OuterWildsRPG.Objects.Quests
{
    public class Quest : Entity<Quest, QuestData>
    {
        public QuestType Type;
        public QuestTheme Theme;
        public List<QuestStep> Steps = new();

        public bool IsTracked => QuestManager.IsTrackingQuest(this);

        public bool IsTrackedManual => QuestManager.IsTrackingQuest(this, false);

        public bool IsStarted => QuestManager.HasStartedQuest(this);
        public bool IsInProgress => !IsComplete && IsStarted;
        public bool IsComplete => QuestManager.HasCompletedQuest(this);

        public float StartedTime => startedTime;
        public float CompletedTime => completedTime;

        private float startedTime;
        private float completedTime;

        List<Sector> autoTrackSectors = new();

        public override void Load(QuestData data, string modID)
        {
            base.Load(data, modID);
            Type = data.type;
            Theme = data.theme;
            Steps = data.steps.Select(s => QuestStep.LoadNew(s, modID)).ToList();
            foreach (var step in Steps)
                step.Quest = this;
        }

        public override void Resolve()
        {
            base.Resolve();
            foreach (var step in Steps) step.Resolve();
        }

        public void SetUp()
        {
            foreach (var step in Steps) step.SetUp();

            foreach (var step in Steps)
            {
                foreach (var cond in step.GetConditions())
                {
                    foreach (var target in cond.GetMarkerTargets().Where(t => t != null))
                    {
                        var sector = target.GetComponentInParent<Sector>();
                        if (sector == null)
                        {
                            sector = target.root.GetComponentInChildren<Sector>();
                        }
                        if (sector != null && !autoTrackSectors.Contains(sector))
                        {
                            sector.OnOccupantEnterSector.AddListener(OnOccupantEnterSector);
                            sector.OnOccupantExitSector.AddListener(OnOccupantExitSector);

                            foreach (var occupant in sector.GetOccupants())
                                OnOccupantEnterSector(occupant);

                            autoTrackSectors.Add(sector);
                        }
                    }
                }
            }
        }

        public void CleanUp()
        {
            foreach (var step in Steps) step.CleanUp();

            foreach (var sector in autoTrackSectors)
            {
                if (sector == null) continue;
                sector.OnOccupantEnterSector.RemoveListener(OnOccupantEnterSector);
                sector.OnOccupantExitSector.RemoveListener(OnOccupantExitSector);
            }
            autoTrackSectors.Clear();
        }

        public void Update(bool markersVisible)
        {
            var ownMarkersVisible = markersVisible && IsStarted && IsTracked;

            foreach (var step in Steps) step.Update(ownMarkersVisible);

            CalculateStatus();
        }

        public void CalculateStatus()
        {
            bool shouldStart = Steps.Any(s => s.IsStarted);
            if (shouldStart && !QuestManager.HasStartedQuest(this))
            {
                QuestManager.StartQuest(this);
                startedTime = Time.unscaledTime;
            }

            bool shouldComplete = Steps.All(s => s.IsComplete || s.Optional);
            if (shouldComplete && !QuestManager.HasCompletedQuest(this))
            {
                QuestManager.CompleteQuest(this);
                completedTime = Time.unscaledTime;
            }
        }

        public Transform GetQuestGiver()
        {
            var markerTargets = Steps
                .SelectMany(s => s.StartOn.Concat(s.CompleteOn))
                .SelectMany(c => c.GetMarkerTargets())
                .Where(t => t != null);
            foreach (var c in markerTargets)
            {
                var dialogue = c.GetComponentInParent<CharacterDialogueTree>();
                if (dialogue == null)
                    dialogue = c.GetComponentInChildren<CharacterDialogueTree>(true);
                if (dialogue == null) {
                    foreach (Transform t in c.parent)
                    {
                        dialogue = t.GetComponentInChildren<CharacterDialogueTree>(true);
                        if (dialogue != null) break;
                    }
                }
                if (dialogue != null)
                {
                    return c;
                }
            }
            return null;
        }

        int occupantCount = 0;

        void OnOccupantEnterSector(SectorDetector detector)
        {
            occupantCount++;
            if (occupantCount > 0 && IsInProgress)
                QuestManager.SetTrackingQuest(this, true, true);
        }

        void OnOccupantExitSector(SectorDetector detector)
        {
            occupantCount--;
            if (occupantCount <= 0)
                QuestManager.SetTrackingQuest(this, false, true);
        }
    }
}
