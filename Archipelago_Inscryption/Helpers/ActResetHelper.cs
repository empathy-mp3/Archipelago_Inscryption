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
            RestorePacks(act);
            ArchipelagoManager.ReapplyReceivedItems();

            SaveManager.SaveToFile(false);
            ArchipelagoData.SaveToFile();

            // Act cards are built once on start screen load, so refresh their labels in place.
            UIHelper.RefreshActCards();
        }

        // Packs are a counter, and VerifyItem cannot tell "spent" from "never applied", so
        // the re-apply pass leaves them behind. Currency needs nothing here: each act restores
        // its own when the reset above reinitialises it.
        private static void RestorePacks(int act)
        {
            APItem packItem = act == 1 ? APItem.Act1CardPack
                : act == 2 ? APItem.Act2CardPack : APItem.Act3CardPack;

            ArchipelagoData.Data.SetPacks(act, ArchipelagoManager.CountReceived(packItem));
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
