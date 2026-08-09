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
            // Both upgrades append to the same list, so they are told apart by what they append. A
            // null list reads as nothing accounted for, which reapplying would then throw on, so it
            // reads as everything accounted for instead and leaves the upgrades alone.
            { APItem.VesselUpgrade,  () => Part3SaveData.Data.sideDeckAbilities?.Count(a => a != Ability.ConduitNull) ?? int.MaxValue },
            { APItem.ConduitUpgrade, () => Part3SaveData.Data.sideDeckAbilities?.Count(a => a == Ability.ConduitNull) ?? int.MaxValue }
        };

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
        // the server has sent is replayed, so what Archipelago granted survives and the rest does not.
        // Two things deliberately do not come back with it: checks the server already holds, which
        // cannot be unsent, and a DeathLink that arrived but has not been applied yet, which is owed.
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

            // Items the reloaded data already accounts for land in the verify queue and are only
            // reapplied if their effect is missing; ones the revert dropped are treated as new.
            VerifyAllItems();

            int reapplied = ApplyQueuedItemsSilently();
            if (reapplied > 0) SaveManager.SaveToFile(false);

            // The replay is deliberately silent, so this is the only trace it leaves.
            ArchipelagoModPlugin.Log.LogInfo($"Reverted unsaved progress. Replayed {received.Count} items, reapplied {reapplied}.");
        }

        // A replay restores items the player already watched arrive, so it skips the sound, the
        // on-screen log and the per-item delay that ProcessNextItem gives genuinely new ones.
        private static int ApplyQueuedItemsSilently()
        {
            int applied = 0;

            while (itemQueue.Count > 0)
            {
                InscryptionItemInfo item = itemQueue.Dequeue();

                // The scene is already unwinding when the revert runs this, and applying an item
                // reaches into live singletons, so one failure must not strand the rest of the queue.
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

        internal static void ApplyItemReceived(APItem receivedItem)
        {
            if (itemStoryPairs.TryGetValue(receivedItem, out StoryEvent storyEvent))
            {
                StoryEventsData.SetEventCompleted(storyEvent);
            }

            if (itemCardPair.TryGetValue(receivedItem, out UnlockableCardInfo info))
            {
                for (int i = 0; i < info.cardsToUnlock.Length; i++)
                {
                    var card = CardLoader.GetCardByName(info.cardsToUnlock[i]);
                    receivingCardForAct = info.isPart3 ? 3 : 1;
                    try {
                        CardPatches.RandomizeSigils(card);
                        (info.isPart3 ? SaveManager.SaveFile.part3Data.deck : RunState.Run.playerDeck).AddCard(card);
                    } finally {
                        receivingCardForAct = 0;
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

            if (receivedItem == APItem.Act1Currency)
            {
                RunState.Run.currency += CURRENCY_PER_ITEM;
            }
            else if (receivedItem == APItem.Act2Currency)
            {
                SaveData.Data.currency += CURRENCY_PER_ITEM;
            }
            else if (receivedItem == APItem.Act3Currency)
            {
                Part3SaveData.Data.currency += CURRENCY_PER_ITEM;
            }
            else if (receivedItem == APItem.Act1CardPack)
            {
                ArchipelagoData.Data.GrantPack(1);
                RandomizerHelper.RefreshPackPile();
            }
            else if (receivedItem == APItem.Act2CardPack)
            {
                ArchipelagoData.Data.GrantPack(2);
                RandomizerHelper.UpdatePackButtonEnabled();
            }
            else if (receivedItem == APItem.Act3CardPack)
            {
                ArchipelagoData.Data.GrantPack(3);
                RandomizerHelper.RefreshPackPile();
            }
            else if (receivedItem == APItem.TrashTrap)
            {
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
                if (TurnManager.Instance == null)
                {
                    ArchipelagoData.Data.bleachTrapCount++;
                }
                else if (TurnManager.Instance.IsPlayerTurn) {
                    if (!RandomizerHelper.BleachTrapRemoveSigils())
                        {
                            ArchipelagoData.Data.bleachTrapCount++;
                        }
                }
                else
                {
                    ArchipelagoData.Data.bleachTrapCount++;
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
                ArchipelagoData.Data.reinforcementsTrapCount++;
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
			    AscensionSaveData.Data.activeChallenges.Remove(AscensionChallenge.HarderDeckTrials);
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
            // Nothing this pass queues is applied until the pass is over, so a shortfall in a counted
            // item would otherwise read the same for every copy of it and requeue them all.
            Dictionary<APItem, int> requeuedCounts = new Dictionary<APItem, int>();

            while (itemsToVerifyQueue.Count() > 0)
            {
                InscryptionItemInfo nextItem = itemsToVerifyQueue.Dequeue();

                bool alreadyApplied;

                switch (RecoveryOf(nextItem.Item))
                {
                    case Recovery.Counted:
                        alreadyApplied = CountedItemAlreadyApplied(nextItem.Item, requeuedCounts);
                        break;
                    case Recovery.Checked:
                        alreadyApplied = VerifyItem(nextItem);
                        break;
                    default:
                        // The rest leave nothing to look for, or leave state the player spends, where
                        // never granted and already spent read the same, or get rebuilt at an act start.
                        alreadyApplied = true;
                        break;
                }

                if (alreadyApplied)
                {
                    continue;
                }

                ArchipelagoModPlugin.Log.LogWarning($"Item ID {nextItem.ItemId} ({nextItem.ItemName}) didn't apply properly. Retrying...");
                itemQueue.Enqueue(nextItem);
            }
        }

        // How the recovery pass gets an item's effect back when the save no longer shows it.
        private enum Recovery
        {
            // Cannot be lost on its own. Either the item writes no state and its effect is read from the
            // received list wherever it is needed, or its state is Archipelago's own data, which is
            // reverted and reloaded as one piece with the record of having received it.
            NoneNeeded,
            // One copy leaves no trace distinguishable from another's, so the shortfall is only visible
            // as a tally against how many were sent. Needs an entry in countedItemTallies.
            Counted,
            // VerifyItem has a check for the state this item writes.
            Checked,
            // Game state kept out of VerifyItem deliberately, because starting a run or an act
            // recomputes it from the received count, or something reconciles it where it is used.
            RebuiltElsewhere,
            // Granted once and deliberately never given back.
            NotRecovered
        }

        // Every item has to name one. This switch has no default arm and CS8509 is an error in this
        // project, so a new APItem does not compile until somebody has decided which of these it is --
        // which is the whole point, because the alternative is finding out from a player.
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
                or APItem.ResplendentBastionGate => Recovery.Checked,
            // Act 2's collected pixel cards and Act 3's deck cards.
            APItem.BoneLordHorn or APItem.GreatKrakenCard or APItem.DrownedSoulCard or APItem.SalmonCard
                or APItem.LonelyWizbotCard or APItem.FishbotCard or APItem.Ourobot => Recovery.Checked,
            // Flags on the save that VerifyItem reads one by one.
            APItem.EpitaphPiece or APItem.EpitaphPieces or APItem.CameraReplica or APItem.MrsBombRemote
                or APItem.ExtraBattery or APItem.NanoArmorGenerator or APItem.Quill
                or APItem.ProgressiveSquirrel => Recovery.Checked,

            APItem.HoloPelt or APItem.VesselUpgrade or APItem.ConduitUpgrade => Recovery.Counted,

            // Read from the received list where they are used, so there is nothing to put back. The two
            // traps are counters in Archipelago's data that battle code spends as it applies them.
            APItem.WardrobeKey or APItem.WoodcarverNode or APItem.MycologistsNode or APItem.BoneAltarNode
                or APItem.SacrificeStonesNode or APItem.BackpackNode or APItem.CampfireNode
                or APItem.GoobertNode or APItem.GBCBridgeRepair or APItem.InspectometerBattery
                or APItem.FactoryBridgeRepair or APItem.Hammer or APItem.Act1 or APItem.Act2 or APItem.Act3
                or APItem.BleachTrap or APItem.ReinforcementsTrap or APItem.COUNT => Recovery.NoneNeeded,

            // Currency and packs are set from the count when an act starts; the challenge items are
            // rebuilt wholesale from the same counts, which is also what fixes the candle and the eye;
            // the deck size trap tops its collection up to its counter in the deck building menu.
            APItem.Act1Currency or APItem.Act2Currency or APItem.Act3Currency or APItem.Act1CardPack
                or APItem.Act2CardPack or APItem.Act3CardPack or APItem.SmallerBackpackChallenge
                or APItem.PriceyPeltsChallenge or APItem.BossTotemsChallenge or APItem.TippedScalesChallenge
                or APItem.AllTotemBattlesChallenge or APItem.MoreDifficultChallenge
                or APItem.ProgressiveCandle or APItem.ProgressiveGrizzlies or APItem.MagnificusEye
                or APItem.DeckSizeTrap => Recovery.RebuiltElsewhere,

            // Whether the broken egg is still in the deck is not worth reasoning about, and a trap that
            // fails to come back is a trap the player got away with.
            APItem.TrashTrap => Recovery.NotRecovered
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

        // Only the modes that leave a deck's cards as themselves. The other two rebuild the deck from a
        // random pool on arriving at a node, and there the card items decide what that pool may hold
        // rather than what the deck holds, so what is in the deck says nothing about them.
        internal static bool DeckKeepsItsCards()
        {
            return ArchipelagoOptions.randomizeDeck == RandomizeDeck.Disable
                || ArchipelagoOptions.randomizeDeck == RandomizeDeck.StarterOnly;
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

            // Nothing else takes cards back out of the Act 3 deck, so a card missing from it was never
            // added. Ourobot has no story event of its own, making this the only trace it leaves; the
            // other two are covered twice over.
            if (DeckKeepsItsCards() && itemCardPair.TryGetValue(receivedItem, out UnlockableCardInfo part3Cards) && part3Cards.isPart3)
            {
                List<CardInfo> deck = Part3SaveData.Data.deck?.Cards;

                foreach (string part3CardName in part3Cards.cardsToUnlock)
                {
                    if (deck == null || !deck.Exists(card => card.name == part3CardName)) return false;
                }
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
            // The second squirrel is what stands in for the bee figurine, and reapplying either copy
            // is harmless: the challenge removal, the event and the totem top are all guarded.
            else if (receivedItem == APItem.ProgressiveSquirrel
                && CountReceived(APItem.ProgressiveSquirrel) >= 2
                && !StoryEventsData.EventCompleted(StoryEvent.BeeFigurineFound))
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

        // Re-runs the connect-time item pass: everything already received is re-checked and
        // anything whose effect is now missing gets applied again. Used after resetting an act, so
        // the act comes back in the state a brand new save would reach once its items arrive.
        internal static int CountReceived(APItem item)
        {
            return ArchipelagoData.Data.receivedItems.Count(i => i.Item == item);
        }

        internal static void ReapplyReceivedItems()
        {
            foreach (InscryptionItemInfo item in ArchipelagoData.Data.receivedItems)
            {
                itemsToVerifyQueue.Enqueue(item);
            }

            VerifyAllItems();
        }

        internal static void SendStoryCheckIfApplicable(StoryEvent storyEvent)
        {
            if (storyCheckPairs.TryGetValue(storyEvent, out APCheck check))
            {
                SendCheck(check);
            }
        }

        // With release on act completion, finishing an act hands over everything still uncollected
        // in it. Location ids run act 1, then 2, then 3, so an act is a contiguous run of APCheck;
        // the counts come from the apworld so adding a location there cannot desync the range.
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

            // Sent and saved once rather than per check, since a whole act goes at a time. No
            // summary is logged: the items themselves are announced as the server hands them
            // out, the same as any other release.
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
