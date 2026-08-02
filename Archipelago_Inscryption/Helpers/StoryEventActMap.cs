using DiskCardGame;

using System;

using System.Collections.Generic;

using System.Linq;



namespace Archipelago_Inscryption.Helpers

{

    internal enum ActScope { None, Act1, Act2, Act3, Epilogue }



    // Story events carry no act information, and vanilla groups them only once (GBCStoryEvents,

    // which is incomplete). Resetting a single act needs that attribution, so it lives here.

    //

    // No entry is based on an event's name. The comment on each line is its evidence: the

    // scene or prefab whose StoryEvent field references it, or the class that references it,

    // where that class's act comes from its namespace, its base type, or the act scenes that

    // instantiate it. Regenerate the bulk with Inscryption-decompiled/tools/story_event_scan.py

    // against an AssetRipper export.

    //

    // Events absent here resolve to None and survive act resets. That is the safe direction: an

    // act resuming too far along is visible and fixable, erasing another act's progress is not.

    // The absentees are menu, epilogue, Kaycee's Mod and unreferenced events, plus two keys that

    // vanilla marks perma-saved and Archipelago grants as items.

    internal static class StoryEventActMap

    {

        private static readonly Dictionary<StoryEvent, ActScope> map = new Dictionary<StoryEvent, ActScope>
        {
            { StoryEvent.AnglerDefeated, ActScope.Act1 },   // Wetlands.asset
            { StoryEvent.AntCardsDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.AscensionBleachPotFound, ActScope.Act1 },   // HoldableBleachPot.cs (only in Act 1 scenes)
            { StoryEvent.AscensionMagnifyingGlassFound, ActScope.Act1 },   // HoldableMagnifyingGlass.cs (only in Act 1 scenes)
            { StoryEvent.AscensionPirateBossDefeated, ActScope.Act1 },   // base type Part1BossBattleSequencer
            { StoryEvent.AscensionStopwatchFound, ActScope.Act1 },   // HoldablePocketWatch.cs (only in Act 1 scenes)
            { StoryEvent.AscensionVinylFound, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.BasicTutorialCompleted, ActScope.Act1 },   // RunIntroSequencer.cs (only in Act 1 scenes)
            { StoryEvent.BeeFigurineFound, ActScope.Act1 },   // HoldableBeeFigurine.cs (only in Act 1 scenes)
            { StoryEvent.BonesTutorialCompleted, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.CabinTarotCardFound, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.CageCardDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.CandleArmFound, ActScope.Act1 },   // HoldableCandleArm.cs (only in Act 1 scenes)
            { StoryEvent.ClockCompartmentOpened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.ClockSmallCompartmentOpened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.CloverFound, ActScope.Act1 },   // HoldableCloverPot.cs (only in Act 1 scenes)
            { StoryEvent.FMVClips1, ActScope.Act1 },   // VictoryFeastSequencer.cs (only in Act 1 scenes)
            { StoryEvent.FailedWithFilmRoll, ActScope.Act1 },   // set inside SaveFile.NewPart1Run()
            { StoryEvent.FigurineFetched, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.FilmRollDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.FishHookUnlocked, ActScope.Act1 },   // RunIntroSequencer.cs (only in Act 1 scenes)
            { StoryEvent.GooBottleFound, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.ImprovedSmokeCardDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.LeshyDefeated, ActScope.Act1 },   // base type Part1BossOpponent
            { StoryEvent.LeshyLostCamera, ActScope.Act1 },   // ChooseEyeballSequencer.cs (only in Act 1 scenes)
            { StoryEvent.LukeVOBeatLeshyAgain, ActScope.Act1 },   // VictoryFeastSequencer.cs (only in Act 1 scenes)
            { StoryEvent.LukeVODieAlready, ActScope.Act1 },   // base type Part1BossOpponent
            { StoryEvent.LukeVOLeshyRematch, ActScope.Act1 },   // base type Part1BossOpponent
            { StoryEvent.LukeVOMantisGod, ActScope.Act1 },   // data/cards/nature/MantisGod.asset
            { StoryEvent.LukeVONewRunAfterVictory, ActScope.Act1 },   // RunIntroSequencer.cs (only in Act 1 scenes)
            { StoryEvent.LukeVOPart1Vision, ActScope.Act1 },   // RunIntroSequencer.cs (only in Act 1 scenes)
            { StoryEvent.LukeVOSickOfBoss, ActScope.Act1 },   // base type Part1BossOpponent
            { StoryEvent.PhotoDroneSeenInCabin, ActScope.Act1 },   // PhotographerDroneEvent.cs (only in Act 1 scenes)
            { StoryEvent.ProspectorDefeated, ActScope.Act1 },   // CurrencyBowl.cs (only in Act 1 scenes)
            { StoryEvent.RingFound, ActScope.Act1 },   // FinaleDeckTrialSequencer.cs (only in Act 1 scenes)
            { StoryEvent.SacrificedStoatInTutorial, ActScope.Act1 },   // StoatTalkingCardTutorial : StoatTalkingCard : PaperTalkingCard
            { StoryEvent.SafeOpened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.SkinkCardDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.SpecialDaggerDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.SpecialDaggerUsed, ActScope.Act1 },   // GooBottleDialogueInteractable.cs (only in Act 1 scenes)
            { StoryEvent.SquirrelHeadDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.StartScreenNewGameUnlocked, ActScope.Act1 },   // SanctumSceneSequencer.cs (only in Act 1 scenes)
            { StoryEvent.StinkbugCardDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.StinkbugIntroduction2, ActScope.Act1 },   // StinkbugTalkingCard : PaperTalkingCard
            { StoryEvent.StinkbugMentionedWolf, ActScope.Act1 },   // StinkbugTalkingCard : PaperTalkingCard
            { StoryEvent.StinkbugStoatReunited, ActScope.Act1 },   // StinkbugTalkingCard : PaperTalkingCard
            { StoryEvent.StoatIntroduction, ActScope.Act1 },   // StoatTalkingCard : PaperTalkingCard
            { StoryEvent.StoatIntroduction2, ActScope.Act1 },   // StoatTalkingCard : PaperTalkingCard
            { StoryEvent.StoatIntroduction3, ActScope.Act1 },   // StoatTalkingCard : PaperTalkingCard
            { StoryEvent.StoatSaysFindWolf, ActScope.Act1 },   // StoatTalkingCard : PaperTalkingCard
            { StoryEvent.StoatWolfReunited, ActScope.Act1 },   // WolfTalkingCard : PaperTalkingCard
            { StoryEvent.TalkingWolfCardDiscovered, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.TrapperTraderDefeated, ActScope.Act1 },   // BuyPeltsSequencer.cs (only in Act 1 scenes)
            { StoryEvent.TutorialRun2Completed, ActScope.Act1 },   // GainConsumablesSequencer.cs (only in Act 1 scenes)
            { StoryEvent.TutorialRun3Completed, ActScope.Act1 },   // Tutorial3GameFlowSequencer drives RunState.Run, the Act 1 run
            { StoryEvent.TutorialRunCompleted, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.UhOhSpaghettiOh, ActScope.Act1 },   // base type Part1GameFlowManager
            { StoryEvent.WardrobeDrawer1Opened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WardrobeDrawer2Opened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WardrobeDrawer3Opened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WardrobeDrawer4Opened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WardrobePanelOpened, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WolfCageBroken, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WolfMentionFilmRoll, ActScope.Act1 },   // WolfTalkingCard : PaperTalkingCard
            { StoryEvent.WolfStatuePlaced, ActScope.Act1 },   // Part1_Cabin.unity
            { StoryEvent.WoodcarverDefeated, ActScope.Act1 },   // WoodcarverBossOpponent : Part1BossOpponent
            { StoryEvent.WoodcarverMet, ActScope.Act1 },   // BuildTotemSequencer.cs (only in Act 1 scenes)

            { StoryEvent.BonelordHoloKeyFound, ActScope.Act2 },   // GBC_Temple_Undead.unity (storyEventToUnlock); Act 3 only requires it
            { StoryEvent.FMVClips2, ActScope.Act2 },   // GBCSpecialEventSequencer.cs (GBC namespace)
            { StoryEvent.GBCAnglerPhoto, ActScope.Act2 },   // GBC_Temple_Nature.unity
            { StoryEvent.GBCBaitPhoto, ActScope.Act2 },   // GBC_Temple_Nature.unity
            { StoryEvent.GBCBoneFound, ActScope.Act2 },   // GBC_Temple_Undead.unity
            { StoryEvent.GBCBonelordRewardsGiven, ActScope.Act2 },   // GBC_Broken_Bridge.unity
            { StoryEvent.GBCCameraBatteryLow, ActScope.Act2 },   // GBCSpecialEventSequencer.cs (GBC namespace)
            { StoryEvent.GBCCloverFound, ActScope.Act2 },   // GBC_Docks.unity
            { StoryEvent.GBCDogFoodFound, ActScope.Act2 },   // DogFoodBowlVolume.cs (GBC namespace)
            { StoryEvent.GBCDogFoodPlaced, ActScope.Act2 },   // DogFoodBowlVolume.cs (GBC namespace)
            { StoryEvent.GBCGrimoraDefeated, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCIntroCompleted, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.GBCLeshyDefeated, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCMagnificusDefeated, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCMonocleFound, ActScope.Act2 },   // GBC_Broken_Bridge.unity
            { StoryEvent.GBCNatureAmbition, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCNatureFinaleChosen, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.GBCObolFound, ActScope.Act2 },   // BonelordCasketEvent.cs (GBC namespace)
            { StoryEvent.GBCObolGiven, ActScope.Act2 },   // BonelordCasketEvent.cs (GBC namespace)
            { StoryEvent.GBCPoeDefeated, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCProspectorPhoto, ActScope.Act2 },   // GBC_Temple_Nature.unity (photoEvent)
            { StoryEvent.GBCTechAmbition, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCTechFinaleChosen, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.GBCTrapperPhoto, ActScope.Act2 },   // GBC_Docks.unity
            { StoryEvent.GBCUndeadAmbition, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCUndeadFinaleChosen, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.GBCWizardAmbition, ActScope.Act2 },   // GBC_Starting_Island.unity
            { StoryEvent.GBCWizardFinaleChosen, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.GBCWorldMapVO, ActScope.Act2 },   // TriggerWorldMapVoiceOver.cs (GBC namespace)
            { StoryEvent.LukeVOPart2Bonelord, ActScope.Act2 },   // BonelordNPC.cs (GBC namespace)
            { StoryEvent.LukeVOPart2Grimora, ActScope.Act2 },   // GrimoraBossOpponent.cs (GBC namespace)
            { StoryEvent.MycologistHutKeyFound, ActScope.Act2 },   // GBC_Mycologist_Hut.unity (storyEventToUnlock); Act 3 only requires it
            { StoryEvent.MycologistHutKeyShown, ActScope.Act2 },   // GBC_Mycologist_Hut.unity
            { StoryEvent.Part2Completed, ActScope.Act2 },   // StartingIslandFinaleSequencer.cs (GBC namespace)
            { StoryEvent.StartScreenNewGameUsed, ActScope.Act2 },   // GBCIntroScene.cs (GBC namespace)

            { StoryEvent.ArchivistDefeated, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.BombRemoteDiscovered, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CanvasDefeated, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle1Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle2Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle3Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle4Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle5Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.CaptchaPuzzle6Complete, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.DredgingRoomUnlocked, ActScope.Act3 },   // HoloGameMap.cs (only in Act 3 scenes)
            { StoryEvent.FMVClips3, ActScope.Act3 },   // VideoCamClipsAreaSequencer.cs (only in Act 3 scenes)
            { StoryEvent.FactoryChestOpened2, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryConveyorBeltMoved, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryCuckooClockAppeared, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryCuckooClockOpenedLarge, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryCuckooClockOpenedSmall, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryGemPedestalAppeared, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryGooSpotted, ActScope.Act3 },   // GooPipeVisualEvent.cs (only in Act 3 scenes)
            { StoryEvent.FactoryPrinterScreenMatched, ActScope.Act3 },   // FactoryScannerScreen.cs (only in Act 3 scenes)
            { StoryEvent.FactoryShopRoomUnlocked, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryWardrobe1Opened, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryWardrobe2Opened, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryWoodcarver1, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryWoodcarver2, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FactoryWoodcarver3, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.FileAccessGiven, ActScope.Act3 },   // HoloMapArea_NatureSidePath.prefab
            { StoryEvent.GemsModuleFetched, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.GemsModuleRequested, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.GooPlaneGoobertComplete, ActScope.Act3 },   // GooPipeDialogueInteractable.cs (only in Act 3 scenes)
            { StoryEvent.GooPlaneGoobertRevealed, ActScope.Act3 },   // GooPipeDialogueInteractable.cs (only in Act 3 scenes)
            { StoryEvent.HandCuffReleased, ActScope.Act3 },   // base type Part3GameFlowManager
            { StoryEvent.HoloMapBatteryFetched, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.HoloMapOutOfPower, ActScope.Act3 },   // HoloGameMap.cs (only in Act 3 scenes)
            { StoryEvent.HoloTechAreaUnlocked, ActScope.Act3 },   // HoloMapArea_NeutralWestTechGate.prefab
            { StoryEvent.HoloTechTempleSatelliteActivated, ActScope.Act3 },   // HoloMapArea_TempleTech_1.prefab
            { StoryEvent.InternetAccessGranted, ActScope.Act3 },   // HoloMapArea_NatureMainPath_5.prefab
            { StoryEvent.LukeVOOPCard, ActScope.Act3 },   // BuildACardSequencer.cs (only in Act 3 scenes)
            { StoryEvent.LukeVOPart3CloseWin, ActScope.Act3 },   // DamageRaceBattleSequencer -> Part3CloseWin
            { StoryEvent.LukeVOPart3File, ActScope.Act3 },   // HoloMapLukeFile.cs (only in Act 3 scenes)
            { StoryEvent.LukeVOPart3Shit, ActScope.Act3 },   // base type Part3GameFlowManager
            { StoryEvent.LukeVOPart3Yes, ActScope.Act3 },   // data/abilities/part3/DrawRandomCardOnDeath.asset
            { StoryEvent.MycologistsBossDefeated, ActScope.Act3 },   // MycologistsBossOpponent : Part3BossOpponent
            { StoryEvent.MycologistsDefeated, ActScope.Act3 },   // HoloMapArea_Mycologists_2.prefab
            { StoryEvent.NatureHoloShortcut, ActScope.Act3 },   // HoloMapArea_NatureEntrance.prefab
            { StoryEvent.OurobotCardDiscovered, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.Part3BonelordRoomUnlocked, ActScope.Act3 },   // HoloMapArea_TempleUndeadRight_2.prefab
            { StoryEvent.Part3Completed, ActScope.Act3 },   // Part3FinaleAreaSequencer.cs
            { StoryEvent.Part3Intro, ActScope.Act3 },   // base type Part3GameFlowManager
            { StoryEvent.Part3MetBonelord, ActScope.Act3 },   // BonelordAreaSequencer.cs (only in Act 3 scenes)
            { StoryEvent.Part3MetScrybes, ActScope.Act3 },   // FactoryLift.cs (only in Act 3 scenes)
            { StoryEvent.Part3MycologistHutUnlocked, ActScope.Act3 },   // HoloMapArea_Mycologists_1.prefab
            { StoryEvent.Part3PhotoDroneActive, ActScope.Act3 },   // PhotoDronePuzzle.cs (only in Act 3 scenes)
            { StoryEvent.Part3PostScrybeMeeting, ActScope.Act3 },   // HoloGameMap.cs (only in Act 3 scenes)
            { StoryEvent.Part3PurchasedHoloBrush, ActScope.Act3 },   // HoloMapArea_TempleWizardSide.prefab
            { StoryEvent.PhotographerDefeated, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.PlayerDeletedArchivistFile, ActScope.Act3 },   // HoloGameMap.cs (only in Act 3 scenes)
            { StoryEvent.TalkingAnglerCardDiscovered, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.TalkingBlueMageCardDiscovered, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.TelegrapherDefeated, ActScope.Act3 },   // Part3_Cabin.unity
            { StoryEvent.UndeadHoloShortcut, ActScope.Act3 },   // HoloMapArea_UndeadMainPath_2.prefab
            { StoryEvent.WizardHoloShortcut, ActScope.Act3 },   // HoloMapArea_WizardMainPath_5.prefab
            // The epilogue, which StartScreenController reads as a completion ladder after Part3Completed.
            { StoryEvent.FinaleCryptCompleted, ActScope.Epilogue },   // GrimoraGameFlowManager -> finale_grimora.unity
            { StoryEvent.FinaleCabinCompleted, ActScope.Epilogue },   // LeshyGoodbyeBattleSequencer; read beside its two siblings
            { StoryEvent.FinaleMagnificusCompleted, ActScope.Epilogue },   // MagnificusGameFlowManager -> finale_magnificus.unity
            { StoryEvent.GrimoraReachedTable, ActScope.Epilogue },   // ChessboardMap -> finale_grimora.unity
        };

        internal static ActScope ScopeOf(StoryEvent storyEvent)

            => map.TryGetValue(storyEvent, out ActScope scope) ? scope : ActScope.None;



        internal static IEnumerable<StoryEvent> EventsForAct(int act)

        {

            ActScope scope = act == 1 ? ActScope.Act1 : act == 2 ? ActScope.Act2 : ActScope.Act3;

            return map.Where(pair => pair.Value == scope).Select(pair => pair.Key);

        }



        // Events added by a future game version would default to None and silently survive

        // resets, so name them once at startup instead of letting that go unnoticed.

        internal static void WarnAboutUnmappedEvents()

        {

            List<string> unmapped = Enum.GetValues(typeof(StoryEvent)).Cast<StoryEvent>()

                .Where(e => e != StoryEvent.NUM_EVENTS && !map.ContainsKey(e))

                .Select(e => e.ToString()).ToList();



            if (unmapped.Count > 0)

                ArchipelagoModPlugin.Log.LogInfo($"StoryEvents with no act ({unmapped.Count}): {string.Join(", ", unmapped)}");

        }

    }

}

