using BepInEx.Logging;
using HarmonyLib;
using InteractiveTerminalAPI.UI;
using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;
using Newtonsoft.Json;

namespace MilkMolars
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPostfix(StartOfRound __instance)
        {
            logger.LogDebug("AutoSaveShipDataPostfix called"); // TODO: make sure if this runs for all players and when it does
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                //NetworkHandler.SaveDataToFile();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.playersFiredGameOver))]
        public static void playersFiredGameOverPrefix(StartOfRound __instance)
        {
            logger.LogDebug("In EndPlayersFiredSequenceClientRpcPostfix"); // TODO: make sure if this runs for all players and when it does

            //NetworkHandler.ResetAllData();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.firstDayAnimation))]
        public static void firstDayAnimationPrefix(StartOfRound __instance)
        {
            logger.LogDebug("In firstDayAnimationPrefix"); // TODO: make sure if this runs for all players and when it does
        }
    }
}
