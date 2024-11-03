using BepInEx.Logging;
using HarmonyLib;
using MilkMolars.LGU;
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
            Name = "keepItemsOnShipChance";
            Title = "Keep Items On Ship";
            Description = "Chance to keep items on ship when all players die, chance increases with tier";
            Type = UpgradeType.TierPercent;
            GetTiersFromString(configKeepItemsOnShipChanceUpgrade.Value);
            Shared = true;
            Visible = !LGUCompatibility.enabled;
        }
    }

    [HarmonyPatch]
    internal class KeepItemsOnShipChanceUpgradePatches
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        private static bool allPlayersDead = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPrefix() // TODO: Test this
        {
            try
            {
                if (MilkMolarController.MegaMilkMolarUpgrades != null)
                {
                    MilkMolarUpgrade keepScrapUpgrade = MilkMolarController.MegaMilkMolarUpgrades.FirstOrDefault(x => x.Name == "keepItemsOnShipChance");
                    if (keepScrapUpgrade != null && keepScrapUpgrade.Unlocked)
                    {
                        int randomNum = UnityEngine.Random.Range(0, 101);
                        allPlayersDead = StartOfRound.Instance.allPlayersDead;
                        StartOfRound.Instance.allPlayersDead = keepScrapUpgrade.CurrentTierAmount >= randomNum;
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
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
        public static void DespawnPropsAtEndOfRoundPostfix() // TODO: Test this
        {
            try
            {
                if (MilkMolarController.MegaMilkMolarUpgrades != null)
                {
                    MilkMolarUpgrade keepScrapUpgrade = MilkMolarController.MegaMilkMolarUpgrades.FirstOrDefault(x => x.Name == "keepItemsOnShipChance");
                    if (keepScrapUpgrade != null && keepScrapUpgrade.Unlocked)
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
