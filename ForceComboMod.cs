using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForceCombo
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class ForceComboMod : BasePlugin
    {
        public const string MOD_ID = "ForceCombo";
        public const string MOD_NAME = "Force Combo";
        public const string MOD_VERSION = "1.2.0";

        public static BepInEx.Logging.ManualLogSource Logger;

        public static GameObject button;
        public static GameObject checkbox;
        public static Toggle checkboxMark;

        public override void Load()
        {
            Logger = Log;
            Harmony.CreateAndPatchAll(typeof(ForceComboMod.Patches));
        }

        public class Patches
        {
            static bool restartOnPFCLoss = false;
            static bool restartOnFCLoss = false;
            static bool restartUponHit = false;
            static bool isRestarting = false;
            static bool isCustom = false;
            static bool directRestart => checkboxMark.isOn;
            static ForceComboMode currentMode = ForceComboMode.None;

            [HarmonyPatch(typeof(Track), nameof(Track.LateUpdate))]
            [HarmonyPostfix]
            private static void ForceComboCode()
            {
                if (isRestarting || !isCustom) return;
                PlayState state = Track.Instance.playStateFirst;
                if (state.wheel.IsCpuControlled || state.isInPracticeMode) return;
                if (restartUponHit)
                    if (state.health < state.MaxHealth) Restart(state);
                if (restartOnPFCLoss)
                    if (state.fullComboState != FullComboState.PerfectFullCombo) Restart(state);
                if (restartOnFCLoss)
                    if (state.fullComboState == FullComboState.None) Restart(state);
            }

            private static void ChangeMode()
            {
                switch (currentMode)
                {
                    case ForceComboMode.None:
                        currentMode = ForceComboMode.NoHit;
                        restartUponHit = true;
                        break;

                    case ForceComboMode.NoHit:
                        currentMode = ForceComboMode.FC;
                        restartOnFCLoss = true;
                        break;

                    case ForceComboMode.FC:
                        currentMode = ForceComboMode.PFC;
                        restartOnPFCLoss = true;
                        break;

                    case ForceComboMode.PFC:
                        currentMode = ForceComboMode.None;
                        restartOnFCLoss = false;
                        restartOnPFCLoss = false;
                        restartUponHit = false;
                        break;
                }
            }

            [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack))]
            [HarmonyPostfix]
            private static void PlayTrackPostfix()
            {
                isRestarting = false;
            }

            [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenMenu))]
            [HarmonyPostfix]
            private static void MainMenuOpen()
            {
                isCustom = false;
            }

            [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.OpenMenu))]
            [HarmonyPostfix]
            private static void CustomMenuOpen()
            {
                isCustom = true;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack))]
            [HarmonyPrefix]
            private static void PreventRestart()
            {
                isRestarting = true;
            }

            [HarmonyPatch(typeof(Track), nameof(Track.FailSong))]
            [HarmonyPostfix]
            private static void InstantRestartTrack(Track __instance)
            {
                if (directRestart) __instance.RestartTrack();
            }

            private static void Restart(PlayState state)
            {
                isRestarting = true;
                if (directRestart) Track.Instance.RestartTrack();
                else state.health = -50;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(TextMeshProUGUI), nameof(TextMeshProUGUI.Awake))]
            public static void AwakePost(TextMeshProUGUI __instance)
            {
                if (__instance.text == null) return;
                if (__instance.name.ToLower().Contains("failed")) __instance.text = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : (restartUponHit ? "Hitless Song" : "Song")));
                if (__instance.name.Contains("ForceComboModeButton")) __instance.text = "Mode: " + currentMode.ToString();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TMP_Text), "set_text")]
            private static bool set_textPrefix(ref string value, TMP_Text __instance)
            {
                if (value == null || __instance.text == null) return true;
                if (__instance.name.ToLower().Contains("failed")) value = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : (restartUponHit ? "Hitless Song" : "Song")));
                if (__instance.name.Contains("ForceComboModeButton")) value = "Mode: " + currentMode.ToString();
                return true;
            }

            [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.Awake))]
            [HarmonyPostfix]
            private static void AddButtonAndText(XDCustomLevelSelectMenu __instance)
            {
                Transform parent = __instance.sortButton.transform.parent.parent.parent.parent;
                button = UnityEngine.Object.Instantiate(__instance.sortButton.transform.parent.gameObject, __instance.transform.parent);
                button.name = "ForceComboModeButton";
                button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Sample Text"; // For some reason removing this prevents the mod from changing the text
                button.gameObject.GetComponentInChildren<TMP_Text>().name = "ForceComboModeButton";
                button.gameObject.GetComponentInChildren<RectTransform>().SetParent(parent);
                Button b = button.GetComponentInChildren<Button>();
                b.interactable = true;
                b.onClick.AddListener(new Action(ChangeMode));
                button.gameObject.GetComponentInChildren<RectTransform>().anchorMin = new Vector2(4.02f, 2.6f);
                button.gameObject.GetComponentInChildren<RectTransform>().anchorMax = new Vector2(4.02f, 2.6f); button.gameObject.GetComponentInChildren<RectTransform>().anchorMax = new Vector2(4.02f, 2.6f);

                checkbox = UnityEngine.Object.Instantiate(__instance.favouriteToggleOnContextMenu.gameObject, __instance.transform.parent);
                checkbox.gameObject.GetComponentInChildren<RectTransform>().SetParent(parent);
                checkbox.gameObject.GetComponentInChildren<RectTransform>().anchorMin = new Vector2(4.360005f, 2.639998f);
                checkbox.gameObject.GetComponentInChildren<RectTransform>().anchorMax = new Vector2(4.360005f, 2.639998f);
                checkboxMark = checkbox.gameObject.GetComponentInChildren<Toggle>();
            }
        }

        public enum ForceComboMode
        {
            None,
            NoHit,
            FC,
            PFC
        }
    }
}
