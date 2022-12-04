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
            if (t.parent) return GetTransformPath(t.parent) + "/" + t.name;
            return t.name;
        }

        public static Transform GetTransformAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new Exception("Tried to look up path but path was null");
            var parts = path.Split('/');
            var root = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == parts[0]);
            if (!root) throw new Exception($"Could not find root object of path: {path}");
            var t = root.transform;
            foreach (var part in parts.Skip(1))
            {
                if (part.Contains(":"))
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
    }
}
