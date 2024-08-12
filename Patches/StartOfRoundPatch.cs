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
        [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
        public static void OnClientConnectPostfix(StartOfRound __instance, ulong clientId) // TODO: Test this
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                //logger.LogDebug("OnClientConnect: " + clientId);
                //NetworkHandler.SendDataToClient(clientId);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.firstDayAnimation))]
        public static void firstDayAnimationPostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                logger.LogError("FIRSTDAYANIMATION");
                MilkMolarController.Init();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPrefix(StartOfRound __instance)
        {
            //var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            //string upgrades = JsonConvert.SerializeObject(MilkMolarController.MilkMolarUpgrades, settings);
            //NetworkHandler.Instance.SendUpgradeDataToServerServerRpc(upgrades, localPlayer.actualClientId);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
        public static void AutoSaveShipDataPostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                //NetworkHandler.SaveDataToFile();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.EndOfGame))]
        public static void EndOfGamePostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                //NetworkHandler.SaveDataToFile();
                
            }
        }
    }
}
