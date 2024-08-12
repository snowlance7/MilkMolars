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
using LethalModDataLib;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace MilkMolars
{
    public static class MilkMolarController
    {
        public static int MilkMolars = 0;

        public static bool InUpgradeUI = false;
        public static bool InMegaUpgradeUI = false;

        [ModData(saveWhen: SaveWhen.OnAutoSave, loadWhen: LoadWhen.Manual, saveLocation: SaveLocation.CurrentSave, resetWhen: ResetWhen.OnGameOver)]
        public static List<MilkMolarUpgrade> MilkMolarUpgrades;

        [ModData(saveWhen: SaveWhen.OnAutoSave, loadWhen: LoadWhen.Manual, saveLocation: SaveLocation.CurrentSave, resetWhen: ResetWhen.OnGameOver)] // TODO: Set up manual loading
        public static List<MilkMolarUpgrade> MegaMilkMolarUpgrades;

        public static List<MilkMolarUpgrade> ExtraMilkMolarUpgrades = new List<MilkMolarUpgrade>();
        public static List<MilkMolarUpgrade> ExtraMegaMilkMolarUpgrades = new List<MilkMolarUpgrade>();

        public static void Init()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                LoggerInstance.LogDebug("Initing milk molar controller");
                if (NetworkHandler.ClientsMilkMolars != null && NetworkHandler.ClientsMilkMolars.Count > 0 && NetworkHandler.ClientsMilkMolars.ContainsKey(localPlayer.actualClientId))
                {
                    LoggerInstance.LogDebug("getting milk molars");
                    MilkMolars = NetworkHandler.ClientsMilkMolars[localPlayer.actualClientId];
                    MilkMolarUpgrades = NetworkHandler.ClientsMilkMolarUpgrades[localPlayer.actualClientId];
                }
                else
                {
                    LoggerInstance.LogDebug("getting milk molar upgrades");
                    MilkMolarUpgrades = GetUpgrades();
                    LoggerInstance.LogDebug($"Got {MilkMolarUpgrades.Count} upgrades");
                    LoggerInstance.LogDebug("getting mega milk molar upgrades");
                    MegaMilkMolarUpgrades = GetUpgrades(mega: true);
                }
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
                if (MegaMilkMolarUpgrades.Where(x => x.name == upgrade.name).FirstOrDefault() == null)
                {
                    MegaMilkMolarUpgrades.Add(upgrade);
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
                return MegaMilkMolarUpgrades.Find(x => x.name == name);
            }
            else
            {
                return MilkMolarUpgrades.Find(x => x.name == name);
            }
        }

        public static void AddMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMilkMolarsServerRpc(player.actualClientId);
            MilkMolars++;
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip("Milk Molar activated!", $"You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)", "Server"); }
        }

        public static void AddMultipleMilkMolars(int amount)
        {
            MilkMolars += amount;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"{amount} Milk Molars activated!", $"You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"{amount} Milk Molars activated! You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)", "Server"); }
            NetworkHandler.Instance.UpdateMilkMolarsServerRpc(localPlayer.actualClientId ,MilkMolars);
        }

        public static void AddMegaMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc(player.actualClientId);
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip("Mega Milk Molar activated!", $"Your group now has {NetworkHandler.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Mega Milk Molar activated! Your group now has {NetworkHandler.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)", "Server"); }
        }

        // Only runs on server
        public static void AddMultipleMegaMilkMolars(int amount)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                NetworkHandler.MegaMilkMolars.Value += amount;
                NetworkHandler.Instance.AddMultipleMegaMilkMolarsClientRpc(amount);
            }
        }

        public static bool BuyMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                if (MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    upgrade.ActivateRepeatableUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(localPlayer.actualClientId, MilkMolars);
                    return true;
                }
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    upgrade.ActivateOneTimeUpgrade();
                    NetworkHandler.Instance.UpdateMilkMolarsServerRpc(localPlayer.actualClientId, MilkMolars);
                    return true;
                }
            }

            if (!upgrade.fullyUpgraded && MilkMolars >= upgrade.nextTierCost)
            {
                MilkMolars -= upgrade.nextTierCost;
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                NetworkHandler.Instance.UpdateMilkMolarsServerRpc(localPlayer.actualClientId, MilkMolars);
                return true;
            }

            return false;
        }

        public static bool BuyMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade, bool callRPC = false)
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