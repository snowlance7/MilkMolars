using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static MilkMolars.Plugin;
using static MilkMolars.MilkMolarController;
using MoreShipUpgrades.Misc.TerminalNodes;
using System.Linq;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player;
using Steamworks.Data;
using MoreShipUpgrades.Managers;

namespace MilkMolars
{
    public static class LGUCompatibility
    {
        private static readonly BepInEx.Logging.ManualLogSource logger = LoggerInstance;

        private static bool? _enabled;

        public static bool enabled
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
        public static List<MilkMolarUpgrade> GetLGUUpgrades(bool mega = false)
        {
            logger.LogDebug("GetLGUUpgrades: " + enabled);
            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();
            if (!mega) // MILK MOLARS
            {
                CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && !x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                
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
                        upgrade.costsPerTier = new int[node.Prices.Length + 1];
                        upgrade.costsPerTier[0] = 0;
                        upgrade.costsPerTier[1] = (int)Math.Ceiling(node.UnlockPrice / configLGUMilkMolarContributeAmount.Value);
                        
                        for (int i = 2; i < upgrade.costsPerTier.Length - 1; i++)
                        {
                            int amount = (int)Math.Ceiling(node.Prices[i - 2] / configLGUMilkMolarContributeAmount.Value);
                            upgrade.costsPerTier[i] = amount;
                        }
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
                CustomTerminalNode[] filteredNodes = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.Visible && x.SharedUpgrade && (x.UnlockPrice > 0 || (x.OriginalName == NightVision.UPGRADE_NAME && (x.Prices.Length > 0 && x.Prices[0] != 0)))).ToArray();
                logger.LogDebug("Got " + filteredNodes.Length + " nodes");

                foreach (var node in filteredNodes)
                {
                    logger.LogDebug("Adding " + node.OriginalName);
                    MilkMolarUpgrade upgrade = new LGUUpgrade();
                    upgrade.name = node.OriginalName;
                    upgrade.title = node.Name;
                    upgrade.LGUUpgrade = true;

                    if (node.Prices.Length > 0)// TODO: Fix this
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUTier;
                        upgrade.costsPerTier = new int[node.Prices.Length + 2];
                        upgrade.costsPerTier[0] = 0;
                        upgrade.costsPerTier[1] = (int)Math.Ceiling(node.UnlockPrice / configLGUMegaMilkMolarContributeAmount.Value);

                        for (int i = 2; i < upgrade.costsPerTier.Length; i++)
                        {
                            int amount = (int)Math.Ceiling(node.Prices[i - 2] / configLGUMegaMilkMolarContributeAmount.Value);
                            upgrade.costsPerTier[i] = amount;
                        }
                        logger.LogDebug("Upgrade costs: " + string.Join(", ", upgrade.costsPerTier));
                    }
                    else
                    {
                        upgrade.type = MilkMolarUpgrade.UpgradeType.LGUOneTimeUnlock;
                        upgrade.cost = (int)Math.Ceiling(node.UnlockPrice / configLGUMegaMilkMolarContributeAmount.Value);
                    }
                    upgrades.Add(upgrade);
                }
            }

            return upgrades;
        }
    }

    public class LGUUpgrade : MilkMolarUpgrade
    {
        public override void ActivateCurrentTierUpgrade()
        {
            base.ActivateCurrentTierUpgrade();

            CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null)
            {
                if (!node.Unlocked)
                {
                    LguStore.Instance.HandleUpgrade(node);
                }
                else if (node.Unlocked && node.MaxUpgrade > node.CurrentUpgrade)
                {
                    LguStore.Instance.HandleUpgrade(node, increment: true);
                }
            }
        }

        public override void ActivateOneTimeUpgrade()
        {
            base.ActivateOneTimeUpgrade();

            CustomTerminalNode node = MoreShipUpgrades.Managers.UpgradeBus.Instance.terminalNodes.Where(x => x.OriginalName == name).FirstOrDefault();
            if (node != null)
            {
                LguStore.Instance.HandleUpgrade(node);
            }
        }
    }
}