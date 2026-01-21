
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Archipelago_Inscryption.Archipelago;
using DiskCardGame;
using GBC;
using HarmonyLib;
using UnityEngine;

namespace Archipelago_Inscryption.Patches
{
    [HarmonyPatch]
    internal class CardPatches
    {
        static int nodeId = 0;
        static int nodeOffset = 0;
        public static void RandomizeSigils(CardInfo card)
        {
            if (ArchipelagoOptions.randomizeSigils != Archipelago.RandomizeSigils.RandomizeOnce)
            {
                return;
            }
            ScriptableObjectLoader<AbilityInfo>.LoadData("Abilities");
            List<AbilityInfo> learnedAbilities;
            if (SaveManager.SaveFile.IsPart1)
            {
                if (card.name == "!DEATHCARD_BASE")
                {
                    return;
                }
                learnedAbilities = ScriptableObjectLoader<AbilityInfo>.allData.FindAll(
                    x => x.metaCategories.Contains(AbilityMetaCategory.Part1Modular)
                    && x.metaCategories.Contains(AbilityMetaCategory.Part1Rulebook)
                    // && x.ability != Ability.RandomAbility // is there a reason this should be excluded, like in deck randomizer?
                    && x.ability != Ability.CreateEgg
                    && x.ability != Ability.HydraEgg
                );
            }
            else if (SaveManager.SaveFile.IsPart3)
            {
                learnedAbilities = ScriptableObjectLoader<AbilityInfo>.allData.FindAll(
                    x => x.metaCategories.Contains(AbilityMetaCategory.Part3Modular)
                    && x.metaCategories.Contains(AbilityMetaCategory.Part3Rulebook)
                );
            }
            else
            {
                return;
            }
            var replacement = new SigilReplacementInfo();
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
            if (nodeId != RunState.Run.currentNodeId)
            {
                nodeId = RunState.Run.currentNodeId;
                nodeOffset = 0;
            }
            seed += nodeOffset * 44;
            // 44 is arbitrary, to make sequential nodes not get the same seeds
            // because inscryption's rng kinda sucks actually
            nodeOffset++;
            for (int i = 0; i < card.abilities.Count; i++)
            {
                var newAbility = learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, seed++)];
                replacement.abilities.Add(newAbility.ability);
                learnedAbilities.Remove(newAbility);
            }
            if (replacement.abilities.Count <= 0) return;
            card.mods.Add(replacement);
        }

        public static void RandomizeSigilsAct2(CardInfo card)
        {
            if (ArchipelagoOptions.randomizeSigils != Archipelago.RandomizeSigils.RandomizeOnce)
            {
                return;
            }
            List<AbilityInfo> learnedAbilities = ScriptableObjectLoader<AbilityInfo>.allData.FindAll(
                x => x.pixelIcon != null
                && x.ability != Ability.ActivatedSacrificeDrawCards
                && x.ability != Ability.CreateEgg
                && x.ability != Ability.HydraEgg
                && x.ability != Ability.Tutor
            );
            // string hash function: djb2 by Dan Bernstein via http://www.cse.yorku.ca/~oz/hash.html
            var hash = 5381;
            foreach (char c in card.displayedName)
            {
                hash = (hash << 5) + hash + c;
            }
            var seed = hash ^ SaveManager.SaveFile.randomSeed;

            card.mods = new List<CardModificationInfo>();
            var replacement = new SigilReplacementInfo();
            for (int i = 0; i < card.abilities.Count; i++)
            {
                var newAbility = learnedAbilities[SeededRandom.Range(0, learnedAbilities.Count, seed++)];
                replacement.abilities.Add(newAbility.ability);
                learnedAbilities.Remove(newAbility);
            }
            if (replacement.abilities.Count <= 0) return;
            card.mods.Add(replacement);
        }

        [HarmonyPatch(typeof(Part1CardChoiceGenerator), "GenerateDirectChoices")]
        [HarmonyPatch(typeof(Part3CardChoiceGenerator), "GenerateChoices")]
        [HarmonyPatch(typeof(Part1RareChoiceGenerator), "GenerateChoices")]
        [HarmonyPatch(typeof(DuplicateMergeSequencer), "GetDuplicateCardChoices")]
        [HarmonyPostfix]
        static void RandomizeDirectCardChoiceSigils(ref List<CardChoice> __result)
        {
            foreach (var choice in __result)
            {
                // there is one place in Part1CardChoiceGenerator::GenerateDirectChoices that
                // doesn't properly clone the card it's getting. so we do that here instead
                var card = UnityEngine.Object.Instantiate(choice.CardInfo);
                card.mods = new List<CardModificationInfo>(choice.CardInfo.mods);
                card.name = choice.CardInfo.name;
                RandomizeSigils(card);
                choice.CardInfo = card;
            }
        }
        [HarmonyPatch(typeof(TradePeltsSequencer), "GetTradeCardInfos")]
        [HarmonyPatch(typeof(DeckTrialSequencer), "GenerateRewardChoices")]
        [HarmonyPostfix]
        static void RandomizeDirectCardInfoSigils(ref List<CardInfo> __result)
        {
            foreach (var card in __result)
            {
                RandomizeSigils(card);
            }
        }
        [HarmonyPatch(typeof(GainConsumablesSequencer), "FullConsumablesSequence")]
        [HarmonyPrefix]
        static void RandomizeRatSigils(GainConsumablesSequencer __instance)
        {
            RandomizeSigils(__instance.fullConsumablesReward);
        }

        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "CostChoiceChosen", MethodType.Enumerator)]
        [HarmonyPatch(typeof(CardSingleChoicesSequencer), "TribeChoiceChosen", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RandomizeCostTribeChoiceSigils(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var found = false;
            foreach(var instruction in instructions)
            {
                if (instruction.Calls(typeof(Card).GetMethod("SetInfo")))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardPatches), "RandomizeSigils", [typeof(CardInfo)]));
                    found = true;
                }
                yield return instruction;
            }
            if (!found)
            {
                ArchipelagoModPlugin.Log.LogError($"Cannot find Card::SetInfo in {__originalMethod.Name}");
            }
        }

        [HarmonyPatch(typeof(BoulderChoiceSequencer), "RewardSequence", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RandomizeBoulderRewardSigils(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var found = false;
            foreach(var instruction in instructions)
            {
                if (instruction.Calls(typeof(SelectableCard).GetMethod("Initialize", [typeof(CardInfo), typeof(Action<SelectableCard>), typeof(Action<SelectableCard>), typeof(bool), typeof(Action<SelectableCard>)])))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardPatches), "RandomizeSigils", [typeof(CardInfo)]));
                    found = true;
                }
                yield return instruction;
            }
            if (!found)
            {
                ArchipelagoModPlugin.Log.LogError($"Cannot find SelectableCard::Initialize in {__originalMethod.Name}");
            }
        }

        [HarmonyPatch(typeof(DeckInfo), "InitializeAsPlayerDeck")]
        [HarmonyPostfix]
        static void RandomizeStartingDeckSigils(DeckInfo __instance)
        {
            foreach (var card in __instance.Cards)
            {
                RandomizeSigils(card);
            }
        }

        [HarmonyPatch(typeof(CardInfo), "DefaultAbilities", MethodType.Getter)]
        [HarmonyPrefix]
        static bool SuppressOriginalSigils(CardInfo __instance, ref List<Ability> __result)
        {
            if (__instance.Mods.Exists(m => m is SigilReplacementInfo))
            {
                __result = new List<Ability>();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CardAbilityIcons), "UpdateAbilityIcons")]
        [HarmonyPrefix]
        static void AddFourSigilPositions(CardAbilityIcons __instance)
        {
            if (__instance.defaultIconGroups.Count >= 4) return;
            var three = __instance.defaultIconGroups[2];
            var four = UnityEngine.Object.Instantiate(three);
            var icons = four.GetComponentsInChildren<AbilityIconInteractable>();
            icons[2].OriginalLocalPosition = new Vector3(
                icons[0].OriginalLocalPosition.x,
                icons[2].OriginalLocalPosition.y,
                icons[2].OriginalLocalPosition.z
            );
            icons[2].transform.localPosition = new Vector3(
                icons[0].transform.localPosition.x,
                icons[2].transform.localPosition.y,
                icons[2].transform.localPosition.z
            );
            var fourth = UnityEngine.Object.Instantiate(icons[2]);
            fourth.OriginalLocalPosition = new Vector3(
                icons[1].OriginalLocalPosition.x,
                icons[2].OriginalLocalPosition.y,
                icons[2].OriginalLocalPosition.z
            );
            fourth.transform.localPosition = new Vector3(
                icons[1].transform.localPosition.x,
                icons[2].transform.localPosition.y,
                icons[2].transform.localPosition.z
            );
            fourth.transform.parent = four.transform; // why is this how this works I hate unity
            four.transform.parent = three.transform.parent;
            four.transform.localPosition = three.transform.localPosition;
            four.transform.localScale = three.transform.localScale;
            __instance.defaultIconGroups.Add(four);
        }

        [HarmonyPatch(typeof(ItemSlot), "CreateItem", [typeof(ItemData), typeof(bool)])]
        [HarmonyPrefix]
        static void DontModifyItemTemplates(ref ItemData data)
        {
            if (!data.name.Contains("Bottle")) return;
            var name = data.name;
            data = UnityEngine.Object.Instantiate(data);
            data.name = name;
        }
        [HarmonyPatch(typeof(ItemSlot), "CreateItem", [typeof(ItemData), typeof(bool)])]
        [HarmonyPostfix]
        static void RandomizeBottleSigil(ItemData data, bool skipDropAnimation, ItemSlot __instance)
        {
            if (__instance.Item is CardBottleItem)
            {
                var bottle = __instance.Item as CardBottleItem;
                var info = UnityEngine.Object.Instantiate(bottle.cardInfo);
                info.name = bottle.cardInfo.name;
                if (data.name.Contains("$"))
                {
                    var replacement = new SigilReplacementInfo();
                    replacement.abilities.Add((Ability)Enum.Parse(typeof(Ability), data.name.Substring(data.name.IndexOf("$") + 1)));
                    info.mods.Add(replacement);
                }
                else
                {
                    RandomizeSigils(info);
                    var mod = info.mods.Find(m => m is SigilReplacementInfo);
                    if (mod is not null)
                    {
                        data.name += "$" + mod.abilities[0].ToString();
                    }
                }
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

        [HarmonyPatch(typeof(CardBottleItem), "ActivateSequence")]
        [HarmonyPostfix] // enumerators are weird. postfix without calling __result.MoveNext() lets us replace the functionality
        static IEnumerator GiveCardWithBottleSigil(IEnumerator __result, CardBottleItem __instance)
        {
            __instance.PlayExitAnimation();
            yield return __instance.StartCoroutine(Singleton<CardSpawner>.Instance.SpawnCardToHand(__instance.cardInfo));
            yield return new WaitForSeconds(0.25f);
        }

        [HarmonyPatch(typeof(ItemsUtil), "GetConsumableByName")]
        [HarmonyPrefix]
        static void GetCorrectBottleItem(ref string name, ref string __state)
        {
            __state = name;
            if (name.Contains("$"))
            {
                name = name.Substring(0, name.IndexOf("$"));
            }
        }

        [HarmonyPatch(typeof(ItemsUtil), "GetConsumableByName")]
        [HarmonyPostfix]
        static void RememberBottleSigil(ref ConsumableItemData __result, string __state)
        {
            __result = UnityEngine.Object.Instantiate(__result);
            __result.name = __state;
        }

        [HarmonyPatch(typeof(ItemsManager), "UpdateItems")]
        [HarmonyPostfix]
        static void FixBottlesInSaveFile(ref ItemsManager __instance)
        {
            __instance.SaveDataItemsList.Clear();
            foreach (var slot in __instance.consumableSlots)
            {
                if (slot.Item is not null)
                {
                    __instance.SaveDataItemsList.Add(slot.Item.Data.name);
                }
            }
        }

        [HarmonyPatch(typeof(CardCollectionInfo), "LoadCards")]
        [HarmonyPostfix]
        static void Act2RandomizeSigilsOnLoad(CardCollectionInfo __instance)
        {
            if (!SaveManager.SaveFile.IsPart2)
            {
                return;
            }
            foreach (var card in __instance.CardInfos)
            {
                RandomizeSigilsAct2(card);
            }
        }

        [HarmonyPatch(typeof(CardCollectionInfo), "AddCard")]
        [HarmonyPatch(typeof(DeckInfo), "AddCard")]
        [HarmonyPrefix]
        static bool Act2AddRandomizedCard(CardInfo card, CardCollectionInfo __instance, ref CardInfo __result)
        {
            if (!SaveManager.SaveFile.IsPart2)
            {
                return true;
            }
            CardInfo cardInfo = card.Clone() as CardInfo;
            RandomizeSigilsAct2(cardInfo);
            __instance.Cards.Add(cardInfo);
            __instance.cardIds.Add(card.name);
            __result = cardInfo;
            (__instance as DeckInfo)?.UpdateModDictionary();
            return false;
        }

        [HarmonyPatch(typeof(PackOpeningUI), "AssignInfoToCards")]
        [HarmonyPostfix]
        static void Act2RandomizeCardPackSigils(PackOpeningUI __instance)
        {
            for (var i = 0; i < __instance.cards.Count; i++)
            {
                var info = UnityEngine.Object.Instantiate(__instance.cards[i].Info);
                info.name = __instance.cards[i].Info.name;
                RandomizeSigilsAct2(info);
                __instance.cards[i].SetInfo(info);
            }
        }

        [HarmonyPatch(typeof(ShopUI), "UpdateInventory")]
        [HarmonyPrefix]
        static void Act2RandomizeShopSigils(List<CardInfo> inventory)
        {
            for (var i = 0; i < inventory.Count; i++)
            {
                var info = UnityEngine.Object.Instantiate(inventory[i]);
                info.name = inventory[i].name;
                RandomizeSigilsAct2(info);
                inventory[i] = info;
            }
        }

        [HarmonyPatch(typeof(SingleCardGainUI), "GainCard")]
        [HarmonyPrefix]
        static void Act2RandomizeSingleCardSigils(ref CardInfo card)
        {
            var info = UnityEngine.Object.Instantiate(card);
            info.name = card.name;
            RandomizeSigilsAct2(info);
            card = info;
        }
    }
}