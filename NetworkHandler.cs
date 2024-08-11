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

namespace MilkMolars
{
    public class NetworkHandler : NetworkBehaviour
    {
        private static ManualLogSource logger = Plugin.LoggerInstance;

        public static NetworkHandler Instance { get; private set; }
        public static PlayerControllerB localPlayer { get { return StartOfRound.Instance.localPlayerController; } }

        public static PlayerControllerB PlayerFromId(ulong id) { return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[id]]; }

        public NetworkVariable<int> MegaMilkMolars = new NetworkVariable<int>(0);


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

        [ServerRpc(RequireOwnership = false)]
        public void AddMilkMolarServerRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                AddMilkMolarClientRpc(clientId);
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

        /*[ServerRpc(RequireOwnership = false)]
        public void AddMultipleMegaMilkMolarsServerRpc(int amount)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                MegaMilkMolars.Value += amount;
                AddMultipleMegaMilkMolarsClientRpc(amount);
            }
        }*/

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
            MilkMolarUpgrade upgrade = MilkMolarController.MegaMilkMolarUpgrades.Where(x => x.name == upgradeName).FirstOrDefault();
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