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

        // CurrentAct cannot answer this for Act 1: IsPart1 is defined as none of the other acts, so
        // the start screen a reset can be run from reads as Act 1 the same as the cabin does.
        private static bool ActIsOnScreen(int act)
            => act == 1
                ? SceneLoader.ActiveSceneName.ToLowerInvariant().Contains("part1")
                : ArchipelagoManager.CurrentAct == act;

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

                    // Collected pixel cards live on SaveFile, outside gbcData, so Initialize
                    // misses them. Cleared first: its postfix re-grants Archipelago's own.
                    SaveManager.SaveFile.gbcCardsCollected = new List<string>();

                    SaveManager.SaveFile.gbcData = new SaveData();
                    SaveManager.SaveFile.gbcData.Initialize();
                    break;
                case 3:
                    EraseActEvents(3);
                    SaveManager.SaveFile.part3Data = new Part3SaveData();
                    SaveManager.SaveFile.part3Data.Initialize();

                    // Cleared before the reapply below, so the cards it recovers wait for the deck
                    // build on re-entry instead of landing beside the slot that build adds.
                    APSaveFile.Act3DeckBuilt = false;
                    break;
                default:
                    return;
            }

            // Moves the act's random seed off what the wiped run already rolled. Act 1 is recorded
            // for consistency only; vanilla already moves its seed by counting past runs.
            APSaveFile.RecordActReset(act);

            // Puts the act where a brand new save lands once its items arrive. Erasing above is
            // deliberately broad because this restores anything Archipelago had already granted.
            ArchipelagoManager.ReapplyReceivedItems();

            SaveManager.SaveToFile(false);
            ArchipelagoData.SaveToFile();

            // Act cards are built once on start screen load, so refresh their labels in place.
            UIHelper.RefreshActCards();

            // Resetting the act being played would leave the scene running on wiped data, so hand
            // the player back to the menu, as vanilla's own save reset does from here.
            if (!ActIsOnScreen(act)) return;

            Singleton<InteractionCursor>.Instance.SetEnabled(false);
            CustomCoroutine.WaitThenExecute(0.1f, delegate
            {
                MenuController.ReturnToStartScreen();
                StartScreenController.startedGame = false;
            }, true);
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
