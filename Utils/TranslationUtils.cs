using Newtonsoft.Json;
using OuterWildsRPG.Objects.Common;
using OuterWildsRPG.Objects.Common.Dialogue;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Language = TextTranslation.Language;

namespace OuterWildsRPG.Utils
{
    public static class TranslationUtils
    {
        static readonly Dictionary<Language, Dictionary<string, string>> generalTranslations = new();
        static readonly Dictionary<Language, Dictionary<string, string>> shipLogTranslations = new();
        static readonly Dictionary<Language, Dictionary<string, string>> dialogueTranslations = new();
        static readonly Dictionary<Language, Dictionary<string, string>> uiTranslations = new();

        public static string GetGeneral(string key, Language language = Language.UNKNOWN)
            => GetInternal(key, generalTranslations, language);

        public static string GetShipLog(string key, Language language = Language.UNKNOWN)
            => GetInternal(key, shipLogTranslations, language);

        public static string GetDialogue(string key, Language language = Language.UNKNOWN)
            => GetInternal(key, dialogueTranslations, language);

        public static string GetUI(string key, Language language = Language.UNKNOWN)
            => GetInternal(key, uiTranslations, language);
        
        public static string GetUI(int key, Language language = Language.UNKNOWN)
        {
            var stringKey = EnumUtils.GetName((UITextType)key);
            return GetInternal(stringKey, uiTranslations, language);
        }

        public static void RegisterAllTranslations()
        {
            foreach (var mod in OuterWildsRPG.Instance.ModHelper.Interaction.GetMods())
            {
                foreach (var language in EnumUtils.GetValues<Language>())
                {
                    if (language is Language.UNKNOWN or Language.TOTAL) continue;
                    var filePath = Path.GetFullPath(Path.Combine(mod.ModHelper.Manifest.ModFolderPath, "translations", $"{language.ToString().ToLower()}.json"));
                    if (File.Exists(filePath))
                    {
                        var json = File.ReadAllText(filePath);
                        var config = JsonConvert.DeserializeObject<TranslationData>(json);
                        RegisterTranslation(config, language);
                    }
                }
            }
        }

        public static void RegisterTranslation(TranslationData config, Language language)
        {
            RegisterDictInternal(generalTranslations, config.GeneralDictionary, language);
            RegisterDictInternal(shipLogTranslations, config.ShipLogDictionary, language);
            RegisterDictInternal(dialogueTranslations, config.DialogueDictionary, language);
            RegisterDictInternal(uiTranslations, config.UIDictionary, language);
        }

        public static void RegisterGeneral(string key, string value, Language language = Language.ENGLISH)
            => RegisterInternal(key, value, generalTranslations, language);

        public static void RegisterShipLog(string key, string value, Language language = Language.ENGLISH)
            => RegisterInternal(key, value, shipLogTranslations, language);

        public static void RegisterDialogue(string key, string value, Language language = Language.ENGLISH)
            => RegisterInternal(key, value, dialogueTranslations, language);

        public static void RegisterUI(string key, string value, Language language = Language.ENGLISH)
            => RegisterInternal(key, value, uiTranslations, language);

        public static void UnregisterGeneral(string key, Language language = Language.ENGLISH)
            => UnregisterInternal(key, generalTranslations, language);

        public static void UnregisterShipLog(string key, Language language = Language.ENGLISH)
            => UnregisterInternal(key, shipLogTranslations, language);

        public static void UnregisterDialogue(string key, Language language = Language.ENGLISH)
            => UnregisterInternal(key, dialogueTranslations, language);

        public static void UnregisterUI(string key, Language language = Language.ENGLISH)
            => UnregisterInternal(key, uiTranslations, language);

        public static void RegisterDialogueTree(DialogueTreeData dialogueTree)
        {
            RegisterDialogue(dialogueTree.characterName, dialogueTree.characterName);
            foreach (var node in dialogueTree.nodes)
                RegisterDialogueNode(node, dialogueTree.characterName);
        }

        public static void RegisterDialogueNode(DialogueNodeData dialogueNode, string characterName)
        {
            foreach (var dialogue in dialogueNode.dialogues)
                RegisterDialogueText(dialogue, dialogueNode.name);
            foreach (var option in dialogueNode.options)
                RegisterDialogueOption(option, dialogueNode.name, characterName);
        }

        public static void RegisterDialogueText(DialogueTextData dialogueText, string dialogueNodeName)
        {
            foreach (var page in dialogueText.pages)
                RegisterDialogue($"{dialogueNodeName}{page}", page);
        }

        public static void RegisterDialogueOption(DialogueOptionData dialogueOption, string dialogueNodeName, string characterName)
        {
            RegisterDialogue($"{characterName}{dialogueNodeName}{dialogueOption.text}", dialogueOption.text);
        }

        static string GetInternal(string key, Dictionary<Language, Dictionary<string, string>> dictionary, Language language)
        {
            if (language == Language.UNKNOWN)
                language = TextTranslation.Get().m_language;

            if (dictionary.TryGetValue(language, out var table))
                if (table.TryGetValue(key, out var translatedText))
                    return translatedText;


            if (dictionary.TryGetValue(Language.ENGLISH, out var englishTable))
            {
                if (englishTable.TryGetValue(key, out var englishText))
                {
                    OuterWildsRPG.Log($"Falling back to english for {key}", true);
                    return englishText;
                }
            }

            return null;
        }

        static void RegisterInternal(string key, string value, Dictionary<Language, Dictionary<string, string>> dict, Language language)
        {
            if (!dict.ContainsKey(language))
                dict.Add(language, new Dictionary<string, string>());

            key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
            value = value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");

            if (!dict[language].ContainsKey(key)) dict[language].Add(key, value);
            else dict[language][key] = value;

            if (dict == uiTranslations && language == Language.ENGLISH)
            {
                RegisterUIKey(key);
            }
        }

        static void RegisterDictInternal(Dictionary<Language, Dictionary<string, string>> dict, Dictionary<string, string> merge, Language language)
        {
            if (merge == null || !merge.Any())
                return;
            if (!dict.ContainsKey(language))
                dict.Add(language, new Dictionary<string, string>());
            foreach (var (key, value) in merge)
                RegisterInternal(key, value, dict, language);
        }

        static int RegisterUIKey(string key)
        {
            var matches = EnumUtils.GetValues<UITextType>().Where(v => EnumUtils.GetName(v) == key);
            if (matches.Any())
            {
                return (int)matches.First();
            }
            return (int)EnumUtils.Create<UITextType>(key);
        }

        static void UnregisterInternal(string key, Dictionary<Language, Dictionary<string, string>> dict, Language language)
        {
            if (dict.ContainsKey(language))
            {
                key = key.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<![CDATA[", "").Replace("]]>", "");
                if (dict[language].ContainsKey(key))
                {
                    dict[language].Remove(key);
                }
            }
        }
    }
}
