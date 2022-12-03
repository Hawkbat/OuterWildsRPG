using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG
{
    public static class ShipLogExporter
    {
        public class DataFile
        {
            public List<DataEntry> entries = new();
            public List<string> dialogueConditions = new();
            public List<string> persistentConditions = new();
        }

        public class DataEntry
        {
            public string id;
            public string name;
            public string location;
            public List<DataFact> facts = new();
        }

        public class DataFact
        {
            public string id;
            public string text;
            public string rumorName;
            public string rumorSource;
        }

        public static void Export()
        {
            var logFile = new DataFile();
            foreach (var entry in Locator.GetShipLogManager().GetEntryList())
            {
                var entryData = new DataEntry();
                entryData.id = entry.GetID();
                entryData.name = entry.GetName(false);
                var location = Locator.GetEntryLocation(entry.GetID());
                if (location != null)
                    entryData.location = Utils.GetTransformPath(location.GetTransform());

                foreach (var fact in entry.GetExploreFacts())
                {
                    var factData = new DataFact();
                    factData.id = fact.GetID();
                    factData.text = fact.GetText();
                    entryData.facts.Add(factData);
                }
                foreach (var fact in entry.GetRumorFacts())
                {
                    var factData = new DataFact();
                    factData.id = fact.GetID();
                    factData.text = fact.GetText();
                    factData.rumorName = fact.GetEntryRumorName();
                    factData.rumorSource = fact.GetSourceID();
                    entryData.facts.Add(factData);
                }
                logFile.entries.Add(entryData);
            }

            HashSet<string> dialogueConditions = new HashSet<string>();
            HashSet<string> persistentConditions = new HashSet<string>();

            foreach (var tree in UnityEngine.Object.FindObjectsOfType<CharacterDialogueTree>())
            {
                if (!tree.LoadXml())
                {
                    OuterWildsRPG.Instance.ModHelper.Console.WriteLine(Utils.GetTransformPath(tree.transform));
                }
                if (tree._mapDialogueNodes != null)
                {
                    foreach (var node in tree._mapDialogueNodes.Values)
                    {
                        foreach (var cond in node.ConditionsToSet)
                            dialogueConditions.Add(cond);
                        foreach (var cond in node.ListEntryCondition)
                            dialogueConditions.Add(cond);
                        foreach (var cond in node.ListTargetCondition)
                            dialogueConditions.Add(cond);
                        if (!string.IsNullOrEmpty(node.PersistentConditionToSet))
                            persistentConditions.Add(node.PersistentConditionToSet);
                        if (!string.IsNullOrEmpty(node.PersistentConditionToDisable))
                            persistentConditions.Add(node.PersistentConditionToDisable);
                    }
                }
                if (tree._listOptionNodes != null)
                {
                    foreach (var opt in tree._listOptionNodes)
                    {
                        if (!string.IsNullOrEmpty(opt.ConditionToSet))
                            dialogueConditions.Add(opt.ConditionToSet);
                        if (!string.IsNullOrEmpty(opt.ConditionRequirement))
                            dialogueConditions.Add(opt.ConditionRequirement);
                        if (!string.IsNullOrEmpty(opt.ConditionToCancel))
                            dialogueConditions.Add(opt.ConditionToCancel);
                        foreach (var cond in opt.PersistentCondition)
                            persistentConditions.Add(cond);
                    }
                }
            }

            logFile.dialogueConditions = dialogueConditions.ToList();
            logFile.persistentConditions = persistentConditions.ToList();

            OuterWildsRPG.Instance.ModHelper.Storage.Save(logFile, "shiplog.json");
        }
    }
}
