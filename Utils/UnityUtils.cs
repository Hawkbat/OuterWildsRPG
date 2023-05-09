using OWML.ModHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace OuterWildsRPG.Utils
{
    public static class UnityUtils
    {
        public static void RunWhen(Func<bool> criteria, Action action)
        {
            OuterWildsRPG.Instance.StartCoroutine(DoRunWhen(criteria, action));
        }

        static IEnumerator DoRunWhen(Func<bool> criteria, Action action)
        {
            while (!criteria()) yield return null;
            action();
        }

        public static void RunAfterSeconds(float seconds, Action action, bool realTime = true)
        {
            OuterWildsRPG.Instance.StartCoroutine(DoRunAfterSeconds(seconds, action, realTime));
        }

        static IEnumerator DoRunAfterSeconds(float seconds, Action action, bool realTime)
        {
            if (realTime) yield return new WaitForSecondsRealtime(seconds);
            else yield return new WaitForSeconds(seconds);
            action();
        }

        public static IEnumerable<Transform> GetChildren(Transform t)
        {
            for (int i = 0; i < t.childCount; i++) yield return t.GetChild(i);
        }

        public static string GetTransformPath(Transform t)
        {
            if (t == null) return null;
            if (t.parent) return GetTransformPath(t.parent) + "/" + t.name;
            return t.name;
        }

        public static Transform GetTransformAtPath(string path, string errorMessage)
            => GetTransformAtPath(null, path, errorMessage);

        public static Transform GetTransformAtPath(Transform root, string path, string errorMessage)
        {
            try
            {
                return GetTransformAtPathUnsafe(path, root);
            } catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    OuterWildsRPG.LogException(ex, errorMessage);
                }
            }
            return null;
        }

        public static Transform GetTransformAtPathUnsafe(string path)
            => GetTransformAtPathUnsafe(path, null);

        public static Transform GetTransformAtPathUnsafe(string path, Transform root)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Tried to look up path but path was null");
            var parts = new Queue<string>(path.Split('/'));
            if (root == null)
            {
                var rootName = parts.Dequeue();
                var rootObj = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == rootName);
                if (!rootObj) throw new Exception($"Could not find root object of path: {path}");
                root = rootObj.transform;
            }
            var t = root;

            while (parts.Count > 0)
            {
                var part = parts.Dequeue();
                if (part == "..")
                {
                    t = t.parent;
                } else if (part == ".")
                {

                } else if (part.Contains(":"))
                {
                    var bits = part.Split(':');
                    var search = bits[0];
                    var index = int.Parse(bits[1]);

                    var child = GetChildren(t).Where(c => c.name == search).ElementAt(index);
                    t = child;
                }
                else
                {
                    t = t.Find(part);
                }
                if (t == null) throw new Exception($"Could not find \"{part}\" in path: {path}");
            }
            return t;
        }

        public static string RichTextColor(string s, Color32 color)
        {
            return $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}>{s}</color>";
        }

        public static RectTransform MakeRectChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.parent = parent;
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            return rt;
        }

        public static Transform PlaceProp(Transform prop, IPropLike data)
        {
            var parent = GetTransformAtPathUnsafe(data.ParentPath);
            prop.parent = parent;
            if (data.IsRelativeToParent)
            {
                prop.localPosition = data.Position;
                prop.localEulerAngles = data.Rotation;
            }
            else
            {
                prop.position = parent.root.TransformPoint(data.Position);
                prop.rotation = parent.root.rotation * Quaternion.Euler(data.Rotation);
            }
            if (data.AlignRadial)
            {
                prop.up = (prop.position - parent.root.position).normalized;
            }
            foreach (var component in prop.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                if (component is SectoredMonoBehaviour sectoredBehavior)
                    sectoredBehavior.SetSector(prop.GetComponentInParent<Sector>() ?? prop.transform.root.GetComponentInChildren<Sector>());
                else if (component is InteractVolume interactVolume)
                    interactVolume._playerCam = Locator.GetPlayerCamera();
                else if (component is DestroyOnEnterTrigger destroyOnEnterTrigger)
                    GameObject.Destroy(destroyOnEnterTrigger);
                else if (component is Collider collider)
                    collider.enabled = true;
                else if (component is Renderer renderer)
                    renderer.enabled = true;
                else if (component is Shape shape)
                    shape.enabled = true;
            }
            return prop;
        }

        public static ColorData ToData(this Color c) => new() { r = c.r, g = c.g, b = c.b, a = c.a };
        public static Color ToColor(this ColorData c) => c != null ? new Color(c.r, c.g, c.b, c.a) : Color.white;
        public static Vector3Data ToData(this Vector3 v) => new() { x = v.x, y = v.y, z = v.z };
        public static Vector3 ToVector3(this Vector3Data v) => v != null ? new Vector3(v.x, v.y, v.z) : Vector3.zero;
    }
}
