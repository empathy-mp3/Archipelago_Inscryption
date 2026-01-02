
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Archipelago_Inscryption.Archipelago;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace Archipelago_Inscryption.Patches
{
    [HarmonyPatch]
    internal class CardPatches
    {
        public static void RandomizeSigils(CardInfo card)
        {
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
            RandomizeSigils(card, ref seed);
        }
        public static void RandomizeSigils(CardInfo card, ref int seed)
        {
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
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
            foreach (var choice in __result)
            {
                RandomizeSigils(choice.CardInfo, ref seed);
            }
        }
        [HarmonyPatch(typeof(TradePeltsSequencer), "GetTradeCardInfos")]
        [HarmonyPatch(typeof(DeckTrialSequencer), "GenerateRewardChoices")]
        [HarmonyPostfix]
        static void RandomizeDirectCardInfoSigils(ref List<CardInfo> __result)
        {
            int seed = SaveManager.SaveFile.GetCurrentRandomSeed();
            foreach (var card in __result)
            {
                RandomizeSigils(card, ref seed);
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
    }
}