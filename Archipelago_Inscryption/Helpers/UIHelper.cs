using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using GBC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Archipelago_Inscryption.Helpers
{
    internal static class UIHelper
    {
        private static GameObject inputFieldPrefab;
        private static void CreateInputFieldTemplate(OptionsUI menu)
        {
            inputFieldPrefab = new GameObject("InputField_");
            inputFieldPrefab.transform.ResetTransform();

            BoxCollider2D collider = inputFieldPrefab.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.86f, 0.16f);

            GameObject labelPrefab = menu.transform.Find("MainPanel/TabGroup_Gameplay/IncrementalSlider_TextSpeed/Title").gameObject;

            GameObject inputFieldLabel = Object.Instantiate(labelPrefab, inputFieldPrefab.transform);
            inputFieldLabel.name = "Title";
            inputFieldLabel.transform.localPosition = labelPrefab.transform.localPosition;

            GameObject fieldContentPrefab = menu.transform.Find("MainPanel/TabGroup_Gameplay/IncrementalField_Language/TextFrame").gameObject;

            GameObject inputFieldContent = Object.Instantiate(fieldContentPrefab, inputFieldPrefab.transform);
            inputFieldContent.name = "TextFrame";
            inputFieldContent.transform.localPosition = fieldContentPrefab.transform.localPosition;

            inputFieldContent.GetComponent<SpriteRenderer>().sprite = AssetsManager.inputFieldSprite;

            Text inputFieldText = inputFieldContent.GetComponentInChildren<Text>(true);
            inputFieldText.rectTransform.offsetMin = new Vector2(-88, -25);
            inputFieldText.rectTransform.offsetMax = new Vector2(88, 25);
            inputFieldText.alignment = TextAnchor.MiddleLeft;

            inputFieldPrefab.AddComponent<Components.InputField>();
            inputFieldPrefab.SetActive(false);
        }
        internal static Components.InputField CreateInputField(OptionsUI menu, Transform parent, string name, string label, string defaultContent, float yPosition, int characterLimit, bool censor = false)
        {
            if (inputFieldPrefab == null)
            {
                CreateInputFieldTemplate(menu);
            }
            GameObject inputFieldInstance = Object.Instantiate(inputFieldPrefab);
            inputFieldInstance.SetActive(true);
            inputFieldInstance.transform.SetParent(parent);
            inputFieldInstance.transform.ResetTransform();
            inputFieldInstance.transform.localPosition = new Vector3(0, yPosition, 0);
            inputFieldInstance.name = name;
            Components.InputField inputField = inputFieldInstance.GetComponent<Components.InputField>();
            inputField.Label = label;
            inputField.Text = defaultContent;
            inputField.CharacterLimit = characterLimit;
            inputField.Censor = censor;

            return inputField;
        }

        // Act cards are built once on start screen load, so a reset leaves them reading "Continue".
        // Only label and art change here; locked state depends on things a reset does not touch.
        internal static void RefreshActCards()
        {
            MenuController menu = MenuController.Instance;
            if (menu == null || menu.cards == null) return;

            foreach (MenuCard card in menu.cards)
            {
                if (card == null || card.permanentlyLocked) continue;

                switch (card.name)
                {
                    case "MenuCard_Act1":
                        SetActCardVisuals(card, 1, StoryEvent.BasicTutorialCompleted, ArchipelagoData.Data.act1Completed,
                            AssetsManager.menuCardAct1NewRun, AssetsManager.menuCardAct1Continue, AssetsManager.menuCardAct1Complete);
                        break;
                    case "MenuCard_Act2":
                        SetActCardVisuals(card, 2, StoryEvent.GBCIntroCompleted, ArchipelagoData.Data.act2Completed,
                            AssetsManager.menuCardAct2Start, AssetsManager.menuCardAct2Continue, AssetsManager.menuCardAct2Complete);
                        break;
                    case "MenuCard_Act3":
                        SetActCardVisuals(card, 3, StoryEvent.Part3Intro, ArchipelagoData.Data.act3Completed,
                            AssetsManager.menuCardAct3Start, AssetsManager.menuCardAct3Continue, AssetsManager.menuCardAct3Complete);
                        break;
                }
            }
        }

        private static void SetActCardVisuals(MenuCard card, int act, StoryEvent startedEvent, bool completed,
            Sprite notStarted, Sprite inProgress, Sprite complete)
        {
            bool started = SaveManager.SaveFile.storyEvents.completedEvents.Contains(startedEvent)
                && !(act == 1 && ArchipelagoData.Data.act1RunFresh);

            card.titleText = started
                ? (completed ? $"Continue Act {act} (Complete!)" : $"Continue Act {act}")
                : $"Start Act {act}";
            card.GetComponent<SpriteRenderer>().sprite = started ? (completed ? complete : inProgress) : notStarted;
        }

        internal static void LoadSelectedChapter(int chapter, bool act1NewRun = true)
        {
            SaveManager.LoadFromFile();

            if (FinaleDeletionWindowManager.instance != null)
            {
                GameObject.Destroy(FinaleDeletionWindowManager.instance.gameObject);
            }

            StoryEventsData.EraseEvent(StoryEvent.FullGameCompleted);
            if (chapter != 4)
                StoryEventsData.EraseEvent(StoryEvent.Part3Completed);

            switch (chapter)
            {
                case 1:
                    ScriptableObjectLoader<CardInfo>.AllData.Find(x => x.name == "Hrokkall").temple = CardTemple.Tech;
                    SaveManager.SaveFile.currentScene = "Part1_Cabin";
                    ArchipelagoData.Data.act1RunFresh = false;
                    if (act1NewRun)
                    {
                        SaveManager.SaveFile.NewPart1Run();
                    }
                    break;
                case 2:
                    ScriptableObjectLoader<CardInfo>.AllData.Find(x => x.name == "Hrokkall").temple = CardTemple.Nature;
                    StoryEventsData.EraseEvent(StoryEvent.GBCUndeadFinaleChosen);
                    StoryEventsData.EraseEvent(StoryEvent.GBCNatureFinaleChosen);
                    StoryEventsData.EraseEvent(StoryEvent.GBCTechFinaleChosen);
                    StoryEventsData.EraseEvent(StoryEvent.GBCWizardFinaleChosen);
                    if (StoryEventsData.EventCompleted(StoryEvent.StartScreenNewGameUsed))
                    {
                        // Reset temple rooms to default entrance
                        SaveData.Data.natureTemple.roomId = "OutdoorsCentral";
                        SaveData.Data.natureTemple.cameraPosition = Vector2.zero;
                        SaveData.Data.undeadTemple.roomId = "MainRoom";
                        SaveData.Data.undeadTemple.cameraPosition = Vector2.zero;
                        SaveData.Data.techTemple.roomId = "--- MainRoom ---";
                        SaveData.Data.techTemple.cameraPosition = Vector2.zero;
                        SaveData.Data.wizardTemple.roomId = "Floor_1";
                        SaveData.Data.wizardTemple.cameraPosition = Vector2.zero;

                        SaveManager.SaveFile.currentScene = StoryEventsData.EventCompleted(StoryEvent.GBCIntroCompleted) ? "GBC_WorldMap" : "GBC_Starting_Island";
                        if (ArchipelagoOptions.act2RandomizeBridge == Act2RandomizeBridge.LeftSideStart)
                            SaveData.Data.overworldNode = "TechElevator";
                        else
                            SaveData.Data.overworldNode = "StartingIsland";
                    }
                    else
                    {
                        SaveManager.SaveFile.currentScene = "GBC_Intro";
                    }
                    break;
                case 3:
                    if (StoryEventsData.EventCompleted(StoryEvent.ArchivistDefeated) &&
                        StoryEventsData.EventCompleted(StoryEvent.PhotographerDefeated) && 
                        StoryEventsData.EventCompleted(StoryEvent.TelegrapherDefeated) && 
                        StoryEventsData.EventCompleted(StoryEvent.CanvasDefeated) &&
                        Part3SaveData.Data.playerPos.worldId == "StartingArea" || 
                        Part3SaveData.Data.playerPos.worldId == "!FINALE_CHAPTER_SELECT")
                        Part3SaveData.Data.playerPos = new Part3SaveData.WorldPosition("NorthNeutralPath", 2, 1);
                    SaveManager.SaveFile.currentScene = "Part3_Cabin";
                    break;
                case 4:
                    SaveManager.SaveFile.currentScene = "Part3_Cabin";
                    SaveManager.SaveFile.part3Data.playerPos = new Part3SaveData.WorldPosition("!FINALE_CHAPTER_SELECT", 2, 1);
                    break;
                default:
                    break;
            }

            LoadingScreenManager.LoadScene(SaveManager.SaveFile.currentScene);
        }
    }
}
