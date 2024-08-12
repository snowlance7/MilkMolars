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

        public static void UpdateClientsMilkMolars(ulong clientId, int amount, bool adding = false)
        {
            if (ClientsMilkMolars.ContainsKey(clientId))
            {
                if (adding) { ClientsMilkMolars[clientId] += amount; }
                else { ClientsMilkMolars[clientId] = amount; }
            }
            else
            {
                ClientsMilkMolars.Add(clientId, amount);
            }
        }

        public static void ResetAllData()
        {
            ClientsMilkMolars = new Dictionary<ulong, int>();
            ClientsMilkMolarUpgrades = new Dictionary<ulong, List<MilkMolarUpgrade>>();
            MegaMilkMolars = new NetworkVariable<int>(0);
            MegaMilkMolarUpgrades = new List<MilkMolarUpgrade>();

            SaveDataToFile();
        }

        public static void SaveDataToFile()
        {
            ES3.Save($"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolars", ClientsMilkMolars);
            ES3.Save($"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolarUpgrades", ClientsMilkMolarUpgrades);
            ES3.Save($"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolars", MegaMilkMolars);
            ES3.Save($"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolarUpgrades", MegaMilkMolarUpgrades);

            //string location = "C:\\Users\\snowl\\source\\repos\\1Lethal Company Debugging\\BepInEx\\plugins\\ClientsMilkMolarUpgrades.json";
            //var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            //string json = JsonConvert.SerializeObject(ClientsMilkMolarUpgrades, settings);
            //File.WriteAllText(location, json);

        }

        public static void LoadDataFromFile()
        {
            ES3.Load($"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolars", ClientsMilkMolars);
            ES3.Load($"{GameNetworkManager.Instance.currentSaveFileName}-ClientsMilkMolarUpgrades", ClientsMilkMolarUpgrades);
            ES3.Load($"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolars", MegaMilkMolars);
            ES3.Load($"{GameNetworkManager.Instance.currentSaveFileName}-MegaMilkMolarUpgrades", MegaMilkMolarUpgrades);
        }

        public static void SendDataToClient(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (ClientsMilkMolars != null && ClientsMilkMolars.ContainsKey(clientId))
                {
                    
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMilkMolarsServerRpc(int newAmount, ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                UpdateClientsMilkMolars(clientId, newAmount);
            }
        }

        [ClientRpc]
        private void SendDataToClientClientRpc(ulong clientId, string upgrades, string megaUpgrades, int milkMolars = 0)
        {
            if (localPlayer.actualClientId == clientId)
            {
                // TODO: Implement
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendUpgradeDataToServerServerRpc(string data, ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                //ClientsMilkMolarUpgrades[clientId] = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(data); // TODO: Temp for testing
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarServerRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (configSharedMilkMolars.Value)
                {
                    foreach (var player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (ClientsMilkMolars.ContainsKey(player.actualClientId)) { ClientsMilkMolars[player.actualClientId]++; }
                        else { ClientsMilkMolars.Add(player.actualClientId, 1); }
                    }

                    AddMilkMolarToAllClientsClientRpc();
                }
                else
                {
                    if (ClientsMilkMolars.ContainsKey(clientId)) { ClientsMilkMolars[clientId]++; }
                    else { ClientsMilkMolars.Add(clientId, 1); }

                    AddMilkMolarClientRpc(clientId);
                }
            }
        }

        [ClientRpc]
        private void AddMilkMolarClientRpc(ulong clientId)
        {
            if (localPlayer.actualClientId == clientId)
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
        public void AddMultipleMilkMolarsClientRpc(ulong clientId, int amount)
        {
            if (localPlayer.actualClientId == clientId)
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
        public void BuyMegaMilkMolarUpgradeServerRpc(string upgradeName, int cost, ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value -= cost;
                BuyMegaMilkMolarUpgradeClientRpc(upgradeName, clientId);
            }
        }

        [ClientRpc]
        public void BuyMegaMilkMolarUpgradeClientRpc(string upgradeName, ulong clientId)
        {
            if (localPlayer.actualClientId == clientId) { return; }
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