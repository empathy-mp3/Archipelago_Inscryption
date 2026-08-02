using Archipelago_Inscryption.Archipelago;
using DiskCardGame;
using GBC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago_Inscryption.Helpers
{
    internal static class ActResetHelper
    {
        internal static bool CanReset(int act)
        {
            switch (act)
            {
                case 1: return ArchipelagoOptions.enableAct1;
                case 2: return ArchipelagoOptions.enableAct2;
                case 3: return ArchipelagoOptions.enableAct3;
                default: return false;
            }
        }

        internal static void ResetAct(int act)
        {
            switch (act)
            {
                case 1:
                    // Vanilla's own reset. Act 1's story events are not ordered, so leaving them
                    // set does not strand the act partway through, and vanilla never clears them.
                    SaveManager.SaveFile.NewPart1Run();
                    ArchipelagoData.Data.act1RunFresh = true;
                    break;
                case 2:
                    EraseActEvents(2);
                    SaveManager.SaveFile.gbcData = new SaveData();
                    SaveManager.SaveFile.gbcData.Initialize();

                    // Collected pixel cards live on SaveFile, outside gbcData, so the
                    // Initialize above misses them. Leaving them would keep pack cards
                    // reading as collected, and would make VerifyItem treat Archipelago's
                    // own pixel cards as already applied, so they would never come back.
                    SaveManager.SaveFile.gbcCardsCollected = new List<string>();
                    break;
                case 3:
                    EraseActEvents(3);
                    SaveManager.SaveFile.part3Data = new Part3SaveData();
                    SaveManager.SaveFile.part3Data.Initialize();
                    break;
                default:
                    return;
            }

            // Puts the act where a brand new save lands once its items arrive. Erasing above is
            // deliberately broad because this restores anything Archipelago had already granted.
            RestoreSpentItems(act);
            ArchipelagoManager.ReapplyReceivedItems();

            SaveManager.SaveToFile(false);

            // Act cards are built once on start screen load, so refresh their labels in place.
            UIHelper.RefreshActCards();
        }

        // Currency and card packs are counters, and VerifyItem cannot tell "spent" from "never
        // applied", so the re-apply pass leaves both behind.
        private static void RestoreSpentItems(int act)
        {
            // Each act keeps its own currency, so a fresh one holds every Currency item received.
            int currency = ArchipelagoData.Data.receivedItems.Count(item => item.Item == APItem.Currency);

            switch (act)
            {
                case 1:
                    RunState.Run.currency = currency;
                    break;
                case 2:
                    SaveData.Data.currency = currency;
                    break;
                case 3:
                    Part3SaveData.Data.currency = currency;
                    break;
            }

            // Packs come from one pool shared by all three acts, so refund only this act's.
            // Packs opened in another act keep their cards and must stay spent.
            ArchipelagoData.Data.availableCardPacks += ArchipelagoData.Data.packsOpenedPerAct[act - 1];
            ArchipelagoData.Data.packsOpenedPerAct[act - 1] = 0;
            RandomizerHelper.UpdatePackButtonEnabled();
        }

        private static void EraseActEvents(int act)
        {
            foreach (StoryEvent storyEvent in StoryEventActMap.EventsForAct(act))
            {
                // Vanilla keeps these across even a full save wipe, so honour that here too.
                if (StoryEventsData.PermaSavedStoryEvents.Contains(storyEvent)) continue;

                StoryEventsData.EraseEvent(storyEvent);
            }
        }
    }
}
