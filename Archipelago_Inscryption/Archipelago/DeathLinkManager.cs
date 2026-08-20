using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Helpers;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using GBC;
using System.Collections;
using UnityEngine;

namespace Archipelago_Inscryption.Archipelago
{
    internal static class DeathLinkManager
    {
        public static DeathLinkService DeathLinkService;
        internal static bool receivedDeath;
        private static bool queuedDeathLink;

        // Deaths that landed while one was still waiting on dialogue, a node or a transition. They are
        // spent together by one application; deaths arriving during that application are amnestied.
        private static bool applyingDeath;
        private static int pendingDeaths;

        // Bumped whenever the context a death belongs to goes away. ApplyDeathLink carries the value it
        // started with, so a coroutine that outlives its act cancels itself instead of firing on re-entry.
        private static int deathGeneration;
        private static int lastAct;

        // The start screen and its chapter select read as Act 1, since IsPart1 is defined as none of
        // the other acts, so the scene is what says whether there is an act on screen to die in.
        private static int ActOnScreen()
        {
            string scene = SceneLoader.ActiveSceneName.ToLowerInvariant();
            if (scene.Contains("part1")) return 1;
            if (scene.Contains("gbc")) return 2;
            if (scene.Contains("part3")) return 3;
            return 0;
        }

        internal static void Init()
        {
            ArchipelagoModPlugin.Log.LogInfo($"DeathLink is set to {ArchipelagoOptions.deathlink}");
            DeathLinkService.OnDeathLinkReceived += ReceiveDeathLink;
            if (ArchipelagoData.DeathLink)
                DeathLinkService.EnableDeathLink();
            else
                DeathLinkService.DisableDeathLink();
        }

        static void ReceiveDeathLink(DeathLink deathLink)
        {
            if (applyingDeath || StoryEventsData.EventCompleted(StoryEvent.Part3Completed))
                return;
            if (ActOnScreen() == 0)
            {
                ArchipelagoModPlugin.Log.LogInfo($"Ignored DeathLink from {deathLink.Source}: no act in progress");
                return;
            }
            pendingDeaths++;
            string message = $"Received DeathLink from {deathLink.Source}: {deathLink.Cause}";
            if (pendingDeaths > 1)
                message += $" ({pendingDeaths} waiting to be applied)";
            ArchipelagoModPlugin.Log.LogMessage(message);
            Singleton<ArchipelagoUI>.Instance.LogMessage(message);
            if (receivedDeath)
                return;
            receivedDeath = true;
            queuedDeathLink = true;
        }

