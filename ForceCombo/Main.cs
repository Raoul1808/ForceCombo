using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpinCore.Translation;
using SpinCore.UI;

namespace ForceCombo
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("srxd.raoul1808.spincore", BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string Guid = "ForceCombo";
        public const string Name = "Force Combo";
        public const string Version = "3.0.0";

        private static ManualLogSource _logger;

        public static ForceComboMode ForceComboState = ForceComboMode.None;
        public static bool InstantRestart = false;
        public static float TargetAccuracy = 0f;

        void Awake()
        {
            _logger = Logger;
            Harmony harmony = new Harmony(Guid);
            harmony.PatchAll<FcPatches>();

            var localeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ForceCombo.locale.json");
            TranslationHelper.LoadTranslationsFromStream(localeStream);
            
            UIHelper.RegisterGroupInQuickModSettings(panelParent =>
            {
                var section = UIHelper.CreateGroup(panelParent, "Force Combo Settings");
                UIHelper.CreateSectionHeader(
                    section.Transform,
                    "ForceComboHeader",
                    "ForceCombo_SectionHeader",
                    false
                );
                UIHelper.CreateMultiChoiceButton(
                    section.Transform,
                    "ForceComboMode",
                    "ForceCombo_ForceComboMode",
                    ForceComboMode.None,
                    v => ForceComboState = v
                );
                UIHelper.CreateToggle(
                    section.Transform,
                    "ForceComboToggle",
                    "ForceCombo_InstantRestart",
                    false,
                    v => InstantRestart = v
                );
                UIHelper.CreateMultiChoiceButton(
                    section.Transform,
                    "ForceComboAccuracy",
                    "ForceCombo_TargetAccuracy",
                    0,
                    v => TargetAccuracy = v / 1000f,
                    () => new IntRange(0, 1001),
                    v => decimal.Divide(v, 10).ToString(CultureInfo.InvariantCulture) + "%"
                );
            });
        }

        public static void Log(object msg) => _logger.LogMessage(msg);
    }
}
