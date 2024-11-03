using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    internal class NetworkHandler : NetworkBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static NetworkHandler Instance { get; private set; } = null!;

        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[id]]; }

        public static NetworkVariable<int> MegaMilkMolars = new NetworkVariable<int>(0);

        public Sprite MilkMolarUIIcon = null!;
        public Sprite MegaMilkMolarUIIcon = null!;

        public static string MilkMolarsPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", MyPluginInfo.PLUGIN_NAME);
                return Path.Combine(path, $"MilkMolars{GameNetworkManager.Instance.saveFileNum}.json");
            }
        }
        public static string MilkMolarUpgradesPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", MyPluginInfo.PLUGIN_NAME);
                return Path.Combine(path, $"MilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json");
            }
        }
        public static string MegaMilkMolarsPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", MyPluginInfo.PLUGIN_NAME);
                return Path.Combine(path, $"MegaMilkMolars{GameNetworkManager.Instance.saveFileNum}.json");
            }
        }
        public static string MegaMilkMolarUpgradesPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", MyPluginInfo.PLUGIN_NAME);
                return Path.Combine(path, $"MegaMilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json");
            }
        }
        public static string FolderPath
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", MyPluginInfo.PLUGIN_NAME);
            }
        }


        public override void OnNetworkSpawn()
        {
            if (IsServerOrHost)
            {
                if (Instance != null)
                {
                    Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
                    logger.LogDebug("Despawned network object");
                }
            }

            Instance = this;
            logger.LogDebug("set instance to this");
            base.OnNetworkSpawn();
            logger.LogDebug("base.OnNetworkSpawn");
        }

        internal void ResetAllData()
        {
            if (IsServerOrHost)
            {
                MegaMilkMolars.Value = 0;
                ResetAllDataClientRpc();
            }
        }

        [ClientRpc]
        private void ResetAllDataClientRpc()
        {
            DeleteSaveData(GameNetworkManager.Instance.saveFileNum);
            MilkMolarController.MilkMolars = 0;
            MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetMilkMolarUpgrades();
            MilkMolarController.MegaMilkMolarUpgrades = MilkMolarController.GetMegaMilkMolarUpgrades();
        }

        internal static void SaveDataToFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (IsServerOrHost)
            {
                string megaMilkMolarsData = MegaMilkMolars.Value.ToString();
                File.WriteAllText(MegaMilkMolarsPath, megaMilkMolarsData);
            }

            string milkMolarsData = JsonConvert.SerializeObject(MilkMolarController.MilkMolars, settings);
            string milkMolarUpgradesData = JsonConvert.SerializeObject(MilkMolarController.MilkMolarUpgrades, settings);
            string megaMilkMolarUpgradesData = JsonConvert.SerializeObject(MilkMolarController.MegaMilkMolarUpgrades, settings);

            File.WriteAllText(MilkMolarsPath, milkMolarsData);
            File.WriteAllText(MilkMolarUpgradesPath, milkMolarUpgradesData);
            File.WriteAllText(MegaMilkMolarUpgradesPath, megaMilkMolarUpgradesData);
        }

        internal static void DeleteSaveData(int fileToDelete)
        {
            logger.LogDebug("Deleting Milk Molar files for save: " + fileToDelete);

            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (File.Exists(MilkMolarsPath)) { File.Delete(MilkMolarsPath); }
            if (File.Exists(MegaMilkMolarsPath)) { File.Delete(MegaMilkMolarsPath); }
            if (File.Exists(MilkMolarUpgradesPath)) { File.Delete(MilkMolarUpgradesPath); }
            if (File.Exists(MegaMilkMolarUpgradesPath)) { File.Delete(MegaMilkMolarUpgradesPath); }
        }

        internal static void LoadDataFromFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (IsServerOrHost)
            {
                if (File.Exists(MegaMilkMolarsPath))
                {
                    logger.LogDebug("Found save data for Mega Milk Molars");
                    string megaMilkMolarsData = File.ReadAllText(MegaMilkMolarsPath);
                    NetworkHandler.MegaMilkMolars.Value = int.Parse(megaMilkMolarsData);
                }
            }

            if (File.Exists(MilkMolarsPath))
            {
                logger.LogDebug("Found save data for Milk Molars");
                string milkMolarsData = File.ReadAllText(MilkMolarsPath);
                MilkMolarController.MilkMolars = int.Parse(milkMolarsData);
            }

            // Get Milk Molar Upgrades
            if (File.Exists(MilkMolarUpgradesPath))
            {
                logger.LogDebug("Found save data for Milk Molar Upgrades");
                string milkMolarUpgradesData = File.ReadAllText(MilkMolarUpgradesPath);
                MilkMolarController.MilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(milkMolarUpgradesData, settings);
            }

            if (MilkMolarController.MilkMolarUpgrades == null || MilkMolarController.MilkMolarUpgrades.Count == 0)
            {
                logger.LogDebug("MilkMolarUpgrades is null");
                MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetMilkMolarUpgrades();
                logger.LogDebug("Added data for MilkMolarUpgrades");
            }
            else
            {
                foreach (var upgrade in MilkMolarController.MilkMolarUpgrades)
                {
                    if (upgrade.Type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock && upgrade.Unlocked)
                    {
                        upgrade.ActivateOneTimeUpgrade();
                    }
                    if ((upgrade.Type == MilkMolarUpgrade.UpgradeType.TierNumber || upgrade.Type == MilkMolarUpgrade.UpgradeType.TierPercent) && upgrade.Unlocked)
                    {
                        upgrade.ActivateCurrentTierUpgrade();
                    }
                }
            }

            // Get Mega Milk Molar Upgrades
            if (File.Exists(MegaMilkMolarUpgradesPath))
            {
                logger.LogDebug("Found save data for Mega Milk Molar Upgrades");
                string megaMilkMolarUpgradesData = File.ReadAllText(MegaMilkMolarUpgradesPath);
                MilkMolarController.MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaMilkMolarUpgradesData, settings);
            }

            if (MilkMolarController.MegaMilkMolarUpgrades == null || MilkMolarController.MegaMilkMolarUpgrades.Count == 0)
            {
                logger.LogDebug("MegaMilkMolarUpgrades is null");
                MilkMolarController.MegaMilkMolarUpgrades = MilkMolarController.GetMegaMilkMolarUpgrades();
                logger.LogDebug("Added data for MegaMilkMolarUpgrades");
            }
            else
            {
                foreach (var upgrade in MilkMolarController.MegaMilkMolarUpgrades)
                {
                    if (upgrade.Type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock && upgrade.Unlocked)
                    {
                        upgrade.ActivateOneTimeUpgrade();
                    }
                    if ((upgrade.Type == MilkMolarUpgrade.UpgradeType.TierNumber || upgrade.Type == MilkMolarUpgrade.UpgradeType.TierPercent) && upgrade.Unlocked)
                    {
                        upgrade.ActivateCurrentTierUpgrade();
                    }
                }
            }

            logger.LogDebug("Finished loading data");
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarServerRpc(ulong clientId)
        {
            if (IsServerOrHost)
            {
                if (configSharedMilkMolars.Value)
                {
                    AddMilkMolarToAllClientsClientRpc();
                }
                else
                {
                    AddMilkMolarClientRpc(clientId);
                }
            }
        }

        [ClientRpc]
        private void AddMilkMolarClientRpc(ulong clientId)
        {
            if (localPlayerId == clientId)
            {
                MilkMolarController.MilkMolars++;
                MilkMolarNotificationHandler.Instance.ShowNotification(mega: false);
            }
        }

        [ClientRpc]
        private void AddMilkMolarToAllClientsClientRpc()
        {
            MilkMolarController.MilkMolars++;
            MilkMolarNotificationHandler.Instance.ShowNotification(mega: false);
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsClientRpc(ulong steamId, int amount)
        {
            if (localPlayerId == steamId)
            {
                MilkMolarController.MilkMolars += amount;
                MilkMolarNotificationHandler.Instance.ShowNotification(mega: false);
            }
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsClientRpc(int amount)
        {
            MilkMolarController.MilkMolars += amount;
            MilkMolarNotificationHandler.Instance.ShowNotification(mega: false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMegaMilkMolarServerRpc()
        {
            if (IsServerOrHost)
            {
                MegaMilkMolars.Value++;
                AddMegaMilkMolarClientRpc();
            }
        }

        [ClientRpc]
        private void AddMegaMilkMolarClientRpc()
        {
            logger.LogDebug("Added mega milk molar");
            MilkMolarNotificationHandler.Instance.ShowNotification(mega: true);
        }

        [ClientRpc]
        public void AddMultipleMegaMilkMolarsClientRpc()
        {
            MilkMolarNotificationHandler.Instance.ShowNotification(mega: true);
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyMegaMilkMolarUpgradeServerRpc(string upgradeName, int cost)
        {
            if (IsServerOrHost)
            {
                MegaMilkMolars.Value -= cost;
                BuyMegaMilkMolarUpgradeClientRpc(upgradeName);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyMegaMilkMolarUpgradeServerRpc(int cost)
        {
            if (IsServerOrHost)
            {
                MegaMilkMolars.Value -= cost;
            }
        }
        

        [ClientRpc]
        private void BuyMegaMilkMolarUpgradeClientRpc(string upgradeName)
        {
            MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName, megaUpgrade: true);

            if (upgrade != null)
            {
                if (upgrade.Type == MilkMolarUpgrade.UpgradeType.Repeatable) { upgrade.ActivateRepeatableUpgrade(); }
                else if (upgrade.Type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock) { upgrade.ActivateOneTimeUpgrade(); }
                else
                {
                    upgrade.GoToNextTier();
                    upgrade.ActivateCurrentTierUpgrade();
                }
            }
            else
            {
                logger.LogError($"Error: Invalid upgrade name: {upgradeName}");
            }
        }
    }

    [HarmonyPatch]
    internal class NetworkObjectManager
    {
        static GameObject networkPrefab;
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        public static void Init()
        {
            logger.LogDebug("Initializing network prefab...");
            if (networkPrefab != null)
                return;

            networkPrefab = (GameObject)ModAssets.LoadAsset("Assets/ModAssets/MilkMolars/NetworkHandlerMilkMolars.prefab");
            logger.LogDebug("Got networkPrefab");

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
            logger.LogDebug("Added networkPrefab");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = UnityEngine.Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                logger.LogDebug("Instantiated networkHandlerHost");
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
                logger.LogDebug("Spawned network object");
            }
        }
    }
}