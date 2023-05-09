using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common
{
    public class TranslationData
    {
        [Description("Translation table for mod-defined text elements.")]
        public Dictionary<string, string> GeneralDictionary = new();
        [Description("Translation table for dialogue.")]
        public Dictionary<string, string> DialogueDictionary = new();
        [Description("Translation table for Ship Log (entries, facts, etc).")]
        public Dictionary<string, string> ShipLogDictionary = new();
        [Description("Translation table for UI elements.")]
        public Dictionary<string, string> UIDictionary = new();

        [Description("Translation table for achievements (If Achievements+ is present). The key is the unique ID of the achievement.")]
        public Dictionary<string, AchievementTranslationData> AchievementTranslations = new();

        public class AchievementTranslationData
        {
            [Description("The name of the achievement.")]
            public string Name;

            [Description("The short description for this achievement.")]
            public string Description;
        }
    }
}
