using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static List<MilkMolarUpgrade> GetLGUUpgrades(bool mega = false)
        {
            logger.LogDebug("GetLGUUpgrades: " + enabled);

            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();
            if (!mega) // MILK MOLARS
            {
                MoreShipUpgrades.Misc.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && !x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                
                foreach (var node in filteredNodes)
                {
                    logger.LogDebug("Adding " + node.OriginalName);
                    MilkMolarUpgrade upgrade = new LGUUpgrade();
                    upgrade.name = node.OriginalName;
                    upgrade.title = node.Name;
                    upgrade.LGUUpgrade = true;
                    
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
                    }
                    else
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUOneTimeUnlock;
                        upgrade.cost = node.UnlockPrice;
                    }
                    upgrades.Add(upgrade);
                }

                LoggerInstance.LogDebug(upgrades.Count);
            }
            else // MEGA MILK MOLARS
            {
                MoreShipUpgrades.Misc.TerminalNodes.CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player.NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                logger.LogDebug("Got " + filteredNodes.Length + " nodes");

                foreach (var node in filteredNodes)
                {
                    logger.LogDebug("Adding " + node.OriginalName);
                    MilkMolarUpgrade upgrade = new LGUUpgrade();
                    upgrade.name = node.OriginalName;
                    upgrade.title = node.Name;
                    upgrade.LGUUpgrade = true;

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
        }
    }

    internal class LGUUpgrade : MilkMolarUpgrade
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void ActivateCurrentTierUpgrade()
        {
            base.ActivateCurrentTierUpgrade();

            MoreShipUpgrades.Misc.TerminalNodes.CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null)
            {
                if (!node.Unlocked)
                {
                    MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node);
                }
                else if (node.Unlocked && node.MaxUpgrade > node.CurrentUpgrade)
                {
                    MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node, increment: true);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public override void ActivateOneTimeUpgrade()
        {
            base.ActivateOneTimeUpgrade();

            MoreShipUpgrades.Misc.TerminalNodes.CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null)
            {
                MoreShipUpgrades.Managers.LguStore.Instance.HandleUpgrade(node);
            }
        }
    }
}