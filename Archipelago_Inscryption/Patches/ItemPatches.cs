using Archipelago.MultiClient.Net.Models;
using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Helpers;
using DiskCardGame;
using GBC;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Archipelago_Inscryption.Patches
{
    [HarmonyPatch]
    internal class ItemPatches
    {
        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        static void SetEyeStateIfEyeReceived(RunState __instance)
        {
            if (ArchipelagoManager.HasItem(APItem.MagnificusEye))
            {
                __instance.eyeState = EyeballState.Wizard;
            }
        }

        // Safety net: logs instead of silently aborting the rest of SaveFile.Initialize() if any
        // postfix on this method throws.
        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyFinalizer]
        static Exception LogInitializeFailure(Exception __exception)
        {
            if (__exception != null)
            {
                ArchipelagoModPlugin.Log.LogError($"RunState.Initialize threw: {__exception}");
            }

            return null;
        }

        // Act 1 spends both of these within a run, so a new run restores them to everything the
        // act has been sent -- the same state an act reset produces. Without this a death takes
        // the run's teeth and any unopened packs with it.
        [HarmonyPatch(typeof(SaveFile), "NewPart1Run")]
        [HarmonyPostfix]
        static void InitializeAct1RunItems()
        {
            if (ArchipelagoData.Data == null || RunState.Run == null)
            {
                ArchipelagoModPlugin.Log.LogWarning($"InitializeAct1RunItems: skipped (ArchipelagoData.Data null: {ArchipelagoData.Data == null}, RunState.Run null: {RunState.Run == null})");
                return;
            }

            int previousCurrency = RunState.Run.currency;
            int receivedCount = ArchipelagoManager.CountReceived(APItem.Act1Currency);
            int newCurrency = receivedCount * ArchipelagoManager.CURRENCY_PER_ITEM;

            RunState.Run.currency = newCurrency;
            ArchipelagoData.Data.SetPacks(1, ArchipelagoManager.CountReceived(APItem.Act1CardPack));

            RandomizerHelper.RefreshPackPile();
            ArchipelagoData.SaveToFile();

            ArchipelagoModPlugin.Log.LogInfo($"InitializeAct1RunItems: ran. Act1Currency items received: {receivedCount}. Currency {previousCurrency} -> {RunState.Run.currency} (expected {newCurrency}).");
        }

        // The caller already ran RunState.Run.currency += excessDamage regardless of act; cancel
        // it before Act 2/3's own animation/grant runs, closing the window before it opens.
        [HarmonyPatch(typeof(Part3CombatPhaseManager), "VisualizeExcessLethalDamage")]
        [HarmonyPatch(typeof(PixelCombatPhaseManager), "VisualizeExcessLethalDamage")]
        [HarmonyPrefix]
        static void UndoAct1CurrencyLeakFromNonAct1Combat(int excessDamage)
        {
            if (RunState.Run != null)
            {
                RunState.Run.currency -= excessDamage;
            }
        }

        // Neither Initialize zeroes currency, so these postfixes are what establish it. They
        // restore the act's card packs alongside it, so an act reset needs no step of its own.
        [HarmonyPatch(typeof(Part3SaveData), "Initialize")]
        [HarmonyPostfix]
        static void InitializeAct3Items(Part3SaveData __instance)
        {
            if (ArchipelagoData.Data == null) return;

            __instance.currency = ArchipelagoManager.CountReceived(APItem.Act3Currency) * ArchipelagoManager.CURRENCY_PER_ITEM;
            ArchipelagoData.Data.SetPacks(3, ArchipelagoManager.CountReceived(APItem.Act3CardPack));

            RandomizerHelper.RefreshPackPile();
        }

        [HarmonyPatch(typeof(SaveData), "Initialize")]
        [HarmonyPostfix]
        static void InitializeItemNewGame(SaveData __instance)
        {
            if (ArchipelagoData.Data == null) return;

            __instance.currency = ArchipelagoManager.CountReceived(APItem.Act2Currency) * ArchipelagoManager.CURRENCY_PER_ITEM;
            ArchipelagoData.Data.SetPacks(2, ArchipelagoManager.CountReceived(APItem.Act2CardPack));

            RandomizerHelper.UpdatePackButtonEnabled();

            int pieceCount = 0;

            if (ArchipelagoOptions.epitaphPiecesRandomization == EpitaphPiecesRandomization.AllPieces)
                pieceCount = ArchipelagoData.Data.receivedItems.Count(item => item.Item == APItem.EpitaphPiece);
            else if (ArchipelagoOptions.epitaphPiecesRandomization == EpitaphPiecesRandomization.Groups)
                pieceCount = ArchipelagoData.Data.receivedItems.Count(item => item.Item == APItem.EpitaphPieces) * 3;
            else
                pieceCount = ArchipelagoManager.HasItem(APItem.EpitaphPieces) ? 9 : 0;

            for (int i = 0; i < pieceCount; i++)
            {
                if (i >= 9) break;

                SaveData.Data.undeadTemple.epitaphPieces[i].found = true;
            }

            if (ArchipelagoManager.HasItem(APItem.CameraReplica))
            {
                __instance.natureTemple.hasCamera = true;
            }

            if (SaveManager.SaveFile.gbcCardsCollected == null) return;

            if (ArchipelagoManager.HasItem(APItem.DrownedSoulCard))
                ArchipelagoManager.ApplyItemReceived(APItem.DrownedSoulCard);
            if (ArchipelagoManager.HasItem(APItem.SalmonCard))
                ArchipelagoManager.ApplyItemReceived(APItem.SalmonCard);
            if (ArchipelagoManager.HasItem(APItem.GreatKrakenCard))
                ArchipelagoManager.ApplyItemReceived(APItem.GreatKrakenCard);
            if (ArchipelagoManager.HasItem(APItem.BoneLordHorn))
                ArchipelagoManager.ApplyItemReceived(APItem.BoneLordHorn);
        }

        [HarmonyPatch(typeof(SaveFile), "ResetGBCSaveData")]
        [HarmonyPostfix]
        static void InitializeStoryEventsNewGame(SaveFile __instance)
        {
            KeyValuePair<APItem, StoryEvent>[] itemStoryEvents = {
                new KeyValuePair<APItem, StoryEvent>( APItem.PileOfMeat,                        StoryEvent.GBCDogFoodFound ),
                new KeyValuePair<APItem, StoryEvent>( APItem.Monocle,                           StoryEvent.GBCMonocleFound ),
                new KeyValuePair<APItem, StoryEvent>( APItem.AncientObol,                       StoryEvent.GBCObolFound ),
                new KeyValuePair<APItem, StoryEvent>( APItem.BoneLordFemur,                     StoryEvent.GBCBoneFound ),
                new KeyValuePair<APItem, StoryEvent>( APItem.GBCCloverPlant,                    StoryEvent.GBCCloverFound )
            };

            for (int i = 0; i < itemStoryEvents.Length; i++)
            {
                if (ArchipelagoManager.HasItem(itemStoryEvents[i].Key))
                    StoryEventsData.SetEventCompleted(itemStoryEvents[i].Value, false, false);
            }
        }

        [HarmonyPatch(typeof(RunState), "InitializeStarterDeckAndItems")]
        [HarmonyPostfix]
        static void AddInsectTotemHeadIfNeeded(RunState __instance)
        {
            if (StoryEventsData.EventCompleted(StoryEvent.BeeFigurineFound) && !__instance.totemTops.Contains(Tribe.Insect))
            {
                __instance.totemTops.Add(Tribe.Insect);
            }
        }

        [HarmonyPatch(typeof(WolfTalkingCard), "get_OnDrawnDialogueId")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> IDontNeedYourReminderJustShutUp(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            int codeIndex = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "WolfFilmRollReminder");

            codes.RemoveAt(codeIndex);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(WolfTalkingCard), "OnDrawnFallbackDialogueId"))
            };

            codes.InsertRange(codeIndex, newCodes);

            return codes.AsEnumerable();
        }

        // The pile lives beside the rulebook in both acts and persists with the room, so it is
        // built once here instead of on entering the deck view.
        [HarmonyPatch(typeof(CabinRulebookInteractable), "Start")]
        [HarmonyPostfix]
        static void CreateCabinPackPile(CabinRulebookInteractable __instance)
        {
            if (ArchipelagoData.Data == null) return;

            CabinPackPile.Create(__instance.transform, SaveManager.SaveFile.IsPart3 ? 3 : 1);
        }

        [HarmonyPatch(typeof(HammerButton), "Start")]
        [HarmonyPostfix]
        static void DisableHammerButton(HammerButton __instance)
        {
            if (ArchipelagoOptions.randomizeHammer != RandomizeHammer.Vanilla)
                if (!ArchipelagoManager.HasItem(APItem.Hammer))
                    __instance.button.gameObject.SetActive(false);
        }

        [HarmonyPatch(typeof(HammerItemSlot), "InitializeHammer")]
        [HarmonyPrefix]
        static bool DisablePart3Hammer(HammerItemSlot __instance)
        {
            if (ArchipelagoOptions.randomizeHammer != RandomizeHammer.Vanilla)
                if (!ArchipelagoManager.HasItem(APItem.Hammer))
                {
                    return false;
                }
            return true;
        }

        [HarmonyPatch(typeof(HammerItemSlot), "CleanupHammer")]
        [HarmonyPrefix]
        static bool FixPart3Cleanup(HammerItemSlot __instance)
        {
            if (__instance.Item == null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(DeckBuildingUI), "OnNamePanelPressed")]
        [HarmonyPrefix]
        static bool TrashTrapDontRemoveBrokenEgg(CardNamePanel panel)
        {
            if (panel.nameText.Text == "Broken Egg")
            {
                AudioController.Instance.PlaySound2D("toneless_negate", MixerGroup.None, 0.3f, 0f, null, new AudioParams.Repetition(0.1f, ""), null, null, false);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(DeckBuildingUI), "OnClearDeckPressed")]
        [HarmonyPrefix]
        static bool TrashTrapDontClearBrokenEgg(DeckBuildingUI __instance)
        {
            AudioController.Instance.PlaySound2D("chipDelay_2", MixerGroup.GBCSFX, 0.75f, 0f, null, null, null, null, false);
            AudioController.Instance.PlaySound2D("toneless_plonklow", MixerGroup.GBCSFX, 0.4f, 0f, null, null, null, null, false);
            foreach (CardInfo card in new List<CardInfo>(SaveData.Data.deck.Cards))
            {
                if (card.name != "BrokenEgg")
                    SaveData.Data.deck.RemoveCard(card);
            }
            __instance.UpdatePanelContents();
            return false;
        }


        [HarmonyPatch(typeof(GBCEncounterManager), "LoadOverworldScene")]
        [HarmonyPrefix]
        static void TrashTrapRemoveOneBrokenEgg(GBCEncounterManager __instance)
        {
            CardInfo egg = SaveData.Data.deck.Cards.Find(card => card.name == "BrokenEgg");
            if (egg != null)
                SaveData.Data.deck.RemoveCard(egg);
        }

        [HarmonyPatch(typeof(TurnManager), "DoCombatPhase")]
        [HarmonyPrefix]
        static void ProcessBleachTrapOnBellRing(TurnManager __instance)
        {
            if (ArchipelagoData.Data.bleachTrapCount > 0)
            {
                if (RandomizerHelper.BleachTrapRemoveSigils())
                    ArchipelagoData.Data.bleachTrapCount--;
            }
        }

        [HarmonyPatch(typeof(Opponent), "QueueNewCards")]
        [HarmonyPostfix]
        static IEnumerator ProcessReinforcementsTrapOnUpkeep(IEnumerator __result, Opponent __instance)
        {
            while (__result.MoveNext())
                yield return __result.Current;
            if (ArchipelagoData.Data.reinforcementsTrapCount > 0)
            {
                yield return new WaitForSeconds(0.5f);
                List<CardSlot> opponentSlots = Singleton<BoardManager>.Instance.OpponentSlotsCopy;
                opponentSlots.RemoveAll((CardSlot x) => __instance.queuedCards.Find((PlayableCard y) => y.QueuedSlot == x));
                if (opponentSlots.Count != 0)
                {
                    int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
                    int seed2 = __instance.NumTurnsTaken;
                    seed += seed2 * 37;
                    if (SaveManager.SaveFile.IsPart1)
                    {
                        List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.portraitTex != null && x.temple == CardTemple.Nature);
                        foreach (CardSlot cardSlot in opponentSlots)
                        {
                            yield return __instance.QueueCard(CardLoader.Clone(list[SeededRandom.Range(0, list.Count, seed++)]), cardSlot, true, false, true);
                            seed += seed2 * 23;
                            seed2++;
                        }
                        ArchipelagoData.Data.reinforcementsTrapCount--;
                    }
                    else if (SaveManager.SaveFile.IsPart2)
                    {
                        List<CardInfo> list = CardLoader.GetPixelCards();
                        foreach (CardSlot cardSlot in opponentSlots)
                        {
                            yield return __instance.QueueCard(CardLoader.Clone(list[SeededRandom.Range(0, list.Count, seed++)]), cardSlot, false, false, true);
                            seed += seed2 * 23;
                            seed2++;
                        }
                        ArchipelagoData.Data.reinforcementsTrapCount--;
                    }
                    else if (SaveManager.SaveFile.IsPart3)
                    {
                        List<CardInfo> list = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.portraitTex != null && x.temple == CardTemple.Tech);
                        foreach (CardSlot cardSlot in opponentSlots)
                        {
                            yield return __instance.QueueCard(CardLoader.Clone(list[SeededRandom.Range(0, list.Count, seed++)]), cardSlot, true, false, true);
                            seed += seed2 * 23;
                            seed2++;
                        }
                        ArchipelagoData.Data.reinforcementsTrapCount--;
                    }
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        [HarmonyPatch(typeof(DeckBuildingUI), "UpdatePanelContents")]
        [HarmonyPostfix]
        static void IncreaseMinDeckSizeInMenu(DeckBuildingUI __instance)
        {
            while (SaveData.Data.collection.cardIds.Count < 20 + ArchipelagoData.Data.deckSizeTrapCount
                && SaveData.Data.collection.cardIds.Count != 0)
            {
                SaveData.Data.collection.AddCard(CardLoader.GetCardByName("DausBell"));
            }
            int deckSize = 20 + ArchipelagoData.Data.deckSizeTrapCount;
            __instance.cardCountText.SetText(SaveData.Data.deck.Cards.Count + "/" + deckSize, false);
            __instance.autoCompleteButton.SetEnabled(SaveData.Data.deck.Cards.Count < deckSize);
            __instance.collection.RefreshPage();
        }

        [HarmonyPatch(typeof(DeckInfo), "IsValidGBCDeck")]
        [HarmonyPostfix]
        static void IsValidGBCDeck(DeckInfo __instance, ref bool __result)
        {
            __result = __instance.cardIds.Count >= 20 + ArchipelagoData.Data.deckSizeTrapCount;
        }

        [HarmonyPatch(typeof(AutoDeckBuilder), "CompleteDeck")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AutoCompleteFullDeck(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var found = false;
            foreach (var instruction in instructions)
            {
                if (instruction.LoadsConstant(20))
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RandomizerHelper), "NewDeckSize"));
                    found = true;
                }
                else
                    yield return instruction;
            }
            if (!found)
            {
                ArchipelagoModPlugin.Log.LogError($"Cannot find List<CardInfo>::get_Count in {__originalMethod.Name}");
            }
        }

        [HarmonyPatch(typeof(MapDataReader), "SpawnAndPlaceElement")]
        [HarmonyPostfix]
        static void ReplaceLockedNodeIcon(MapElementData data, GameObject __result)
        {
            if (!ArchipelagoOptions.randomizeNodes) return;
            if ((data is CardMergeNodeData && !ArchipelagoManager.HasItem(APItem.SacrificeStonesNode))
                || (data is DuplicateMergeNodeData && !ArchipelagoManager.HasItem(APItem.MycologistsNode))
                || (data is CardRemoveNodeData && !ArchipelagoManager.HasItem(APItem.BoneAltarNode))
                || (data is CardStatBoostNodeData && !ArchipelagoManager.HasItem(APItem.CampfireNode))
                || (data is GainConsumablesNodeData && !ArchipelagoManager.HasItem(APItem.BackpackNode))
                || (data is BuildTotemNodeData && !ArchipelagoManager.HasItem(APItem.WoodcarverNode))
                || (data is CopyCardNodeData && !ArchipelagoManager.HasItem(APItem.GoobertNode)))
            {
                var sprite = __result.GetComponentInChildren<AnimatingSprite>();
                sprite.textureFrames = AssetsManager.lockedNodeFrames;
                sprite.SetFrame(0);
            }
        }

        [HarmonyPatch(typeof(GameFlowManager), "DoTransitionSequence")]
        [HarmonyPostfix]
        static IEnumerator SkipAct1Nodes(IEnumerator __result, GameState gameState, NodeData triggeringNodeData)
        {
            if (gameState == GameState.SpecialCardSequence && ArchipelagoOptions.randomizeNodes)
            {
                if ((triggeringNodeData is CardMergeNodeData && !ArchipelagoManager.HasItem(APItem.SacrificeStonesNode))
                || (triggeringNodeData is DuplicateMergeNodeData && !ArchipelagoManager.HasItem(APItem.MycologistsNode))
                || (triggeringNodeData is CardRemoveNodeData && !ArchipelagoManager.HasItem(APItem.BoneAltarNode))
                || (triggeringNodeData is CardStatBoostNodeData && !ArchipelagoManager.HasItem(APItem.CampfireNode))
                || (triggeringNodeData is GainConsumablesNodeData && !ArchipelagoManager.HasItem(APItem.BackpackNode))
                || (triggeringNodeData is BuildTotemNodeData && !ArchipelagoManager.HasItem(APItem.WoodcarverNode))
                || (triggeringNodeData is CopyCardNodeData && !ArchipelagoManager.HasItem(APItem.GoobertNode)))
                {
                    yield return new WaitForSeconds(0.05f);
                    PaperGameMap gameMap = PaperGameMap.Instance;
                    MapDataReader dataReader = new MapDataReader();
                    Vector2 sampleRange = new Vector2(gameMap.mapProgress, gameMap.mapProgress + 1f);
                    List<MapNode> nodeList = gameMap.GetComponentsInChildren<MapNode>().ToList();
                    List<PathSegment> pathList = gameMap.GetComponentsInChildren<PathSegment>().ToList();
                    MapNode currentNode = nodeList.Find((MapNode x) => x.Data.id == RunState.Run.currentNodeId);
                    if (currentNode != null)
                    {
                        dataReader.SetNodeAndPathColors(nodeList, pathList, currentNode);
                    }
                    Singleton<MapNodeManager>.Instance.FindAndSetActiveNodeInteractable();
                    SaveManager.SaveToFile(true);
                    yield break;
                }
                else
                {
                    while (__result.MoveNext())
                        yield return __result.Current;
                }
            }
            else
            {
                while (__result.MoveNext())
                    yield return __result.Current;
            }
        }

        [HarmonyPatch(typeof(NodeData.IsAscension), "Satisfied")]
        [HarmonyPostfix]
        static void MakeGoobertNodeAppear(ref bool __result)
        {
            if (ArchipelagoOptions.randomizeNodes)
            {
                __result = true;
            }
        }

        // Vanilla only preloads challenge data in the Kaycee's Mod menu, which an Act 1 run never
        // visits, so showing the challenge array below would otherwise load it on first pause.
        [HarmonyPatch(typeof(Part1GameFlowManager), "Awake")]
        [HarmonyPostfix]
        static void PreloadKayceeChallengeData()
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable)
                ScriptableObjectLoader<AscensionChallengeInfo>.LoadData(AscensionChallengesUtil.DATA_PATH);
        }

        [HarmonyPatch(typeof(PauseMenu3D), "Start")]
        [HarmonyPrefix]
        static bool ShowKayceeChallengesInPauseMenu1(PauseMenu3D __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable &&
                SaveManager.SaveFile.IsPart1)
            {
                __instance.ascensionRunInfoBar.SetActive(true);
                __instance.ascensionChallengeArray.gameObject.SetActive(true);
                __instance.menuController.transform.localPosition = new Vector3(0f, __instance.ascensionMenuYOffset, 0f);
                __instance.optionsUIPanel.transform.localPosition = new Vector3(0f, -__instance.ascensionMenuYOffset, 0f);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(PauseMenu3D), "Update")]
        [HarmonyPrefix]
        static bool ShowKayceeChallengesInPauseMenu2(PauseMenu3D __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable &&
                SaveManager.SaveFile.IsPart1)
            {
                __instance.ascensionChallengeArray.SetIconsEnabled(!__instance.optionsMenuParent.activeSelf);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        static void SetChallengesOnStartup(RunState __instance)
        {
            // SaveFile.Initialize reaches RunState.Initialize before it creates ascensionData, and
            // runs before a save slot is picked, so throwing here would abort building the save.
            if (AscensionSaveData.Data == null || ArchipelagoData.Data == null) return;

            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable)
            {
                AscensionSaveData.Data.activeChallenges = new List<AscensionChallenge>();
                if (!ArchipelagoManager.HasItem(APItem.SmallerBackpackChallenge))
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.LessConsumables);
                if (!ArchipelagoManager.HasItem(APItem.PriceyPeltsChallenge))
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.ExpensivePelts);
                if (!ArchipelagoManager.HasItem(APItem.BossTotemsChallenge))
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.BossTotems);
                if (!ArchipelagoManager.HasItem(APItem.TippedScalesChallenge))
                {
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                }
                else if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.TippedScalesChallenge) == 1)
                {
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                }
                else
                {
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.StartingDamage);
                }
                if (!ArchipelagoManager.HasItem(APItem.AllTotemBattlesChallenge))
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.AllTotems);
                if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.MoreDifficultChallenge) == 0)
                {
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.BaseDifficulty);
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.BaseDifficulty);
                }
                else if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.MoreDifficultChallenge) == 1)
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.BaseDifficulty);
                if (!ArchipelagoManager.HasItem(APItem.ProgressiveCandle))
                {
                    RunState.Run.maxPlayerLives = 1;
                    RunState.Run.playerLives = 1;
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.LessLives);
                }
                if (!ArchipelagoManager.HasItem(APItem.ProgressiveSquirrel))
                    AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.SubmergeSquirrels);
                if (ArchipelagoOptions.randomizeChallenges == RandomizeChallenges.Randomize)
                {
                    if (!ArchipelagoManager.HasItem(APItem.ProgressiveGrizzlies))
                    {
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                    }
                    else if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.ProgressiveGrizzlies) == 1)
                    {
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                    }
                    else if (ArchipelagoData.Data.receivedItems.Count(x => x.Item == APItem.ProgressiveGrizzlies) == 2)
                    {
                        AscensionSaveData.Data.activeChallenges.Add(AscensionChallenge.GrizzlyMode);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AscensionUnlockSchedule), "ChallengeIsUnlockedForLevel")]
        [HarmonyPostfix]
        static void UnlockAllChallenges(ref bool __result)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable)
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(AscensionSaveData), "GetNumChallengesOfTypeActive")]
        [HarmonyPostfix]
        static void NotRequireAscensionForChallenges(AscensionChallenge challenge, ref int __result, AscensionSaveData __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable &&
                SaveManager.SaveFile.IsPart1)
            {
                __result = __instance.activeChallenges.FindAll((AscensionChallenge x) => x == challenge).Count; ;
            }
        }

        [HarmonyPatch(typeof(TurnManager), "SetupPhase")]
        [HarmonyPostfix]
        static IEnumerator ExtraTippedScales(IEnumerator __result)
        {
            while (__result.MoveNext())
                yield return __result.Current;
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable &&
                SaveManager.SaveFile.IsPart1)
            {
                List<AscensionChallenge> challenges = [.. AscensionSaveData.Data.activeChallenges];
                challenges.RemoveAll((AscensionChallenge x) => x is not AscensionChallenge.StartingDamage);
                if (challenges.Contains(AscensionChallenge.StartingDamage))
                {
                    challenges.Remove(AscensionChallenge.StartingDamage);
                    foreach (AscensionChallenge i in challenges)
                    {
                        if (i == AscensionChallenge.StartingDamage)
                            yield return Singleton<LifeManager>.Instance.ShowDamageSequence(1, 1, true, 0.125f, null, 0f, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Part1BossOpponent), "HasGrizzlyGlitchPhase")]
        [HarmonyPostfix]
        static void DoProgressiveGrizzlies(ref bool __result, Part1BossOpponent __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges == RandomizeChallenges.Randomize)
            {
                int grizzlyChallenges = 0;
                if (__instance is ProspectorBossOpponent)
                    grizzlyChallenges = 3;
                if (__instance is AnglerBossOpponent)
                    grizzlyChallenges = 2;
                if (__instance is TrapperTraderBossOpponent)
                    grizzlyChallenges = 1;
                __result = AscensionSaveData.Data.GetNumChallengesOfTypeActive(AscensionChallenge.GrizzlyMode) >= grizzlyChallenges;
            }
        }

        [HarmonyPatch(typeof(RunState), "InitializeStarterDeckAndItems")]
        [HarmonyPostfix]
        static void RemoveSquirrelBottleIfSmallerBackpack(RunState __instance)
        {
            if (StoryEventsData.EventCompleted(StoryEvent.FishHookUnlocked) && AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.LessConsumables))
            {
                __instance.consumables.Remove("SquirrelBottle");
            }
        }

        [HarmonyPatch(typeof(BrokenBridgeEntrance), "BridgeFixed")]
        [HarmonyPostfix]
        static void ApplyAct2BridgeRando(ref bool __result, BrokenBridgeEntrance __instance)
        {
            if (ArchipelagoOptions.act2RandomizeBridge != Act2RandomizeBridge.Disable)
            {
                __result = ArchipelagoManager.HasItem(APItem.GBCBridgeRepair);
            }
        }

        [HarmonyPatch(typeof(SceneTransitionVolume), "OnPlayerCollision")]
        [HarmonyPrefix]
        static void GoToLeftSideAct2(SceneTransitionVolume __instance)
        {
            if (ArchipelagoOptions.act2RandomizeBridge == Act2RandomizeBridge.LeftSideStart)
            {
                if (SceneManager.GetActiveScene().name == "GBC_Starting_Island")
                {
                    SaveData.Data.overworldNode = "Tech Elevator";
                }
            }
        }

        [HarmonyPatch(typeof(NavigationZone2D), "CanMoveInDirection")]
        [HarmonyPostfix]
        static void BlockBrokenBridgeFromOtherSide(ref bool __result, NavigationZone2D __instance, LookDirection dir)
        {
            if (ArchipelagoOptions.act2RandomizeBridge == Act2RandomizeBridge.LeftSideStart &&
                __instance.name == "WestBridge" && dir == LookDirection.East)
            {
                __result = ArchipelagoManager.HasItem(APItem.GBCBridgeRepair);
            }
        }

        [HarmonyPatch(typeof(BrokenBridgeAreaSequencer), "EasternBossCleared", MethodType.Getter)]
        [HarmonyPrefix]
        static bool FixAct3Bridge(ref bool __result)
        {
            if (ArchipelagoOptions.act3Overhaul)
            {
                __result = ArchipelagoManager.HasItem(APItem.FactoryBridgeRepair);
                return false;
            }
            return true;
        }

        // True only while OnTakenToGameTable's charging sequence is actively running -- can
        // never be true before the player is holding a battery, so this can't cause a softlock.
        static bool chargingBatteryInProgress = false;

        // The only two states that may lock the map. Deliberately omits vanilla's out-of-power
        // lockout, which would strand a player whose Inspectometer Battery item hasn't arrived.
        static bool MapLockedOut =>
            chargingBatteryInProgress
            || (StoryEventsData.EventCompleted(StoryEvent.DredgingRoomUnlocked)
                && !StoryEventsData.EventCompleted(StoryEvent.Part3MetScrybes));

        // Vanilla cuffs the player to the table until the holomap loses power, a fixed beat in
        // Act 3's intro. Act 1 does not hold the player through its intro, so Act 3 should not
        // either. Marking the event early hands the cuff to vanilla's own handling: the first
        // attempt plays the escape, later ones just stand up.
        //
        // Only safe with act3_overhaul, where GateMapScreenPower replaces PoweredOff with
        // MapLockedOut and the event no longer decides whether the map works. It also skips
        // PowerOutAreaSequencer's power-out cutscene, which is the intro beat being dropped.
        [HarmonyPatch(typeof(Part3GameFlowManager), "TransitionToFirstPerson")]
        [HarmonyPrefix]
        static void AllowLeavingTableEarly()
        {
            if (!ArchipelagoOptions.act3Overhaul) return;

            StoryEventsData.SetEventCompleted(StoryEvent.HoloMapOutOfPower);
        }

        [HarmonyPatch(typeof(HoloGameMap), "PoweredOff", MethodType.Getter)]
        [HarmonyPrefix]
        static bool GateMapScreenPower(ref bool __result)
        {
            if (!ArchipelagoOptions.act3Overhaul) return true;

            __result = MapLockedOut;
            return false;
        }

        // Marks charging as started and forces the screen dark immediately as the battery is
        // placed, matching vanilla.
        [HarmonyPatch(typeof(HoldableBattery), "OnTakenToGameTable")]
        [HarmonyPrefix]
        static void StartChargingBattery()
        {
            if (ArchipelagoOptions.act3Overhaul)
            {
                chargingBatteryInProgress = true;
                HoloGameMap.Instance.ShowPoweredOn(poweredOn: false);

                // Vanilla reaches charging with no area loaded, since PowerOutAreaSequencer
                // destroys it and a dead map can't respawn one. Our usable map can, so match it.
                HoloMapArea currentArea = Singleton<HoloMapAreaManager>.Instance.CurrentArea;
                if (currentArea != null) UnityEngine.Object.Destroy(currentArea.gameObject);
            }
        }

        [HarmonyPatch(typeof(HoloGameMap), "ShowPoweredOn")]
        [HarmonyPostfix]
        static void RestoreMapStateAfterPowerChange(bool poweredOn)
        {
            // OnTakenToGameTable's own final step, so a reliable "charging finished" signal
            // (unlike a postfix on the coroutine method, which fires at creation time).
            if (poweredOn) chargingBatteryInProgress = false;

            // ShowPoweredOn(false) hides the marker, so restore it whenever the map is still
            // usable -- otherwise it stays hidden on a map the player can freely reopen.
            if (ArchipelagoOptions.act3Overhaul && !MapLockedOut && PlayerMarker.Instance != null)
            {
                PlayerMarker.Instance.gameObject.SetActive(true);
            }
        }

        // Item randomization can hand out the battery before Part3MetScrybes, so
        // OnTakenToGameTable's own map-open call can retrigger this reminder mid-delivery.
        [HarmonyPatch(typeof(TextDisplayer), "PlayDialogueEvent")]
        [HarmonyPrefix]
        static bool SkipFixCameraReminderDuringCharging(string eventId, ref IEnumerator __result)
        {
            if (ArchipelagoOptions.act3Overhaul && chargingBatteryInProgress && eventId == "P03FixCameraReminder")
            {
                __result = SkippedDialogue();
                return false;
            }
            return true;
        }

        static IEnumerator SkippedDialogue()
        {
            yield break;
        }

        // Vanilla triggers this on area entry, so match that. MoveAreasSequence is the
        // player-driven move only; cutscene teleports use MoveToAreaDirectly and are skipped.
        [HarmonyPatch(typeof(HoloMapAreaManager), "MoveAreasSequence")]
        [HarmonyPostfix]
        static void CheckDredgingRoomUnlockOnAreaMove(ref IEnumerator __result)
        {
            __result = AppendDredgingRoomUnlockCheck(__result);
        }

        static IEnumerator AppendDredgingRoomUnlockCheck(IEnumerator original)
        {
            yield return original;

            if (!ArchipelagoOptions.act3Overhaul || StoryEventsData.EventCompleted(StoryEvent.DredgingRoomUnlocked))
            {
                yield break;
            }

            // The real sequencer only exists while its own area is loaded. It has no fields,
            // so one persistent instance on CustomCoroutine's utility object stands in for it.
            if (dredgingRoomSequencer == null)
            {
                dredgingRoomSequencer = CustomCoroutine.Instance.gameObject.AddComponent<UnlockDredgingRoomAreaSequencer>();
            }
            dredgingRoomSequencer.OnAreaEntered();
        }

        static UnlockDredgingRoomAreaSequencer dredgingRoomSequencer;

        // Replaces the TelegrapherDefeated requirement with "all 4 bosses defeated" -- covers
        // both our per-area check above and the vanilla walk-in path with one guard.
        [HarmonyPatch(typeof(UnlockDredgingRoomAreaSequencer), "CanTriggerSequence")]
        [HarmonyPrefix]
        static bool RequireAllBossesForDredgingRoomTrigger(ref bool __result)
        {
            if (ArchipelagoOptions.act3Overhaul)
            {
                __result = !StoryEventsData.EventCompleted(StoryEvent.DredgingRoomUnlocked)
                    && Part3SaveData.GetNumBossesDefeated() == 4;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HoloMapArea), "Start")]
        [HarmonyPostfix]
        static void ReworkInspectometerBattery(HoloMapArea __instance)
        {
            if (ArchipelagoOptions.act3Overhaul && __instance.name == "HoloMapArea_NeutralEastMain_7(Clone)")
            {
                if (!StoryEventsData.EventCompleted(StoryEvent.HoloMapBatteryFetched))
                {
                    __instance.ForceDisableDirectionNode(LookDirection.North);
                }
            }
        }
    }

    [HarmonyPatch]
    class AnglerHookRemovalPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(RunIntroSequencer).GetNestedType("<RunIntroSequence>d__1", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PreventFishHook(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.LoadsConstant(0x7A));

            index -= 2;

            codes.RemoveRange(index, 4);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldc_I4_1)
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

}

