using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Helpers;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using GBC;
using HarmonyLib;
using Pixelplacement;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Archipelago_Inscryption.Patches
{
    [HarmonyPatch]
    internal class CheckPatches
    {
        [HarmonyPatch(typeof(StoryEventsData), "SetEventCompleted")]
        [HarmonyPrefix]
        static bool SendCheckOnStoryEvent(StoryEvent storyEvent)
        {
            if (storyEvent == StoryEvent.NUM_EVENTS) return false;

            ArchipelagoManager.SendStoryCheckIfApplicable(storyEvent);

            if (storyEvent == StoryEvent.StartScreenNewGameUnlocked && !ArchipelagoData.Data.act1Completed)
            {
                ArchipelagoUI.Instance.LogImportant("Act 1 completed!");
                ArchipelagoData.Data.act1Completed = true;
            }
            else if (storyEvent == StoryEvent.Part2Completed && !ArchipelagoData.Data.act2Completed)
            {
                ArchipelagoUI.Instance.LogImportant("Act 2 completed!");
                ArchipelagoData.Data.act2Completed = true;
            }
            else if (storyEvent == StoryEvent.Part3Completed && !ArchipelagoData.Data.act3Completed)
            {
                ArchipelagoUI.Instance.LogImportant("Act 3 completed!");
                ArchipelagoData.Data.act3Completed = true;
            }
            else if (storyEvent == StoryEvent.FigurineFetched && !ProgressionData.LearnedMechanic(MechanicsConcept.FirstPersonNavigation))
            {
                // backup in case you missed these during the first tutorial run
                ProgressionData.SetMechanicLearned(MechanicsConcept.FirstPersonNavigation);
                ProgressionData.SetMechanicLearned(MechanicsConcept.LosingLife);
            }

            ArchipelagoManager.VerifyGoalCompletion();

            return true;
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "AddChosenCardToDeck")]
        [HarmonyPrefix]
        static bool DontAddIfCheckCard(CardSingleChoicesSequencer __instance)
        {
            if (__instance.chosenReward.Info.name.Contains("Archipelago"))
            {
                if (__instance.chosenReward.Info.name.Contains("ArchipelagoCheck"))
                {
                    string cardName = __instance.chosenReward.Info.name;
                    string checkName = cardName.Substring(cardName.IndexOf('_') + 1);
                    APCheck check = Enum.GetValues(typeof(APCheck)).Cast<APCheck>().FirstOrDefault(c => c.ToString() == checkName);
                    ArchipelagoManager.SendCheck(check);
                }

                __instance.deckPile.AddToPile(__instance.chosenReward.transform);

                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(SaveFile), "CollectGBCCard")]
        [HarmonyPrefix]
        static bool SendCheckInsteadOfAddingCard(CardInfo card)
        {
            if (card.name.Contains("Archipelago"))
            {
                string checkName = card.name.Substring(card.name.IndexOf('_') + 1);
                APCheck check = Enum.GetValues(typeof(APCheck)).Cast<APCheck>().FirstOrDefault(c => c.ToString() == checkName);
                ArchipelagoManager.SendCheck(check);
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(SafeInteractable), "Start")]
        [HarmonyPrefix]
        static bool ReplaceStinkbugCardWithCheck(SafeInteractable __instance)
        {
            GameObject stinkbugCard = __instance.regularContents.GetComponentInChildren<DiscoverableTalkingCardInteractable>(true).gameObject;
            DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(stinkbugCard, APCheck.CabinSafe, true, StoryEvent.SafeOpened);

            MainInputInteractable key = __instance.interiorObjects[1];

            if (!ArchipelagoManager.HasItem(APItem.WardrobeKey))
            {
                key.transform.parent.gameObject.SetActive(false);
                __instance.gameObject.AddComponent<ActivateOnItemReceived>().Init(key.transform.parent.gameObject, APItem.WardrobeKey);
            }
            else
            {
                key.transform.parent.gameObject.SetActive(true);
            }

            __instance.interiorObjects.Clear();
            __instance.interiorObjects.Add(key);

            if (checkCard)
            {
                __instance.interiorObjects.Add(checkCard);
                checkCard.SetEnabled(false);
            }

            if (ArchipelagoOptions.randomizeCodes)
            {
                __instance.correctLockPositions = ArchipelagoData.Data.cabinSafeCode.Select(digit => (10 - digit) % 10).ToList();
            }

            return true;
        }

        [HarmonyPatch(typeof(WardrobeDrawerInteractable), "Start")]
        [HarmonyPrefix]
        static bool ReplaceWardrobeCardWithCheck(WardrobeDrawerInteractable __instance)
        {
            APCheck check;
            StoryEvent storyEvent;

            if (__instance.name.Contains("1"))
            {
                check = APCheck.CabinDrawer1;
                storyEvent = StoryEvent.WardrobeDrawer1Opened;
            }
            else if (__instance.name.Contains("2"))
            {
                check = APCheck.CabinDrawer2;
                storyEvent = StoryEvent.WardrobeDrawer2Opened;
            }
            else if (__instance.name.Contains("3"))
            {
                check = APCheck.CabinDrawer3;
                storyEvent = StoryEvent.WardrobeDrawer3Opened;
            }
            else if (__instance.name.Contains("4"))
            {
                check = APCheck.CabinDrawer4;
                storyEvent = StoryEvent.WardrobeDrawer4Opened;

                Transform squirrelHead = __instance.drawerContents[0].transform;
                squirrelHead.eulerAngles = new Vector3(90, 114, 0);
                squirrelHead.localScale = Vector3.one * 0.7114f;
            }
            else if (__instance.drawerContents[0].name.Contains("Card"))
            {
                check = APCheck.FactoryDrawer2;
                storyEvent = StoryEvent.FactoryWardrobe2Opened;
            }
            else
            {
                check = APCheck.FactoryDrawer1;
                storyEvent = StoryEvent.FactoryWardrobe1Opened;
            }

            DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(__instance.drawerContents[0].gameObject, check, true, storyEvent);
            __instance.drawerContents.Clear();

            if (checkCard)
                __instance.drawerContents.Add(checkCard);

            return true; 
        }

        [HarmonyPatch(typeof(CuckooClock), "Start")]
        [HarmonyPrefix]
        static bool ReplaceClockContentsWithChecks(CuckooClock __instance)
        {
            if (SaveManager.SaveFile.IsPart3)
            {
                GameObject ourobotCard = __instance.largeCompartmentContents[0].gameObject;
                __instance.largeCompartmentContents.Clear();

                DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(ourobotCard, APCheck.FactoryClock, true, StoryEvent.FactoryCuckooClockOpenedLarge);

                int fplLayer = LayerMask.NameToLayer("FirstPersonLighting");

                if (checkCard)
                {
                    __instance.largeCompartmentContents.Add(checkCard);
                    checkCard.gameObject.SetLayerRecursive(fplLayer);
                    if (!StoryEventsData.EventCompleted(StoryEvent.FactoryCuckooClockOpenedLarge))
                        checkCard.SetEnabled(false);
                }

                if (ArchipelagoOptions.randomizeCodes)
                {
                    __instance.solutionPositionsLarge = ArchipelagoData.Data.factoryClockCode.ToArray();
                    __instance.solutionPositionsSmall = ArchipelagoData.Data.cabinSmallClockCode.ToArray();
                }
            }
            else
            {
                GameObject stuntedWolfCard = __instance.largeCompartmentContents[0].gameObject;
                GameObject ring = __instance.smallCompartmentContents[0].gameObject;
                ring.transform.eulerAngles = new Vector3(0, 180, 0);
                ring.transform.localScale = Vector3.one * 0.7114f;

                DiscoverableCheckInteractable checkCard1 = RandomizerHelper.CreateDiscoverableCardCheck(stuntedWolfCard, APCheck.CabinClock1, true, StoryEvent.ClockCompartmentOpened);
                DiscoverableCheckInteractable checkCard2 = RandomizerHelper.CreateDiscoverableCardCheck(ring, APCheck.CabinClock2, true, StoryEvent.ClockSmallCompartmentOpened);
                GameObject.Destroy(__instance.largeCompartmentContents[1].gameObject);
                __instance.largeCompartmentContents.Clear();
                __instance.smallCompartmentContents.Clear();

                int fplLayer = LayerMask.NameToLayer("FirstPersonLighting");

                if (checkCard1)
                {
                    __instance.largeCompartmentContents.Add(checkCard1);
                    checkCard1.gameObject.SetLayerRecursive(fplLayer);
                    if (!StoryEventsData.EventCompleted(StoryEvent.ClockCompartmentOpened))
                        checkCard1.SetEnabled(false);
                }

                if (checkCard2)
                {
                    checkCard2.closeUpEulers = Vector3.zero;
                    checkCard2.closeUpDistance = 2.2f;
                    checkCard2.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.4f);

                    __instance.smallCompartmentContents.Add(checkCard2);
                    checkCard2.gameObject.SetLayerRecursive(fplLayer);
                    if (!StoryEventsData.EventCompleted(StoryEvent.ClockCompartmentOpened))
                        checkCard2.SetEnabled(false);
                }

                if (ArchipelagoOptions.randomizeCodes)
                {
                    __instance.solutionPositionsLarge = ArchipelagoData.Data.cabinClockCode.ToArray();
                    __instance.solutionPositionsSmall = ArchipelagoData.Data.cabinSmallClockCode.ToArray();

                    Transform clock = __instance.transform.Find("CuckooClock");

                    List<Transform> children = new List<Transform>();
                    foreach (Transform child in clock)
                    {
                        children.Add(child);
                    }

                    children.First(x => x.gameObject.name == "WizardMark_Tall" && x.eulerAngles.z > 270).gameObject.SetActive(false);
                    children.First(x => x.gameObject.name == "WizardMark_Tall" && x.eulerAngles.z < 270).gameObject.SetActive(false);
                    children.First(x => x.gameObject.name == "WizardMark_Short").gameObject.SetActive(false);

                    GameObject cluesObject = GameObject.Instantiate(AssetsManager.clockCluesPrefab, clock);

                    cluesObject.transform.localPosition = new Vector3(0f, 1.9459f, 0.6f);
                    cluesObject.transform.eulerAngles = new Vector3(0, 180, 0);

                    Transform secondHandClue = cluesObject.transform.Find("SecondCluePivot");
                    Transform minuteHandClue = cluesObject.transform.Find("MinuteCluePivot");
                    Transform hourHandClue = cluesObject.transform.Find("HourCluePivot");

                    secondHandClue.localEulerAngles = new Vector3(0, 0, 360 - 30 * ArchipelagoData.Data.cabinClockCode[0]);
                    minuteHandClue.localEulerAngles = new Vector3(0, 0, 360 - 30 * ArchipelagoData.Data.cabinClockCode[1]);
                    hourHandClue.localEulerAngles = new Vector3(0, 0, 360 - 30 * ArchipelagoData.Data.cabinClockCode[2]);

                    cluesObject.SetLayerRecursive(LayerMask.NameToLayer("WizardEyeVisible"));
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(WolfStatueSlotInteractable), "Start")]
        [HarmonyPrefix]
        static bool ReplaceDaggerWithCheck(WolfStatueSlotInteractable __instance)
        {
            DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(__instance.dagger.gameObject, APCheck.CabinDagger, true);

            if (!checkCard) return true;

            checkCard.requireStoryEventToAddToDeck = true;
            checkCard.requiredStoryEvent = StoryEvent.WolfStatuePlaced;
            checkCard.closeUpDistance = 2.2f;
            checkCard.closeUpEulers = Vector3.zero;
            checkCard.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.4f);

            checkCard.transform.position = new Vector3(28.1565f, 8.5275f, 11.2946f);
            checkCard.transform.eulerAngles = new Vector3(46.6807f, 140f, 0f);
            checkCard.transform.localScale = Vector3.one * 0.7114f;

            return true;
        }

        [HarmonyPatch(typeof(WolfStatueSlotInteractable), "UnlockDagger")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> IgnoreDagger(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(Animator), "Play", new System.Type[]{ typeof(string), typeof(int), typeof(float)})));

            index++;

            codes.RemoveRange(index, 10);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(OilPaintingPuzzle), "OnRewardTaken")]
        [HarmonyPrefix]
        static bool TakeCardInstead(OilPaintingPuzzle __instance)
        {
            __instance.state.rewardTaken = true;
            RandomizerHelper.ClaimPaintingCheck(__instance.state.rewardIndex);
            __instance.DisplaySaveState(__instance.state);

            return false;
        }

        [HarmonyPatch(typeof(OilPaintingPuzzle.SaveState), "get_RewardRedeemed")]
        [HarmonyPrefix]
        static bool SkipStoryEventVerification(OilPaintingPuzzle.SaveState __instance, ref bool __result)
        {
            __result = __instance.rewardTaken;

            return false;
        }

        [HarmonyPatch(typeof(OilPaintingPuzzle), "Start")]
        [HarmonyPrefix]
        static bool ReplacePaintingRewardsWithChecks(OilPaintingPuzzle __instance)
        {
            GameObject reference = new GameObject();
            reference.transform.position = new Vector3(19.22f, 9.5f, -15.9f);
            reference.transform.eulerAngles = new Vector3(0, 180, 0);
            reference.transform.localScale = Vector3.one * 0.7114f;
            reference.AddComponent<BoxCollider>().size = new Vector3(0f, 0f, 0f);

            Transform rewardDisplayParent = __instance.rewardDisplayedItems[0].transform.parent;

            foreach (GameObject go in __instance.rewardDisplayedItems)
            {
                GameObject.Destroy(go);
            }

            __instance.rewardDisplayedItems.Clear();

            DiscoverableCheckInteractable checkCard1 = RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.CabinPainting1, false);
            DiscoverableCheckInteractable checkCard2 = RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.CabinPainting2, false);
            DiscoverableCheckInteractable checkCard3 = RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.CabinPainting3, true);

            int offscreenLayer = LayerMask.NameToLayer("CardOffscreen");

            if (checkCard1)
            {
                checkCard1.card.RenderCard();
                GameObject.Destroy(checkCard1.card.GetComponent<BoxCollider>());
                __instance.rewardDisplayedItems.Add(GameObject.Instantiate(checkCard1.card.gameObject, rewardDisplayParent));
                checkCard1.SetEnabled(false);
            }
            else
            {
                __instance.rewardDisplayedItems.Add(new GameObject());
            }

            if (checkCard2)
            {
                checkCard2.card.RenderCard();
                GameObject.Destroy(checkCard2.card.GetComponent<BoxCollider>());
                __instance.rewardDisplayedItems.Add(GameObject.Instantiate(checkCard2.card.gameObject, rewardDisplayParent));
                checkCard2.SetEnabled(false);
            }
            else
            {
                __instance.rewardDisplayedItems.Add(new GameObject());
            }

            if (checkCard3)
            {
                checkCard3.card.RenderCard();
                GameObject.Destroy(checkCard3.card.GetComponent<BoxCollider>());
                __instance.rewardDisplayedItems.Add(GameObject.Instantiate(checkCard3.card.gameObject, rewardDisplayParent));
                checkCard3.SetEnabled(false);
            }
            else
            {
                __instance.rewardDisplayedItems.Add(new GameObject());
            }

            foreach (GameObject go in __instance.rewardDisplayedItems)
            {
                go.transform.localPosition = new Vector3(0f, 0.8618f, 0.9273f);
                go.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                go.transform.localScale = Vector3.one * 0.7114f;
                go.SetLayerRecursive(offscreenLayer);
            }

            RandomizerHelper.SetPaintingRewards(checkCard1 , checkCard2 , checkCard3);

            return true;
        }

        [HarmonyPatch(typeof(WallCandlesPuzzle), "Start")]
        [HarmonyPrefix]
        static bool ReplaceGreaterSmokeWithCheck(WallCandlesPuzzle __instance)
        {
            GameObject reference = new GameObject();
            reference.transform.position = new Vector3(13.4709f, 11.3545f, 19.8158f);
            reference.transform.eulerAngles = Vector3.zero;
            reference.transform.localScale = Vector3.one * 0.7114f;
            reference.AddComponent<BoxCollider>().size = new Vector3(0f, 0f, 0f);

            __instance.card.gameObject.SetActive(false);

            DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.CabinSmoke, true);

            if (!checkCard) return true;

            checkCard.SetEnabled(false);
            __instance.card = checkCard;
            GameObject.Destroy(checkCard.card.GetComponent<BoxCollider>());

            return true;
        }

        [HarmonyPatch(typeof(WallCandlesPuzzle), "OnCandleClicked")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UnlockSmokeCheckInstead(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(WallCandlesPuzzle), "UnlockCardSequence")));

            index--;

            codes.RemoveRange(index, 4);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WallCandlesPuzzle), "card")),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(DiscoverableObjectInteractable), "Discover"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(ContainerVolume), "Start")]
        [HarmonyPostfix]
        static void ReplaceContainterContentWithCheck(ContainerVolume __instance)
        {
            if (__instance.transform.GetPath() == "Temple/BasementRoom/Casket/ContainerVolume") return;

            if (__instance.pickupEvent.GetPersistentEventCount() > 0)
            {
                __instance.pickupEvent = new EventTrigger.TriggerEvent();
                __instance.pickupEvent.AddListener(data => RandomizerHelper.GiveObjectRelatedCheck(__instance.gameObject));
            }
            else if (__instance.postTextEvent.GetPersistentEventCount() > 0)
            {
                __instance.postTextEvent = new EventTrigger.TriggerEvent();
                __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveObjectRelatedCheck(__instance.gameObject));
            }
            else
            {
                return;
            }

            if (!__instance.GetComponent<GainEpitaphPiece>())
            {
                __instance.textLines.Clear();
                __instance.textLines.Add("You found a strange card inside.");
            }
        }

        [HarmonyPatch(typeof(GainEpitaphPiece), "GetTextBoxPickupLine")]
        [HarmonyPrefix]
        static bool ReplaceEpitaphText(ref string __result)
        {
            __result = "...Upon closer inspection, it's actually a strange looking card.";

            return false;
        }

        [HarmonyPatch(typeof(GainEpitaphPiece), "Start")]
        [HarmonyPrefix]
        static bool AddEpitaphCheck(GainEpitaphPiece __instance)
        {
            PickupObjectVolume pickup = __instance.GetComponent<PickupObjectVolume>();

            if (pickup != null)
            {
                pickup.pickupEvent = new EventTrigger.TriggerEvent();
                pickup.postTextEvent.AddListener(data => RandomizerHelper.GiveObjectRelatedCheck(__instance.gameObject));
            }

            return true;
        }

        [HarmonyPatch(typeof(WellVolume), "OnPostMessage")]
        [HarmonyPrefix]
        static bool ReplaceWellItemsWithChecks(WellVolume __instance)
        {
            if (__instance.saveState.State.intVal == 0)
            {
                RandomizerHelper.GiveGBCCheck(APCheck.GBCEpitaphPiece9);
            }
            else if (__instance.saveState.State.intVal == 1)
            {
                RandomizerHelper.GiveGBCCheck(APCheck.GBCCryptWell);
            }

            __instance.saveState.State.intVal++;

            return false;
        }

        [HarmonyPatch(typeof(PickupObjectVolume), "Start")]
        [HarmonyPrefix]
        static bool ReplacePickupWithCheck(PickupObjectVolume __instance)
        {
            if (__instance.unlockStoryEvent)
            {
                if (__instance.storyEventToUnlock == StoryEvent.GBCCloverFound)
                {
                    __instance.unlockStoryEvent = false;
                    __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCClover));
                    __instance.textLines.Clear();
                    __instance.textLines.Add("You picked the clover leaf from the stem...");
                    __instance.textLines.Add("...but it suddenly turned itself into a strange card.");
                }
                else if (__instance.storyEventToUnlock == StoryEvent.GBCBoneFound)
                {
                    __instance.unlockStoryEvent = false;
                    __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCBoneLordFemur));
                    __instance.textLines.Clear();
                    __instance.textLines.Add("You took the Bone Lord's femur from the pedestal...");
                    __instance.textLines.Add("...but it suddenly turned itself into a strange card.");
                }
                else if (__instance.storyEventToUnlock == StoryEvent.BonelordHoloKeyFound)
                {
                    __instance.unlockStoryEvent = false;
                    __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCBoneLordHoloKey));
                    __instance.textLines.Clear();
                    __instance.textLines.Add("You found a strange flickering key...");
                    __instance.textLines.Add("...but as you touched it, the key turned itself into a card.");
                }
                else if (__instance.storyEventToUnlock == StoryEvent.MycologistHutKeyFound)
                {
                    __instance.unlockStoryEvent = false;
                    __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCMycologistsHoloKey));
                    __instance.textLines.Clear();
                    __instance.textLines.Add("You found a strange flickering key...");
                    __instance.textLines.Add("...but as you touched it, the key turned itself into a card.");
                }
            }
            else if (SceneLoader.ActiveSceneName == "GBC_Temple_Tech" && __instance.gameObject.name == "RecyclingBinVolume")
            {
                __instance.pickupEvent = new EventTrigger.TriggerEvent();
                __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCFactoryTrashCan));
                __instance.textLines.Clear();
                __instance.textLines.Add("You rummage through the junk cards... And find a strange card that didn't seem to belong with the others.");
            }
            else if (SceneLoader.ActiveSceneName == "GBC_Temple_Undead" && __instance.gameObject.name == "Card")
            {
                __instance.pickupEvent = new EventTrigger.TriggerEvent();
                __instance.postTextEvent.AddListener(data => RandomizerHelper.GiveGBCCheck(APCheck.GBCBoneLordHorn));
            }

            return true;
        }

        [HarmonyPatch(typeof(BrokenCoin), "RespondsToOtherCardAssignedToSlot")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AllowObolRepairIfCheckAvailable(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(StoryEventsData), "EventCompleted")));

            index--;

            codes.RemoveRange(index, 2);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCAncientObol),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ArchipelagoManager), "HasCompletedCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(InspectorNPC), "Suicide")]
        [HarmonyPostfix]
        static void GiveInspectorCheck()
        {
            RandomizerHelper.GiveGBCCheck(APCheck.GBCBattleInspector);
        }

        [HarmonyPatch(typeof(SmelterNPC), "Suicide")]
        [HarmonyPostfix]
        static void GiveMelterCheck()
        {
            RandomizerHelper.GiveGBCCheck(APCheck.GBCBattleMelter);
        }

        [HarmonyPatch(typeof(GainMonocleVolume), "Start")]
        [HarmonyPostfix]
        static void ChangeMonocleMessage(GainMonocleVolume __instance)
        {
            __instance.textLines.Add("Your vision was suddenly obstructed. You take off the monocle...");
            __instance.textLines.Add("...or so you thought. It was a strange card instead.");
        }

        [HarmonyPatch(typeof(GainMonocleVolume), "OnPostMessage")]
        [HarmonyPrefix]
        static bool GiveMonocleCheck(GainMonocleVolume __instance)
        {
            __instance.SaveState.boolVal = true;
            __instance.Hide();
            RandomizerHelper.GiveGBCCheck(APCheck.GBCMonocle);

            return false;
        }

        [HarmonyPatch(typeof(CubeChestInteractable), "Start")]
        [HarmonyPostfix]
        static void ReplaceAnglerCardWithCheck(CubeChestInteractable __instance)
        {
            GameObject card = __instance.GetComponentInChildren<DiscoverableTalkingCardInteractable>(true).gameObject;
            RandomizerHelper.CreateDiscoverableCardCheck(card, APCheck.FactoryChest, true);
        }

        [HarmonyPatch(typeof(HoloMapArea), "Start")]
        [HarmonyPrefix]
        static void ReplaceMapNodesWithChecks(HoloMapArea __instance)
        {
            switch (__instance.name)
            {
                case "HoloMapArea_StartingIslandBattery(Clone)":
                    RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/UnlockItemNode3D_Battery").gameObject, APCheck.FactoryExtraBattery);
                    break;
                case "HoloMapArea_Shop(Clone)":
                    RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ShopNode3D_ShieldGenItem/UnlockItemNode3D_ShieldGenerator").gameObject, APCheck.FactoryNanoArmorGenerator);
                    RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ShopNode3D_PickupPelt/PickupPeltNode3D").gameObject, APCheck.FactoryHoloPelt1);
                    break;
                case "HoloMapArea_TempleWizardSide(Clone)":
                    Transform clue = __instance.transform.Find("Splatter/clue");
                    clue.GetComponent<MeshRenderer>().material.mainTexture = AssetsManager.factoryClockClueTexs[ArchipelagoData.Data.factoryClockCode[2]];
                    break;
                case "HoloMapArea_UndeadMainPath_2(Clone)":
                    if (ArchipelagoOptions.randomizeShortcuts != RandomizeShortcuts.Vanilla)
                        RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/DialogueNode3D").gameObject, APCheck.FactoryFilthyCorpseWorldShortcut);
                    break;
                case "HoloMapArea_NatureMainPath_4(Clone)":
                    if (ArchipelagoOptions.randomizeShortcuts != RandomizeShortcuts.Vanilla)
                        RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/DialogueNode3D").gameObject, APCheck.FactoryFoulBackwaterShortcut);
                    break;
                case "HoloMapArea_WizardMainPath_5(Clone)":
                    if (ArchipelagoOptions.randomizeShortcuts != RandomizeShortcuts.Vanilla)
                        RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/DialogueNode3D").gameObject, APCheck.FactoryGaudyGemLandShortcut);
                    break;
                case "HoloMapArea_TempleNatureBoss(Clone)" or "HoloMapArea_TempleUndeadBoss(Clone)" or 
                     "HoloMapArea_TempleWizardBoss(Clone)" or "HoloMapArea_TempleTech_1(Clone)":
                    if (ArchipelagoOptions.randomizeVesselUpgrades != RandomizeVesselUpgrades.Vanilla) {
                        if (ArchipelagoData.Data.vesselUpgrade1Location == "" || ArchipelagoData.Data.vesselUpgrade1Location == __instance.name)
                        {
                            RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ModifySideDeckNode3D").gameObject, APCheck.FactoryVesselUpgrade1);
                            if (ArchipelagoData.Data.vesselUpgrade1Location == "") ArchipelagoData.Data.vesselUpgrade1Location = __instance.name;
                        }
                        else if (ArchipelagoData.Data.vesselUpgrade2Location == "" || ArchipelagoData.Data.vesselUpgrade2Location == __instance.name)
                        {
                            RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ModifySideDeckNode3D").gameObject, APCheck.FactoryVesselUpgrade2);
                            if (ArchipelagoData.Data.vesselUpgrade2Location == "") ArchipelagoData.Data.vesselUpgrade2Location = __instance.name;
                        }
                        else if (ArchipelagoData.Data.vesselUpgrade3Location == "" || ArchipelagoData.Data.vesselUpgrade3Location == __instance.name)
                        {
                            RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ModifySideDeckNode3D").gameObject, APCheck.FactoryVesselUpgrade3);
                            if (ArchipelagoData.Data.vesselUpgrade3Location == "") ArchipelagoData.Data.vesselUpgrade3Location = __instance.name;
                        }
                    }
                    break;
                case "HoloMapArea_TechEntrance(Clone)":
                    if (ArchipelagoOptions.randomizeVesselUpgrades != RandomizeVesselUpgrades.Vanilla)
                        RandomizerHelper.CreateHoloMapNodeCheck(__instance.transform.Find("Nodes/ModifySideDeckNode3D").gameObject, APCheck.FactoryConduitUpgrade);
                    break;
            }
        }


        [HarmonyPatch(typeof(HoloMapBossNode), "SpewLoot")]
        [HarmonyPrefix]
        static void FixBossSpewVesselUpgrades(HoloMapBossNode __instance)
        {
            if (ArchipelagoOptions.randomizeVesselUpgrades != RandomizeVesselUpgrades.Vanilla)
            {
                __instance.lootNodes.RemoveAll(entry => entry.nodeType == HoloMapNode.NodeDataType.ModifySideDeck);
                HoloMapArea Area = __instance.GetComponentInParent<HoloMapArea>();
                HoloMapNode check = Area.transform.Find("Nodes/CardChoiceNode3D(Clone)").GetComponent<HoloMapNode>();
                if (check)
                    __instance.lootNodes.Add(check);
            }
        }
        

        [HarmonyPatch(typeof(HoloMapDialogueNode), "DialogueSequence")]
        [HarmonyPrefix]
        static bool GiveShortcutDialogueCheck(HoloMapDialogueNode __instance)
        {
            if (ArchipelagoOptions.randomizeShortcuts != RandomizeShortcuts.Vanilla)
            {
                APCheck check;

                if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_UndeadMainPath2(Clone)")
                    check = APCheck.FactoryFilthyCorpseWorldShortcut;
                else if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_NatureMainPath_4(Clone)")
                    check = APCheck.FactoryFoulBackwaterShortcut;
                else if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_WizardMainPath_5(Clone)")
                    check = APCheck.FactoryGaudyGemLandShortcut;
                else
                    return true;

                ArchipelagoManager.SendCheck(check);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(InspectionMachineInteractable), "Start")]
        [HarmonyPrefix]
        static void CreateBatteryCheck(InspectionMachineInteractable __instance)
        {
            GameObject reference = new GameObject();
            reference.transform.position = new Vector3(90.3f, 5f, 5f);
            reference.transform.eulerAngles = new Vector3(0, 90, 0);
            reference.transform.localScale = Vector3.one * 0.7114f;
            reference.AddComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.2f);

            RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.FactoryInspectometerBattery, true);

            HoldableBattery battery = __instance.GetComponentInChildren<HoldableBattery>();

            if (battery)
            {
                if (!ArchipelagoManager.HasItem(APItem.InspectometerBattery))
                {
                    battery.transform.parent.gameObject.SetActive(false);
                    __instance.gameObject.AddComponent<ActivateOnItemReceived>().Init(battery.transform.parent.gameObject, APItem.InspectometerBattery);
                }
                else
                {
                    battery.transform.parent.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch(typeof(HoloMapPeltMinigame), "Start")]
        [HarmonyPrefix]
        static bool ReplacePeltWithCheck(HoloMapPeltMinigame __instance)
        {
            APCheck check = APCheck.FactoryHoloPelt1;

            switch (__instance.GetComponentInParent<HoloMapArea>().gameObject.name)
            {
                case "HoloMapArea_NeutralWest_Secret(Clone)":
                    check = APCheck.FactoryHoloPelt2;
                    break;
                case "HoloMapArea_NatureSecret(Clone)":
                    check = APCheck.FactoryHoloPelt3;
                    break;
                case "HoloMapArea_TempleUndeadShop(Clone)":
                    check = APCheck.FactoryHoloPelt4;
                    break;
                case "HoloMapArea_WizardSecret(Clone)":
                    check = APCheck.FactoryHoloPelt5;
                    break;
            }

            HoloMapNode checknode = RandomizerHelper.CreateHoloMapNodeCheck(__instance.rewardNode.gameObject, check);

            if (checknode)
            {
                __instance.rewardNode = checknode;
                return true;
            }

            if (__instance.trapInteractable.Completed)
            {
                __instance.rabbitAnim.gameObject.SetActive(false);
                __instance.trapAnim.Play("shut", 0, 1f);
            }

            return false;
        }

        [HarmonyPatch(typeof(HoloMapLukeFile), "OnFolderHitMapKeyframe")]
        [HarmonyPostfix]
        static void GiveLukeFileCheck(HoloMapLukeFile __instance)
        {
            APCheck check;

            if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_NatureSidePath(Clone)")
                check = APCheck.FactoryLukeFileEntry1;
            else if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_NeutralWestSide_2(Clone)")
                check = APCheck.FactoryLukeFileEntry2;
            else if (__instance.transform.parent.parent.gameObject.name == "HoloMapArea_NeutralWest_LukeFile(Clone)")
                check = APCheck.FactoryLukeFileEntry3;
            else
                check = APCheck.FactoryLukeFileEntry4;

            ArchipelagoManager.SendCheck(check);
        }

        [HarmonyPatch(typeof(HoloMapWell), "Start")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ChangeDredgedCondition(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(Part3SaveData), "foundUndeadTempleQuill")));

            index--;

            codes.RemoveRange(index, 2);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.FactoryWell),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ArchipelagoManager), "HasCompletedCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(HoloMapWell), "Start")]
        [HarmonyPostfix]
        static void ReplaceQuillWithCheck(HoloMapWell __instance)
        {
            HoloMapNode checkNode = RandomizerHelper.CreateHoloMapNodeCheck(__instance.itemNodes[0].gameObject, APCheck.FactoryWell);

            if (checkNode)
            {
                __instance.itemNodes[0] = checkNode;
            }
        }
        [HarmonyPatch(typeof(FactoryGemsDrone), "Start")]
        [HarmonyPrefix]
        static bool DontRemoveGemsDrone(FactoryGemsDrone __instance)
        {
            if (!ArchipelagoManager.HasCompletedCheck(APCheck.FactoryGemsDrone))
            {
                ShelfInteractable shelfInteractable = __instance.shelf;
                shelfInteractable.HoldableTaken = (Action)Delegate.Combine(shelfInteractable.HoldableTaken, new Action(__instance.OnGemsTaken));
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FactoryGemsDrone), "Start")]
        [HarmonyPostfix]
        static void CreateGemsDroneCheck(FactoryGemsDrone __instance)
        {
            GameObject reference = new GameObject();
            reference.transform.SetParent(__instance.transform.Find("Anim"));
            reference.transform.localPosition = new Vector3(0.0109f, 0.2764f, 1.6309f);
            reference.transform.localEulerAngles = new Vector3(90, 0, 0);
            reference.transform.localScale = Vector3.one * 0.7114f;
            reference.AddComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.2f);

            RandomizerHelper.CreateDiscoverableCardCheck(reference, APCheck.FactoryGemsDrone, true);

            __instance.shelf.gameObject.SetActive(false);
            __instance.gameObject.AddComponent<ActivateOnItemReceived>().Init(__instance.shelf.gameObject, APItem.GemsModule);
        }

        [HarmonyPatch(typeof(FirstPersonItemHolder), "PickUpHoldable")]
        [HarmonyPrefix]
        static bool SendDroneCheckIfNotCompleted(HoldableInteractable holdable)
        {
            if (holdable is HoldableGemsModule && !ArchipelagoManager.HasCompletedCheck(APCheck.FactoryGemsDrone))
            {
                DiscoverableCheckInteractable checkCard = GameObject.FindObjectsOfType<DiscoverableCheckInteractable>().FirstOrDefault(go => go.name.Contains("FactoryGemsDrone"));

                if (checkCard)
                {
                    checkCard.Discover();
                }
                else
                {
                    ArchipelagoManager.SendCheck(APCheck.FactoryGemsDrone);
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(FactoryGemsDrone), "OnGemsTaken")]
        [HarmonyPrefix]
        static bool PreventDroneFlyOff(FactoryGemsDrone __instance)
        {
            __instance.shelf.SetEnabled(false);
            return false;
        }

        [HarmonyPatch(typeof(FetchGemsModuleAreaSequencer), "PreEnteredSequence")]
        [HarmonyPostfix]
        static IEnumerator NoStopIconsIfHasGemsModule(IEnumerator __result, FetchGemsModuleAreaSequencer __instance)
        {
            while (__result.MoveNext())
                yield return __result.Current;
            if (ArchipelagoManager.HasItem(APItem.GemsModule))
                __instance.SetStopIconsShown(false);
        }

        [HarmonyPatch(typeof(HoloMapSatelliteDish), "OnInteracted")]
        [HarmonyPrefix]
        static bool ReplaceWizardSatelliteWithCheck(HoloMapSatelliteDish __instance)
        {
            if (__instance.wizardAreaSatellite)
            {
                ArchipelagoManager.SendCheck(APCheck.FactoryWizardTowerSatelliteDish);
			    SaveManager.SaveToFile(true);
			    __instance.node.gameObject.SetActive(false);
			    __instance.StartCoroutine(__instance.ActivateSequence());
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(BonelordNPC), "SetRewardsGiven")]
        [HarmonyPostfix]
        static void MoveFemurPedestal(BonelordNPC __instance)
        {
            __instance.bonelordRewardsParent.transform.Find("Pedestal/Femur").parent.position = new Vector3(9.22f, 15.07f, 0f);
        }

        [HarmonyPatch(typeof(LeshyDialogueNPC), "ManagedLateUpdate")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PreventLeshyBattleIfCameraCheckNotCompleted(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(LeshyDialogueNPC), "npcVolumes")));

            index++;

            codes.RemoveRange(index, 10);

            codes.Insert(index, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "IsLeshyNotReadyForBattle")));

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(GhoulNPC), "SetPuzzleSolved")]
        [HarmonyPrefix]
        static bool KeepEnabledIfGhoulCheckNotDone(GhoulNPC __instance)
        {
            if (__instance.gameObject.name.Contains("Sawyer"))
                return ArchipelagoManager.HasCompletedCheck(APCheck.GBCBattleSawyer);
            if (__instance.gameObject.name.Contains("Royal"))
                return ArchipelagoManager.HasCompletedCheck(APCheck.GBCBattleRoyal);
            if (__instance.gameObject.name.Contains("Briar"))
                return ArchipelagoManager.HasCompletedCheck(APCheck.GBCBattleKaycee);

            return true;
        }

        [HarmonyPatch(typeof(Opponent), "OutroSequence")]
        [HarmonyPatch(typeof(TotemOpponent), "OutroSequence")]
        [HarmonyPostfix]
        static IEnumerator GrantAct1BattleChecks(IEnumerator __result, Opponent __instance, bool wasDefeated)
		{
            if ((ArchipelagoOptions.randomizeNodes || ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable) 
                && __instance is not Part1BossOpponent && (__instance is Part1Opponent || __instance is TotemOpponent)) {
                if (wasDefeated)
                {
                    ArchipelagoManager.SendCheck(APCheck.CabinWoodlandsBattle1 + ArchipelagoData.Data.act1BattlesThisRun);
                }
                ArchipelagoData.Data.act1BattlesThisRun++;
            }
            while (__result.MoveNext())
                yield return __result.Current;
		}

        [HarmonyPatch(typeof(RunState), "Initialize")]
        [HarmonyPostfix]
        static void ResetAct1Battles(RunState __instance)
        {
            ArchipelagoData.Data.act1BattlesThisRun = 0;
        }

        [HarmonyPatch(typeof(TradePeltsSequencer), "GetTradeCardInfos")]
        [HarmonyPostfix]
        static void TraderChecks(ref List<CardInfo> __result, int tier)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable)
            {
                if (tier == 0 && !ArchipelagoManager.HasCompletedCheck(APCheck.CabinTraderRabbitPelt))
                {
                    __result.RemoveAt(7);
                    __result.Add(RandomizerHelper.GenerateCardInfo(APCheck.CabinTraderRabbitPelt));
                }
                else if (tier == 1 && !ArchipelagoManager.HasCompletedCheck(APCheck.CabinTraderWolfPelt))
                {
                    __result.RemoveAt(7);
                    __result.Add(RandomizerHelper.GenerateCardInfo(APCheck.CabinTraderWolfPelt));
                }
                else if (tier == 2 && !ArchipelagoManager.HasCompletedCheck(APCheck.CabinTraderGoldenPelt))
                {
                    __result.RemoveAt(3);
                    __result.Add(RandomizerHelper.GenerateCardInfo(APCheck.CabinTraderGoldenPelt));
                }
            }
        }

        [HarmonyPatch(typeof(TradePeltsSequencer), "OnCardSelected")]
        [HarmonyPrefix]
        static bool DontAddTraderCardIfCheckCard(TradePeltsSequencer __instance, SelectableCard card)
        {
            if (card.Info.name.Contains("ArchipelagoCheck"))
            {
			    card.SetEnabled(false);
			    __instance.tradeCards.Remove(card);
                string cardName = card.Info.name;
                string checkName = cardName.Substring(cardName.IndexOf('_') + 1);
                APCheck check = Enum.GetValues(typeof(APCheck)).Cast<APCheck>().FirstOrDefault(c => c.ToString() == checkName);
                ArchipelagoManager.SendCheck(check);
				__instance.deckPile.MoveCardToPile(card, true, 0f, 0.7f);
                __instance.deckPile.AddToPile(card.transform);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FreeTeethSkull), "SpawnTeethIfNotAscension")]
        [HarmonyPrefix]
        static bool NoFreeTeethIfRandomizeChallenge(FreeTeethSkull __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(GainConsumablesSequencer), "GenerateItemChoices")]
        [HarmonyPostfix]
        static void GuaranteeConsumableChecks(List<ConsumableItemData> __result, GainConsumablesSequencer __instance)
        {
            int area = RunState.Run.regionTier;
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable && area < 3 && ArchipelagoOptions.randomizeNodes)
            {
                List<ConsumableItemSlot> consumableslots = ItemsManager.Instance.consumableSlots.FindAll(x => x.Consumable is CardBottleItem);
                List<APCheck> checksInBottles = [];
                foreach (ConsumableItemSlot slot in consumableslots)
                {
                    CardBottleItem bottle = slot.Consumable as CardBottleItem;
                    string checkName = bottle.cardInfo.name.Substring(bottle.cardInfo.name.IndexOf('_') + 1);
                    APCheck check = Enum.GetValues(typeof(APCheck)).Cast<APCheck>().FirstOrDefault(c => c.ToString() == checkName);
                    checksInBottles.Add(check);
                }
                if (!ArchipelagoManager.HasCompletedCheck(APCheck.CabinWoodlandsConsumableCheck1 + area*3) &&
                    !checksInBottles.Contains(APCheck.CabinWoodlandsConsumableCheck1 + area*3)){
                    __result[0] = ItemsUtil.GetConsumableByName("TerrainBottle");
                }
                if (!ArchipelagoManager.HasCompletedCheck(APCheck.CabinWoodlandsConsumableCheck2 + area*3) &&
                !checksInBottles.Contains(APCheck.CabinWoodlandsConsumableCheck2 + area*3)){
                    __result[1] = ItemsUtil.GetConsumableByName("GoatBottle");
                }
            }
        }

        [HarmonyPatch(typeof(ItemSlot), "CreateItem")]
        [HarmonyPostfix]
        static void ReplaceBottlesWithCheck(ItemSlot __instance)
        {
            if (__instance.Item is CardBottleItem && ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable) 
            {
                APCheck check = 0;
                var bottle = __instance.Item as CardBottleItem;
                int area = RunState.Run.regionTier;
                if (bottle.Data.name.Contains("_"))
                {
                    check = (APCheck)Enum.Parse(typeof(APCheck), bottle.Data.name.Substring(bottle.Data.name.IndexOf("_") + 1));
                }
                else if (area < 3 && Singleton<GameFlowManager>.Instance.CurrentGameState == GameState.SpecialCardSequence) {
                    if (!ArchipelagoManager.HasCompletedCheck(APCheck.CabinWoodlandsConsumableCheck1 + area*3) &&
                        bottle.Data.name.Contains("TerrainBottle")){
                        check = APCheck.CabinWoodlandsConsumableCheck1 + area*3;
                    }
                    if (!ArchipelagoManager.HasCompletedCheck(APCheck.CabinWoodlandsConsumableCheck2 + area*3) &&
                    bottle.Data.name.Contains("GoatBottle") && ArchipelagoOptions.randomizeNodes){
                        check = APCheck.CabinWoodlandsConsumableCheck2 + area*3;
                    }
                    bottle.Data.name = "CheckBottle_" + check.ToString();
                }
                if (check != 0 && !bottle.cardInfo.name.Contains("ArchipelagoCheck"))
                {
                    bottle.cardInfo = RandomizerHelper.GenerateCardInfo(check);
                    var info = UnityEngine.Object.Instantiate(bottle.cardInfo);
                    info.name = bottle.cardInfo.name;
                    bottle.cardInfo = info;
                    var card = bottle.GetComponentInChildren<SelectableCard>();
                    var card2 = UnityEngine.Object.Instantiate(card);
                    card2.name = card.name;
                    card2.SetInfo(info);
                    card2.transform.parent = card.transform.parent;
                    card2.transform.localPosition = card.transform.localPosition;
                    card2.transform.localRotation = card.transform.localRotation;
                    card.gameObject.SetActive(false);
                    card.transform.parent = null;
                    UnityEngine.Object.Destroy(card);
                }
            }
        }

        [HarmonyPatch(typeof(CardBottleItem), "ActivateSequence")]
        [HarmonyPostfix]
        static IEnumerator SendCheckInsteadOfAddingCard(IEnumerator __result, CardBottleItem __instance)
        {
            if (__instance.cardInfo.name.Contains("ArchipelagoCheck"))
            {
                __instance.PlayExitAnimation();
                string cardName = __instance.cardInfo.name;
                string checkName = cardName.Substring(cardName.IndexOf('_') + 1);
                APCheck check = Enum.GetValues(typeof(APCheck)).Cast<APCheck>().FirstOrDefault(c => c.ToString() == checkName);
                ArchipelagoManager.SendCheck(check);
                yield return new WaitForSeconds(0.25f);
            }
            else {
                while (__result.MoveNext())
                    yield return __result.Current;
            }
        }

        [HarmonyPatch(typeof(ActiveAfterAmountOfRuns), "ConditionIsMet")]
        [HarmonyPostfix]
        static void SetTarotCardActive(ActiveAfterAmountOfRuns __instance, ref bool __result)
        {
            if (__instance.gameObject.name != "TarotCardInteractable") return;
            if (ArchipelagoOptions.randomizeChallenges == RandomizeChallenges.Disable) return;
            if (ArchipelagoManager.HasCompletedCheck(APCheck.CabinTarotCardBelowFigurines)) return;
            __result = true;
        }

        [HarmonyPatch(typeof(CabinSecretCardEvent), "Start")]
        [HarmonyPrefix]
        static bool ReplaceCabinTarotCardWithCheck(CabinSecretCardEvent __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable && SaveManager.saveFile.IsPart1) {
                __instance.gameObject.SetActive(true);
                DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(__instance.pickupInteractable.gameObject, APCheck.CabinTarotCardBelowFigurines, true);
                __instance.card.gameObject.SetActive(false);
                if (!checkCard) return true;
                checkCard.transform.eulerAngles = new Vector3(70f, 100f, 0f);
                checkCard.transform.localScale = Vector3.one * 0.7114f;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FreeTeethSkull), "Start")]
        [HarmonyPrefix]
        static void CreateSkullCardCheck(FreeTeethSkull __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable && SaveManager.saveFile.IsPart1)
            {
                DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(__instance.teethObjects[0].gameObject, APCheck.CabinFreeTeethSkull, false);
                if (!checkCard) return;
                checkCard.transform.localEulerAngles = new Vector3(80f, 180f, 356f);
                checkCard.transform.position = new Vector3(7f, 7.6f, 17.4f);
                checkCard.transform.localScale = Vector3.one * 0.7114f;
            }
        }

        [HarmonyPatch(typeof(CabinRulebookInteractable), "Start")]
        [HarmonyPostfix]
        static void CreateCheckByRulebook(CabinRulebookInteractable __instance)
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable && SaveManager.saveFile.IsPart1)
            {
                DiscoverableCheckInteractable checkCard = RandomizerHelper.CreateDiscoverableCardCheck(__instance.gameObject, APCheck.CabinCardByRulebook, false);
                if (!checkCard) return;
                checkCard.transform.localEulerAngles = new Vector3(60f, 270f, 0f);
                checkCard.transform.position = new Vector3(-7.5f, 8.08f, -11.5f);
                checkCard.transform.localScale = Vector3.one * 0.7114f;
            }
        }
    }

    [HarmonyPatch]
    class WizardEyePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(ChooseEyeballSequencer).GetNestedType("<ChooseEyeball>d__5", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceWizardEyeWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(StoryEventsData), "EventCompleted")));

            index--;

            codes.RemoveRange(index, 3);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ChooseEyeballSequencer), "wizardEyeball")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "CreateWizardEyeCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2BattlePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(CardBattleNPC).GetNestedType("<PostCombatEncounterSequence>d__64", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceCardPackRewardWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(GainCardPacks), "OpenPacksSequence")));

            index--;

            codes.RemoveRange(index, 2);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "CombatRewardCheckSequence"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2CardGainMessagePatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(SingleCardGainUI).GetNestedType("<HideEndingSequence>d__10", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceMessageIfCheckCard(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "The card was added to your collection.");

            codes.RemoveAt(index);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SingleCardGainUI), "currentCard")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GetCardGainedMessage"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2GhoulEpitaphCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(GhoulNPC).GetNestedType("<OnDefeatedSequence>d__11", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceEpitaphWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(GainEpitaphPiece), "GainPiece")));

            index -= 2;

            codes.RemoveRange(index, 3);

            index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(Tween), "Shake")));

            index += 2;

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Component), "gameObject")),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GiveObjectRelatedCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2MagnificusCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(MagnificusNPC).GetNestedType("<GlitchOutSequence>d__8", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplacePackWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(GainCardPacks), "OpenPacksSequence")));

            index -= 2;

            codes.RemoveRange(index, 3);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCBossMagnificus),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GiveGBCCheckSequence"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2TentacleCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(TentacleInteractable).GetNestedType("<>c__DisplayClass14_0", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("<GiveCardSequence>b__1", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplacePackWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(GainSingleCards), "TriggerCardsSequence")));

            index -= 3;

            codes.RemoveRange(index, 4);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCTentacle),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GiveGBCCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2SafeCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(SafeVolume).GetNestedType("<GainDogFoodSequence>d__6", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceMeatWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(TextBox), "ShowUntilInput")));

            index -= 11;

            codes.RemoveRange(index, 12);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCCabinSafe),
                new CodeInstruction(OpCodes.Ldstr, "You found a strange card inside."),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GiveGBCCheckWithMessageSequence"))
            };

            codes.InsertRange(index, newCodes);

            index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(StoryEventsData), "SetEventCompleted")));

            index -= 3;

            codes.RemoveRange(index, 4);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2ObolCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(BrokenCoin).GetNestedType("<OnOtherCardAssignedToSlot>d__3", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceObolWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            CodeInstruction messageInstruction = codes.Find(x => x.opcode == OpCodes.Ldstr && (string)x.operand == "You received an Ancient Obol.");
            messageInstruction.operand = $"You received an Ancient Obol...but it turned itself into a strange card.";

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(StoryEventsData), "SetEventCompleted")));

            index -= 3;

            codes.RemoveRange(index, 4);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCAncientObol),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ArchipelagoManager), "SendCheck"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class Act2CameraCheckPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(LeshyDialogueNPC).GetNestedType("<PostDialogueSequence>d__5", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceCameraWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.LoadsField(AccessTools.Field(typeof(NatureTempleSaveData), "hasCamera")));

            index -= 2;

            codes.RemoveRange(index, 3);

            var newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCCameraReplica),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ArchipelagoManager), "HasCompletedCheck"))
            };

            codes.InsertRange(index, newCodes);

            index = codes.FindIndex(x => x.StoresField(AccessTools.Field(typeof(NatureTempleSaveData), "hasCamera")));

            index -= 3;

            codes.RemoveRange(index, 4);

            newCodes = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldc_I4, (int)APCheck.GBCCameraReplica),
                new CodeInstruction(OpCodes.Ldstr, "The camera suddenly turned into a strange card."),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "GiveGBCCheckWithMessage"))
            };

            codes.InsertRange(index, newCodes);

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch]
    class TarotCardsPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(TraderMaskInteractable).GetNestedType("<DialogueSequence>d__11", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceTarotCardsWithCheck(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(TraderMaskInteractable), "ChooseTarotSequence")));

            codes.RemoveAt(index);

            codes.Insert(index, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomizerHelper), "TraderPeltRewardCheckSequence")));

            return codes.AsEnumerable();
        }
    }
}
