using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace ForceCombo
{
    [BepInPlugin(MOD_ID, MOD_NAME, MOD_VERSION)]
    [BepInDependency("com.pink.spinrhythm.spincore", BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string MOD_ID = "ForceCombo";
        public const string MOD_NAME = "Force Combo";
        public const string MOD_VERSION = "1.4.0";

        private static ManualLogSource _logger;

        public static ForceComboMode ForceComboState = ForceComboMode.None;
        public static bool InstantRestart = false;
        public static bool InArcade = false;
        public static bool InCustoms = false;

        void Awake()
        {
            _logger = Logger;
            Harmony harmony = new Harmony(MOD_ID);
            harmony.PatchAll<FCLogic>();
            harmony.PatchAll<FCUI>();
        }

        public static void Log(LogLevel level, object msg) => _logger.Log(level, msg);
        public static void LogInfo(object msg) => Log(LogLevel.Info, msg);
        public static void LogMessage(object msg) => Log(LogLevel.Info, msg);
        public static void LogWarning(object msg) => Log(LogLevel.Warning, msg);
        public static void LogError(object msg) => Log(LogLevel.Error, msg);
        public static void LogDebug(object msg) => Log(LogLevel.Debug, msg);
    }
}
