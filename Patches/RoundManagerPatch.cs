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
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;
        private const string tooth = "🦷";
        private static bool allPlayersDead = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
        public static void SpawnScrapInLevelPostfix(RoundManager __instance) // TODO: Test this
        {
            MilkMolarController.SpawnMolarsInLevel();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPrefix() // TODO: Test this
        {
            MilkMolarUpgrade keepScrapUpgrade = NetworkHandler.MegaMilkMolarUpgrades.FirstOrDefault(x => x.name == "keepItemsOnShipChance");
            if (keepScrapUpgrade != null && keepScrapUpgrade.unlocked)
            {
                int randomNum = UnityEngine.Random.Range(0, 101);
                allPlayersDead = keepScrapUpgrade.currentTierAmount >= randomNum;
                StartOfRound.Instance.allPlayersDead = allPlayersDead;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPostfix() // TODO: Test this
        {
            MilkMolarUpgrade keepScrapUpgrade = NetworkHandler.MegaMilkMolarUpgrades.FirstOrDefault(x => x.name == "keepItemsOnShipChance");
            if (keepScrapUpgrade != null && keepScrapUpgrade.unlocked)
            {
                StartOfRound.Instance.allPlayersDead = allPlayersDead;
            }
        }
    }
}
