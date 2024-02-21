using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

namespace CyberMultiplier
{
    
    [BepInPlugin("fenicemaster.CyberMultiplier", "CyberMultiplier", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("fenicemaster.CyberMultiplier");
        public static Plugin instance;
        internal static ManualLogSource mls;
        // LonerGrind
        internal static ConfigEntry<int> length;
        internal static ConfigEntry<int> width;
        internal static ConfigEntry<bool> syncIn;
        internal static ConfigEntry<bool> syncOut;
        internal static ConfigEntry<bool> screenCram;
        // Debug
        internal static ConfigEntry<bool> allInvValues;
        internal static ConfigEntry<bool> logGrids;

        public void Awake()
        {

            // create the logger
            mls = BepInEx.Logging.Logger.CreateLogSource(" fenicemaster.CyberMultiplier");
            mls.LogInfo("CyberMultiplier is loading...");

            // start configs, set instance, patch everything, and start to listen
            configs();
            if (instance == null) instance = this;
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(CyberGrindPatch));
            harmony.PatchAll(typeof(CyberManager));
            CyberManager.Listen();
            
            mls.LogInfo("CyberMultiplier is loaded");
        }

        public void configs()
        {
            // MorbiusGrind settings
            length = Config.Bind<int>("CyberGrind", "CyberGrind arena length", 2, "This setting will set the CyberGrind length.");
            width = Config.Bind<int>("CyberGrind", "CyberGrind arena width", 2, "This setting will set the CyberGrind width.");
            syncIn = Config.Bind<bool>("CyberGrind", "Start sync", true, "This setting allows the activation of everything at once at the activation of one grid.");
            syncOut = Config.Bind<bool>("CyberGrind", "Change sync", false, "This setting makes grids wait the death of all enemies to change.");
            screenCram = Config.Bind<bool>("CyberGrind", "Screen cram", true, "This setting makes the screen count for all the grids enemies.");

            // Debug settings
            allInvValues = Config.Bind<bool>("Debug", "Allow invalid values", false, "This setting will make the generation accept " +
                "invalid values that are < than 1.");
            logGrids = Config.Bind<bool>("Debug", "Log things", true, "This setting will toggle the logs for most stuff.");
        }
    }
}