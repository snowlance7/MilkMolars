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
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace MilkMolars
{
    public class NetworkHandler : NetworkBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static NetworkHandler Instance { get; private set; }
        public static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[id]]; }

        [ModData(saveWhen: SaveWhen.OnSave, loadWhen: LoadWhen.OnLoad, saveLocation: SaveLocation.CurrentSave, resetWhen: ResetWhen.OnGameOver)]
        public static NetworkVariable<int> MegaMilkMolars = new NetworkVariable<int>(0);

        [ModData(saveWhen: SaveWhen.OnSave, loadWhen: LoadWhen.OnLoad, saveLocation: SaveLocation.CurrentSave, resetWhen: ResetWhen.OnGameOver)]
        public static Dictionary<ulong, int> ClientsMilkMolars = new Dictionary<ulong, int>();

        [ModData(saveWhen: SaveWhen.OnSave, loadWhen: LoadWhen.OnLoad, saveLocation: SaveLocation.CurrentSave, resetWhen: ResetWhen.OnGameOver)]
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

        public static void SendDataToClient(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                int amount = 0;
                string upgrades;
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                if (ClientsMilkMolars.ContainsKey(clientId) && ClientsMilkMolarUpgrades.ContainsKey(clientId))
                {
                    amount = ClientsMilkMolars[clientId];
                    upgrades = JsonConvert.SerializeObject(ClientsMilkMolarUpgrades[clientId], settings);
                }
                else
                {
                    List<MilkMolarUpgrade> upgradesList = MilkMolarController.GetUpgrades();
                    upgrades = JsonConvert.SerializeObject(upgradesList, settings);
                }

                string? megaUpgrades = JsonConvert.SerializeObject(MilkMolarController.MegaMilkMolarUpgrades);
                Instance.SendDataToClientClientRpc(clientId, upgrades, megaUpgrades, amount);
            }
        }

        [ClientRpc]
        private void SendDataToClientClientRpc(ulong clientId, string upgrades, string megaUpgrades, int amount = 0)
        {
            if (localPlayer.actualClientId == clientId)
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

                MilkMolarController.MilkMolars = amount;

                MilkMolarController.MilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(upgrades, settings);

                MilkMolarController.MegaMilkMolarUpgrades = JsonConvert.DeserializeObject<List<MilkMolarUpgrade>>(megaUpgrades, settings);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateMilkMolarsServerRpc(ulong clientId, int amount)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                ClientsMilkMolars[clientId] = amount;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarsServerRpc(ulong clientId, int amount = 1)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (configSharedMilkMolars.Value)
                {
                    AddMilkMolarClientRpc(clientId);

                    foreach (var player in StartOfRound.Instance.allPlayerScripts)
                    {
                        if (ClientsMilkMolars.ContainsKey(player.actualClientId))
                        {
                            ClientsMilkMolars[player.actualClientId] += amount;
                        }
                        else
                        {
                            ClientsMilkMolars.Add(player.actualClientId, amount);
                        }
                    }
                }
                else
                {
                    if (ClientsMilkMolars.ContainsKey(clientId))
                    {
                        ClientsMilkMolars[clientId] += amount;
                    }
                    else
                    {
                        ClientsMilkMolars.Add(clientId, amount);
                    }
                }
            }
        }

        [ClientRpc]
        private void AddMilkMolarClientRpc(ulong clientId)
        {
            if (localPlayer.actualClientId == clientId) { return; }
            MilkMolarController.MilkMolars++;
            PlayerControllerB player = PlayerFromId(clientId);
            logger.LogDebug("Added milk molar found by " + player.playerUsername);
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"{player.playerUsername} activated a Milk Molar!", $"You now have {MilkMolarController.MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"{player.playerUsername} activated a Milk Molar! You now have {MilkMolarController.MilkMolars} unspent Milk Molars. Open the upgrade menu to spend your Milk Molars. (M by default)", "Server"); }
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddMegaMilkMolarServerRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value++;
                AddMegaMilkMolarClientRpc(clientId);
            }
        }

        [ClientRpc]
        private void AddMegaMilkMolarClientRpc(ulong clientId)
        {
            if (localPlayer.actualClientId == clientId) { return; }
            PlayerControllerB player = PlayerFromId(clientId);
            logger.LogDebug("Added mega milk molar found by " + player.playerUsername);
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value == 1) { HUDManager.Instance.DisplayTip($"{player.playerUsername} activated a Mega Milk Molar!", $"Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars. Open the upgrade menu to spend them. (M by default)"); }
            else if (configNotifyMethod.Value == 2) { HUDManager.Instance.AddChatMessage($"{player.playerUsername} activated a Mega Milk Molar! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars. Open the upgrade menu to spend them. (M by default)", "Server"); }
        }

        [ClientRpc]
        public void AddMultipleMegaMilkMolarsClientRpc(int amount)
        {
            if (configPlaySound.Value) { localPlayer.statusEffectAudio.PlayOneShot(ActivateSFX, 1f); }
            if (configNotifyMethod.Value != 3) { HUDManager.Instance.AddChatMessage($"{MegaMilkMolars.Value} Mega Milk Molars activated! Your crew now has {MegaMilkMolars.Value} unspent Mega Milk Molars. Open the upgrade menu to spend them. (M by default)", "Server"); }
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