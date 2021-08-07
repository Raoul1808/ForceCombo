using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ForceCombo
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class ForceComboMod : BasePlugin
    {
        public const string MOD_ID = "ForceCombo";
        public const string MOD_NAME = "Force Combo";
        public const string MOD_VERSION = "1.0.0";

        public static BepInEx.Logging.ManualLogSource Logger;
        
        public override void Load()
        {
            Logger = Log;
            Logger.LogWarning("Press F1 in a menu to cycle through Force Combo modes!");
            Harmony.CreateAndPatchAll(typeof(ForceComboMod.Patches));
        }

        public class Patches
        {
            static bool restartOnPFCLoss = false;
            static bool restartOnFCLoss = false;
            static bool isRestarting = false;

            [HarmonyPatch(typeof(Track), nameof(Track.LateUpdate))]
            [HarmonyPostfix]
            private static void ForceComboCode()
            {
                if (isRestarting) return;
                PlayState state = Track.Instance.playStateFirst;
                if (state.wheel.IsCpuControlled || state.isInPracticeMode) return;
                if (restartOnPFCLoss)
                    if (state.fullComboState != FullComboState.PerfectFullCombo) Restart(state);
                if (restartOnFCLoss)
                    if (state.fullComboState == FullComboState.None) Restart(state);
            }

            [HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.Update))]
            [HarmonyPostfix]
            private static void MenuUpdatePostfix()
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (!restartOnFCLoss && !restartOnPFCLoss)
                    {
                        restartOnFCLoss = true;
                        Logger.LogWarning("Force Combo Mode: Full Combo");
                    }
                    else if (restartOnFCLoss && !restartOnPFCLoss)
                    {
                        restartOnFCLoss = false;
                        restartOnPFCLoss = true;
                        Logger.LogWarning("Force Combo Mode: Perfect Full Combo");
                    }
                    else if (!restartOnFCLoss && restartOnPFCLoss)
                    {
                        restartOnPFCLoss = false;
                        Logger.LogWarning("Force Combo Mode: None");
                    }
                }
            }

            [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
            [HarmonyPostfix]
            private static void PlayTrackPostfix()
            {
                isRestarting = false;
            }

            private static void Restart(PlayState state)
            {
                isRestarting = true;
                state.health = -50;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(TextMeshProUGUI), nameof(TextMeshProUGUI.Awake))]
            public static void AwakePost(TextMeshProUGUI __instance)
            {
                if (__instance.text == null) return;
                if (__instance.name.ToLower().Contains("failed")) __instance.text = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : "Song"));
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TMP_Text), "set_text")]
            private static bool set_textPrefix(ref string value, TMP_Text __instance)
            {
                if (value == null || __instance.text == null) return true;
                if (__instance.name.ToLower().Contains("failed"))
                {
                    value = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : "Song"));
                }
                return true;
            }
        }
    }
}
