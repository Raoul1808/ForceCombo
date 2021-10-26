using HarmonyLib;

namespace ForceCombo
{
    public class FCLogic
    {
        private static bool isRestarting = false;

        [HarmonyPatch(typeof(Track), nameof(Track.LateUpdate))]
        [HarmonyPostfix]
        private static void ForceComboMainLogic()
        {
            if (isRestarting || !(Main.InCustoms || Main.InArcade) || Track.IsEditing || Track.IsPaused || Track.Instance.playStateFirst.isInPracticeMode) return;

            PlayState playState = Track.Instance.playStateFirst;

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
                    if (playState.fullComboState != FullComboState.PerfectFullCombo)
                        Restart();
                    break;
            }
        }

        private static void Restart()
        {
            isRestarting = true;
            if (Main.InstantRestart)
            {
                Track.Instance.RestartTrack();
                return;
            }
            Track.Instance.playStateFirst.health = -50;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
        [HarmonyPostfix]
        private static void ForceComboResetValues()
        {
            isRestarting = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.FailSong))]
        [HarmonyPostfix]
        private static void FailSong()
        {
            if (Main.InstantRestart)
                Track.Instance.RestartTrack();
        }

        [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack))]
        [HarmonyPrefix]
        private static void PreventRestart()
        {
            isRestarting = true;
        }
    }
}
