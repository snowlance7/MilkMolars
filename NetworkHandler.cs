using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using static Netcode.Transports.Facepunch.FacepunchTransport;
using static MilkMolars.Plugin;
using Newtonsoft.Json;
using Steamworks.Data;
using System.IO;
using System.Threading;

namespace MilkMolars
{
    public class NetworkHandler : NetworkBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static NetworkHandler Instance { get; private set; }

        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[id]]; }

        public static NetworkVariable<int> MegaMilkMolars = new NetworkVariable<int>(0);

        public static List<MilkMolarUpgrade> MegaMilkMolarUpgrades;

        public static Dictionary<ulong, int> ClientsMilkMolars = new Dictionary<ulong, int>();

        public static Dictionary<ulong, List<MilkMolarUpgrade>> ClientsMilkMolarUpgrades = new Dictionary<ulong, List<MilkMolarUpgrade>>(); // TODO: Get these


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

        public static void UpdateClientsMilkMolars(ulong steamId, int amount, bool adding = false)
        {
            if (ClientsMilkMolars.ContainsKey(steamId))
            {
                if (adding) { ClientsMilkMolars[steamId] += amount; }
                else { ClientsMilkMolars[steamId] = amount; }
            }
            else
            {
                ClientsMilkMolars.Add(steamId, amount);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMilkMolarsServerRpc(int newAmount, ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                UpdateClientsMilkMolars(steamId, newAmount);
            }
        }

        public static void ResetAllData()
        {
            MegaMilkMolars.Value = 0;
            ClientsMilkMolars = new Dictionary<ulong, int>();
            ClientsMilkMolarUpgrades = new Dictionary<ulong, List<MilkMolarUpgrade>>();
            List<MilkMolarUpgrade> upgrades = MilkMolarController.GetUpgrades();

            foreach(var player in StartOfRound.Instance.allPlayerScripts)
            {
                ClientsMilkMolars.Add(player.playerSteamId, 0);
                ClientsMilkMolarUpgrades.Add(player.playerSteamId, upgrades);
            }

            SaveDataToFile();
            Instance.ResetAllDataClientRpc();
        }

        [ClientRpc]
        private void ResetAllDataClientRpc()
        {
            MegaMilkMolarUpgrades = new List<MilkMolarUpgrade>();
            MegaMilkMolarUpgrades = MilkMolarController.GetUpgrades(mega: true);
            MilkMolarController.MilkMolars = 0;
            MilkMolarController.MilkMolarUpgrades = new List<MilkMolarUpgrade>();
            MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetUpgrades();
        }

        public static void SaveDataToFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            logger.LogDebug("Got appData path at: " + appDataPath); // C:\Users\snowl\AppData\Local // C:\Users\snowl\AppData\LocalLow\ZeekerssRBLX\Lethal Company
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);
            logger.LogDebug("Got save path at: " + path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string milkMolarsData = JsonConvert.SerializeObject(ClientsMilkMolars, settings);
            string megaMilkMolarsData = MegaMilkMolars.Value.ToString();
            string milkMolarUpgradesData = JsonConvert.SerializeObject(ClientsMilkMolarUpgrades, settings);
            string megaMilkMolarUpgradesData = JsonConvert.SerializeObject(MegaMilkMolarUpgrades, settings);

            File.WriteAllText(Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolars.json"), milkMolarsData);
            File.WriteAllText(Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolars.json"), megaMilkMolarsData);
            File.WriteAllText(Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolarUpgrades.json"), milkMolarUpgradesData);
            File.WriteAllText(Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolarUpgrades.json"), megaMilkMolarUpgradesData);

            logger.LogDebug("Finished saving data to " + path);
        }

        public static void LoadDataFromFile()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            logger.LogDebug("Got appData path at: " + appDataPath); // C:\Users\snowl\AppData\Local // C:\Users\snowl\AppData\LocalLow\ZeekerssRBLX\Lethal Company
            string path = Path.Combine(appDataPath + "Low", "ZeekerssRBLX", "Lethal Company", modName);
            logger.LogDebug("Got save path at: " + path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string milkMolarsPath = Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolars.json");
            string megaMilkMolarsPath = Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolars.json");
            string milkMolarUpgradesPath = Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolarUpgrades.json");
            string megaMilkMolarUpgradesPath = Path.Combine(path, $"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolarUpgrades.json");

            if (File.Exists(milkMolarsPath))
            {
                logger.LogDebug("Found save data for Milk Molars");
                string milkMolarsData = File.ReadAllText(milkMolarsPath);
                ClientsMilkMolars = JsonConvert.DeserializeObject<Dictionary<ulong, int>>(milkMolarsData, settings);
            }
            if (File.Exists(megaMilkMolarsPath))
            {
                logger.LogDebug("Found save data for Mega Milk Molars");
                string megaMilkMolarsData = File.ReadAllText(megaMilkMolarsPath);
                MegaMilkMolars.Value = int.Parse(megaMilkMolarsData);
            }
            if (File.Exists(milkMolarUpgradesPath))
            {
                logger.LogDebug("Found save data for Milk Molar Upgrades");
                string milkMolarUpgradesData = File.ReadAllText(milkMolarUpgradesPath);
                ClientsMilkMolarUpgrades = JsonConvert.DeserializeObject<Dictionary<ulong, List<MilkMolarUpgrade>>>(milkMolarUpgradesData, settings);
            }
            if (File.Exists(megaMilkMolarUpgradesPath))
            {
                logger.LogDebug("Found save data for Mega Milk Molar Upgrades");
                string megaMilkMolarUpgradesData = File.ReadAllText(megaMilkMolarUpgradesPath);
                MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaMilkMolarUpgradesData, settings);
            }


            if (ClientsMilkMolars == null || ClientsMilkMolars.Count == 0)
            {
                logger.LogDebug("ClientsMilkMolars is null");
                ClientsMilkMolars = new Dictionary<ulong, int>();
                MilkMolarController.MilkMolars = 0;
                ClientsMilkMolars.Add(localPlayer.playerSteamId, 0);
                logger.LogDebug("Added data for ClientsMilkMolars");
            }
            if (ClientsMilkMolarUpgrades == null || ClientsMilkMolarUpgrades.Count == 0)
            {
                logger.LogDebug("ClientsMilkMolarUpgrades is null");
                ClientsMilkMolarUpgrades = new Dictionary<ulong, List<MilkMolarUpgrade>>();
                MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetUpgrades();
                ClientsMilkMolarUpgrades.Add(localPlayer.playerSteamId, MilkMolarController.MilkMolarUpgrades);
                logger.LogDebug("Added data for ClientsMilkMolarUpgrades");
            }
            if (MegaMilkMolarUpgrades == null || MegaMilkMolarUpgrades.Count == 0)
            {
                logger.LogDebug("MegaMilkMolarUpgrades is null");
                MegaMilkMolarUpgrades = MilkMolarController.GetUpgrades(mega: true);
                logger.LogDebug("Added data for MegaMilkMolarUpgrades");
            }

            MilkMolarController.MilkMolars = ClientsMilkMolars[localPlayer.playerSteamId];
            MilkMolarController.MilkMolarUpgrades = ClientsMilkMolarUpgrades[localPlayer.playerSteamId];

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
                if (ClientsMilkMolars != null && ClientsMilkMolars.ContainsKey(steamId))
                {
                    logger.LogDebug("Found data for client, sending data to client");
                    string upgrades = JsonConvert.SerializeObject(ClientsMilkMolarUpgrades[steamId], settings);
                    string megaUpgrades = JsonConvert.SerializeObject(MegaMilkMolarUpgrades, settings);
                    Instance.SendDataToClientClientRpc(steamId, upgrades, megaUpgrades, ClientsMilkMolars[steamId]);
                }
                else
                {
                    logger.LogDebug("Sending default data to client");
                    ClientsMilkMolarUpgrades.Add(steamId, MilkMolarController.GetUpgrades());
                    ClientsMilkMolars.Add(steamId, 0);
                    string megaUpgrades = JsonConvert.SerializeObject(MegaMilkMolarUpgrades, settings);
                    Instance.SendDefaultDataClientRpc(steamId, megaUpgrades);
                }
            }
        }

        [ClientRpc]
        private void SendDataToClientClientRpc(ulong steamId, string upgrades, string megaUpgrades, int milkMolars)
        {
            if (localPlayer.playerSteamId == steamId)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                MilkMolarController.MilkMolars = milkMolars;
                MilkMolarController.MilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(upgrades, settings);
                MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings); 
            }
        }

        [ClientRpc]
        private void SendDefaultDataClientRpc(ulong steamId, string megaUpgrades)
        {
            if (localPlayer.playerSteamId == steamId)
            {
                logger.LogDebug("Getting default data for self");
                MilkMolarController.MilkMolars = 0;
                MilkMolarController.MilkMolarUpgrades = MilkMolarController.GetUpgrades();
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings);
            }
        }

        public static void SendAllDataToServer()
        {
            Instance.UpdateMilkMolarsServerRpc(MilkMolarController.MilkMolars, localPlayer.playerSteamId);
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string upgrades = JsonConvert.SerializeObject(MilkMolarController.MilkMolarUpgrades, settings);
            Instance.SendUpgradeDataToServerServerRpc(upgrades, localPlayer.playerSteamId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendUpgradeDataToServerServerRpc(string data, ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                if (ClientsMilkMolarUpgrades.ContainsKey(steamId))
                {
                    ClientsMilkMolarUpgrades[steamId] = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(data, settings);
                }
                else
                {
                    ClientsMilkMolarUpgrades.Add(steamId, JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(data, settings));
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarServerRpc(ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (configSharedMilkMolars.Value)
                {
                    foreach (var player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (ClientsMilkMolars.ContainsKey(player.playerSteamId)) { ClientsMilkMolars[player.playerSteamId]++; }
                        else { ClientsMilkMolars.Add(player.playerSteamId, 1); }
                    }

                    AddMilkMolarToAllClientsClientRpc();
                }
                else
                {
                    if (ClientsMilkMolars.ContainsKey(steamId)) { ClientsMilkMolars[steamId]++; }
                    else { ClientsMilkMolars.Add(steamId, 1); }

                    AddMilkMolarClientRpc(steamId);
                }
            }
        }

        [ClientRpc]
        private void AddMilkMolarClientRpc(ulong steamId)
        {
            if (localPlayer.playerSteamId == steamId)
            {
                MilkMolarController.MilkMolars++;
                if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
                if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"Milk Molar activated", $"You now have {MilkMolarController.MilkMolars} unspent Milk Molars."); }
                else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server"); }
            }
        }

        [ClientRpc]
        private void AddMilkMolarToAllClientsClientRpc()
        {
            MilkMolarController.MilkMolars++;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"Milk Molar activated", $"You now have {MilkMolarController.MilkMolars} unspent Milk Molars."); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Milk Molar activated! You now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server"); }
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsClientRpc(ulong steamId, int amount)
        {
            if (localPlayer.playerSteamId == steamId)
            {
                MilkMolarController.MilkMolars += amount;
                if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
                if (configNotifyMethod.Value != 3) { HUDManager.Instance.AddChatMessage($"{amount} Milk Molars activated! Your now have {MilkMolarController.MilkMolars} unspent Milk Molars.", "Server"); }
            }
        }

        [ClientRpc]
        public void AddMultipleMilkMolarsAllClientsClientRpc(int amount)
        {
            MilkMolarController.MilkMolars += amount;
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
            if (configNotifyMethod.Value != 3) { HUDManager.Instance.AddChatMessage($"{MegaMilkMolars.Value} Mega Milk Molars activated! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server"); }
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
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"Mega Milk Molar activated", $"Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars."); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"Mega Milk Molar activated! Your group now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server"); }
        }

        [ClientRpc]
        public void AddMultipleMegaMilkMolarsClientRpc(int amount)
        {
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 0.5f); }
            if (configNotifyMethod.Value != 3) { HUDManager.Instance.AddChatMessage($"{MegaMilkMolars.Value} Mega Milk Molars activated! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars.", "Server"); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuyMegaMilkMolarUpgradeServerRpc(string upgradeName, int cost, ulong steamId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value -= cost;
                BuyMegaMilkMolarUpgradeClientRpc(upgradeName, steamId);
            }
        }

        [ClientRpc]
        public void BuyMegaMilkMolarUpgradeClientRpc(string upgradeName, ulong steamId)
        {
            if (localPlayer.playerSteamId == steamId) { return; }
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