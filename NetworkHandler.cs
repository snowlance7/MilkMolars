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
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
                logger.LogDebug("Despawned network object");
            }

            Instance = this;
            logger.LogDebug("set instance to this");
            base.OnNetworkSpawn();
            logger.LogDebug("base.OnNetworkSpawn");
        }

        internal void ResetAllData()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
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

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                string megaMilkMolarsData = MegaMilkMolars.Value.ToString();
                string megaMilkMolarUpgradesData = JsonConvert.SerializeObject(MilkMolarController.MegaMilkMolarUpgrades, settings);
                File.WriteAllText(MegaMilkMolarsPath, megaMilkMolarsData);
                File.WriteAllText(MegaMilkMolarUpgradesPath, megaMilkMolarUpgradesData);
            }

            string milkMolarsData = JsonConvert.SerializeObject(MilkMolarController.MilkMolars, settings);
            string milkMolarUpgradesData = JsonConvert.SerializeObject(MilkMolarController.MilkMolarUpgrades, settings);

            File.WriteAllText(MilkMolarsPath, milkMolarsData);
            File.WriteAllText(MilkMolarUpgradesPath, milkMolarUpgradesData);
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

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (File.Exists(MegaMilkMolarsPath))
                {
                    logger.LogDebug("Found save data for Mega Milk Molars");
                    string megaMilkMolarsData = File.ReadAllText(MegaMilkMolarsPath);
                    NetworkHandler.MegaMilkMolars.Value = int.Parse(megaMilkMolarsData);
                }
                if (File.Exists(MegaMilkMolarUpgradesPath))
                {
                    logger.LogDebug("Found save data for Mega Milk Molar Upgrades");
                    string megaMilkMolarUpgradesData = File.ReadAllText(MegaMilkMolarUpgradesPath);
                    MilkMolarController.MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaMilkMolarUpgradesData, settings);
                }

                if (MilkMolarController.MegaMilkMolarUpgrades == null || MilkMolarController.MegaMilkMolarUpgrades.Count == 0)
                {
                    logger.LogDebug("MegaMilkMolarUpgrades is null");
                    MilkMolarController.GetMegaMilkMolarUpgrades();
                    logger.LogDebug("Added data for MegaMilkMolarUpgrades");
                }
                else
                {
                    foreach (var upgrade in MilkMolarController.MegaMilkMolarUpgrades)
                    {
                        if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock && upgrade.unlocked)
                        {
                            upgrade.ActivateOneTimeUpgrade();
                        }
                        if ((upgrade.type == MilkMolarUpgrade.UpgradeType.TierNumber || upgrade.type == MilkMolarUpgrade.UpgradeType.TierPercent) && upgrade.unlocked)
                        {
                            upgrade.ActivateCurrentTierUpgrade();
                        }
                    }
                }
            }

            if (File.Exists(MilkMolarsPath))
            {
                logger.LogDebug("Found save data for Milk Molars");
                string milkMolarsData = File.ReadAllText(MilkMolarsPath);
                MilkMolarController.MilkMolars = int.Parse(milkMolarsData);
            }
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
                    if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock && upgrade.unlocked)
                    {
                        upgrade.ActivateOneTimeUpgrade();
                    }
                    if ((upgrade.type == MilkMolarUpgrade.UpgradeType.TierNumber || upgrade.type == MilkMolarUpgrade.UpgradeType.TierPercent) && upgrade.unlocked)
                    {
                        upgrade.ActivateCurrentTierUpgrade();
                    }
                }
            }

            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                MilkMolarController.GetMegaMilkMolarUpgrades();
            }

            logger.LogDebug("Finished loading data");
        }

        [ServerRpc(RequireOwnership = false)]
        public void GetMegaMilkMolarUpgradesServerRpc(ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                string megaUpgrades = JsonConvert.SerializeObject(MilkMolarController.MegaMilkMolarUpgrades, settings);
                Instance.SendMegaMilkMolarUpgradesClientRpc(steamId, megaUpgrades);
            }
        }

        [ClientRpc]
        public void SendMegaMilkMolarUpgradesClientRpc(ulong steamId, string megaUpgrades)
        {
            if (localPlayerId == steamId)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                MilkMolarController.MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings);
            }
        }

        [ClientRpc]
        public void SendMegaMilkMolarUpgradesToAllClientRpc(string megaUpgrades)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            MilkMolarController.MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarServerRpc(ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (configSharedMilkMolars.Value)
                {
                    AddMilkMolarToAllClientsClientRpc();
                }
                else
                {
                    AddMilkMolarClientRpc(steamId);
                }
            }
        }

        [ClientRpc]
        private void AddMilkMolarClientRpc(ulong steamId)
        {
            if (localPlayerId == steamId)
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
        public void AddMultipleMilkMolarsAllClientsClientRpc(int amount)
        {
            MilkMolarController.MilkMolars += amount;
            MilkMolarNotificationHandler.Instance.ShowNotification(mega: false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMegaMilkMolarServerRpc()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
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
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value -= cost;
                MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName, megaUpgrade: true);

                if (upgrade != null)
                {
                    if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable) { upgrade.ActivateRepeatableUpgrade(); }
                    else if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock) { upgrade.ActivateOneTimeUpgrade(); }
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
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
            {
                MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName, megaUpgrade: true);

                if (upgrade != null)
                {
                    if (upgrade.type == MilkMolarUpgrade.UpgradeType.Repeatable) { return; }
                    else if (upgrade.type == MilkMolarUpgrade.UpgradeType.OneTimeUnlock) { upgrade.unlocked = true; }
                    else
                    {
                        upgrade.GoToNextTier();
                    }
                }
                else
                {
                    logger.LogError($"Error: Invalid upgrade name: {upgradeName}");
                }
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