        static IEnumerator ApplyDeathLink()
        {
            int generation = deathGeneration;

            if (Singleton<TextDisplayer>.Instance != null && Singleton<TextDisplayer>.Instance.PlayingEvent)
                yield return new WaitUntil(() => !Singleton<TextDisplayer>.Instance.PlayingEvent);

            if (Singleton<MapNodeManager>.Instance != null && Singleton<MapNodeManager>.Instance.MovingNodes)
                yield return new WaitUntil(() => !Singleton<MapNodeManager>.Instance.MovingNodes);

            if (Singleton<InteractionCursor>.Instance != null && Singleton<InteractionCursor>.Instance.InteractionDisabled == true)
                yield return new WaitUntil(() => !Singleton<InteractionCursor>.Instance.InteractionDisabled);

            // A transition only assigns CurrentGameState at its end, and a special node runs its choice on
            // the same sequencer this death uses, so let both settle before deciding what a death means.
            while (Singleton<GameFlowManager>.Instance != null && (Singleton<GameFlowManager>.Instance.Transitioning
                || Singleton<GameFlowManager>.Instance.CurrentGameState == GameState.SpecialCardSequence))
                yield return null;

            if (Singleton<FirstPersonController>.Instance != null && 
                Singleton<GameFlowManager>.Instance.CurrentGameState == GameState.FirstPerson3D &&
                (SaveManager.SaveFile.IsPart3 || ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.Sacrificed || RunState.Run.playerLives <= 1))
                yield return Singleton<GameFlowManager>.Instance.DoTransitionSequence(GameState.Map, null);

            if (PauseMenu.instance && PauseMenu.instance.Paused)
                yield return new WaitUntil(() => !PauseMenu.instance.Paused);

            // The waits above can be satisfied by a scene that replaced the one the death arrived in,
            // and its run is gone, so a death from an older generation is dropped here rather than applied.
            if (generation != deathGeneration)
            {
                ArchipelagoModPlugin.Log.LogInfo("Discarded DeathLink: its act was left before it could apply");
                yield break;
            }

            applyingDeath = true;
            int deaths = Mathf.Max(1, pendingDeaths);

            if (SaveManager.saveFile.IsPart1 && Singleton<GameFlowManager>.Instance != null && ProgressionData.LearnedMechanic(MechanicsConcept.LosingLife))
            {
                PauseMenu.pausingDisabled = true;

                RunState finishedRun = RunState.Run;

                if (Singleton<GameFlowManager>.Instance.CurrentGameState == GameState.CardBattle && !(Singleton<TurnManager>.Instance.GameEnding && Singleton<TurnManager>.Instance.opponent == null))
                {
                    int prevLives = RunState.Run.playerLives;
                    yield return new WaitUntil(() => Singleton<TurnManager>.Instance.IsPlayerTurn || Singleton<TurnManager>.Instance.GameIsOver());
                    Singleton<TurnManager>.Instance.PlayerSurrendered = true;

                    if (ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.Sacrificed)
                        yield return new WaitUntil(() => RunState.Run.playerLives == 0);
                    else
                        yield return new WaitUntil(() => RunState.Run.playerLives == prevLives - 1);

                    deaths--;
                }

                // The surrender above spends one death; any banked behind it are spent here, as are
                // all of them when the death did not land in a battle.
                if (deaths > 0 && RunState.Run == finishedRun && RunState.Run.playerLives > 0)
                {
                    if (ArchipelagoData.Act1DeathLinkBehaviour == Act1DeathLink.Sacrificed)
                    {
                        Singleton<GameFlowManager>.Instance.CurrentGameState = GameState.CardBattle;
                        while (RunState.Run.playerLives > 0)
                            yield return Singleton<CandleHolder>.Instance.BlowOutCandleSequence();
                        yield return RandomizerHelper.PrePlayerDeathSequence(Singleton<Part1GameFlowManager>.Instance);
                    }
                    else
                    {
                        while (deaths > 0 && RunState.Run.playerLives > 1)
                        {
                            deaths--;
                            RunState.Run.playerLives--;
                            int smokeIndex = RunState.Run.playerLives;
                            if (Singleton<CandleHolder>.Instance.activeSmoke != null && Singleton<CandleHolder>.Instance.activeSmoke.Count > smokeIndex)
                            {
                                Singleton<CandleHolder>.Instance.activeSmoke[smokeIndex].SetActive(true);
                                CustomCoroutine.WaitThenExecute(20f, delegate
                                {
                                    if (Singleton<CandleHolder>.Instance.activeSmoke != null)
                                    {
                                        Singleton<CandleHolder>.Instance.activeSmoke[smokeIndex].SetActive(false);
                                    }
                                }, false);
                            }
                            Singleton<CandleHolder>.Instance.BlowOutCandle(RunState.Run.playerLives);
                            yield return new WaitForSeconds(0.5f);
                        }

                        if (deaths > 0)
                        {
                            Singleton<GameFlowManager>.Instance.CurrentGameState = GameState.CardBattle;
                            yield return Singleton<CandleHolder>.Instance.BlowOutCandleSequence();
                            yield return RandomizerHelper.PrePlayerDeathSequence(Singleton<Part1GameFlowManager>.Instance);
                        }
                    }
                }

                PauseMenu.pausingDisabled = false;
                if (RunState.Run.playerLives <= 0)
                    yield return new WaitUntil(() => RunState.Run != finishedRun);
            }
            else if (SaveManager.saveFile.IsPart2)
            {
                if (SceneLoader.ActiveSceneName != "GBC_Starting_Island" && SceneLoader.ActiveSceneName != "GBC_WorldMap")
                {
                    if (Singleton<DialogueHandler>.Instance != null && Singleton<DialogueHandler>.Instance.Playing)
                        yield return new WaitUntil(() => !Singleton<DialogueHandler>.Instance.Playing);

                    if (GBCEncounterManager.Instance != null && GBCEncounterManager.Instance.EncounterOccurring)
                    {
                        yield return new WaitUntil(() => 
                            Singleton<TurnManager>.Instance != null && 
                            (Singleton<TurnManager>.Instance.IsPlayerTurn || (Singleton<TurnManager>.Instance.opponent != null && Singleton<TurnManager>.Instance.GameIsOver())));

                        Singleton<TurnManager>.Instance.PlayerSurrendered = true;

                        yield return new WaitUntil(() => !GBCEncounterManager.Instance.EncounterOccurring);
                    }

                    SaveData.Data.natureTemple.roomId = "OutdoorsCentral";
                    SaveData.Data.natureTemple.cameraPosition = Vector2.zero;
                    SaveData.Data.undeadTemple.roomId = "MainRoom";
                    SaveData.Data.undeadTemple.cameraPosition = Vector2.zero;
                    SaveData.Data.techTemple.roomId = "--- MainRoom ---";
                    SaveData.Data.techTemple.cameraPosition = Vector2.zero;
                    SaveData.Data.wizardTemple.roomId = "Floor_1";
                    SaveData.Data.wizardTemple.cameraPosition = Vector2.zero;

                    SaveManager.SaveFile.currentScene = "GBC_WorldMap";
                    if (ArchipelagoOptions.act2RandomizeBridge == Act2RandomizeBridge.LeftSideStart)
                        SaveData.Data.overworldNode = "TechElevator";
                    else
                        SaveData.Data.overworldNode = "StartingIsland";
                    LoadingScreenManager.LoadScene(SaveManager.SaveFile.currentScene);
                }
            }
            else if (SaveManager.saveFile.IsPart3 && Singleton<GameFlowManager>.Instance != null)
            {
                PauseMenu.pausingDisabled = true;

                if (Singleton<GameFlowManager>.Instance.CurrentGameState == GameState.CardBattle)
                {
                    yield return new WaitUntil(() => Singleton<TurnManager>.Instance.IsPlayerTurn);
                    Singleton<TurnManager>.Instance.PlayerSurrendered = true;
                    yield return new WaitUntil(() => Part3SaveData.Data.playerLives < Part3SaveData.Data.playerMaxLives);
                    yield return new WaitUntil(() => Part3SaveData.Data.playerLives == Part3SaveData.Data.playerMaxLives);
                }
                else if (Singleton<HoloGameMap>.Instance != null && !Singleton<HoloGameMap>.Instance.PoweredOff)
                {
                    yield return new WaitUntil(() => Singleton<GameMap>.Instance.FullyUnrolled);
                    yield return Singleton<Part3GameFlowManager>.Instance.PlayerRespawnSequence();
                }

                PauseMenu.pausingDisabled = false;
            }
        }

