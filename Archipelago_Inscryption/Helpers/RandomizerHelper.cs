using Archipelago.MultiClient.Net.Enums;
using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using GBC;
using HarmonyLib;
using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static GBC.DialogueSpeaker;

namespace Archipelago_Inscryption.Helpers
{
    [HarmonyPatch]
    internal static class RandomizerHelper
    {
        private static DiscoverableCheckInteractable[] paintingChecks;

        private static int randomSeed = UnityEngine.Random.Range(1, 500);

        private static readonly string[] checkCardLeshyDialog =
        {
            "This... does not belong here.",
            "What creature could this be?",
            "I don't remember leaving this card there.",
            "How strange...",
            "I believe this belongs to someone.",
            "I don't recognize this...",
            "Perhaps this can be useful to someone.",
            "This is not of this world... What could it be?"
        };

        private static readonly string[] checkCardP03Dialog =
        {
            "Huh? What even is this?",
            "How did that end up there?",
            "Wait, this doesn't belong in Botopia...",
            "I don't remember printing that.",
            "That's not mine...",
            "I'd be embarassed to give that to anybody.",
            "This looks completely useless.",
            "That's weird... Don't let it distract you, though."
        };
        /*
        private static readonly string[] checkCardTraderDialog =
        {
            "A fine reward for your first pelt, don't you think?",
            "Quite mysterious, but surely worth the pelt.",
            "Here's another strange one. Do you know what this is?",
            "I am still unsure where these cards come from.",
            "This is the last one I found."
        };*/

        private static readonly Dictionary<Character, APCheck> npcCheckPairs = new Dictionary<Character, APCheck>()
        {
            { Character.Angler,                             APCheck.GBCBattleAngler },
            { Character.Prospector,                         APCheck.GBCBattleProspector },
            { Character.Trader,                             APCheck.GBCBattleTrapper },
            { Character.Trapper,                            APCheck.GBCBattleTrapper },
            { Character.Leshy,                              APCheck.GBCBossLeshy },
            { Character.GhoulRoyal,                         APCheck.GBCBattleRoyal },
            { Character.GhoulBriar,                         APCheck.GBCBattleKaycee },
            { Character.GhoulSawyer,                        APCheck.GBCBattleSawyer },
            { Character.Grimora,                            APCheck.GBCBossGrimora },
            { Character.GreenWizard,                        APCheck.GBCBattleGoobert },
            { Character.OrangeWizard,                       APCheck.GBCBattlePikeMage },
            { Character.BlueWizard,                         APCheck.GBCBattleLonelyWizard },
            { Character.Magnificus,                         APCheck.GBCBossMagnificus },
            { Character.Inspector,                          APCheck.GBCBattleInspector },
            { Character.Smelter,                            APCheck.GBCBattleMelter },
            { Character.Dredger,                            APCheck.GBCBattleDredger },
            { Character.P03,                                APCheck.GBCBossP03 }
        };

