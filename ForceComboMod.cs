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
        public const string MOD_VERSION = "1.1.0";

        public static BepInEx.Logging.ManualLogSource Logger;

        public static GameObject button;

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
            [HarmonyPrefix]
            private static void CustomMenuOpen()
            {
                isCustom = true;
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
                if (__instance.name.ToLower().Contains("failed")) __instance.text = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : (restartUponHit ? "Hitless Song" : "Song")));
                if (__instance.name.Contains("ForceComboModeButton")) __instance.text = "Mode: " + currentMode.ToString();
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TMP_Text), "set_text")]
            private static bool set_textPrefix(ref string value, TMP_Text __instance)
            {
                if (value == null || __instance.text == null) return true;
                if (__instance.name.ToLower().Contains("failed"))
                {
                    value = "Failed " + (restartOnPFCLoss ? "PFC" : (restartOnFCLoss ? "FC" : (restartUponHit ? "Hitless Song" : "Song")));
                }
                if (__instance.name.Contains("ForceComboModeButton"))
                {
                    value = "Mode: " + currentMode.ToString();
                }
                return true;
            }

            [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.Awake))]
            [HarmonyPostfix]
            private static void AddButtonAndText(XDCustomLevelSelectMenu __instance)
            {

                button = UnityEngine.Object.Instantiate(__instance.sortButton.transform.parent.gameObject, __instance.transform.parent);
                button.name = "ForceComboModeButton";
                button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Sample Text"; // For some reason removing this prevents the mod from changing the text
                button.gameObject.GetComponentInChildren<TMP_Text>().name = "ForceComboModeButton";
                button.gameObject.GetComponentInChildren<RectTransform>().SetParent(__instance.sortButton.transform.parent.parent.parent.parent);
                Button b = button.GetComponentInChildren<Button>();
                b.interactable = true;
                b.onClick.AddListener(new Action(ChangeMode));

                RectTransform rect = button.gameObject.GetComponentInChildren<RectTransform>();
                rect.anchorMin = new Vector2(4.02f, 2.6f);
                rect.anchorMax = new Vector2(4.02f, 2.6f);
            }

            //[HarmonyPatch(typeof(XDLevelSelectMenuBase), nameof(XDLevelSelectMenuBase.Update))]
            //[HarmonyPostfix]
            //private static void MoveButton()
            //{
            //    RectTransform rect = button.gameObject.GetComponentInChildren<RectTransform>();
            //    float x = 0f;
            //    float y = 0f;
            //    if (Input.GetKey(KeyCode.I)) y += 0.01f;
            //    if (Input.GetKey(KeyCode.J)) x -= 0.01f;
            //    if (Input.GetKey(KeyCode.K)) y -= 0.01f;
            //    if (Input.GetKey(KeyCode.L)) x += 0.01f;
            //    rect.anchorMin = new Vector2(rect.anchorMin.x + x, rect.anchorMin.y + y);
            //    rect.anchorMax = new Vector2(rect.anchorMax.x + x, rect.anchorMax.y + y);
            //    if (Input.GetKeyDown(KeyCode.N)) Logger.LogMessage(rect.anchorMin.x.ToString() + " " + rect.anchorMin.y.ToString());
            //}
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
