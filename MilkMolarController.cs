using BepInEx.Logging;
using GameNetcodeStuff;
using MilkMolars.Upgrades;
using Newtonsoft.Json;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static MilkMolars.Plugin;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace MilkMolars
{
    public static class MilkMolarController
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        internal static int MilkMolars = 0;

        internal static bool InUpgradeUI = false;
        internal static bool InMegaUpgradeUI = false;

        public static List<MilkMolarUpgrade> MilkMolarUpgrades;
        public static List<MilkMolarUpgrade> MegaMilkMolarUpgrades;

        public static List<MilkMolarUpgrade> ExtraMilkMolarUpgrades;
        public static List<MilkMolarUpgrade> ExtraMegaMilkMolarUpgrades;

        public static List<MilkMolarUpgrade> AllMilkMolarUpgrades { get { return MilkMolarUpgrades.Concat(MegaMilkMolarUpgrades).ToList(); } }

        public static int GetMolarPrice(int price, bool shared)
        {
            if (shared)
            {
                return (int)Math.Ceiling(price / configLGUMegaMilkMolarContributeAmount.Value);
            }
            else
            {
                return (int)Math.Ceiling(price / configLGUMilkMolarContributeAmount.Value);
            }
        }

        public static int GetCurrentMolarCount(bool shared)
        {
            if (shared)
            {
                return NetworkHandler.MegaMilkMolars.Value;
            }
            else
            {
                return MilkMolars;
            }
        }

        internal static void Init()
        {
            LoggerInstance.LogDebug("Initing milk molar controller");

            NetworkHandler.LoadDataFromFile();
            MilkMolarNotificationHandler.GetUIIcons();
        }

        internal static List<MilkMolarUpgrade> GetMilkMolarUpgrades() // TODO: Implement commented out upgrades
        {
            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();


            // Fall damage reduction
            upgrades.Add(new DaredevilUpgrade());

            // Sprint speed
            //upgrades.Add(new MilkMolarUpgrade("SprintSpeed", "Sprint Speed", MilkMolarUpgrade.UpgradeType.TierNumber, configSprintSpeedUpgrade.Value));

            // Sprint endurance
            //upgrades.Add(new MilkMolarUpgrade("SprintEndurance", "Sprint Endurance", MilkMolarUpgrade.UpgradeType.TierNumber, configSprintEnduranceUpgrade.Value));

            // Sprint regeneration // TODO: implement

            // Jump height
            //upgrades.Add(new MilkMolarUpgrade("JumpHeight", "Jump Height", MilkMolarUpgrade.UpgradeType.TierNumber, configJumpHeightUpgrade.Value));

            // Carry weight
            //upgrades.Add(new MilkMolarUpgrade("CarryWeight", "Carry Weight", MilkMolarUpgrade.UpgradeType.TierPercent, configCarryWeightUpgrade.Value));

            // Shovel damage
            //upgrades.Add(new MilkMolarUpgrade("ShovelDamage", "Shovel Damage", MilkMolarUpgrade.UpgradeType.TierNumber, configShovelDamageUpgrade.Value));

            // Damage resistance
            upgrades.Add(new DamageResistanceUpgrade());

            // Increased inventory
            //upgrades.Add(new MilkMolarUpgrade("IncreasedInventory", "Increased Inventory", MilkMolarUpgrade.UpgradeType.TierNumber, configIncreasedInventorySizeUpgrade.Value));

            // Crit chance
            //upgrades.Add(new MilkMolarUpgrade("CritChance", "Crit Chance", MilkMolarUpgrade.UpgradeType.TierPercent, configCritChanceUpgrade.Value));

            // Climb speed
            //upgrades.Add(new MilkMolarUpgrade("ClimbSpeed", "Climb Speed", MilkMolarUpgrade.UpgradeType.TierNumber, configClimbSpeedUpgrade.Value));

            // Health Regen
            //upgrades.Add(new MilkMolarUpgrade("HealthRegen", "Health Regen", MilkMolarUpgrade.UpgradeType.TierNumber, configHealthRegenUpgrade.Value));

            // Bail Out
            //upgrades.Add(new MilkMolarUpgrade("BailOut", "Bail Out", MilkMolarUpgrade.UpgradeType.TierPercent, configBailOutUpgrade.Value));

            // Corporate Kickback
            //upgrades.Add(new MilkMolarUpgrade("CorporateKickback", "Corporate Kickback", MilkMolarUpgrade.UpgradeType.TierPercent, configCorporateKickbackUpgrade.Value));


            /*if (ExtraMilkMolarUpgrades.Count > 0)
            {
                upgrades.AddRange(ExtraMilkMolarUpgrades);
            }*/

            LoggerInstance.LogDebug("Got milk molar upgrades: " + upgrades.Count);

            return upgrades;
        }

        internal static List<MilkMolarUpgrade> GetMegaMilkMolarUpgrades() // TODO: Implement commented out upgrades
        {
            List<MilkMolarUpgrade> upgrades = new List<MilkMolarUpgrade>();

            //// Mega Milk Molars

            // Signal Transmitter Upgrade
            //MilkMolarUpgrade signalTransmitter = new MilkMolarUpgrade("SignalTransmitterUpgrade", "Signal Transmitter Upgrades", MilkMolarUpgrade.UpgradeType.OneTimeUnlock, configSignalTransmitterUpgrade.Value);
            //upgrades.Add(signalTransmitter);

            // Increased shop deals: Increases the maximum amount of items that can be on sale in the store.

            //upgrades.Add(new ItemDropshipLandingSpeedUpgrade());

            // Travel discount
            //MilkMolarUpgrade travelDiscount = new MilkMolarUpgrade("travelDiscount", "Travel Discount", MilkMolarUpgrade.UpgradeType.TierPercent, configTravelDiscountUpgrade.Value);
            //upgrades.Add(travelDiscount);

            // Company Cruiser health
            // Company Cruiser acceleration
            // Company Cruiser max speed
            // Company Cruiser turning
            // Company Cruiser damage reduction

            // Keep items on ship chance
            upgrades.Add(new KeepItemsOnShipChanceUpgrade());

            upgrades.Add(new RevivePlayerUpgrade());


            /*if (ExtraMegaMilkMolarUpgrades.Count > 0)
            {
                upgrades.AddRange(ExtraMegaMilkMolarUpgrades);
            }*/

            logger.LogDebug("Got mega milk molar upgrades: " + upgrades.Count);

            return upgrades;
        }

        /*public static void RegisterMilkMolarUpgrade(MilkMolarUpgrade upgrade)
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
        }*/

        public static MilkMolarUpgrade GetUpgradeByName(string name, bool megaUpgrade = false)
        {
            if (megaUpgrade)
            {
                return MegaMilkMolarUpgrades.Find(x => x.Name == name);
            }
            else
            {
                return MilkMolarUpgrades.Find(x => x.Name == name);
            }
        }

        internal static void AddMilkMolar(PlayerControllerB player)
        {
            NetworkHandler.Instance.AddMilkMolarServerRpc(player.actualClientId); // TODO: Change this to steamId later
        }

        internal static void AddMegaMilkMolar()
        {
            NetworkHandler.Instance.AddMegaMilkMolarServerRpc();
        }

        internal static bool BuyMilkMolarUpgrade(MilkMolarUpgrade upgrade)
        {
            logger.LogDebug("Attempting to buy Milk Molar upgrade: " + upgrade.Name);

            if (upgrade.Type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                logger.LogDebug("Upgrade type is Repeatable. Checking if we have enough Milk Molars.");
                if (MilkMolars >= upgrade.UnlockCost)
                {
                    MilkMolars -= upgrade.UnlockCost;
                    logger.LogDebug("Conditions met. Activating Repeatable upgrade and updating server.");
                    upgrade.ActivateRepeatableUpgrade();
                    return true;
                }
                logger.LogDebug("Not enough Milk Molars for Repeatable upgrade.");
                return false;
            }

            if (upgrade.Type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                logger.LogDebug("Upgrade type is OneTimeUnlock. Checking if upgrade is not fully upgraded and if we have enough Milk Molars.");
                if (!upgrade.FullyUpgraded && MilkMolars >= upgrade.UnlockCost)
                {
                    MilkMolars -= upgrade.UnlockCost;
                    logger.LogDebug("Conditions met. Activating OneTimeUnlock upgrade and updating server.");
                    upgrade.ActivateOneTimeUpgrade();
                    return true;
                }
                logger.LogDebug("Not enough Milk Molars or upgrade is fully upgraded.");
                return false;
            }

            logger.LogDebug("Checking if upgrade is not fully upgraded and if we have enough Milk Molars for next tier.");
            logger.LogDebug("Current Tier: " + upgrade.CurrentTier);
            if (!upgrade.FullyUpgraded && MilkMolars >= upgrade.NextTierCost)
            {
                MilkMolars -= upgrade.NextTierCost;
                logger.LogDebug("Conditions met. Going to next tier, activating current tier upgrade, and updating server.");
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                return true;
            }

            logger.LogDebug("Upgrade purchase failed. Conditions not met.");
            return false;
        }

        internal static bool BuyMegaMilkMolarUpgrade(MilkMolarUpgrade upgrade) // THIS IS GOOD DONT TOUCH IT
        {
            logger.LogDebug("Attempting to buy Mega Milk Molar upgrade: " + upgrade.Name);
            int megaMilkMolars = NetworkHandler.MegaMilkMolars.Value;

            if (upgrade.Type == MilkMolarUpgrade.UpgradeType.Repeatable)
            {
                logger.LogDebug("Upgrade type is Repeatable. Checking if we have enough Mega Milk Molars");
                if (megaMilkMolars >= upgrade.UnlockCost)
                {
                    logger.LogDebug("Conditions met. Activating Repeatable upgrade.");
                    NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.Name, upgrade.UnlockCost);
                    return true;
                }
                return false;
            }

            if (upgrade.Type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock)
            {
                logger.LogDebug("Upgrade type is OneTimeUnlock. Checking if upgrade is not fully upgraded and if we have enough Mega Milk Molars or RPC is not required.");
                if (!upgrade.FullyUpgraded && megaMilkMolars >= upgrade.UnlockCost)
                {
                    logger.LogDebug("Conditions met. Activating OneTimeUnlock upgrade.");
                    NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.Name, upgrade.UnlockCost);
                    return true;
                }
                return false;
            }

            logger.LogDebug("Checking if upgrade is not fully upgraded and if we have enough Mega Milk Molars or RPC is not required.");
            logger.LogDebug("Current Tier: " + upgrade.CurrentTier);
            if ((!upgrade.FullyUpgraded && megaMilkMolars >= upgrade.NextTierCost))
            {
                logger.LogDebug("Conditions met. Going to next tier and activating current tier upgrade.");

                NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.Name, upgrade.NextTierCost);
                return true;
            }

            return false;
        }

        internal static void SpawnMolarsInLevel()
        {
            List<RandomScrapSpawn> spawnNodes = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(x => !x.spawnUsed).ToList();

            // Spawning Milk Molars
            List<string> levelAmount = configMilkMolarSpawnAmount.Value.Split(",").ToList();
            string? amount = levelAmount.Where(x => x.Trim().Split(":")[0] == RoundManager.Instance.currentLevel.name).FirstOrDefault();
            if (amount == null)
            {
                amount = levelAmount.Where(x => x.Trim().Split(":")[0] == "All").FirstOrDefault();
                if (amount == null) { logger.LogDebug("Couldnt find amount in amount config. Not spawning."); return; }
            }

            string[] minmax = amount.Trim().Split(":")[1].Split("-");
            int min = int.Parse(minmax[0]);
            int max = int.Parse(minmax[1]);
            int spawnAmount = UnityEngine.Random.Range(min, max + 1);

            for (int i = 0; i < spawnAmount; i++)
            {
                if (spawnNodes.Count <= 0) break;

                int index = UnityEngine.Random.Range(0, spawnNodes.Count);
                RandomScrapSpawn randomScrapSpawn = spawnNodes[index];
                UnityEngine.Vector3 vector = randomScrapSpawn.transform.position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    spawnNodes.RemoveAt(index);
                }
                else
                {
                    vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit);
                }
                // spawn item at random position
                Item molar = LethalLib.Modules.Items.LethalLibItemList.Where(x => x.name == "MilkMolarItem").FirstOrDefault();
                if (molar == null) { logger.LogError("Unable to find molar in LethalLibItemList"); return; }
                GameObject gameObject = UnityEngine.Object.Instantiate(molar.spawnPrefab, vector, UnityEngine.Quaternion.identity, StartOfRound.Instance.propsContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }

            // Spawning Mega Milk Molars
            levelAmount = configMegaMilkMolarSpawnAmount.Value.Split(",").ToList();
            amount = levelAmount.Where(x => x.Trim().Split(":")[0] == RoundManager.Instance.currentLevel.name).FirstOrDefault();
            if (amount == null) { logger.LogError("Unable to spawn molars, couldnt find level in MegaMilkMolarSpawnAmount config"); return; }

            minmax = amount.Trim().Split(":")[1].Split("-");
            min = int.Parse(minmax[0]);
            max = int.Parse(minmax[1]);
            spawnAmount = UnityEngine.Random.Range(min, max + 1);

            for (int i = 0; i < spawnAmount; i++)
            {
                if (spawnNodes.Count <= 0) break;

                int index = UnityEngine.Random.Range(0, spawnNodes.Count);
                RandomScrapSpawn randomScrapSpawn = spawnNodes[index];
                UnityEngine.Vector3 vector = randomScrapSpawn.transform.position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    spawnNodes.RemoveAt(index);
                }
                else
                {
                    vector = RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, RoundManager.Instance.navHit);
                }
                // spawn item at random position
                Item molar = LethalLib.Modules.Items.LethalLibItemList.Where(x => x.name == "MegaMilkMolarItem").FirstOrDefault();
                if (molar == null) { logger.LogError("Unable to find molar in LethalLibItemList"); return; }
                GameObject gameObject = UnityEngine.Object.Instantiate(molar.spawnPrefab, vector, UnityEngine.Quaternion.identity, StartOfRound.Instance.propsContainer);
                gameObject.GetComponent<GrabbableObject>().fallTime = 0f;
                gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}