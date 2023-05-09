using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsRPG.Utils
{
    public static class PlayerStateUtils
    {
        public static bool InPauseMenu => OuterWildsRPG.Instance.ModHelper.Menus.PauseMenu.IsOpen;
        public static bool IsRoasting => isRoasting;
        public static bool InDialogue => inDialogue;
        public static bool InComputer => inComputer;
        public static bool IsGUIHidden => GUIMode.IsHiddenMode() || GUIMode.IsCaptureMode();

        public static bool IsPlayable
            => !InPauseMenu && !IsRoasting && !InDialogue && !InComputer && !IsGUIHidden;

        static bool isRoasting;
        static bool inDialogue;
        static bool inComputer;

        public static void SetUp()
        {
            GlobalMessenger<Campfire>.AddListener("EnterRoastingMode", OnEnterRoastingMode);
            GlobalMessenger.AddListener("ExitRoastingMode", OnExitRoastingMode);
            GlobalMessenger.AddListener("EnterConversation", OnEnterConversation);
            GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
            GlobalMessenger.AddListener("EnterShipComputer", OnEnterShipComputer);
            GlobalMessenger.AddListener("ExitShipComputer", OnExitShipComputer);
        }

        public static void CleanUp()
        {
            GlobalMessenger<Campfire>.RemoveListener("EnterRoastingMode", OnEnterRoastingMode);
            GlobalMessenger.RemoveListener("ExitRoastingMode", OnExitRoastingMode);
            GlobalMessenger.RemoveListener("EnterConversation", OnEnterConversation);
            GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
            GlobalMessenger.RemoveListener("EnterShipComputer", OnEnterShipComputer);
            GlobalMessenger.RemoveListener("ExitShipComputer", OnExitShipComputer);

            isRoasting = false;
            inDialogue = false;
            inComputer = false;
        }

        static void OnEnterRoastingMode(Campfire campfire) => isRoasting = true;
        static void OnExitRoastingMode() => isRoasting = false;
        static void OnEnterConversation() => inDialogue = true;
        static void OnExitConversation() => inDialogue = false;
        static void OnEnterShipComputer() => inComputer = true;
        static void OnExitShipComputer() => inComputer = false;
    }
}
