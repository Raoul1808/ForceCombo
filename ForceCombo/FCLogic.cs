using System;
using HarmonyLib;

namespace ForceCombo
{
    public class FcPatches
    {
        private static bool _isRestarting = false;
        private static bool _inEditor = false;

        [HarmonyPatch(typeof(Track), "LateUpdate")]
        [HarmonyPostfix]
        private static void ForceComboMainLogic()
        {
            if (_inEditor || _isRestarting || Track.PlayStates.Length == 0 || Track.PlayStates[0].isInPracticeMode) return;
            PlayState playState = Track.PlayStates[0];

            float maxAchievableAccuracy = GetMaxAccuracy(playState);
            Main.Log("Max Achievable Accuracy: " + Math.Round(maxAchievableAccuracy * 1000) / 10 + "%");
            if (Main.TargetAccuracy > maxAchievableAccuracy)
            {
                Restart();
                return;
            }

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

        private static float GetMaxAccuracy(PlayState playState)
        {
            float accuracy = 0f;
            int sectionCount = playState.trackData.EditorTrackCuePoints.Count - 1;
            for (int i = 0; i < sectionCount; i++)
            {
                (int currentScore, int maxPotentialScore, int maxScore) = playState.GetCurrentTotalsForPracticeSection(i);
                accuracy += (float)maxPotentialScore / maxScore;
            }
            return accuracy / sectionCount;
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
            _inEditor = false;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.OnEditingTrackBecameActive))]
        [HarmonyPostfix]
        private static void SetEditingFlag()
        {
            _inEditor = true;
        }

        [HarmonyPatch(typeof(Track), nameof(Track.PauseGame))]
        [HarmonyPrefix]
        private static bool PreventPauseWhenRestarting()
        {
            return !_isRestarting;
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
    }
}
