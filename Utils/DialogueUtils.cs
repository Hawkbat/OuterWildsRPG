using OuterWildsRPG.Objects.Common.Dialogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace OuterWildsRPG.Utils
{
    public static class DialogueUtils
    {
        public static void Clear(CharacterDialogueTree tree)
        {
            if (!tree._initialized)
            {
                LateInitializerManager.UnregisterLateInitializer(tree);
            }
            tree._xmlCharacterDialogueAsset = null;
            tree._characterName = string.Empty;
            if (tree._mapDialogueNodes != null)
                tree._mapDialogueNodes.Clear();
            else
                tree._mapDialogueNodes = new Dictionary<string, DialogueNode>();
            if (tree._listOptionNodes != null)
                tree._listOptionNodes.Clear();
            else
                tree._listOptionNodes = new();
            tree._initialized = true;
        }

        public static void Merge(CharacterDialogueTree tree, DialogueTreeData data)
        {
            if (data == null) return;
            if (!tree._initialized) tree.LoadXml();
            if (!string.IsNullOrEmpty(data.characterName))
            {
                ChangeName(tree, data.characterName);
            }
            if (data.nodes.Any())
            {
                var convertedNodes = data.nodes.Select(ConvertNode);
                AddOrReplaceNodes(tree, convertedNodes);
            }
        }

        public static void ChangeName(CharacterDialogueTree tree, string name)
        {
            if (!tree._initialized) tree.LoadXml();
            tree._characterName = name;
            foreach (var option in tree._listOptionNodes)
            {
                var node = tree._mapDialogueNodes.Values.FirstOrDefault(n => n.ListDialogueOptions.Contains(option));
                if (node != null)
                {
                    option.SetNodeId(node.Name, name);
                }
            }
        }

        public static void AddOrReplaceNodes(CharacterDialogueTree tree, IEnumerable<DialogueNode> nodes)
        {
            if (!tree._initialized) tree.LoadXml();
            nodes = nodes.Select(n =>
            {
                if (tree._mapDialogueNodes.TryGetValue(n.Name, out var source)) {
                    return MergeNodes(source, n);
                }
                return n;
            }).ToList();

            foreach (var node in nodes)
            {
                tree._listOptionNodes.RemoveAll(o => o._textID == $"{tree._characterName}{node.Name}{o._text}");
            }

            foreach (var node in nodes)
            {
                foreach (var entryCondition in node.ListEntryCondition)
                    AddCondition(entryCondition);
                foreach (var option in node.ListDialogueOptions)
                {
                    option.SetNodeId(node.Name, tree._characterName);
                    AddCondition(option.ConditionRequirement);
                    AddCondition(option.CancelledRequirement);
                    AddCondition(option.ConditionToSet);
                    AddCondition(option.ConditionToCancel);
                    tree._listOptionNodes.Add(option);
                }

                tree._mapDialogueNodes[node.Name] = node;
            }

            foreach (var node in tree._mapDialogueNodes.Values)
            {
                if (node.Target != null && string.IsNullOrEmpty(node.TargetName))
                {
                    node.TargetName = node.Target.Name;
                } else if (node.Target == null && !string.IsNullOrEmpty(node.TargetName))
                {
                    if (tree._mapDialogueNodes.TryGetValue(node.TargetName, out var target))
                    {
                        node.Target = target;
                    }
                }
            }
        }

        public static void RemoveNodes(CharacterDialogueTree tree, IEnumerable<DialogueNode> nodes)
        {
            if (!tree._initialized) tree.LoadXml();
            foreach (var node in nodes)
            {
                tree._mapDialogueNodes.Remove(node.Name);
                foreach (var option in node.ListDialogueOptions)
                {
                    tree._listOptionNodes.Remove(option);
                }
            }
            foreach (var remainingNode in tree._mapDialogueNodes.Values)
            {
                if (nodes.Any(n => remainingNode.Target == n || remainingNode.TargetName == n.Name))
                    remainingNode.Target = null;
            }
        }

        static void AddCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return;
            if (!DialogueConditionManager.SharedInstance.ConditionExists(condition))
                DialogueConditionManager.SharedInstance.AddCondition(condition);
        }

        public static DialogueNode MergeNodes(DialogueNode source, DialogueNode merge)
        {
            return new DialogueNode()
            {
                Name = MergeStrings(source.Name, merge.Name),
                DisplayTextData = new DialogueText(Enumerable.Empty<XElement>(), merge.DisplayTextData._randomize || source.DisplayTextData._randomize)
                {
                    _listTextBlocks = MergeLists(source.DisplayTextData._listTextBlocks, merge.DisplayTextData._listTextBlocks),
                },
                TargetName = MergeStrings(source.TargetName, merge.TargetName),
                Target = merge.Target ?? source.Target,
                ListEntryCondition = MergeLists(source.ListEntryCondition, merge.ListEntryCondition),
                ListTargetCondition = MergeLists(source.ListTargetCondition, merge.ListTargetCondition),
                ConditionsToSet = MergeLists(source.ConditionsToSet, merge.ConditionsToSet),
                PersistentConditionToSet = MergeStrings(source.PersistentConditionToSet, merge.PersistentConditionToSet),
                PersistentConditionToDisable = MergeStrings(source.PersistentConditionToDisable, merge.PersistentConditionToDisable),
                DBEntriesToSet = MergeLists(source.DBEntriesToSet, merge.DBEntriesToSet).ToArray(),
                ListDialogueOptions = MergeLists(source.ListDialogueOptions, merge.ListDialogueOptions),
            };
        }



        public static DialogueTreeData ConvertTree(string xml)
            => ConvertTree(XDocument.Parse(OWUtilities.RemoveByteOrderMark(new TextAsset(xml))).Element("DialogueTree"));

        public static DialogueTreeData ConvertTree(XElement xml)
        {
            return new DialogueTreeData
            {
                characterName = xml.Element("NameField")?.Value,
                nodes = xml.Elements("DialogueNode").Select(ConvertNode).ToList()
            };
        }

        public static DialogueTreeData ConvertTree(CharacterDialogueTree data)
        {
            return new DialogueTreeData
            {
                characterName = data._characterName,
                turnOnFlashlight = data._turnOnFlashlight,
                turnOffFlashlight = data._turnOffFlashlight,
                attentionPointPath = UnityUtils.GetTransformPath(data._attentionPoint),
                attentionPointOffset = data._attentionPointOffset.ToData(),
                nodes = data._mapDialogueNodes.Values.Select(ConvertNode).ToList(),
            };
        }

        public static DialogueNodeData ConvertNode(string xml)
            => ConvertNode(XDocument.Parse(OWUtilities.RemoveByteOrderMark(new TextAsset(xml))).Element("DialogueNode"));

        public static DialogueNodeData ConvertNode(XElement xml)
        {
            return new DialogueNodeData
            {
                name = xml.Element("Name")?.Value,
                entryConditions = xml.Elements("EntryCondition").Select(e => e.Value).ToList(),
                setConditions = xml.Elements("SetCondition").Select(e => e.Value).ToList(),
                setPersistentCondition = xml.Element("SetPersistentCondition")?.Value,
                cancelPersistentCondition = xml.Element("DisablePersistentCondition")?.Value,
                revealFacts = xml.Elements("RevealFacts").SelectMany(e => e.Elements("FactID")).Select(e => e.Value).ToList(),
                randomize = xml.Element("Randomize") != null,
                dialogues = xml.Elements("Dialogue").Select(ConvertText).ToList(),
                targetShipLogConditions = xml.Elements("DialogueTargetShipLogCondition").Select(e => e.Value).ToList(),
                target = xml.Element("DialogueTarget")?.Value,
                options = xml.Elements("DialogueOptionsList").SelectMany(e => e.Elements("DialogueOption")).Select(ConvertOption).ToList()
            };
        }

        public static DialogueNode ConvertNode(DialogueNodeData data)
        {
            return new DialogueNode()
            {
                Name = data.name ?? string.Empty,
                DisplayTextData = ConvertText(data.dialogues, data.randomize),
                TargetName = data.target ?? string.Empty,
                ListEntryCondition = data.entryConditions,
                ListTargetCondition = data.targetShipLogConditions,
                ConditionsToSet = data.setConditions,
                PersistentConditionToSet = data.setPersistentCondition ?? string.Empty,
                PersistentConditionToDisable = data.cancelPersistentCondition ?? string.Empty,
                DBEntriesToSet = data.revealFacts.ToArray(),
                ListDialogueOptions = data.options.Select(ConvertOption).ToList(),
            };
        }

        public static DialogueNodeData ConvertNode(DialogueNode data)
        {
            return new DialogueNodeData()
            {
                name = data.Name ?? string.Empty,
                dialogues = ConvertText(data.DisplayTextData),
                randomize = data.DisplayTextData._randomize,
                target = data.TargetName ?? string.Empty,
                entryConditions = data.ListEntryCondition,
                targetShipLogConditions = data.ListTargetCondition,
                setConditions = data.ConditionsToSet,
                setPersistentCondition = data.PersistentConditionToSet ?? string.Empty,
                cancelPersistentCondition = data.PersistentConditionToDisable ?? string.Empty,
                revealFacts = data.DBEntriesToSet.ToList(),
                options = data.ListDialogueOptions.Select(ConvertOption).ToList(),
            };
        }

        public static DialogueOptionData ConvertOption(string xml)
            => ConvertOption(XDocument.Parse(OWUtilities.RemoveByteOrderMark(new TextAsset(xml))).Element("DialogueOption"));

        public static DialogueOptionData ConvertOption(XElement xml)
        {
            return new DialogueOptionData
            {
                text = OWUtilities.CleanupXmlText(xml.Element("Text")?.Value ?? "", false),
                target = xml.Element("DialogueTarget")?.Value,
                requiredCondition = xml.Element("RequiredCondition")?.Value,
                cancelledCondition = xml.Element("CancelledCondition")?.Value,
                requiredPersistentConditions = xml.Elements("RequiredPersistentCondition").Select(e => e.Value).ToList(),
                cancelledPersistentConditions = xml.Elements("CancelledPersistentCondition").Select(e => e.Value).ToList(),
                requiredFacts = xml.Elements("RequiredLogCondition").Select(e => e.Value).ToList(),
                setCondition = xml.Element("ConditionToSet")?.Value,
                cancelCondition = xml.Element("ConditionToCancel")?.Value
            };
        }

        public static DialogueOption ConvertOption(DialogueOptionData data)
        {
            return new DialogueOption()
            {
                Text = data.text ?? string.Empty,
                TargetName = data.target ?? string.Empty,
                ConditionRequirement = data.requiredCondition ?? string.Empty,
                PersistentCondition = data.requiredPersistentConditions,
                CancelledRequirement = data.cancelledCondition ?? string.Empty,
                CancelledPersistentRequirement = data.cancelledPersistentConditions,
                LogConditionRequirement = data.requiredFacts,
                ConditionToSet = data.setCondition ?? string.Empty,
                ConditionToCancel = data.cancelCondition ?? string.Empty,
            };
        }

        public static DialogueOptionData ConvertOption(DialogueOption data)
        {
            return new DialogueOptionData()
            {
                text = data._text ?? string.Empty,
                target = data.TargetName ?? string.Empty,
                requiredCondition = data.ConditionRequirement ?? string.Empty,
                requiredPersistentConditions = data.PersistentCondition,
                cancelledCondition = data.CancelledRequirement ?? string.Empty,
                cancelledPersistentConditions = data.CancelledPersistentRequirement,
                requiredFacts = data.LogConditionRequirement,
                setCondition = data.ConditionToSet ?? string.Empty,
                cancelCondition = data.ConditionToCancel ?? string.Empty,
            };
        }

        public static DialogueTextData ConvertText(string xml)
            => ConvertText(XDocument.Parse(OWUtilities.RemoveByteOrderMark(new TextAsset(xml))).Element("Dialogue"));

        public static DialogueTextData ConvertText(XElement xml)
        {
            return new DialogueTextData
            {
                pages = xml.Elements("Page").Select(e => CleanUpText(e.Value)).ToList(),
                condition = xml.Element("RequiredCondition")?.Value
            };
        }

        static string CleanUpText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return OWUtilities.CleanupXmlText(text, false).Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
        }

        public static DialogueText ConvertText(List<DialogueTextData> data, bool randomize)
        {
            return new DialogueText(Enumerable.Empty<XElement>(), false)
            {
                _randomize = randomize,
                _listTextBlocks = data.Select(d => new DialogueText.TextBlock()
                {
                    listPageText = d.pages,
                    condition = d.condition ?? string.Empty,
                }).ToList(),
            };
        }

        public static List<DialogueTextData> ConvertText(DialogueText data)
        {
            return data._listTextBlocks.Select(t => new DialogueTextData()
            {
                condition = t.condition,
                pages = t.listPageText,
            }).ToList();
        }

        static string MergeStrings(string source, string merge)
            => !string.IsNullOrEmpty(merge) ? merge : source;

        static List<T> MergeLists<T>(IEnumerable<T> source, IEnumerable<T> merge)
            => source.Concat(merge).ToList();
    }
}
