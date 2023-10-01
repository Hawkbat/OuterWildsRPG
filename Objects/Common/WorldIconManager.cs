using OuterWildsRPG.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Objects.Common
{
    public static class WorldIconManager
    {
        static readonly List<WorldIconTarget> targets = new();

        public static IEnumerable<WorldIconTarget> GetAllTargets() => targets;

        public static WorldIconTarget MakeTarget(Transform t)
        {
            var existing = t.GetComponentInParent<WorldIconTarget>();
            if (existing != null) return existing;
            var newTarget = t.gameObject.AddComponent<WorldIconTarget>();
            targets.Add(newTarget);
            return newTarget;
        }

        public static void SetUp()
        {
            foreach (var dialogueTree in GameObject.FindObjectsOfType<CharacterDialogueTree>())
            {
                MakeTarget(dialogueTree.transform);
            }
            foreach (var nomaiText in GameObject.FindObjectsOfType<NomaiText>())
            {
                MakeTarget(nomaiText.transform);
            }
        }

        public static void CleanUp()
        {
            targets.Clear();
        }
    }
}
