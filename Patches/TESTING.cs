/*using BepInEx.Logging;
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

            logger.LogDebug(RoundManager.Instance.currentLevel.name);

            ItemDropship dropShip = UnityEngine.Object.FindObjectsOfType<ItemDropship>().FirstOrDefault();
            if (dropShip != null )
            {
                logger.LogDebug("Timer: " + dropShip.shipTimer);
            }

            MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();

            foreach (var node in filteredNodes)
            {
                logger.LogDebug(node.OriginalName);
                logger.LogDebug(node.Name);
                logger.LogDebug("Unlock price: " + node.UnlockPrice);
                logger.LogDebug("Prices: " + string.Join(", ", node.Prices));
                logger.LogDebug("Max Upgrade: " + node.MaxUpgrade);
                logger.LogDebug("Current Tier: " + node.CurrentUpgrade);
            }

            logger.LogDebug(StartOfRound.Instance.randomMapSeed);
            // Grab anims: HoldShotgun, HoldLung, GrabClipboard, HoldJetpack, HoldKnife
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
                NetworkHandler.Instance.ResetAllData();
                HUDManager.Instance.DisplayTip("Testing", "Reset Data");
            }
            if (args[0] == "/sprintTime")
            {
                localPlayer.sprintTime = float.Parse(args[1]);
            }
            if (args[0] == "/sprintMultiplier")
            {
                localPlayer.sprintMultiplier = float.Parse(args[1]);
            }
            if (args[0] == "/climbSpeed")
            {
                localPlayer.climbSpeed = float.Parse(args[1]);
            }
            if (args[0] == "/land")
            {
                ItemDropship dropShip = UnityEngine.Object.FindObjectsOfType<ItemDropship>().FirstOrDefault();
                if (dropShip != null)
                {
                    dropShip.shipTimer = float.Parse(args[1]);
                }
            }
            if (args[0] == "/clip")
            {
                Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                int index = int.Parse(args[1]);
                localPlayer.statusEffectAudio.PlayOneShot(terminal.syncedAudios[index], 1f);
            }
        }
    }
}*/