using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static MilkMolars.Plugin;

namespace MilkMolars.Patches
{
    [HarmonyPatch]
    internal class TESTING : MonoBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        private static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        [HarmonyPostfix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        public static void PingScan_performedPostFix()
        {
            logger.LogDebug("Milk Molar Points: " + MilkMolarController.MilkMolars);
            logger.LogDebug($"Milk molar upgrades: {MilkMolarController.MilkMolarUpgrades.Count}");
            logger.LogDebug($"Mega Milk molar upgrades: {MilkMolarController.MegaMilkMolarUpgrades.Count}");
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
        public static void SubmitChat_performedPrefix(HUDManager __instance)
        {
            string msg = __instance.chatTextField.text;
            string[] args = msg.Split(" ");
            logger.LogDebug(msg);


            // Comment these out
            if (args[0] == "/refresh")
            {
                RoundManager.Instance.RefreshEnemiesList();
            }
            if (args[0] == "/height")
            {
                //UpgradeUIController.setHeight = float.Parse(args[1]);
            }
            if (args[0] == "/molar")
            {
                MilkMolarController.AddMultipleMilkMolars(int.Parse(args[1]));
            }
        }
    }
}