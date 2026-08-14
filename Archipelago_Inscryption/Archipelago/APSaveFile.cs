using DiskCardGame;
using System;

namespace Archipelago_Inscryption.Archipelago
{
    // Odin writes the game save from the object's runtime type, so a subclass is how Archipelago
    // gets state into that file -- only for state that must be lost and restored in lockstep with it.
    internal class APSaveFile : SaveFile
    {
        // What each act has been given, indexed by act, so index 0 is unused. Spent and never granted
        // read the same, and these live here so a failed game save loses the ledger with the balance.
        public int[] currencyItemsGranted = new int[4];
        public int[] cardPackItemsGranted = new int[4];

        // Unopened packs. Here rather than in ArchipelagoData so that spending one and the cards it
        // deals land in the same write: a save that loses the cards has not spent the pack either.
        public int[] cardPacksAvailable = new int[4];

        // Traps waiting for battle code to spend them. Both effects last only the battle they are
        // spent on, so a pending count kept anywhere else would outlive the battle that used it.
        public int bleachTrapsPending;
        public int reinforcementsTrapsPending;

        // Traps handed over. A spent trap leaves nothing behind, so these are what tell a trap that
        // was applied from one that never arrived.
        public int bleachTrapsGranted;
        public int reinforcementsTrapsGranted;
        public int trashTrapsGranted;
        public int deckSizeTrapsGranted;

        // Whether Act 3's one-time deck build has run. That build hands over every card item held
        // by then, so a grant arriving before it must leave the deck to it and not add its own.
        public bool act3DeckBuilt;

        // Whether the run in progress has been handed each of the Act 1 consumables the server sends.
        // A run start re-adds both from their story events and a spent one leaves nothing behind, so
        // only a per-run record tells one that was delivered from one the run is still owed.
        public bool daggerDeliveredThisRun;
        public bool fishHookDeliveredThisRun;

        // Whether the two above have ever been written. A run already underway when they were added
        // cannot say what it has spent, and guessing risks a second copy, so it is taken as served.
        public bool act1ConsumablesTracked;

        // How many times each act has been reset, indexed by act. Everything the seed for Acts 2 and
        // 3 is built from is wiped by their reset, so without this a reset run repeats every roll.
        public int[] actResets = new int[4];

        private static APSaveFile Current => SaveManager.SaveFile as APSaveFile;

        internal static int BleachTrapsPending
        {
            get => Current?.bleachTrapsPending ?? 0;
            set { if (Current != null) Current.bleachTrapsPending = value; }
        }

        internal static int ReinforcementsTrapsPending
        {
            get => Current?.reinforcementsTrapsPending ?? 0;
            set { if (Current != null) Current.reinforcementsTrapsPending = value; }
        }

        // As with the other ledgers, a save that has none reads as fully granted and is left alone.
        internal static int BleachTrapsGranted
        {
            get => Current?.bleachTrapsGranted ?? int.MaxValue;
            set { if (Current != null) Current.bleachTrapsGranted = value; }
        }

        internal static int ReinforcementsTrapsGranted
        {
            get => Current?.reinforcementsTrapsGranted ?? int.MaxValue;
            set { if (Current != null) Current.reinforcementsTrapsGranted = value; }
        }

        internal static int TrashTrapsGranted
        {
            get => Current?.trashTrapsGranted ?? int.MaxValue;
            set { if (Current != null) Current.trashTrapsGranted = value; }
        }

        // This one is never spent, so what was handed over is also what is in effect. The two only
        // read apart on a save with no record: nothing to repair, and no traps to be playing under.
        internal static int DeckSizeTrapsGranted
        {
            get => Current?.deckSizeTrapsGranted ?? int.MaxValue;
            set { if (Current != null) Current.deckSizeTrapsGranted = value; }
        }

        internal static int DeckSizeTrapsInEffect => Current?.deckSizeTrapsGranted ?? 0;

        // A save from before this class, or one already past Act 3's intro, reads as built: either
        // way the build has had its say, so a card item belongs in the deck now rather than to it.
        internal static bool Act3DeckBuilt
        {
            get => Current == null || Current.act3DeckBuilt
                || StoryEventsData.EventCompleted(StoryEvent.Part3Intro);
            set { if (Current != null) Current.act3DeckBuilt = value; }
        }

        // No save to deliver into reads as served, so the pass below leaves it alone either way.
        internal static bool DaggerDeliveredThisRun
        {
            get => Current == null || Current.daggerDeliveredThisRun;
            set { if (Current != null) Current.daggerDeliveredThisRun = value; }
        }

        internal static bool FishHookDeliveredThisRun
        {
            get => Current == null || Current.fishHookDeliveredThisRun;
            set { if (Current != null) Current.fishHookDeliveredThisRun = value; }
        }

        internal static bool Act1ConsumablesTracked
        {
            get => Current == null || Current.act1ConsumablesTracked;
            set { if (Current != null) Current.act1ConsumablesTracked = value; }
        }

        private static int[] LedgerFor(Func<APSaveFile, int[]> field)
        {
            return SaveManager.SaveFile is APSaveFile save ? field(save) : null;
        }

        private static int Read(Func<APSaveFile, int[]> field, int act, int fallback)
        {
            int[] values = LedgerFor(field);

            return values != null && act < values.Length ? values[act] : fallback;
        }

        private static void Write(Func<APSaveFile, int[]> field, int act, int value)
        {
            int[] values = LedgerFor(field);

            if (values != null && act < values.Length) values[act] = value;
        }

        // A save from before this class has no record to repair from, so its ledgers read as fully
        // granted: the verify pass then leaves them alone rather than handing out a second copy.
        internal static int CurrencyGranted(int act) => Read(save => save.currencyItemsGranted, act, int.MaxValue);
        internal static void SetCurrencyGranted(int act, int count) => Write(save => save.currencyItemsGranted, act, count);

        internal static int PacksGranted(int act) => Read(save => save.cardPackItemsGranted, act, int.MaxValue);
        internal static void SetPacksGranted(int act, int count) => Write(save => save.cardPackItemsGranted, act, count);

        // A live count rather than a ledger, so a save without one simply has no packs to open.
        internal static int PacksAvailable(int act) => Read(save => save.cardPacksAvailable, act, 0);
        internal static void SetPacksAvailable(int act, int count) => Write(save => save.cardPacksAvailable, act, count);

        internal static void GrantPack(int act)
        {
            SetPacksAvailable(act, PacksAvailable(act) + 1);
            SetPacksGranted(act, PacksGranted(act) + 1);
        }

        internal static void SpendPack(int act) => SetPacksAvailable(act, PacksAvailable(act) - 1);

        // No record means no reset has happened, which is where a save that has never been reset is.
        internal static int ActResets(int act) => Read(save => save.actResets, act, 0);
        internal static void RecordActReset(int act) => Write(save => save.actResets, act, ActResets(act) + 1);

        // Both records move together when an act starts, since it hands the act everything it was sent.
        internal static void ResetPacksForAct(int act, int count)
        {
            SetPacksAvailable(act, count);
            SetPacksGranted(act, count);
        }
    }
}
