using Archipelago.MultiClient.Net.Models;
using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Helpers;
using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

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

        [HarmonyPatch(typeof(SaveData), "Initialize")]
        [HarmonyPostfix]
        static void InitializeItemNewGame(SaveData __instance)
        {
            if (ArchipelagoData.Data == null) return;

            List<InscryptionItemInfo> receivedItem = ArchipelagoData.Data.receivedItems;
            int countCurrency = receivedItem.Count(item => item.Item == APItem.Currency);
            __instance.currency = countCurrency;

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

        [HarmonyPatch(typeof(DeckReviewSequencer), "OnEnterDeckView")]
        [HarmonyPostfix]
        static void SpawnCardPackPile(DeckReviewSequencer __instance)
        {
            if (ArchipelagoData.Data.availableCardPacks <= 0 || Singleton<GameFlowManager>.Instance.CurrentGameState != GameState.Map || !Singleton<GameMap>.Instance.FullyUnrolled) return;

            RandomizerHelper.SpawnPackPile(__instance);
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), "OnEnterDeckView")]
        [HarmonyPostfix]
        static void SpawnCardPackPile(Part3DeckReviewSequencer __instance)
        {
            if (!StoryEventsData.EventCompleted(StoryEvent.GemsModuleFetched) || ArchipelagoData.Data.availableCardPacks <= 0 || Singleton<GameFlowManager>.Instance.CurrentGameState != GameState.Map) return;

            RandomizerHelper.SpawnPackPile(__instance);
        }

        [HarmonyPatch(typeof(DeckReviewSequencer), "OnExitDeckView")]
        [HarmonyPostfix]
        static void DestroyCardPackPile(DeckReviewSequencer __instance)
        {
            RandomizerHelper.DestroyPackPile();
        }

        [HarmonyPatch(typeof(Part3DeckReviewSequencer), "OnExitDeckView")]
        [HarmonyPostfix]
        static void DestroyPart3CardPackPile(Part3DeckReviewSequencer __instance)
        {
            RandomizerHelper.DestroyPackPile();
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
                if (!ArchipelagoManager.HasItem(APItem.Hammer)) {
                    return false;
                }
            return true;
        }

        [HarmonyPatch(typeof(HammerItemSlot), "CleanupHammer")]
        [HarmonyPrefix]
        static bool FixPart3Cleanup(HammerItemSlot __instance)
        {
            if (__instance.Item == null) {
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
                    seed += seed2*37;
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
            foreach(var instruction in instructions)
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
            if (!ArchipelagoOptions.act1RandomizeNodes) return;
            if ((data is CardMergeNodeData && !ArchipelagoManager.HasItem(APItem.SacrificeStonesNode))
                || (data is DuplicateMergeNodeData && !ArchipelagoManager.HasItem(APItem.MycologistsNode))
                || (data is CardRemoveNodeData && !ArchipelagoManager.HasItem(APItem.BoneAltarNode))
                || (data is CardStatBoostNodeData && !ArchipelagoManager.HasItem(APItem.CampfireNode))
                || (data is GainConsumablesNodeData && !ArchipelagoManager.HasItem(APItem.BackpackNode))
                || (data is BuildTotemNodeData && !ArchipelagoManager.HasItem(APItem.WoodcarverNode))
                || (data is TradePeltsNodeData && !ArchipelagoManager.HasItem(APItem.TraderNode))
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
            if (gameState == GameState.SpecialCardSequence && ArchipelagoOptions.act1RandomizeNodes) {
			    if ((triggeringNodeData is CardMergeNodeData && !ArchipelagoManager.HasItem(APItem.SacrificeStonesNode))
			    || (triggeringNodeData is DuplicateMergeNodeData && !ArchipelagoManager.HasItem(APItem.MycologistsNode))
			    || (triggeringNodeData is CardRemoveNodeData && !ArchipelagoManager.HasItem(APItem.BoneAltarNode))
			    || (triggeringNodeData is CardStatBoostNodeData && !ArchipelagoManager.HasItem(APItem.CampfireNode))
			    || (triggeringNodeData is GainConsumablesNodeData && !ArchipelagoManager.HasItem(APItem.BackpackNode))
			    || (triggeringNodeData is BuildTotemNodeData && !ArchipelagoManager.HasItem(APItem.WoodcarverNode))
			    || (triggeringNodeData is TradePeltsNodeData && !ArchipelagoManager.HasItem(APItem.TraderNode))
			    || (triggeringNodeData is CopyCardNodeData && !ArchipelagoManager.HasItem(APItem.GoobertNode))) {
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
			if (ArchipelagoOptions.act1RandomizeNodes)
            {
                __result = true;
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

