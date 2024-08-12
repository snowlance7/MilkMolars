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
                logger.LogDebug("OnClientConnect: " + clientId);
                NetworkHandler.SendDataToClient(clientId);
                /*if (clientId != localPlayer.actualClientId)
                {
                    NetworkHandler.SendDataToClient(clientId);
                }*/
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.firstDayAnimation))]
        public static void firstDayAnimationPostfix(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MilkMolarController.Init();
            }
        }
    }
}
