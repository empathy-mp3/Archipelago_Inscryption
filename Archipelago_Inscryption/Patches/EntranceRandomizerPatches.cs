using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Helpers;
using DiskCardGame;
using GBC;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Archipelago_Inscryption.Patches
{
    internal class EntranceRandomizerPatches
    {
        [HarmonyPatch(typeof(SceneTransitionVolume), "ChangeScene")]
        [HarmonyPrefix]
        static void ChangeSceneTransitionDestination(SceneTransitionVolume __instance)
        {
            switch (SceneLoader.ActiveSceneName) {
                case "GBC_Temple_Nature":
                    __instance.sceneId = "GBC_Temple_Wizard";
                    break;
                case "GBC_Temple_Wizard":
                    __instance.sceneId = "GBC_Temple_Tech";
                    break;
                case "GBC_Temple_Tech":
                    __instance.sceneId = "GBC_Temple_Undead";
                    break;
                case "GBC_Temple_Undead":
                    __instance.sceneId = "GBC_Temple_Nature";
                    break;
            }
        }
    }
}