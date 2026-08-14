using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace Archipelago_Inscryption
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ArchipelagoModPlugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "ballininc.inscryption.archipelagomod";
        internal const string PluginName = "ArchipelagoMod";
        internal const string PluginVersion = "1.5.2";

        internal static ManualLogSource Log;
        internal static string SavePath => savePathConfig.Value;

        // Remembered across saves so a new save starts from the last server you reached, rather
        // than the defaults. Per-save details still win when an existing save is picked.
        internal static string LastHostName { get => lastHostNameConfig.Value; set => lastHostNameConfig.Value = value; }
        internal static int LastPort { get => lastPortConfig.Value; set => lastPortConfig.Value = value; }
        internal static string LastSlotName { get => lastSlotNameConfig.Value; set => lastSlotNameConfig.Value = value; }
        internal static string LastPassword { get => lastPasswordConfig.Value; set => lastPasswordConfig.Value = value; }

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, "Archipelago_Inscryption.cfg"), true);
        private static ConfigEntry<string> savePathConfig;
        private static ConfigEntry<string> lastHostNameConfig;
        private static ConfigEntry<int> lastPortConfig;
        private static ConfigEntry<string> lastSlotNameConfig;
        private static ConfigEntry<string> lastPasswordConfig;

        private void Awake()
        {
            Log = Logger;
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            FileSystem.Init();
            AssetsManager.LoadAssets();
            ArchipelagoManager.Init();

            savePathConfig = configFile.Bind<string>("Saves", "Saves Path", Path.Combine(FileSystem.GetDataPath(), "..", "ArchipelagoSaveFiles"), "Where to create Archipelago-related save data");

            lastHostNameConfig = configFile.Bind("Connection", "Last Host Name", "archipelago.gg", "Host name prefilled on the connect screen for a new save");
            lastPortConfig = configFile.Bind("Connection", "Last Port", 38281, "Port prefilled on the connect screen for a new save");
            lastSlotNameConfig = configFile.Bind("Connection", "Last Slot Name", "", "Slot name prefilled on the connect screen for a new save");
            lastPasswordConfig = configFile.Bind("Connection", "Last Password", "", "Password prefilled on the connect screen for a new save. Stored as plain text");

            // To remove the lag spike when obtaining a card during the connection screen
            ScriptableObjectLoader<CardInfo>.LoadData();

            // Needs the card data above, and only has to happen once per launch.
            ArchipelagoManager.MakeGrantedCardsChoosable();
        }
    }
}
