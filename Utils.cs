using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace OuterWildsRPG
{
    public static class Utils
    {
        public static void RunAfterSeconds(float seconds, Action action)
        {
            var timeout = Time.time + seconds;
            OuterWildsRPG.Instance.ModHelper.Events.Unity.RunWhen(() => Time.time > timeout, action);
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
                } else
                {
                    t = t.Find(part);
                }
                if (t == null) throw new Exception($"Could not find \"{part}\" in path: {path}");
            }
            return t;
        }
    }
}
