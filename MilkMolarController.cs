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

                MilkMolarUpgrades = GetUpgrades();
                //NetworkHandler.ClientsMilkMolarUpgrades.Add(localPlayer.actualClientId, MilkMolarUpgrades);
                NetworkHandler.MegaMilkMolarUpgrades = GetUpgrades(true);
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
            NetworkHandler.Instance.AddMilkMolarServerRpc(player.actualClientId);
        }

        public static void AddMegaMilkMolar()
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc();
        }

        public static bool BuyMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                if (MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    upgrade.ActivateRepeatableUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayer.actualClientId);
                    return true;
                }
                return false;
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    upgrade.ActivateOneTimeUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayer.actualClientId);
                    return true;
                }
                return false;
            }

            if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.nextTierCost)
            {
                MilkMolars -= upgrade.nextTierCost;
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                NetworkHandler.Instance.UpdateMilkMolarsServerRpc(MilkMolars, localPlayer.actualClientId);
                return true;
            }

            return false;
        }

        public static bool BuyMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade, bool callRPC = false) // THIS IS GOOD DONT TOUCH IT
        {
            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                if (NetworkHandler.MegaMilkMolars.Value >= upgrade.cost || callRPC == false)
                {
                    if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayer.actualClientId); }
                    upgrade.ActivateRepeatableUpgrade();
                    return true;
                }
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                if ((!upgrade.fullyUpgraded && NetworkHandler.MegaMilkMolars.Value >= upgrade.cost) || callRPC == false)
                {
                    if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayer.actualClientId); }
                    upgrade.ActivateOneTimeUpgrade();
                    return true;
                }
            }

            if ((!upgrade.fullyUpgraded && NetworkHandler.MegaMilkMolars.Value >= upgrade.nextTierCost) || callRPC == false)
            {
                if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.nextTierCost, localPlayer.actualClientId); }
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                return true;
            }

            return false;
        }
    }
}