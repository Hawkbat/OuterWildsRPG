using OuterWildsRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Objects.Common.Dialogue
{
    public class DialogueTreeData
    {
        public string characterName;
        public List<DialogueNodeData> nodes = new();
        public bool turnOnFlashlight;
        public bool turnOffFlashlight;
        public string attentionPointPath;
        public Vector3Data attentionPointOffset;
    }
}
