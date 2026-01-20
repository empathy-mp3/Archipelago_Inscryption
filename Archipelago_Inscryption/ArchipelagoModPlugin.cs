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
        internal const string PluginVersion = "1.2.0";

        internal static ManualLogSource Log;
        internal static string SavePath => savePathConfig.Value;

        private static readonly ConfigFile configFile = new ConfigFile(System.IO.Path.Combine(Paths.ConfigPath, "Archipelago_Inscryption.cfg"), true);
        private static ConfigEntry<string> savePathConfig;

        private void Awake()
        {
            Log = Logger;
            Harmony harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            FileSystem.Init();
            AssetsManager.LoadAssets();
            ArchipelagoManager.Init();

            savePathConfig = configFile.Bind<string>("Saves", "Saves Path", Path.Combine(FileSystem.GetDataPath(), "..", "ArchipelagoSaveFiles"), "Where to create Archipelago-related save data");

            // To remove the lag spike when obtaining a card during the connection screen
            ScriptableObjectLoader<CardInfo>.LoadData();
        }
    }
}
