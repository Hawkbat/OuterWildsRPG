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
        public static bool IsGUIHidden => GUIMode.IsHiddenMode() || GUIMode.IsCaptureMode();

        public static bool IsPlayable =>
            !InPauseMenu &&
            !IsRoasting &&
            !inFlashback &&
            !PlayerState.InConversation() &&
            !PlayerState.UsingShipComputer() &&
            !PlayerState.IsDead() &&
            !PlayerState.IsSleepingAtCampfire() &&
            !PlayerState.IsSleepingAtDreamCampfire() &&
            !PlayerState.IsPeeping() &&
            !PlayerState.IsFastForwarding() &&
            !PlayerState.UsingNomaiRemoteCamera() &&
            !PlayerState.AtFlightConsole() &&
            !PlayerState.UsingTelescope() &&
            !IsGUIHidden;

        static bool isRoasting;
        static bool inFlashback;

        public static void SetUp()
        {
            GlobalMessenger<Campfire>.AddListener("EnterRoastingMode", OnEnterRoastingMode);
            GlobalMessenger.AddListener("ExitRoastingMode", OnExitRoastingMode);
            GlobalMessenger.AddListener("TriggerFlashback", OnTriggerMemoryUplink);
            GlobalMessenger.AddListener("TriggerMemoryUplink", OnTriggerMemoryUplink);
            GlobalMessenger.AddListener("MemoryUplinkComplete", OnMemoryUplinkComplete);
        }

        public static void CleanUp()
        {
            GlobalMessenger<Campfire>.RemoveListener("EnterRoastingMode", OnEnterRoastingMode);
            GlobalMessenger.RemoveListener("ExitRoastingMode", OnExitRoastingMode);
            GlobalMessenger.RemoveListener("TriggerFlashback", OnTriggerMemoryUplink);
            GlobalMessenger.RemoveListener("TriggerMemoryUplink", OnTriggerMemoryUplink);
            GlobalMessenger.RemoveListener("MemoryUplinkComplete", OnMemoryUplinkComplete);

            isRoasting = false;
            inFlashback = false;
        }

        static void OnEnterRoastingMode(Campfire campfire) => isRoasting = true;
        static void OnExitRoastingMode() => isRoasting = false;
        static void OnTriggerMemoryUplink() => inFlashback = true;
        static void OnMemoryUplinkComplete() => inFlashback = false;
    }
}
