using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ForceCombo
{
    public class FCUI
    {
        private static GameObject customsFCButton;
        private static GameObject arcadeFCButton;
        private static GameObject customsFCcheckbox;
        private static GameObject arcadeFCcheckbox;

        [HarmonyPatch(typeof(XDCustomLevelSelectMenu), nameof(XDCustomLevelSelectMenu.Awake))]
        [HarmonyPostfix]
        private static void CreateCustomsButton(XDCustomLevelSelectMenu __instance)
        {
            customsFCButton = CreateButton(__instance, new Vector2(4.02f, 2.6f));
            customsFCcheckbox = CreateCheckbox(__instance, new Vector2(4.360005f, 2.639998f));
        }

        [HarmonyPatch(typeof(XDLevelSelectMenu), nameof(XDLevelSelectMenu.Awake))]
        [HarmonyPostfix]
        private static void CreateArcadeButton(XDLevelSelectMenu __instance)
        {
            arcadeFCButton = CreateButton(__instance, new Vector2(10.14014f, 5.740037f));
            arcadeFCButton.gameObject.GetComponentInChildren<RectTransform>().SetParent(__instance.tutorialsButton.transform.parent); // Yes this code is dirty
            arcadeFCcheckbox = CreateCheckbox(__instance, new Vector2(11.04016f, 5.830039f));
            arcadeFCcheckbox.gameObject.GetComponentInChildren<RectTransform>().SetParent(__instance.tutorialsButton.transform.parent); // I tried passing the parent as an argument, it didn't work
        }

        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenMenu))]
        [HarmonyPostfix]
        private static void ResetArcadeCustomsVariables()
        {
            Main.InCustoms = false;
            Main.InArcade = false;
        }

        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenCustomTrackSelect))]
        [HarmonyPostfix]
        private static void SetCustomsVariable()
        {
            Main.InCustoms = true;
        }
        
        [HarmonyPatch(typeof(XDMainMenu), nameof(XDMainMenu.OpenLevelSelect))]
        [HarmonyPostfix]
        private static void SetArcadeVariable()
        {
            Main.InArcade = true;
        }

        private static GameObject CreateButton(XDLevelSelectMenuBase menuInstance, Vector2 anchor)
        {
            Transform parent = menuInstance.sortButton.transform.parent.parent.parent.parent;
            GameObject button = Object.Instantiate(menuInstance.sortButton.transform.parent.gameObject, menuInstance.transform.parent);
            button.name = "ForceComboModeButton";
            button.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Sample Text"; // For some reason removing this prevents the mod from changing the text
            button.gameObject.GetComponentInChildren<TMP_Text>().name = "ForceComboModeButton";
            button.gameObject.GetComponentInChildren<RectTransform>().SetParent(parent);
            button.GetComponentInChildren<Button>().interactable = true;
            button.GetComponentInChildren<Button>().onClick.AddListener(new System.Action(ChangeMode));
            button.gameObject.GetComponentInChildren<RectTransform>().anchorMin = anchor;
            button.gameObject.GetComponentInChildren<RectTransform>().anchorMax = anchor;
            return button;
        }

        private static GameObject CreateCheckbox(XDLevelSelectMenuBase menuInstance, Vector2 anchor)
        {
            Transform parent = menuInstance.sortButton.transform.parent.parent.parent.parent;
            GameObject checkbox = Object.Instantiate(menuInstance.favouriteToggleOnContextMenu.gameObject, menuInstance.transform.parent);
            checkbox.gameObject.GetComponentInChildren<RectTransform>().SetParent(parent);
            checkbox.gameObject.GetComponentInChildren<RectTransform>().anchorMin = anchor;
            checkbox.gameObject.GetComponentInChildren<RectTransform>().anchorMax = anchor;
            checkbox.gameObject.GetComponentInChildren<Toggle>().onValueChanged.AddListener(new System.Action<bool>(ChangeRestart));
            return checkbox;
        }

        [HarmonyPatch(typeof(TMP_Text), "set_text")]
        [HarmonyPrefix]
        private static void ChangeFCButtonName(ref string value, TMP_Text __instance)
        {
            if (__instance.name == "ForceComboModeButton")
                value = "Mode: " + Main.ForceComboState;
        }

        private static void ChangeMode()
        {
            Main.ForceComboState++;
            if ((int)Main.ForceComboState >= 4)
                Main.ForceComboState = 0;
        }

        private static void ChangeRestart(bool newValue)
        {
            Main.InstantRestart = newValue;
        }
    }
}
