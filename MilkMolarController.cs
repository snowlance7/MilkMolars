using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    public static class MilkMolarController
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static int MilkMolars = 0;

        public static bool InUpgradeUI = false;
        public static bool InMegaUpgradeUI = false;

        public static List<MilkMolarUpgrade> MilkMolarUpgrades = new List<MilkMolarUpgrade>();

        public static List<MilkMolarUpgrade> ExtraMilkMolarUpgrades = new List<MilkMolarUpgrade>();
        public static List<MilkMolarUpgrade> ExtraMegaMilkMolarUpgrades = new List<MilkMolarUpgrade>();

        public static void Init()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                LoggerInstance.LogDebug("Initing milk molar controller");

                NetworkHandler.LoadDataFromFile();
            }
        }

        public static List<MilkMolarUpgrade> GetUpgrades(bool mega = false)
        {
            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();
            if (!mega)
            {
                //// Milk Molars // TODO: Sync these with host
                // Shovel damage
                MilkMolarUpgrade shovelDamage = new MilkMolarUpgrade();
                shovelDamage.name = "ShovelDamage";
                shovelDamage.title = "Shovel Damage";
                shovelDamage.type = MilkMolarUpgrade.UpgradeType.TierNumber;
                shovelDamage.GetTiersFromString(configShovelDamageUpgrade.Value);
                upgrades.Add(shovelDamage);

                // Damage resistance
                MilkMolarUpgrade damageResistance = new MilkMolarUpgrade();
                damageResistance.name = "DamageResistance";
                damageResistance.title = "Damage Resistance";
                damageResistance.type = MilkMolarUpgrade.UpgradeType.TierPercent;
                damageResistance.GetTiersFromString(configDamageResistanceUpgrade.Value);
                upgrades.Add(damageResistance);

                // Sprint speed


                // Sprint endurance
                // Sprint regeneration
                // Jump height
                // Carry weight stamina cost
                // Carry weight sprint speed
                // Increased inventory
                // Crit chance
                // Climb speed
                // Stun gun upgrades
                // Fall damage reduction

                if (ExtraMilkMolarUpgrades.Count > 0)
                {
                    upgrades.AddRange(ExtraMilkMolarUpgrades);
                }

                LoggerInstance.LogDebug(upgrades.Count);
            }
            else
            {
                //// Mega Milk Molars
                // Signal Transmitter Upgrade
                MilkMolarUpgrade signalTransmitter = new MilkMolarUpgrade();
                signalTransmitter.name = "SignalTransmitterUpgrade";
                signalTransmitter.title = "Signal Transmitter Upgrade";
                signalTransmitter.type = MilkMolarUpgrade.UpgradeType.OneTimeUnlock;
                signalTransmitter.cost = configSignalTransmitterUpgrade.Value;
                upgrades.Add(signalTransmitter);

                // Increased shop deals: Increases the maximum amount of items that can be on sale in the store.
                // Landing speed
                // Item dropship landing speed
                // Keep items on ship chance
                // Travel discount
                // Time on moon
                // Company Cruiser health
                // Company Cruiser acceleration
                // Company Cruiser max speed
                // Company Cruiser turning
                // Company Cruiser damage reduction

                // Revive player

                if (ExtraMegaMilkMolarUpgrades.Count > 0)
                {
                    upgrades.AddRange(ExtraMegaMilkMolarUpgrades);
                }
            }

            return upgrades;
        }

        public static void UpdateMilkMolarUpgrades()
        {
            // TODO: Sync these with host
        }
        
        public static void RegisterMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            if (MilkMolarUpgrades.Where(x => x.name == upgrade.name).FirstOrDefault() == null)
            {
                MilkMolarUpgrades.Add(upgrade);
            }
            else
            {
                LoggerInstance.LogError($"Error: Theres already a Milk Molar upgrade with the name: {upgrade.name}");
            }
        }

        public static void RegisterMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (NetworkHandler.MegaMilkMolarUpgrades.Where(x => x.name == upgrade.name).FirstOrDefault() == null)
                {
                    NetworkHandler.MegaMilkMolarUpgrades.Add(upgrade);
                }
                else
                {
                    LoggerInstance.LogError($"Error: Theres already a Mega Milk Molar upgrade with the name: {upgrade.name}");
                }
            }
        }

        public static MilkMolarUpgrade GetUpgradeByName(string name, bool megaUpgrade = false)
        {
            if (megaUpgrade)
            {
                return NetworkHandler.MegaMilkMolarUpgrades.Find(x => x.name == name);
            }
            else
            {
                return MilkMolarUpgrades.Find(x => x.name == name);
            }
        }

        public static void AddMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMilkMolarServerRpc(player.actualClientId); // TODO: Change this to steamId later
        }

        public static void AddMegaMilkMolar()
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc();
        }

        public static bool BuyMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            logger.LogDebug("Attempting to buy Milk Molar upgrade: " + upgrade.name);

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                logger.LogDebug("Upgrade type is Repeatable. Checking if we have enough Milk Molars.");
                if (MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    logger.LogDebug("Conditions met. Activating Repeatable upgrade and updating server.");
                    upgrade.ActivateRepeatableUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayerId);
                    return true;
                }
                logger.LogDebug("Not enough Milk Molars for Repeatable upgrade.");
                return false;
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                logger.LogDebug("Upgrade type is OneTimeUnlock. Checking if upgrade is not fully upgraded and if we have enough Milk Molars.");
                if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    logger.LogDebug("Conditions met. Activating OneTimeUnlock upgrade and updating server.");
                    upgrade.ActivateOneTimeUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayerId);
                    return true;
                }
                logger.LogDebug("Not enough Milk Molars or upgrade is fully upgraded.");
                return false;
            }

            logger.LogDebug("Checking if upgrade is not fully upgraded and if we have enough Milk Molars for next tier.");
            if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.nextTierCost)
            {
                MilkMolars -= upgrade.nextTierCost;
                logger.LogDebug("Conditions met. Going to next tier, activating current tier upgrade, and updating server.");
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayerId);
                return true;
            }

            logger.LogDebug("Upgrade purchase failed. Conditions not met.");
            return false;
        }


        public static bool BuyMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade, bool callRPC = false) // THIS IS GOOD DONT TOUCH IT
        {
            logger.LogDebug("Attempting to buy Mega Milk Molar upgrade: " + upgrade.name);

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                logger.LogDebug("Upgrade type is Repeatable. Checking if we have enough Mega Milk Molars or RPC is not required.");
                if (NetworkHandler.MegaMilkMolars.Value >= upgrade.cost || callRPC == false)
                {
                    logger.LogDebug("Conditions met. Activating Repeatable upgrade.");
                    if (callRPC)
                    {
                        logger.LogDebug("Calling server RPC to buy upgrade.");
                        NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayerId);
                    }
                    upgrade.ActivateRepeatableUpgrade();
                    return true;
                }
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                logger.LogDebug("Upgrade type is OneTimeUnlock. Checking if upgrade is not fully upgraded and if we have enough Mega Milk Molars or RPC is not required.");
                if ((!upgrade.fullyUpgraded && NetworkHandler.MegaMilkMolars.Value >= upgrade.cost) || callRPC == false)
                {
                    logger.LogDebug("Conditions met. Activating OneTimeUnlock upgrade.");
                    if (callRPC)
                    {
                        logger.LogDebug("Calling server RPC to buy upgrade.");
                        NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayerId);
                    }
                    upgrade.ActivateOneTimeUpgrade();
                    return true;
                }
            }

            logger.LogDebug("Checking if upgrade is not fully upgraded and if we have enough Mega Milk Molars or RPC is not required.");
            if ((!upgrade.fullyUpgraded && NetworkHandler.MegaMilkMolars.Value >= upgrade.nextTierCost) || callRPC == false)
            {
                logger.LogDebug("Conditions met. Going to next tier and activating current tier upgrade.");
                if (callRPC)
                {
                    logger.LogDebug("Calling server RPC to buy upgrade.");
                    NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.nextTierCost, localPlayerId);
                }
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                return true;
            }

            logger.LogDebug("Upgrade purchase failed. Conditions not met.");
            return false;
        }

    }
}