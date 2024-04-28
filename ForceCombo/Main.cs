using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ForceCombo
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string MOD_ID = "ForceCombo";
        public const string MOD_NAME = "Force Combo";
        public const string MOD_VERSION = "2.0.0";

        private static ManualLogSource _logger;

        public static ForceComboMode ForceComboState = ForceComboMode.None;
        public static bool InstantRestart = false;

        void Awake()
        {
            _logger = Logger;
            Harmony harmony = new Harmony(MOD_ID);
            harmony.PatchAll<FCLogic>();
        }

        public static void Log(object msg) => _logger.LogMessage(msg);
    }
}
