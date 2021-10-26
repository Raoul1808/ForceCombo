using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace ForceCombo
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class Main : BasePlugin
    {
        public const string MOD_ID = "ForceCombo";
        public const string MOD_NAME = "Force Combo";
        public const string MOD_VERSION = "1.3.0";

        public static BepInEx.Logging.ManualLogSource Logger;

        public static ForceComboMode ForceComboState = ForceComboMode.None;
        public static bool InstantRestart = false;
        public static bool InArcade = false;
        public static bool InCustoms = false;

        public override void Load()
        {
            Logger = Log;
            Harmony harmony = new Harmony(MOD_ID);
            harmony.PatchAll<FCLogic>();
            harmony.PatchAll<FCUI>();
        }
    }
}
