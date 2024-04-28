using HarmonyLib;
using UnityEngine;

namespace ForceCombo
{
    public class FcPatches
    {
        private static bool _isRestarting = false;

        [HarmonyPatch(typeof(Track), "LateUpdate")]
        [HarmonyPostfix]
        private static void ForceComboMainLogic()
        {
            if (_isRestarting || Track.PlayStates.Length == 0) return;
            PlayState playState = Track.PlayStates[0];

            switch (Main.ForceComboState)
            {
                case ForceComboMode.NoHit:
                    if (playState.health < playState.MaxHealth)
                        Restart();
                    break;
                case ForceComboMode.FC:
                    if (playState.fullComboState == FullComboState.None)
                        Restart();
                    break;
                case ForceComboMode.PFC:
                    if (playState.fullComboState < FullComboState.Perfect)
                        Restart();
                    break;
            }
        }

        private static void Restart()
        {
            if (Track.PlayStates.Length == 0) return;
            _isRestarting = true;
            if (Main.InstantRestart)
            {
                Track.RestartTrack();
                return;
            }

            Track.PlayStates[0].health = -50;
            Track.FailSong();
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
        [HarmonyPostfix]
        private static void ForceComboResetValues()
        {
            _isRestarting = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.FailSong))]
        [HarmonyPostfix]
        private static void FailSong()
        {
            if (Main.InstantRestart)
                Track.RestartTrack();
        }

        [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack))]
        [HarmonyPrefix]
        private static void PreventRestart()
        {
            _isRestarting = true;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.Update))]
        [HarmonyPostfix]
        private static void SwitchForceComboState()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Main.ForceComboState++;
                if (Main.ForceComboState > ForceComboMode.PFC)
                {
                    Main.ForceComboState = ForceComboMode.None;
                }
                NotificationSystemGUI.AddMessage("Force Combo Mode: " + Main.ForceComboState);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                Main.InstantRestart = !Main.InstantRestart;
                NotificationSystemGUI.AddMessage("Force Combo Instant Restart: " + (Main.InstantRestart ? "Enabled" : "Disabled"));
            }
        }
    }
}
