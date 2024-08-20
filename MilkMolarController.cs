using BepInEx.Logging;
using GameNetcodeStuff;
using Newtonsoft.Json;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static MilkMolars.Plugin;
using static UnityEngine.ParticleSystem.PlaybackState;

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

                if (!configLGUCompatible.Value || !LGUCompatibility.enabled)
                {
                    // Fall damage reduction
                    MilkMolarUpgrade fallDamageReduction = new MilkMolarUpgrade("FallDamageReduction", "Fall Damage Reduction", MilkMolarUpgrade.UpgradeType.TierPercent, configFallDamageReductionUpgrade.Value);
                    upgrades.Add(fallDamageReduction);

                    // Shovel damage
                    MilkMolarUpgrade shovelDamage = new MilkMolarUpgrade("ShovelDamage", "Shovel Damage", MilkMolarUpgrade.UpgradeType.TierNumber, configShovelDamageUpgrade.Value);
                    upgrades.Add(shovelDamage);

                    // Sprint speed
                    MilkMolarUpgrade sprintSpeed = new MilkMolarUpgrade("SprintSpeed", "Sprint Speed", MilkMolarUpgrade.UpgradeType.TierNumber, configSprintSpeedUpgrade.Value);
                    upgrades.Add(sprintSpeed);

                    // Sprint endurance
                    MilkMolarUpgrade sprintEndurance = new MilkMolarUpgrade("SprintEndurance", "Sprint Endurance", MilkMolarUpgrade.UpgradeType.TierNumber, configSprintEnduranceUpgrade.Value);
                    upgrades.Add(sprintEndurance);

                    // Sprint regeneration // TODO: implement

                    // Jump height
                    MilkMolarUpgrade jumpHeight = new MilkMolarUpgrade("JumpHeight", "Jump Height", MilkMolarUpgrade.UpgradeType.TierNumber, configJumpHeightUpgrade.Value);
                    upgrades.Add(jumpHeight);

                    // Carry weight
                    MilkMolarUpgrade carryWeight = new MilkMolarUpgrade("CarryWeight", "Carry Weight", MilkMolarUpgrade.UpgradeType.TierPercent, configCarryWeightUpgrade.Value);
                    upgrades.Add(carryWeight);
                }
                else
                {
                    List<MilkMolarUpgrade> lguUpgrades = new List<MilkMolarUpgrade>();
                    lguUpgrades = LGUCompatibility.GetLGUUpgrades(mega);
                    upgrades.AddRange(lguUpgrades);
                }

                // Damage resistance
                MilkMolarUpgrade damageResistance = new MilkMolarUpgrade("DamageResistance", "Damage Resistance", MilkMolarUpgrade.UpgradeType.TierPercent, configDamageResistanceUpgrade.Value);
                upgrades.Add(damageResistance);

                // Increased inventory
                MilkMolarUpgrade increasedInventory = new MilkMolarUpgrade("IncreasedInventory", "Increased Inventory", MilkMolarUpgrade.UpgradeType.TierNumber, configIncreasedInventorySizeUpgrade.Value);
                upgrades.Add(increasedInventory);

                // Crit chance
                MilkMolarUpgrade critChance = new MilkMolarUpgrade("CritChance", "Crit Chance", MilkMolarUpgrade.UpgradeType.TierPercent, configCritChanceUpgrade.Value);
                upgrades.Add(critChance);

                // Climb speed
                MilkMolarUpgrade climbSpeed = new MilkMolarUpgrade("ClimbSpeed", "Climb Speed", MilkMolarUpgrade.UpgradeType.TierNumber, configClimbSpeedUpgrade.Value);
                upgrades.Add(climbSpeed);

                // Health Regen
                MilkMolarUpgrade healthRegen = new MilkMolarUpgrade("HealthRegen", "Health Regen", MilkMolarUpgrade.UpgradeType.TierNumber, configHealthRegenUpgrade.Value);
                upgrades.Add(healthRegen);

                // Bail Out
                MilkMolarUpgrade bailOut = new MilkMolarUpgrade("BailOut", "Bail Out", MilkMolarUpgrade.UpgradeType.TierPercent, configBailOutUpgrade.Value);
                upgrades.Add(bailOut);

                // Corporate Kickback
                MilkMolarUpgrade corporateKickback = new MilkMolarUpgrade("CorporateKickback", "Corporate Kickback", MilkMolarUpgrade.UpgradeType.TierPercent, configCorporateKickbackUpgrade.Value);
                upgrades.Add(corporateKickback);


                if (ExtraMilkMolarUpgrades.Count > 0)
                {
                    upgrades.AddRange(ExtraMilkMolarUpgrades);
                }

                LoggerInstance.LogDebug(upgrades.Count);
            }
            else
            {
                //// Mega Milk Molars
                if (!configLGUCompatible.Value || !LGUCompatibility.enabled)
                {
                    // Signal Transmitter Upgrade
                    MilkMolarUpgrade signalTransmitter = new MilkMolarUpgrade("SignalTransmitterUpgrade", "Signal Transmitter Upgrades", MilkMolarUpgrade.UpgradeType.OneTimeUnlock, configSignalTransmitterUpgrade.Value);
                    upgrades.Add(signalTransmitter);

                    // Increased shop deals: Increases the maximum amount of items that can be on sale in the store.

                    // Item dropship landing speed
                    MilkMolarUpgrade itemDropshipLandingSpeed = new MilkMolarUpgrade("itemDropshipLandingSpeed", "Item Dropship Landing Speed", MilkMolarUpgrade.UpgradeType.OneTimeUnlock, configItemDropshipLandingSpeedUpgrade.Value);
                    upgrades.Add(itemDropshipLandingSpeed);

                    // Travel discount
                    MilkMolarUpgrade travelDiscount = new MilkMolarUpgrade("travelDiscount", "Travel Discount", MilkMolarUpgrade.UpgradeType.TierPercent, configTravelDiscountUpgrade.Value);
                    upgrades.Add(travelDiscount);

                    // Company Cruiser health
                    // Company Cruiser acceleration
                    // Company Cruiser max speed
                    // Company Cruiser turning
                    // Company Cruiser damage reduction
                }
                else
                {
                    logger.LogDebug("Getting lguUpgrades. mega: " + mega);
                    List<MilkMolarUpgrade> lguUpgrades = new List<MilkMolarUpgrade>();
                    lguUpgrades = LGUCompatibility.GetLGUUpgrades(mega);
                    upgrades.AddRange(lguUpgrades);
                }

                // Keep items on ship chance
                MilkMolarUpgrade keepItemsOnShipChance = new MilkMolarUpgrade("keepItemsOnShipChance", "Keep Items On Ship", MilkMolarUpgrade.UpgradeType.TierPercent, configKeepItemsOnShipChanceUpgrade.Value);
                upgrades.Add(keepItemsOnShipChance);

                // Revive player
                MilkMolarUpgrade revivePlayer = new MilkMolarUpgrade("revivePlayer", "Revive Player", MilkMolarUpgrade.UpgradeType.OneTimeUnlock, configRevivePlayerUpgrade.Value);
                upgrades.Add(revivePlayer);


                if (ExtraMegaMilkMolarUpgrades.Count > 0)
                {
                    upgrades.AddRange(ExtraMegaMilkMolarUpgrades);
                }
            }

            return upgrades;
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
                        NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayerId, upgrade.LGUUpgrade);
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
                        NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.cost, localPlayerId, upgrade.LGUUpgrade);
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
                    NetworkHandler.Instance.BuyMegaMilkMolarUpgradeServerRpc(upgrade.name, upgrade.nextTierCost, localPlayerId, upgrade.LGUUpgrade);
                }
                upgrade.GoToNextTier();
                upgrade.ActivateCurrentTierUpgrade();
                return true;
            }

            logger.LogDebug("Upgrade purchase failed. Conditions not met.");
            return false;
        }

        public static void SpawnMolarsInLevel() // TODO: TEST THIS, JUST IMPLEMENTED
        {
            List<RandomScrapSpawn> spawnNodes = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>().Where(x => !x.spawnUsed).ToList();

            // Spawning Milk Molars
            List<string> levelAmount = configMilkMolarSpawnAmount.Value.Split(",").ToList();
            string? amount = levelAmount.Where(x => x.Trim().Split(":")[0] == RoundManager.Instance.currentLevel.name).FirstOrDefault();
            if (amount == null) { logger.LogError("Unable to spawn molars, couldnt find level in MilkMolarSpawnAmount config"); return; }

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