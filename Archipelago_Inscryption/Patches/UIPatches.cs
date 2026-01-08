using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Helpers;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace Archipelago_Inscryption.Patches
{
    [HarmonyPatch]
    internal class UIPatches
    {
        /*[HarmonyPatch(typeof(TabbedUIPanel), "Awake")]
        [HarmonyPrefix]
        static bool AddArchipelagoOptionTab(TabbedUIPanel __instance)
        {
            
            if (!__instance.gameObject.name.Contains("Options")) return true;

            // Setup tab button

            Transform tab = __instance.transform.Find("MainPanel/Tabs/Tab_4");
            if (!tab) return true;

            __instance.tabButtons.Add(tab.GetComponent<GenericUIButton>());

            tab.gameObject.SetActive(true);

            SpriteRenderer tabIcon = tab.transform.Find("Icon").GetComponent<SpriteRenderer>();

            tabIcon.sprite = AssetsManager.archiSettingsTabSprite;

            // Setup tab content

            Transform tabGroup = __instance.transform.Find("MainPanel/TabGroup_");
            if (!tabGroup) return true;

            tabGroup.gameObject.name = "TabGroup_Archipelago";

            GameObject inputFieldPrefab = new GameObject("InputField_");
            inputFieldPrefab.transform.ResetTransform();

            BoxCollider2D collider = inputFieldPrefab.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.86f, 0.16f);

            GameObject labelPrefab = __instance.transform.Find("MainPanel/TabGroup_Gameplay/IncrementalSlider_TextSpeed/Title").gameObject;

            GameObject inputFieldLabel = Object.Instantiate(labelPrefab, inputFieldPrefab.transform);
            inputFieldLabel.name = "Title";
            inputFieldLabel.transform.localPosition = labelPrefab.transform.localPosition;

            GameObject fieldContentPrefab = __instance.transform.Find("MainPanel/TabGroup_Gameplay/IncrementalField_Language/TextFrame").gameObject;

            GameObject inputFieldContent = Object.Instantiate(fieldContentPrefab, inputFieldPrefab.transform);
            inputFieldContent.name = "TextFrame";
            inputFieldContent.transform.localPosition = fieldContentPrefab.transform.localPosition;

            inputFieldContent.GetComponent<SpriteRenderer>().sprite = AssetsManager.inputFieldSprite;

            Text inputFieldText = inputFieldContent.GetComponentInChildren<Text>(true);
            inputFieldText.rectTransform.offsetMin = new Vector2(-88, -25);
            inputFieldText.rectTransform.offsetMax = new Vector2(88, 25);
            inputFieldText.alignment = TextAnchor.MiddleLeft;

            inputFieldPrefab.AddComponent<Components.InputField>();

            Components.InputField hostNameField = UIHelper.CreateInputField(inputFieldPrefab, tabGroup, "InputField_HostName", "HOST NAME", "archipelago.gg", 0.74f, 100);
            Components.InputField portField = UIHelper.CreateInputField(inputFieldPrefab, tabGroup, "InputField_Port", "PORT", "", 0.4f, 5);
            Components.InputField slotNameField = UIHelper.CreateInputField(inputFieldPrefab, tabGroup, "InputField_SlotName", "SLOT NAME", "", 0.06f, 16);
            Components.InputField passwordField = UIHelper.CreateInputField(inputFieldPrefab, tabGroup, "InputField_Pass", "PASSWORD", "", -0.28f, 100, true);

            Object.Destroy(inputFieldPrefab);

            GameObject statusBox = Object.Instantiate(fieldContentPrefab, tabGroup);
            statusBox.transform.localPosition = new Vector3(0, -0.6f, 0);
            statusBox.GetComponent<SpriteRenderer>().enabled = false;

            GameObject buttonPrefab = __instance.transform.Find("MainPanel/TabGroup_Gameplay/Button_ApplyGraphics").gameObject;
            GameObject connectButton = Object.Instantiate(buttonPrefab, tabGroup.transform);
            connectButton.name = "Button_Connect";
            connectButton.GetComponentInChildren<Text>().text = "CONNECT";

            ArchipelagoOptionsMenu archipelagoMenu = tabGroup.gameObject.AddComponent<ArchipelagoOptionsMenu>();
            archipelagoMenu.SetFields(hostNameField, portField, slotNameField, passwordField, statusBox, connectButton.GetComponent<GenericUIButton>());

            return true;
        }*/

        [HarmonyPatch(typeof(StartScreenController), "Start")]
        [HarmonyPrefix]
        static bool CreateStatusAndLogsUIAndChapterMenuCards(StartScreenController __instance)
        {
            if (!ArchipelagoUI.exists)
            {
                GameObject uiObj = Object.Instantiate(AssetsManager.archipelagoUIPrefab);
                Object.DontDestroyOnLoad(uiObj);
                __instance.gameObject.SetActive(false);
                Singleton<ArchipelagoUI>.Instance.startScreen = __instance;

                return false;
            }

            var menu = MenuController.Instance;
            menu.cards.RemoveAt(2); // ascension mode (kaycee's mod), gets removed later anyway
            var index = 0;
            var inOrder = ArchipelagoData.Data.goalType == Goal.ActsInOrder;
            var locked = false;

            var startedAct1 = SaveManager.SaveFile.storyEvents.completedEvents.Contains(StoryEvent.BasicTutorialCompleted);
            var completedAct1 = ArchipelagoData.Data.act1Completed;
            var act1NewRun = menu.cards[index++];
            var act1 = menu.cards[index++];
            act1.titleSprite = null;
            act1.lockedTitleSprite = null;
            act1.titleLocId = "";

            if (ArchipelagoData.Data.enableAct1)
            {
                act1NewRun.name = "MenuCard_Act1NewRun";
                act1NewRun.titleText = startedAct1 ? "New Act 1 Run" : "Start Act 1";
                act1NewRun.GetComponent<SpriteRenderer>().sprite = AssetsManager.menuCardAct1NewRun;

                if (startedAct1)
                {
                    act1.name = "MenuCard_Act1";
                    act1.titleText = completedAct1 ? "Continue Act 1 (Complete!)" : "Continue Act 1";
                    act1.GetComponent<SpriteRenderer>().sprite = completedAct1 ? AssetsManager.menuCardAct1Complete : AssetsManager.menuCardAct1Continue;
                }
                else
                {
                    index--;
                    act1.SetEnabled(false);
                    act1.enabled = false;
                    menu.cards.Remove(act1);
                }

                if (inOrder && !completedAct1) locked = true;
            }
            else
            {
                index -= 2;
                act1NewRun.SetEnabled(false);
                act1NewRun.enabled = false;
                menu.cards.Remove(act1NewRun);
                act1.SetEnabled(false);
                act1.enabled = false;
                menu.cards.Remove(act1);
            }

            if (ArchipelagoData.Data.enableAct2)
            {
                var startedAct2 = SaveManager.SaveFile.storyEvents.completedEvents.Contains(StoryEvent.GBCIntroCompleted);
                var completedAct2 = ArchipelagoData.Data.act2Completed;
                var act2 = Object.Instantiate(act1, act1.transform.parent);
                menu.cards.Insert(index++, act2);
                act2.name = "MenuCard_Act2";
                act2.titleText = startedAct2 ? (completedAct2 ? "Continue Act 2 (Complete!)" : "Continue Act 2") : "Start Act 2";
                act2.GetComponent<SpriteRenderer>().sprite = startedAct2 ? (completedAct2 ? AssetsManager.menuCardAct2Complete : AssetsManager.menuCardAct2Continue) : AssetsManager.menuCardAct2Start;

                if (locked)
                {
                    act2.permanentlyLocked = true;
                    act2.titleText = "Locked";
                    act2.GetComponent<SpriteRenderer>().sprite = AssetsManager.menuCardAct2Locked;
                }
                if (inOrder && !completedAct2) locked = true;
            }

            if (ArchipelagoData.Data.enableAct2)
            {
                var startedAct3 = SaveManager.SaveFile.storyEvents.completedEvents.Contains(StoryEvent.Part3Intro);
                var completedAct3 = ArchipelagoData.Data.act3Completed;
                var act3 = Object.Instantiate(act1, act1.transform.parent);
                menu.cards.Insert(index++, act3);
                act3.name = "MenuCard_Act3";
                act3.titleText = startedAct3 ? (completedAct3 ? "Continue Act 3 (Complete!)" : "Continue Act 3") : "Start Act 3";
                act3.GetComponent<SpriteRenderer>().sprite = startedAct3 ? (completedAct3 ? AssetsManager.menuCardAct3Complete : AssetsManager.menuCardAct3Continue) : AssetsManager.menuCardAct3Start;

                if (locked)
                {
                    act3.permanentlyLocked = true;
                    act3.titleText = "Locked";
                    act3.GetComponent<SpriteRenderer>().sprite = AssetsManager.menuCardAct3Locked;
                }
            }

            if (!ArchipelagoData.Data.skipEpilogue
            && (!ArchipelagoData.Data.enableAct1 || ArchipelagoData.Data.act1Completed)
            && (!ArchipelagoData.Data.enableAct2 || ArchipelagoData.Data.act2Completed)
            && (!ArchipelagoData.Data.enableAct3 || ArchipelagoData.Data.act3Completed))
            {
                var act4 = Object.Instantiate(act1, act1.transform.parent);
                menu.cards.Insert(index++, act4);
                act4.name = "MenuCard_Act4";
                act4.titleText = "Play Epilogue";
                act4.GetComponent<SpriteRenderer>().sprite = AssetsManager.menuCardAct4;
            }

            var cardSpacingX = 0.458f;
            var leftAnchorX = -cardSpacingX * (menu.cards.Count - 1) / 2f; // -1 because it's from the centers
            for (int i = 0; i < menu.cards.Count; i++)
            {
                menu.cards[i].transform.localPosition = new Vector2(leftAnchorX + (float)i * cardSpacingX, menu.cards[i].transform.localPosition.y);
                menu.cards[i].ReInitPosition(menu.cards[i].transform.localPosition);
            }
            return true;
        }

        [HarmonyPatch(typeof(MenuController), "LoadGameFromMenu")]
        [HarmonyPrefix]
        public static bool LoadSelectedActFromMenu()
        {
            switch (Singleton<MenuController>.Instance.slottedCard.name)
            {
                case "MenuCard_Act1NewRun":
                    UIHelper.LoadSelectedChapter(1, true);
                    break;
                case "MenuCard_Act1":
                    UIHelper.LoadSelectedChapter(1, false);
                    break;
                case "MenuCard_Act2":
                    UIHelper.LoadSelectedChapter(2);
                    break;
                case "MenuCard_Act3":
                    UIHelper.LoadSelectedChapter(3);
                    break;
                case "MenuCard_Act4":
                    UIHelper.LoadSelectedChapter(4);
                    break;
            }
            return false;
        }

        [HarmonyPatch(typeof(GenericUIButton), "UpdateInputButton")]
        [HarmonyPrefix]
        static bool PreventButtonUpdateIfInputFieldSelected()
        {
            return !Components.InputField.IsAnySelected;
        }

        [HarmonyPatch(typeof(GenericUIButton), "UpdateInputKey")]
        [HarmonyPrefix]
        static bool PreventKeyUpdateIfInputFieldSelected()
        {
            return !Components.InputField.IsAnySelected;
        }

        [HarmonyPatch(typeof(DeckBuildingUI), "Start")]
        [HarmonyPostfix]
        static void CreateCardPackButton(DeckBuildingUI __instance)
        {
            GameObject newButtonObject = Object.Instantiate(__instance.autoCompleteButton.gameObject);
            newButtonObject.name = "OpenCardPackButton";
            newButtonObject.transform.SetParent(__instance.transform);
            newButtonObject.transform.localPosition = new Vector3(-1.66f, 0.66f, 0f);
            newButtonObject.GetComponent<BoxCollider2D>().size = new Vector2(0.49f, 0.68f);

            GenericUIButton newButton = newButtonObject.GetComponent<GenericUIButton>();
            newButton.defaultSprite = AssetsManager.packButtonSprites[0];
            newButton.hoveringSprite = AssetsManager.packButtonSprites[1];
            newButton.downSprite = AssetsManager.packButtonSprites[2];
            newButton.disabledSprite = AssetsManager.packButtonSprites[3];

            newButtonObject.GetComponent<SpriteRenderer>().sprite = newButton.defaultSprite;

            Object.Destroy(newButtonObject.transform.GetChild(0).gameObject);

            newButton.CursorSelectEnded = (x => CustomCoroutine.instance.StartCoroutine(RandomizerHelper.OnPackButtonPressed()));

            RandomizerHelper.packButton = newButton;
            RandomizerHelper.UpdatePackButtonEnabled();
        }

        [HarmonyPatch(typeof(MenuController), "OnCardReachedSlot")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceAct1NewRunPrompt(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(x => x.Calls(AccessTools.Method(typeof(StoryEventsData), "EventCompleted")));
            codes[index - 1].operand = (int)StoryEvent.BasicTutorialCompleted;

            index = codes.FindIndex(x => x.Is(OpCodes.Ldstr, "Erase save data and start New Game?"));
            codes[index].operand = "Start new Act 1 run?";

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(MenuCard), "Awake")]
        [HarmonyPrefix]
        static bool ReplaceNewGameText(MenuCard __instance)
        {
            if (__instance.MenuAction == MenuAction.NewGame)
            {
                __instance.titleSprite = null;
                __instance.lockedTitleSprite = null;
                __instance.titleLocId = "";
                __instance.titleText = "CHAPTER SELECT";
                __instance.lockAfterStoryEvent = false;
                if (ArchipelagoOptions.goal == Goal.ActsAnyOrder || !ArchipelagoOptions.enableAct1)
                    __instance.lockBeforeStoryEvent = false;
            }

            return true;
        }

        [HarmonyPatch(typeof(StartScreenThemeSetter), "Start")]
        [HarmonyPostfix]
        static void ChangeActThemeOnStart(StartScreenThemeSetter __instance)
        {
            if (SaveManager.SaveFile.currentScene.Contains("Part1"))
            {
                __instance.SetTheme(__instance.themes[0]);
            }
            else if (SaveManager.SaveFile.currentScene.Contains("GBC"))
            {
                __instance.SetTheme(__instance.themes[1]);
            }
            else if (SaveManager.SaveFile.currentScene.Contains("Part3"))
            {
                __instance.SetTheme(__instance.themes[2]);
            }
        }

        [HarmonyPatch(typeof(StartScreenController), "Start")]
        [HarmonyPrefix]
        static void ChangeActOnInitialize(StartScreenController __instance)
        {   
            if (!ArchipelagoOptions.enableAct1 && SaveManager.SaveFile.currentScene.Contains("Part1") && 
                !StoryEventsData.EventCompleted(StoryEvent.FinaleCryptCompleted))
            {
                if (ArchipelagoOptions.enableAct2)
                    SaveManager.SaveFile.currentScene = "GBC_Intro";
                else if (ArchipelagoOptions.enableAct3)
                    SaveManager.SaveFile.currentScene = "Part3_Cabin";
            }
        }   
    }
}
