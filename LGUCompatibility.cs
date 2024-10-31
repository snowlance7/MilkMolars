using HarmonyLib;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.UI.TerminalNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class LGUCompatibility
    {
        private static readonly BepInEx.Logging.ManualLogSource logger = LoggerInstance;

        private static bool? _enabled;

        internal static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades") && configLGUCompatible.Value;
                }
                return (bool)_enabled;
            }
        }

        /*[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static List<MilkMolarUpgrade> GetLGUUpgrades(bool mega = false)
        {
            logger.LogDebug("GetLGUUpgrades: " + enabled);

            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();
            if (!mega) // MILK MOLARS
            {
                MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && !x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                
                foreach (var node in filteredNodes)
                {
                    logger.LogDebug("Adding " + node.OriginalName);
                    MilkMolarUpgrade upgrade = new LGUUpgrade();
                    upgrade.name = node.OriginalName;
                    upgrade.title = node.Name;
                    upgrade.LGUUpgrade = true;
                    upgrade.unlocked = node.Unlocked;

                    if (node.Prices.Length > 0)
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUTier;

                        List<int> costsPerTierList = new List<int>();
                        costsPerTierList.Add(0);
                        costsPerTierList.Add((int)Math.Ceiling(node.UnlockPrice / configLGUMilkMolarContributeAmount.Value));

                        foreach (var price in node.Prices)
                        {
                            costsPerTierList.Add((int)Math.Ceiling(price / configLGUMilkMolarContributeAmount.Value));
                        }

                        upgrade.costsPerTier = costsPerTierList.ToArray();
                        logger.LogDebug("Upgrade costs: " + string.Join(", ", upgrade.costsPerTier));

                        if (!upgrade.unlocked)
                            upgrade.currentTier = 0;
                        else upgrade.currentTier = node.CurrentUpgrade + 1;

                        logger.LogDebug($"Upgrade unlocked and tier: {upgrade.unlocked}, {upgrade.currentTier}");

                        if (upgrade.currentTier >= upgrade.maxTier)
                        {
                            upgrade.fullyUpgraded = true;
                        }
                        logger.LogDebug($"Upgrade fully upgrade: {upgrade.fullyUpgraded}");
                    }
                    else
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUOneTimeUnlock;
                        upgrade.cost = (int)Math.Ceiling(node.UnlockPrice / configLGUMilkMolarContributeAmount.Value);
                    }
                    upgrades.Add(upgrade);
                }

                LoggerInstance.LogDebug(upgrades.Count);
            }
            else // MEGA MILK MOLARS
            {
                MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                logger.LogDebug("Got " + filteredNodes.Length + " nodes");

                foreach (var node in filteredNodes)
                {
                    logger.LogDebug("Adding " + node.OriginalName);
                    MilkMolarUpgrade upgrade = new LGUUpgrade();
                    upgrade.name = node.OriginalName;
                    upgrade.title = node.Name;
                    upgrade.LGUUpgrade = true;
                    upgrade.unlocked = node.Unlocked;

                    if (node.Prices.Length > 0)
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUTier;

                        List<int> costsPerTierList = new List<int>();
                        costsPerTierList.Add(0);
                        costsPerTierList.Add((int)Math.Ceiling(node.UnlockPrice / configLGUMegaMilkMolarContributeAmount.Value));

                        foreach(var price in node.Prices)
                        {
                            costsPerTierList.Add((int)Math.Ceiling(price / configLGUMegaMilkMolarContributeAmount.Value));
                        }

                        upgrade.costsPerTier = costsPerTierList.ToArray();
                        logger.LogDebug("Upgrade costs: " + string.Join(", ", upgrade.costsPerTier));

                        if (!upgrade.unlocked)
                            upgrade.currentTier = 0;
                        else upgrade.currentTier = node.CurrentUpgrade + 1;

                        logger.LogDebug($"Upgrade unlocked and tier: {upgrade.unlocked}, {upgrade.currentTier}");

                        if (upgrade.currentTier >= upgrade.maxTier)
                        {
                            upgrade.fullyUpgraded = true;
                        }
                        logger.LogDebug($"Upgrade fully upgrade: {upgrade.fullyUpgraded}");
                    }
                    else
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUOneTimeUnlock;
                        upgrade.cost = (int)Math.Ceiling(node.UnlockPrice / configLGUMegaMilkMolarContributeAmount.Value);
                        logger.LogDebug("One time upgrade costs: " + upgrade.cost);
                    }
                    upgrades.Add(upgrade);
                }
            }

            return upgrades;
        }*/
    }

    internal class LGUUpgrade : MilkMolarUpgrade
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void ActivateCurrentTierUpgrade()
        {
            base.ActivateCurrentTierUpgrade();

            MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null)
            {
                if (!node.Unlocked)
                {
                    LoggerInstance.LogDebug("Unlocking lgu upgrade for first time");
                    MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node);
                    LoggerInstance.LogDebug("Done unlocking lgu upgrade for first time");
                }
                else if (node.Unlocked && node.MaxUpgrade > node.CurrentUpgrade) // TODO: Test this
                {
                    LoggerInstance.LogDebug($"{node.MaxUpgrade} > {node.CurrentUpgrade}");
                    MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node, increment: true);
                    LoggerInstance.LogDebug("Done incrementing lgu upgrade");
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void ActivateOneTimeUpgrade()
        {
            base.ActivateOneTimeUpgrade();

            MoreShipUpgrades.UI.TerminalNodes.CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null && !node.Unlocked)
            {
                MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node);
            }
        }
    }
    [HarmonyPatch]
    internal static class LguStorePatcher
    {
        [HarmonyPatch(typeof(LguStore), nameof(LguStore.UpdateUpgrades))]
        [HarmonyPostfix]
        static void UpdateUpgradesPostfix(CustomTerminalNode node)
        {
            MilkMolarUpgrade molarUpgrade;
            if (node.SharedUpgrade)
            {
                molarUpgrade = NetworkHandler.MegaMilkMolarUpgrades.Where(x => x.LGUUpgrade && x.name == node.OriginalName).FirstOrDefault();
            }
            else
            {
                molarUpgrade = MilkMolarController.MilkMolarUpgrades.Where(x => x.LGUUpgrade && x.name == node.OriginalName).FirstOrDefault();
            }
            if (molarUpgrade == null) return; // Wasn't found an upgrade for the LGU one
            molarUpgrade.GoToNextTier();
        }
        [HarmonyPatch(typeof(LguStore), nameof(LguStore.WaitForUpgradeObject), MethodType.Enumerator)]
        [HarmonyPostfix]
        static void WaitForUpgradeObjectPostfix(bool __result)
        {
            if (__result) return;
            Plugin.LoggerInstance.LogDebug("YIPPPPPPPPPPPPPPPPPIE");
            NetworkHandler.MegaMilkMolarUpgrades.AddRange(LGUCompatibility.GetLGUUpgrades(mega: true));
            MilkMolarController.MilkMolarUpgrades.AddRange(LGUCompatibility.GetLGUUpgrades());

            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                MilkMolarController.GetMegaMilkMolarUpgrades();
            }
        }
    }
}