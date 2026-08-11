using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Helpers;
using Archipelago_Inscryption.Patches;
using DiskCardGame;
using GBC;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Archipelago_Inscryption.Archipelago
{
    internal static class ArchipelagoManager
    {
        internal static Action<APItem> onItemReceived;

        internal const int ID_OFFSET = 147000;

        // Currency was one item paying all three acts at once. Splitting it per act would
        // have cut each act's income to a third, so one item is worth three to keep it level.
        internal const int CURRENCY_PER_ITEM = 3;

        // When one of the following events is completed, send the associated check.
        private static readonly Dictionary<StoryEvent, APCheck> storyCheckPairs = new Dictionary<StoryEvent, APCheck>()
        {
            { StoryEvent.ProspectorDefeated,            APCheck.CabinBossProspector },
            { StoryEvent.AnglerDefeated,                APCheck.CabinBossAngler },
            { StoryEvent.TrapperTraderDefeated,         APCheck.CabinBossTrapper },
            { StoryEvent.LeshyDefeated,                 APCheck.CabinBossLeshy },
            { StoryEvent.StartScreenNewGameUnlocked,    APCheck.CabinNewGameButton},
            { StoryEvent.PhotographerDefeated,          APCheck.FactoryBossPhotographer },
            { StoryEvent.ArchivistDefeated,             APCheck.FactoryBossArchivist },
            { StoryEvent.CanvasDefeated,                APCheck.FactoryBossUnfinished },
            { StoryEvent.TelegrapherDefeated,           APCheck.FactoryBossG0lly },
            { StoryEvent.MycologistsBossDefeated,       APCheck.FactoryBossMycologists },
            { StoryEvent.Part3Completed,                APCheck.FactoryGreatTranscendence },
            { StoryEvent.Part3MetBonelord,              APCheck.FactoryBoneLordRoom },
            { StoryEvent.GooPlaneGoobertRevealed,       APCheck.FactoryGoobertPainting }
        };

        // When one of the following items is received, set the associated story event as completed.
        private static readonly Dictionary<APItem, StoryEvent> itemStoryPairs = new Dictionary<APItem, StoryEvent>()
        {
            { APItem.StinkbugCard,                      StoryEvent.StinkbugCardDiscovered },
            { APItem.StuntedWolfCard,                   StoryEvent.TalkingWolfCardDiscovered },
            { APItem.FilmRoll,                          StoryEvent.FilmRollDiscovered },
            { APItem.SkinkCard,                         StoryEvent.SkinkCardDiscovered },
            { APItem.AntCards,                          StoryEvent.AntCardsDiscovered },
            { APItem.CagedWolfCard,                     StoryEvent.CageCardDiscovered },
            { APItem.SquirrelTotemHead,                 StoryEvent.SquirrelHeadDiscovered },
            { APItem.Dagger,                            StoryEvent.SpecialDaggerDiscovered },
            { APItem.CabinCloverPlant,                  StoryEvent.CloverFound },
            { APItem.ExtraCandle,                       StoryEvent.CandleArmFound },
            { APItem.BeeFigurine,                       StoryEvent.BeeFigurineFound },
            { APItem.GreaterSmoke,                      StoryEvent.ImprovedSmokeCardDiscovered },
            { APItem.AnglerHook,                        StoryEvent.FishHookUnlocked },
            { APItem.Ring,                              StoryEvent.RingFound },
            { APItem.PileOfMeat,                        StoryEvent.GBCDogFoodFound },
            { APItem.Monocle,                           StoryEvent.GBCMonocleFound },
            { APItem.AncientObol,                       StoryEvent.GBCObolFound },
            { APItem.MycologistsHoloKey,                StoryEvent.MycologistHutKeyFound },
            { APItem.BoneLordHoloKey,                   StoryEvent.BonelordHoloKeyFound },
            { APItem.BoneLordFemur,                     StoryEvent.GBCBoneFound },
            { APItem.GBCCloverPlant,                    StoryEvent.GBCCloverFound },
            { APItem.FishbotCard,                       StoryEvent.TalkingAnglerCardDiscovered },
            { APItem.LonelyWizbotCard,                  StoryEvent.TalkingBlueMageCardDiscovered },
            // Vanilla's own event for the cabin's Ourobot card, which only that scene ever set. It
            // records that the item was granted, which the deck cannot: Act 3 lets you spend a card.
            { APItem.Ourobot,                           StoryEvent.OurobotCardDiscovered },
            { APItem.FoulBackwaterShortcut,             StoryEvent.NatureHoloShortcut},
            { APItem.FilthyCorpseWorldShortcut,         StoryEvent.UndeadHoloShortcut},
            { APItem.GaudyGemLandShortcut,              StoryEvent.WizardHoloShortcut},
            { APItem.GemsModule,                        StoryEvent.GemsModuleFetched},
            { APItem.ResplendentBastionGate,            StoryEvent.HoloTechAreaUnlocked}
        };

        // When one of the following items is received, add the associated card(s) to the deck.
        private static readonly Dictionary<APItem, UnlockableCardInfo> itemCardPair = new Dictionary<APItem, UnlockableCardInfo>()
        {
            { APItem.StinkbugCard,                      new UnlockableCardInfo(false, ["Stinkbug_Talking"], ["Stinkbug_Talking", "Stoat_Talking"]) },
            { APItem.StuntedWolfCard,                   new UnlockableCardInfo(false, ["Wolf_Talking"], ["Wolf_Talking", "Stoat_Talking"]) },
            { APItem.SkinkCard,                         new UnlockableCardInfo(false, ["Skink"]) },
            { APItem.AntCards,                          new UnlockableCardInfo(false, ["Ant", "AntQueen"]) },
            { APItem.CagedWolfCard,                     new UnlockableCardInfo(false, ["CagedWolf"]) },
            { APItem.LonelyWizbotCard,                  new UnlockableCardInfo(true, ["BlueMage_Talking"]) },
            { APItem.FishbotCard,                       new UnlockableCardInfo(true, ["Angler_Talking"]) },
            { APItem.Ourobot,                           new UnlockableCardInfo(true, ["Ouroboros_Part3"]) }
        };

        // When one of the following items is received, add the associated card to the act 2 deck.
        private static readonly Dictionary<APItem, string> itemPixelCardPair = new Dictionary<APItem, string>()
        {
            { APItem.BoneLordHorn,                      "BonelordHorn" },
            { APItem.GreatKrakenCard,                   "Kraken" },
            { APItem.DrownedSoulCard,                   "DrownedSoul" },
            { APItem.SalmonCard,                        "Salmon" }
        };

        // Items whose effect is a count rather than a flag, so one copy leaves nothing of its own
        // behind to look for. Each entry says how many of that item the save still accounts for.
        private static readonly Dictionary<APItem, Func<int>> countedItemTallies = new Dictionary<APItem, Func<int>>()
        {
            // The trader turns each pelt into exactly one tarot, so the two together are the tally.
            { APItem.HoloPelt,       () => Part3SaveData.Data.pelts + Part3SaveData.Data.collectedTarots.Count },
            // Currency and packs are spent, so what is left of them says nothing. These count what
            // was handed over instead, which spending does not touch.
            { APItem.Act1Currency,   () => APSaveFile.CurrencyGranted(1) },
            { APItem.Act2Currency,   () => APSaveFile.CurrencyGranted(2) },
            { APItem.Act3Currency,   () => APSaveFile.CurrencyGranted(3) },
            { APItem.Act1CardPack,   () => APSaveFile.PacksGranted(1) },
            { APItem.Act2CardPack,   () => APSaveFile.PacksGranted(2) },
            { APItem.Act3CardPack,   () => APSaveFile.PacksGranted(3) },
            // Taking a challenge off the run's list is the whole effect, and nothing puts one back
            // mid-run, so what is left of a challenge is the copies still owed.
            { APItem.SmallerBackpackChallenge,  ChallengeTally(AscensionChallenge.LessConsumables, 1) },
            { APItem.PriceyPeltsChallenge,      ChallengeTally(AscensionChallenge.ExpensivePelts, 1) },
            { APItem.BossTotemsChallenge,       ChallengeTally(AscensionChallenge.BossTotems, 1) },
            { APItem.AllTotemBattlesChallenge,  ChallengeTally(AscensionChallenge.AllTotems, 1) },
            { APItem.TippedScalesChallenge,     ChallengeTally(AscensionChallenge.StartingDamage, 3) },
            { APItem.MoreDifficultChallenge,    ChallengeTally(AscensionChallenge.BaseDifficulty, 2) },
            { APItem.ProgressiveGrizzlies,      ChallengeTally(AscensionChallenge.GrizzlyMode, 3) },
            // Two copies with unlike effects: the first takes its challenge off the run, the second
            // stands in for the cabin item this mode leaves out of the pool. One half of the tally each.
            { APItem.ProgressiveCandle,         LadderTally(AscensionChallenge.LessLives, StoryEvent.CandleArmFound) },
            { APItem.ProgressiveSquirrel,       LadderTally(AscensionChallenge.SubmergeSquirrels, StoryEvent.BeeFigurineFound) },
            // A spent trap leaves only a battle behind, and battles are not saved, so these count what
            // was handed over. Pending counts sit beside them and revert with the battle they were spent on.
            { APItem.BleachTrap,                () => APSaveFile.BleachTrapsGranted },
            { APItem.ReinforcementsTrap,        () => APSaveFile.ReinforcementsTrapsGranted },
            { APItem.TrashTrap,                 () => APSaveFile.TrashTrapsGranted },
            // Both upgrades append to one list, so they are told apart by what they append. A null
            // list reads as fully accounted for, since reapplying against it would throw.
            { APItem.VesselUpgrade,  () => Part3SaveData.Data.sideDeckAbilities?.Count(a => a != Ability.ConduitNull) ?? int.MaxValue },
            { APItem.ConduitUpgrade, () => Part3SaveData.Data.sideDeckAbilities?.Count(a => a == Ability.ConduitNull) ?? int.MaxValue }
        };

        // Copies already applied, counted by what is missing from the list a run start builds. No
        // list yet reads as fully applied: a run that has not been set up has nothing to repair.
        private static Func<int> ChallengeTally(AscensionChallenge challenge, int countAtRunStart)
        {
            return () =>
            {
                List<AscensionChallenge> active = AscensionSaveData.Data?.activeChallenges;

                if (active == null) return int.MaxValue;

                return countAtRunStart - active.Count(entry => entry == challenge);
            };
        }

        private static Func<int> LadderTally(AscensionChallenge challenge, StoryEvent secondCopyEvent)
        {
            return () =>
            {
                if (AscensionSaveData.Data?.activeChallenges == null) return int.MaxValue;

                return (AscensionSaveData.Data.activeChallenges.Contains(challenge) ? 0 : 1)
                    + (StoryEventsData.EventCompleted(secondCopyEvent) ? 1 : 0);
            };
        }

        private static Dictionary<APCheck, CheckInfo> checkInfos = new Dictionary<APCheck, CheckInfo>();

        private static Queue<InscryptionItemInfo> itemQueue = new Queue<InscryptionItemInfo>();

        private static Queue<InscryptionItemInfo> itemsToVerifyQueue = new Queue<InscryptionItemInfo>();
        
        private static int receivingCardForAct = 0;
        internal static int CurrentAct {
            get {
                if (receivingCardForAct > 0) return receivingCardForAct;
                if (SaveManager.SaveFile.IsPart1) return 1;
                if (SaveManager.SaveFile.IsPart2) return 2;
                if (SaveManager.SaveFile.IsPart3) return 3;
                return 0;
            }
        }

        internal static void Init()
        {
            ArchipelagoClient.onConnectAttemptDone += OnConnectAttempt;
            ArchipelagoClient.onNewItemReceived += OnItemReceived;
            ArchipelagoClient.onProcessedItemReceived += OnItemToVerifyReceived;

            ReportCountedItemsWithoutTallies();
            StoryEventActMap.WarnAboutUnmappedEvents();
        }

        // The switch can force every item to be classified but not that a Counted one has something to
        // count against, and a Counted item with no tally is quietly never recovered.
        private static void ReportCountedItemsWithoutTallies()
        {
            foreach (APItem item in Enum.GetValues(typeof(APItem)))
            {
                bool counted = RecoveryOf(item) == Recovery.Counted;

                if (counted == countedItemTallies.ContainsKey(item)) continue;

                ArchipelagoModPlugin.Log.LogError(counted
                    ? $"{item} is classified Counted but has no tally, so a shortfall of it is never recovered."
                    : $"{item} has a tally but is not classified Counted, so the tally is never consulted.");
            }
        }

        private static void OnItemReceived(InscryptionItemInfo item)
        {
            itemQueue.Enqueue(item);
        }

        private static void OnItemToVerifyReceived(InscryptionItemInfo item)
        {
            itemsToVerifyQueue.Enqueue(item);
        }

        internal static bool ProcessNextItem()
        {
            if (itemQueue.Count > 0)
            {
                AudioController.Instance.PlaySound2D("creepy_rattle_lofi");

                InscryptionItemInfo item = itemQueue.Dequeue();

                string message;
                if (item.PlayerSlot == ArchipelagoClient.session.ConnectionInfo.Slot)
                    message = "You have found your " + item.ItemName;
                else
                    message = "Received " + item.ItemName + " from " + item.PlayerName;

                if (ArchipelagoData.itemLogMode != ItemLogMode.Disabled)
                {
                    Singleton<ArchipelagoUI>.Instance.LogImportant(message);
                    ArchipelagoModPlugin.Log.LogMessage(message);
                }

                ApplyItemReceived(item.Item);

                Singleton<ArchipelagoUI>.Instance.StartCoroutine(Singleton<ArchipelagoUI>.Instance.QueueSave());

                return true;
            }

            return false;
        }

        // A battle that has not reached its end sequence yet, so a save taken now would record its
        // Act 1 node as beaten. Gates both the deferred save and the warning that guards it.
        internal static bool IsBattleUnresolved(TurnManager turnManager)
            => turnManager != null && !turnManager.GameEnding && !turnManager.GameEnded;

        // Reproduces closing and reopening the game: everything unsaved is dropped, then every item
        // the server sent is replayed, so what Archipelago granted survives and the rest does not.
        internal static void RevertUnsavedProgressAndReplayItems()
        {
            SaveManager.LoadFromFile();

            ArchipelagoData reloaded = ArchipelagoData.LoadFromFile(ArchipelagoData.dataFilePath);

            if (reloaded == null)
            {
                ArchipelagoModPlugin.Log.LogWarning("Reverted the game save, but Archipelago data could not be reloaded, so received items were left alone.");
                return;
            }

            ArchipelagoData.Data = reloaded;

            // Offline there is nothing to replay from, so the next connect's item pass recovers
            // whatever the revert dropped, exactly as it does after the game has been closed.
            if (!ArchipelagoClient.IsConnected) return;

            // The replay rebuilds the full set from the server, so anything still queued from before
            // the revert would be granted twice.
            itemQueue.Clear();
            itemsToVerifyQueue.Clear();

            var received = ArchipelagoClient.session.Items.AllItemsReceived;

            foreach (ItemInfo item in received)
                ArchipelagoClient.ProcessItem(item);

            ArchipelagoData.Data.index = (uint)received.Count;

            // Ones the revert dropped were queued as new by the replay above. They are a replay too,
            // so they are applied silently here rather than announced again by ProcessNextItem.
            List<InscryptionItemInfo> toApply = new List<InscryptionItemInfo>();
            while (itemQueue.Count > 0) toApply.Add(itemQueue.Dequeue());

            // Items the reloaded data already accounts for went to the verify queue instead, and
            // join them only if their effect is missing.
            toApply.AddRange(TakeItemsNeedingReapply());

            int reapplied = ApplyItemsSilently(toApply);
            if (reapplied > 0) SaveManager.SaveToFile(false);

            // The replay is deliberately silent, so this is the only trace it leaves.
            ArchipelagoModPlugin.Log.LogInfo($"Reverted unsaved progress. Replayed {received.Count} items, reapplied {reapplied}.");
        }

        // A replay restores items the player already watched arrive, so it skips the sound, the
        // on-screen log and the per-item delay that ProcessNextItem gives genuinely new ones.
        private static int ApplyItemsSilently(List<InscryptionItemInfo> items)
        {
            int applied = 0;

            foreach (InscryptionItemInfo item in items)
            {
                // The scene may already be unwinding, and applying an item reaches into live
                // singletons, so one failure must not strand the rest of the list.
                try
                {
                    ApplyItemReceived(item.Item);
                    applied++;
                }
                catch (Exception e)
                {
                    ArchipelagoModPlugin.Log.LogError($"Failed to reapply {item.ItemName} (ID {item.ItemId}): {e}");
                }
            }

            return applied;
        }

        // Sigils are randomized on the way in, before the deck sees the card, so a granted card is
        // no more vanilla than one the run was dealt.
        private static void AddCardToDeck(CardInfo card, int act, DeckInfo deck)
        {
            receivingCardForAct = act;
            try {
                CardPatches.RandomizeSigils(card);
                deck.AddCard(card);
            } finally {
                receivingCardForAct = 0;
            }
        }

        private static void AddUnlockedCard(string cardName, int act, DeckInfo deck)
        {
            AddCardToDeck(CardLoader.GetCardByName(cardName), act, deck);
        }

        // Cards a fresh deck has to be handed, because nothing reads these items' events. Excluded:
        // the wolf, talking wolf and stinkbug, which vanilla rebuilds, and Ourobot, which is guaranteed.
        private static readonly Dictionary<int, APItem[]> runStartCards = new Dictionary<int, APItem[]>()
        {
            { 1, new APItem[] { APItem.SkinkCard, APItem.AntCards } },
            { 3, new APItem[] { APItem.FishbotCard, APItem.LonelyWizbotCard } }
        };

        // Every card Archipelago hands out that a vanilla pool can also reach. Withheld from those
        // pools until the item arrives, or a choice node would give away what a check is paying for.
        private static readonly Dictionary<string, APItem> gatedCards = new Dictionary<string, APItem>()
        {
            { "Kraken",           APItem.GreatKrakenCard },
            { "BonelordHorn",     APItem.BoneLordHorn },
            { "DrownedSoul",      APItem.DrownedSoulCard },
            { "Salmon",           APItem.SalmonCard },
            { "Skink",            APItem.SkinkCard },
            { "Ant",              APItem.AntCards },
            { "AntQueen",         APItem.AntCards },
            { "Stinkbug_Talking", APItem.StinkbugCard },
            { "Wolf_Talking",     APItem.StuntedWolfCard },
            { "Angler_Talking",   APItem.FishbotCard },
            { "BlueMage_Talking", APItem.LonelyWizbotCard },
            { "Ouroboros_Part3",  APItem.Ourobot }
        };

        internal static bool CardIsWithheld(string cardName)
            => gatedCards.TryGetValue(cardName, out APItem item) && !HasItem(item);

        // Cards this mod makes choosable, keyed by the act whose deck they belong to. Vanilla has no
        // one-per-deck rule for them, and its own check reads Act 1's deck whichever act you are in.
        private static readonly Dictionary<string, int> uniqueGrantedCards = new Dictionary<string, int>()
        {
            { "Stinkbug_Talking", 1 },
            { "Wolf_Talking",     1 },
            { "Angler_Talking",   3 },
            { "BlueMage_Talking", 3 },
            { "Ouroboros_Part3",  3 }
        };

        internal static bool CardAlreadyInDeck(string cardName)
        {
            if (!uniqueGrantedCards.TryGetValue(cardName, out int act)) return false;

            List<CardInfo> deck = act == 3
                ? Part3SaveData.Data?.deck?.Cards
                : RunState.Run?.playerDeck?.Cards;

            return deck != null && deck.Exists(card => card.name == cardName);
        }

        // The base game never offers these at a choice node, so Archipelago's copies could only ever
        // be handed over. Made choosable so they can be found too; the gate above keeps them honest.
        internal static void MakeGrantedCardsChoosable()
        {
            foreach (string cardName in uniqueGrantedCards.Keys)
            {
                CardInfo card = ScriptableObjectLoader<CardInfo>.AllData.Find(c => c.name == cardName);

                if (card == null)
                {
                    ArchipelagoModPlugin.Log.LogError($"Cannot make {cardName} choosable: no such card.");
                    continue;
                }

                if (!card.metaCategories.Contains(CardMetaCategory.ChoiceNode))
                    card.metaCategories.Add(CardMetaCategory.ChoiceNode);

                // Vanilla's own singleton rule reads this, which covers every base-game pool at once.
                card.onePerDeck = true;

                ArchipelagoModPlugin.Log.LogInfo($"{cardName} is now choosable and unique (temple {card.temple}).");
            }
        }

        // Per card, not per item: a deck that already holds an Ant keeps the one it has and is still
        // given the Ant Queen it does not.
        private static void GrantMissingCards(APItem item, int act, DeckInfo deck)
        {
            if (!HasItem(item) || !itemCardPair.TryGetValue(item, out UnlockableCardInfo info)) return;

            foreach (string cardName in info.cardsToUnlock)
            {
                if (!deck.Cards.Exists(card => card.name == cardName))
                    AddUnlockedCard(cardName, act, deck);
            }
        }

        internal static void GrantRunStartCards(int act, DeckInfo deck)
        {
            foreach (APItem item in runStartCards[act]) GrantMissingCards(item, act, deck);
        }

        // Ourobot exists to make Act 3 easier, so it is the one card that act guarantees -- present
        // in every deck the way the caged wolf is, and never left to a roll.
        internal static void GrantOurobotIfReceived(DeckInfo deck)
        {
            GrantMissingCards(APItem.Ourobot, 3, deck);
        }

        // What a randomized starter deals as extra slots instead of as themselves, so an item widens
        // the deck without guaranteeing a card the roll did not pick.
        internal static List<string> RunStartCardNames(int act)
        {
            List<string> names = new List<string>();

            foreach (APItem item in runStartCards[act])
            {
                if (HasItem(item) && itemCardPair.TryGetValue(item, out UnlockableCardInfo info))
                    names.AddRange(info.cardsToUnlock);
            }

            return names;
        }

        internal static void ApplyItemReceived(APItem receivedItem)
        {
            if (itemStoryPairs.TryGetValue(receivedItem, out StoryEvent storyEvent))
            {
                StoryEventsData.SetEventCompleted(storyEvent);
            }

            if (itemCardPair.TryGetValue(receivedItem, out UnlockableCardInfo info))
            {
                DeckInfo deck = info.isPart3
                    ? SaveManager.SaveFile.part3Data?.deck
                    : RunState.Run?.playerDeck;

                // Skipped while Act 3's build is pending, or when there is no deck to add to yet:
                // either way the act's own start hands over every card item held by then.
                if (deck != null && (!info.isPart3 || APSaveFile.Act3DeckBuilt))
                {
                    int act = info.isPart3 ? 3 : 1;

                    // Finding a card hands you that card, as it does in the base game, whatever the
                    // deck is randomized to. A run start still decides what a run opens holding.
                    foreach (string cardName in info.cardsToUnlock)
                    {
                        if (!CardAlreadyInDeck(cardName)) AddUnlockedCard(cardName, act, deck);
                    }
                }

                for (int i = 0; i < info.rigDraws.Length; i++)
                {
                    if (!SaveManager.SaveFile.RiggedDraws.Contains(info.rigDraws[i]))
                        SaveManager.SaveFile.RiggedDraws.Add(info.rigDraws[i]);
                }
            }
            else if (itemPixelCardPair.TryGetValue(receivedItem, out string cardName))
            {
                receivingCardForAct = 2;
                try {
                    SaveManager.SaveFile.CollectGBCCard(CardLoader.GetCardByName(cardName));
                } finally {
                    receivingCardForAct = 0;
                }
            }

            // The ledger moves with the balance and is written to the same file, so whichever of the
            // two a failed save loses, it loses both, and the next connect can see the shortfall.
            if (receivedItem == APItem.Act1Currency)
            {
                RunState.Run.currency += CURRENCY_PER_ITEM;
                APSaveFile.SetCurrencyGranted(1, APSaveFile.CurrencyGranted(1) + 1);
            }
            else if (receivedItem == APItem.Act2Currency)
            {
                SaveData.Data.currency += CURRENCY_PER_ITEM;
                APSaveFile.SetCurrencyGranted(2, APSaveFile.CurrencyGranted(2) + 1);
            }
            else if (receivedItem == APItem.Act3Currency)
            {
                Part3SaveData.Data.currency += CURRENCY_PER_ITEM;
                APSaveFile.SetCurrencyGranted(3, APSaveFile.CurrencyGranted(3) + 1);
            }
            else if (receivedItem == APItem.Act1CardPack)
            {
                APSaveFile.GrantPack(1);
                RandomizerHelper.RefreshPackPile();
            }
            else if (receivedItem == APItem.Act2CardPack)
            {
                APSaveFile.GrantPack(2);
                RandomizerHelper.UpdatePackButtonEnabled();
            }
            else if (receivedItem == APItem.Act3CardPack)
            {
                APSaveFile.GrantPack(3);
                RandomizerHelper.RefreshPackPile();
            }
            else if (receivedItem == APItem.TrashTrap)
            {
                // Counted even when no scene matches and no card is dealt, so a trap that arrives
                // between acts stays dropped rather than landing in whichever act is opened next.
                APSaveFile.TrashTrapsGranted++;

                if (SaveManager.SaveFile.currentScene.Contains("Part1"))
                    RunState.Run.playerDeck.AddCard(CardLoader.GetCardByName("BrokenEgg"));
                else if (SaveManager.SaveFile.currentScene.Contains("GBC"))
                    SaveData.Data.deck.AddCard(CardLoader.GetCardByName("BrokenEgg"));
                else if (SaveManager.SaveFile.currentScene.Contains("Part3"))
                {
                    if (ArchipelagoOptions.randomizeSigils == RandomizeSigils.Disable || ArchipelagoOptions.randomizeSigils == RandomizeSigils.RandomizeOnce)
                        Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName("Angler_Fish_Bad"));
                    else
                        Part3SaveData.Data.deck.AddCard(CardLoader.GetCardByName("EmptyVessel"));
                }
            }
            else if (receivedItem == APItem.BleachTrap)
            {
                APSaveFile.BleachTrapsGranted++;

                if (TurnManager.Instance == null)
                {
                    APSaveFile.BleachTrapsPending++;
                }
                else if (TurnManager.Instance.IsPlayerTurn) {
                    if (!RandomizerHelper.BleachTrapRemoveSigils())
                        {
                            APSaveFile.BleachTrapsPending++;
                        }
                }
                else
                {
                    APSaveFile.BleachTrapsPending++;
                }
            }
            else if (receivedItem == APItem.DeckSizeTrap)
            {
                ArchipelagoData.Data.deckSizeTrapCount++;
                while (SaveData.Data.collection.cardIds.Count < 20 + ArchipelagoData.Data.deckSizeTrapCount
                    && SaveData.Data.collection.cardIds.Count >= 20)
                {
                    SaveData.Data.collection.AddCard(CardLoader.GetCardByName("DausBell"));
                }
            }
            else if (receivedItem == APItem.ReinforcementsTrap)
            {
                APSaveFile.ReinforcementsTrapsPending++;
                APSaveFile.ReinforcementsTrapsGranted++;
            }
            else if (receivedItem == APItem.SquirrelTotemHead && !RunState.Run.totemTops.Contains(Tribe.Squirrel))
            {
                RunState.Run.totemTops.Add(Tribe.Squirrel);
            }
            else if (receivedItem == APItem.BeeFigurine && !RunState.Run.totemTops.Contains(Tribe.Insect))
            {
                RunState.Run.totemTops.Add(Tribe.Insect);
            }
            else if (receivedItem == APItem.MagnificusEye)
            {
                RunState.Run.eyeState = EyeballState.Wizard;
            }
            else if (receivedItem == APItem.ExtraCandle)
            {
                RunState.Run.maxPlayerLives = 3;
            }
            else if (receivedItem == APItem.Dagger && SaveManager.SaveFile.IsPart1)
            {
                if (RunState.Run.consumables.Count >= RunState.Run.MaxConsumables)
                {
                    string itemName = RunState.Run.consumables[0];
                    if (RunState.Run.consumables.Contains("Pliers"))
                    {
                        itemName = "Pliers";
                    }
                    else
                    {
                        int lessConsumables = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables);
                        for (int i = 2 - lessConsumables; i >= 0; i--)
                        {
                            if (RunState.Run.consumables[i] != "FishHook")
                            {
                                itemName = RunState.Run.consumables[i];
                                break;
                            }
                        }
                    }
                    if (Singleton<ItemsManager>.Instance)
                        Singleton<ItemsManager>.Instance.DestroyItem(itemName);
                    else
                        RunState.Run.consumables.Remove(itemName);
                }
                RunState.Run.consumables.Add("SpecialDagger");
                if (Singleton<ItemsManager>.Instance)
                    Singleton<ItemsManager>.Instance.UpdateItems(false);
            }
            else if (receivedItem == APItem.AnglerHook && SaveManager.SaveFile.IsPart1)
            {
                if (RunState.Run.consumables.Count >= RunState.Run.MaxConsumables)
                {
                    string itemName = RunState.Run.consumables[0];
                    int lessConsumables = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.LessConsumables);
                    for (int i = 2 - lessConsumables; i >= 0; i--)
                    {
                        if (RunState.Run.consumables[i] != "SpecialDagger")
                        {
                            itemName = RunState.Run.consumables[i];
                            break;
                        }
                    }

                    if (Singleton<ItemsManager>.Instance)
                        Singleton<ItemsManager>.Instance.DestroyItem(itemName);
                    else
                        RunState.Run.consumables.Remove(itemName);
                }
                RunState.Run.consumables.Add("FishHook");
                if (Singleton<ItemsManager>.Instance)
                    Singleton<ItemsManager>.Instance.UpdateItems(false);
            }
            else if (receivedItem.ToString().Contains("Epitaph"))
            {
                int pieceCount = 0;

                if (receivedItem == APItem.EpitaphPiece)
                    pieceCount = ArchipelagoData.Data.receivedItems.Count(item => item.Item == APItem.EpitaphPiece);
                else if (ArchipelagoOptions.epitaphPiecesRandomization == EpitaphPiecesRandomization.Groups)
                    pieceCount = ArchipelagoData.Data.receivedItems.Count(item => item.Item == APItem.EpitaphPieces) * 3;
                else
                    pieceCount = 9;

                for (int i = 0; i < pieceCount; i++)
                {
                    if (i >= 9) break;

                    SaveData.Data.undeadTemple.epitaphPieces[i].found = true;
                }
                
            }
            else if (receivedItem == APItem.Monocle && Singleton<WizardMonocleEffect>.Instance)
            {
                Singleton<WizardMonocleEffect>.Instance.ShowLayer();
            }
            else if (receivedItem == APItem.CameraReplica)
            {
                SaveData.Data.natureTemple.hasCamera = true;
            }
            else if (receivedItem == APItem.MrsBombRemote && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.BombRemote))
            {
                Part3SaveData.Data.unlockedItems.Add(Part3SaveData.ItemUnlock.BombRemote);
                Part3SaveData.Data.items.Add(Part3SaveData.ItemUnlock.BombRemote.ToString());
                if (Singleton<ItemsManager>.Instance && SaveManager.SaveFile.IsPart3)
                    Singleton<ItemsManager>.Instance.UpdateItems(false);
            }
            else if (receivedItem == APItem.ExtraBattery && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.Battery))
            {
                Part3SaveData.Data.unlockedItems.Add(Part3SaveData.ItemUnlock.Battery);
                Part3SaveData.Data.items.Add(Part3SaveData.ItemUnlock.Battery.ToString());
                if (Singleton<ItemsManager>.Instance && SaveManager.SaveFile.IsPart3)
                    Singleton<ItemsManager>.Instance.UpdateItems(false);
            }
            else if (receivedItem == APItem.NanoArmorGenerator && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.ShieldGenerator))
            {
                Part3SaveData.Data.unlockedItems.Add(Part3SaveData.ItemUnlock.ShieldGenerator);
                Part3SaveData.Data.items.Add(Part3SaveData.ItemUnlock.ShieldGenerator.ToString());
                if (Singleton<ItemsManager>.Instance && SaveManager.SaveFile.IsPart3)
                    Singleton<ItemsManager>.Instance.UpdateItems(false);
            }
            else if (receivedItem == APItem.HoloPelt)
            {
                Part3SaveData.Data.pelts++;
            }
            else if (receivedItem == APItem.Quill)
            {
                Part3SaveData.Data.foundUndeadTempleQuill = true;
            }
            else if (receivedItem == APItem.VesselUpgrade)
            {
                List<Ability> validVesselUpgrades = [Ability.WhackAMole, Ability.Sharp, Ability.Reach, 
                Ability.RandomAbility, Ability.GainBattery, Ability.ExplodeOnDeath, Ability.DeathShield, 
                    Ability.LatchExplodeOnDeath, Ability.LatchDeathShield, Ability.LatchBrittle, 
                    Ability.Sentry, Ability.DebuffEnemy, Ability.CellBuffSelf];
                List<Ability> extraVesselUpgrades = [Ability.CreateBells, Ability.DrawRabbits];
                if (ArchipelagoOptions.extraSigils) validVesselUpgrades = validVesselUpgrades.Concat(extraVesselUpgrades).ToList();
                foreach (Ability sigil in Part3SaveData.Data.sideDeckAbilities)
                {
                    if (validVesselUpgrades.Contains(sigil)) validVesselUpgrades.Remove(sigil);
                }
                int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
                Ability randomSigil = validVesselUpgrades[SeededRandom.Range(0, validVesselUpgrades.Count, seed++)];
                Part3SaveData.Data.sideDeckAbilities.Add(randomSigil);
            }
            else if (receivedItem == APItem.ConduitUpgrade)
            {
                Part3SaveData.Data.sideDeckAbilities.Add(Ability.ConduitNull);
            }   
            else if (receivedItem == APItem.SmallerBackpackChallenge)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.LessConsumables);
            }
			else if (receivedItem == APItem.PriceyPeltsChallenge)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.ExpensivePelts);
            }
			else if (receivedItem == APItem.BossTotemsChallenge)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.BossTotems);
            }
			else if (receivedItem == APItem.TippedScalesChallenge)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.StartingDamage);
            }
			else if (receivedItem == APItem.AllTotemBattlesChallenge)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.AllTotems);
            }
			else if (receivedItem == APItem.MoreDifficultChallenge)
            {
                // BaseDifficulty is what a run start adds for this item. HarderDeckTrials was never
                // added by anything, so removing it left the challenge up until the next run.
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.BaseDifficulty);
            }
			else if (receivedItem == APItem.ProgressiveCandle)
            {
                if (RunState.Run.maxPlayerLives >= 2)
                {
                    StoryEventsData.SetEventCompleted(StoryEvent.CandleArmFound);
                }
                RunState.Run.maxPlayerLives++;
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.LessLives);
            }
			else if (receivedItem == APItem.ProgressiveSquirrel)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.SubmergeSquirrels);
                if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.ProgressiveSquirrel) >= 2)
                {
                    StoryEventsData.SetEventCompleted(StoryEvent.BeeFigurineFound);
                    if (!RunState.Run.totemTops.Contains(Tribe.Insect))
                        RunState.Run.totemTops.Add(Tribe.Insect);
                }
            }
			else if (receivedItem == APItem.ProgressiveGrizzlies)
            {
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.GrizzlyMode);
            }

            if (Singleton<GameFlowManager>.Instance != null && SaveManager.SaveFile.IsPart1)
            {
                if (receivedItem == APItem.MagnificusEye && Singleton<GameFlowManager>.Instance is Part1GameFlowManager)
                {
                    Singleton<UIManager>.Instance.Effects.GetEffect<WizardEyeEffect>().SetIntensity(1f, 0f);
                }

                if (receivedItem == APItem.ExtraCandle && Singleton<GameFlowManager>.Instance is Part1GameFlowManager)
                {
                    if (Singleton<TurnManager>.Instance == null || !(Singleton<TurnManager>.Instance.Opponent is Part1BossOpponent))
                        RunState.Run.playerLives = Mathf.Min(RunState.Run.maxPlayerLives, RunState.Run.playerLives + 1);

                    Singleton<CandleHolder>.Instance.UpdateArmsAndFlames();
                    Singleton<CandleHolder>.Instance.anim.Play("add_candle");
                }

                if (receivedItem == APItem.ProgressiveCandle && Singleton<GameFlowManager>.Instance is Part1GameFlowManager)
                {
                    if (Singleton<TurnManager>.Instance == null || !(Singleton<TurnManager>.Instance.Opponent is Part1BossOpponent))
                        RunState.Run.playerLives = Mathf.Min(RunState.Run.maxPlayerLives, RunState.Run.playerLives + 1);

                    Singleton<CandleHolder>.Instance.UpdateArmsAndFlames();
                }

                if (Singleton<CardDrawPiles>.Instance is Part1CardDrawPiles piles)
                {
                    if (receivedItem == APItem.BeeFigurine || 
                    (receivedItem == APItem.ProgressiveSquirrel && piles.sidePileFigurine == SidePileFigurine.Squirrel))
                        piles.SetSidePileFigurine(SidePileFigurine.Bee);
                    else if (receivedItem == APItem.ProgressiveSquirrel && piles.sidePileFigurine == SidePileFigurine.Aquasquirrel)
                        piles.SetSidePileFigurine(SidePileFigurine.Squirrel);
                }
                
            }
            else
            {
                if (receivedItem == APItem.ExtraCandle || receivedItem == APItem.ProgressiveCandle)
                {
                    RunState.Run.playerLives = Mathf.Min(RunState.Run.maxPlayerLives, RunState.Run.playerLives + 1);
                }
            }

            if (onItemReceived != null)
                onItemReceived(receivedItem);
        }

        private static void OnConnectAttempt(LoginResult result)
        {
            Singleton<ArchipelagoUI>.Instance.UpdateConnectionStatus(result.Successful);
            if (result.Successful)
            {
                AudioController.Instance.PlaySound2D("creepy_rattle_glassy", MixerGroup.None, 0.5f);
            }
            else
            {
                AudioController.Instance.PlaySound2D("glitch", MixerGroup.None, 0.5f);
            }
        }

        internal static void InitializeFromServer()
        {
            if (ArchipelagoClient.slotData.TryGetValue("death_link", out var deathLink))
                ArchipelagoOptions.deathlink = Convert.ToInt32(deathLink) != 0;
            else if (ArchipelagoClient.slotData.TryGetValue("deathlink", out var deathlink))
                ArchipelagoOptions.deathlink = Convert.ToInt32(deathlink) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("act1_death_link_behaviour", out var act1DeathLink))
                ArchipelagoOptions.act1DeathLinkBehaviour = (Act1DeathLink)Convert.ToInt32(act1DeathLink);
            else if (ArchipelagoClient.slotData.TryGetValue("act1_deathlink_behaviour", out var act1Deathlink))
                ArchipelagoOptions.act1DeathLinkBehaviour = (Act1DeathLink)Convert.ToInt32(act1Deathlink);
            if (ArchipelagoClient.slotData.TryGetValue("optional_death_card", out var optionalDeathCard))
                ArchipelagoOptions.optionalDeathCard = (OptionalDeathCard)Convert.ToInt32(optionalDeathCard);
            if (ArchipelagoClient.slotData.TryGetValue("enable_act_1", out var enableAct1))
                ArchipelagoOptions.enableAct1 = Convert.ToInt32(enableAct1) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("enable_act_2", out var enableAct2))
                ArchipelagoOptions.enableAct2 = Convert.ToInt32(enableAct2) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("enable_act_3", out var enableAct3))
                ArchipelagoOptions.enableAct3 = Convert.ToInt32(enableAct3) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("act_unlocks", out var actUnlocks))
                ArchipelagoOptions.actUnlocks = (ActUnlocks)Convert.ToInt32(actUnlocks);
            if (ArchipelagoClient.slotData.TryGetValue("goal", out var goal))
                ArchipelagoOptions.goal = (Goal)Convert.ToInt32(goal);
            if (ArchipelagoClient.slotData.TryGetValue("randomize_codes", out var randomizeCodes))
                ArchipelagoOptions.randomizeCodes = Convert.ToInt32(randomizeCodes) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("randomize_deck", out var randomizeDeck))
                ArchipelagoOptions.randomizeDeck = (RandomizeDeck)Convert.ToInt32(randomizeDeck);
            if (ArchipelagoClient.slotData.TryGetValue("randomize_sigils", out var randomizeSigils))
                ArchipelagoOptions.randomizeSigils = (RandomizeSigils)Convert.ToInt32(randomizeSigils);
            if (ArchipelagoClient.slotData.TryGetValue("extra_sigils", out var extraSigils))
                ArchipelagoOptions.extraSigils = Convert.ToInt32(extraSigils) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("randomize_nodes", out var randomizeNodes))
                ArchipelagoOptions.randomizeNodes = Convert.ToInt32(randomizeNodes) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("randomize_challenges", out var randomizeChallenges))
                ArchipelagoOptions.randomizeChallenges = (RandomizeChallenges)Convert.ToInt32(randomizeChallenges);
            if (ArchipelagoClient.slotData.TryGetValue("act2_randomize_bridge", out var act2RandomizeBridge))
                ArchipelagoOptions.act2RandomizeBridge = (Act2RandomizeBridge)Convert.ToInt32(act2RandomizeBridge);
            if (ArchipelagoClient.slotData.TryGetValue("act3_overhaul", out var act3Overhaul))
                ArchipelagoOptions.act3Overhaul = Convert.ToInt32(act3Overhaul) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("release_on_act_completion", out var releaseOnActCompletion))
                ArchipelagoOptions.releaseOnActCompletion = Convert.ToInt32(releaseOnActCompletion) != 0;
            for (int act = 1; act <= 3; act++)
            {
                if (ArchipelagoClient.slotData.TryGetValue($"act{act}_location_start", out var locationStart))
                    ArchipelagoOptions.actLocationStarts[act - 1] = Convert.ToInt32(locationStart);
                if (ArchipelagoClient.slotData.TryGetValue($"act{act}_location_count", out var locationCount))
                    ArchipelagoOptions.actLocationCounts[act - 1] = Convert.ToInt32(locationCount);
            }
            if (ArchipelagoClient.slotData.TryGetValue("randomize_hammer", out var randomizeHammer))
                ArchipelagoOptions.randomizeHammer = (RandomizeHammer)Convert.ToInt32(randomizeHammer);
            if (ArchipelagoClient.slotData.TryGetValue("randomize_shortcuts", out var randomizeShortcuts))
                ArchipelagoOptions.randomizeShortcuts = (RandomizeShortcuts)Convert.ToInt32(randomizeShortcuts);
            if (ArchipelagoClient.slotData.TryGetValue("randomize_vessel_upgrades", out var randomizeVesselUpgrades))
                ArchipelagoOptions.randomizeVesselUpgrades = (RandomizeVesselUpgrades)Convert.ToInt32(randomizeVesselUpgrades);
            if (ArchipelagoClient.slotData.TryGetValue("skip_tutorial", out var skipTutorial))
                ArchipelagoOptions.skipTutorial = Convert.ToInt32(skipTutorial) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("skip_epilogue", out var skipEpilogue))
                ArchipelagoOptions.skipEpilogue = Convert.ToInt32(skipEpilogue) != 0;
            if (ArchipelagoClient.slotData.TryGetValue("epitaph_pieces_randomization", out var piecesRandomization))
                ArchipelagoOptions.epitaphPiecesRandomization = (EpitaphPiecesRandomization)Convert.ToInt32(piecesRandomization);

            ArchipelagoData.Data.seed = ArchipelagoClient.session.RoomState.Seed;
            ArchipelagoData.Data.playerCount = ArchipelagoClient.session.Players.AllPlayers.Count() - 1;
            ArchipelagoData.Data.totalLocationsCount = ArchipelagoClient.session.Locations.AllLocations.Count();
            ArchipelagoData.Data.totalItemsCount = ArchipelagoData.Data.totalLocationsCount;
            ArchipelagoData.Data.goalType = ArchipelagoOptions.goal;
            ArchipelagoData.Data.enableAct1 = ArchipelagoOptions.enableAct1;
            ArchipelagoData.Data.enableAct2 = ArchipelagoOptions.enableAct2;
            ArchipelagoData.Data.enableAct3 = ArchipelagoOptions.enableAct3;
            ArchipelagoData.Data.skipEpilogue = ArchipelagoOptions.skipEpilogue;

            DeathLinkManager.DeathLinkService = ArchipelagoClient.session.CreateDeathLinkService();
            DeathLinkManager.Init();

            if (ArchipelagoOptions.randomizeCodes)
            {
                if (ArchipelagoData.Data.cabinClockCode.Count <= 0)
                {
                    int seed = int.Parse(ArchipelagoClient.session.RoomState.Seed.Substring(ArchipelagoClient.session.RoomState.Seed.Length - 6)) + 20 * ArchipelagoClient.session.ConnectionInfo.Slot;

                    ArchipelagoOptions.RandomizeCodes(seed);
                }

                ArchipelagoOptions.SetupRandomizedCodes();
            }

            if (ArchipelagoOptions.skipTutorial && !StoryEventsData.EventCompleted(StoryEvent.TutorialRun3Completed))
                ArchipelagoOptions.SkipTutorial();
            if (ArchipelagoOptions.randomizeShortcuts == RandomizeShortcuts.Open && !StoryEventsData.EventCompleted(StoryEvent.WizardHoloShortcut))
            {
                StoryEventsData.SetEventCompleted(StoryEvent.NatureHoloShortcut, false, false);
                StoryEventsData.SetEventCompleted(StoryEvent.UndeadHoloShortcut, false, false);
                StoryEventsData.SetEventCompleted(StoryEvent.WizardHoloShortcut, false, false);
            }

            ScoutChecks();
            VerifyGoalCompletion();
            ArchipelagoClient.SendChecksToServerAsync();
        }

        internal static void VerifyAllItems()
        {
            foreach (InscryptionItemInfo item in TakeItemsNeedingReapply()) itemQueue.Enqueue(item);
        }

        // The items this pass found missing, left to the caller to apply. Nothing is applied until
        // the pass is over, so a counted shortfall would otherwise read alike for every copy of it.
        private static List<InscryptionItemInfo> TakeItemsNeedingReapply()
        {
            Dictionary<APItem, int> requeuedCounts = new Dictionary<APItem, int>();
            List<InscryptionItemInfo> missing = new List<InscryptionItemInfo>();

            while (itemsToVerifyQueue.Count > 0)
            {
                InscryptionItemInfo nextItem = itemsToVerifyQueue.Dequeue();

                // Anything else leaves nothing to look for, leaves state the player spends -- where
                // never granted and already spent read the same -- or gets rebuilt at an act start.
                bool alreadyApplied = RecoveryOf(nextItem.Item) switch
                {
                    Recovery.Counted => CountedItemAlreadyApplied(nextItem.Item, requeuedCounts),
                    Recovery.Checked => VerifyItem(nextItem),
                    _ => true
                };

                if (alreadyApplied) continue;

                ArchipelagoModPlugin.Log.LogWarning($"Item ID {nextItem.ItemId} ({nextItem.ItemName}) didn't apply properly. Retrying...");
                missing.Add(nextItem);
            }

            return missing;
        }

        // How the recovery pass gets an item's effect back when the save no longer shows it.
        private enum Recovery
        {
            // Cannot be lost on its own: the item writes no state, and its effect is read from the
            // received list wherever it is needed.
            NoneNeeded,
            // One copy leaves no trace distinguishable from another's, so the shortfall is only visible
            // as a tally against how many were sent. Needs an entry in countedItemTallies.
            Counted,
            // VerifyItem has a check for the state this item writes.
            Checked,
            // Game state kept out of VerifyItem deliberately, because starting a run or an act
            // recomputes it from the received count, or something reconciles it where it is used.
            RebuiltElsewhere
        }

        // Every item has to name one. No default arm, and CS8509 is an error in this project, so a
        // new APItem does not compile until somebody has decided which of these it is.
        private static Recovery RecoveryOf(APItem item) => item switch
        {
            // Story events, which is what VerifyItem looks at for most of these. Several also grant a
            // card or a consumable, and those ride along on the event rather than being checked.
            APItem.StinkbugCard or APItem.StuntedWolfCard or APItem.SkinkCard or APItem.AntCards
                or APItem.CagedWolfCard or APItem.SquirrelTotemHead or APItem.Dagger or APItem.FilmRoll
                or APItem.Ring or APItem.CabinCloverPlant or APItem.ExtraCandle or APItem.BeeFigurine
                or APItem.GreaterSmoke or APItem.AnglerHook or APItem.PileOfMeat or APItem.Monocle
                or APItem.AncientObol or APItem.BoneLordFemur or APItem.GBCCloverPlant
                or APItem.MycologistsHoloKey or APItem.BoneLordHoloKey or APItem.FoulBackwaterShortcut
                or APItem.FilthyCorpseWorldShortcut or APItem.GaudyGemLandShortcut or APItem.GemsModule
                or APItem.ResplendentBastionGate or APItem.LonelyWizbotCard or APItem.FishbotCard
                or APItem.Ourobot => Recovery.Checked,
            
            // Act 2's collected pixel cards.
            APItem.BoneLordHorn or APItem.GreatKrakenCard or APItem.DrownedSoulCard
                or APItem.SalmonCard => Recovery.Checked,
            
            // Flags on the save that VerifyItem reads one by one.
            APItem.EpitaphPiece or APItem.EpitaphPieces or APItem.CameraReplica or APItem.MrsBombRemote
                or APItem.ExtraBattery or APItem.NanoArmorGenerator or APItem.Quill
                or APItem.MagnificusEye => Recovery.Checked,

            APItem.HoloPelt or APItem.VesselUpgrade or APItem.ConduitUpgrade
                or APItem.Act1Currency or APItem.Act2Currency or APItem.Act3Currency
                or APItem.Act1CardPack or APItem.Act2CardPack or APItem.Act3CardPack
                or APItem.SmallerBackpackChallenge or APItem.PriceyPeltsChallenge
                or APItem.BossTotemsChallenge or APItem.AllTotemBattlesChallenge
                or APItem.TippedScalesChallenge or APItem.MoreDifficultChallenge
                or APItem.ProgressiveGrizzlies or APItem.ProgressiveCandle
                or APItem.ProgressiveSquirrel or APItem.BleachTrap or APItem.ReinforcementsTrap
                or APItem.TrashTrap => Recovery.Counted,

            // Read from the received list at the point of use, so they hold no state of their own for a
            // save to lose. COUNT rides along as the enum's sentinel, which is never received at all.
            APItem.WardrobeKey or APItem.WoodcarverNode or APItem.MycologistsNode or APItem.BoneAltarNode
                or APItem.SacrificeStonesNode or APItem.BackpackNode or APItem.CampfireNode
                or APItem.GoobertNode or APItem.GBCBridgeRepair or APItem.InspectometerBattery
                or APItem.FactoryBridgeRepair or APItem.Hammer or APItem.Act1 or APItem.Act2 or APItem.Act3
                or APItem.COUNT => Recovery.NoneNeeded,

            // Counts up in Archipelago's own data, which cannot come apart from the record of having
            // received it, and tops its collection back up whenever the deck building menu is opened.
            APItem.DeckSizeTrap => Recovery.RebuiltElsewhere,

        };

        // The shortfall is shared by every copy, so the copies this pass has already queued are allowed
        // for before deciding whether this one is still needed.
        private static bool CountedItemAlreadyApplied(APItem item, Dictionary<APItem, int> requeuedCounts)
        {
            if (!countedItemTallies.TryGetValue(item, out Func<int> tally)) return true;

            requeuedCounts.TryGetValue(item, out int alreadyRequeued);

            if (tally() + alreadyRequeued >= CountReceived(item)) return true;

            requeuedCounts[item] = alreadyRequeued + 1;

            return false;
        }

        // Only for items whose effect one copy either has or has not left behind. Counted ones can
        // only be checked as a group, so their shortfall is worked out in the caller instead.
        internal static bool VerifyItem(InscryptionItemInfo item)
        {
            APItem receivedItem = item.Item;

            if (itemStoryPairs.TryGetValue(receivedItem, out StoryEvent storyEvent) && !StoryEventsData.EventCompleted(storyEvent))
            {
                return false;
            }

            if (itemPixelCardPair.TryGetValue(receivedItem, out string cardName) && !SaveManager.SaveFile.gbcCardsCollected.Contains(cardName))
            {
                return false;
            }

            if (receivedItem.ToString().Contains("Epitaph"))
            {
                int pieceCount = 0;

                if (receivedItem == APItem.EpitaphPiece)
                    pieceCount = ArchipelagoData.Data.receivedItems.Count(i => i.Item == APItem.EpitaphPiece);
                else if (ArchipelagoOptions.epitaphPiecesRandomization == EpitaphPiecesRandomization.Groups)
                    pieceCount = ArchipelagoData.Data.receivedItems.Count(i => i.Item == APItem.EpitaphPieces) * 3;
                else
                    pieceCount = 9;

                for (int i = 0; i < pieceCount; i++)
                {
                    if (i >= 9) break;

                    if (!SaveData.Data.undeadTemple.epitaphPieces[i].found) return false;
                }
            }
            else if (receivedItem == APItem.CameraReplica && !SaveData.Data.natureTemple.hasCamera)
            {
                return false;
            }
            else if (receivedItem == APItem.MrsBombRemote && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.BombRemote))
            {
                return false;
            }
            else if (receivedItem == APItem.ExtraBattery && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.Battery))
            {
                return false;
            }
            else if (receivedItem == APItem.NanoArmorGenerator && !Part3SaveData.Data.unlockedItems.Contains(Part3SaveData.ItemUnlock.ShieldGenerator))
            {
                return false;
            }
            else if (receivedItem == APItem.Quill && !Part3SaveData.Data.foundUndeadTempleQuill)
            {
                return false;
            }
            // Losing the eye costs the run the clock's large compartment, so it is repaired -- except
            // after the dagger, which trades it for the choice of a new one, so a goat eye is an answer.
            else if (receivedItem == APItem.MagnificusEye
                && RunState.Run != null
                && RunState.Run.eyeState != EyeballState.Wizard
                && !(RunState.Run.storyEventsCompleted?.Contains(StoryEvent.SpecialDaggerUsed) ?? false))
            {
                return false;
            }

            return true;
        }

        internal static void ScoutChecks()
        {
            checkInfos.Clear();
            ArchipelagoClient.ScoutLocationsAsync(OnScoutDone);
        }

        private static void OnScoutDone(Dictionary<long, ScoutedItemInfo> packet)
        {
            foreach (ScoutedItemInfo scoutInfo in packet.Values)
            {
                checkInfos.Add((APCheck)(scoutInfo.LocationId - ID_OFFSET), new CheckInfo(
                    scoutInfo.LocationId,
                    scoutInfo.Player.Slot,
                    scoutInfo.Player.Name,
                    scoutInfo.ItemId,
                    scoutInfo.ItemDisplayName,
                    scoutInfo.Flags)
                );
            }
        }

        internal static int CountReceived(APItem item)
        {
            return ArchipelagoData.Data.receivedItems.Count(i => i.Item == item);
        }

        // Re-runs the connect-time item pass: everything already received is re-checked and
        // anything whose effect is now missing is applied again, before the caller's save.
        internal static void ReapplyReceivedItems()
        {
            foreach (InscryptionItemInfo item in ArchipelagoData.Data.receivedItems)
            {
                itemsToVerifyQueue.Enqueue(item);
            }

            // Applied here rather than left on itemQueue, so the caller's save includes them. Only
            // what this pass found missing: anything already queued is a new item still to announce.
            ApplyItemsSilently(TakeItemsNeedingReapply());
        }

        internal static void SendStoryCheckIfApplicable(StoryEvent storyEvent)
        {
            if (storyCheckPairs.TryGetValue(storyEvent, out APCheck check))
            {
                SendCheck(check);
            }
        }

        // Finishing an act hands over everything still uncollected in it. Location ids run act 1,
        // then 2, then 3, and the apworld supplies each act's span so it cannot desync.
        internal static void ReleaseAct(int act)
        {
            if (!ArchipelagoOptions.releaseOnActCompletion || ArchipelagoData.Data == null) return;

            int start = ArchipelagoOptions.actLocationStarts[act - 1];
            int count = ArchipelagoOptions.actLocationCounts[act - 1];
            int released = 0;

            for (int check = start; check < start + count; check++)
            {
                long checkID = ID_OFFSET + check;
                if (ArchipelagoData.Data.completedChecks.Contains(checkID)) continue;

                ArchipelagoData.Data.completedChecks.Add(checkID);
                released++;
            }

            if (released == 0) return;

            // Sent and saved once rather than per check, since a whole act goes at a time. The items
            // are announced as the server hands them out, so no summary is logged here.
            ArchipelagoClient.SendChecksToServerAsync();
            Singleton<ArchipelagoUI>.Instance.StartCoroutine(Singleton<ArchipelagoUI>.Instance.QueueSave());
        }

        internal static void SendCheck(APCheck check)
        {
            if (ArchipelagoData.Data == null) return;

            long checkID = ID_OFFSET + (long)check;

            if (!ArchipelagoData.Data.completedChecks.Contains(checkID))
            {
                ArchipelagoData.Data.completedChecks.Add(checkID);
                ArchipelagoClient.SendChecksToServerAsync();
                Singleton<ArchipelagoUI>.Instance.StartCoroutine(Singleton<ArchipelagoUI>.Instance.QueueSave());
            }
        }

        internal static bool HasCompletedCheck(APCheck check)
        {
            if (ArchipelagoData.Data == null) return false;

            long checkID = ID_OFFSET + (long)check;

            return ArchipelagoData.Data.completedChecks.Contains(checkID);
        }

        internal static bool HasItem(APItem item)
        {
            if (ArchipelagoData.Data == null) return false;

            return ArchipelagoData.Data.receivedItems.Any(x => x.Item == item);
        }

        internal static bool MetEpilogueRequirements()
        {
            int enabled = 0;
            if (ArchipelagoOptions.enableAct1) enabled++;
            if (ArchipelagoOptions.enableAct2) enabled++;
            if (ArchipelagoOptions.enableAct3) enabled++;
            int completed = 0;
            if (ArchipelagoData.Data.act1Completed) completed++;
            if (ArchipelagoData.Data.act2Completed) completed++;
            if (ArchipelagoData.Data.act3Completed) completed++;
            bool finished = ArchipelagoData.Data.goalType switch {
                Goal.OneAct => completed >= 1 || completed >= enabled,
                Goal.TwoActs => completed >= 2 || completed >= enabled,
                Goal.AllActs => completed >= enabled,
                _ => false,
            };
            return finished;
        }

        internal static void VerifyGoalCompletion()
        {
            if (ArchipelagoData.Data == null || ArchipelagoData.Data.goalCompletedAndSent) return;

            if (MetEpilogueRequirements() && (ArchipelagoOptions.skipEpilogue || ArchipelagoData.Data.epilogueCompleted)) 
                ArchipelagoClient.SendGoalCompleted();
        }

        internal static CheckInfo GetCheckInfo(APCheck check)
        {
            if (checkInfos.TryGetValue(check, out CheckInfo info))
            {
                return info;
            }

            CheckInfo basicInfo = new CheckInfo((int)check + ID_OFFSET, 0, "Player", 0, check.ToString(), ItemFlags.None);

            return basicInfo;
        }
    }

    internal struct CheckInfo
    {
        internal long checkId;
        internal int recipientId;
        internal string recipientName;
        internal long itemId;
        internal string itemName;
        internal ItemFlags category;

        public CheckInfo(long checkId, int recipientId, string recipientName, long itemId, string itemName, ItemFlags category)
        {
            this.checkId = checkId;
            this.recipientId = recipientId;
            this.recipientName = recipientName;
            this.itemId = itemId;
            this.itemName = itemName;
            this.category = category;
        }
    }

    internal struct UnlockableCardInfo
    {
        internal string[] cardsToUnlock;
        internal string[] rigDraws;
        internal bool isPart3;

        public UnlockableCardInfo(bool isPart3, string[] cardsToUnlock)
        {
            this.cardsToUnlock = cardsToUnlock;
            this.rigDraws = new string[0];
            this.isPart3 = isPart3;
        }

        public UnlockableCardInfo(bool isPart3, string[] cardsToUnlock, string[] rigDraws)
        {
            this.cardsToUnlock = cardsToUnlock;
            this.rigDraws = rigDraws;
            this.isPart3 = isPart3;
        }
    }
}