        private static readonly Dictionary<string, APCheck> gbcObjectCheckPair = new Dictionary<string, APCheck>()
        {
            { "GBC_Docks/Room/Objects/Chest/ContainerVolume",                                           APCheck.GBCDockChest },
            { "GBC_Temple_Nature/Temple/OutdoorsCentral/Chest_NaturePack/ContainerVolume",              APCheck.GBCForestChest },
            { "GBC_Temple_Nature/Temple/Meadow/Objects/Chest_NaturePack/ContainerVolume",               APCheck.GBCForestBurrowChest },
            { "GBC_Temple_Nature/Temple/Cabin/Objects/SliderPuzzleContainer",                           APCheck.GBCCabinDrawer },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/Casket_CardPack (1)/ContainerVolume",          APCheck.GBCCryptCasket1 },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/Casket_CardPack/ContainerVolume",              APCheck.GBCCryptCasket2 },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/EpitaphPieceVolume",                           APCheck.GBCEpitaphPiece1 },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/EpitaphPieceVolume (1)",                       APCheck.GBCEpitaphPiece2 },
            { "GBC_Temple_Undead/Temple/MainRoom/OverworldGhoulNPC_Sawyer",                             APCheck.GBCEpitaphPiece3 },
            { "GBC_Temple_Undead/Temple/BasementRoom/EpitaphPieceVolume (2)",                           APCheck.GBCEpitaphPiece4 },
            { "GBC_Temple_Undead/Temple/MainRoom/OverworldGhoulNPC_Royal",                              APCheck.GBCEpitaphPiece5 },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/Casket_Piece/ContainerVolume",                 APCheck.GBCEpitaphPiece6 },
            { "GBC_Temple_Undead/Temple/MirrorRoom/EpitaphPieceVolume",                                 APCheck.GBCEpitaphPiece7 },
            { "GBC_Temple_Undead/Temple/MainRoom/OverworldGhoulNPC_Briar",                              APCheck.GBCEpitaphPiece8 },
            { "GBC_Temple_Undead/Temple/MainRoom/Objects/Well/ContainerVolume",                         APCheck.GBCEpitaphPiece9 },
            { "GBC_Temple_Wizard/Temple/Floor_1/Chest_WizardPack/ContainerVolume",                      APCheck.GBCTowerChest1 },
            { "GBC_Temple_Wizard/Temple/Floor_2/Objects/Chest_WizardPack (1)/ContainerVolume",          APCheck.GBCTowerChest2 },
            { "GBC_Temple_Wizard/Temple/Floor_3/Objects/Chest_Card/ContainerVolume",                    APCheck.GBCTowerChest3 },
            { "GBC_Temple_Tech/Temple/--- MainRoom ---/Objects/TechSliderPuzzleContainer",              APCheck.GBCFactoryDrawer1 },
            { "GBC_Temple_Tech/Temple/--- MainRoom ---/Objects/TechSliderPuzzleContainer (1)",          APCheck.GBCFactoryDrawer2 },
            { "GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Objects/Chest_TechPack/ContainerVolume",     APCheck.GBCFactoryChest1 },
            { "GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Objects/Chest_TechPack (1)/ContainerVolume", APCheck.GBCFactoryChest2 },
            { "GBC_Temple_Tech/Temple/--- DredgingRoom ---/Objects/Chest_TechPack/ContainerVolume",     APCheck.GBCFactoryChest3 },
            { "GBC_Temple_Tech/Temple/--- DredgingRoom ---/Objects/Chest_TechPack (1)/ContainerVolume", APCheck.GBCFactoryChest4 },
        };

        internal static GenericUIButton packButton;

        private static bool doDeathCard = true;

        internal static DiscoverableCheckInteractable CreateDiscoverableCardCheck(GameObject originalObject, APCheck check, bool destroyOriginal, StoryEvent activeStoryFlag = StoryEvent.NUM_EVENTS)
        {
            if (!ArchipelagoManager.HasCompletedCheck(check))
            {
                GameObject objectToFollow;
                SelectableCard originalSelectableCard = originalObject.GetComponentInChildren<SelectableCard>(true);
                if (originalSelectableCard != null) 
                    objectToFollow = originalSelectableCard.gameObject;
                else 
                    objectToFollow = originalObject;

                GameObject newCheckCard = new GameObject("DiscoverableCheck_" + check.ToString());
                newCheckCard.transform.SetParent(originalObject.transform.parent);
                newCheckCard.transform.position = objectToFollow.transform.position;
                newCheckCard.transform.rotation = objectToFollow.transform.rotation;
                newCheckCard.transform.localScale = 
                    originalSelectableCard ? 
                    Vector3.Scale(originalObject.transform.localScale, originalSelectableCard.transform.localScale) 
                    : originalObject.transform.localScale;
                newCheckCard.AddComponent<BoxCollider>().size = originalObject.GetComponent<BoxCollider>().size;

                float closeUpDistance = 2.2f;
                Vector3 closeUpEulers = Vector3.zero;
                float closeUpVerticalOffset = 0f;

                DiscoverableObjectInteractable originalCardInteractable = originalObject.GetComponent<DiscoverableObjectInteractable>();

                if (originalCardInteractable)
                {
                    closeUpDistance = originalCardInteractable.closeUpDistance;
                    closeUpEulers = originalCardInteractable.closeUpEulers;
                    closeUpVerticalOffset = originalCardInteractable.closeUpVerticalOffset;
                }

                CardInfo info = GenerateCardInfo(check);

                DiscoverableCheckInteractable newCardInteractable = newCheckCard.AddComponent<DiscoverableCheckInteractable>();

                newCardInteractable.check = check;
                newCardInteractable.closeUpDistance = closeUpDistance;
                newCardInteractable.closeUpEulers = closeUpEulers;
                newCardInteractable.closeUpVerticalOffset = closeUpVerticalOffset;
                newCardInteractable.onDiscoverText = info.description;
                newCardInteractable.storyEvent = StoryEvent.NUM_EVENTS;
                newCardInteractable.requireStoryEventToAddToDeck = false;
                GameObject newCard = GameObject.Instantiate(SaveManager.SaveFile.IsPart3 ? AssetsManager.selectableDiskCardPrefab : AssetsManager.selectableCardPrefab, newCheckCard.transform);
                newCard.name = "ArchipelagoCheckCard_" + check.ToString();
                newCard.transform.ResetTransform();
                newCardInteractable.card = newCard.GetComponent<SelectableCard>();
                newCardInteractable.card.SetInfo(info);

                if (activeStoryFlag < StoryEvent.NUM_EVENTS)
                {
                    ActiveIfStoryFlag storyFlagCondition = newCardInteractable.gameObject.AddComponent<ActiveIfStoryFlag>();
                    storyFlagCondition.targetObject = newCard;
                    storyFlagCondition.checkConditionEveryFrame = true;
                    storyFlagCondition.activeIfConditionMet = true;
                    storyFlagCondition.storyFlag = activeStoryFlag;
                }

                if (destroyOriginal)
                    GameObject.Destroy(originalObject);

                return newCardInteractable;
            }
            else
            {
                if (destroyOriginal)
                    GameObject.Destroy(originalObject);

                return null;
            }
            
        }

        internal static HoloMapNode CreateHoloMapNodeCheck(GameObject originalNodeObject, APCheck check)
        {
            HoloMapNode originalNode = originalNodeObject.GetComponent<HoloMapNode>();

            HoloMapNode newNode = null;

            if (!ArchipelagoManager.HasCompletedCheck(check))
            {
                GameObject newNodeObject = GameObject.Instantiate(AssetsManager.cardChoiceHoloNodePrefab, originalNodeObject.transform.parent);
                newNodeObject.transform.localPosition = originalNodeObject.transform.localPosition;
                newNodeObject.transform.localRotation = originalNodeObject.transform.localRotation;

                newNode = newNodeObject.GetComponent<HoloMapNode>();
                newNode.nodeId = originalNode.nodeId;
                newNode.fixedChoices = new List<CardInfo>() { GenerateCardInfo(check) };
                newNodeObject.transform.Find("RendererParent/Renderer").GetComponent<MeshFilter>().sharedMesh = AssetsManager.checkCardHoloNodeMesh;
                Renderer rendererToDelete = newNode.nodeRenderers[1];
                newNode.nodeRenderers.RemoveAt(1);
                GameObject.Destroy(rendererToDelete.gameObject);
                newNode.AssignNodeData();

                if (!originalNodeObject.activeSelf)
                    newNodeObject.SetActive(false);

                Singleton<MapNodeManager>.Instance.nodes.Add(newNode);

                HoloMapShopNode shopNode = originalNode.GetComponentInParent<HoloMapShopNode>();

                if (shopNode)
                {
                    shopNode.nodeToBuy = newNode;
                    newNode.defaultColor = new Color(1f, 0.5725f, 0.149f);
                    foreach (Renderer renderer in newNode.nodeRenderers)
                    {
                        renderer.material.SetColor("_MainColor", newNode.defaultColor);
                    }
                    GameObject.Destroy(newNodeObject.GetComponent<BoxCollider>());
                }
            }

            if (Singleton<MapNodeManager>.Instance.nodes.Contains(originalNode))
                Singleton<MapNodeManager>.Instance.nodes.Remove(originalNode);

            GameObject.Destroy(originalNodeObject);

            return newNode;
        }

        internal static void CreateWizardEyeCheck(EyeballInteractable wizardEye)
        {
            GameObject reference = new GameObject();
            reference.transform.SetParent(wizardEye.transform.parent);
            reference.transform.position = wizardEye.transform.position;
            reference.transform.localEulerAngles = new Vector3(90, 0, 0);
            reference.transform.localScale = Vector3.one * 0.7114f;
            reference.AddComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.4f);

            DiscoverableCheckInteractable checkCard = CreateDiscoverableCardCheck(reference, APCheck.CabinMagnificusEye, true);
        }

        internal static void SetPaintingRewards(DiscoverableCheckInteractable card1, DiscoverableCheckInteractable card2, DiscoverableCheckInteractable card3)
        {
            paintingChecks = new DiscoverableCheckInteractable[] { card1, card2, card3 };
        }

        internal static void ClaimPaintingCheck(int rewardIndex)
        {
            paintingChecks[rewardIndex].Discover();
        }

        internal static CardInfo GenerateCardInfo(APCheck check)
        {
            CheckInfo checkInfo = ArchipelagoManager.GetCheckInfo(check);

            CardInfo info = ScriptableObject.CreateInstance<CardInfo>();
            info.name = "ArchipelagoCheck_" + check.ToString();
            info.displayedName = checkInfo.itemName;
            info.hideAttackAndHealth = true;
            info.portraitTex = AssetsManager.cardPortraitSprite;
            info.pixelPortrait = AssetsManager.cardPixelPortraitSprite;
            if (!SaveManager.SaveFile.IsPart3)
            {
                if (checkInfo.category is ItemFlags.None)
                {
                    info.appearanceBehaviour.Add(CardAppearanceBehaviour.Appearance.TerrainBackground);
                }
                else if (checkInfo.category is ItemFlags.Advancement)
                {
                    info.appearanceBehaviour.Add(CardAppearanceBehaviour.Appearance.RareCardBackground);
                    info.metaCategories.Add(CardMetaCategory.Rare);
                }
            }
            string[] discoverTextDialogs = SaveManager.SaveFile.IsPart3 ? checkCardP03Dialog : checkCardLeshyDialog;
            info.description = discoverTextDialogs[UnityEngine.Random.Range(0, discoverTextDialogs.Length)];
            return info;
        }

        // RenameItemInSaveData only queues; ApplyPendingRenames is the sole writer, since
        // UpdateItems calls CreateItem (which triggers renames) from inside its own list iteration.
        private static bool updatingItems = false;
        private static readonly List<(string oldName, string newName)> pendingRenames = new();

        [HarmonyPatch(typeof(ItemsManager), "UpdateItems")]
        [HarmonyPrefix]
        static void MarkUpdateItemsInProgress()
        {
            RegisterCheckBottleData();
            updatingItems = true;
        }

        // A check bottle gets a data object of its own. Branding the shared one instead renames the
        // asset every later roll draws from, so a check already taken kept coming back on offer.
        private static readonly Dictionary<APCheck, ConsumableItemData> checkBottleData = new();

        private static readonly APCheck[] consumableChecks = new APCheck[]
        {
            APCheck.CabinWoodlandsConsumableCheck1, APCheck.CabinWoodlandsConsumableCheck2,
            APCheck.CabinWetlandsConsumableCheck1,  APCheck.CabinWetlandsConsumableCheck2,
            APCheck.CabinSnowLineConsumableCheck1,  APCheck.CabinSnowLineConsumableCheck2
        };

        // Built up front, since a save reloaded while one is held names it in the saved item list and
        // UpdateItems resolves that name before anything has had cause to build the data behind it.
        internal static void RegisterCheckBottleData()
        {
            if (checkBottleData.Count == consumableChecks.Length) return;

            foreach (APCheck check in consumableChecks)
            {
                if (checkBottleData.ContainsKey(check)) continue;

                ConsumableItemData source = FindBottleData(check.ToString().EndsWith("1") ? "TerrainBottle" : "GoatBottle");

                if (source == null) return;

                ConsumableItemData data = UnityEngine.Object.Instantiate(source);
                data.name = "CheckBottle_" + check.ToString();
                // Kept out of the offer pool: placing one is for the code that owns this check, and
                // GetUnlockedConsumablesForRegion drops a region-specific item that no region lists.
                data.regionSpecific = true;
                data.notRandomlyGiven = true;

                checkBottleData[check] = data;
                ScriptableObjectLoader<ItemData>.AllData.Add(data);
            }
        }

        // A bottle that has already rolled a sigil carries it in the name, so an exact match alone
        // would miss the very asset this needs to copy.
        private static ConsumableItemData FindBottleData(string bottleName)
        {
            return ItemsUtil.AllConsumables.Find(x => x.name == bottleName || x.name.StartsWith(bottleName + "$"));
        }

        internal static ConsumableItemData GetCheckBottleData(APCheck check)
        {
            RegisterCheckBottleData();

            return checkBottleData.TryGetValue(check, out ConsumableItemData data) ? data : null;
        }

        // Whether a consumable check still has anywhere to go. One already sent is finished with, and
        // one sitting in the player's slots is spoken for: putting either back on offer duplicates it.
        internal static bool ConsumableCheckAvailable(APCheck check)
        {
            if (ArchipelagoManager.HasCompletedCheck(check)) return false;

            return !(RunState.Run?.consumables?.Contains("CheckBottle_" + check.ToString()) ?? false);
        }

        [HarmonyPatch(typeof(ItemsManager), "UpdateItems")]
        [HarmonyFinalizer]
        static void UnmarkUpdateItemsInProgress()
        {
            updatingItems = false;
            ApplyPendingRenames();
        }

        // A bottle's rolled sigil (or check id) is encoded into the live ItemData's name; the
        // saved list needs the same rename, queued here and applied once it's safe.
        internal static void RenameItemInSaveData(ItemSlot slot, string oldName, string newName)
        {
            if (oldName == newName) return;

            ItemsManager manager = Singleton<ItemsManager>.Instance;
            // CreateItem also fires for slots that are not inventory -- card choice offers, trader
            // displays -- whose items share these names and must never rewrite the saved list.
            if (manager == null || !(slot is ConsumableItemSlot consumableSlot)) return;
            if (!manager.consumableSlots.Contains(consumableSlot)) return;

            pendingRenames.Add((oldName, newName));
            if (!updatingItems) ApplyPendingRenames();
        }

        private static void ApplyPendingRenames()
        {
            if (pendingRenames.Count == 0) return;

            ItemsManager manager = Singleton<ItemsManager>.Instance;
            if (manager == null) return;

            var toApply = new List<(string oldName, string newName)>(pendingRenames);
            pendingRenames.Clear();
            foreach (var rename in toApply)
            {
                int index = manager.SaveDataItemsList.IndexOf(rename.oldName);
                if (index >= 0) manager.SaveDataItemsList[index] = rename.newName;
            }
        }

        internal static CardInfo GenerateCardInfoWithName(string name, string description)
        {
            CardInfo info = ScriptableObject.CreateInstance<CardInfo>();
            info.name = "Archipelago_" + name;
            info.displayedName = name;
            info.hideAttackAndHealth = true;
            string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            info.portraitTex = AssetsManager.cardPortraitSprite;
            info.pixelPortrait = AssetsManager.cardPixelPortraitSprite;
            info.description = description;
            return info;
        }

        internal static APCheck GetCheckGainedFromNPC(Character npcCharacter)
        {
            if (npcCheckPairs.TryGetValue(npcCharacter, out APCheck check))
                return check;
            else
                return APCheck.COUNT;
        }

        internal static IEnumerator CombatRewardCheckSequence(CardBattleNPC npc)
        {
            if (npc.gainPacks != null)
            {
                npc.gainPacks = null;
                APCheck check = GetCheckGainedFromNPC(npc.DialogueSpeaker.characterId);
                yield return GiveGBCCheckSequence(check);
            }
        }

        internal static void GiveObjectRelatedCheck(GameObject instance)
        {
            string objectPath = instance.transform.GetPath();
            string key = $"{SceneLoader.ActiveSceneName}/{objectPath}";
            if (gbcObjectCheckPair.TryGetValue(key, out APCheck check) && !ArchipelagoManager.HasCompletedCheck(check))
            {
                GiveGBCCheck(check);
            }
        }

        internal static void GiveGBCCheck(APCheck check)
        {
            CustomCoroutine.Instance.StartCoroutine(GiveGBCCheckSequence(check));
        }

        internal static IEnumerator GiveGBCCheckSequence(APCheck check)
        {
            CardInfo card = GenerateCardInfo(check);
            if (!ArchipelagoManager.HasCompletedCheck(check))
            {
                Singleton<PlayerMovementController>.Instance.SetEnabled(false);
                yield return SingleCardGainUI.instance.GainCard(card, true);
                Singleton<PlayerMovementController>.Instance.SetEnabled(true);
            }
            else
            {
                yield return null;
            }
        }

        internal static void GiveGBCCheckWithMessage(APCheck check, string message)
        {
            CustomCoroutine.instance.StartCoroutine(GiveGBCCheckWithMessageSequence(check, message));
        }

        internal static IEnumerator GiveGBCCheckWithMessageSequence(APCheck check, string message)
        {
            Singleton<PlayerMovementController>.Instance.SetEnabled(false);
            yield return Singleton<TextBox>.Instance.ShowUntilInput(message, TextBox.Style.Nature);
            yield return new WaitForSeconds(0.25f);
            yield return GiveGBCCheckSequence(check);
            if (!Singleton<PlayerMovementController>.Instance.enabled)
                Singleton<PlayerMovementController>.Instance.SetEnabled(true);
        }

        internal static string GetCardGainedMessage(CardInfo info)
        {
            if (info.name.Contains("Archipelago"))
                return "The card was sent to its rightful owner.";
            else
                return "The card was added to your collection.";
        }

        internal static IEnumerator OnPackButtonPressed()
        {
            PauseMenu.instance.SetPaused(false);
            PauseMenu.pausingDisabled = true;

            if (Singleton<PlayerMovementController>.Instance != null)
                Singleton<PlayerMovementController>.Instance.SetEnabled(false);

            yield return new WaitForSeconds(0.25f);

            bool result = false;
            TextBox.Prompt prompt = new TextBox.Prompt("Open a pack", "Cancel", option => result = (option == 0));
            int packsAvailable = APSaveFile.PacksAvailable(2);
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"You have {packsAvailable} card pack{(packsAvailable > 1 ? "s" : "")} available.", TextBox.Style.Neutral, null, TextBox.ScreenPosition.ForceTop, 0, true, false, prompt);
            if (result)
            {
                APSaveFile.SpendPack(2);

                // Advances the seed AssignInfoToCards reads, so successive packs differ and a
                // reload cannot reroll one. Also shifts Act 1 and 3's; see AddOpenedPacksToSeed.
                SaveManager.SaveFile.gbcData.packsOpened++;

                // Vanilla picks the pack's cards from the save's seed but nothing chose its temple,
                // so it was rolled fresh on every open and the contents moved with it.
                CardTemple temple = (CardTemple)SeededRandom.Range(0, (int)CardTemple.NUM_TEMPLES,
                    SaveManager.SaveFile.GetCurrentRandomSeed());

                yield return PackOpeningUI.instance.OpenPack(temple);

                // The spend, the counter and the cards the pack added all land together, rather
                // than waiting on a later save that leaving the act would discard.
                SaveManager.SaveToFile(false);
            }

            yield return new WaitForSeconds(0.05f);

            UpdatePackButtonEnabled();

            if (Singleton<PlayerMovementController>.Instance != null)
                Singleton<PlayerMovementController>.Instance.SetEnabled(true);

            PauseMenu.instance.SetPaused(true);
            PauseMenu.instance.menuController.PlayMenuCardImmediate((PauseMenu.instance as GBCPauseMenu).modifyDeckCard);
            PauseMenu.pausingDisabled = false;
        }

        internal static void UpdatePackButtonEnabled()
        {
            if (packButton == null) return;

            packButton.SetEnabled(APSaveFile.PacksAvailable(2) > 0 && SceneLoader.ActiveSceneName != "GBC_WorldMap");
        }

        // The generator walks consecutive seeds up from the one it is given, so each pack and retry needs its
        // own block. Kept coprime, with MAX_PACK_ROLLS * RETRY_STRIDE < PACK_STRIDE so blocks cannot overlap.
        private const int PACK_SEED_STRIDE = 7919;
        private const int RETRY_SEED_STRIDE = 251;
        private const int MAX_PACK_ROLLS = 8;

        // Uses vanilla's own Act 1 choice generator, so a pack offers what a card choice node
        // would have. It can yield fewer than asked, so reroll with a fresh seed until full.
        internal static List<CardInfo> RollPackCards(int act, int count)
        {
            List<CardInfo> cards = new List<CardInfo>();

            CardChoiceGenerator generator;
            if (act == 3)
            {
                if (Singleton<HoloMapAreaManager>.Instance == null) return cards;
                generator = new Part3CardChoiceGenerator();
            }
            else
            {
                if (RunState.Run == null || RunState.CurrentMapRegion == null) return cards;
                generator = new Part1CardChoiceGenerator();
            }

            CardChoicesNodeData data = new CardChoicesNodeData { choicesType = CardChoicesType.Random };

            // The run seed only moves between map nodes, so packs opened in one spot would roll
            // alike. Packs opened is saved state, so it varies them without letting a reload reroll.
            int packsOpened = ArchipelagoManager.CountReceived(
                    act == 3 ? APItem.Act3CardPack : APItem.Act1CardPack)
                - APSaveFile.PacksAvailable(act);
            int packSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + packsOpened * PACK_SEED_STRIDE;

            for (int attempt = 0; attempt < MAX_PACK_ROLLS && cards.Count < count; attempt++)
            {
                int seed = packSeed + attempt * RETRY_SEED_STRIDE;

                foreach (CardChoice choice in generator.GenerateChoices(data, seed))
                {
                    if (choice.CardInfo == null) continue;
                    // The generator reads vanilla's pool, which still holds the cards Archipelago
                    // hands out as items, so a pack could deal one before its item arrived.
                    if (ArchipelagoManager.CardIsWithheld(choice.CardInfo.name)) continue;
                    if (cards.Exists(existing => existing.name == choice.CardInfo.name)) continue;

                    cards.Add(choice.CardInfo);
                    if (cards.Count == count) break;
                }
            }

            return cards;
        }

        internal static void RefreshPackPile()
        {
            if (CabinPackPile.instance != null) CabinPackPile.instance.Rebuild();
        }

        internal static IEnumerator PrePlayerDeathSequence(Part1GameFlowManager manager)
        {
            if (!DeathLinkManager.receivedDeath && ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.Sacrificed)
                DeathLinkManager.SendDeathLink();
            if ((!DeathLinkManager.receivedDeath && ArchipelagoOptions.optionalDeathCard == OptionalDeathCard.EnableOnlyOnDeathLink)
                || ArchipelagoOptions.optionalDeathCard == OptionalDeathCard.Disable)
            {
                doDeathCard = true;
                Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);
                yield return manager.KillPlayerSequence();
                yield break;
            }
            if (Singleton<GameMap>.Instance.FullyUnrolled)
                Singleton<GameMap>.Instance.HideMapImmediate();
            yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Choose if you want to create a new death card.");
            CardChoicesNodeData choice = new CardChoicesNodeData();
            choice.gemifyChoices = true;
            CardChoice c1 = new CardChoice();
            c1.CardInfo = GenerateCardInfoWithName("Yes", "You will create a death card before restarting.");
            CardChoice c2 = new CardChoice();
            c2.CardInfo = GenerateCardInfoWithName("No", "You will restart without creating a new death card.");
            choice.overrideChoices = new List<CardChoice> {c1, c2};
            Singleton<ViewManager>.Instance.SwitchToView(View.BoardCentered, false, true);
            yield return Singleton<CardSingleChoicesSequencer>.Instance.CardSelectionSequence(choice);
            Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);
            if (Singleton<CardSingleChoicesSequencer>.Instance.chosenReward.Info.name == "Archipelago_Yes")
                doDeathCard = true;
            else
                doDeathCard = false;
            yield return manager.KillPlayerSequence();
        }

        internal static IEnumerator LeshySaysMessage(string message)
        {
            yield return Singleton<TextDisplayer>.Instance.ShowMessage(message);
        }

        internal static void AfterPlayerDeathSequence()
        {
            if (doDeathCard)
                SceneLoader.Load("Part1_Sanctum");
            else
            {
                SaveManager.SaveFile.NewPart1Run();
                SceneLoader.Load("Part1_Cabin");
            }
        }

        internal static IEnumerator TraderPeltRewardCheckSequence(TraderMaskInteractable instance)
        {
            APCheck check = APCheck.FactoryTrader1 + Part3SaveData.Data.collectedTarots.Count;
            if (ArchipelagoManager.HasCompletedCheck(check))
            {
                Part3SaveData.Data.collectedTarots.Add(instance.GetAvailableTarotTypes().First());
                yield break; 
            }

            Singleton<InteractionCursor>.Instance.InteractionDisabled = false;
            Singleton<ViewManager>.Instance.OffsetPosition(new Vector3(0f, -2f, 8f), 0.25f);
            Singleton<ViewManager>.Instance.OffsetRotation(new Vector3(50f, 0f, 0f), 0.25f);

            yield return new WaitForSeconds(0.1f);

            GameObject reference = new GameObject("Ref");
            reference.transform.SetParent(instance.cardsParent);
            reference.transform.ResetTransform();
            reference.transform.localScale = Vector3.one * 0.5f;
            reference.AddComponent<BoxCollider>().size = new Vector3(1.2f, 1.8f, 0.4f);

            DiscoverableCheckInteractable checkCard = CreateDiscoverableCardCheck(reference, check, true);
            checkCard.closeUpDistance *= 1.5f;
            Vector3 targetPos = checkCard.transform.position;
            checkCard.transform.position += new Vector3(0, 3, 1);
            Tween.Position(checkCard.transform, targetPos, 0.25f, 0f, Tween.EaseIn);
            checkCard.SetEnabled(false);

            yield return new WaitForSeconds(0.25f);

            checkCard.SetEnabled(true);

            yield return new WaitUntil(() => checkCard.Discovering);
            yield return new WaitUntil(() => !checkCard.Discovering);

            Part3SaveData.Data.collectedTarots.Add(instance.GetAvailableTarotTypes().First());

            yield return new WaitForSeconds(0.75f);

            Singleton<ViewManager>.Instance.OffsetPosition(Vector3.zero, 0.25f);
            Singleton<ViewManager>.Instance.OffsetRotation(new Vector3(10f, 0f, 0f), 0.25f);
        }

        internal static IEnumerator BlowOutOneOrAllCandles(bool fromBoss)
        {
            if (DeathLinkManager.receivedDeath && ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.Sacrificed)
            {
                if (Singleton<GameMap>.Instance.FullyUnrolled)
                    Singleton<GameMap>.Instance.HideMapImmediate();
                while (RunState.Run.playerLives > 0)
                {
                    yield return Singleton<CandleHolder>.Instance.BlowOutCandleSequence(fromBoss);
                }
            }
            else
            {
                if (ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.CandleExtinguished)
                    DeathLinkManager.SendDeathLink();

                yield return Singleton<CandleHolder>.Instance.BlowOutCandleSequence(fromBoss);
            }
        }

        public static CardInfo RandomizeOneCardAct3(ref int seed, ref List<CardInfo> cardsInfoRandomPool, ref List<CardInfo> cardsInfoRandomGemPool, ref List<CardInfo> cardsInfoRandomConduitPool, CardInfo c)
        {
            CardInfo card;
            if (ArchipelagoOptions.randomizeDeck == RandomizeDeck.RandomizeType)
            {
                if (c.name.Contains("Conduit") || c.name.Contains("Cell"))
                    card = cardsInfoRandomConduitPool[SeededRandom.Range(0, cardsInfoRandomConduitPool.Count, seed++)];
                else if (c.name.Contains("Sentinel") || c.name.Contains("Gem"))
                    card = cardsInfoRandomGemPool[SeededRandom.Range(0, cardsInfoRandomGemPool.Count, seed++)];
                else
                    card = cardsInfoRandomPool[SeededRandom.Range(0, cardsInfoRandomPool.Count, seed++)];
            }
            else
                card = cardsInfoRandomPool[SeededRandom.Range(0, cardsInfoRandomPool.Count, seed++)];
            if (card.name == "BlueMage_Talking" || card.name == "Angler_Talking" || card.name == "Ouroboros_Part3" || card.name == "!BUILDACARD_BASE" || card.name == "!MYCOCARD_BASE")
                cardsInfoRandomPool.Remove(card);
            if (card.name != "!BUILDACARD_BASE" && card.name != "!MYCOCARD_BASE")
                card = (CardInfo)card.Clone();
            return card;
        }

        internal static List<CardInfo> GetAllDeathCards()
        {
            List<CardInfo> list = new List<CardInfo>();
            List<CardModificationInfo> choosableDeathcardMods = SaveManager.SaveFile.GetChoosableDeathcardMods();
            if (choosableDeathcardMods.Count > 0)
            {
                foreach (CardModificationInfo deathcardMod in choosableDeathcardMods)
                    list.Add(CardLoader.CreateDeathCard(deathcardMod));
            }
            else
            {
                CardModificationInfo cardModificationInfo2 = new CardModificationInfo();
                cardModificationInfo2.nameReplacement = "Luke Carder";
                cardModificationInfo2.deathCardInfo = new DeathCardInfo(CompositeFigurine.FigurineType.Gravedigger, 0, 0);
                cardModificationInfo2.attackAdjustment = 4;
                cardModificationInfo2.healthAdjustment = 4;
                list.Add(CardLoader.CreateDeathCard(cardModificationInfo2));
            }
            return list;
        }

        internal static List<CardInfo> GetAllCustomCards()
        {
            List<CardInfo> list = new List<CardInfo>();

            foreach (CardModificationInfo customCardMod in ArchipelagoData.Data.customCardsModsAct3)
            {
                CardInfo c = CardLoader.GetCardByName("!BUILDACARD_BASE");
                c.mods.Add(customCardMod);
                list.Add(c);
            }

            if (ArchipelagoData.Data.mycoCardMod != null)
            {
                CardInfo c = CardLoader.GetCardByName("!MYCOCARD_BASE");
                c.mods.Add(ArchipelagoData.Data.mycoCardMod);
                list.Add(c);
            }

            return list;
        }

        internal static void AddCustomMod(CardModificationInfo mod, string name)
        {
            ArchipelagoData.Data.customCardInfos.Add(new CustomCardInfo(mod.singletonId, name, mod.attackAdjustment, mod.healthAdjustment, mod.energyCostAdjustment, mod.abilities, mod.buildACardPortraitInfo.spriteIndices));
            ArchipelagoData.Data.customCardsModsAct3.Add(mod);
        }

        internal static void AddMycoMod(CardModificationInfo mod)
        {
            ArchipelagoData.Data.mycoCardInfo = new CustomCardInfo(mod.singletonId, "Mycobot", mod.attackAdjustment, mod.healthAdjustment, mod.energyCostAdjustment, mod.abilities, null);
            ArchipelagoData.Data.mycoCardMod = mod;
        }

        internal static CardInfo RandomRareCardInAct1(int seed)
        {
            List<CardInfo> cardsInfoRandomPool = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => (x.metaCategories.Contains(CardMetaCategory.Rare)
            && x.temple == CardTemple.Nature && x.portraitTex != null && !x.metaCategories.Contains(CardMetaCategory.AscensionUnlock) && ConceptProgressionTree.Tree.CardUnlocked(x, false)
            && !ArchipelagoManager.CardIsWithheld(x.name)) || x.name == "Ouroboros");
            return (CardInfo)cardsInfoRandomPool[SeededRandom.Range(0, cardsInfoRandomPool.Count, seed++)];
        }

        internal static void RemoveUniqueAct1CardIfApplicable(ref List<CardInfo> cardsInfoRandomPool, ref CardInfo card)
        {
            if (card.name == "Stoat_Talking" || card.name == "Stinkbug_Talking" || card.name == "Wolf_Talking" || card.name == "CagedWolf")
                cardsInfoRandomPool.Remove(card);
        }

        internal static void UpdateItemsWhenDoneDiscovering(DiscoverableCheckInteractable discoveringCard)
        {
            CustomCoroutine.Instance.StartCoroutine(UpdateItemsWhenDoneDiscoveringSequence(discoveringCard));
        }

        private static IEnumerator UpdateItemsWhenDoneDiscoveringSequence(DiscoverableCheckInteractable card)
        {
            yield return new WaitUntil(() => !card.Discovering);
            Singleton<ItemsManager>.Instance.UpdateItems();
        }

        internal static void OldDataOpened()
        {
            ArchipelagoData.Data.epilogueCompleted = true;
            ArchipelagoManager.VerifyGoalCompletion();
        }

        private static void OnStartLoadEpilogue()
        {
            Singleton<ArchipelagoUI>.Instance.StartCoroutine(LoadAppropriateSceneAfterAct3());
        }

        // Both callers arrive here having just finished an act rather than abandoning one, so that
        // is committed first: leaving discards whatever is still unsaved.
        internal static void GoToMainMenu()
        {
            SaveManager.SaveToFile(false);
            ArchipelagoManager.MarkActCommittedOnLeave();

            StartScreenController.startedGame = true;
            MenuController.ReturnToStartScreen();
        }

        private static IEnumerator LoadAppropriateSceneAfterAct3()
        {
            if (!ArchipelagoOptions.skipEpilogue && (!ArchipelagoOptions.enableAct1 || ArchipelagoData.Data.act1Completed) && 
            (!ArchipelagoOptions.enableAct2 || ArchipelagoData.Data.act2Completed))
            {
                AsyncOperation asyncOp = SceneLoader.StartAsyncLoad("finale_grimora");
                asyncOp.allowSceneActivation = false;
                yield return new WaitForSeconds(7.55f);
                SceneLoader.CompleteAsyncLoad(asyncOp);
            }
            else
            {
                yield return new WaitForSeconds(7.55f);
                GoToMainMenu();
            }
        }

        public static string GetPaintingAnimal()
        {
            if (ArchipelagoOptions.randomizeChallenges != RandomizeChallenges.Disable &&
                !ArchipelagoManager.HasItem(APItem.ProgressiveSquirrel))
                    return "AquaSquirrel";
            return StoryEventsData.EventCompleted(StoryEvent.BeeFigurineFound) ? "Bee" : "Squirrel";
        }

        // Every card the side deck has ever handed out, so a stale solution can be spotted whichever
        // one it was baked with.
        private static readonly string[] paintingAnimals = new string[] { "AquaSquirrel", "Squirrel", "Bee" };

        // The solution is baked into the save when a run starts, so an upgrade arriving mid-run would
        // otherwise leave the painting asking for a side deck card the player can no longer draw.
        internal static void RefreshPaintingAnimal()
        {
            OilPaintingPuzzle.SaveState state = SaveFile.IsAscension
                ? AscensionSaveData.Data.oilPaintingState
                : SaveManager.SaveFile.oilPaintingState;

            if (state == null || state.puzzleSolution == null || state.puzzleSolved)
                return;

            string animal = GetPaintingAnimal();
            int index = state.puzzleSolution.FindIndex(x => paintingAnimals.Contains(x));

            if (index < 0 || state.puzzleSolution[index] == animal)
                return;

            state.puzzleSolution[index] = animal;

            // The painting only redraws itself once per showing, so an open one has to be told to.
            OilPaintingPuzzle puzzle = UnityEngine.Object.FindObjectOfType<OilPaintingPuzzle>();

            if (puzzle != null)
                puzzle.displayPuzzleWhenActive = true;
        }

        public static bool IsLeshyNotReadyForBattle(List<CardBattleNPC> battleNPCs)
        {
            return battleNPCs.Exists((CardBattleNPC x) => !x.Defeated) || !ArchipelagoManager.HasCompletedCheck(APCheck.GBCCameraReplica);
        }

        public static List<CardInfo> GenerateCardPoolAct1()
        {
            List<CardInfo> cardsInfoRandomPool;

            if (ArchipelagoOptions.randomizeDeck == RandomizeDeck.RandomizeAll)
            {
                cardsInfoRandomPool = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => ((x.metaCategories.Contains(CardMetaCategory.Rare) || x.metaCategories.Contains(CardMetaCategory.ChoiceNode))
                                             && x.temple == CardTemple.Nature && x.portraitTex != null 
                                             && !x.metaCategories.Contains(CardMetaCategory.AscensionUnlock) && ConceptProgressionTree.Tree.CardUnlocked(x, false)) 
                                             || x.name == "Ouroboros");
            }
            else
            {
                cardsInfoRandomPool = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => x.temple == CardTemple.Nature && x.portraitTex != null
                                      && x.metaCategories.Contains(CardMetaCategory.ChoiceNode) && !x.metaCategories.Contains(CardMetaCategory.AscensionUnlock)
                                      && !x.metaCategories.Contains(CardMetaCategory.Rare) && ConceptProgressionTree.Tree.CardUnlocked(x, false));
            }

            cardsInfoRandomPool.RemoveAll(c => ArchipelagoManager.CardIsWithheld(c.name));

            // The talking cards this mod made choosable are in the base result above already; only
            // the stoat, which Archipelago does not hand out, still has to be added by hand.
            cardsInfoRandomPool.Add(CardLoader.GetCardByName("Stoat_Talking"));
            cardsInfoRandomPool.AddRange(GetAllDeathCards());

            return cardsInfoRandomPool;
        }

        // What a randomized Act 3 starter deck is built from. The bots and Ourobot are choosable now,
        // so this pool already holds them, withheld until their item lands and capped at one copy.
        public static List<CardInfo> GenerateStarterPoolAct3()
        {
            return CardLoader.GetUnlockedCards(CardMetaCategory.ChoiceNode, CardTemple.Tech);
        }

        public static bool BleachTrapRemoveSigils()
        {
            List<CardSlot> ownValidCardSlots = Singleton<BoardManager>.Instance.PlayerSlotsCopy;
			ownValidCardSlots.RemoveAll((CardSlot x) => x.Card == null || x.Card.Info.Abilities.Count <= 0 || 
                x.Card.temporaryMods.Any(mod => mod.negateAbilities.Count != 0));
            if (ownValidCardSlots.Count == 0) return false;
            if (SaveManager.SaveFile.IsPart2) {
                foreach (CardSlot cardSlot in ownValidCardSlots)
                {
                    var info = UnityEngine.Object.Instantiate(cardSlot.Card.Info);
                    info.name = cardSlot.Card.Info.name;
                    for (int i = 0; i < info.mods.Count; i++)
                    {
                        if (info.mods[i] is SigilReplacementInfo)
                        {
                            info.mods.RemoveAt(i--);
                        }
                    }
                    cardSlot.Card.SetInfo(info);
                }
            }
            AudioController.Instance.PlaySound2D("magnificus_brush_splatter_bleach", MixerGroup.None, 0.5f);
            BleachPotItem bleach = new BleachPotItem();
            foreach (CardSlot cardSlot in ownValidCardSlots)
            {
                bleach.RemoveCardAbilities(cardSlot.Card);
            }
            return true;
        }

        public static int NewDeckSize()
        {
            return 20 + APSaveFile.DeckSizeTrapsInEffect;
        }

        // Every starter deck is 20 cards and nothing else fills the collection that far, so this is
        // what tells one that has been picked up from one holding only what Archipelago granted.
        internal static bool Act2StarterDeckTaken
            => (SaveManager.SaveFile?.gbcData?.collection?.cardIds?.Count ?? 0) >= 20;
    }
}
