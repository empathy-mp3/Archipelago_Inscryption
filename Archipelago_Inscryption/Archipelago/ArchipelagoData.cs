using Archipelago_Inscryption.Utils;
using DiskCardGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Archipelago_Inscryption.Archipelago
{

    internal class ArchipelagoData
    {
        [JsonIgnore]
        internal static int currentVersion = 1;

        [JsonIgnore]
        internal static string saveName = "";

        [JsonIgnore]
        internal static string saveFilePath = "";

        [JsonIgnore]
        internal static string dataFilePath = "";

        [JsonIgnore]
        internal static ArchipelagoData Data;

        [JsonProperty("version")]
        internal int version = 0;

        [JsonProperty("hostName")]
        internal string hostName = "archipelago.gg";
        [JsonProperty("port")]
        internal int port = 38281;
        [JsonProperty("slotName")]
        internal string slotName = "";
        [JsonProperty("password")]
        internal string password = "";

        [JsonProperty("seed")]
        internal string seed = "";
        [JsonProperty("playerCount")]
        internal int playerCount = 0;
        [JsonProperty("totalLocationsCount")]
        internal int totalLocationsCount = 0;
        [JsonProperty("totalItemsCount")]
        internal int totalItemsCount = 0;
        [JsonProperty("goalType")]
        internal Goal goalType = Goal.COUNT;
        [JsonProperty("enableAct1")]
        internal bool enableAct1 = false;
        [JsonProperty("enableAct2")]
        internal bool enableAct2 = false;
        [JsonProperty("enableAct3")]
        internal bool enableAct3 = false;
        [JsonProperty("skipEpilogue")]
        internal bool skipEpilogue = false;

        [JsonProperty("vesselUpgrade1Location")]
        internal string vesselUpgrade1Location = "";
        [JsonProperty("vesselUpgrade2Location")]
        internal string vesselUpgrade2Location = "";
        [JsonProperty("vesselUpgrade3Location")]
        internal string vesselUpgrade3Location = "";

        [JsonProperty("act1Battles")]
        internal int act1BattlesThisRun = 0;

        [JsonProperty("completedChecks")]
        internal List<long> completedChecks = new List<long>();
        [JsonProperty("receivedItems")]
        internal List<InscryptionItemInfo> receivedItems = new List<InscryptionItemInfo>();
        [JsonIgnore]
        internal List<InscryptionItemInfo> itemsUnaccountedFor = new List<InscryptionItemInfo>();

        [JsonProperty("customCardInfos")]
        internal List<CustomCardInfo> customCardInfos = new List<CustomCardInfo>();
        [JsonProperty("mycoCardInfo")]
        internal CustomCardInfo mycoCardInfo;
        [JsonIgnore]
        internal List<CardModificationInfo> customCardsModsAct3 = new List<CardModificationInfo>();
        [JsonIgnore]
        internal CardModificationInfo mycoCardMod = null;

        [JsonProperty("cabinSafeCode")]
        internal List<int> cabinSafeCode = new List<int>();
        [JsonProperty("cabinClockCode")]
        internal List<int> cabinClockCode = new List<int>();
        [JsonProperty("cabinSmallClockCode")]
        internal List<int> cabinSmallClockCode = new List<int>();
        [JsonProperty("factoryClockCode")]
        internal List<int> factoryClockCode = new List<int>();
        [JsonProperty("wizardCode1")]
        internal List<int> wizardCode1 = new List<int>();
        [JsonProperty("wizardCode2")]
        internal List<int> wizardCode2 = new List<int>();
        [JsonProperty("wizardCode3")]
        internal List<int> wizardCode3 = new List<int>();

        // Act 1 has not been entered since it was last made fresh, which BasicTutorialCompleted
        // cannot tell on its own: a reset keeps it, and skip_tutorial sets it on an unplayed save.
        [JsonProperty("act1RunFresh")]
        internal bool act1RunFresh = false;
        [JsonProperty("act1Completed")]
        internal bool act1Completed = false;
        [JsonProperty("act2Completed")]
        internal bool act2Completed = false;
        [JsonProperty("act3Completed")]
        internal bool act3Completed = false;
        [JsonProperty("epilogueCompleted")]
        internal bool epilogueCompleted = false;
        [JsonProperty("goalCompletedAndSent")]
        internal bool goalCompletedAndSent = false;

        [JsonIgnore]
        internal uint index = 0;

        // Per save, not static: as static fields Newtonsoft still deserialized them process-wide,
        // so the last save listed by the select screen decided both for the session.
        [JsonProperty("itemLogMode")]
        private ItemLogMode itemLogModeSetting = ItemLogMode.AllItems;
        [JsonProperty("deathLinkOverride")]
        private DeathLinkOverride deathLinkOverrideSetting = DeathLinkOverride.Default;

        internal static ItemLogMode itemLogMode
        {
            get => Data?.itemLogModeSetting ?? ItemLogMode.AllItems;
            set { if (Data != null) Data.itemLogModeSetting = value; }
        }

        internal static DeathLinkOverride deathLinkOverride
        {
            get => Data?.deathLinkOverrideSetting ?? DeathLinkOverride.Default;
            set { if (Data != null) Data.deathLinkOverrideSetting = value; }
        }

        public static bool DeathLink => deathLinkOverride switch
        {
            DeathLinkOverride.Disabled => false,
            DeathLinkOverride.OneCandle => true,
            DeathLinkOverride.EndRun => true,
            _ => ArchipelagoOptions.deathlink,
        };
        public static Act1DeathLink Act1DeathLinkBehaviour => deathLinkOverride switch
        {
            DeathLinkOverride.OneCandle => Act1DeathLink.CandleExtinguished,
            DeathLinkOverride.EndRun => Act1DeathLink.Sacrificed,
            _ => ArchipelagoOptions.act1DeathLinkBehaviour,
        };

        internal static void SaveToFile()
        {
            // No path means no save slot is active, e.g. right after a save data reset.
            if (dataFilePath == "") return;

            string json = JsonConvert.SerializeObject(Data);
            FileSystem.WriteAllText(dataFilePath, json);
        }

        // Returns null if the file can't be read or is corrupted, leaving the caller to report it
        // with whatever context it has. The result is not assigned to Data; that is the caller's.
        internal static ArchipelagoData LoadFromFile(string path)
        {
            // A missing file is an ordinary outcome, e.g. a slot whose data was just reset, so it
            // returns quietly. Only a file that exists and cannot be read is worth reporting.
            if (!FileSystem.FileExists(path)) return null;

            string content;

            try
            {
                content = FileSystem.ReadAllText(path);
            }
            catch (Exception e)
            {
                ArchipelagoModPlugin.Log.LogError("Failed to read Archipelago data from " + path + ": " + e.Message);
                return null;
            }

            ArchipelagoData loaded;

            try
            {
                loaded = JsonConvert.DeserializeObject<ArchipelagoData>(content);
            }
            catch
            {
                loaded = null;
            }

            loaded?.RebuildRuntimeState();

            return loaded;
        }

        // The [JsonIgnore] fields that are derived from serialized data rather than stored, so they
        // have to be rebuilt on every load. Clears first so it is safe to re-run on live data.
        private void RebuildRuntimeState()
        {
            itemsUnaccountedFor = new List<InscryptionItemInfo>(receivedItems);

            customCardsModsAct3.Clear();

            foreach (var cI in customCardInfos)
            {
                CardModificationInfo customCardMod = new CardModificationInfo();
                customCardMod.singletonId = cI.SingletonId;
                customCardMod.nameReplacement = cI.NameReplacement;
                customCardMod.attackAdjustment = cI.AttackAdjustment;
                customCardMod.healthAdjustment = cI.HealthAdjustment;
                customCardMod.energyCostAdjustment = cI.EnergyCostAdjustment;
                customCardMod.abilities = cI.Abilities;
                BuildACardPortraitInfo portraitInfo = new BuildACardPortraitInfo();
                portraitInfo.spriteIndices = cI.SpriteIndices;
                customCardMod.buildACardPortraitInfo = portraitInfo;
                customCardsModsAct3.Add(customCardMod);
            }

            mycoCardMod = null;

            if (mycoCardInfo.Abilities != null && mycoCardInfo.Abilities.Count > 0)
            {
                CardModificationInfo mod = new CardModificationInfo();
                mod.singletonId = mycoCardInfo.SingletonId;
                mod.attackAdjustment = mycoCardInfo.AttackAdjustment;
                mod.healthAdjustment = mycoCardInfo.HealthAdjustment;
                mod.energyCostAdjustment = mycoCardInfo.EnergyCostAdjustment;
                mod.abilities = mycoCardInfo.Abilities;
                mycoCardMod = mod;
            }
        }
    }

    internal struct CustomCardInfo
    {
        [JsonProperty("singletonId")]
        public string SingletonId { get; set; }

        [JsonProperty("nameReplacement")]
        public string NameReplacement { get; set; }

        [JsonProperty("attackAdjustment")]
        public int AttackAdjustment { get; set; }

        [JsonProperty("healthAdjustment")]
        public int HealthAdjustment { get; set; }

        [JsonProperty("energyCostAdjustment")]
        public int EnergyCostAdjustment { get; set; }

        [JsonProperty("abilities")]
        public List<Ability> Abilities { get; set; }

        [JsonProperty("spriteIndices")] 
        public int[] SpriteIndices { get; set; }

        public CustomCardInfo(string singletonId, string nameReplacement, int attackAdjustment, int healthAdjustment, int energyCostAdjustment, List<Ability> abilities, int[] spriteIndices)
        {
            SingletonId = singletonId;
            NameReplacement = nameReplacement;
            AttackAdjustment = attackAdjustment;
            HealthAdjustment = healthAdjustment;
            EnergyCostAdjustment = energyCostAdjustment;
            Abilities = abilities;
            SpriteIndices = spriteIndices;
        }
    }
}
