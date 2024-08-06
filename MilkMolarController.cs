using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    public static class MilkMolarController
    {
        public static int MilkMolars = 0;

        public static Dictionary<string, MilkMolarUpgrade> MilkMolarUpgrades = new Dictionary<string, MilkMolarUpgrade>();
        public static Dictionary<string, MilkMolarUpgrade> MegaMilkMolarUpgrades = new Dictionary<string, MilkMolarUpgrade>();

        public static void Init()
        {
            MilkMolarUpgrades.Clear();
            MegaMilkMolarUpgrades.Clear();

            //// Milk Molars
            // Shovel damage
            MilkMolarUpgrade shovelDamage = new MilkMolarUpgrade();
            shovelDamage.type = MilkMolarUpgrade.UpgradeType.Int;
            shovelDamage

            // Damage resistance
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
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip("Mega Milk Molar activated!", $"Your group now has {NetworkHandler.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Mega Milk Molar activated! Your group now has {NetworkHandler.MegaMilkMolars} unspent Mega Milk Molars. Open the upgrade menu to spend Mega Milk Molars for the group. (M by default)", "Server"); }
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
                NetworkHandler.MegaMilkMolars.Value += amount;
                NetworkHandler.Instance.AddMultipleMegaMilkMolarsClientRpc(amount);
            }
        }

        public static void BuyMilkMolarUpgrade(string upgradeName)
        {
            MilkMolarUpgrades[upgradeName]++;
        }
    }

    public class MilkMolarUpgrade
    {
        public int cost;
        public UpgradeType type;

        public enum UpgradeType
        {
            Unlock,
            Percent,
            Int,
            Float
        }

        public bool unlocked;

        public int currentTier;
        public int maxTiers;
        public float[] amountPerTier;
        public int[] costsPerTier;
    }
}