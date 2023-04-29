using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public abstract class BuiltElement : MonoBehaviour
    {
        public RectTransform rectTransform;

        bool isBuilt;
        bool needsRebuild;

        public abstract void Setup();

        public abstract void Cleanup();

        public abstract void Rebuild();

        public abstract bool Animate();

        public bool IsBuilt() => isBuilt;

        public void TriggerRebuild(bool immediate = false)
        {
            if (immediate)
            {
                needsRebuild = false;
                Rebuild();
                isBuilt = true;
            }
            else
            {
                needsRebuild = true;
                enabled = true;
            }
        }

        public void TriggerAnimation()
        {
            enabled = true;
        }

        public T MakeComponent<T>(T existing) where T : Component
        {
            if (existing != null) return existing;
            var t = gameObject.AddComponent<T>();
            return t;
        }

        public T MakeChild<T>(T existing, string name) where T : Component
        {
            if (existing != null) Destroy(existing.gameObject);
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.parent = rectTransform;
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            var t = go.AddComponent<T>();
            return t;
        }

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Setup();
            TriggerRebuild();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        void Update()
        {
            if (needsRebuild)
            {
                Rebuild();
                isBuilt = true;
                needsRebuild = false;
            }
            if (!Animate())
            {
                enabled = false;
            }
        }
    }
}
