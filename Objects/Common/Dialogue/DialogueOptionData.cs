using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Dialogue
{
    public class DialogueOptionData
    {
        [Required]
        public string text;
        public string target;
        public string requiredCondition;
        public string cancelledCondition;
        public List<string> requiredPersistentConditions = new();
        public List<string> cancelledPersistentConditions = new();
        public List<string> requiredFacts = new();
        public string setCondition;
        public string cancelCondition;
    }
}
