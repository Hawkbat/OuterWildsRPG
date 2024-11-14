using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Components
{
    public class WorldIconTarget : MonoBehaviour
    {
        DropPickupController dropPickup;
        QuestGiverController questGiver;
        ShopkeeperController shopkeeper;
        NomaiText nomaiText;
        CharacterDialogueTree dialogueTree;

        void Start()
        {
            dropPickup = GetComponentInChildren<DropPickupController>();
            questGiver = GetComponentInChildren<QuestGiverController>();
            shopkeeper = GetComponentInChildren<ShopkeeperController>();
            nomaiText = GetComponentInChildren<NomaiText>();
            dialogueTree = GetComponentInChildren<CharacterDialogueTree>();
        }

        public Sprite GetIcon()
        {
            if (questGiver != null && !questGiver.GetQuest().IsStarted)
            {
                return Assets.WorldIconQuestSprite;
            } else if (shopkeeper != null)
            {
                return Assets.WorldIconShopSprite;
            } else if (nomaiText != null)
            {
                return Assets.WorldIconTextSprite;
            } else if (dialogueTree != null)
            {
                switch (dialogueTree._characterName)
                {
                    case "SIGN": return Assets.WorldIconSignSprite;
                    case "RECORDING": return Assets.WorldIconRecordingSprite;
                    default: return Assets.WorldIconTalkSprite;
                }
            }
            return null;
        }

        public string GetName()
        {
            if (dialogueTree != null && dialogueTree._characterName != "SIGN" && dialogueTree._characterName != "RECORDING")
            {
                return dialogueTree._characterName;
            }
            return string.Empty;
        }

        public float GetMinDistance()
        {
            return 10f;
        }

        public float GetMaxDistance()
        {
            if (questGiver != null && !questGiver.GetQuest().IsStarted)
            {
                return 100f;
            } else if (shopkeeper != null)
            {
                return 100f;
            }
            return 50f;
        }

        public Transform GetTarget()
        {
            if (!gameObject.activeInHierarchy) return null;
            if (dialogueTree != null)
            {
                return dialogueTree._attentionPoint;
            }
            return transform;
        }

        public Vector3 GetOffset()
        {
            if (dialogueTree != null)
            {
                return dialogueTree._attentionPointOffset + Vector3.up;
            }
            return Vector3.up;
        }
    }
}
