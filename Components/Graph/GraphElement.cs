using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components.Graph
{
    public abstract class GraphElement : MonoBehaviour
    {
        protected GraphMode graphMode;
        protected IGraphProvider graphProvider;

        [SerializeField] protected RectTransform rectTransform;

        protected bool isRevealAnimReady;
        protected bool updateRevealAnim;
        protected float startRevealTime;
        protected float revealDuration;
        protected bool hasFocus;
        bool hidden;

        public abstract string GetID();

        public bool IsHidden() => hidden;

        public bool IsVisible() => !hidden && gameObject.activeSelf;

        public bool IsRevealAnimationReady() => isRevealAnimReady;

        void Awake()
        {
            enabled = false;
        }

        protected void Init(GraphMode graphMode, IGraphProvider graphProvider)
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            this.graphMode = graphMode;
            this.graphProvider = graphProvider;

            isRevealAnimReady = false;
            updateRevealAnim = false;
            hasFocus = false;
            hidden = false;
        }

        protected void PostInit()
        {
            Refresh();
            enabled = false;
        }

        public void Show()
        {
            hidden = false;
            Refresh();
        }

        public void Hide()
        {
            hidden = true;
            Refresh();
        }

        public void Focus()
        {
            hasFocus = true;
            enabled = true;
            Refresh();
        }

        public void Unfocus()
        {
            hasFocus = false;
            Refresh();
        }

        public abstract bool CanSelect();

        public abstract bool AttemptSelect();

        public abstract bool AttemptDeselect();

        public abstract IEnumerable<string> GetDescription();

        public abstract bool CheckPointCollision(Vector3 point);

        public abstract void Refresh();

        public abstract bool ShouldPlayRevealAnimation();

        public void PrepareRevealAnimation()
        {
            isRevealAnimReady = ShouldPlayRevealAnimation();
            if (isRevealAnimReady)
            {
                StartRevealAnimation();
            }
        }
        protected abstract void StartRevealAnimation();

        public void PlayRevealAnimation(float duration)
        {
            if (!isRevealAnimReady) return;
            isRevealAnimReady = false;
            updateRevealAnim = true;
            startRevealTime = Time.unscaledTime;
            revealDuration = duration;
            enabled = true;
        }

        protected abstract void UpdateRevealAnimation(float t);

        protected abstract void FinishRevealAnimation();

        protected virtual bool Animate() => false;

        void Update()
        {
            if (updateRevealAnim)
            {
                var t = Mathf.InverseLerp(startRevealTime, startRevealTime + revealDuration, Time.unscaledTime);
                UpdateRevealAnimation(t);
                if (t >= 1f)
                {
                    updateRevealAnim = false;
                    FinishRevealAnimation();
                    Refresh();
                }
            }
            if (!updateRevealAnim && !Animate())
                enabled = false;
        }
    }
}
