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

        /*[HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
        public static void OnClientConnectPostfix(StartOfRound __instance, ulong steamId) // TODO: Test this
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                logger.LogDebug("OnClientConnectPostfix called, sending data to client with id " + steamId);
                NetworkHandler.SendDataToClient(steamId);
            }
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPrefix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                logger.LogDebug("AutoSaveShipDataPrefix called");
                NetworkHandler.SendAllDataToServer();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                logger.LogDebug("AutoSaveShipDataPostfix called");
                NetworkHandler.SaveDataToFile();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.EndOfGame))]
        public static void EndOfGamePostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                NetworkHandler.ResetAllData();
                
            }
        }
    }
}
