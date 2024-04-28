using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ForceCombo
{
    [BepInPlugin(Guid, Name, Version)]
    public class Main : BaseUnityPlugin
    {
        public const string Guid = "ForceCombo";
        public const string Name = "Force Combo";
        public const string Version = "2.0.0";

        private static ManualLogSource _logger;

        public static ForceComboMode ForceComboState = ForceComboMode.None;
        public static bool InstantRestart = false;

        void Awake()
        {
            _logger = Logger;
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll<FcPatches>();
        }

        public static void Log(object msg) => _logger.LogMessage(msg);
    }
}
