using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MilkMolars.Plugin;

namespace MilkMolars.Upgrades
{
    internal class KeepItemsOnShipChanceUpgrade : MilkMolarUpgrade
    {
        public KeepItemsOnShipChanceUpgrade()
        {
            name = "keepItemsOnShipChance";
            title = "Keep Items On Ship";
            description = "Chance to keep items on ship when all players die, chance increases with tier";
            type = UpgradeType.TierPercent;
            GetTiersFromString(configKeepItemsOnShipChanceUpgrade.Value);
        }
    }

    [HarmonyPatch]
    internal class KeepItemsOnShipChanceUpgradePatches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        private static bool allPlayersDead = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPrefix() // TODO: Test this
        {
            try
            {
                if (NetworkHandler.MegaMilkMolarUpgrades != null)
                {
                    MilkMolarUpgrade keepScrapUpgrade = NetworkHandler.MegaMilkMolarUpgrades.FirstOrDefault(x => x.name == "keepItemsOnShipChance");
                    if (keepScrapUpgrade != null && keepScrapUpgrade.unlocked)
                    {
                        int randomNum = UnityEngine.Random.Range(0, 101);
                        allPlayersDead = keepScrapUpgrade.currentTierAmount >= randomNum;
                        StartOfRound.Instance.allPlayersDead = allPlayersDead;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
                return;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPostfix() // TODO: Test this
        {
            try
            {
                if (NetworkHandler.MegaMilkMolarUpgrades != null)
                {
                    MilkMolarUpgrade keepScrapUpgrade = NetworkHandler.MegaMilkMolarUpgrades.FirstOrDefault(x => x.name == "keepItemsOnShipChance");
                    if (keepScrapUpgrade != null && keepScrapUpgrade.unlocked)
                    {
                        StartOfRound.Instance.allPlayersDead = allPlayersDead;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
                return;
            }
        }
    }
}
