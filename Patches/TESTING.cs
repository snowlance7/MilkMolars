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

        [HarmonyPostfix, HarmonyPatch(typeof(HUDManager), nameof(HUDManager.PingScan_performed))]
        public static void PingScan_performedPostFix()
        {
            logger.LogDebug("Milk Molars: " + MilkMolarController.MilkMolars);
            logger.LogDebug("Mega Milk Molars: " + NetworkHandler.MegaMilkMolars.Value);
            logger.LogDebug($"Milk molar upgrades: {MilkMolarController.MilkMolarUpgrades.Count}");
            logger.LogDebug($"Mega Milk molar upgrades: {NetworkHandler.MegaMilkMolarUpgrades.Count}");
            logger.LogDebug(localPlayer.playerClientId);
            logger.LogDebug(localPlayer.playerSteamId);
            logger.LogDebug(localPlayer.playerUsername);
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
                logger.LogDebug("Refreshed enemies list");
            }
            if (args[0] == "/height")
            {
                //UpgradeUIController.setHeight = float.Parse(args[1]);
            }
            if (args[0] == "/molar")
            {
                MilkMolarController.MilkMolars = int.Parse(args[1]);
                HUDManager.Instance.DisplayTip("Testing", $"Milk Molars: {MilkMolarController.MilkMolars}");
            }
            if (args[0] == "/mega")
            {
                NetworkHandler.MegaMilkMolars.Value = int.Parse(args[1]);
                HUDManager.Instance.DisplayTip("Testing", $"Mega Milk Molars: {NetworkHandler.MegaMilkMolars.Value}");
            }
            if (args[0] == "/save")
            {
                NetworkHandler.SendAllDataToServer();
                NetworkHandler.SaveDataToFile();
                HUDManager.Instance.DisplayTip("Testing", "Saved Data");
            }
            if (args[0] == "/load")
            {
                NetworkHandler.LoadDataFromFile();
                HUDManager.Instance.DisplayTip("Testing", "Loaded Data");
            }
            if (args[0] == "/reset")
            {
                NetworkHandler.ResetAllData();
                HUDManager.Instance.DisplayTip("Testing", "Reset Data");
            }
        }
    }
}