        private static void OnApplyDeathLinkDone(bool success)
        {
            if (!success)
                ArchipelagoModPlugin.Log.LogError("DeathLink has failed to apply correctly due to an error.");

            applyingDeath = false;
            pendingDeaths = 0;
            receivedDeath = false;
        }

        static public void SendDeathLink()
        {
            if (!ArchipelagoData.DeathLink || receivedDeath)
                return;
            ArchipelagoModPlugin.Log.LogMessage("Sharing death with your friends...");
            var alias = ArchipelagoClient.session.Players.GetPlayerAliasAndName(ArchipelagoClient.session.ConnectionInfo.Slot);
            int i = UnityEngine.Random.Range(0, 2);
            string cause;
            if (i == 0)
                cause = " skill issue";
            else if (i == 1)
                cause = " lack of skill";
            else
                cause = " ineptitude";
            DeathLinkService.SendDeathLink(new DeathLink(ArchipelagoClient.GetPlayerName(ArchipelagoClient.session.ConnectionInfo.Slot), alias + cause));
        }

        // Entering or leaving an act starts from a clean slate: anything banked belonged to the run
        // being left, and a stale applyingDeath would otherwise ignore every later death.
        private static void VoidPendingDeaths()
        {
            if (receivedDeath || applyingDeath || pendingDeaths > 0)
                ArchipelagoModPlugin.Log.LogInfo("Discarded DeathLink: its act was left before it could apply");
            deathGeneration++;
            receivedDeath = false;
            queuedDeathLink = false;
            applyingDeath = false;
            pendingDeaths = 0;
        }

        internal static void HandleDeathLink()
        {
            int act = ActOnScreen();
            // Loading screens read as no act, so only the start screen counts as having left one.
            bool loading = act == 0 && SceneLoader.ActiveSceneName != SceneLoader.StartSceneName;
            if (!loading && act != lastAct)
            {
                lastAct = act;
                VoidPendingDeaths();
            }

            if (queuedDeathLink)
            {
                queuedDeathLink = false;
                FailsafeCoroutine.Start(Singleton<ArchipelagoUI>.Instance, ApplyDeathLink(), OnApplyDeathLinkDone);
            }
        }
    }
}

