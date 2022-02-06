using HarmonyLib;
using SMU.Utilities;
using SpinCore.Handlers;
using SpinCore.Handlers.UI;

namespace ForceCombo
{
    public class FCUI
    {
        private static Bindable<CustomDropDown.DropDownOption> forceComboMode;
        private static Bindable<bool> instantRestart;

        [HarmonyPatch(typeof(Track), "Awake")]
        [HarmonyPostfix]
        private static void AddButton()
        {
            forceComboMode = new Bindable<CustomDropDown.DropDownOption>();
            forceComboMode.Bind(option => Main.ForceComboState = (ForceComboMode)option.OptionID);

            instantRestart = new Bindable<bool>();
            instantRestart.Bind(ticked => Main.InstantRestart = ticked);

            InstanceHandler.OnCustomLevelsOpen += delegate
            {
                CustomDropDown customDropDown = new CustomDropDown("Force Combo Mode", forceComboMode, SpinCoreMenu.CustomLevelsOptionsContextMenu, new string[]
                {
                    "None",
                    "No Hit",
                    "Full Combo",
                    "Perfect Full Combo",
                });
                CustomCheckbox checkbox = new CustomCheckbox("Instantly Restart on fail", instantRestart, SpinCoreMenu.CustomLevelsOptionsContextMenu);
            };
            
            InstanceHandler.OnOfficialLevelsOpen += delegate
            {
                CustomDropDown customDropDown = new CustomDropDown("Force Combo Mode", forceComboMode, SpinCoreMenu.OfficialLevelsOptionsContextMenu, new string[]
                {
                    "None",
                    "No Hit",
                    "Full Combo",
                    "Perfect Full Combo",
                });
                CustomCheckbox checkbox = new CustomCheckbox("Instantly Restart on fail", instantRestart, SpinCoreMenu.OfficialLevelsOptionsContextMenu);
            };
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
    }
}
