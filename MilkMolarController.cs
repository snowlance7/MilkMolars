using GameNetcodeStuff;
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

        //public static Dictionary<string, MilkMolarUpgrade> MilkMolarUpgrades = new Dictionary<string, MilkMolarUpgrade>();
        //public static Dictionary<string, MilkMolarUpgrade> MegaMilkMolarUpgrades = new Dictionary<string, MilkMolarUpgrade>();

        public static List<MilkMolarUpgrade> MilkMolarUpgrades = new List<MilkMolarUpgrade>();
        public static List<MilkMolarUpgrade> MegaMilkMolarUpgrades = new List<MilkMolarUpgrade>();

        public static void Init()
        {
            MilkMolarUpgrades.Clear();

            //// Milk Molars // TODO: Sync these with host
            // Shovel damage
            MilkMolarUpgrade shovelDamage = new MilkMolarUpgrade();
            shovelDamage.name = "ShovelDamage";
            shovelDamage.title = "Shovel Damage";
            shovelDamage.type = MilkMolarUpgrade.UpgradeType.TierNumber;
            shovelDamage.GetTiersFromConfig(configShovelDamageUpgrade.Value);
            MilkMolarUpgrades.Add(shovelDamage);

            // Damage resistance
            MilkMolarUpgrade damageResistance = new MilkMolarUpgrade();
            damageResistance.name = "DamageResistance";
            damageResistance.title = "Damage Resistance";
            damageResistance.type = MilkMolarUpgrade.UpgradeType.TierPercent;
            damageResistance.GetTiersFromConfig(configDamageResistanceUpgrade.Value);
            MilkMolarUpgrades.Add(damageResistance);

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

        }

        public static void AddMilkMolar(PlayerControllerB player)
        {
            if (configSharedMilkMolars.Value)
            {
                NetworkHandler.Instance.AddMilkMolarServerRpc(player.actualClientId);
            }

            MilkMolars++;
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip("Milk Molar activated!", $"You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)", "Server"); }
        }

        public static void AddMegaMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc(player.actualClientId);
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip("Mega Milk Molar activated!", $"Your group now has {NetworkHandler.Instance.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Mega Milk Molar activated! Your group now has {NetworkHandler.Instance.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)", "Server"); }
        }

        public static void AddMultipleMilkMolars(int amount)
        {
            MilkMolars += amount;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"{amount} Milk Molars activated!", $"You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"{amount} Milk Molars activated! You now have {MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)", "Server"); }
        }

        public static void AddMultipleMegaMilkMolars(int amount)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                NetworkHandler.Instance.MegaMilkMolars.Value += amount;
                NetworkHandler.Instance.AddMultipleMegaMilkMolarsClientRpc(amount);
            }
        }

        public static bool BuyMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Infinite)
            {
                if (MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    ActivateRepeatableUpgrade(upgrade);
                    return true;
                }
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                if (MilkMolars >= upgrade.cost)
                {
                    MilkMolars -= upgrade.cost;
                    upgrade.unlocked = true;
                    return true;
                }
            }

            if (MilkMolars >= upgrade.costsPerTier[upgrade.currentTier + 1])
            {
                MilkMolars -= upgrade.costsPerTier[upgrade.currentTier + 1];
                upgrade.GoToNextTier();
                return true;
            }

            return false;
        }

        public static bool BuyMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade, bool callRPC = false)
        {
            if (upgrade.type == MilkMolarUpgrade.UpgradeType.Infinite)
            {
                if (NetworkHandler.Instance.MegaMilkMolars.Value >= upgrade.cost || callRPC == false)
                {
                    if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayer.actualClientId); }
                    ActivateRepeatableUpgrade(upgrade);
                    return true;
                }
            }

            if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                if (NetworkHandler.Instance.MegaMilkMolars.Value >= upgrade.cost || callRPC == false)
                {
                    if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayer.actualClientId); }
                    upgrade.unlocked = true;
                    return true;
                }
            }

            if (NetworkHandler.Instance.MegaMilkMolars.Value >= upgrade.costsPerTier[upgrade.currentTier + 1] || callRPC == false)
            {
                if (callRPC) { NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.costsPerTier[upgrade.currentTier + 1], localPlayer.actualClientId); }
                upgrade.GoToNextTier();
                return true;
            }

            return false;
        }

        public static bool ActivateRepeatableUpgrade(MilkMolarUpgrade upgrade)
        {


            return false;
        }
    }

    public class MilkMolarUpgrade
    {
        public const string upgradePoint = "\u2B1C";
        public const string filledUpgradePoint = "\u2B1B";
        public const string tooth = "🦷";

        public string name;
        public string title;
        public int cost;
        public UpgradeType type;

        public enum UpgradeType
        {
            TierNumber,
            TierPercent,
            OneTimeUnlock,
            Infinite
        }

        public float progress { get { return (float)currentTier / (float)maxTiers; } }

        public bool unlocked;
        public bool fullyUpgraded;

        public int currentTier = -1;
        public int maxTiers;
        public float[] amountPerTier;
        public int[] costsPerTier;

        public void GetTiersFromConfig(string configString)
        {
            string[] tiers = configString.Split(',');
            maxTiers = tiers.Length;
            costsPerTier = new int[maxTiers];
            amountPerTier = new float[maxTiers];
            for (int i = 0; i < maxTiers; i++)
            {
                string[] tierSplit = tiers[i].Split(':');
                costsPerTier[i] = int.Parse(tierSplit[0].Trim());
                amountPerTier[i] = float.Parse(tierSplit[1].Trim());
            }
        }

        public void GoToNextTier()
        {
            currentTier++;
            if (currentTier >= maxTiers)
            {
                fullyUpgraded = true;
            }
        }

        public string GetUpgradeString()
        {
            string upgradeString = "";

            switch (type)
            {
                case MilkMolarUpgrade.UpgradeType.TierNumber:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTiers - 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else if (currentTier == -1)
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"0 -> {amountPerTier[0]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"{amountPerTier[currentTier]} -> {amountPerTier[currentTier + 1]}";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.TierPercent:
                    if (fullyUpgraded)
                    {
                        upgradeString = $"{title} (Fully Upgraded) " +
                            $"{amountPerTier[maxTiers - 1]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else if (currentTier == -1)
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"0% -> {amountPerTier[0]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    else
                    {
                        upgradeString = $"{costsPerTier[currentTier + 1]}{tooth} " +
                            $"{title}: " +
                            $"{amountPerTier[currentTier]}% -> {amountPerTier[currentTier + 1]}%";
                        upgradeString += $"\n{GetUpgradeSymbols()}";
                    }
                    break;
                case MilkMolarUpgrade.UpgradeType.OneTimeUnlock:
                    if (fullyUpgraded) { upgradeString = $"{title} (Fully Upgraded)"; }
                    else { upgradeString = $"{cost}{tooth} {title}"; }
                    break;
                case MilkMolarUpgrade.UpgradeType.Infinite:
                    upgradeString = $"{cost}{tooth} {title} (Repeatable)";
                    break;
                default:
                    break;
            }

            return upgradeString;
        }

        public string GetUpgradeSymbols()
        {
            string text = "";
            if (currentTier == -1)
            {
                for (int i = 0; i < maxTiers; i++)
                {
                    text += upgradePoint;
                }
            }
            else
            {
                for (int i = 0; i < currentTier; i++)
                {
                    text += filledUpgradePoint;
                }
                for (int i = currentTier; i < maxTiers; i++)
                {
                    text += upgradePoint;
                }
            }
            return text;
        }
    }
}