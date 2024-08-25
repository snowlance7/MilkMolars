using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static MilkMolars.Plugin;

namespace MilkMolars
{
    public class NetworkHandler : NetworkBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static NetworkHandler Instance { get; private set; }

        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[id]]; }

        public static NetworkVariable<int> MegaMilkMolars = new NetworkVariable<int>(0);

        public static List<MilkMolarUpgrade> MegaMilkMolarUpgrades;


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

        internal static void DeleteSaveData(int fileToDelete)
        {
            logger.LogDebug("Deleting Milk Molar files for save: " + fileToDelete);

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            logger.LogDebug("Got appData path at: " + appDataPath);
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);
            logger.LogDebug("Got save path at: " + path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string milkMolarsPath = Path.Combine(path, $"MilkMolars{fileToDelete}.json");
            string megaMilkMolarsPath = Path.Combine(path, $"MegaMilkMolars{fileToDelete}.json");
            string milkMolarUpgradesPath = Path.Combine(path, $"MilkMolarUpgrades{fileToDelete}.json");
            string megaMilkMolarUpgradesPath = Path.Combine(path, $"MegaMilkMolarUpgrades{fileToDelete}.json");

            if (File.Exists(milkMolarsPath)) { File.Delete(milkMolarsPath); }
            if (File.Exists(megaMilkMolarsPath)) { File.Delete(megaMilkMolarsPath); }
            if (File.Exists(milkMolarUpgradesPath)) { File.Delete(milkMolarUpgradesPath); }
            if (File.Exists(megaMilkMolarUpgradesPath)) { File.Delete(megaMilkMolarUpgradesPath); }
        }

        public static void ResetAllData()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value = 0;
            }

            DeleteSaveData(GameNetworkManager.Instance.saveFileNum);
            Instance.ResetAllDataClientRpc();
        }

        [ClientRpc]
        private void ResetAllDataClientRpc()
        {
            MilkMolarController.MilkMolars = 0;
            MegaMilkMolarUpgrades = MilkMolarController.GetUpgrades(mega: true);
            MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetUpgrades();
        }

        public static void SaveDataToFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                string megaMilkMolarsData = MegaMilkMolars.Value.ToString();
                string megaMilkMolarUpgradesData = JsonConvert.SerializeObject(MegaMilkMolarUpgrades.Where(x => !x.LGUUpgrade), settings);
                File.WriteAllText(Path.Combine(path, $"MegaMilkMolars{GameNetworkManager.Instance.saveFileNum}.json"), megaMilkMolarsData);
                File.WriteAllText(Path.Combine(path, $"MegaMilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json"), megaMilkMolarUpgradesData);
            }

            string milkMolarsData = JsonConvert.SerializeObject(MilkMolarController.MilkMolars, settings);
            string milkMolarUpgradesData = JsonConvert.SerializeObject(MilkMolarController.MilkMolarUpgrades.Where(x => !x.LGUUpgrade), settings);

            File.WriteAllText(Path.Combine(path, $"MilkMolars{GameNetworkManager.Instance.saveFileNum}.json"), milkMolarsData);
            File.WriteAllText(Path.Combine(path, $"MilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json"), milkMolarUpgradesData);
        }

        public static void LoadDataFromFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string milkMolarsPath = Path.Combine(path, $"MilkMolars{GameNetworkManager.Instance.saveFileNum}.json");
            string milkMolarUpgradesPath = Path.Combine(path, $"MilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json");

            if (File.Exists(milkMolarsPath))
            {
                logger.LogDebug("Found save data for Milk Molars");
                string milkMolarsData = File.ReadAllText(milkMolarsPath);
                MilkMolarController.MilkMolars = int.Parse(milkMolarsData);
            }
            if (File.Exists(milkMolarUpgradesPath))
            {
                logger.LogDebug("Found save data for Milk Molar Upgrades");
                string milkMolarUpgradesData = File.ReadAllText(milkMolarUpgradesPath);
                MilkMolarController.MilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(milkMolarUpgradesData, settings);
            }

            if (MilkMolarController.MilkMolarUpgrades == null || MilkMolarController.MilkMolarUpgrades.Count == 0)
            {
                logger.LogDebug("MilkMolarUpgrades is null");
                MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetUpgrades();
                logger.LogDebug("Added data for MilkMolarUpgrades");

                if (LGUCompatibility.enabled) { MilkMolarController.MilkMolarUpgrades.AddRange(LGUCompatibility.GetLGUUpgrades()); }
            }
            else
            {
                foreach (var upgrade in MilkMolarController.MilkMolarUpgrades)
                {
                    if (!upgrade.LGUUpgrade)
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

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                string megaMilkMolarsPath = Path.Combine(path, $"MegaMilkMolars{GameNetworkManager.Instance.saveFileNum}.json");
                string megaMilkMolarUpgradesPath = Path.Combine(path, $"MegaMilkMolarUpgrades{GameNetworkManager.Instance.saveFileNum}.json");

                if (File.Exists(megaMilkMolarsPath))
                {
                    logger.LogDebug("Found save data for Mega Milk Molars");
                    string megaMilkMolarsData = File.ReadAllText(megaMilkMolarsPath);
                    MegaMilkMolars.Value = int.Parse(megaMilkMolarsData);
                }
                if (File.Exists(megaMilkMolarUpgradesPath))
                {
                    logger.LogDebug("Found save data for Mega Milk Molar Upgrades");
                    string megaMilkMolarUpgradesData = File.ReadAllText(megaMilkMolarUpgradesPath);
                    MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaMilkMolarUpgradesData, settings);

                    if (LGUCompatibility.enabled) { MegaMilkMolarUpgrades.AddRange(LGUCompatibility.GetLGUUpgrades(mega: true)); }
                }

                if (MegaMilkMolarUpgrades == null || MegaMilkMolarUpgrades.Count == 0)
                {
                    logger.LogDebug("MegaMilkMolarUpgrades is null");
                    MegaMilkMolarUpgrades = MilkMolarController.GetUpgrades(mega: true);
                    logger.LogDebug("Added data for MegaMilkMolarUpgrades");
                }
            }
            else
            {
                Instance.GetDataFromServerServerRpc(localPlayerId);
            }

            logger.LogDebug("Finished loading data");
        }

        [ServerRpc(RequireOwnership = false)]
        public void GetDataFromServerServerRpc(ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                SendDataToClient(steamId);
            }
        }

        public static void SendDataToClient(ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                string megaUpgrades = JsonConvert.SerializeObject(MegaMilkMolarUpgrades, settings);
                Instance.SendDataToClientClientRpc(steamId, megaUpgrades);
            }
        }

        [ClientRpc]
        private void SendDataToClientClientRpc(ulong steamId, string megaUpgrades)
        {
            if (localPlayerId == steamId)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings);

                if (LGUCompatibility.enabled) { MegaMilkMolarUpgrades.AddRange(LGUCompatibility.GetLGUUpgrades(mega: true)); }
            }
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
                if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
                HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server");
            }
        }

        [ClientRpc]
        private void AddMilkMolarToAllClientsClientRpc()
        {
            MilkMolarController.MilkMolars++;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server");
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsClientRpc(ulong steamId, int amount)
        {
            if (localPlayerId == steamId)
            {
                MilkMolarController.MilkMolars += amount;
                if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
                HUDManager.Instance.AddChatMessage($"{amount} Milk Molars activated! Your now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server");
            }
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsAllClientsClientRpc(int amount)
        {
            MilkMolarController.MilkMolars += amount;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
            HUDManager.Instance.AddChatMessage($"{MegaMilkMolars.Value} Mega Milk Molars activated! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server");
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
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            HUDManager.Instance.AddChatMessage($"Mega Milk Molar activated! Your group now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server");
        }

        [ClientRpc]
        public void AddMultipleMegaMilkMolarsClientRpc(int amount)
        {
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
            HUDManager.Instance.AddChatMessage($"{MegaMilkMolars.Value} Mega Milk Molars activated! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server");
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyMegaMilkMolarUpgradeServerRpc(string upgradeName, int cost, ulong steamId, bool lguUpgrade)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value -= cost;
                if (!lguUpgrade) { BuyMegaMilkMolarUpgradeClientRpc(upgradeName, steamId); }
            }
        }

        [ClientRpc]
        public void BuyMegaMilkMolarUpgradeClientRpc(string upgradeName, ulong steamId)
        {
            if (localPlayerId == steamId) { return; }
            logger.LogDebug("Buying mega milk molar upgrade");
            MilkMolarUpgrade upgrade = MilkMolarController.GetUpgradeByName(upgradeName);
            MilkMolarController.BuyMegaMilkMolarUpgrade(upgrade, callRPC: false);
        }
    }

    [HarmonyPatch]
    public class NetworkObjectManager
    {
        static GameObject networkPrefab;
        private static ManualLogSource logger = Plugin.LoggerInstance;

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
        public static void Init()
        {
            logger.LogDebug("Initializing network prefab...");
            if (networkPrefab != null)
                return;

            networkPrefab = (GameObject)Plugin.ModAssets.LoadAsset("Assets/ModAssets/MilkMolars/NetworkHandlerMilkMolars.prefab");
            logger.LogDebug("Got networkPrefab");
            networkPrefab.AddComponent<NetworkHandler>();
            logger.LogDebug("Added component");

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